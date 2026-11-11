using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
namespace WoolGame {
public sealed partial class WoolHud {
 RectTransform homeLevelTwoHand;bool homeLevelTwoHintUsed;Coroutine homeLevelTwoHintMotion;
 void ShowHomeLevelTwoHint(){
  if(WoolProgress.OnboardingStep!=WoolOnboardingStep.Complete||homeLevelTwoHintUsed||WoolProgress.CurrentLevel!=2||!WoolProgress.HasCompleted(1)||!referenceHome)return;
  var target=referenceHome.Find("Level area/Play") as RectTransform;if(!target)return;
  homeLevelTwoHintUsed=true;
  homeLevelTwoHand=Rect("Point to Level 2",target,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(165,185),Vector2.zero);
  var layer=homeLevelTwoHand.gameObject.AddComponent<Canvas>();layer.overrideSorting=true;layer.sortingOrder=300;
  var hand=homeLevelTwoHand.gameObject.AddComponent<WoolWarehouseHand>();hand.color=WoolArt.Hex("30DFFF");hand.raycastTarget=false;
  homeLevelTwoHintMotion=StartCoroutine(AnimateHomeLevelTwoHint());
 }
 IEnumerator AnimateHomeLevelTwoHint(){
  yield return null;yield return null;
  while(homeLevelTwoHand&&menu.activeInHierarchy&&homePage.gameObject.activeInHierarchy&&!ModalVisible){
   if((Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)||(Touchscreen.current!=null&&Touchscreen.current.primaryTouch.press.wasPressedThisFrame))break;
   homeLevelTwoHand.anchoredPosition=new Vector2(35,8+Mathf.Sin(Time.unscaledTime*4)*7);homeLevelTwoHand.localScale=Vector3.one*.8f;yield return null;
  }
  if(homeLevelTwoHand)Destroy(homeLevelTwoHand.gameObject);homeLevelTwoHand=null;homeLevelTwoHintMotion=null;
 }
 void DismissHomeLevelTwoHint(){
  if(homeLevelTwoHintMotion!=null)StopCoroutine(homeLevelTwoHintMotion);homeLevelTwoHintMotion=null;
  if(homeLevelTwoHand)Destroy(homeLevelTwoHand.gameObject);homeLevelTwoHand=null;
 }
}
}
