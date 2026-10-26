#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
public sealed partial class WoolHud {
 public string ApplyReferenceLossArt(){
  if(Application.isPlaying)throw new System.InvalidOperationException("Edit Mode required");
  var p=losePresentation;
  foreach(Transform c in p.offerPage.transform)c.gameObject.SetActive(false);
  foreach(Transform c in p.retryPage.transform)c.gameObject.SetActive(false);
  var popup=LossSprite("Assets/WoolGame/Designer/UI/Defeat/ClearSlotsPopup.png");
  var chrome=LossSprite("Assets/WoolGame/Designer/UI/Defeat/RetryChrome.png");
  var sheet=LossSprite("Assets/WoolGame/Designer/UI/Defeat/LossCharactersSheet.png");
  var panel=Panel("Reference clear slots art",p.offerPage.transform,new Vector2(.5f,.5f),new Vector2(740,1080),new Vector2(0,0),Color.white);panel.GetComponent<Image>().sprite=popup;panel.GetComponent<Image>().preserveAspect=true;panel.GetComponent<Image>().raycastTarget=false;
  var page=Panel("Retry screen chrome",p.retryPage.transform,new Vector2(.5f,.5f),new Vector2(760,1320),Vector2.zero,Color.white);page.GetComponent<Image>().sprite=chrome;page.GetComponent<Image>().preserveAspect=true;page.GetComponent<Image>().raycastTarget=false;
  var chars=Panel("Animated duck and dragon no wool",p.retryPage.transform,new Vector2(.5f,.5f),new Vector2(720,610),new Vector2(0,-10),Color.white);p.hero=chars;p.animatedCharacters=chars.GetComponent<Image>();p.animatedCharacters.preserveAspect=true;p.animatedCharacters.raycastTarget=false;
  p.characterFrames=new Sprite[8];var tex=sheet.texture;
  for(int i=0;i<8;i++){var path="Assets/WoolGame/Designer/UI/Defeat/LossFrame"+i+".asset";var frame=Sprite.Create(tex,new Rect(i%4*tex.width/4f+8,(1-i/4)*tex.height/2f+8,tex.width/4f-16,tex.height/2f-16),new Vector2(.5f,.12f),100,0,SpriteMeshType.FullRect);var old=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);if(old){UnityEditor.EditorUtility.CopySerialized(frame,old);Object.DestroyImmediate(frame);frame=old;}else UnityEditor.AssetDatabase.CreateAsset(frame,path);p.characterFrames[i]=frame;}
  p.animatedCharacters.sprite=p.characterFrames[0];p.smilingDragon=null;p.tears=new Image[0];
  LossHit(p.coinButton,p.offerPage.transform,new Vector2(-155,-350),new Vector2(290,145));
  LossHit(p.continueButton,p.offerPage.transform,new Vector2(155,-350),new Vector2(290,145));
  LossHit(p.cancelOffer,p.offerPage.transform,new Vector2(315,215),new Vector2(110,110));
  LossHit(p.retry,p.retryPage.transform,new Vector2(0,-485),new Vector2(450,155));
  LossHit(p.home,p.retryPage.transform,new Vector2(-305,590),new Vector2(110,110));
  var paymentCoin=Panel("Canonical duck footprint coin",p.coinButton.transform,new Vector2(.5f,.5f),new Vector2(105,105),new Vector2(-78,0),Color.white);paymentCoin.GetComponent<Image>().sprite=LossSprite("Assets/WoolGame/Designer/UI/Unlock/DuckFootprintCoin.png");paymentCoin.GetComponent<Image>().preserveAspect=true;paymentCoin.GetComponent<Image>().raycastTarget=false;
  p.backdrop=modal.GetComponent<Image>();p.gameObject.SetActive(false);
  UnityEditor.EditorUtility.SetDirty(p);UnityEditor.AssetDatabase.SaveAssets();UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(gameObject.scene);return "Reference loss pages and 8 animation frames saved";
 }
 static Sprite LossSprite(string path){var ti=(UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(path);ti.textureType=UnityEditor.TextureImporterType.Sprite;ti.spriteImportMode=UnityEditor.SpriteImportMode.Single;ti.alphaIsTransparency=true;ti.mipmapEnabled=false;ti.maxTextureSize=4096;ti.textureCompression=UnityEditor.TextureImporterCompression.Uncompressed;ti.SaveAndReimport();return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);}
 static void LossHit(Button b,Transform parent,Vector2 position,Vector2 size){b.transform.SetParent(parent,false);b.gameObject.SetActive(true);var r=(RectTransform)b.transform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=position;r.sizeDelta=size;r.localScale=Vector3.one;foreach(Transform c in r)c.gameObject.SetActive(false);b.GetComponent<Image>().color=Color.clear;var colors=b.colors;colors.normalColor=colors.highlightedColor=colors.pressedColor=colors.selectedColor=colors.disabledColor=Color.white;b.colors=colors;}
}}
#endif
