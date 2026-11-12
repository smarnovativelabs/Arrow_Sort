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
 static readonly string[] ToneNames={"Green","Blue","Purple","Amber","Quiet","Card","Red","GoldCard","Close"};
 bool[] tintLabels;string objectName;
 Image image;WoolAbilityTileGraphic tile;Button button;TMP_Text[] labels;Coffee.UIEffects.UIShiny shine;
 static readonly Dictionary<string,Sprite> faces=new Dictionary<string,Sprite>();
 static Sprite Face(string name){
  if(faces.TryGetValue(name,out var face)&&face)return face;
  if(name=="Close"||name=="GoldCard"){face=Resources.Load<Sprite>("WoolButtons/"+name);faces[name]=face;return face;}
  Color top,bottom;
  switch(name){
   case "Blue":top=WoolArt.Hex("5BC8F4");bottom=WoolArt.Hex("299AD1");break;
   case "Purple":top=WoolArt.Hex("BC83EE");bottom=WoolArt.Hex("9958CD");break;
   case "Amber":case "GoldCard":top=WoolArt.Hex("FFC54D");bottom=WoolArt.Hex("EEA32A");break;
   case "Red":top=WoolArt.Hex("F87870");bottom=WoolArt.Hex("D6504B");break;
   case "Quiet":case "Card":top=WoolArt.Hex("D7EEE2");bottom=WoolArt.Hex("B6D9C7");break;
   case "Disabled":top=WoolArt.Hex("ADC1B6");bottom=WoolArt.Hex("92AA9C");break;
   default:top=WoolArt.Hex("77D62C");bottom=WoolArt.Hex("49B925");break;
  }
  const int width=256,height=128;var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);texture.name="Soft rounded "+name;texture.wrapMode=TextureWrapMode.Clamp;
  var pixels=new Color[width*height];
  for(int y=0;y<height;y++)for(int x=0;x<width;x++){
   float px=x+.5f-width*.5f,py=y+.5f-height*.5f;
   var q=new Vector2(Mathf.Abs(px)-(width*.5f-30),Mathf.Abs(py)-(height*.5f-30));
   float distance=new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-30;
   float alpha=1-Mathf.Clamp01(distance+.5f);
   float vertical=y/(height-1f);
   Color color=Color.Lerp(bottom,top,Mathf.SmoothStep(0,1,vertical));
   // A continuous bevel catches light above and forms a shallow base below.
   float edgeDepth=-distance;
   float bevel=1-Mathf.SmoothStep(2,12,edgeDepth);
   float upper=Mathf.SmoothStep(.42f,.8f,vertical);
   Color bevelColor=Color.Lerp(bottom*.78f,Color.Lerp(top,Color.white,.48f),upper);
   color=Color.Lerp(color,bevelColor,bevel*.85f);
   float innerHighlight=Mathf.Exp(-Mathf.Pow((edgeDepth-13)/3f,2))*upper;
   color=Color.Lerp(color,Color.Lerp(top,Color.white,.3f),innerHighlight*.65f);
   float outerRim=1-Mathf.SmoothStep(0,2,edgeDepth);
   color=Color.Lerp(color,Color.Lerp(bottom,top,upper)*.86f,outerRim*.55f);
      // A distinct lower extrusion remains visible at normal game scale.
   if(y<18)color=Color.Lerp(bottom*.58f,bottom*.72f,Mathf.Clamp01(y/18f));
   else {
    float faceHeight=Mathf.Clamp01((y-18f)/100f);
    color=Color.Lerp(bottom,top,faceHeight);
    float side=Mathf.Clamp01((10-edgeDepth)/10f);
    color=Color.Lerp(color,Color.Lerp(bottom*.82f,top,vertical),side*.45f);
    if(y<23)color=Color.Lerp(bottom*.82f,color,(y-18f)/5f);
    if(y>116)color=Color.Lerp(color,Color.Lerp(top,Color.white,.48f),Mathf.Clamp01((y-116f)/6f)*.8f);
   }
      // An inset pad sits above the outer base, with a rim on every side.
   var padQ=new Vector2(Mathf.Abs(px)-(width*.5f-8-22),Mathf.Abs(py-2)-(height*.5f-9-22));
   float padDistance=new Vector2(Mathf.Max(padQ.x,0),Mathf.Max(padQ.y,0)).magnitude+Mathf.Min(Mathf.Max(padQ.x,padQ.y),0)-22;
   Color baseColor=Color.Lerp(bottom*.68f,Color.Lerp(top,Color.white,.32f),vertical);
   float padBlend=1-Mathf.Clamp01(padDistance+.5f);
   float padEdge=1-Mathf.SmoothStep(0,6,-padDistance);
   Color padColor=Color.Lerp(bottom,top,Mathf.Clamp01((y-20f)/90f));
   Color padRim=py>5?Color.Lerp(top,Color.white,.52f):bottom*.85f;
   padColor=Color.Lerp(padColor,padRim,padEdge*.85f);
   color=Color.Lerp(baseColor,padColor,padBlend);
   float outerHighlight=(1-Mathf.SmoothStep(1,4,-distance))*Mathf.SmoothStep(.35f,.9f,vertical);
   color=Color.Lerp(color,Color.Lerp(top,Color.white,.55f),outerHighlight*.8f);
   color.a=alpha;pixels[y*width+x]=color;
  }
  texture.SetPixels(pixels);texture.Apply(false,true);
  face=Sprite.Create(texture,new Rect(0,0,width,height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(32,32,32,32));face.name="Soft rounded "+name;faces[name]=face;return face;
 }
 void Awake(){Cache();EnsureEdgeShading();}
 void EnsureEdgeShading(){var effect=GetComponent<WoolButtonEdgeShading>();if(effect)effect.enabled=false;}
 void Cache(){if(!button)button=GetComponent<Button>();if(!image)image=GetComponent<Image>();if(!tile)tile=GetComponent<WoolAbilityTileGraphic>();if(labels==null){labels=GetComponentsInChildren<TMP_Text>(true);tintLabels=new bool[labels.Length];for(int i=0;i<labels.Length;i++){var labelName=labels[i].name;tintLabels[i]=labelName.IndexOf("count",System.StringComparison.OrdinalIgnoreCase)<0&&labelName!="Alert"&&labelName.IndexOf("amount",System.StringComparison.OrdinalIgnoreCase)<0;}}if(objectName==null)objectName=name;if(!shine)shine=GetComponent<Coffee.UIEffects.UIShiny>();}
 void OnEnable(){Cache();EnsureEdgeShading();ConfigureShine();Apply();}
 void LateUpdate(){Apply();}
 static bool Contains(string text,string value)=>text.IndexOf(value,System.StringComparison.OrdinalIgnoreCase)>=0;
 static bool IsCancel(string name,string caption){caption=(caption??string.Empty).Trim();return Contains(name,"cancel")||Contains(name,"dismiss")||Contains(name,"decline")||Contains(name,"close")||string.Equals(caption,"x",System.StringComparison.OrdinalIgnoreCase)||caption=="\u00D7"||string.Equals(caption,"cancel",System.StringComparison.OrdinalIgnoreCase)||string.Equals(caption,"no thanks",System.StringComparison.OrdinalIgnoreCase);}

 public static void HomeAction(Button target){
  if(!target)return;var style=Style(target,true);if(!style)return;
  style.tone=Tone.Green;style.enabled=true;style.Apply();
 }
 public static WoolGlossyButton Style(Button target,bool compact=false){
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
  if(!compact&&!card&&!symbol&&!cancel&&(rect.rect.width<140||rect.rect.width<rect.rect.height*1.35f))return null;
  if((!text||string.IsNullOrWhiteSpace(text.text))&&!card&&!cancel)return null;
  var style=target.GetComponent<WoolGlossyButton>();if(!style)style=target.gameObject.AddComponent<WoolGlossyButton>();
  foreach(var surface in target.GetComponentsInChildren<WoolDailySurface>(true))if(surface.name=="Glossy button face")surface.enabled=false;
  // Old SpriteSwap states otherwise replace the new face while pressed/disabled.
  target.transition=Selectable.Transition.ColorTint;target.spriteState=new SpriteState();
  var colors=target.colors;colors.normalColor=Color.white;colors.highlightedColor=Color.white;colors.selectedColor=Color.white;colors.pressedColor=new Color(.84f,.9f,.87f);colors.disabledColor=Color.white;colors.fadeDuration=.04f;target.colors=colors;if(image)image.overrideSprite=null;
  bool greenPage=target.GetComponentInParent<WoolShopView>(true)||target.GetComponentInParent<WoolSpecialOffer>(true)||target.GetComponentInParent<WoolAdPassOffer>(true);
  style.tone=roundClose?Tone.Close:cancel?Tone.Red:greenPage?Tone.Green:name=="toggle"?Tone.Blue:homeCard?Tone.GoldCard:card?Tone.Card:symbol||name.Contains("home")||name.Contains("back")?Tone.Quiet:name.Contains("video")||name.Contains("double")||name.Contains("spin")||name.Contains("reward")?Tone.Purple:name.Contains("purchase")?Tone.Amber:name.Contains("browse")||name.Contains("shop")||name.Contains("map")?Tone.Blue:Tone.Green;
  if(roundClose){foreach(var caption in target.GetComponentsInChildren<TMP_Text>(true))caption.enabled=false;foreach(var art in target.GetComponentsInChildren<Graphic>(true))if(art.transform!=target.transform&&(art.name.StartsWith("Cancel stroke")||art.name=="Close icon"))art.enabled=false;if(!target.GetComponent<Coffee.UIEffects.UIShiny>())target.gameObject.AddComponent<Coffee.UIEffects.UIShiny>();}
  style.subtleShine=!roundClose&&!card&&!symbol;style.Cache();style.ConfigureShine();style.Apply();return style;
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
  if(objectName=="Toggle"&&labels.Length>0)off=labels[0].text=="OFF";
  bool cancel=IsCancel(objectName,null);foreach(var label in labels)if(label&&IsCancel(string.Empty,label.text)){cancel=true;break;}
  Tone current=cancel?(tone==Tone.Close?Tone.Close:Tone.Red):off?Tone.Quiet:tone;var face=Face(!enabled?"Disabled":ToneNames[(int)current]);if(!face)face=background;if(!face)return;
  if(image){if(image.sprite!=face)image.sprite=face;var imageType=current==Tone.Close||current==Tone.GoldCard?Image.Type.Simple:Image.Type.Sliced;if(image.type!=imageType)image.type=imageType;if(current!=Tone.Close&&current!=Tone.GoldCard)image.pixelsPerUnitMultiplier=Mathf.Max(1,180f/Mathf.Max(1,((RectTransform)transform).rect.height));if(current==Tone.Close)image.pixelsPerUnitMultiplier=1;if(image.preserveAspect)image.preserveAspect=false;if(image.color!=Color.white)image.color=Color.white;}
  if(tile&&tile.tile!=face){tile.tile=face;tile.SetAllDirty();}
  Color ink=current==Tone.GoldCard?new Color(.32f,.12f,.025f):current==Tone.Quiet||current==Tone.Card?new Color(.13f,.23f,.22f):Color.white;if(!enabled)ink=new Color(.95f,.97f,.98f);
  for(int i=0;i<labels.Length;i++){var label=labels[i];if(!label||!tintLabels[i])continue;if(label.color!=ink)label.color=ink;if(label.font&&label.fontSharedMaterial!=label.font.material)label.fontSharedMaterial=label.font.material;}
  if(shine){bool show=current!=Tone.Close&&subtleShine&&enabled;if(shine.enabled!=show)shine.enabled=show;}
 }
}
}
