using UnityEngine;
namespace WoolGame
{
    // One reusable effect instance; gameplay owns its clock and pause state.
    public sealed class WoolDragonFlame : MonoBehaviour
    {
        [SerializeField] GameObject effectPrefab;
        [SerializeField] float effectScale=.55f;
        [SerializeField] Material portraitFireMaterial;
        Transform effect;
        ParticleSystem[] particles;
        readonly System.Collections.Generic.List<Material> fireMaterials=new System.Collections.Generic.List<Material>();
        float cycle,remaining;
        public bool Bursting=>remaining>0;
        public void Configure(GameObject prefab){effectPrefab=prefab;}
        void Prepare()
        {
            if(effect||!effectPrefab)return;
            var instance=Instantiate(effectPrefab,transform);
            effect=instance.transform;effect.name=effectPrefab.name;
            effect.localScale=Vector3.one*effectScale;
            WoolSceneRig.SetViewLayer(effect,"Rescue View");
            particles=effect.GetComponentsInChildren<ParticleSystem>(true);
            var shader=portraitFireMaterial?portraitFireMaterial.shader:Shader.Find("Universal Render Pipeline/Particles/Unlit");
            foreach(var ps in particles)
            {
                var main=ps.main;main.playOnAwake=false;main.loop=true;main.stopAction=ParticleSystemStopAction.None;
                main.maxParticles=Mathf.Min(main.maxParticles,160);
                // A compact, dense flame stays readable against the bright path.
                if(ps.name==effectPrefab.name){main.startSpeedMultiplier*=.72f;main.startSizeMultiplier*=1.25f;}
                if(ps.name=="Glow")main.startColor=new Color(1,.75f,.18f,.85f);
                var renderer=ps.GetComponent<ParticleSystemRenderer>();
                if(shader&&renderer&&renderer.sharedMaterial)
                {
                    var original=renderer.sharedMaterial;
                    var material=portraitFireMaterial?new Material(portraitFireMaterial):new Material(shader);
                    material.name=original.name+" Portrait fire";material.renderQueue=3100;
                    var texture=original.HasProperty("_MainTex")?original.GetTexture("_MainTex"):original.mainTexture;
                    material.SetTexture("_BaseMap",texture);material.SetColor("_BaseColor",Color.white);
                    material.SetFloat("_Surface",1);material.SetFloat("_ZWrite",0);material.SetFloat("_Cull",0);
                    material.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetFloat("_DstBlend",(float)(ps.name=="Glow"?UnityEngine.Rendering.BlendMode.One:UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));
                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.SetOverrideTag("RenderType","Transparent");
                    renderer.sharedMaterial=material;fireMaterials.Add(material);
                }
                ps.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            instance.SetActive(true);
        }
        public void ResetBreath(){cycle=0;remaining=0;if(particles!=null)foreach(var ps in particles)ps.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);}
        public void Burst(){Prepare();remaining=2;if(particles!=null)foreach(var ps in particles){ps.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);ps.Play(false);ps.Pause(false);}}
        public void Tick(float dt,Vector3 mouth,Vector3 direction,bool periodic)
        {
            if(periodic){cycle+=dt;if(cycle>=5){cycle%=5;Burst();}}
            if(!effect||remaining<=0)return;
            effect.position=mouth;effect.rotation=Quaternion.LookRotation(direction.normalized,Vector3.up);
            foreach(var ps in particles)ps.Simulate(Mathf.Min(dt,remaining),false,false,true);
            remaining-=dt;
            if(remaining<=0)foreach(var ps in particles)ps.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        void OnDestroy(){foreach(var material in fireMaterials)if(material)Destroy(material);}
    }
}
