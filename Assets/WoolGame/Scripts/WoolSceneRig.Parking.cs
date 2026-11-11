using UnityEngine;
namespace WoolGame
{
    public sealed partial class WoolSceneRig
    {
        public Vector3 ParkingPresentationOffset {get;private set;}
        public float TravelLeft {get;private set;}=-5.5f;
        public float TravelRight {get;private set;}=5.5f;
        public float TravelBottom {get;private set;}=-12;
        public float TravelTop {get;private set;}
        Vector2 parkedBodyMin,parkedBodyMax;
        public float TravelRadius {get;private set;}=1;
        public bool ParkingTravelReady {get;private set;}
        // Presentation offsets never alter logical tile positions or blocking rules.
        public void CompactParking(WoolVehicleSources sources,bool tutorial=false)
        {
            Physics.SyncTransforms();
            var min=Vector2.one*float.MaxValue;var max=Vector2.one*float.MinValue;
            float radius=.5f;
            foreach(var v in vehicles)
            {
                if(!v||!v.gameObject.activeInHierarchy)continue;
                var b=v.GetComponent<BoxCollider>().bounds;
                var center=world.InverseTransformPoint(b.center);
                var localMin=Vector2.one*float.MaxValue;var localMax=Vector2.one*float.MinValue;
                for(int i=0;i<8;i++)
                {
                    var p=(Vector2)world.InverseTransformPoint(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));
                    localMin=Vector2.Min(localMin,p);localMax=Vector2.Max(localMax,p);
                }
                min=Vector2.Min(min,localMin);max=Vector2.Max(max,localMax);
                radius=Mathf.Max(radius,(localMax-localMin).magnitude*.5f);
            }
            if(min.x==float.MaxValue)return;
            TravelRadius=radius;
            float slotY=world.InverseTransformPoint(slots[0].position).y;
            TravelTop=slotY-1.0f;
            float shift=TravelTop-radius-.45f-max.y;
            if(tutorial)shift-=.6f;
            ParkingPresentationOffset=Vector3.up*shift;
            foreach(var v in vehicles)if(v&&v.gameObject.activeInHierarchy)v.localPosition+=ParkingPresentationOffset;
            if(sources)sources.ShiftPresentation(ParkingPresentationOffset);
            min.y+=shift;max.y+=shift;
            parkedBodyMin=min;parkedBodyMax=max;
            TravelLeft=min.x-radius-.30f;TravelRight=max.x+radius+.30f;
            TravelBottom=min.y-radius-.30f;
            ParkingTravelReady=true;
            Physics.SyncTransforms();
        }
        public void RefreshSlotLabelLayout(){PositionSlotCounts(Screen.width,Screen.height);}
        void PositionSlotCounts(int width,int height)
        {
            if(!vehicleCamera)return;
            if(Application.isPlaying&&!GetComponent<WoolLockedSlotOverlay>())gameObject.AddComponent<WoolLockedSlotOverlay>();
            float scale=GameplayFrame.width>0?GameplayFrame.width/1080f:WoolHud.PortraitScale(width,height);
            var camera=SlotCamera;
            float pixelsPerUnit=camera.pixelHeight/(2*camera.orthographicSize);
            for(int i=0;i<counts.Length;i++)
            {
                var text=counts[i];if(!text||string.IsNullOrEmpty(text.text))continue;if(text.text!="Unlock")text.color=WoolArt.Hex("FFF3CF");
                var p=camera.WorldToScreenPoint(slots[i].position);
                bool locked=text.text=="Unlock"||text.text=="AD +"||text.text=="+";
                if(locked&&slotUnlockIcons!=null&&i<slotUnlockIcons.Length&&slotUnlockIcons[i]){var icon=slotUnlockIcons[i];var sr=icon.GetComponent<SpriteRenderer>();var iconPoint=p;iconPoint.z=8;icon.transform.SetPositionAndRotation(camera.ScreenToWorldPoint(iconPoint),camera.transform.rotation);if(sr&&sr.sprite){float diameter=48*scale/Mathf.Max(.001f,pixelsPerUnit);float iconSize=diameter/sr.sprite.bounds.size.x;var iconParentScale=icon.transform.parent.lossyScale;icon.transform.localScale=new Vector3(iconSize/Mathf.Abs(iconParentScale.x),iconSize/Mathf.Abs(iconParentScale.y),iconSize/Mathf.Abs(iconParentScale.z));}}
                p.y-=locked?65*scale:64*scale;p.z=8;
                text.transform.SetPositionAndRotation(camera.ScreenToWorldPoint(p),camera.transform.rotation);
                text.alignment=TMPro.TextAlignmentOptions.Center;text.enableAutoSizing=false;
                text.rectTransform.sizeDelta=new Vector2(64,32);
                text.overflowMode=TMPro.TextOverflowModes.Overflow;text.textWrappingMode=TMPro.TextWrappingModes.NoWrap;
                float lineHeight=Mathf.Max(.1f,text.GetPreferredValues(text.text,100,100).y);
                float size=(locked?23:36)*scale/(lineHeight*Mathf.Max(.001f,pixelsPerUnit));
                var parent=text.transform.parent.lossyScale;
                text.transform.localScale=new Vector3(size/Mathf.Abs(parent.x),size/Mathf.Abs(parent.y),size/Mathf.Abs(parent.z));
            }
        }
    }
}

