using UnityEngine;
namespace WoolGame
{
    public sealed partial class WoolArrowPool
    {
        public WoolSceneRig sceneRig;
        Vector2 flightStartScreen;
        Vector3 flightWorldScale;
        float flightDuration,flightProgress;
        Camera flightCamera;
        Vector2 ScreenUV(Camera camera,Vector3 position)
        {
            var p=camera.WorldToViewportPoint(position);
            return new Vector2(camera.rect.x+p.x*camera.rect.width,camera.rect.y+p.y*camera.rect.height);
        }
        Vector3 FromScreenUV(Camera camera,Vector2 uv,float depth)
        {
            var rect=camera.rect;
            return camera.ViewportToWorldPoint(new Vector3((uv.x-rect.x)/rect.width,(uv.y-rect.y)/rect.height,depth));
        }
        Vector3 ProjectedAim(Vector3 destination,Vector3 origin)
        {
            if(!sceneRig||!sceneRig.vehicleCamera)return destination-origin;
            var camera=sceneRig.vehicleCamera;
            return FromScreenUV(camera,ScreenUV(sceneRig.gameCamera,destination),camera.WorldToViewportPoint(origin).z)-origin;
        }
        void BeginProjectedFlight()
        {
            flightProgress=0;
            flightWorldScale=arrow.lossyScale;
            flightDuration=Mathf.Clamp(Vector3.Distance(arrow.position,target.position)/arrowSpeed,.28f,.65f);
            flightCamera=sceneRig?sceneRig.vehicleCamera:null;
            if(flightCamera)
            {
                flightStartScreen=ScreenUV(flightCamera,arrow.position);
                // Keep the authored arrow silhouette readable on portrait phones.
                var renderers=arrow.GetComponentsInChildren<MeshRenderer>();
                if(renderers.Length>0)
                {
                    var bounds=renderers[0].bounds;
                    for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
                    float pixelsPerUnit=flightCamera.pixelHeight/(2*flightCamera.orthographicSize);
                    float pixels=bounds.size.magnitude*pixelsPerUnit;
                    flightWorldScale*=36f*(Screen.width/900f)/Mathf.Max(1,pixels);
                }
            }
            WoolSceneRig.SetViewLayer(arrow,"Vehicle View");
        }
        bool AdvanceProjectedFlight(Vector3 end,float dt)
        {
            if(!sceneRig||!sceneRig.vehicleCamera)
            {
                arrow.position=Vector3.MoveTowards(arrow.position,end,arrowSpeed*dt);
                return (arrow.position-end).sqrMagnitude<=.0025f;
            }
            // World-space trails cannot span the two camera coordinate systems.
            visual.trail.emitting=false;
            visual.trail.Clear();
            flightProgress=Mathf.Min(1,flightProgress+dt/flightDuration);
            var destination=ScreenUV(sceneRig.gameCamera,end);
            var screen=Vector2.Lerp(flightStartScreen,destination,flightProgress);
            var camera=screen.y>=sceneRig.gameCamera.rect.yMin?sceneRig.gameCamera:sceneRig.vehicleCamera;
            if(camera!=flightCamera)
            {
                // A trail cannot join points expressed in two different projections.
                visual.trail.Clear();flightCamera=camera;
                WoolSceneRig.SetViewLayer(arrow,camera==sceneRig.gameCamera?"Rescue View":"Vehicle View");
            }
            // This is a screen-projected flight between two independent world views.
            // Target depth would bury its early upper-view positions below the ground.
            // A foreground plane preserves muzzle/impact alignment without terrain occlusion.
            float depth=camera.nearClipPlane+5f;
            var position=FromScreenUV(camera,screen,depth);
            var direction=FromScreenUV(camera,destination,depth)-position;
            if(direction.sqrMagnitude>.000001f)arrow.rotation=Quaternion.LookRotation(direction,-camera.transform.forward);
            arrow.position=position;
            float screenScale=(camera.orthographicSize/camera.rect.height)/(sceneRig.vehicleCamera.orthographicSize/sceneRig.vehicleCamera.rect.height);
            var parentScale=arrow.parent.lossyScale;
            var scale=flightWorldScale*screenScale;
            arrow.localScale=new Vector3(scale.x/parentScale.x,scale.y/parentScale.y,scale.z/parentScale.z);
            return flightProgress>=1;
        }
    }
}
