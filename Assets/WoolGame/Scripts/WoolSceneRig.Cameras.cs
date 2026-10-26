using UnityEngine;
namespace WoolGame
{
    public sealed partial class WoolSceneRig
    {
        [Header("Portrait cameras")]
        public Camera vehicleCamera;
        public Camera uiCamera;
        [Range(25,65)] public float rescueCameraPitch=40;
        [Range(50,90)] public float vehicleCameraPitch=90;
        [Min(1)] public float vehicleCameraSize=15;
        [Range(.2f,.65f)] public float rescueScreenShare=.42f;
        public Camera CameraForScreenPoint(Vector2 point)
        {
            return vehicleCamera&&vehicleCamera.enabled&&vehicleCamera.pixelRect.Contains(point)?vehicleCamera:gameCamera;
        }
        public static void SetViewLayer(Transform root,string layerName)
        {
            int layer=LayerMask.NameToLayer(layerName);
            if(!root||layer<0)return;
            foreach(var child in root.GetComponentsInChildren<Transform>(true))child.gameObject.layer=layer;
        }
        public void ConfigureCameraVisibility()
        {
            int rescue=LayerMask.NameToLayer("Rescue View"),parking=LayerMask.NameToLayer("Vehicle View");
            if(rescue<0||parking<0)return;
            gameCamera.cullingMask=~((1<<parking)|(1<<5));
            vehicleCamera.cullingMask=~((1<<rescue)|(1<<5));
            SetViewLayer(dragon,"Rescue View");SetViewLayer(kitten,"Rescue View");
            if(duckRescue)SetViewLayer(duckRescue.transform,"Rescue View");
            foreach(var t in yarn)SetViewLayer(t,"Rescue View");
            foreach(var t in vehicles)SetViewLayer(t,"Vehicle View");
            foreach(var t in slots)SetViewLayer(t,"Vehicle View");
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
            foreach(var t in counts)if(t){SetViewLayer(t.transform,"Vehicle View");t.transform.rotation=vehicleCamera.transform.rotation;}
            foreach(var icon in slotUnlockIcons)if(icon)icon.transform.rotation=vehicleCamera.transform.rotation;
        }
    }
}



