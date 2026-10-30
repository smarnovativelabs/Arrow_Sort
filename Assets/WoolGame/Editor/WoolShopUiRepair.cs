#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace WoolGame.Editor {
public static class WoolShopUiRepair {
 static T Remap<T>(T old,Transform source,Transform clone) where T:Component {
  if(!old)return null;return clone.Find(AnimationUtility.CalculateTransformPath(old.transform,source)).GetComponent<T>();
 }
 static void Place(Transform t,Vector2 pos,Vector2 size){var r=(RectTransform)t;r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;r.localScale=Vector3.one;}
 [MenuItem("Wool/Repair shop and victory UI")]
 public static string Apply(){
  if(EditorApplication.isPlaying)throw new InvalidOperationException("Run in Edit Mode.");
  var shop=UnityEngine.Object.FindFirstObjectByType<WoolShopView>(FindObjectsInactive.Include);
  var win=UnityEngine.Object.FindFirstObjectByType<WoolWinPresentation>(FindObjectsInactive.Include);
  if(!shop||shop.offers.Length<10||!win)throw new InvalidOperationException("Missing authored shop or victory UI.");
  Undo.RegisterFullObjectHierarchyUndo(shop.gameObject,"Repair six bundle shop");
  Undo.RegisterFullObjectHierarchyUndo(win.gameObject,"Fit victory button text");
  var offers=new List<WoolShopOffer>(shop.offers);
  string[] first={"remove_ads","hatchling_pack","pond_patrol_pack","golden_flock_pack","coins_500","coins_1000","coins_2500","coins_5000","coins_10000","coins_25000"};
  for(int i=0;i<10;i++)offers[i].productId=first[i];
  var template=offers[3];var original=template.purchase.transform.parent;
  while(offers.Count<13){
   int tier=offers.Count-7;var copy=UnityEngine.Object.Instantiate(original.gameObject,original.parent).transform;
   Undo.RegisterCreatedObjectUndo(copy.gameObject,"Add duck bundle tier");
   var offer=new WoolShopOffer{productId=WoolIapCatalog.BundleIds[tier],
    title=Remap(template.title,original,copy),coinAmount=Remap(template.coinAmount,original,copy),
    priceText=Remap(template.priceText,original,copy),purchase=Remap(template.purchase,original,copy),counts=new TMP_Text[4]};
   for(int k=0;k<4;k++)offer.counts[k]=Remap(template.counts[k],original,copy);
   offers.Add(offer);
  }
  shop.offers=offers.ToArray();
  var headerWallet=shop.masthead.Find("Duck coin wallet");
  if(headerWallet){headerWallet.GetComponent<Image>().color=Color.clear;var wr=(RectTransform)headerWallet;wr.anchoredPosition=new Vector2(262,-153);wr.sizeDelta=new Vector2(218,82);var walletView=headerWallet.GetComponent<WoolCoinWallet>();if(walletView){if(walletView.coin)walletView.coin.gameObject.SetActive(false);walletView.Refresh();}}

  var obsoleteHeading=shop.design.Find("Coin packs heading");if(obsoleteHeading)obsoleteHeading.gameObject.SetActive(false);
  int[] indices={1,2,3,10,11,12};
  for(int tier=0;tier<6;tier++){
   var offer=shop.offers[indices[tier]];offer.productId=WoolIapCatalog.BundleIds[tier];offer.Refresh();
   var row=(RectTransform)offer.purchase.transform.parent;row.name=offer.bundleName;
   row.sizeDelta=new Vector2(900,400);row.anchoredPosition=new Vector2(0,-610-tier*440);
   // Retain authored artwork; hide obsolete reward cells behind the current boutique panels.
   foreach(Transform child in row)if(child.name=="Bundle title backing"||child.name=="Premium coin reward"||child.name.StartsWith("Ability reward "))child.gameObject.SetActive(false);
   var coins=row.Find("Boutique coin panel");var boosters=row.Find("Boutique booster panel");var gift=row.Find("Boutique duck gift");
   Place(coins,new Vector2(-305,42),new Vector2(230,230));
   Place(boosters,new Vector2(-6,42),new Vector2(348,230));
   if(gift)Place(gift,new Vector2(304,42),new Vector2(210,218));
   var heading=boosters.Find("Boutique booster heading");if(heading)heading.gameObject.SetActive(false);
   var hudData=new SerializedObject(shop.owner);var gameplayIcons=hudData.FindProperty("icons");
   for(int k=0;k<4;k++){
    var icon=boosters.Find("Boutique ability "+k);icon.gameObject.SetActive(true);
    Place(icon,new Vector2(k%2==0?-78:78,k<2?62:-28),new Vector2(108,78));
    var image=icon.GetComponent<Image>();var sprite=gameplayIcons.GetArrayElementAtIndex(k).objectReferenceValue as Sprite;
    if(!sprite)throw new InvalidOperationException("Missing gameplay ability sprite "+k);
    image.sprite=sprite;image.color=Color.white;image.type=Image.Type.Simple;image.preserveAspect=true;
    var count=offer.counts[k];count.transform.SetParent(boosters,false);count.gameObject.SetActive(k==0);
    Place(count.transform,new Vector2(0,-90),new Vector2(300,40));
    count.enableAutoSizing=true;count.fontSizeMin=28;count.fontSizeMax=38;count.alignment=TextAlignmentOptions.Center;count.color=new Color(.28f,.12f,.035f,1);
   }
   offer.coinAmount.transform.SetParent(coins,false);offer.coinAmount.gameObject.SetActive(true);
   Place(offer.coinAmount.transform,new Vector2(0,-81),new Vector2(210,48));
   offer.title.gameObject.SetActive(true);offer.title.transform.SetAsLastSibling();
   Place(offer.title.transform,new Vector2(-152,-136),new Vector2(520,64));
   offer.title.color=Color.white;offer.title.fontSizeMax=46;offer.title.fontSizeMin=28;offer.title.enableWordWrapping=false;
   offer.purchase.transform.SetAsLastSibling();Place(offer.purchase.transform,new Vector2(288,-136),new Vector2(282,88));
   Place(offer.priceText.transform,Vector2.zero,new Vector2(250,60));offer.priceText.margin=new Vector4(6,3,6,3);
   offer.priceText.alignment=TextAlignmentOptions.Center;offer.Refresh();
  }
  for(int i=4;i<10;i++){var row=(RectTransform)shop.offers[i].purchase.transform.parent;
   row.anchoredPosition=new Vector2(((i-4)%3-1)*306,-3370-((i-4)/3)*320);shop.offers[i].Refresh();}
  ((RectTransform)shop.dailyClaim.transform.parent).anchoredPosition=new Vector2(0,-4100);
  shop.designHeight=4330;shop.design.sizeDelta=new Vector2(900,4330);shop.Fit(true);
  var special=UnityEngine.Object.FindFirstObjectByType<WoolSpecialOffer>(FindObjectsInactive.Include);
  if(special){Undo.RecordObject(special,"Match introductory offer price");special.DisplayPrice=WoolIapCatalog.Find(WoolIapCatalog.Intro).displayPrice;EditorUtility.SetDirty(special);}
  var layout=typeof(WoolWinPresentation).GetMethod("LayoutRewardButton",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
  layout.Invoke(win,new object[]{win.collect,win.collectLabel,win.collectAmount,true,false});
  layout.Invoke(win,new object[]{win.bonus,win.bonusLabel,win.bonusAmount,true,true});
  foreach(var c in shop.GetComponentsInChildren<Component>(true))if(c)EditorUtility.SetDirty(c);
  foreach(var c in win.GetComponentsInChildren<Component>(true))if(c)EditorUtility.SetDirty(c);
  EditorUtility.SetDirty(shop);EditorUtility.SetDirty(win);EditorSceneManager.MarkSceneDirty(shop.gameObject.scene);
  AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(shop.gameObject.scene);
  return "Saved six duck bundle tiers, visible ability icons, matching introductory prices and fitted victory labels.";
 }
}
}
#endif
