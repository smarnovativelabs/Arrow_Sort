using UnityEngine;
namespace WoolGame
{
    [DefaultExecutionOrder(200)]
    public sealed partial class WoolSceneRig
    {
        Vector2 rescueMin,rescueMax,parkingMin,parkingMax,boardMin,boardMax,slotMin,slotMax;
        bool portraitBoundsReady;
        int cameraFitWidth,cameraFitHeight;
        Rect cameraFitSafe;
        float cameraFitAbilityTop;
        void LateUpdate()
        {
            if(transitCamera&&transitFrame!=Time.frameCount){transitCamera.enabled=false;if(transitImage)transitImage.enabled=false;}
            if(!Application.isPlaying||!portraitBoundsReady||!world||!world.gameObject.activeInHierarchy||!hud)return;
            if(cameraFitWidth!=Screen.width||cameraFitHeight!=Screen.height||cameraFitSafe!=Screen.safeArea||
                Mathf.Abs(hud.AbilityStripTopPixels-cameraFitAbilityTop)>.5f)
                FitPortraitCamera(Screen.width,Screen.height,Screen.safeArea);
        }


        Renderer portraitEnvironmentRenderer;
        Renderer portraitEdgeRenderer;
        Texture portraitEnvironmentTexture;
        MaterialPropertyBlock portraitEnvironmentTint;

        public Rect GameplayFrame {get;private set;}
        public float GameplayDesignHeight {get;private set;}
        public static Rect CompactPortraitFrame(Rect safe,float designHeight)
        {
            float scale=Mathf.Min(safe.width/1080f,safe.height/designHeight);
            var size=new Vector2(1080,designHeight)*scale;
            return new Rect(safe.center-size*.5f,size);
        }
        // Keep the authored ground geometry and UVs unchanged on every device.
        void MatchPortraitEnvironment()
        {
            if(!Application.isPlaying||portraitEnvironmentRenderer||!authoredLayout||!authoredLayout.environment)return;
            foreach(var renderer in authoredLayout.environment.GetComponentsInChildren<Renderer>(true))
                if(renderer.name=="Game_Background_Grass_NoPath")
                {
                    portraitEnvironmentRenderer=renderer;
                    portraitEnvironmentTexture=renderer.sharedMaterial.GetTexture("_BaseMap");
                    var edges=authoredLayout.environment.transform.Find("Screen background edges");
                    if(edges)portraitEdgeRenderer=edges.GetComponent<Renderer>();
                    portraitEnvironmentTint=new MaterialPropertyBlock();
                    UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering+=TintPortraitEnvironment;
                    break;
                }
        }
        void TintPortraitEnvironment(UnityEngine.Rendering.ScriptableRenderContext context,Camera camera)
        {
            if(!portraitEnvironmentRenderer||(camera!=gameCamera&&camera!=vehicleCamera&&camera!=slotCamera))return;
            portraitEnvironmentRenderer.GetPropertyBlock(portraitEnvironmentTint);
            bool upper=camera==gameCamera;
            portraitEnvironmentTint.SetColor("_BaseColor",upper?new Color(.68f,.78f,.60f,1):camera==slotCamera?Color.white:new Color(1,.975f,.92f,1));
            portraitEnvironmentTint.SetTexture("_BaseMap",camera!=vehicleCamera&&portraitEnvironmentTexture?portraitEnvironmentTexture:Texture2D.whiteTexture);
            portraitEnvironmentTint.SetVector("_BaseMap_ST",new Vector4(1,1,0,0));
            portraitEnvironmentRenderer.SetPropertyBlock(portraitEnvironmentTint);
            if(portraitEdgeRenderer)portraitEdgeRenderer.SetPropertyBlock(portraitEnvironmentTint);
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
            var pathTint=new MaterialPropertyBlock();pathTint.SetColor("_BaseColor",Color.white);
            if(authoredLayout&&authoredLayout.paths!=null)foreach(var path in authoredLayout.paths)
                if(path.road)foreach(var renderer in path.road.GetComponentsInChildren<Renderer>())renderer.SetPropertyBlock(pathTint);
            rescueMin=parkingMin=boardMin=Vector2.one*float.MaxValue;
            rescueMax=parkingMax=boardMax=Vector2.one*float.MinValue;
            slotMin=Vector2.one*float.MaxValue;slotMax=Vector2.one*float.MinValue;
            if(authoredLayout&&authoredLayout.paths!=null)
                foreach(var path in authoredLayout.paths)
                    if(path.road&&path.points!=null&&path.road.activeSelf)
                        foreach(var point in path.FramingPoints)
                        {
                            var p=path.road.transform.TransformPoint(point);
                            // Fog-hidden entrance geometry must not reserve visible screen space.
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
                Include(vehicleCamera,slot.position,1.05f,ref slotMin,ref slotMax);
                Include(vehicleCamera,slot.position+Vector3.up*1.8f,.4f,ref slotMin,ref slotMax);
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
                    Include(vehicleCamera,p,.90f,ref boardMin,ref boardMax);
                }
            }
            if(parkingMin.x==float.MaxValue)
            {
                Include(vehicleCamera,world.TransformPoint(new Vector3(-5,-10,0)),.5f,ref parkingMin,ref parkingMax);
                Include(vehicleCamera,world.TransformPoint(new Vector3(5,-2,0)),.5f,ref parkingMin,ref parkingMax);
            }
            if(definition&&definition.hasRail)Include(vehicleCamera,world.TransformPoint(ParkingPresentationOffset+new Vector3(0,WoolEndlessGenerator.RailY,-.3f)),1.2f,ref parkingMin,ref parkingMax);
            if(boardMin.x==float.MaxValue){boardMin=parkingMin;boardMax=parkingMax;}
            if(definition&&definition.tutorial)
            {
                float center=(boardMin.x+boardMax.x)*.5f;
                float halfWidth=Mathf.Max((boardMax.x-boardMin.x)*.5f,world.TransformVector(Vector3.right*5).magnitude);
                boardMin.x=center-halfWidth;boardMax.x=center+halfWidth;
            }

            // Fit the full downward exit lane, including the rotating vehicle body
            // and trail width, into the board above the ability controls.
            if(ParkingTravelReady&&world)
            {
                float scale=Mathf.Max(Mathf.Abs(world.lossyScale.x),Mathf.Abs(world.lossyScale.y),Mathf.Abs(world.lossyScale.z));
                float radius=(TravelRadius+.2f)*scale;
                var left=Project(vehicleCamera,world.TransformPoint(new Vector3(TravelLeft,TravelBottom,-.3f)));
                var right=Project(vehicleCamera,world.TransformPoint(new Vector3(TravelRight,TravelBottom,-.3f)));
                parkingMin.y=Mathf.Min(parkingMin.y,Mathf.Min(left.y,right.y)-radius);
                // Keep the original board framing and vehicle readability.
                // Source art is sized to the board, never the other way around.
            }

            // A small visible shoulder around every layout leaves room to exit and turn.
            float laneScale=world?Mathf.Max(Mathf.Abs(world.lossyScale.x),Mathf.Abs(world.lossyScale.y)):.3f;
            boardMin-=new Vector2(.65f,.35f)*laneScale;
            boardMax+=new Vector2(.65f,1.15f)*laneScale;
            // Keep the same model proportions inside the expanded movement area.
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
            // Fit each region to its actual pixel aspect without stretching geometry.
            float fitAspect=camera.aspect;
            float size=Mathf.Max(extent.y/(2*localH),extent.x/(2*fitAspect*localW));
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
            // Camera-only adaptation: keep model transforms and authored geometry intact.
            const float designWidth=1080,padding=18;
            // A uniform reference scale also keeps the control reserve consistent
            // on tablets; camera viewports still use the full safe screen width.
            float referenceWidth=Mathf.Min(safe.width,safe.height*(1080f/2400f));
            GameplayFrame=new Rect(safe.center.x-referenceWidth*.5f,safe.yMin,referenceWidth,safe.height);
            GameplayDesignHeight=safe.height/(referenceWidth/designWidth);
            float scale=referenceWidth/designWidth;
            // Bound the playfield on wide tablets; scenery remains full bleed.
            float playWidth=safe.width;
            float left=(safe.center.x-playWidth*.5f)/width,right=(safe.center.x+playWidth*.5f)/width;
            float bottom=(safe.yMin+260*scale)/height;
            float top=safe.yMax/height;
            if(hud)
            {
                hud.ApplyPortraitFrame(width,height,safe);
                hud.ApplyCompactGameplayFrame(GameplayFrame,GameplayDesignHeight,width,height);
                Canvas.ForceUpdateCanvases();
                hud.ClampMobileControls();
                Canvas.ForceUpdateCanvases();
                bottom=(hud.AbilityStripTopPixels+18*scale)/height;
            }
            bottom=Mathf.Min(bottom,top-.25f);
            bool tablet=safe.width/safe.height>=.60f;
            float topPadding=tablet?40:56;
            float rescueAspect=(rescueMax.y-rescueMin.y)/Mathf.Max(.1f,rescueMax.x-rescueMin.x);
            float boardAspect=(boardMax.y-boardMin.y)/Mathf.Max(.1f,boardMax.x-boardMin.x);
            float slotAspect=(slotMax.y-slotMin.y)/Mathf.Max(.1f,slotMax.x-slotMin.x);
            float aspectSum=rescueAspect+boardAspect+slotAspect;
            // Allocate each region by its real aspect, giving the path, slots and
            // vehicle board the same visible width on phones and wide tablets.
            playWidth=Mathf.Min(safe.width-2*padding*scale,((top-bottom)*height-(topPadding+28)*scale)/Mathf.Max(.1f,aspectSum));
            if(hud&&playWidth>0)
            {
                GameplayFrame=new Rect(safe.center.x-playWidth*.5f,safe.yMin,playWidth,safe.height);
                GameplayDesignHeight=safe.height/(playWidth/designWidth);scale=playWidth/designWidth;
                hud.ApplyCompactGameplayFrame(GameplayFrame,GameplayDesignHeight,width,height);
                Canvas.ForceUpdateCanvases();hud.ClampMobileControls();Canvas.ForceUpdateCanvases();
                bottom=(hud.AbilityStripTopPixels+18*scale)/height;
                playWidth=Mathf.Min(safe.width-2*padding*scale,((top-bottom)*height-(topPadding+28)*scale)/Mathf.Max(.1f,aspectSum));
            }
            playWidth=Mathf.Max(64,playWidth);
            left=(safe.center.x-playWidth*.5f)/width;right=(safe.center.x+playWidth*.5f)/width;
            float split=top-(rescueAspect*playWidth+(topPadding+8)*scale)/height;
            float slotHeight=(slotAspect*playWidth+8*scale)/height;
            float boardTop=split-slotHeight;
            var rescueViewport=Rect.MinMaxRect(0,split,1,1);
            var boardViewport=Rect.MinMaxRect(0,0,1,boardTop);
            vehicleCamera.enabled=true;
            MatchPortraitEnvironment();
            FitArea(gameCamera,rescueViewport,Rect.MinMaxRect(left,split+4*scale/height,right,top-topPadding*scale/height),rescueMin,rescueMax,width,height,true);
            FitArea(vehicleCamera,boardViewport,Rect.MinMaxRect(left,bottom,right,boardTop-8*scale/height),boardMin,boardMax,width,height,false);
            FitRescueHaze();
            vehicleCamera.backgroundColor=gameCamera.backgroundColor;vehicleCamera.depth=gameCamera.depth-1;
            EnsureSlotCamera();
            var slotViewport=Rect.MinMaxRect(0,boardTop,1,split);
            FitArea(slotCamera,slotViewport,Rect.MinMaxRect(left,boardTop+4*scale/height,right,split-4*scale/height),slotMin,slotMax,width,height,true);
            cameraFitWidth=width;cameraFitHeight=height;cameraFitSafe=safeArea;
            cameraFitAbilityTop=hud?hud.AbilityStripTopPixels:0;
            PositionSlotCounts(width,height);
            if(hud)
            {
                float slotBottom=1;
                if(slots!=null)foreach(var slot in slots)if(slot)
                {
                    float y=SlotCamera.WorldToViewportPoint(slot.position).y;
                    slotBottom=Mathf.Min(slotBottom,SlotCamera.rect.y+y*SlotCamera.rect.height);
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
                rescueHazeMaterial.SetFloat("_Density",1f);
                rescueHazeMaterial.SetFloat("_MinimumDensity",.138f);
                rescueHazeMaterial.SetFloat("_EndFade",0f);
                rescueHazeMaterial.SetFloat("_SideFade",.12f);
                rescueHazeMaterial.SetFloat("_StartFade",.557f);
                root.GetComponent<Renderer>().sharedMaterial=rescueHazeMaterial;
                root.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            // Shared world-space band: no fog below lower, full white above upper.
            Vector3 upper=authoredLayout&&authoredLayout.fogBoundary?authoredLayout.fogBoundary.position:Vector3.zero;
            upper.y=.8f;
            // The boundary marks the thick upper fog; the visible band extends below it.
            Vector3 lower=upper+Vector3.forward*2f;
            float bottom=gameCamera.WorldToViewportPoint(lower).y;
            // Keep the final endpoint clear on every authored route.
            if(authoredLayout&&authoredLayout.paths!=null)
                foreach(var path in authoredLayout.paths)
                    if(path.road&&path.road.activeSelf&&path.points!=null&&path.points.Length>0)
                    {
                        var endpoint=path.road.transform.TransformPoint(path.points[path.points.Length-1])+Vector3.up*.8f;
                        bottom=Mathf.Max(bottom,gameCamera.WorldToViewportPoint(endpoint).y+.035f);
                    }
            float heavy=gameCamera.WorldToViewportPoint(upper).y;
            float top=Mathf.Max(1.2f,heavy+.15f);
            float halfHeight=gameCamera.orthographicSize;
            float halfWidth=halfHeight*gameCamera.aspect;
            rescueHazeMaterial.SetFloat("_SideFade",0);
            rescueHazeMaterial.SetFloat("_EndFade",0);
            rescueHazeMaterial.SetFloat("_StartFade",.557f);
            rescueHaze.localPosition=new Vector3(0,((bottom+top)*.5f-.5f)*2*halfHeight,gameCamera.nearClipPlane+1);
            rescueHaze.localRotation=Quaternion.identity;
            rescueHaze.localScale=new Vector3(2*halfWidth+1,(top-bottom)*2*halfHeight,1);
        }

        void OnDestroy()
        {
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering-=TintPortraitEnvironment;
            if(rescueHazeMaterial)Destroy(rescueHazeMaterial);
            if(transitTexture){transitTexture.Release();Destroy(transitTexture);}
        }
    }
}







