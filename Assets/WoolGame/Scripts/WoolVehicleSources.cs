using System;
using UnityEngine;
using TMPro;
namespace WoolGame
{
    // Displays only six rail vehicles and one front vehicle per depot.
    // Queued entries are data backed by the already-authored vehicle pool.
    public sealed class WoolVehicleSources : MonoBehaviour
    {
        WoolSceneRig rig;WoolLevel level;WoolLevelDefinition definition;
        Transform rail;readonly Transform[] depots=new Transform[2];
        readonly TMP_Text[] depotCounts=new TMP_Text[2];TMP_Text railCount;
        readonly int[] front={-1,-1},departing={-1,-1};
        readonly int[] beltDeparting={-1,-1,-1,-1,-1,-1};
        readonly int[] beltSlots={-1,-1,-1,-1,-1,-1};
        readonly int[] assignedSlots=new int[WoolEndlessGenerator.MaxVehicles];
        const float RailScale=1.25f;
        float travel;bool initializeRail;
        public float RailTravel=>travel;
        // Keep the complete vehicle footprint inside the exposed belt opening.
        public bool RailExitClear(int index)=>level!=null&&index>=0&&index<level.Tiles.Count&&Mathf.Abs(level.Tiles[index].x)+level.Tiles[index].width*.5f+.15f<3.9f*RailScale;
        public Vector3 RailEntryWorld=>rail?rig.world.TransformPoint(rail.localPosition+new Vector3(-4.1f*RailScale,0,-.3f)):transform.position;
        public int FrontVehicle(int depot)=>depot>=0&&depot<front.Length?front[depot]:-1;
        public bool HasAssignedSlot(int index)=>index>=0&&index<assignedSlots.Length&&assignedSlots[index]>=0;
        public void NotifySlotAssigned(int index,int slot){if(level!=null&&index>=0&&index<level.Tiles.Count&&level.Tiles[index].selected&&slot>=0&&slot<WoolLevel.Slots)assignedSlots[index]=slot;}
        // Explicit removal is distinct from a failed tap; hold replacement until a slot is free.
        public void NotifyRemoved(int index){if(level!=null&&index>=0&&index<level.Tiles.Count&&level.Tiles[index].selected)assignedSlots[index]=-2;}
        bool Released(int index,bool free)=>assignedSlots[index]>=0||assignedSlots[index]==-2&&free;
        TMP_Text Count(Transform parent,Vector3 p)
        {
            var text=Instantiate(rig.counts[0],rig.world);text.name="Vehicles remaining";
            WoolSceneRig.SetViewLayer(text.transform,"Vehicle View");
            // Place in board coordinates, then retain world size when parenting under
            // the non-uniformly scaled designer prefab.
            text.transform.localPosition=parent.localPosition+p;
            text.transform.rotation=rig.vehicleCamera.transform.rotation;
            text.transform.localScale=Vector3.one*.48f;
            text.transform.SetParent(parent,true);
            text.fontSize=12;text.color=Color.white;text.alignment=TextAlignmentOptions.Center;
            text.rectTransform.sizeDelta=new Vector2(3,2);text.enableAutoSizing=false;
            text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth,.2f);
            text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor,new Color(.12f,.1f,.08f,1));
            text.gameObject.SetActive(true);return text;
        }
        public void Load(WoolSceneRig scene,WoolLevel rules,WoolLevelDefinition data)
        {
            rig=scene;level=rules;definition=data;travel=0;initializeRail=true;for(int n=0;n<assignedSlots.Length;n++)assignedSlots[n]=-1;for(int n=0;n<beltDeparting.Length;n++)beltDeparting[n]=-1;
            if(!rail)
            {
                rail=Instantiate(rig.authoredLayout.railPrefab,rig.world).transform;
                rail.localScale*=RailScale;
                rail.name="Designer bottom rail tunnel";WoolSceneRig.SetViewLayer(rail,"Vehicle View");
                rail.localPosition=WoolSceneRig.ParkingOffset+new Vector3(0,WoolEndlessGenerator.RailY,0);
                railCount=Count(rail,new Vector3(-4.05f*RailScale,0,-1.5f*RailScale));
                railCount.textWrappingMode=TextWrappingModes.NoWrap;
                railCount.overflowMode=TextOverflowModes.Overflow;
                railCount.alignment=TextAlignmentOptions.Center;
            }
            rail.localPosition=WoolSceneRig.ParkingOffset+new Vector3(0,WoolEndlessGenerator.RailY,0);
            rail.gameObject.SetActive(data.hasRail);
            for(int c=0;c<2;c++)
            {
                front[c]=departing[c]=-1;
                if(!depots[c])
                {
                    depots[c]=Instantiate(rig.authoredLayout.tunnelPrefab,rig.world).transform;
                    depots[c].name="Designer vehicle tunnel "+(c+1);WoolSceneRig.SetViewLayer(depots[c],"Vehicle View");
                    depotCounts[c]=Count(depots[c],new Vector3(0,0,-1.05f));
                }
                bool enabled=data.containerPositions!=null&&c<data.containerPositions.Length;
                depots[c].gameObject.SetActive(enabled);if(enabled)depots[c].localPosition=WoolSceneRig.ParkingOffset+(Vector3)data.containerPositions[c];
            }
            for(int n=0;n<beltSlots.Length;n++)beltSlots[n]=-1;
            for(int i=0;i<level.Tiles.Count;i++)if(level.Tiles[i].source!=WoolVehicleSource.Board){level.Tiles[i].hidden=true;rig.vehicles[i].gameObject.SetActive(false);}
            foreach(var tile in level.Tiles)if(tile.source!=WoolVehicleSource.Board){tile.width=.87f;tile.length=tile.size==WoolSize.Small?.87f:tile.size==WoolSize.Medium?1.35f:1.83f;}
            Tick(0,true);
        }
        public void ShiftPresentation(Vector3 delta)
        {
            if(rail)rail.localPosition+=delta;
            foreach(var depot in depots)if(depot)depot.localPosition+=delta;
        }
        int Next(WoolVehicleSource source,int id)
        {
            int result=-1,order=int.MaxValue;
            for(int i=0;i<level.Tiles.Count;i++){var t=level.Tiles[i];if(t.source==source&&t.sourceId==id&&!t.selected&&t.hidden&&t.sourceOrder<order){result=i;order=t.sourceOrder;}}
            return result;
        }
        int Remaining(WoolVehicleSource source,int id)
        {int result=0;for(int i=0;i<level.Tiles.Count;i++){var t=level.Tiles[i];if(t.source==source&&t.sourceId==id&&(!t.selected||assignedSlots[i]==-1))result++;}return result;}
        void Show(int i,Vector2 p)
        {
            var t=level.Tiles[i];t.x=p.x;t.y=p.y;t.hidden=false;
            rig.SetVehicle(i,t,0,1);
        }
        public void Tick(float dt)=>Tick(dt,true);
        public void Tick(float dt,bool hasFreeSlot)
        {
            if(level==null)return;
            float previousTravel=travel;
            bool advance=true;
            // Belt circulation is independent of parking capacity and departure attempts.
            if(advance)travel=Mathf.Repeat(travel+dt*.45f*WoolGameController.VehicleSpeedMultiplier,7.8f);
            if(definition.hasRail)
            {
                for(int n=0;n<beltSlots.Length;n++)
                {
                    int i=beltSlots[n];if(i>=0&&level.Tiles[i].selected){if(!Released(i,hasFreeSlot))continue;beltDeparting[n]=i;beltSlots[n]=i=-1;}
                    float x=(Mathf.Repeat(n*1.3f+travel+3.9f,7.8f)-3.9f)*RailScale;
                    float previousX=(Mathf.Repeat(n*1.3f+previousTravel+3.9f,7.8f)-3.9f)*RailScale;
                    bool insideEntry=initializeRail||x<previousX;
                    bool clear=beltDeparting[n]<0||!rig.vehicles[beltDeparting[n]].gameObject.activeSelf||rig.vehicles[beltDeparting[n]].localPosition.y>WoolEndlessGenerator.RailY+WoolSceneRig.ParkingOffset.y+rig.ParkingPresentationOffset.y+1.5f;
                    if(i<0&&clear&&insideEntry){i=Next(WoolVehicleSource.Rail,0);if(i>=0){beltSlots[n]=i;Show(i,new Vector2(x,WoolEndlessGenerator.RailY));}}
                    if(i>=0&&!level.Tiles[i].probing){var t=level.Tiles[i];t.x=x;t.carried=true;t.transitDirection=WoolDirection.Up;t.motionX=t.motionY=0;rig.vehicles[i].localPosition=rig.ParkingPresentationOffset+WoolSceneRig.ParkingOffset+new Vector3(x,WoolEndlessGenerator.RailY,-.3f);rig.vehicles[i].localRotation=Quaternion.Euler(-90,0,0);}
                }
                railCount.SetText("{0}\n<size=45%>LEFT</size>",Remaining(WoolVehicleSource.Rail,0));
            }
            initializeRail=false;
            for(int c=0;c<depots.Length;c++)if(depots[c].gameObject.activeSelf)
            {
                int i=front[c];if(i>=0&&level.Tiles[i].selected&&Released(i,hasFreeSlot)){departing[c]=i;front[c]=i=-1;}
                bool clear=departing[c]<0||!rig.vehicles[departing[c]].gameObject.activeSelf||((Vector2)(rig.vehicles[departing[c]].localPosition-depots[c].localPosition)).sqrMagnitude>3.2f;
                if(i<0&&clear){i=Next(WoolVehicleSource.Container,c);if(i>=0){front[c]=i;Show(i,definition.containerPositions[c]+Vector2.up*1.15f);}}
                depotCounts[c].SetText("{0}\n<size=45%>LEFT</size>",Remaining(WoolVehicleSource.Container,c));
            }
        }
    }
}
