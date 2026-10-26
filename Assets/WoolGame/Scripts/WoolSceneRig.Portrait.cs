using UnityEngine;
namespace WoolGame
{
    public sealed partial class WoolSceneRig
    {
        Vector2 rescueMin,rescueMax,parkingMin,parkingMax,boardMin,boardMax;
        bool portraitBoundsReady;


        Renderer portraitEnvironmentRenderer;
        MaterialPropertyBlock portraitEnvironmentTint;
        MeshFilter portraitEnvironmentFilter;
        Mesh portraitEnvironmentMesh,originalEnvironmentMesh;

        public Rect GameplayFrame {get;private set;}
        public float GameplayDesignHeight {get;private set;}
        public static Rect CompactPortraitFrame(Rect safe,float designHeight)
        {
            float scale=Mathf.Min(safe.width/1080f,safe.height/designHeight);
            var size=new Vector2(1080,designHeight)*scale;
            return new Rect(safe.center-size*.5f,size);
        }
        // The authored background already has clamped edge geometry. Render it
        // through the margins while fitting gameplay into the original frame.
        void MatchPortraitEnvironment()
        {
            if(!Application.isPlaying||portraitEnvironmentRenderer||!authoredLayout||!authoredLayout.environment)return;
            foreach(var renderer in authoredLayout.environment.GetComponentsInChildren<Renderer>(true))
                if(renderer.name=="Game_Background_Grass_NoPath")
                {
                    portraitEnvironmentRenderer=renderer;
                    portraitEnvironmentTint=new MaterialPropertyBlock();
                    portraitEnvironmentFilter=renderer.GetComponent<MeshFilter>();
                    if(portraitEnvironmentFilter&&portraitEnvironmentFilter.sharedMesh)
                    {
                        originalEnvironmentMesh=portraitEnvironmentFilter.sharedMesh;
                        portraitEnvironmentMesh=Instantiate(originalEnvironmentMesh);
                        portraitEnvironmentMesh.name="Portrait background edge continuation";
                        var vertices=portraitEnvironmentMesh.vertices;var bounds=portraitEnvironmentMesh.bounds;
                        // Only extend the UV-clamped outer border. The center artwork
                        // and gameplay retain their authored size and position.
                        for(int i=0;i<vertices.Length;i++)
                        {
                            if(Mathf.Approximately(vertices[i].x,bounds.min.x)||Mathf.Approximately(vertices[i].x,bounds.max.x))vertices[i].x=bounds.center.x+(vertices[i].x-bounds.center.x)*10;
                            if(Mathf.Approximately(vertices[i].z,bounds.min.z)||Mathf.Approximately(vertices[i].z,bounds.max.z))vertices[i].z=bounds.center.z+(vertices[i].z-bounds.center.z)*10;
                        }
                        portraitEnvironmentMesh.vertices=vertices;portraitEnvironmentMesh.RecalculateBounds();
                        portraitEnvironmentFilter.sharedMesh=portraitEnvironmentMesh;
                    }
                    UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering+=TintPortraitEnvironment;
                    break;
                }
        }
        void TintPortraitEnvironment(UnityEngine.Rendering.ScriptableRenderContext context,Camera camera)
        {
            if(!portraitEnvironmentRenderer||(camera!=gameCamera&&camera!=vehicleCamera))return;
            portraitEnvironmentRenderer.GetPropertyBlock(portraitEnvironmentTint);
            portraitEnvironmentTint.SetColor("_BaseColor",camera==gameCamera?new Color(.72f,.84f,.62f,1):Color.white);
            portraitEnvironmentRenderer.SetPropertyBlock(portraitEnvironmentTint);
        }
        static Vector2 Project(Camera camera,Vector3 point)
        {
            return new Vector2(Vector3.Dot(point,camera.transform.right),Vector3.Dot(point,camera.transform.up));
        }
        static void Include(Camera camera,Vector3 point,float padding,ref Vector2 min,ref Vector2 max)
        {
            var p=Project(camera,point);
            min=Vector2.Min(min,p-Vector2.one*padding);max=Vector2.Max(max,p+Vector2.one*padding);
        }
        public void CapturePortraitBounds(WoolLevelDefinition definition)
        {
            if(!gameCamera||!vehicleCamera)return;
            gameCamera.transform.rotation=Quaternion.Euler(rescueCameraPitch,180,0);
            vehicleCamera.transform.rotation=Quaternion.Euler(vehicleCameraPitch,180,0);
            ConfigureCameraVisibility();
            rescueMin=parkingMin=boardMin=Vector2.one*float.MaxValue;
            rescueMax=parkingMax=boardMax=Vector2.one*float.MinValue;
            if(authoredLayout&&authoredLayout.paths!=null)
                foreach(var path in authoredLayout.paths)
                    if(path.road&&path.points!=null&&path.road.activeSelf)
                        foreach(var point in path.points)
                        {
                            var p=path.road.transform.TransformPoint(point);
                            if(authoredLayout.IsInFog(p))continue;
                            Include(gameCamera,p,.85f,ref rescueMin,ref rescueMax);
                            Include(gameCamera,p+Vector3.up*1.8f,.35f,ref rescueMin,ref rescueMax);
                        }
            if(duckRescue){Include(gameCamera,duckRescue.mother.position,1.2f,ref rescueMin,ref rescueMax);Include(gameCamera,duckRescue.baby.position,1f,ref rescueMin,ref rescueMax);}
            else if(kitten)Include(gameCamera,kitten.position,1.5f,ref rescueMin,ref rescueMax);
            if(slots!=null)foreach(var slot in slots)if(slot)
            {
                Include(vehicleCamera,slot.position,.95f,ref parkingMin,ref parkingMax);
                Include(vehicleCamera,slot.position+Vector3.up*1.4f,.4f,ref parkingMin,ref parkingMax);
            }
            if(vehicles!=null)foreach(var vehicle in vehicles)
            {
                if(!vehicle||!vehicle.gameObject.activeInHierarchy)continue;
                var collider=vehicle.GetComponent<BoxCollider>();if(!collider)continue;
                var bounds=collider.bounds;
                for(int corner=0;corner<8;corner++)
                {
                    var p=bounds.center+Vector3.Scale(bounds.extents,new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1));
                    Include(vehicleCamera,p,.08f,ref parkingMin,ref parkingMax);
                    Include(vehicleCamera,p,.65f,ref boardMin,ref boardMax);
                }
            }
            if(parkingMin.x==float.MaxValue)
            {
                Include(vehicleCamera,world.TransformPoint(new Vector3(-5,-10,0)),.5f,ref parkingMin,ref parkingMax);
                Include(vehicleCamera,world.TransformPoint(new Vector3(5,-2,0)),.5f,ref parkingMin,ref parkingMax);
            }
            if(definition&&definition.hasRail)Include(vehicleCamera,world.TransformPoint(ParkingPresentationOffset+new Vector3(0,WoolEndlessGenerator.RailY,-.3f)),1.2f,ref parkingMin,ref parkingMax);
            // Fit visible geometry without an additional side-lane margin.
            portraitBoundsReady=rescueMax.y>rescueMin.y&&parkingMax.y>parkingMin.y;
        }
        static void FitArea(Camera camera,Rect viewport,Rect content,Vector2 min,Vector2 max,int width,int height,bool bottomAnchor,float requestedSize=0)
        {
            camera.rect=viewport;camera.orthographic=true;
            // Full-height portrait margins can look farther along the ground
            // than the old 70-unit clip distance at the rescue camera's pitch.
            camera.farClipPlane=Mathf.Max(200,camera.farClipPlane);
            camera.aspect=(float)width*viewport.width/(height*viewport.height);
            float localW=content.width/viewport.width,localH=content.height/viewport.height;
            var extent=max-min;
            float size=Mathf.Max(extent.y/(2*localH),extent.x/(2*camera.aspect*localW));
            if(requestedSize>0)size=Mathf.Max(requestedSize,size);
            camera.orthographicSize=Mathf.Max(.1f,size);
            float localX=(content.center.x-viewport.x)/viewport.width;
            float edgeY=((bottomAnchor?content.yMin:content.yMax)-viewport.y)/viewport.height;
            var center=new Vector2((min.x+max.x)*.5f-(localX-.5f)*2*size*camera.aspect,
                (bottomAnchor?min.y:max.y)-(edgeY-.5f)*2*size);
            // Fixed view-plane distance prevents near/far clipping after changing pitch.
            var t=camera.transform;
            t.position=t.right*center.x+t.up*center.y-t.forward*50;
        }
        // Screen-space overlay renders the HUD once, above both independent views.
        public void EnsureUICamera()
        {
            if(uiCamera)uiCamera.enabled=false;
            if(!hud)return;
            var canvas=hud.GetComponent<Canvas>();
            if(canvas&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay)
            {
                canvas.renderMode=RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera=null;
            }
        }
        public void FitPortraitCamera(int width,int height,Rect safeArea)
        {
            if(!gameCamera||!vehicleCamera||width<=0||height<=0)return;
            EnsureUICamera();
            var safe=WoolHud.PortraitPixels(width,height,safeArea);
            if(safe.width<=0||safe.height<=0)safe=new Rect(0,0,width,height);
            if(!portraitBoundsReady)return;
            // One reference composition on every device. Spare space belongs outside
            // this frame, so tablets cannot stretch the board or change its angle.
            const float designWidth=1080,header=0,footer=230,padding=24;
            const float rescueHeight=810,boardHeight=1240;
            GameplayDesignHeight=header+rescueHeight+boardHeight+footer;
            GameplayFrame=CompactPortraitFrame(safe,GameplayDesignHeight);
            float scale=GameplayFrame.width/designWidth;
            float left=GameplayFrame.xMin/width,right=GameplayFrame.xMax/width;
            float bottom=(GameplayFrame.yMin+footer*scale)/height;
            float split=bottom+boardHeight*scale/height;
            float top=split+rescueHeight*scale/height;
            var rescueViewport=Rect.MinMaxRect(0,split,1,1);
            var boardViewport=Rect.MinMaxRect(0,0,1,split);
            vehicleCamera.enabled=true;
            MatchPortraitEnvironment();
            FitArea(gameCamera,rescueViewport,Rect.MinMaxRect(left+padding*scale/width,split,right-padding*scale/width,top),rescueMin,rescueMax,width,height,true);
            FitArea(vehicleCamera,boardViewport,Rect.MinMaxRect(left+padding*scale/width,bottom,right-padding*scale/width,split-40*scale/height),parkingMin,parkingMax,width,height,false);
            FitRescueHaze();
            vehicleCamera.backgroundColor=gameCamera.backgroundColor;vehicleCamera.depth=gameCamera.depth-1;
            PositionSlotCounts(width,height);
            if(hud)
            {
                hud.ApplyPortraitFrame(width,height,safe);
                hud.ApplyCompactGameplayFrame(GameplayFrame,GameplayDesignHeight,width,height);
                float slotBottom=1;
                if(slots!=null)foreach(var slot in slots)if(slot)
                {
                    float y=vehicleCamera.WorldToViewportPoint(slot.position).y;
                    slotBottom=Mathf.Min(slotBottom,vehicleCamera.rect.y+y*vehicleCamera.rect.height);
                }
                hud.PositionTutorial(new Vector2(.5f,(slotBottom*height-GameplayFrame.yMin)/GameplayFrame.height));
            }
        }
        Transform rescueHaze;
        Material rescueHazeMaterial;
        void FitRescueHaze()
        {
            if(!Application.isPlaying)return;
            if(!rescueHaze)
            {
                var shader=Shader.Find("WoolGame/SoftFog");if(!shader)return;
                var root=GameObject.CreatePrimitive(PrimitiveType.Quad);root.name="Rescue atmospheric fade";
                Destroy(root.GetComponent<Collider>());rescueHaze=root.transform;
                rescueHaze.SetParent(gameCamera.transform,false);SetViewLayer(rescueHaze,"Rescue View");
                rescueHazeMaterial=new Material(shader);
                rescueHazeMaterial.SetColor("_Color",Color.white);
                rescueHazeMaterial.SetFloat("_Density",.55f);
                rescueHazeMaterial.SetFloat("_MinimumDensity",0);
                rescueHazeMaterial.SetFloat("_EndFade",.4f);
                rescueHazeMaterial.SetFloat("_StartFade",.95f);
                root.GetComponent<Renderer>().sharedMaterial=rescueHazeMaterial;
                root.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            rescueHaze.localPosition=new Vector3(0,0,gameCamera.nearClipPlane+1);
            rescueHaze.localRotation=Quaternion.identity;
            rescueHaze.localScale=new Vector3(2*gameCamera.orthographicSize*gameCamera.aspect,2*gameCamera.orthographicSize,1);
        }
        void OnDestroy()
        {
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering-=TintPortraitEnvironment;
            if(portraitEnvironmentFilter&&originalEnvironmentMesh)portraitEnvironmentFilter.sharedMesh=originalEnvironmentMesh;
            if(portraitEnvironmentMesh)Destroy(portraitEnvironmentMesh);
            if(rescueHazeMaterial)Destroy(rescueHazeMaterial);
        }
    }
}







