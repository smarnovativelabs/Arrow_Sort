using UnityEngine;
namespace WoolGame
{
    // One serial shot, reusing the authored slot weapons, arrows and shared effects.
    public sealed partial class WoolArrowPool : MonoBehaviour
    {
        public WoolAuthoredLayout authoredLayout;
        public ParticleSystem hitEffect, muzzleEffect;
        public AudioClip hitSound;
        [System.NonSerialized] public AudioClip shotSound,dockSound;
        public AudioSource audioSource;
        public Color[] shotColors;
        public MeshFilter impactSegment;
        public MeshRenderer impactRenderer;
        [Header("Shot timing")]
        [Min(1)] public float arrowSpeed=28;
        [Range(.06f,.10f)] public float prepareSeconds=.08f;
        [Range(.08f,.15f)] public float recoilSeconds=.12f;
        [Range(.06f,.15f)] public float impactSeconds=.10f;
        [Range(.04f,.2f)] public float recoverySeconds=.08f;
        [Header("Visual response")]
        [Range(0,.25f)] public float drawDistance=.14f;
        [Range(0,.2f)] public float recoilDistance=.10f;
        [Range(0,.4f)] public float targetPunch=.22f;
        [Min(.1f)] public float flightScale=1;
        enum ShotStage { Idle, Prepare, Travel }
        ShotStage stage;
        readonly Transform[] activeBows=new Transform[8];
        readonly int[] activeColors=new int[8];
        Transform arrow,target,returnParent,weapon;
        Renderer targetRenderer;
        MeshFilter targetMesh;
        WoolBowVisual visual,recovering;
        Transform recoveringArrow;
        int recoveringSlot=-1;
        Vector3 impactScale,recoveryStart;
        float impactTime,recoveryTime;
        MaterialPropertyBlock impactProperties;
        Vector3 returnPosition,returnScale;
        Quaternion returnRotation;
        float timer,flightTime;
        int shooter=-1,color;
        public bool Busy=>stage!=ShotStage.Idle;
        public bool InFlight=>stage==ShotStage.Travel;
        public int ShotsLaunched {get;private set;}
        public int Hits {get;private set;}
        void Awake()
        {
            if(hitEffect&&!hitEffect.gameObject.scene.IsValid())
            {
                var source=hitEffect.gameObject;
                hitEffect=Instantiate(source,transform).GetComponent<ParticleSystem>();
            }
            if(hitEffect)
            {
                var main=hitEffect.main;
                main.loop=false;main.playOnAwake=false;
                main.startSizeMultiplier*=1.6f;main.startLifetimeMultiplier*=1.3f;
            }
        }
        public void ResetPool()
        {
            Cancel();ShotsLaunched=Hits=0;
            if(impactSegment)impactSegment.gameObject.SetActive(false);
            recovering=null;recoveringArrow=null;
            if(impactProperties==null)impactProperties=new MaterialPropertyBlock();
            if(hitEffect)hitEffect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            if(muzzleEffect)muzzleEffect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            for(int i=0;i<authoredLayout.slots.Length;i++)
            {
                var slot=authoredLayout.slots[i];
                foreach(var bow in slot.colors)bow.gameObject.SetActive(false);
                for(int j=0;j<slot.arrows.Length;j++)
                {
                    slot.arrows[j].gameObject.SetActive(false);
                    var v=slot.visuals[j];slot.colors[j].localRotation=v.restRotation;v.trail.emitting=false;v.trail.Clear();
                    v.frame.localPosition=Vector3.zero;v.frame.localScale=Vector3.one;
                    slot.arrows[j].localPosition=v.loadedPosition;
                    slot.arrows[j].localRotation=Quaternion.identity;
                    slot.arrows[j].localScale=v.loadedScale;
                    SetString(v,0);
                    SampleBow(v,0);
                }
                slot.arrival.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                activeBows[i]=null;
            }
        }
        public void Dock(int slot,int color,Vector3 position)
        {
            var binding=authoredLayout.slots[slot];
            foreach(var variant in binding.colors)variant.gameObject.SetActive(false);
            activeBows[slot]=binding.colors[color];activeColors[slot]=color;
            binding.arrival.gameObject.SetActive(true);
            binding.arrival.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            binding.arrival.Play(true);
            if(dockSound&&audioSource)audioSource.PlayOneShot(dockSound,.3f);
        }
        public void Reveal(int slot)
        {
            activeBows[slot].gameObject.SetActive(true);
            authoredLayout.slots[slot].arrows[activeColors[slot]].gameObject.SetActive(true);
        }
        public void Release(int slot)
        {
            if(activeBows[slot])activeBows[slot].gameObject.SetActive(false);
            activeBows[slot]=null;
        }
        public void Launch(int slot,int shotColor,Transform destination)
        {
            if(Busy||!destination||authoredLayout.IsInFog(destination.position)||!activeBows[slot])return;
            shooter=slot;color=shotColor;target=destination;weapon=activeBows[slot];
            targetRenderer=target.GetComponent<Renderer>();targetMesh=target.GetComponent<MeshFilter>();
            var binding=authoredLayout.slots[slot];
            visual=binding.visuals[color];arrow=binding.arrows[color];
            if(recovering==visual){recovering=null;recoveringArrow=null;visual.frame.localPosition=Vector3.zero;}
            returnParent=arrow.parent;returnPosition=arrow.localPosition;
            returnRotation=arrow.localRotation;returnScale=arrow.localScale;
            var aim=ProjectedAim(target.position,weapon.position);aim.y=0;
            if(aim.sqrMagnitude>.0001f)weapon.rotation=Quaternion.LookRotation(aim,Vector3.up);
            arrow.gameObject.SetActive(true);
            timer=flightTime=0;stage=ShotStage.Prepare;
            SampleBow(visual,0);
        }
        public int Tick(float dt)
        {
            if(!Busy)return -1;
            dt=Mathf.Max(0,dt);timer+=dt;
            if(!target||!target.gameObject.activeInHierarchy||authoredLayout.IsInFog(target.position))
            {Cancel();return -1;}
            if(stage==ShotStage.Prepare)
            {
                float t=Mathf.Clamp01(timer/prepareSeconds);
                visual.frame.localScale=visual.animationRoot?Vector3.one:new Vector3(1-.035f*t,1,1-.07f*t);
                SampleBow(visual,t*.8f);
                arrow.localPosition=returnPosition-Vector3.forward*(drawDistance*t);
                SetString(visual,drawDistance*t);
                if(timer<prepareSeconds)return -1;
                arrow.SetParent(authoredLayout.environment.transform,true);
                BeginProjectedFlight();
                visual.trail.Clear();visual.trail.emitting=true;
                Emit(muzzleEffect,arrow.position-arrow.forward*.35f,ShotColor(),5,.12f);
                if(shotSound&&audioSource)audioSource.PlayOneShot(shotSound,.5f);
                ShotsLaunched++;dt=timer-prepareSeconds;timer=0;stage=ShotStage.Travel;
            }
            if(stage==ShotStage.Travel)
            {
                flightTime+=dt;
                float kick=Mathf.Sin(Mathf.Clamp01(flightTime/recoilSeconds)*Mathf.PI);
                visual.frame.localPosition=Vector3.back*(kick*recoilDistance);
                visual.frame.localScale=Vector3.one;
                SetString(visual,-.06f*kick);
                SampleBow(visual,.8f+Mathf.Clamp01(flightTime/recoilSeconds)*.2f);
                // Tip is the mesh origin: the impact point and visible arrowhead agree.
                var bounds=targetRenderer.bounds;
                var end=new Vector3(bounds.center.x,bounds.max.y+.08f,bounds.center.z);
                if(!AdvanceProjectedFlight(end,dt))return -1;
                ReturnArrow();
                PlayHitEffect(end+Vector3.up*.35f,ShotColor());
                if(hitSound&&audioSource)audioSource.PlayOneShot(hitSound,.35f);
                // Keep the original shot cadence: impact visuals outlive the committed hit.
                // The visual uses this segment's original FBX mesh, never a gameplay duplicate.
                if(impactSegment&&impactRenderer)
                {
                    impactSegment.sharedMesh=targetMesh.sharedMesh;
                    var renderer=targetRenderer;
                    impactRenderer.sharedMaterial=renderer.sharedMaterial;
                    renderer.GetPropertyBlock(impactProperties);impactRenderer.SetPropertyBlock(impactProperties);
                    impactSegment.transform.SetPositionAndRotation(target.position,target.rotation);
                    impactScale=target.lossyScale;impactSegment.transform.localScale=impactScale;
                    impactSegment.gameObject.SetActive(true);impactTime=0;
                }
                int result=shooter;Hits++;
                var recoveryVisual=visual;var loadedArrow=arrow;var recoil=visual.frame.localPosition;
                Finish();
                recovering=recoveryVisual;recoveringArrow=loadedArrow;recoveringSlot=result;
                recoveryStart=recoil;recovering.frame.localPosition=recoil;recoveryTime=0;
                SampleBow(recovering,.9f);
                return result;
            }
            return -1;
        }
        // Driven by the game controller, including the final hit after the win transition.
        public void TickPresentation(float dt)
        {
            // Editor script reloads can retain the recovery record after its
            // scene transform is replaced. Discard that stale presentation state.
            if(recovering!=null&&!recovering.frame)
            {recovering=null;recoveringArrow=null;recoveringSlot=-1;}
            if(impactSegment&&impactSegment.gameObject.activeSelf)
            {
                impactTime+=dt;float t=Mathf.Clamp01(impactTime/impactSeconds);
                float p=Mathf.Sin(t*Mathf.PI);
                impactSegment.transform.localScale=Vector3.Scale(impactScale,new Vector3(1+targetPunch*p,1-targetPunch*.45f*p,1+targetPunch*p));
                if(t>=1)impactSegment.gameObject.SetActive(false);
            }
            if(recovering!=null)
            {
                recoveryTime+=dt;float t=Mathf.Clamp01(recoveryTime/recoverySeconds);
                recovering.frame.localPosition=Vector3.Lerp(recoveryStart,Vector3.zero,t);
                SetString(recovering,0);
                SampleBow(recovering,Mathf.Lerp(.9f,authoredLayout.bowAttack?authoredLayout.bowAttack.length:1,t));
                if(t>=1)
                {
                    if(recoveringSlot>=0&&recoveringSlot<activeBows.Length&&activeBows[recoveringSlot]&&recoveringArrow)recoveringArrow.gameObject.SetActive(true);
                    recovering=null;recoveringArrow=null;
                }
            }
        }
        Color ShotColor()=>shotColors!=null&&color<shotColors.Length?shotColors[color]:Color.white;
        void PlayHitEffect(Vector3 at,Color tint)
        {
            if(!hitEffect)return;
            hitEffect.transform.position=at;
            hitEffect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=hitEffect.main;main.loop=false;main.startColor=tint;
            hitEffect.Play(true);
        }
        static void SetString(WoolBowVisual v,float pull)
        {
            if(!v.bowstring)return;
            v.bowstring.SetPosition(0,new Vector3(-.66f,.15f,.22f));
            v.bowstring.SetPosition(1,new Vector3(0,.15f,-.26f-pull));
            v.bowstring.SetPosition(2,new Vector3(.66f,.15f,.22f));
        }
        void SampleBow(WoolBowVisual v,float time)
        {
            if(v!=null&&v.animationRoot&&authoredLayout.bowAttack)
                authoredLayout.bowAttack.SampleAnimation(v.animationRoot,Mathf.Clamp(time,0,authoredLayout.bowAttack.length));
        }
        static void Emit(ParticleSystem effect,Vector3 at,Color tint,int count,float size)
        {
            if(!effect)return;
            effect.transform.position=at;effect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            effect.Play(true);
            var p=new ParticleSystem.EmitParams{startColor=tint,startSize=size};
            effect.Emit(p,count);
            p.startColor=new Color(1,1,.88f,1);p.startSize=size*2.4f;p.startLifetime=.075f;p.velocity=Vector3.zero;
            effect.Emit(p,1);
        }
        void ReturnArrow()
        {
            if(!arrow)return;
            visual.trail.emitting=false;visual.trail.Clear();
            arrow.gameObject.SetActive(false);
            arrow.SetParent(returnParent,false);WoolSceneRig.SetViewLayer(arrow,"Vehicle View");
            arrow.localPosition=returnPosition;arrow.localRotation=returnRotation;arrow.localScale=returnScale;
        }
        void Finish()
        {
            if(visual!=null&&visual.frame){visual.frame.localPosition=Vector3.zero;visual.frame.localScale=Vector3.one;SetString(visual,0);SampleBow(visual,0);}
            arrow=null;target=null;targetRenderer=null;targetMesh=null;returnParent=null;weapon=null;visual=null;shooter=-1;
            timer=flightTime=0;stage=ShotStage.Idle;
        }
        // Cancellation never reports a hit and restores the loaded pose without a return flight.
        public void Cancel()
        {
            if(impactSegment)impactSegment.gameObject.SetActive(false);
            if(recovering!=null)
            {
                if(recovering.frame)recovering.frame.localPosition=Vector3.zero;
                SetString(recovering,0);
                SampleBow(recovering,0);
                if(recoveringSlot>=0&&recoveringSlot<activeBows.Length&&activeBows[recoveringSlot]&&recoveringArrow)recoveringArrow.gameObject.SetActive(true);
                recovering=null;recoveringArrow=null;
            }
            if(arrow)
            {
                ReturnArrow();
                if(shooter>=0&&activeBows[shooter])arrow.gameObject.SetActive(true);
            }
            Finish();
        }
        void OnDisable()=>Cancel();
    }
}


