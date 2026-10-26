#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame.Editor {
public static class WoolVictoryAuthoring {
 const string Folder="Assets/WoolGame/Designer/UI/Victory";
 static RectTransform Rect(string name,Transform parent,Vector2 size,Vector2 position){var rt=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer)).GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.sizeDelta=size;rt.anchoredPosition=position;return rt;}
 static TMP_Text Text(TMP_Text template,string name,Transform parent,string text,Vector2 size,Vector2 pos,float fontSize){var t=UnityEngine.Object.Instantiate(template,parent);t.name=name;t.gameObject.SetActive(true);t.text=text;t.rectTransform.anchorMin=t.rectTransform.anchorMax=new Vector2(.5f,.5f);t.rectTransform.pivot=new Vector2(.5f,.5f);t.rectTransform.sizeDelta=size;t.rectTransform.anchoredPosition=pos;t.rectTransform.localScale=Vector3.one;t.fontSize=fontSize;t.fontSizeMax=fontSize;t.fontSizeMin=fontSize*.75f;t.enableAutoSizing=true;t.raycastTarget=false;return t;}
 static Image Picture(string name,Transform parent,Sprite sprite,Vector2 size,Vector2 pos){var i=Rect(name,parent,size,pos).gameObject.AddComponent<Image>();i.sprite=sprite;i.preserveAspect=true;i.raycastTarget=false;return i;}
 static Sprite GreyButton(){
  string path=Folder+"/VictoryButtonCollected.png";var tex=new Texture2D(256,132,TextureFormat.RGBA32,false);
  for(int y=0;y<132;y++)for(int x=0;x<256;x++){
   float dx=Mathf.Max(Mathf.Abs(x-127.5f)-96,0),dy=Mathf.Max(Mathf.Abs(y-65.5f)-34,0),d=Mathf.Sqrt(dx*dx+dy*dy)-26,v=y/131f;
   Color c=Color.Lerp(new Color(.20f,.23f,.28f),new Color(.55f,.59f,.65f),v);
   if(d>-6)c=Color.Lerp(new Color(.32f,.35f,.40f),new Color(.86f,.89f,.94f),v);c.a=Mathf.Clamp01(-d);tex.SetPixel(x,y,c);
  }tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);
  var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
 }
 static Sprite[] ImportDance(){
  string path=Folder+"/HappyDuckDance.png";AssetDatabase.ImportAsset(path);
  var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Multiple;imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.maxTextureSize=2048;imp.npotScale=TextureImporterNPOTScale.None;imp.textureCompression=TextureImporterCompression.CompressedHQ;
  var cells=new SpriteMetaData[6];for(int i=0;i<6;i++)cells[i]=new SpriteMetaData{name="HappyDuck"+i,rect=new Rect((i%3)*512,(1-i/3)*512,512,512),alignment=(int)SpriteAlignment.Custom,pivot=new Vector2(.5f,.08f)};
  imp.spritesheet=cells;imp.SaveAndReimport();return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
 }
 [MenuItem("Wool/Author polished victory")]
 public static void Apply(){
  if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before authoring.");
  var p=UnityEngine.Object.FindFirstObjectByType<WoolWinPresentation>(FindObjectsInactive.Include);if(!p)throw new InvalidOperationException("Missing victory presentation.");
  Undo.RegisterFullObjectHierarchyUndo(p.gameObject,"Polished victory screen");
  Sprite[] frames=ImportDance();if(frames.Length!=6)throw new InvalidOperationException("Expected six duck poses.");
  p.danceFrames=frames;p.danceFrameSeconds=.18f;p.collectedButton=GreyButton();
  var root=(RectTransform)p.transform;p.card.sizeDelta=new Vector2(820,1000);p.card.GetComponent<Image>().enabled=false;
  p.duck.sizeDelta=new Vector2(540,540);p.duck.anchoredPosition=new Vector2(0,-5);p.duck.localScale=Vector3.one;p.duck.localRotation=Quaternion.identity;p.duck.GetComponent<Image>().sprite=frames[0];p.duck.GetComponent<Image>().preserveAspect=true;
  var bg=root.Find("Victory full screen background").GetComponent<Image>();bg.sprite=null;bg.color=new Color(.035f,.026f,.075f,1);bg.raycastTarget=false;
  var g=p.card.Find("Golden victory spotlight") as RectTransform;if(!g){g=Rect("Golden victory spotlight",p.card,new Vector2(970,970),new Vector2(0,0));g.gameObject.AddComponent<WoolVictoryGlow>().raycastTarget=false;}g.SetAsFirstSibling();p.glow=g;
  var banner=p.card.Find("Victory banner").GetComponent<Image>();banner.gameObject.SetActive(true);banner.rectTransform.sizeDelta=new Vector2(790,395);banner.rectTransform.anchoredPosition=new Vector2(0,345);banner.color=Color.white;
  // Existing polished ribbon art is shared with the game's other celebration panels.
  var candidates=AssetDatabase.FindAssets("t:Sprite",new[]{"Assets/WoolGame/Designer/UI"}).Select(AssetDatabase.GUIDToAssetPath).Where(x=>x.IndexOf("WinBanner",StringComparison.OrdinalIgnoreCase)>=0||x.IndexOf("VictoryBanner",StringComparison.OrdinalIgnoreCase)>=0).ToArray();
  if(candidates.Length>0)banner.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(candidates[0]);
  var crown=AssetDatabase.LoadAssetAtPath<Sprite>(Folder+"/VictoryCrown.png");if(crown)banner.sprite=crown;
  var title=p.card.Find("Editable victory title").GetComponent<TMP_Text>();title.gameObject.SetActive(true);title.text="Victory!";title.rectTransform.sizeDelta=new Vector2(670,145);title.rectTransform.anchoredPosition=new Vector2(0,320);title.fontSize=104;title.fontSizeMax=104;title.fontSizeMin=85;title.enableAutoSizing=true;title.color=new Color(1,.94f,.52f);title.fontSharedMaterial=p.collectLabel.fontSharedMaterial;title.enableVertexGradient=true;title.colorGradient=new VertexGradient(new Color(1,1,.8f),new Color(1,1,.8f),new Color(1,.63f,.10f),new Color(1,.63f,.10f));
  title.gameObject.SetActive(!crown);
  var row=(RectTransform)p.rewardLabel.transform.parent;row.sizeDelta=new Vector2(400,105);row.anchoredPosition=new Vector2(0,-325);row.GetComponent<Image>().color=new Color(1,1,1,.92f);p.rewardOrigin.sizeDelta=new Vector2(88,88);p.rewardOrigin.anchoredPosition=new Vector2(-100,0);p.rewardLabel.rectTransform.anchoredPosition=new Vector2(45,0);p.rewardLabel.rectTransform.sizeDelta=new Vector2(250,90);p.rewardLabel.fontSizeMax=66;
  if(!p.card.Find("Victory caption")){var subtitle=Text(p.collectLabel,"Victory caption",p.card,"LEVEL COMPLETE",new Vector2(600,65),new Vector2(0,-425),31);subtitle.color=new Color(1,.84f,.51f);subtitle.characterSpacing=6;}
  if(!p.rewardButtons)p.rewardButtons=Rect("Victory reward buttons",root,new Vector2(820,160),Vector2.zero);p.rewardButtons.anchorMin=p.rewardButtons.anchorMax=new Vector2(.5f,0);
  var bs=new[]{p.collect,p.bonus};for(int i=0;i<2;i++){
   var b=bs[i];b.transform.SetParent(p.rewardButtons,false);var rt=(RectTransform)b.transform;rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.sizeDelta=new Vector2(397,160);rt.anchoredPosition=new Vector2(i==0?-210:210,0);rt.localScale=Vector3.one;
   b.GetComponent<Image>().sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Folder+"/VictoryButton"+(i==0?2:0)+".png");b.GetComponent<Image>().color=Color.white;
   var cb=b.colors;cb.normalColor=Color.white;cb.highlightedColor=Color.white;cb.pressedColor=new Color(.82f,.82f,.82f);cb.disabledColor=new Color(.72f,.72f,.72f,1);b.colors=cb;
   var label=i==0?p.collectLabel:p.bonusLabel;label.text=i==0?"Continue":"2x coins";label.rectTransform.sizeDelta=new Vector2(355,64);label.rectTransform.anchoredPosition=new Vector2(0,33);label.fontSize=47;label.fontSizeMax=47;label.fontSizeMin=38;
   foreach(Transform ch in b.transform)if(ch.GetComponent<Image>()||ch.name=="Reward video icon")ch.gameObject.SetActive(false);
   var coin=b.transform.Find("Reward coin")?b.transform.Find("Reward coin").GetComponent<Image>():Picture("Reward coin",b.transform,p.walletCoin.sprite,new Vector2(65,65),new Vector2(i==0?-72:-33,-37));coin.gameObject.SetActive(true);
   var amount=b.transform.Find("Editable reward amount")?b.transform.Find("Editable reward amount").GetComponent<TMP_Text>():Text(label,"Editable reward amount",b.transform,i==0?"+50":"+100",new Vector2(165,60),new Vector2(i==0?38:72,-37),46);
   if(i==0)p.collectAmount=amount;else {p.bonusAmount=amount;var video=b.transform.Find("Reward video icon");if(video){video.gameObject.SetActive(true);var v=(RectTransform)video;v.sizeDelta=new Vector2(56,50);v.anchoredPosition=new Vector2(-133,-34);}}
  }
  p.home.transform.SetParent(root,false);var hr=(RectTransform)p.home.transform;hr.anchorMin=hr.anchorMax=new Vector2(0,1);hr.pivot=new Vector2(0,1);hr.sizeDelta=new Vector2(90,90);
  if(!p.close){p.close=UnityEngine.Object.Instantiate(p.home,root);p.close.name="Victory Close";}var cr=(RectTransform)p.close.transform;cr.anchorMin=cr.anchorMax=Vector2.one;cr.pivot=Vector2.one;cr.sizeDelta=new Vector2(90,90);
  var top=new[]{p.home,p.close};for(int i=0;i<2;i++){
   var b=top[i];foreach(var text in b.GetComponentsInChildren<TMP_Text>(true))text.gameObject.SetActive(false);
   var icon=b.transform.Find("Safe area navigation icon");if(!icon){var rt=Rect("Safe area navigation icon",b.transform,new Vector2(57,57),Vector2.zero);var graphic=rt.gameObject.AddComponent<WoolVictoryTopIcon>();graphic.home=i==0;graphic.color=new Color(1,.96f,.79f);graphic.raycastTarget=false;}
  }
  p.wallet.anchorMin=p.wallet.anchorMax=new Vector2(.5f,1);p.wallet.pivot=new Vector2(.5f,1);p.wallet.sizeDelta=new Vector2(330,90);
  for(int i=3;i<p.flyingCoins.Length;i++)if(p.flyingCoins[i])p.flyingCoins[i].gameObject.SetActive(false);p.flyingCoins=p.flyingCoins.Take(3).ToArray();
  foreach(var coin in p.flyingCoins){coin.rectTransform.sizeDelta=new Vector2(72,72);coin.transform.SetAsLastSibling();}
  var map=UnityEngine.Object.FindFirstObjectByType<WoolSkywayMap>(FindObjectsInactive.Include);if(map){map.idleFrames=new[]{frames[0]};map.walkFrames=new[]{frames[1],frames[3]};map.happyFrames=frames;map.duck.sprite=frames[0];map.duck.name="Current level duck";map.duck.rectTransform.pivot=new Vector2(.5f,.08f);map.duck.rectTransform.localScale=Vector3.one;EditorUtility.SetDirty(map);}
  foreach(var graphic in p.GetComponentsInChildren<Graphic>(true))if(!graphic.GetComponent<CanvasRenderer>())graphic.gameObject.AddComponent<CanvasRenderer>();
  EditorUtility.SetDirty(p);EditorSceneManager.MarkSceneDirty(p.gameObject.scene);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(p.gameObject.scene);
  Debug.Log("Victory authored: six duck poses, three flying coins, safe navigation and one current-level map marker.");
 }
}}
#endif
