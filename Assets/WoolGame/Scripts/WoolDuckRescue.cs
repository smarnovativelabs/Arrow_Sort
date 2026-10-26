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
        GroundSplinePath route;
        Transform board;
        WoolAuthoredLayout layout;
        float distance,target,clock,firstStop,secondStop;
        float motherWait,babyWait,motherJumpTime,babyJumpTime;
        bool motherJumping,babyJumping;
        int motherJumps,babyJumps;
        public bool MotherCaught {get;private set;}
        public bool BabyCaught {get;private set;}
        public bool BothCaught=>MotherCaught&&BabyCaught;
        bool celebrating;
        public int EscapeStage {get;private set;}
        public bool Walking {get;private set;}
        public float MotherDistance=>distance;
        public float BabyDistance=>Mathf.Min(route.Length,distance+babyLead);
        public void ResetForLevel(GroundSplinePath path,Transform world,WoolAuthoredLayout authored)
        {
            route=path;board=world;layout=authored;EscapeStage=0;Walking=false;clock=0;
            float end=Mathf.Max(0,route.Length-babyLead);
            firstStop=FindClearStop(route.Length*.5f,route.Length*.2f,end-2);
            secondStop=FindClearStop(Mathf.Lerp(firstStop,end,.5f),firstStop+.8f,end-.8f);
            distance=target=firstStop;MotherCaught=BabyCaught=celebrating=false;mother.gameObject.SetActive(true);baby.gameObject.SetActive(true);
            motherJumping=babyJumping=false;motherJumps=babyJumps=0;motherWait=1.3f;babyWait=2.2f;
            gameObject.SetActive(true);mother.localScale=baby.localScale=Vector3.one;
            Present(0);
        }
        public void Tick(float dt,float dragonDistance,float dragonSpeed)
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
            // The head centre passes halfway across each duck before it disappears.
            if(EscapeStage==2&&!Walking)
            {
                if(dragonDistance+.35f>=MotherDistance+.2f){MotherCaught=true;mother.gameObject.SetActive(false);}
                if(dragonDistance+.35f>=BabyDistance+.2f){BabyCaught=true;baby.gameObject.SetActive(false);}
            }
            Present(dt);
        }
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
            if(model&&clip)clip.SampleAnimation(model,Mathf.Repeat(time,clip.length));
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