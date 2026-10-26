using UnityEngine;
namespace WoolGame
{
    public enum WoolDragonAnimationState { Dead=0,MouthOpen=1,MouthClosed=2,MouthOpenClose=3,EyesAndMouthDemo=4 }

    public sealed partial class WoolSceneRig
    {
        [Header("Replaceable art")]
        public WoolModelSet models;
        Animator dragonAnimator;
        static readonly int AnimationState=Animator.StringToHash("Dragon_Animation_State");
        int lastAnimationState=-1;
        public bool DragonMouthOpen=>lastAnimationState==(int)WoolDragonAnimationState.MouthOpen;
        public void SetDragonAnimation(int state)
        {
            if(!dragonAnimator||lastAnimationState==state)return;
            lastAnimationState=state;dragonAnimator.SetInteger(AnimationState,state);
        }
        public void PrepareSnakeModels()
        {
            dragonAnimator=dragon.GetComponentInChildren<Animator>(true);
            if(dragonAnimator){dragonAnimator.applyRootMotion=false;dragonAnimator.cullingMode=AnimatorCullingMode.AlwaysAnimate;}
        }
        public void AnimateDragon(bool mouthOpen,bool paused)
        {
            AnimateDragon(mouthOpen?WoolDragonAnimationState.MouthOpenClose:WoolDragonAnimationState.EyesAndMouthDemo,paused);
        }
        public void AnimateDragon(WoolDragonAnimationState state,bool paused)
        {
            if(!dragonAnimator)return;
            dragonAnimator.speed=paused?0:1;
            SetDragonAnimation((int)state);
        }
        public void SetSectionModel(int poolIndex,int color)=>ColorSection(yarn[poolIndex],color);
        public void ColorSection(Transform section,int color)
        {
            var renderer=section.GetComponent<Renderer>();
            if(models&&models.tailMeshes!=null&&color>=0&&color<models.tailMeshes.Length&&models.tailMeshes[color])
            {
                section.GetComponent<MeshFilter>().sharedMesh=models.tailMeshes[color];
                renderer.sharedMaterial=models.tailMaterial;renderer.enabled=true;renderer.SetPropertyBlock(null);
            }
            else renderer.sharedMaterial=woolMaterials[color];
        }
    }
}
