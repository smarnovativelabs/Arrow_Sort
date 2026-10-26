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
        float travel;
        TMP_Text Count(Transform parent,Vector3 p)
        {
            var text=Instantiate(rig.counts[0],rig.world);text.name="Vehicles remaining";
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
            rig=scene;level=rules;definition=data;travel=0;for(int n=0;n<beltDeparting.Length;n++)beltDeparting[n]=-1;
            if(!rail)
            {
                rail=Instantiate(rig.authoredLayout.railPrefab,rig.world).transform;
                rail.name="Designer bottom rail tunnel";WoolSceneRig.SetViewLayer(rail,"Vehicle View");
                rail.localPosition=WoolSceneRig.ParkingOffset+new Vector3(0,WoolEndlessGenerator.RailY,0);
                railCount=Count(rail,new Vector3(-4.05f,0,-1.5f));
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
            Tick(0);
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
        {int result=0;foreach(var t in level.Tiles)if(t.source==source&&t.sourceId==id&&!t.selected)result++;return result;}
        void Show(int i,Vector2 p)
        {
            var t=level.Tiles[i];t.x=p.x;t.y=p.y;t.hidden=false;
            rig.SetVehicle(i,t,0,1);
        }
        public void Tick(float dt)
        {
            if(level==null)return;
            travel=Mathf.Repeat(travel+dt*.45f,7.8f);
            if(definition.hasRail)
            {
                for(int n=0;n<beltSlots.Length;n++)
                {
                    int i=beltSlots[n];if(i>=0&&level.Tiles[i].selected){beltDeparting[n]=i;beltSlots[n]=i=-1;}
                    float x=Mathf.Repeat(n*1.3f+travel+3.9f,7.8f)-3.9f;
                    bool clear=beltDeparting[n]<0||!rig.vehicles[beltDeparting[n]].gameObject.activeSelf||rig.vehicles[beltDeparting[n]].localPosition.y>WoolEndlessGenerator.RailY+WoolSceneRig.ParkingOffset.y+rig.ParkingPresentationOffset.y+1.5f;
                    if(i<0&&clear){i=Next(WoolVehicleSource.Rail,0);if(i>=0){beltSlots[n]=i;Show(i,new Vector2(x,WoolEndlessGenerator.RailY));}}
                    if(i>=0){var t=level.Tiles[i];t.x=x;rig.vehicles[i].localPosition=rig.ParkingPresentationOffset+WoolSceneRig.ParkingOffset+new Vector3(x,WoolEndlessGenerator.RailY,-.3f);}
                }
                railCount.SetText("{0}\n<size=45%>LEFT</size>",Remaining(WoolVehicleSource.Rail,0));
            }
            for(int c=0;c<depots.Length;c++)if(depots[c].gameObject.activeSelf)
            {
                int i=front[c];if(i>=0&&level.Tiles[i].selected){departing[c]=i;front[c]=i=-1;}
                bool clear=departing[c]<0||!rig.vehicles[departing[c]].gameObject.activeSelf||((Vector2)(rig.vehicles[departing[c]].localPosition-depots[c].localPosition)).sqrMagnitude>3.2f;
                if(i<0&&clear){i=Next(WoolVehicleSource.Container,c);if(i>=0){front[c]=i;Show(i,definition.containerPositions[c]+Vector2.up*1.15f);}}
                depotCounts[c].SetText("{0}\n<size=45%>LEFT</size>",Remaining(WoolVehicleSource.Container,c));
            }
        }
    }
}


