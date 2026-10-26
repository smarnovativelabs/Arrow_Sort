using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace WoolGame
{
    public sealed partial class WoolHud : MonoBehaviour
    {
        [SerializeField] Canvas canvas;
        [SerializeField] RectTransform safe;
        [SerializeField] TMP_Text level, coins, hint, progress, modalTitle, modalBody;
        [SerializeField] Image progressFill;
        [SerializeField] GameObject modal;
        [SerializeField] RectTransform modalCard;
        [SerializeField] Button primary, secondary;
        [SerializeField] TMP_Text primaryText, secondaryText;
        [SerializeField] Sprite rounded;
        [SerializeField] TMP_FontAsset font;
        Rect lastSafe;
        int lastWidth, lastHeight;
        [SerializeField] RectTransform header, footer, meter, tutorialCard;
        [SerializeField] TMP_Text tutorialTip;
        [SerializeField] Button close, cancelSelection;
        [SerializeField] GameObject launch;
        [SerializeField] TMP_Text launchTitle,launchProgress;
        [SerializeField] Sprite[] icons=new Sprite[16];
        [SerializeField] Image boosterIcon;
        [SerializeField] RectTransform boosterPicture;
        public bool ModalVisible => (modal && modal.activeSelf) || (utilityPanel && utilityPanel.gameObject.activeInHierarchy);
        public event Action PausePressed, PlayPressed, FreeCoinsPressed;
        public event Action<int> BoosterPressed;

        public void Build()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Author the UI in Edit Mode before playing.");
            font = TMP_Settings.defaultFontAsset;
            var iconTexture=Resources.Load<Texture2D>("WoolIcons");
            if(iconTexture)for(int i=0;i<icons.Length;i++)icons[i]=Sprite.Create(iconTexture,new Rect((i%4)*iconTexture.width/4f,(3-i/4)*iconTexture.height/4f,iconTexture.width/4f,iconTexture.height/4f),new Vector2(.5f,.5f),100);
            var texture = new Texture2D(64,64,TextureFormat.RGBA32,false);
            var data = new Color[4096];
            for(int y=0;y<64;y++) for(int x=0;x<64;x++)
            {
                float dx=Mathf.Max(14-x, x-49),dy=Mathf.Max(14-y,y-49);
                float d = new Vector2(Mathf.Max(0,dx),Mathf.Max(0,dy)).magnitude;
                data[y*64+x]=new Color(1,1,1,Mathf.Clamp01(14-d));
            }
            texture.SetPixels(data);texture.Apply();
            rounded=Sprite.Create(texture,new Rect(0,0,64,64),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(20,20,20,20));
            canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=20;
            var scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1080,1920);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scaler.matchWidthOrHeight=0;
            gameObject.AddComponent<GraphicRaycaster>();
            if(!FindFirstObjectByType<EventSystem>()) new GameObject("UI Input",typeof(EventSystem),typeof(InputSystemUIInputModule)).transform.SetParent(transform);
            safe=Rect("Safe area",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            header=Panel("Header",safe,new Vector2(.5f,1),new Vector2(650,112),new Vector2(0,-70),WoolArt.Hex("FFF9EC"));
            Label("Brand",header,"WOOL CRUSH",21,new Vector2(180,28),new Vector2(-203,25),WoolArt.Hex("2B5260"));
            level=Label("Level",header,"LEVEL 1",30,new Vector2(220,40),new Vector2(-184,-15),WoolArt.Hex("243F50"));
            coins=Label("Coins",header,"0",27,new Vector2(150,40),new Vector2(127,0),WoolArt.Hex("AB7730"));
            ButtonAt("Pause",header,"II",new Vector2(56,58),new Vector2(274,0),()=>PausePressed?.Invoke());
            meter=Panel("Progress track",safe,new Vector2(.5f,1),new Vector2(480,12),new Vector2(0,-144),WoolArt.Hex("B3CDC4"));
            progressFill=Panel("Progress fill",meter,new Vector2(0,.5f),new Vector2(480,12),Vector2.zero,WoolArt.Hex("56B690")).GetComponent<Image>();
            progressFill.rectTransform.pivot=new Vector2(0,.5f);
            progress=Label("Progress text",safe,"SAVE THE DUCK FAMILY",18,new Vector2(520,30),Vector2.zero,WoolArt.Hex("3F6B60"));
            Place(progress.rectTransform,new Vector2(.5f,1),new Vector2(0,-174));
            hint=Label("Hint",safe,"Tap a clear arrow tile to match its wool color",20,new Vector2(650,52),Vector2.zero,WoolArt.Hex("3F6266"));
            Place(hint.rectTransform,new Vector2(.5f,0),new Vector2(0,194));
            footer=Panel("Tools",safe,new Vector2(.5f,0),new Vector2(650,126),new Vector2(0,99),WoolArt.Hex("FFF9EC"));
            for(int i=0;i<4;i++)
            {
                int kind=i;var power=ButtonAt(WoolBoosters.Names[i],footer,WoolBoosters.Names[i],new Vector2(138,106),new Vector2(-231+154*i,0),()=>BoosterPressed?.Invoke(kind));
                var caption=power.GetComponentInChildren<TMP_Text>();caption.fontSize=18;caption.rectTransform.anchoredPosition=new Vector2(0,-36);caption.rectTransform.sizeDelta=new Vector2(132,28);
                Icon(power.transform,i,new Vector2(84,76),new Vector2(0,13));
            }
            var signature=Label("Footer",safe,"A LITTLE COLOR. A LITTLE COURAGE.",14,new Vector2(650,25),Vector2.zero,WoolArt.Hex("628A7D"));
            Place(signature.rectTransform,new Vector2(.5f,0),new Vector2(0,20));
            tutorialCard=Panel("Tutorial prompt",safe,new Vector2(.5f,.395f),new Vector2(600,100),Vector2.zero,WoolArt.Hex("FFF3D8"));
            tutorialCard.gameObject.SetActive(false);
            tutorialTip=Label("Tutorial tip",safe,"Tip: Move all boxes to complete the level!",23,new Vector2(600,80),Vector2.zero,WoolArt.Hex("A9875E"));
            Place(tutorialTip.rectTransform,new Vector2(.5f,.15f),Vector2.zero);tutorialTip.gameObject.SetActive(false);
            cancelSelection=ButtonAt("Cancel removal",safe,"CANCEL",new Vector2(170,58),Vector2.zero,null);
            Place(cancelSelection.GetComponent<RectTransform>(),new Vector2(.5f,0),new Vector2(0,280));cancelSelection.gameObject.SetActive(false);
            modal=new GameObject("Modal",typeof(RectTransform),typeof(Image));modal.transform.SetParent(transform,false);
            var mr=modal.GetComponent<RectTransform>();mr.anchorMin=Vector2.zero;mr.anchorMax=Vector2.one;mr.offsetMin=mr.offsetMax=Vector2.zero;
            modal.GetComponent<Image>().color=new Color(.05f,.12f,.16f,.68f);
            modalCard=Panel("Card",modal.transform,new Vector2(.5f,.5f),new Vector2(590,490),Vector2.zero,WoolArt.Hex("FFF9EC"));
            modalTitle=Label("Title",modalCard,"",38,new Vector2(530,90),new Vector2(0,146),WoolArt.Hex("294C52"));
            modalBody=Label("Message",modalCard,"",24,new Vector2(510,134),new Vector2(0,40),WoolArt.Hex("58736F"));
            primary=ButtonAt("Primary",modalCard,"",new Vector2(460,74),new Vector2(0,-88),null);primaryText=primary.GetComponentInChildren<TMP_Text>();
            secondary=ButtonAt("Secondary",modalCard,"",new Vector2(460,65),new Vector2(0,-173),null);secondaryText=secondary.GetComponentInChildren<TMP_Text>();
            close=ButtonAt("Close",modalCard,"X",new Vector2(52,52),new Vector2(267,220),null);close.gameObject.SetActive(false);
            boosterPicture=Panel("Booster illustration",modalCard,new Vector2(.5f,.5f),new Vector2(500,240),new Vector2(0,130),WoolArt.Hex("FFDBB6"));
            boosterIcon=Icon(boosterPicture,0,new Vector2(205,205),Vector2.zero);boosterPicture.gameObject.SetActive(false);
            BuildMenu();
            launch=Panel("Launch screen",transform,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero,Color.red).gameObject;
            var lr=launch.GetComponent<RectTransform>();lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=lr.offsetMax=Vector2.zero;
            launchTitle=Label("Launch title",launch.transform,"",58,new Vector2(650,180),Vector2.zero,Color.white);
            launchProgress=Label("Loading",launch.transform,"",28,new Vector2(400,70),new Vector2(0,-300),Color.white);launch.SetActive(false);
            modal.SetActive(false); RefreshSafeArea();
        }

        void Update() { if(lastSafe!=CurrentSafeArea||lastWidth!=Screen.width||lastHeight!=Screen.height) RefreshSafeArea(); }
        void RefreshSafeArea() { ApplyPortraitFrame(Screen.width,Screen.height,CurrentSafeArea);if(compactDesignHeight>0)ApplyCompactGameplayFrame(WoolSceneRig.CompactPortraitFrame(CurrentSafeArea,compactDesignHeight),compactDesignHeight,Screen.width,Screen.height); }
        internal static Rect CurrentSafeArea
        {
            get
            {
#if UNITY_EDITOR
                if(EditorSafeArea.HasValue)
                {
                    if(editorSafeResolution==new Vector2Int(Screen.width,Screen.height))return EditorSafeArea.Value;
                    EditorSafeArea=null;
                }
#endif
                return Screen.safeArea;
            }
        }
#if UNITY_EDITOR
        // Editor-only device preview; never serialized and absent from player builds.
        static Rect? editorSafeArea;
        static Vector2Int editorSafeResolution;
        public static Rect? EditorSafeArea
        {
            get=>editorSafeArea;
            set{editorSafeArea=value;if(!value.HasValue)editorSafeResolution=Vector2Int.zero;}
        }
        public static void SetEditorSafeArea(Rect area,int width,int height)
        {editorSafeArea=area;editorSafeResolution=new Vector2Int(width,height);}
#endif
        public void Status(int stage,int money,int remaining,int total)
        {
            level.text="LEVEL "+stage;coins.text=money.ToString();RefreshDifficulty(stage);RefreshAbilityInventory();
            float cleared=1-(float)remaining/Mathf.Max(1,total);
            progressFill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,480*cleared);
            progress.text=remaining==0?"FAMILY RESCUED!":Mathf.RoundToInt(cleared*100)+"%  •  SAVE THE DUCK FAMILY";
        }
        public void Hint(string text) { hint.text=text; }
        public void Tutorial(bool enabled)
        {
            safe.gameObject.SetActive(true);header.gameObject.SetActive(true);footer.gameObject.SetActive(true);meter.gameObject.SetActive(true);progress.gameObject.SetActive(true);
            var toolsInput=footer.GetComponent<CanvasGroup>();if(!toolsInput)toolsInput=footer.gameObject.AddComponent<CanvasGroup>();
            toolsInput.interactable=!enabled;toolsInput.blocksRaycasts=!enabled;
            tutorialCard.gameObject.SetActive(enabled);tutorialTip.gameObject.SetActive(false);SelectionMode(false,null);
            hint.rectTransform.SetParent(enabled?tutorialCard:safe,false);
            Place(hint.rectTransform,enabled?new Vector2(.5f,.5f):new Vector2(.5f,1),enabled?Vector2.zero:new Vector2(-70,-165));
            hint.rectTransform.sizeDelta=new Vector2(enabled?570:420,80);hint.fontSize=enabled?27:20;
            hint.gameObject.SetActive(enabled);
        }
        public void SelectionMode(bool enabled,Action cancel)
        {
            cancelSelection.gameObject.SetActive(enabled);cancelSelection.onClick.RemoveAllListeners();if(cancel!=null)cancelSelection.onClick.AddListener(()=>cancel());
        }
        public void Launch(string text,Color color,float progressValue){launch.SetActive(true);launch.GetComponent<Image>().color=color;launchTitle.text=text;launchProgress.text=progressValue>0?Mathf.RoundToInt(progressValue*100)+"%":"";}
        public void EndLaunch(){launch.SetActive(false);}
        public void Show(string title,string body,string button,Action action,string other=null,Action secondaryAction=null,Action dismissed=null)
        {
            RestoreLosePresentation();RestoreWinPresentation();RestoreUnlockPresentation();
            if(rewardButton)rewardButton.gameObject.SetActive(false);
            int booster=Array.IndexOf(WoolBoosters.Names,title);bool power=booster>=0;
            modalCard.sizeDelta=new Vector2(590,power?710:490);
            modalCard.GetComponent<Image>().color=WoolArt.Hex("FFF0D3");
            modalTitle.rectTransform.anchoredPosition=new Vector2(0,power?295:146);
            modalBody.rectTransform.anchoredPosition=new Vector2(0,power?-58:40);
            primary.GetComponent<RectTransform>().anchoredPosition=new Vector2(power?-132:0,power?-242:-88);
            secondary.GetComponent<RectTransform>().anchoredPosition=new Vector2(power?132:0,power?-242:-173);
            primary.GetComponent<RectTransform>().sizeDelta=new Vector2(power?246:460,74);
            secondary.GetComponent<RectTransform>().sizeDelta=new Vector2(power?246:460,74);
            primaryText.rectTransform.sizeDelta=secondaryText.rectTransform.sizeDelta=new Vector2(power?232:440,64);
            primary.GetComponent<Image>().color=WoolArt.Hex("70D108");secondary.GetComponent<Image>().color=WoolArt.Hex("38B3F7");
            primaryText.color=secondaryText.color=Color.white;primaryText.fontSize=secondaryText.fontSize=power?23:21;
            primaryText.enableAutoSizing=secondaryText.enableAutoSizing=true;
            primaryText.fontSizeMin=secondaryText.fontSizeMin=16;
            primaryText.fontSizeMax=secondaryText.fontSizeMax=power?23:21;
            modalTitle.enableAutoSizing=true;modalTitle.fontSizeMin=24;modalTitle.fontSizeMax=38;
            modalBody.enableAutoSizing=true;modalBody.fontSizeMin=18;modalBody.fontSizeMax=26;
            boosterPicture.gameObject.SetActive(power);if(power)boosterIcon.sprite=icons[booster];
            close.GetComponent<RectTransform>().anchoredPosition=new Vector2(265,power?321:220);
            modal.SetActive(true);modal.transform.SetAsLastSibling();modalTitle.text=title;modalBody.text=body;primaryText.text=button;
            primary.onClick.RemoveAllListeners();if(action!=null)primary.onClick.AddListener(()=>action());
            primary.interactable=action!=null;secondary.gameObject.SetActive(other!=null);
            secondaryText.text=other;secondary.onClick.RemoveAllListeners();if(secondaryAction!=null)secondary.onClick.AddListener(()=>secondaryAction());
            close.gameObject.SetActive(dismissed!=null);close.onClick.RemoveAllListeners();if(dismissed!=null)close.onClick.AddListener(()=>dismissed());ApplyModalSkin();ApplyUnlockPresentation(title,button,other);FitReleaseUI();
        }
        public void Hide() { RestoreWarehouseTutorial();RestoreWinPresentation();modal.SetActive(false); }
        Image Icon(Transform parent,int index,Vector2 size,Vector2 position)
        {
            var rt=Rect("Icon "+index,parent,new Vector2(.5f,.5f),new Vector2(.5f,.5f),size,position);
            var img=rt.gameObject.AddComponent<Image>();img.sprite=icons[index];img.preserveAspect=true;img.raycastTarget=false;return img;
        }
        RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 size,Vector2 pos)
        {
            var go=new GameObject(name,typeof(RectTransform));var rt=go.GetComponent<RectTransform>();rt.SetParent(parent,false);
            rt.anchorMin=min;rt.anchorMax=max;rt.sizeDelta=size;rt.anchoredPosition=pos;return rt;
        }
        RectTransform Panel(string name,Transform parent,Vector2 anchor,Vector2 size,Vector2 pos,Color color)
        {
            var rt=Rect(name,parent,anchor,anchor,size,pos);var img=rt.gameObject.AddComponent<Image>();img.sprite=rounded;img.type=Image.Type.Sliced;img.color=color;return rt;
        }
        TMP_Text Label(string name,Transform parent,string value,int size,Vector2 bounds,Vector2 pos,Color color)
        {
            var rt=Rect(name,parent,new Vector2(.5f,.5f),new Vector2(.5f,.5f),bounds,pos);
            var text=rt.gameObject.AddComponent<TextMeshProUGUI>();text.font=font;text.text=value;text.fontSize=size;text.color=color;text.alignment=TextAlignmentOptions.Center;
            text.fontStyle=FontStyles.Bold;text.raycastTarget=false;text.textWrappingMode=TextWrappingModes.Normal;return text;
        }
        Button ButtonAt(string name,Transform parent,string label,Vector2 size,Vector2 pos,Action action)
        {
            var rt=Panel(name,parent,new Vector2(.5f,.5f),size,pos,WoolArt.Hex("DCEEE3"));var b=rt.gameObject.AddComponent<Button>();
            var colors=b.colors;colors.highlightedColor=WoolArt.Hex("C4E4D3");colors.pressedColor=WoolArt.Hex("9FD3BC");b.colors=colors;
            Label("Label",rt,label,21,size-Vector2.one*8,Vector2.zero,WoolArt.Hex("2B6255"));if(action!=null)b.onClick.AddListener(()=>action());BindButtonAudio(b);return b;
        }
        void Place(RectTransform rt,Vector2 anchor,Vector2 pos){rt.anchorMin=rt.anchorMax=anchor;rt.anchoredPosition=pos;}
        // Called after deserialization; only listeners are bound at runtime.
        public void InitializeAuthored()
        {
            if(!canvas||!safe)throw new InvalidOperationException("The authored HUD is missing. Run Wool Crush/Author core scene.");
            safe.gameObject.SetActive(true);canvas.enabled=true;if(profilePanel)profilePanel.SetActive(false);
            foreach(var button in GetComponentsInChildren<Button>(true))
            {
                button.onClick.RemoveAllListeners();
                var name=button.name;
                if(name=="Pause")button.onClick.AddListener(()=>PausePressed?.Invoke());
                else if(name=="Play level")button.onClick.AddListener(()=>PlayPressed?.Invoke());
                else if(name=="Settings")button.onClick.AddListener(ShowMenuSettings);
                else if(name=="Profile")button.onClick.AddListener(ShowProfile);
                else if(name=="Shop"||name=="OFFERS"||name=="NO ADS")button.onClick.AddListener(ShowShop);
                else if(name=="Home")button.onClick.AddListener(()=>Navigate(2));
                else if(name=="Cards")button.onClick.AddListener(ShowCards);
                else if(name=="Purchase")button.onClick.AddListener(()=>Show("STORE UNAVAILABLE","Purchases are currently unavailable.","BACK",Hide));
                else if(name=="TASKS"||name.StartsWith("Lv."))button.onClick.AddListener(()=>Show("ADVENTURE","Complete puzzles to continue your adventure.","BACK",Hide));
                else {int kind=Array.IndexOf(WoolBoosters.Names,name);if(kind>=0)button.onClick.AddListener(()=>BoosterPressed?.Invoke(kind));}
            }
            ConfigureInputTargets();BindPolishedUI();BindCampaignUI();BindEngagementUI();BindSpeedControl();BindReferencePages();BindAdPassOffer();BindSpecialOffer();BindLuckySpin();BindProfileView();BindUnlockPresentation();RefreshSafeArea();
        }
        void ShowProfile()
        {
            if(profileView){CloseEngagement();Hide();if(profilePanel)profilePanel.SetActive(false);profileView.Show();return;}
            OpenProfile();
        }
    }
}

