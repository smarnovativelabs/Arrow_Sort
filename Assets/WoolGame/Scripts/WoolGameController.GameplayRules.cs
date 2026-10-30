using System.Collections;using UnityEngine;
namespace WoolGame {public sealed partial class WoolGameController {
 IEnumerator BeginGameplayRulesThenAbility(){
  if(!automatedInput&&!definition.tutorial&&!WoolProgress.GameplayRulesTutorialSeen){
   while(entering||hud.ModalVisible||hud.AbilityGuideVisible){if(ended||phase==Phase.Menu)yield break;yield return null;}
   paused=true;bool done=false;
   hud.ShowGameplayRule(rig,false,()=>done=true);
   while(!done)yield return null;
   done=false;hud.ShowGameplayRule(rig,true,()=>done=true);
   while(!done)yield return null;
   hud.HideGameplayRule();WoolProgress.MarkGameplayRulesTutorialSeen();paused=false;
  }
  yield return BeginAbilityLesson();
 }
}}

