using UnityEngine;
namespace WoolGame
{
    public sealed partial class WoolSceneRig
    {
        [Header("Portrait cameras")]
        public Camera vehicleCamera;
        public Camera slotCamera;
        public Camera SlotCamera=>slotCamera?slotCamera:vehicleCamera;
        public Camera uiCamera;
        [Range(25,65)] public float rescueCameraPitch=40;
        [Range(50,90)] public float vehicleCameraPitch=90;
        [Min(1)] public float vehicleCameraSize=15;
        [Range(.2f,.65f)] public float rescueScreenShare=.42f;
        public Camera CameraForScreenPoint(Vector2 point)
        {
            if(slotCamera&&slotCamera.enabled&&slotCamera.pixelRect.Contains(point))return slotCamera;
            return vehicleCamera&&vehicleCamera.enabled&&vehicleCamera.pixelRect.Contains(point)?vehicleCamera:gameCamera;
        }
        Camera transitCamera;int transitFrame=-1;
        RenderTexture transitTexture;UnityEngine.UI.RawImage transitImage;
        public void PresentDockingVehicle(Transform vehicle,Vector3 from,Vector3 to,float progress,Vector3 originalScale,bool finalLeg=true)
        {
            if(!slotCamera)return;
            int layer=LayerMask.NameToLayer("Vehicle Transit");if(layer<0)return;
            if(!transitCamera)
            {
                var root=new GameObject("Vehicle Transit Camera");root.transform.SetParent(transform,false);
                transitCamera=root.AddComponent<Camera>();
            }
            transitCamera.CopyFrom(vehicleCamera);transitCamera.rect=new Rect(0,0,1,1);
            transitCamera.aspect=(float)Screen.width/Screen.height;
            transitCamera.depth=Mathf.Max(gameCamera.depth,slotCamera.depth)+1;
            transitCamera.clearFlags=CameraClearFlags.SolidColor;transitCamera.backgroundColor=Color.clear;transitCamera.cullingMask=1<<layer;
            transitCamera.allowHDR=false;transitCamera.allowMSAA=false;
            if(!transitTexture||transitTexture.width!=Screen.width||transitTexture.height!=Screen.height)
            {
                if(transitTexture){transitTexture.Release();Destroy(transitTexture);}
                transitTexture=new RenderTexture(Screen.width,Screen.height,16,RenderTextureFormat.ARGB32);transitTexture.Create();
            }
            transitCamera.targetTexture=transitTexture;
            if(!transitImage)
            {
                var overlay=new GameObject("Vehicle transit overlay",typeof(Canvas));overlay.transform.SetParent(transform,false);
                var canvas=overlay.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=21;
                var image=new GameObject("Moving vehicles",typeof(RectTransform),typeof(UnityEngine.UI.RawImage));image.transform.SetParent(overlay.transform,false);
                transitImage=image.GetComponent<UnityEngine.UI.RawImage>();transitImage.raycastTarget=false;
                var rect=transitImage.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            }
            transitImage.texture=transitTexture;transitImage.enabled=true;
            transitCamera.transform.SetPositionAndRotation(vehicleCamera.transform.position,vehicleCamera.transform.rotation);
            transitCamera.enabled=true;transitFrame=Time.frameCount;
            gameCamera.cullingMask&=~(1<<layer);vehicleCamera.cullingMask&=~(1<<layer);slotCamera.cullingMask&=~(1<<layer);
            float p=Mathf.Clamp01(progress);
            var endCamera=finalLeg?slotCamera:vehicleCamera;
            Vector3 start=vehicleCamera.WorldToScreenPoint(world.TransformPoint(from));
            Vector3 end=endCamera.WorldToScreenPoint(world.TransformPoint(to));
            // Preserve the collision-free outer route; never clamp it across parked vehicles.
            var screen=Vector3.Lerp(start,end,p);screen.z=transitCamera.nearClipPlane+10;
            var trail=vehicle.GetComponent<TrailRenderer>();if(trail){trail.emitting=false;trail.Clear();}
            SetViewLayer(vehicle,"Vehicle Transit");vehicle.position=transitCamera.ScreenToWorldPoint(screen);
            float boardPixels=vehicleCamera.pixelHeight/(2*vehicleCamera.orthographicSize);
            float slotPixels=endCamera.pixelHeight/(2*endCamera.orthographicSize);
            float pixels=transitCamera.pixelHeight/(2*transitCamera.orthographicSize);
            vehicle.localScale=originalScale*(boardPixels/pixels);
        }
        void EnsureSlotCamera()
        {
            if(!slotCamera)
            {
                var root=new GameObject("Slot Camera");root.transform.SetParent(transform,false);
                slotCamera=root.AddComponent<Camera>();
            }
            slotCamera.CopyFrom(vehicleCamera);slotCamera.depth=vehicleCamera.depth+.1f;
            slotCamera.transform.rotation=vehicleCamera.transform.rotation;
            slotCamera.enabled=vehicleCamera.enabled;
            int slot=LayerMask.NameToLayer("Slot View"),rescue=LayerMask.NameToLayer("Rescue View"),board=LayerMask.NameToLayer("Vehicle View");
            slotCamera.cullingMask=~((1<<board)|(1<<rescue)|(1<<5));
            vehicleCamera.cullingMask=~((1<<slot)|(1<<rescue)|(1<<5));
        }
        public static void SetViewLayer(Transform root,string layerName)
        {
            int layer=LayerMask.NameToLayer(layerName);
            if(!root||layer<0)return;
            foreach(var child in root.GetComponentsInChildren<Transform>(true))child.gameObject.layer=layer;
        }
        public void ConfigureCameraVisibility()
        {
            int rescue=LayerMask.NameToLayer("Rescue View"),parking=LayerMask.NameToLayer("Vehicle View"),slot=LayerMask.NameToLayer("Slot View");
            if(rescue<0||parking<0)return;
            gameCamera.cullingMask=~((1<<parking)|(1<<slot)|(1<<5));
            vehicleCamera.cullingMask=~((1<<rescue)|(1<<slot)|(1<<5));
            SetViewLayer(dragon,"Rescue View");SetViewLayer(kitten,"Rescue View");
            if(duckRescue)SetViewLayer(duckRescue.transform,"Rescue View");
            foreach(var t in yarn)SetViewLayer(t,"Rescue View");
            foreach(var t in vehicles)SetViewLayer(t,"Vehicle View");
            foreach(var t in slots)SetViewLayer(t,"Slot View");
            SetViewLayer(hand,"Vehicle View");
            if(authoredLayout)
            {
                SetViewLayer(authoredLayout.tailTip,"Rescue View");
                // Reuse the original fog, aligned to the rescue view so its opaque
                // upper region covers the entrance even at a lower camera pitch.
                var fogPlane=authoredLayout.environment.transform.Find("Fog");
                if(fogPlane&&authoredLayout.fogBoundary)
                {
                    var bottom=authoredLayout.fogBoundary.position;
                    fogPlane.position=bottom+gameCamera.transform.up*15-gameCamera.transform.forward*12;
                    fogPlane.rotation=gameCamera.transform.rotation;
                    var parentScale=fogPlane.parent.lossyScale;
                    fogPlane.localScale=new Vector3(82/Mathf.Abs(parentScale.x),30/Mathf.Abs(parentScale.y),1);
                    SetViewLayer(fogPlane,"Rescue View");
                }
                foreach(var p in authoredLayout.paths)if(p.road)
                {
                    SetViewLayer(p.road.transform,"Rescue View");

                }
            }
            foreach(var t in counts)if(t){SetViewLayer(t.transform,"Slot View");t.transform.rotation=vehicleCamera.transform.rotation;}
            foreach(var icon in slotUnlockIcons)if(icon){SetViewLayer(icon.transform,"Slot View");icon.transform.rotation=vehicleCamera.transform.rotation;}
        }
    }
}



