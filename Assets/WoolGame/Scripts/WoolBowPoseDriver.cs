using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace WoolGame {
 public sealed class WoolBowPoseDriver : MonoBehaviour {
  PlayableGraph graph; AnimationClipPlayable pose; AnimationClip source;
  public void Sample(AnimationClip clip,float time) {
   if(!clip||!gameObject.activeInHierarchy)return;
   if(!graph.IsValid()||source!=clip){
    if(graph.IsValid())graph.Destroy();
    var animator=GetComponent<Animator>();if(!animator)animator=gameObject.AddComponent<Animator>();
    animator.enabled=true;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
    foreach(var skin in GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=true;
    graph=PlayableGraph.Create("Crossbow pose");graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    pose=AnimationClipPlayable.Create(graph,clip);pose.SetSpeed(0);pose.SetApplyFootIK(false);pose.SetApplyPlayableIK(false);
    AnimationPlayableOutput.Create(graph,"Bow",animator).SetSourcePlayable(pose);source=clip;graph.Play();
   }
   pose.SetTime(time);graph.Evaluate(0);
  }
  void OnDestroy(){if(graph.IsValid())graph.Destroy();}
 }
}
