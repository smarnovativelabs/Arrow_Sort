using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    public sealed class WoolLoadingScreen : MonoBehaviour
    {
        public Sprite duck, family; public TMP_FontAsset font;
        public Sprite illustration,logo;
        RectTransform scenicBackground,bar;
        bool illustrated;
        RectTransform content,mascot,title,fill; TMP_Text status; CanvasGroup group;
        float started,progress; bool built;
        public bool Ready => built;
        public float Progress => progress;
        public void SetProgress(float value){progress=Mathf.Clamp01(value);ApplyFill();}
        void ApplyFill(){if(!built||!fill)return;float width=(illustrated?742:640)*progress;fill.sizeDelta=new Vector2(width,illustrated?60:26);fill.anchoredPosition=new Vector2((illustrated?-371:-320)+width*.5f,illustrated?0:-450);}
        void OnEnable(){if(!built)Build();started=Time.unscaledTime;progress=0;ApplyFill();Layout();}
        RectTransform Rect(string name,Transform parent,Vector2 size,Vector2 pos)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.sizeDelta=size;r.anchoredPosition=pos;return r;
        }
        Color C(string s)=>WoolArt.Hex(s);
        void Surface(string name,Transform parent,Vector2 size,Vector2 pos,string a,string b,float radius)
        {
            var r=Rect(name,parent,size,pos);var g=r.gameObject.AddComponent<WoolDailySurface>();g.top=C(a);g.bottom=C(b);g.radius=radius;g.stroke=0;g.raycastTarget=false;
        }
        TMP_Text Text(string name,string text,Transform parent,Vector2 size,Vector2 pos,float height,string color)
        {
            var t=Rect(name,parent,size,pos).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=text;t.fontSize=height;t.alignment=TextAlignmentOptions.Center;t.color=C(color);t.raycastTarget=false;return t;
        }
        void Picture(string name,Sprite sprite,Transform parent,Vector2 size,Vector2 pos)
        {
            var im=Rect(name,parent,size,pos).gameObject.AddComponent<Image>();im.sprite=sprite;im.preserveAspect=true;im.raycastTarget=false;
        }
        void Build()
        {
            if(illustration&&logo){BuildIllustrated();return;}
            foreach(Transform child in transform)child.gameObject.SetActive(false);
            // Independent overlay covers the physical display; only its content uses safe-area bounds.
            var root=new GameObject("Arrow Sort loading overlay",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));root.transform.SetParent(transform,false);
            var canvas=root.GetComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=32000;
            var rr=root.GetComponent<RectTransform>();rr.anchorMin=Vector2.zero;rr.anchorMax=Vector2.one;rr.offsetMin=rr.offsetMax=Vector2.zero;
            Surface("Midnight purple",root.transform,new Vector2(10000,10000),Vector2.zero,"372560","121B40",0);
            var block=root.AddComponent<Image>();block.color=Color.clear;block.raycastTarget=true;
            content=Rect("Safe loading composition",root.transform,new Vector2(900,1400),Vector2.zero);
            group=content.gameObject.AddComponent<CanvasGroup>();
            Surface("Halo outer",content,new Vector2(740,740),new Vector2(0,20),"493D82","263661",370);
            Surface("Halo inner",content,new Vector2(620,620),new Vector2(0,20),"6461A0","334877",310);
            Text("Rescue invitation","A LITTLE PUZZLE. A BIG RESCUE.",content,new Vector2(820,70),new Vector2(0,620),27,"DACAF2");
            title=Rect("Animated game name",content,new Vector2(880,250),new Vector2(0,410));
            Text("Title shadow","Arrow Sort Puzzle",title,new Vector2(880,115),new Vector2(0,40),79,"16152F");
            Text("Game name","Arrow Sort Puzzle",title,new Vector2(880,115),new Vector2(0,47),79,"FFF5D5");
            Text("Game subtitle","Rescue Ducks",title,new Vector2(820,115),new Vector2(0,-63),88,"FFD34A");
            mascot=Rect("Floating duck",content,new Vector2(550,540),new Vector2(0,30));
            Picture("Your duck mascot",duck,mascot,new Vector2(500,510),new Vector2(0,45));
            Picture("Duck family",family,content,new Vector2(260,210),new Vector2(185,-175));
            Text("Rescue motto","Sort the arrows. Save the ducks.",content,new Vector2(820,80),new Vector2(0,-340),38,"FFF1D7");
            Surface("Progress gold rim",content,new Vector2(660,44),new Vector2(0,-450),"FFD76A","C48527",22);
            Surface("Progress track",content,new Vector2(648,32),new Vector2(0,-450),"171D42","171D42",16);
            fill=Rect("Animated loading fill",content,new Vector2(0,26),new Vector2(-320,-450));fill.pivot=new Vector2(0,.5f);var fg=fill.gameObject.AddComponent<WoolDailySurface>();fg.top=C("B7F66A");fg.bottom=C("40BF69");fg.radius=13;fg.stroke=0;fg.raycastTarget=false;
            status=Text("Loading dots","Loading",content,new Vector2(700,80),new Vector2(0,-535),36,"E0D9F5");
            built=true;
        }
        void BuildIllustrated()
        {
            foreach(Transform child in transform)child.gameObject.SetActive(false);
            var root=new GameObject("Illustrated rescue loading",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster),typeof(Image));root.transform.SetParent(transform,false);
            var canvas=root.GetComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=32000;
            var rr=(RectTransform)root.transform;rr.anchorMin=Vector2.zero;rr.anchorMax=Vector2.one;rr.offsetMin=rr.offsetMax=Vector2.zero;root.GetComponent<Image>().color=Color.black;
            scenicBackground=Rect("Castle and duck family artwork",rr,Vector2.one,Vector2.zero);var bg=scenicBackground.gameObject.AddComponent<Image>();bg.sprite=illustration;bg.raycastTarget=false;
            content=Rect("Safe loading composition",rr,new Vector2(940,1672),Vector2.zero);group=content.gameObject.AddComponent<CanvasGroup>();
            title=Rect("Animated game name",content,new Vector2(900,340),Vector2.zero);var ti=title.gameObject.AddComponent<Image>();ti.sprite=logo;ti.preserveAspect=true;ti.raycastTarget=false;
            var shine=title.gameObject.AddComponent<Coffee.UIEffects.UIShiny>();shine.width=.12f;shine.softness=.6f;shine.brightness=.3f;shine.gloss=.4f;shine.effectPlayer.play=true;shine.effectPlayer.loop=true;shine.effectPlayer.duration=1.2f;shine.effectPlayer.loopDelay=1.4f;shine.effectPlayer.updateMode=AnimatorUpdateMode.UnscaledTime;
            bar=Rect("Live loading progress",content,new Vector2(790,100),Vector2.zero);
            Surface("Gold frame",bar,new Vector2(790,100),Vector2.zero,"FFE485","DA730A",50);
            Surface("Dark inner track",bar,new Vector2(754,68),Vector2.zero,"743406","9C4A06",34);
            fill=Rect("Live green fill",bar,new Vector2(0,60),new Vector2(-371,0));fill.pivot=new Vector2(0,.5f);var fg=fill.gameObject.AddComponent<WoolDailySurface>();fg.top=C("C1FF24");fg.bottom=C("35BF05");fg.radius=30;fg.stroke=0;fg.raycastTarget=false;
            status=Text("Loading status","Loading...",content,new Vector2(800,80),Vector2.zero,48,"FFF4D8");
            illustrated=built=true;
        }
        void Layout()
        {
            if(!content)return;var canvas=GetComponentInParent<Canvas>().rootCanvas;var cr=canvas.GetComponent<RectTransform>().rect;
            var safe=Screen.safeArea;float sx=cr.width/Screen.width,sy=cr.height/Screen.height;
            var scale=Mathf.Min(safe.width*sx/940,safe.height*sy/1480);content.localScale=Vector3.one*scale;
            content.anchoredPosition=new Vector2((safe.center.x-Screen.width*.5f)*sx,(safe.center.y-Screen.height*.5f)*sy);
            if(illustrated){WoolCanvasArt.Cover(scenicBackground,illustration.rect.width/illustration.rect.height);float half=safe.height*sy/scale*.5f;title.anchoredPosition=new Vector2(0,half-195);bar.anchoredPosition=new Vector2(0,-half+185);status.rectTransform.anchoredPosition=new Vector2(0,-half+85);}
        }
        void Update()
        {
            if(!built)return;Layout();float t=Time.unscaledTime-started;
            group.alpha=Mathf.Clamp01(t/.3f);title.localScale=Vector3.one*(1+.012f*Mathf.Sin(t*2));
            if(illustrated){title.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*1.7f)*.65f);title.anchoredPosition+=new Vector2(0,Mathf.Sin(t*2)*7);float width=742*progress;fill.sizeDelta=new Vector2(width,60);fill.anchoredPosition=new Vector2(-371+width*.5f,0);status.text="Loading"+new string('.',1+(int)(t*2)%3);return;}
            mascot.anchoredPosition=new Vector2(0,30+Mathf.Sin(t*2.5f)*10);mascot.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*1.8f)*1.5f);
            fill.sizeDelta=new Vector2(640*progress,26);status.text="Loading"+new string('.',1+(int)(t*2)%3);
        }
    }
}
