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
        public float TravelRadius {get;private set;}=1;
        public bool ParkingTravelReady {get;private set;}
        // Presentation offsets never alter logical tile positions or blocking rules.
        public void CompactParking(WoolVehicleSources sources)
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
            TravelTop=slotY-.8f;
            float shift=TravelTop-radius-.18f-max.y;
            ParkingPresentationOffset=Vector3.up*shift;
            foreach(var v in vehicles)if(v&&v.gameObject.activeInHierarchy)v.localPosition+=ParkingPresentationOffset;
            if(sources)sources.ShiftPresentation(ParkingPresentationOffset);
            min.y+=shift;max.y+=shift;
            TravelLeft=min.x-radius-.12f;TravelRight=max.x+radius+.12f;
            TravelBottom=min.y-radius-.12f;
            ParkingTravelReady=true;
            Physics.SyncTransforms();
        }
        public void RefreshSlotLabelLayout(){PositionSlotCounts(Screen.width,Screen.height);}
        void PositionSlotCounts(int width,int height)
        {
            if(!vehicleCamera)return;
            float scale=GameplayFrame.width>0?GameplayFrame.width/1080f:WoolHud.PortraitScale(width,height);
            float pixelsPerUnit=vehicleCamera.pixelHeight/(2*vehicleCamera.orthographicSize);
            for(int i=0;i<counts.Length;i++)
            {
                var text=counts[i];if(!text||string.IsNullOrEmpty(text.text))continue;
                var p=vehicleCamera.WorldToScreenPoint(slots[i].position);
                p.y-=text.text=="+"?0:29*scale;p.z=8;
                text.transform.SetPositionAndRotation(vehicleCamera.ScreenToWorldPoint(p),vehicleCamera.transform.rotation);
                text.alignment=TMPro.TextAlignmentOptions.Center;text.enableAutoSizing=false;
                text.rectTransform.sizeDelta=new Vector2(64,32);
                text.overflowMode=TMPro.TextOverflowModes.Overflow;text.textWrappingMode=TMPro.TextWrappingModes.NoWrap;
                float lineHeight=Mathf.Max(.1f,text.GetPreferredValues(text.text,100,100).y);
                float size=36*scale/(lineHeight*Mathf.Max(.001f,pixelsPerUnit));
                var parent=text.transform.parent.lossyScale;
                text.transform.localScale=new Vector3(size/Mathf.Abs(parent.x),size/Mathf.Abs(parent.y),size/Mathf.Abs(parent.z));
            }
        }
    }
}

