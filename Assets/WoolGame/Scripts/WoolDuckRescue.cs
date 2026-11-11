using UnityEngine;

namespace WoolGame
{
    // Path distances, not straight-line proximity, drive the two escapes.
    public sealed class WoolDuckRescue : MonoBehaviour
    {
        public Transform mother,baby;
        public GameObject motherModel,babyModel;
        public AnimationClip motherJump,motherWalk,babyJump,babyWalk,motherEat,babyEat;
        [Min(.1f)] public float approachDistance=2.4f;
        [Min(.1f)] public float babyLead=.9f;
        [Min(.1f)] public float walkSpeed=3;
        WoolHud headerHud;Camera duckCamera;GroundSplinePath route;
        Transform board;
        WoolAuthoredLayout layout;
        Renderer[] motherGeometry,babyGeometry;
        float distance,target,clock,firstStop,secondStop;
        float safeDistance,safeTarget;int safeStage;bool safeWalking;
        float motherWait,babyWait,motherJumpTime,babyJumpTime;
        bool motherJumping,babyJumping;
        int motherJumps,babyJumps;
        public bool MotherCaught {get;private set;}
        public bool BabyCaught {get;private set;}
        public bool BothCaught=>MotherCaught&&BabyCaught;
        public event System.Action DuckCaught;
        bool celebrating;
        public int EscapeStage {get;private set;}
        public bool Walking {get;private set;}
        public float MotherDistance=>distance;
        public float BabyDistance=>Mathf.Min(route.Length,distance+babyLead);
        public float MotherCaptureDistance=>BodyCaptureDistance(motherGeometry,MotherDistance,.25f);
        public float BabyCaptureDistance=>BodyCaptureDistance(babyGeometry,BabyDistance,1f);
        float BodyCaptureDistance(Renderer[] geometry,float pathDistance,float fraction)
        {
            if(!route||!board||geometry==null||geometry.Length==0)return pathDistance;
            var bounds=geometry[0].bounds;
            for(int i=1;i<geometry.Length;i++)bounds.Encapsulate(geometry[i].bounds);
                        // Measure visible geometry along the path, from the edge nearest the dragon.
            var origin=route.Position(pathDistance);var tangent=route.Tangent(pathDistance).normalized;
            float near=float.PositiveInfinity,far=float.NegativeInfinity;
            for(int corner=0;corner<8;corner++){
                var sign=new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1);
                var point=board.InverseTransformPoint(bounds.center+Vector3.Scale(bounds.extents,sign));
                float along=Vector3.Dot(point-origin,tangent);near=Mathf.Min(near,along);far=Mathf.Max(far,along);
            }
            return pathDistance+Mathf.Lerp(near,far,Mathf.Clamp01(fraction));
        }
        public void ShiftRouteOrigin(float delta){firstStop+=delta;secondStop+=delta;distance+=delta;target+=delta;}
        public void ResetForLevel(GroundSplinePath path,Transform world,WoolAuthoredLayout authored)
        {
            headerHud=FindFirstObjectByType<WoolHud>();var rig=FindFirstObjectByType<WoolSceneRig>();duckCamera=rig?rig.gameCamera:Camera.main;route=path;board=world;layout=authored;EscapeStage=0;Walking=false;clock=0;
            if(motherGeometry==null)motherGeometry=motherModel.GetComponentsInChildren<Renderer>(true);
            if(babyGeometry==null)babyGeometry=babyModel.GetComponentsInChildren<Renderer>(true);
            float end=Mathf.Max(0,route.Length-babyLead);
            firstStop=FindClearStop(route.EntryExtension+route.PlayableLength*.5f+.65f,route.EntryExtension+route.PlayableLength*.2f,end-2);
            secondStop=FindClearStop(Mathf.Lerp(firstStop,end,.5f),firstStop+.8f,end-.8f);
            distance=target=firstStop;MotherCaught=BabyCaught=celebrating=false;mother.gameObject.SetActive(true);baby.gameObject.SetActive(true);
            motherJumping=babyJumping=false;motherJumps=babyJumps=0;motherWait=1.3f;babyWait=2.2f;
            gameObject.SetActive(true);mother.localScale=baby.localScale=Vector3.one;
            Present(0);
            RememberSafePoint();
        }
        public void Tick(float dt,float dragonDistance,float dragonSpeed,float mouthLead=.35f)
        {
            if(!route)return;
            if(!Walking&&EscapeStage<2&&distance-dragonDistance<=approachDistance)
            {
                EscapeStage++;
                float end=Mathf.Max(0,route.Length-babyLead);
                target=EscapeStage==1?secondStop:end;
                Walking=true;clock=0;
            }
            if(Walking)
            {
                distance=Mathf.MoveTowards(distance,target,dt*Mathf.Max(walkSpeed,dragonSpeed*4)*.5f);
                if(Mathf.Abs(distance-target)<.001f){Walking=false;clock=0;motherJumping=babyJumping=false;motherWait=1.1f;babyWait=1.9f;}
            }
            Present(dt);
            if(!MotherCaught&&!BabyCaught)RememberSafePoint();
            // Parent disappears at 25% capture; the child stays until fully captured.
            if(EscapeStage==2&&!Walking)
            {
                if(!MotherCaught&&dragonDistance+mouthLead>=MotherCaptureDistance){MotherCaught=true;mother.gameObject.SetActive(false);DuckCaught?.Invoke();}
                if(!BabyCaught&&dragonDistance+mouthLead>=BabyCaptureDistance){BabyCaught=true;baby.gameObject.SetActive(false);DuckCaught?.Invoke();}
            }
        }
        void RememberSafePoint(){safeDistance=distance;safeTarget=target;safeStage=EscapeStage;safeWalking=Walking;}
        public void RestoreLastSafePoint(){if(!route)return;distance=safeDistance;target=safeTarget;EscapeStage=safeStage;Walking=safeWalking;MotherCaught=BabyCaught=celebrating=false;clock=0;motherJumping=babyJumping=false;motherWait=1.1f;babyWait=1.9f;gameObject.SetActive(true);mother.gameObject.SetActive(true);baby.gameObject.SetActive(true);mother.localScale=baby.localScale=Vector3.one;Present(0);}
        public void Celebrate(float dt)
        {
            if(!celebrating){clock=0;celebrating=true;}
            Walking=false;clock+=dt;
            Sample(motherModel,motherJump,clock);Sample(babyModel,babyJump,clock+.15f);
            Place(mother,distance);Place(baby,BabyDistance);
        }
        void Present(float dt)
        {
            clock+=dt;
            if(Walking){Sample(motherModel,motherWalk,clock);Sample(babyModel,babyWalk,clock+.17f);}
            else
            {
                Idle(motherModel,motherJump,motherEat,dt,ref motherWait,ref motherJumpTime,ref motherJumping,ref motherJumps,2.6f);
                Idle(babyModel,babyJump,babyEat,dt,ref babyWait,ref babyJumpTime,ref babyJumping,ref babyJumps,1.9f);
            }
            Place(mother,distance);Place(baby,BabyDistance);
        }
        static void Sample(GameObject model,AnimationClip clip,float time)
        {
            if(model&&clip){var driver=model.GetComponent<WoolDuckAnimationDriver>();if(!driver)driver=model.AddComponent<WoolDuckAnimationDriver>();driver.Sample(clip,time);}
        }
        static void Idle(GameObject model,AnimationClip jump,AnimationClip eat,float dt,ref float wait,ref float time,ref bool animating,ref int cycles,float rest)
        {
            if(!model||!jump)return;
            // Two feeding cycles followed by a playful jump; mother/baby are staggered.
            var clip=cycles%3==2||!eat?jump:eat;
            if(!animating){wait-=dt;if(wait<=0){animating=true;time=0;}}
            if(animating)
            {
                time+=dt;
                if(time>=clip.length){animating=false;wait=rest+(++cycles%3)*.35f;}
            }
            Sample(model,animating?clip:jump,animating?Mathf.Min(time,clip.length-.001f):0);
        }
        public bool ClearStandingPoint(float d)
        {
                        if(headerHud&&duckCamera&&board&&route){
                for(int duck=0;duck<2;duck++){
                    var position=board.TransformPoint(route.Position(Mathf.Min(route.Length,d+(duck==0?0:babyLead))));
                    var feet=duckCamera.WorldToScreenPoint(position);var head=duckCamera.WorldToScreenPoint(position+Vector3.up*1.3f);
                    float radius=Mathf.Max(24,Mathf.Abs(head.y-feet.y)*.55f);
                    if(headerHud.GameplayHeaderOverlaps(Rect.MinMaxRect(Mathf.Min(feet.x,head.x)-radius,Mathf.Min(feet.y,head.y)-12,Mathf.Max(feet.x,head.x)+radius,Mathf.Max(feet.y,head.y)+12)))return false;
                }
            }
            if(!layout||!layout.fogBoundary)return true;
            // Leave room for both silhouettes at the soft edge, not just their pivots.
            float edge=layout.fogBoundary.position.z+.8f;
            return board.TransformPoint(route.Position(d)).z>edge&&board.TransformPoint(route.Position(Mathf.Min(route.Length,d+babyLead))).z>edge;
        }
        float FindClearStop(float desired,float min,float max)
        {
            desired=Mathf.Clamp(desired,min,max);
            if(ClearStandingPoint(desired))return desired;
            float best=desired,error=float.MaxValue;
            for(float d=min;d<=max;d+=.05f)if(ClearStandingPoint(d)&&Mathf.Abs(d-desired)<error){best=d;error=Mathf.Abs(d-desired);}
            return best;
        }
        void Place(Transform actor,float d)
        {
            actor.position=board.TransformPoint(route.Position(d)+Vector3.back*.02f);
            Vector3 forward=board.TransformDirection(route.Tangent(d));forward.y=0;
            if(forward.sqrMagnitude>.0001f)actor.rotation=Quaternion.LookRotation(forward,Vector3.up);
        }
    }
}
