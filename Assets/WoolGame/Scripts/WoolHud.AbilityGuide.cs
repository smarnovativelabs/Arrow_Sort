using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace WoolGame {
public sealed class WoolAbilityFocus:MonoBehaviour {
 public RectTransform hand,halo;public Vector2 handOrigin;float clock;
 void OnEnable(){clock=0;}
 void Update(){clock+=Time.unscaledDeltaTime;if(hand){hand.anchoredPosition=handOrigin+new Vector2(12*Mathf.Sin(clock*3),18*Mathf.Sin(clock*3));hand.localScale=Vector3.one*(1+.08f*Mathf.Sin(clock*5));}if(halo){halo.localRotation=Quaternion.Euler(0,0,clock*24);halo.localScale=Vector3.one*(1+.12f*Mathf.Sin(clock*5));}}
}
public sealed partial class WoolHud {
 GameObject abilityGuide,abilityEffect;Button guideTap;TMP_Text guideCopy;RectTransform guideHand,guideHalo;int abilityStage;
 public bool AbilityGuideVisible=>abilityGuide&&abilityGuide.activeSelf;
 public void SetAbilityLevels(int stage){abilityStage=stage;if(!footer)return;for(int k=0;k<4;k++){var b=footer.Find(WoolBoosters.Names[k]);if(!b)continue;bool available=WoolBoosters.Available(k,stage);b.GetComponent<Button>().interactable=available;var shade=b.GetComponent<CanvasGroup>();if(!shade)shade=b.gameObject.AddComponent<CanvasGroup>();shade.alpha=available?1:.8f;if(abilityStrip&&k<abilityStrip.counts.Length&&abilityStrip.counts[k])abilityStrip.counts[k].text=available?(WoolProgress.AbilityCount(k)>0?WoolProgress.AbilityCount(k).ToString():"+"):"Lv "+WoolBoosters.UnlockLevels[k];}}
 void BuildAbilityGuide(){if(abilityGuide)return;var root=Rect("Guided ability spotlight",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);abilityGuide=root.gameObject;var block=root.gameObject.AddComponent<Image>();block.color=new Color(0,0,0,.94f);block.raycastTarget=true;var focus=root.gameObject.AddComponent<WoolAbilityFocus>();
 var glow=Rect("Focused ability glow",root,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(250,250),Vector2.zero);glow.gameObject.AddComponent<CanvasRenderer>();glow.gameObject.AddComponent<WoolAbilityGlow>().raycastTarget=false;guideHalo=focus.halo=glow;
 guideTap=ButtonAt("Use highlighted free ability",root,"USE FREE",new Vector2(180,135),Vector2.zero,null);guideTap.GetComponent<Image>().color=WoolArt.Hex("FFD06B");guideTap.GetComponentInChildren<TMP_Text>().fontSize=27;
 guideCopy=Label("Why this power is needed",root,"",32,new Vector2(850,210),Vector2.zero,Color.white);guideCopy.enableAutoSizing=true;guideCopy.fontSizeMin=22;guideCopy.fontSizeMax=36;
 guideHand=Rect("Animated tutorial hand",root,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(165,185),Vector2.zero);guideHand.gameObject.AddComponent<CanvasRenderer>();guideHand.gameObject.AddComponent<WoolWarehouseHand>().raycastTarget=false;focus.hand=guideHand;abilityGuide.SetActive(false);
 }
 Vector2 GuidePoint(Vector2 screen){RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,screen,null,out var p);return p;}
 public void ShowAbilityGuide(int kind,string message,Action use){BuildAbilityGuide();var source=footer.Find(WoolBoosters.Names[kind]) as RectTransform;var screen=RectTransformUtility.WorldToScreenPoint(null,source.position);PlaceGuide(screen,message,use);guideTap.GetComponent<Image>().sprite=unlockPresentation.abilityIcons[kind];guideTap.GetComponent<Image>().color=Color.white;guideTap.GetComponent<Image>().preserveAspect=true;var label=guideTap.GetComponentInChildren<TMP_Text>();label.text="FREE";label.rectTransform.anchoredPosition=new Vector2(0,-65);label.fontSize=25;}
 public void ShowWorldAbilityGuide(Vector2 screen,string message,Action use){BuildAbilityGuide();PlaceGuide(screen,message,use);guideTap.GetComponent<Image>().sprite=rounded;guideTap.GetComponent<Image>().color=WoolArt.Hex("FFD06B");var label=guideTap.GetComponentInChildren<TMP_Text>();label.text="REMOVE";label.rectTransform.anchoredPosition=Vector2.zero;}
 void PlaceGuide(Vector2 screen,string message,Action use){abilityGuide.SetActive(true);abilityGuide.transform.SetAsLastSibling();var point=GuidePoint(screen);guideHalo.anchoredPosition=point;((RectTransform)guideTap.transform).anchoredPosition=point;guideHand.anchoredPosition=new Vector2(point.x+50,Mathf.Max(point.y-35,-((RectTransform)transform).rect.height*.5f+120));abilityGuide.GetComponent<WoolAbilityFocus>().handOrigin=guideHand.anchoredPosition;var height=((RectTransform)transform).rect.height;guideCopy.rectTransform.anchoredPosition=new Vector2(0,Mathf.Clamp(point.y+230,-height*.5f+300,height*.5f-200));guideCopy.rectTransform.sizeDelta=new Vector2(Mathf.Min(850,((RectTransform)transform).rect.width-70),210);guideCopy.text=message;guideTap.onClick.RemoveAllListeners();bool used=false;guideTap.onClick.AddListener(()=>{if(used)return;used=true;use?.Invoke();});}
 public void HideAbilityGuide(){if(abilityGuide)abilityGuide.SetActive(false);}
 public void ShowAbilityEffect(int kind,Vector2 screen){HideAbilityEffect();var root=Rect("Ability activation feedback",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);abilityEffect=root.gameObject;root.SetAsLastSibling();var point=GuidePoint(screen);var ring=Rect("Expanding power halo",root,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(350,350),point);ring.gameObject.AddComponent<CanvasRenderer>();ring.gameObject.AddComponent<WoolAbilityGlow>().raycastTarget=false;root.gameObject.AddComponent<WoolAbilityFocus>().halo=ring;string[] labels={"SLOT UNLOCKED!","VEHICLE REMOVED!","COLORS REARRANGED!","SMOKE PIERCING POWER!"};var title=Label("Ability effect caption",root,labels[kind],42,new Vector2(850,100),new Vector2(0,100),WoolArt.Hex("FFE07A"));title.enableAutoSizing=true;title.fontSizeMin=22;title.fontSizeMax=42;title.rectTransform.sizeDelta=new Vector2(Mathf.Min(850,((RectTransform)transform).rect.width-60),110);}
 public void HideAbilityEffect(){if(abilityEffect){Destroy(abilityEffect);abilityEffect=null;}}
 }
}



