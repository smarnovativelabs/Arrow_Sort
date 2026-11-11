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
        Renderer[] railRenderers;
        Transform rail;readonly Transform[] depots=new Transform[2];
        readonly TMP_Text[] depotCounts=new TMP_Text[2];TMP_Text railCount;RectTransform railBadge;TMP_Text railBadgeLabel;
        readonly TMP_Text[] depotBadges=new TMP_Text[2];
        readonly Vector3[] depotCountAnchors=new Vector3[2];Vector3 railCountAnchor;
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
            text.transform.SetParent(rig.world,true);
            text.fontSize=12;text.color=Color.white;text.alignment=TextAlignmentOptions.Center;
            text.rectTransform.sizeDelta=new Vector2(3,2);text.enableAutoSizing=false;
            text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth,.2f);
            text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor,new Color(.12f,.1f,.08f,1));
            text.fontMaterial.SetFloat("_ZTestMode",(float)UnityEngine.Rendering.CompareFunction.Always);
            text.gameObject.SetActive(true);return text;
        }
        public void Load(WoolSceneRig scene,WoolLevel rules,WoolLevelDefinition data)
        {
            rig=scene;level=rules;definition=data;travel=0;initializeRail=true;for(int n=0;n<assignedSlots.Length;n++)assignedSlots[n]=-1;for(int n=0;n<beltDeparting.Length;n++)beltDeparting[n]=-1;
            if(!rail)
            {
                rail=Instantiate(rig.authoredLayout.railPrefab,rig.world).transform;
                rail.localScale*=RailScale;
                railRenderers=rail.GetComponentsInChildren<Renderer>(true);
                rail.name="Designer bottom rail tunnel";WoolSceneRig.SetViewLayer(rail,"Vehicle View");
                rail.localPosition=WoolSceneRig.ParkingOffset+new Vector3(0,WoolEndlessGenerator.RailY,0);
                railCount=Count(rail,new Vector3(-4.05f*RailScale,0,-1.5f*RailScale));
                railCountAnchor=rail.InverseTransformPoint(railCount.transform.position);
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
                    depotCountAnchors[c]=depots[c].InverseTransformPoint(depotCounts[c].transform.position);
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
                railCount.gameObject.SetActive(false);UpdateRailBadge();
            }
            if(!definition.hasRail&&railCount)railCount.gameObject.SetActive(false);
            RefreshRailBadgeVisibility();
            for(int c=0;c<depots.Length;c++)if(depotCounts[c])depotCounts[c].gameObject.SetActive(depots[c].gameObject.activeSelf);
            initializeRail=false;
            for(int c=0;c<depots.Length;c++)if(depots[c].gameObject.activeSelf)
            {
                int i=front[c];if(i>=0&&level.Tiles[i].selected&&Released(i,hasFreeSlot)){departing[c]=i;front[c]=i=-1;}
                bool clear=departing[c]<0||!rig.vehicles[departing[c]].gameObject.activeSelf||((Vector2)(rig.vehicles[departing[c]].localPosition-depots[c].localPosition)).sqrMagnitude>3.2f;
                if(i<0&&clear){i=Next(WoolVehicleSource.Container,c);if(i>=0){front[c]=i;Show(i,definition.containerPositions[c]+Vector2.up*1.15f);}}
                depotCounts[c].SetText("{0}\n<size=60%>LEFT</size>",Remaining(WoolVehicleSource.Container,c));

            }
        }
        void LateUpdate(){if(definition!=null&&definition.hasRail&&rail&&railCount)UpdateRailBadge();if(railBadge)railBadge.SetAsFirstSibling();RefreshRailBadgeVisibility();RefreshDepotBadges();}
        void RefreshRailBadgeVisibility(){if(railBadge)railBadge.gameObject.SetActive(definition!=null&&definition.hasRail&&rail&&rail.gameObject.activeInHierarchy&&rig.world.gameObject.activeInHierarchy&&rig.vehicleCamera&&rig.vehicleCamera.isActiveAndEnabled&&rig.hud.GameplayControlsVisible&&!rig.hud.AdVisible);}
        void UpdateRailBadge(){
            if(!railBadge){
                railBadge=new GameObject("Tunnel remaining count",typeof(RectTransform)).GetComponent<RectTransform>();railBadge.SetParent(rig.hud.transform,false);
                var text=new GameObject("Remaining vehicles",typeof(RectTransform),typeof(TextMeshProUGUI));text.transform.SetParent(railBadge,false);railBadgeLabel=text.GetComponent<TMP_Text>();railBadgeLabel.font=rig.counts[0].font;railBadgeLabel.color=Color.white;railBadgeLabel.outlineColor=new Color(.12f,.14f,.20f,1);railBadgeLabel.outlineWidth=.12f;railBadgeLabel.alignment=TextAlignmentOptions.Center;railBadgeLabel.raycastTarget=false;railBadgeLabel.textWrappingMode=TextWrappingModes.NoWrap;
                var rect=railBadgeLabel.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
            }
            var oldBackground=railBadge.GetComponent<UnityEngine.UI.Image>();if(oldBackground)oldBackground.enabled=false;
            var root=(RectTransform)rig.hud.transform;var canvas=rig.hud.GetComponentInParent<Canvas>().rootCanvas;var uiCamera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
            float frame=rig.GameplayFrame.width>0?rig.GameplayFrame.width:Screen.width;
            if(railRenderers==null)railRenderers=rail.GetComponentsInChildren<Renderer>(true);
            float minX=float.PositiveInfinity,maxX=float.NegativeInfinity,minY=float.PositiveInfinity,maxY=float.NegativeInfinity;
            foreach(var renderer in railRenderers){if(!renderer||renderer.GetComponent<TMP_Text>()||!renderer.enabled)continue;var bounds=renderer.bounds;for(int corner=0;corner<8;corner++){var offset=new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1);var projected=rig.vehicleCamera.WorldToScreenPoint(bounds.center+Vector3.Scale(bounds.extents,offset));minX=Mathf.Min(minX,projected.x);maxX=Mathf.Max(maxX,projected.x);minY=Mathf.Min(minY,projected.y);maxY=Mathf.Max(maxY,projected.y);}}
            float beltWidth=float.IsInfinity(minY)?frame*.74f:maxX-minX;float beltHeight=float.IsInfinity(minY)?frame*.16f:maxY-minY;
            var screen=rig.vehicleCamera.WorldToScreenPoint(rail.position);if(!float.IsInfinity(minY)){screen.x=minX+beltWidth*.105f;screen.y+=beltWidth*.035f;}
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root,screen,uiCamera,out var point);float units=root.rect.width/Mathf.Max(1,Screen.width);
            railBadge.anchorMin=railBadge.anchorMax=railBadge.pivot=Vector2.one*.5f;railBadge.anchoredPosition=point;railBadge.sizeDelta=new Vector2(beltWidth*.12f,beltHeight*.62f)*units;
            railBadgeLabel.fontSize=beltWidth*.054f*units;railBadgeLabel.enableAutoSizing=false;railBadgeLabel.overflowMode=TextOverflowModes.Overflow;railBadgeLabel.lineSpacing=-8;railBadgeLabel.margin=Vector4.zero;railBadgeLabel.text=Remaining(WoolVehicleSource.Rail,0)+"\n<size=60%>LEFT</size>";
        }
        void OnDestroy(){if(railBadge)Destroy(railBadge.gameObject);foreach(var label in depotBadges)if(label){Destroy(label.fontMaterial);Destroy(label.gameObject);}}
        void RefreshDepotBadges(){
            if(!rig||!rig.hud||!rig.vehicleCamera||level==null)return;
            var root=(RectTransform)rig.hud.transform;var canvas=rig.hud.GetComponentInParent<Canvas>().rootCanvas;var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
            float units=root.rect.width/Mathf.Max(1,Screen.width);
            for(int i=0;i<depots.Length;i++){
                if(!depots[i]||!depotCounts[i])continue;
                depotCounts[i].gameObject.SetActive(false);
                if(!depotBadges[i]){
                    var go=new GameObject("Container remaining count "+i,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(root,false);
                    var label=go.GetComponent<TMP_Text>();depotBadges[i]=label;label.font=rig.counts[0].font;label.fontMaterial=new Material(label.font.material);label.color=Color.white;label.outlineColor=new Color(.12f,.14f,.20f,1);label.outlineWidth=.18f;label.fontMaterial.EnableKeyword("OUTLINE_ON");label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;label.enableAutoSizing=false;label.textWrappingMode=TextWrappingModes.NoWrap;
                }
                var text=depotBadges[i];text.transform.SetAsFirstSibling();text.gameObject.SetActive(depots[i].gameObject.activeInHierarchy&&rig.world.gameObject.activeInHierarchy&&rig.hud.GameplayControlsVisible&&!rig.hud.AdVisible);
                var position=depots[i].TransformPoint(depotCountAnchors[i]);var screen=rig.vehicleCamera.WorldToScreenPoint(position);
                var bounds=new Bounds(depots[i].position,Vector3.zero);foreach(var renderer in depots[i].GetComponentsInChildren<Renderer>())bounds.Encapsulate(renderer.bounds);
                float width=Mathf.Abs(rig.vehicleCamera.WorldToScreenPoint(bounds.center+Vector3.right*bounds.extents.x).x-rig.vehicleCamera.WorldToScreenPoint(bounds.center-Vector3.right*bounds.extents.x).x);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(root,screen,camera,out var point);var rect=text.rectTransform;rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.one*.5f;rect.anchoredPosition=point;rect.sizeDelta=new Vector2(width*.85f,width*.85f)*units;
                text.fontSize=width*.32f*units;text.text=Remaining(WoolVehicleSource.Container,i)+"\n<size=60%>LEFT</size>";
            }
        }

    }
}