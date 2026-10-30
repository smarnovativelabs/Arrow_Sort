using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
// Shared restrained button faces, including controls whose state is refreshed at runtime.
[RequireComponent(typeof(Button))]
public sealed class WoolGlossyButton:MonoBehaviour {
 public enum Tone { Green,Blue,Purple,Amber,Quiet,Card,Red,GoldCard,Close }
 public Sprite background;public Tone tone;public bool subtleShine=true;
 Image image;WoolAbilityTileGraphic tile;Button button;TMP_Text[] labels;Coffee.UIEffects.UIShiny shine;
 static readonly Dictionary<string,Sprite> faces=new Dictionary<string,Sprite>();
 static Sprite Face(string name){if(!faces.TryGetValue(name,out var face)){face=Resources.Load<Sprite>("WoolButtons/"+name);if(face)faces[name]=face;}return face;}
 void Awake(){Cache();}
 void Cache(){if(!button)button=GetComponent<Button>();if(!image)image=GetComponent<Image>();if(!tile)tile=GetComponent<WoolAbilityTileGraphic>();if(labels==null)labels=GetComponentsInChildren<TMP_Text>(true);if(!shine)shine=GetComponent<Coffee.UIEffects.UIShiny>();}
 void OnEnable(){Cache();ConfigureShine();Apply();}
 void LateUpdate(){Apply();}
 static bool IsCancel(string name,string caption){caption=(caption??string.Empty).Trim().ToLowerInvariant();return name.Contains("cancel")||name.Contains("dismiss")||name.Contains("decline")||name.Contains("close")||caption=="x"||caption=="×"||caption=="cancel"||caption=="no thanks";}
 public static WoolGlossyButton Style(Button target){
  if(!target)return null;var rect=target.transform as RectTransform;if(!rect)return null;
  // This hit target uses the close artwork already painted into the loss popup.
  var loss=target.GetComponentInParent<WoolLosePresentation>(true);if(loss&&target==loss.cancelOffer)return null;
  var image=target.GetComponent<Image>();var tile=target.GetComponent<WoolAbilityTileGraphic>();
  if(!image&&!tile)return null;
  string name=target.name.ToLowerInvariant();
  string parent=target.transform.parent?target.transform.parent.name:string.Empty;
  if(name=="edit name"||name=="coins"||name=="current level"||name=="gameplay speed"||parent=="Navigation"||parent=="Bottom navigation")return null;
  if(name.StartsWith("stage ")||name.StartsWith("level node")||name.StartsWith("avatar ")||name.StartsWith("gift ")||name.Contains("wallet")||name.Contains("balance")||name=="profile"||name=="settings"||name=="edit avatar"||name=="700 credit chest")return null;
  if(image&&image.sprite&&(image.sprite.name.Contains("CoinPill")||image.sprite.name=="Pencil"))return null;
  TMP_Text text=null;foreach(var caption in target.GetComponentsInChildren<TMP_Text>(true))if(!string.IsNullOrWhiteSpace(caption.text)&&(text==null||!text.gameObject.activeSelf)){text=caption;if(caption.gameObject.activeSelf)break;}
  bool homeCard=target.transform.parent&&target.transform.parent.name=="Action cards";
  bool card=tile||homeCard;
  bool symbol=text&&(text.text=="X"||text.text=="×"||text.text=="<"||text.text==">");
  bool cancel=IsCancel(name,text?text.text:null);
  bool roundClose=cancel&&rect.rect.width<280&&rect.rect.height<280&&Mathf.Abs(rect.rect.width-rect.rect.height)<30;
  if(cancel&&(!text||string.IsNullOrWhiteSpace(text.text))&&rect.rect.width>300&&rect.rect.height>300)return null;
  if(!card&&!symbol&&!cancel&&(rect.rect.width<140||rect.rect.width<rect.rect.height*1.35f))return null;
  if((!text||string.IsNullOrWhiteSpace(text.text))&&!card&&!cancel)return null;
  var style=target.GetComponent<WoolGlossyButton>();if(!style)style=target.gameObject.AddComponent<WoolGlossyButton>();
  foreach(var surface in target.GetComponentsInChildren<WoolDailySurface>(true))if(surface.name=="Glossy button face")surface.enabled=false;
  // Old SpriteSwap states otherwise replace the new face while pressed/disabled.
  target.transition=Selectable.Transition.ColorTint;target.spriteState=new SpriteState();
  var colors=target.colors;colors.normalColor=Color.white;colors.highlightedColor=Color.white;colors.selectedColor=Color.white;colors.pressedColor=new Color(.84f,.9f,.87f);colors.disabledColor=Color.white;colors.fadeDuration=.04f;target.colors=colors;if(image)image.overrideSprite=null;
  bool greenPage=target.GetComponentInParent<WoolShopView>(true)||target.GetComponentInParent<WoolSpecialOffer>(true)||target.GetComponentInParent<WoolAdPassOffer>(true);
  style.tone=roundClose?Tone.Close:cancel?Tone.Red:greenPage?Tone.Green:name=="toggle"?Tone.Blue:homeCard?Tone.GoldCard:card?Tone.Card:symbol||name.Contains("home")||name.Contains("back")?Tone.Quiet:name.Contains("video")||name.Contains("double")||name.Contains("spin")||name.Contains("reward")?Tone.Purple:name.Contains("purchase")?Tone.Amber:name.Contains("browse")||name.Contains("shop")||name.Contains("map")?Tone.Blue:Tone.Green;
  if(roundClose){foreach(var caption in target.GetComponentsInChildren<TMP_Text>(true))caption.enabled=false;foreach(var art in target.GetComponentsInChildren<Graphic>(true))if(art.transform!=target.transform&&(art.name.StartsWith("Cancel stroke")||art.name=="Close icon"))art.enabled=false;if(!target.GetComponent<Coffee.UIEffects.UIShiny>())target.gameObject.AddComponent<Coffee.UIEffects.UIShiny>();}
  style.subtleShine=roundClose||(!card&&!symbol);style.Cache();style.ConfigureShine();style.Apply();return style;
 }
 public void ConfigureShine(){
  Cache();if(!shine)return;
  shine.width=.045f;shine.softness=.85f;shine.brightness=.12f;shine.gloss=.10f;shine.rotation=22;
  shine.effectPlayer.duration=1.2f;shine.effectPlayer.loopDelay=7;shine.effectPlayer.initialPlayDelay=2+(Mathf.Abs(GetInstanceID())%13)*.17f;
  shine.effectPlayer.updateMode=AnimatorUpdateMode.UnscaledTime;
  shine.effectPlayer.play=true;shine.effectPlayer.loop=true;
  shine.enabled=subtleShine&&button.IsInteractable();
 }
 public void Apply(){
  Cache();if(!button)return;bool enabled=button.IsInteractable();bool off=false;
  if(name=="Toggle"&&labels.Length>0)off=labels[0].text=="OFF";
  bool cancel=IsCancel(name.ToLowerInvariant(),null);foreach(var label in labels)if(label&&IsCancel(string.Empty,label.text)){cancel=true;break;}
  Tone current=cancel?(tone==Tone.Close?Tone.Close:Tone.Red):off?Tone.Quiet:tone;var face=Face(!enabled?"Disabled":current.ToString());if(!face)face=background;if(!face)return;
  if(image){if(image.sprite!=face)image.sprite=face;var imageType=current==Tone.GoldCard||current==Tone.Close?Image.Type.Simple:Image.Type.Sliced;if(image.type!=imageType)image.type=imageType;if(image.preserveAspect)image.preserveAspect=false;if(image.color!=Color.white)image.color=Color.white;}
  if(tile&&tile.tile!=face){tile.tile=face;tile.SetAllDirty();}
  Color ink=current==Tone.GoldCard?new Color(.32f,.12f,.025f):current==Tone.Quiet||current==Tone.Card?new Color(.13f,.23f,.22f):Color.white;if(!enabled)ink=new Color(.95f,.97f,.98f);
  foreach(var label in labels){if(!label||label.name.Contains("count")||label.name.Contains("Count")||label.name=="Alert"||label.name.Contains("amount")||label.name.Contains("Amount"))continue;if(label.color!=ink)label.color=ink;if(label.font&&label.fontSharedMaterial!=label.font.material)label.fontSharedMaterial=label.font.material;}
  if(shine){bool show=subtleShine&&enabled;if(shine.enabled!=show)shine.enabled=show;}
 }
}
}
