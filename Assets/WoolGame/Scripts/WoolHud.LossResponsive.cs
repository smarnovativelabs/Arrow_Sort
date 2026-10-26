#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
public sealed partial class WoolHud {
 public string SettleLossLayout(){
 if(Application.isPlaying)throw new System.InvalidOperationException("Edit Mode required");
 var p=losePresentation;
 foreach(Transform c in p.retryPage.transform)if(c.name=="Retry screen chrome")c.gameObject.SetActive(false);
 var tex=UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WoolGame/Designer/UI/Defeat/RetryChrome.png");
 Sprite Slice(string name,float x,float y,float w,float h){string path="Assets/WoolGame/Designer/UI/Defeat/"+name+".asset";var old=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);if(old)return old;var sp=Sprite.Create(tex,new Rect(x*tex.width,y*tex.height,w*tex.width,h*tex.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);UnityEditor.AssetDatabase.CreateAsset(sp,path);return sp;}
 RectTransform Art(string name,Sprite sp,Vector2 size){var t=p.retryPage.transform.Find(name);var r=t?(RectTransform)t:Panel(name,p.retryPage.transform,new Vector2(.5f,.5f),size,Vector2.zero,Color.white);r.sizeDelta=size;var im=r.GetComponent<Image>();im.sprite=sp;im.preserveAspect=true;im.raycastTarget=false;return r;}
 p.retryBanner=Art("Responsive retry banner",Slice("RetryBanner",0,.72f,1,.18f),new Vector2(750,260));
 p.retryCaption=Art("Responsive retry caption",Slice("RetryCaption",.13f,.20f,.74f,.10f),new Vector2(650,140));
 void Face(Button b,Sprite sp,Vector2 size){var im=b.GetComponent<Image>();im.sprite=sp;im.type=Image.Type.Simple;im.preserveAspect=true;im.color=Color.white;((RectTransform)b.transform).sizeDelta=size;}
 Face(p.retry,Slice("RetryButton",.14f,.045f,.72f,.145f),new Vector2(530,170));Face(p.home,Slice("RetryHome",.005f,.90f,.17f,.10f),new Vector2(100,100));
 var wt=p.offerPage.transform.Find("Live coin wallet");p.wallet=wt?(RectTransform)wt:Panel("Live coin wallet",p.offerPage.transform,new Vector2(.5f,.5f),new Vector2(340,145),Vector2.zero,WoolArt.Hex("573110"));
 if(!wt){var ci=Panel("Game coin",p.wallet,new Vector2(.5f,.5f),new Vector2(82,82),new Vector2(-110,22),Color.white);ci.GetComponent<Image>().sprite=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/Unlock/DuckFootprintCoin.png");ci.GetComponent<Image>().preserveAspect=true;p.balance=Label("Current coin balance",p.wallet,"0",43,new Vector2(220,65),new Vector2(48,23),Color.white);p.affordability=Label("Affordability",p.wallet,"",21,new Vector2(330,50),new Vector2(0,-44),Color.white);}
 p.gameObject.SetActive(false);UnityEditor.EditorUtility.SetDirty(p);UnityEditor.AssetDatabase.SaveAssets();UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(gameObject.scene);return "Responsive loss layout and live coin wallet saved";
 }
}}
#endif
