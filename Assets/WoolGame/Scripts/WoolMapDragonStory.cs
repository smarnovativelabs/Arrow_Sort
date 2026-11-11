using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
[DefaultExecutionOrder(250)]
public sealed class WoolMapDragonStory:MonoBehaviour {
 public WoolSkywayMap map;
 public Sprite[] poses;
 public Image dragon;
 public TMPro.TMP_Text worldTitle;
 public Image progress;
 public RectTransform sky,scenery,flame;
 float nextProgress,clock,changedAt;
 int shown=-1;

 public int Posture{get;private set;}
 void FitCave(){
  if(!map||!map.artwork||!dragon)return;
  // Cave, path and painted fire belong to the same scrolling image.
  // The dragon uses image coordinates so scrolling never separates it from its cave.
  if(dragon.transform.parent!=map.artwork)dragon.transform.SetParent(map.artwork,false);
  if(sky)sky.gameObject.SetActive(false);
  if(scenery)scenery.gameObject.SetActive(false);
  if(flame)flame.gameObject.SetActive(false);
  float width=map.artwork.rect.width;
  var body=dragon.rectTransform;
  body.anchorMin=body.anchorMax=new Vector2(.61f,.815f);
  body.pivot=new Vector2(.5f,0);
  body.sizeDelta=new Vector2(width*.43f,width*.23f);
  body.anchoredPosition=Vector2.zero;
  dragon.maskable=true;dragon.raycastTarget=false;
 }
 void Update(){
  if(!map||!dragon||poses==null||poses.Length<11)return;
  clock+=Time.unscaledDeltaTime;FitCave();
  if(worldTitle)worldTitle.text="WORLD "+((map.VisiblePageStart-1)/11+1);
  if(progress&&clock>=nextProgress){nextProgress=clock+.4f;int complete=0;for(int n=map.VisiblePageStart;n<map.VisiblePageStart+11;n++)if(WoolProgress.HasCompleted(n))complete++;progress.fillAmount=complete/11f;if(map.status)map.status.text=complete+" / 11 RESCUED";}
  int stage=Mathf.Clamp(map.PresentedLevel-map.VisiblePageStart,0,10);Posture=stage;
  if(shown!=stage){shown=stage;changedAt=clock;dragon.sprite=poses[stage];}
  float settle=Mathf.SmoothStep(.94f,1,Mathf.Clamp01((clock-changedAt)/.65f));
  float breathe=1+Mathf.Sin(clock*(stage>=7?3:1.5f))*(stage>=7?.012f:.006f);
  dragon.rectTransform.localScale=Vector3.one*settle*breathe;
  dragon.rectTransform.localRotation=Quaternion.Euler(0,0,stage==10?Mathf.Sin(clock*8)*.7f:0);
  if(map.sleepMarks)map.sleepMarks.gameObject.SetActive(stage<2);
 }
}
}
