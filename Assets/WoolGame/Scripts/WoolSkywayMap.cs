using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    // All objects are authored in the scene. Runtime work only updates their presentation.
    public sealed class WoolSkywayMap:MonoBehaviour
    {
        public WoolHud owner;
        public ScrollRect scroll;
        public RectTransform viewport,content,artwork,navigation;
        public CanvasGroup pageFade;
        public Image duck;
        public Sprite[] idleFrames,walkFrames,happyFrames;
        public Button[] nodes;
        public WoolMapNodeGraphic[] faces;
        public TMP_Text[] numbers;
        public Button previous,next,home,shop;
        public TMP_Text pageLabel,status;
        public WoolMapNodeGraphic arrivalRing;
        public RectTransform sleepMarks;
        public float artAspect=.5628f;
        int pageStart=1,presented,frameMode;
        float width,height,clock,modeClock;
        bool travelling;
        Coroutine transition;
        Vector2 duckPoint;
        static readonly Color Green=new Color(.36f,.77f,.16f,1),Grey=new Color(.66f,.65f,.70f,1);
        // Pixel-aligned stops from the supplied 941 x 1672 skyway artwork, bottom to top.
        public static readonly Vector2[] Stops={
            new Vector2(.373f,.212f),new Vector2(.502f,.295f),new Vector2(.697f,.354f),
            new Vector2(.736f,.444f),new Vector2(.541f,.504f),new Vector2(.353f,.562f),
            new Vector2(.520f,.623f),new Vector2(.721f,.657f),new Vector2(.765f,.728f),
            new Vector2(.600f,.774f),new Vector2(.359f,.819f)};
        public bool IsTravelling=>travelling;
        public int VisiblePageStart=>pageStart;
        public int PresentedLevel=>presented;
        public string AnimationState=>frameMode==1?"Walking":frameMode==2?"Celebrating":"Idle";
        void Awake(){BindButtons();}
        void BindButtons()
        {
            for(int i=0;i<nodes.Length;i++)
            {
                int slot=i;nodes[i].onClick.RemoveAllListeners();
                nodes[i].onClick.AddListener(()=>{int level=pageStart+slot;if(!travelling&&WoolProgress.IsUnlocked(level))owner.OpenSkywayLevel(level);});
            }
            previous.onClick.RemoveAllListeners();previous.onClick.AddListener(()=>ChangePage(-1));
            next.onClick.RemoveAllListeners();next.onClick.AddListener(()=>ChangePage(1));
            home.onClick.RemoveAllListeners();home.onClick.AddListener(owner.SkywayHome);
            shop.onClick.RemoveAllListeners();shop.onClick.AddListener(owner.SkywayShop);
        }
        void OnDisable()
        {
            if(transition!=null)StopCoroutine(transition);transition=null;travelling=false;
            if(scroll)scroll.enabled=true;
            if(pageFade)pageFade.alpha=1;
            if(arrivalRing)arrivalRing.gameObject.SetActive(false);
            if(nodes!=null)foreach(var node in nodes)if(node)node.transform.localScale=Vector3.one;
        }
        public void OpenMap()
        {
            BindButtons();
            if(transition!=null)StopCoroutine(transition);
            travelling=false;scroll.enabled=true;pageFade.alpha=1;clock=modeClock=0;frameMode=0;
            int target=WoolProgress.MapDestinationLevel;
            presented=WoolProgress.MapPresentedLevel;
            pageStart=((Mathf.Max(1,target)-1)/Stops.Length)*Stops.Length+1;
            Canvas.ForceUpdateCanvases();Fit();RefreshNodes();
            bool shouldTravel=target>presented;
            if(WoolProgress.OnboardingStep==WoolOnboardingStep.Map&&target>1&&presented>=target){presented=target-1;shouldTravel=true;}
            SetDuckPoint(shouldTravel&&presented<pageStart?Stops[0]-new Vector2(.15f,.12f):Stops[Mathf.Clamp(presented-pageStart,0,Stops.Length-1)]);
            duck.rectTransform.localScale=Vector3.one;duck.gameObject.SetActive(true);
            Focus(duckPoint.y);if(!shouldTravel)WoolProgress.MarkMapPresented(target);SetStatus();
            transition=StartCoroutine(shouldTravel?TravelTo(target):FadeIn());
        }
        IEnumerator FadeIn()
        {
            pageFade.alpha=0;
            for(float t=0;t<.25f;t+=Time.unscaledDeltaTime){pageFade.alpha=Mathf.SmoothStep(0,1,t/.25f);yield return null;}
            pageFade.alpha=1;transition=null;
        }
        public void Fit()
        {
            var root=(RectTransform)transform;
            if(root.rect.width<=0||root.rect.height<=0)return;
            float navWidth=Mathf.Min(root.rect.width,1180);
            float navHeight=navWidth*(214f/941f);
            navigation.anchorMin=navigation.anchorMax=new Vector2(.5f,0);navigation.pivot=new Vector2(.5f,0);
            navigation.sizeDelta=new Vector2(navWidth,navHeight);navigation.anchoredPosition=Vector2.zero;
            // The scrolling map itself reaches every screen edge; controls remain in the safe parent.
            WoolCanvasArt.FillCanvas(viewport);
            float w=viewport.rect.width,h=viewport.rect.height;
            if(Mathf.Abs(w-width)<.1f&&Mathf.Abs(h-height)<.1f)return;
            width=w;height=h;
            if(pageLabel){var caption=(RectTransform)pageLabel.transform.parent;caption.anchorMin=caption.anchorMax=new Vector2(.5f,1);caption.pivot=new Vector2(.5f,1);caption.anchoredPosition=new Vector2(0,-22);caption.sizeDelta=new Vector2(Mathf.Min(w-240,520),106);}
            for(int side=0;side<2;side++){var arrow=(RectTransform)(side==0?previous:next).transform;arrow.anchorMin=arrow.anchorMax=new Vector2(side,1);arrow.pivot=new Vector2(side,1);arrow.anchoredPosition=new Vector2(side==0?22:-22,-22);arrow.sizeDelta=new Vector2(90,90);}
            float artWidth=Mathf.Max(w,h*artAspect);
            float ah=artWidth/Mathf.Max(.1f,artAspect);
            content.sizeDelta=new Vector2(w,Mathf.Max(h,ah));content.localScale=Vector3.one;
            artwork.sizeDelta=new Vector2(artWidth,ah);artwork.anchoredPosition=Vector2.zero;artwork.localScale=Vector3.one;
            for(int i=0;i<nodes.Length;i++)
            {
                var rt=(RectTransform)nodes[i].transform;
                rt.anchorMin=rt.anchorMax=Stops[i];rt.anchoredPosition=Vector2.zero;rt.sizeDelta=new Vector2(w*.115f,w*.095f);
                numbers[i].enableAutoSizing=true;numbers[i].fontSizeMin=w*.018f;numbers[i].fontSizeMax=w*.042f;numbers[i].fontSize=w*.042f;
            }
            duck.rectTransform.sizeDelta=Vector2.one*w*.20f;
            arrivalRing.rectTransform.sizeDelta=new Vector2(w*.18f,w*.13f);
            if(sleepMarks){sleepMarks.sizeDelta=new Vector2(w*.10f,w*.10f);var text=sleepMarks.GetComponent<TMP_Text>();if(text)text.fontSize=w*.032f;}
        }
        void RefreshNodes(int holdLocked=-1)
        {
            for(int i=0;i<nodes.Length;i++)
            {
                int level=pageStart+i;bool valid=level<=WoolProgress.TotalLevels;
                nodes[i].gameObject.SetActive(valid);if(!valid)continue;
                bool unlocked=WoolProgress.IsUnlocked(level)&&level!=holdLocked;
                faces[i].face=unlocked?Green:Grey;faces[i].SetVerticesDirty();
                numbers[i].text=level.ToString();numbers[i].color=Color.white;
                numbers[i].outlineWidth=.14f;numbers[i].outlineColor=unlocked?new Color(.12f,.36f,.06f,1):new Color(.31f,.30f,.36f,1);
                nodes[i].interactable=unlocked&&!travelling;nodes[i].transform.localScale=Vector3.one;
            }
            previous.gameObject.SetActive(pageStart>1);next.gameObject.SetActive(pageStart+Stops.Length<=WoolProgress.TotalLevels);
            previous.interactable=!travelling&&pageStart>1;
            next.interactable=!travelling&&pageStart+Stops.Length<=WoolProgress.TotalLevels;
            pageLabel.text="LEVELS "+pageStart+" - "+Mathf.Min(pageStart+Stops.Length-1,WoolProgress.TotalLevels);
            SetStatus();
        }
        void SetStatus()
        {
            if(status)status.text=travelling?"RESCUE COMPLETE!":WoolProgress.CompletedCount==0?"YOUR RESCUE ADVENTURE STARTS HERE":WoolProgress.CompletedCount+" LEVELS RESCUED";
        }
        void Update()
        {
            Fit();float dt=Mathf.Min(Time.unscaledDeltaTime,.1f);clock+=dt;modeClock+=dt;
            if(duck&&duck.gameObject.activeSelf)
            {
                Sprite[] frames=frameMode==1?walkFrames:frameMode==2?happyFrames:idleFrames;
                if(frames!=null&&frames.Length>0)duck.sprite=frames[Mathf.FloorToInt(modeClock*(frameMode==1?16:12))%frames.Length];
            }
            if(!travelling)
            {
                int index=WoolProgress.CurrentLevel-pageStart;
                if(index>=0&&index<nodes.Length)nodes[index].transform.localScale=Vector3.one*(1+.035f*Mathf.Sin(clock*2.6f));
            }
            if(sleepMarks)
            {
                sleepMarks.anchoredPosition=new Vector2(0,Mathf.Sin(clock*1.2f)*width*.008f);
                var color=sleepMarks.GetComponent<TMP_Text>().color;color.a=.55f+.25f*Mathf.Sin(clock*1.2f);
                sleepMarks.GetComponent<TMP_Text>().color=color;
            }
        }
        void SetMode(int mode){if(frameMode!=mode){frameMode=mode;modeClock=0;}}
        void SetDuckPoint(Vector2 point)
        {
            duckPoint=point;duck.rectTransform.anchorMin=duck.rectTransform.anchorMax=point;
            duck.rectTransform.anchoredPosition=Vector2.zero;
        }
        void Focus(float y)
        {
            float travel=content.rect.height-viewport.rect.height;
            scroll.StopMovement();
            if(travel>.1f)scroll.verticalNormalizedPosition=Mathf.Clamp01((y*artwork.rect.height-viewport.rect.height*.46f)/travel);
            else scroll.verticalNormalizedPosition=0;
        }
        IEnumerator TravelTo(int target)
        {
            travelling=true;scroll.enabled=false;pageFade.alpha=1;
            int unlock=target==WoolProgress.CurrentLevel?target:-1;RefreshNodes(unlock);
            yield return new WaitForSecondsRealtime(.3f);SetMode(1);
            int destination=Mathf.Clamp(target-pageStart,0,Stops.Length-1);
            int first=presented>=pageStart&&presented<pageStart+Stops.Length?presented-pageStart+1:0;
            if(first>destination)first=destination;
            for(int index=first;index<=destination;index++)
            {
                Vector2 start=duckPoint,end=Stops[index];
                Vector2 before=index>1?Stops[index-2]:start-(end-start)*.4f;
                Vector2 after=index+1<Stops.Length?Stops[index+1]:end+(end-start)*.4f;
                float duration=Mathf.Clamp(Vector2.Distance(start,end)*7,1f,1.9f);
                duck.rectTransform.localScale=new Vector3(end.x>=start.x?-1:1,1,1);
                for(float t=0;t<duration;t+=Mathf.Min(Time.unscaledDeltaTime,.05f))
                {
                    float u=Mathf.SmoothStep(0,1,t/duration);
                    // Catmull-Rom keeps the walk following the winding skyway between stops.
                    Vector2 p=.5f*((2*start)+(-before+end)*u+(2*before-5*start+4*end-after)*u*u+(-before+3*start-3*end+after)*u*u*u);
                    SetDuckPoint(p);Focus(p.y);yield return null;
                }
                SetDuckPoint(end);
            }
            SetMode(2);duck.rectTransform.localScale=Vector3.one;
            arrivalRing.gameObject.SetActive(true);
            arrivalRing.rectTransform.anchorMin=arrivalRing.rectTransform.anchorMax=Stops[destination];
            arrivalRing.rectTransform.anchoredPosition=Vector2.zero;
            for(float t=0;t<.8f;t+=Mathf.Min(Time.unscaledDeltaTime,.05f))
            {
                arrivalRing.rectTransform.localScale=Vector3.one*(.55f+t*1.5f);
                arrivalRing.color=new Color(1,.87f,.25f,1-t/.8f);arrivalRing.SetVerticesDirty();yield return null;
            }
            arrivalRing.gameObject.SetActive(false);
            int unlockedIndex=unlock-pageStart;
            if(unlockedIndex>=0&&unlockedIndex<nodes.Length)
            {
                for(float t=0;t<.45f;t+=Mathf.Min(Time.unscaledDeltaTime,.05f))
                {
                    float u=Mathf.Clamp01(t/.45f);faces[unlockedIndex].face=Color.Lerp(Grey,Green,u);faces[unlockedIndex].SetVerticesDirty();
                    nodes[unlockedIndex].transform.localScale=Vector3.one*(1+Mathf.Sin(u*Mathf.PI)*.2f);yield return null;
                }
            }
            yield return new WaitForSecondsRealtime(.35f);
            presented=target;WoolProgress.MarkMapPresented(target);travelling=false;scroll.enabled=true;
            SetMode(0);RefreshNodes();transition=null;
        }
        void ChangePage(int direction)
        {
            int target=pageStart+direction*Stops.Length;
            if(travelling||target<1||target>WoolProgress.TotalLevels)return;
            if(transition!=null)StopCoroutine(transition);
            pageFade.alpha=1;transition=StartCoroutine(PageTransition(target));
        }
        IEnumerator PageTransition(int target)
        {
            travelling=true;scroll.enabled=false;
            for(float t=0;t<.18f;t+=Time.unscaledDeltaTime){pageFade.alpha=1-t/.18f;yield return null;}
            pageStart=target;RefreshNodes();
            int local=presented-pageStart;duck.gameObject.SetActive(local>=0&&local<Stops.Length);
            if(duck.gameObject.activeSelf)SetDuckPoint(Stops[local]);
            Focus(duck.gameObject.activeSelf?duckPoint.y:Stops[0].y);
            for(float t=0;t<.22f;t+=Time.unscaledDeltaTime){pageFade.alpha=t/.22f;yield return null;}
            pageFade.alpha=1;travelling=false;scroll.enabled=true;RefreshNodes();transition=null;
        }
#if UNITY_EDITOR
        static RectTransform Rect(string name,Transform parent)
        {
            var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rt.SetParent(parent,false);return rt;
        }
        static Image Picture(string name,Transform parent,Sprite sprite)
        {
            var rt=Rect(name,parent);var image=rt.gameObject.AddComponent<Image>();image.sprite=sprite;image.raycastTarget=false;return image;
        }
        static void Fill(RectTransform rt){rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;}
        static TMP_Text Text(string name,Transform parent,TMP_FontAsset font,float size)
        {
            var rt=Rect(name,parent);var label=rt.gameObject.AddComponent<TextMeshProUGUI>();label.font=font;label.fontSize=size;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;label.color=new Color(.34f,.20f,.09f,1);return label;
        }
        static Button Hit(string name,Transform parent)
        {
            var rt=Rect(name,parent);var image=rt.gameObject.AddComponent<Image>();image.color=Color.clear;var b=rt.gameObject.AddComponent<Button>();b.targetGraphic=image;b.transition=Selectable.Transition.None;return b;
        }
        public void Author(WoolHud hud,Sprite background,Sprite navSprite,Sprite[] idle,Sprite[] walk,Sprite[] happy,TMP_FontAsset font)
        {
            owner=hud;idleFrames=idle;walkFrames=walk;happyFrames=happy;artAspect=background.rect.width/background.rect.height;
            var root=(RectTransform)transform;Fill(root);
            var backdrop=gameObject.AddComponent<Image>();backdrop.color=new Color(.30f,.73f,.96f,1);backdrop.raycastTarget=false;
            var view=Rect("Skyway viewport",transform);Fill(view);view.gameObject.AddComponent<Image>().color=Color.clear;view.gameObject.AddComponent<RectMask2D>();
            viewport=view;scroll=view.gameObject.AddComponent<ScrollRect>();scroll.viewport=view;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.decelerationRate=.08f;scroll.scrollSensitivity=45;
            content=Rect("Skyway content",view);content.anchorMin=content.anchorMax=new Vector2(.5f,1);content.pivot=new Vector2(.5f,1);scroll.content=content;pageFade=content.gameObject.AddComponent<CanvasGroup>();
            artwork=Rect("Reference map art",content);artwork.anchorMin=artwork.anchorMax=new Vector2(.5f,0);artwork.pivot=new Vector2(.5f,0);
            var picture=Picture("Floating sky islands",artwork,background);Fill(picture.rectTransform);
            nodes=new Button[Stops.Length];faces=new WoolMapNodeGraphic[Stops.Length];numbers=new TMP_Text[Stops.Length];
            for(int i=0;i<nodes.Length;i++)
            {
                var rt=Rect("Level node "+(i+1),artwork);var face=rt.gameObject.AddComponent<WoolMapNodeGraphic>();face.raycastTarget=true;
                var button=rt.gameObject.AddComponent<Button>();button.targetGraphic=face;button.transition=Selectable.Transition.None;
                var label=Text("Level number",rt,font,40);Fill(label.rectTransform);label.rectTransform.offsetMin=new Vector2(0,8);
                nodes[i]=button;faces[i]=face;numbers[i]=label;
            }
            var ring=Rect("Arrival sparkle ring",artwork);arrivalRing=ring.gameObject.AddComponent<WoolMapNodeGraphic>();arrivalRing.ring=true;arrivalRing.raycastTarget=false;ring.gameObject.SetActive(false);
            duck=Picture("Duck family - animated",artwork,idle[0]);duck.rectTransform.pivot=new Vector2(.5f,.18f);duck.preserveAspect=true;
            var z=Text("Sleeping dragon",artwork,font,30);z.text="z  z  Z";z.color=new Color(1,1,1,.75f);sleepMarks=z.rectTransform;sleepMarks.anchorMin=sleepMarks.anchorMax=new Vector2(.80f,.94f);sleepMarks.localRotation=Quaternion.Euler(0,0,15);
            navigation=Rect("Map navigation",transform);var nav=Picture("Map selected navigation",navigation,navSprite);Fill(nav.rectTransform);
            shop=Hit("Shop",navigation);home=Hit("Home",navigation);var map=Hit("Map selected",navigation);
            var buttons=new[]{shop,home,map};for(int i=0;i<3;i++){var rt=(RectTransform)buttons[i].transform;rt.anchorMin=new Vector2(i/3f,0);rt.anchorMax=new Vector2((i+1)/3f,1);rt.offsetMin=rt.offsetMax=Vector2.zero;}
            var labelRoot=Rect("Page caption",transform);labelRoot.anchorMin=labelRoot.anchorMax=new Vector2(.5f,1);labelRoot.pivot=new Vector2(.5f,1);labelRoot.anchoredPosition=new Vector2(0,-18);labelRoot.sizeDelta=new Vector2(490,88);
            var band=labelRoot.gameObject.AddComponent<Image>();band.sprite=hud.SkywayPanelSprite;band.type=Image.Type.Sliced;band.color=new Color(1,.96f,.84f,.9f);band.raycastTarget=false;
            pageLabel=Text("Page levels",labelRoot,font,27);Fill(pageLabel.rectTransform);pageLabel.rectTransform.offsetMin=new Vector2(6,36);
            status=Text("Journey status",labelRoot,font,17);Fill(status.rectTransform);status.rectTransform.offsetMax=new Vector2(-6,-43);
            previous=Hit("Previous skyway",transform);next=Hit("Next skyway",transform);
            for(int i=0;i<2;i++)
            {
                var b=i==0?previous:next;var rt=(RectTransform)b.transform;rt.anchorMin=rt.anchorMax=new Vector2(i,1);rt.pivot=new Vector2(i,1);rt.sizeDelta=new Vector2(100,100);rt.anchoredPosition=new Vector2(i==0?12:-12,-18);
                b.GetComponent<Image>().sprite=hud.SkywayPanelSprite;b.GetComponent<Image>().type=Image.Type.Sliced;b.GetComponent<Image>().color=new Color(1,.92f,.65f,.9f);var label=Text("Arrow",rt,font,52);label.text=i==0?"<":">";Fill(label.rectTransform);
            }
            Canvas.ForceUpdateCanvases();Fit();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
