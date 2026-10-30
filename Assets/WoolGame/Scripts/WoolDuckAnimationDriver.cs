using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace WoolGame {
// Evaluate the supplied non-legacy FBX clips through the runtime animation system.
// Editor pose sampling alone does not provide an Animator for player builds.
[DisallowMultipleComponent]
public sealed class WoolDuckAnimationDriver:MonoBehaviour {
 PlayableGraph graph;AnimationMixerPlayable mixer;Animator animator;
 readonly Dictionary<AnimationClip,int> inputs=new Dictionary<AnimationClip,int>();
 readonly List<AnimationClipPlayable> clips=new List<AnimationClipPlayable>();
 public int SampleCount{get;private set;}public AnimationClip CurrentClip{get;private set;}
 void PrepareAnimator(){if(!animator){animator=GetComponent<Animator>();if(!animator)animator=gameObject.AddComponent<Animator>();animator.enabled=true;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;foreach(var skin in GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=true;}}
 void Prepare(){if(graph.IsValid())return;PrepareAnimator();graph=PlayableGraph.Create(name+" runtime duck poses");graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);mixer=AnimationMixerPlayable.Create(graph,0);var output=AnimationPlayableOutput.Create(graph,"Duck skeleton",animator);output.SetSourcePlayable(mixer);graph.Play();}
 public void Sample(AnimationClip clip,float time){if(!clip||!gameObject.activeInHierarchy)return;PrepareAnimator();float normalized=Mathf.Repeat(time,Mathf.Max(.001f,clip.length))/Mathf.Max(.001f,clip.length);if(animator.runtimeAnimatorController){string state=clip.name.Contains("Walk")?"Walk":clip.name.Contains("Eating")?"Feed":"Jump";animator.speed=0;animator.Play(Animator.StringToHash(state),0,normalized);animator.Update(0);CurrentClip=clip;SampleCount++;return;}Prepare();if(!inputs.TryGetValue(clip,out int input)){input=clips.Count;mixer.SetInputCount(input+1);var playable=AnimationClipPlayable.Create(graph,clip);playable.SetApplyFootIK(false);playable.SetApplyPlayableIK(false);playable.SetSpeed(0);graph.Connect(playable,0,mixer,input);clips.Add(playable);inputs.Add(clip,input);}for(int i=0;i<clips.Count;i++)mixer.SetInputWeight(i,i==input?1:0);clips[input].SetTime(normalized*clip.length);graph.Evaluate(0);CurrentClip=clip;SampleCount++;}
 void OnEnable(){if(graph.IsValid())graph.Play();}
 void OnDisable(){if(graph.IsValid())graph.Stop();}
 void OnDestroy(){if(graph.IsValid())graph.Destroy();}
}
}
