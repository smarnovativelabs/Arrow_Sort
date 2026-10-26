#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace WoolGame {
public sealed partial class WoolHud {
 public string AuthorLossFlow(){
  if(Application.isPlaying)throw new System.InvalidOperationException("Edit Mode required");
  if(losePresentation.offerPage)return "Loss flow already authored";
  var p=losePresentation;UnityEditor.Undo.RegisterFullObjectHierarchyUndo(p.gameObject,"Author original loss flow");
  p.background.gameObject.SetActive(false);
  foreach(Transform c in p.card)c.gameObject.SetActive(false);
  var offer=Rect("Second chance offer",p.card,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);p.offerPage=offer.gameObject;
  var retryPage=Rect("Try again page",p.card,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);p.retryPage=retryPage.gameObject;
  var ink=WoolArt.Hex("173C48");var cream=WoolArt.Hex("FFF2CF");var mint=WoolArt.Hex("63E2C0");
  var plate=Panel("Rescue station",offer,new Vector2(.5f,.5f),new Vector2(720,890),new Vector2(0,20),ink);
  Label("Offer eyebrow",plate,"RESCUE STATION",27,new Vector2(600,60),new Vector2(0,355),mint);
  Label("Offer title",plate,"Make room for hope",49,new Vector2(650,95),new Vector2(0,265),cream);
  Label("Slot symbol",plate,"[  ]   [  ]   [  ]",65,new Vector2(600,130),new Vector2(0,115),mint);
  Label("Offer description",plate,"Clear every occupied slot\nand send the dragon back into the smoke.",30,new Vector2(610,130),new Vector2(0,-20),Color.white);
  p.coinButton=ButtonAt("Clear slots with coins",plate,"1,200",new Vector2(295,115),new Vector2(-165,-200),null);
  p.continueButton=ButtonAt("Clear slots with video",plate,"Watch video",new Vector2(295,115),new Vector2(165,-200),null);
  p.cancelOffer=ButtonAt("Decline rescue",plate,"No thanks",new Vector2(300,75),new Vector2(0,-350),null);
  var wallet=Panel("Duck coin wallet",offer,new Vector2(.5f,1),new Vector2(280,85),new Vector2(200,-60),ink);
  p.balance=Label("Available coins",wallet,"0",35,new Vector2(180,70),new Vector2(35,0),cream);
  var coin=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/Unlock/DuckFootprintCoin.png");
  if(!coin){foreach(var a in UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/WoolGame/Designer/UI/Unlock/DuckFootprintCoin.png"))if(a is Sprite s){coin=s;break;}}
  var ci=Panel("Duck footprint coin",wallet,new Vector2(.5f,.5f),new Vector2(70,70),new Vector2(-95,0),Color.white);ci.GetComponent<Image>().sprite=coin;ci.GetComponent<Image>().preserveAspect=true;
  var cb=Panel("Duck coin payment",p.coinButton.transform,new Vector2(.5f,.5f),new Vector2(65,65),new Vector2(-94,0),Color.white);cb.GetComponent<Image>().sprite=coin;cb.GetComponent<Image>().preserveAspect=true;
  Label("Retry eyebrow",retryPage,"A BUMP IN THE JOURNEY",27,new Vector2(700,70),new Vector2(0,510),mint);
  Label("Retry title",retryPage,"Brave ducks try again",53,new Vector2(720,150),new Vector2(0,400),cream);
  p.hero.SetParent(retryPage,false);p.hero.gameObject.SetActive(true);p.hero.sizeDelta=new Vector2(385,500);p.hero.anchoredPosition=new Vector2(-175,15);
  var homeMotion=UnityEngine.Object.FindFirstObjectByType<WoolHomePresentation>(FindObjectsInactive.Include);
  if(homeMotion&&homeMotion.dragon){var dr=Panel("Smiling dragon",retryPage,new Vector2(.5f,.5f),new Vector2(400,510),new Vector2(175,60),Color.white);dr.GetComponent<Image>().sprite=homeMotion.dragon.GetComponent<Image>().sprite;dr.GetComponent<Image>().preserveAspect=true;p.smilingDragon=dr;dr.SetAsFirstSibling();}
  Label("Encouragement",retryPage,"The dragon won this round.\nYour duck family is counting on you!",31,new Vector2(680,130),new Vector2(0,-285),Color.white);
  p.retry=ButtonAt("Retry level",retryPage,"TRY AGAIN",new Vector2(610,125),new Vector2(0,-520),null);
  p.home=ButtonAt("Return home",retryPage,"HOME",new Vector2(160,75),new Vector2(-275,610),null);
  foreach(var b in new[]{p.coinButton,p.continueButton,p.cancelOffer,p.retry,p.home}){b.GetComponent<Image>().color=b==p.cancelOffer?ink:mint;var text=b.GetComponentInChildren<TMP_Text>();text.color=b==p.cancelOffer?Color.white:ink;text.fontSize=b==p.cancelOffer||b==p.home?29:36;}
  p.card.pivot=new Vector2(.5f,.5f);p.card.anchorMin=p.card.anchorMax=new Vector2(.5f,.5f);
  p.gameObject.SetActive(false);UnityEditor.EditorUtility.SetDirty(p);UnityEditor.EditorUtility.SetDirty(this);UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(gameObject.scene);
  return "Loss flow authored and scene saved";
 }
}}
#endif
