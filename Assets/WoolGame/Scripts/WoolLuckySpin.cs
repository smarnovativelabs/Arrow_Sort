using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    public sealed class WoolLuckySpin : MonoBehaviour
    {
        public RectTransform card,wheel;
        public Image dimmer;
        public Button spin,close,info;
        public TMP_Text buttonLabel,timerLabel,streakLabel,progressLabel,resultLabel;
        public TMP_Text guaranteeLabel;
        public Image milestoneFill;
        public TMP_Text milestoneCounter;
        public WoolSpinLamp[] lamps;
        public Sprite referenceBackdrop,referenceWheelFace,referenceHub,referencePointer;
        public Sprite referenceDuck,referenceCoins,referenceCalendar,referenceClock,referenceGift;
        public Sprite availableButton,unavailableButton;Image buttonGraphic;
        RectTransform disabledFace,artProgressFill;
        bool animating;
        int pendingSlot=-1;
        public bool IsAnimating=>animating;
        float nextRefresh;
        int lampPhase=-1;
        bool wasVerified;
        Action walletChanged;
        RectTransform root,canvasRect;
        Vector2 lastSize,lastCanvas;Rect lastSafe;
        public void Bind(Action onWalletChanged)
        {
            walletChanged=onWalletChanged;
            spin.onClick.RemoveAllListeners();spin.onClick.AddListener(Spin);
            close.onClick.RemoveAllListeners();close.onClick.AddListener(()=>gameObject.SetActive(false));
            info.onClick.RemoveAllListeners();info.onClick.AddListener(()=>resultLabel.text="Daily spins count once a day. Miss a day? Restart.\nBonus spins do not count toward 30 days.");
        }
        void OnEnable(){WoolTrustedClock.Ensure();nextRefresh=0;lampPhase=-1;Fit();Refresh();}
        void OnDisable(){StopAllCoroutines();if(pendingSlot>=0){wheel.localRotation=Quaternion.Euler(0,0,PrizeAngle(pendingSlot));walletChanged?.Invoke();pendingSlot=-1;}animating=false;if(close)close.interactable=true;if(spin)spin.transform.localScale=Vector3.one;}
        public void Show(){if(!WoolProgress.LuckySpinUnlocked)return;gameObject.SetActive(true);transform.SetAsLastSibling();Fit();Refresh();}
        public void Fit()
        {
            if(!card||!dimmer)return;
            root=(RectTransform)transform;
            var canvas=GetComponentInParent<Canvas>();if(!canvas)return;canvasRect=(RectTransform)canvas.transform;
            var size=root.rect.size;if(size.x<1||size.y<1)return;
            var safe=WoolHud.CurrentSafeArea;lastSafe=safe;
            float top=Screen.height>0?(Screen.height-safe.yMax)/Screen.height*size.y:0,bottom=Screen.height>0?safe.yMin/Screen.height*size.y:0;
            card.anchorMin=card.anchorMax=new Vector2(.5f,.5f);card.anchoredPosition=new Vector2(0,(bottom-top)*.5f);
            card.sizeDelta=referenceBackdrop?new Vector2(1024,1536):new Vector2(1000,1540);if(referenceBackdrop)LayoutArtwork();else LayoutReference();card.localScale=Vector3.one*Mathf.Min(size.x*.97f/card.sizeDelta.x,(size.y-top-bottom-16)/card.sizeDelta.y);
            dimmer.color=Color.black;dimmer.raycastTarget=true;
            dimmer.rectTransform.anchorMin=dimmer.rectTransform.anchorMax=new Vector2(.5f,.5f);
            dimmer.rectTransform.position=canvasRect.TransformPoint(canvasRect.rect.center);
            dimmer.rectTransform.sizeDelta=canvasRect.rect.size;dimmer.rectTransform.localScale=Vector3.one;
            lastSize=size;lastCanvas=canvasRect.rect.size;
        }
        void Place(Transform target,float x,float y,float w,float h)
        {
            if(!target)return;var rt=target as RectTransform;if(!rt)return;
            rt.anchorMin=rt.anchorMax=new Vector2(.5f,1);rt.pivot=new Vector2(.5f,.5f);
            rt.anchoredPosition=new Vector2(x-card.sizeDelta.x*.5f,-y);rt.sizeDelta=new Vector2(w,h);
        }
        float PrizeAngle(int slot)=>slot*45f;
        RectTransform Surface(string name,WoolSpinSurface.Kind kind,float x,float y,float w,float h)
        {
            var rt=card.Find(name) as RectTransform;
            if(!rt){rt=new GameObject(name,typeof(RectTransform),typeof(WoolSpinSurface)).GetComponent<RectTransform>();rt.SetParent(card,false);}
            var g=rt.GetComponent<WoolSpinSurface>();g.kind=kind;g.raycastTarget=false;Place(rt,x,y,w,h);return rt;
        }
        void ReferenceIcon(string name,Sprite sprite,float x,float y,float w,float h)
        {
            var rt=card.Find(name) as RectTransform;if(!rt){rt=new GameObject(name,typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();rt.SetParent(card,false);}
            var image=rt.GetComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;image.enabled=sprite;Place(rt,x,y,w,h);rt.SetAsLastSibling();
        }
        void ArtworkText(TMP_Text label,float x,float y,float w,float h,float size,Color color)
        {
            if(!label)return;label.transform.SetParent(card,false);label.gameObject.SetActive(true);Place(label.transform,x,y,w,h);
            label.enableAutoSizing=true;label.fontSizeMin=size*.7f;label.fontSizeMax=size;label.fontSize=size;label.color=color;
            label.alignment=TextAlignmentOptions.Center;label.textWrappingMode=TextWrappingModes.NoWrap;label.raycastTarget=false;
        }
        void LayoutArtwork()
        {
            foreach(Transform child in card)child.gameObject.SetActive(false);
            var surface=card.GetComponent<WoolSpinSurface>();if(surface)surface.enabled=false;
            var backdrop=card.Find("Supplied artwork") as RectTransform;
            if(!backdrop){backdrop=new GameObject("Supplied artwork",typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();backdrop.SetParent(card,false);}
            backdrop.gameObject.SetActive(true);backdrop.SetAsFirstSibling();Place(backdrop,512,768,1024,1536);
            var picture=backdrop.GetComponent<Image>();picture.sprite=referenceBackdrop;picture.useSpriteMesh=true;picture.raycastTarget=false;
            var clip=Surface("Circular wheel clip",WoolSpinSurface.Kind.Hub,512,520,600,600);clip.gameObject.SetActive(true);
            var circleMask=clip.GetComponent<Mask>();if(!circleMask)circleMask=clip.gameObject.AddComponent<Mask>();circleMask.showMaskGraphic=false;
            if(wheel.parent!=clip)wheel.SetParent(clip,false);
            wheel.gameObject.SetActive(true);foreach(Transform child in wheel)child.gameObject.SetActive(false);
            var diskGraphic=wheel.GetComponent<WoolSpinSurface>();if(diskGraphic)diskGraphic.enabled=false;
            var diskRect=wheel.Find("Reference spinning face") as RectTransform;
            if(!diskRect){diskRect=new GameObject("Reference spinning face",typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();diskRect.SetParent(wheel,false);}
            diskRect.gameObject.SetActive(true);diskRect.anchorMin=Vector2.zero;diskRect.anchorMax=Vector2.one;diskRect.offsetMin=diskRect.offsetMax=Vector2.zero;
            var disk=diskRect.GetComponent<Image>();disk.enabled=true;disk.sprite=referenceWheelFace;disk.useSpriteMesh=true;disk.raycastTarget=false;
            wheel.anchorMin=wheel.anchorMax=wheel.pivot=Vector2.one*.5f;wheel.anchoredPosition=Vector2.zero;wheel.sizeDelta=new Vector2(600,600);wheel.localScale=Vector3.one;
            ReferenceIcon("Exact duck hub",referenceHub,512,523,162,162);
            close.gameObject.SetActive(true);Place(close.transform,934,118,118,118);foreach(var g in close.GetComponentsInChildren<Graphic>(true))g.enabled=false;
            var closeStyle=close.GetComponent<WoolGlossyButton>();if(closeStyle)closeStyle.enabled=false;
            var hit=close.GetComponent<Image>();hit.enabled=true;hit.color=Color.clear;
            spin.gameObject.SetActive(true);Place(spin.transform,512,1275,720,140);foreach(Transform child in spin.transform)child.gameObject.SetActive(false);
            var style=spin.GetComponent<WoolGlossyButton>();if(style)style.enabled=false;var spinImage=spin.GetComponent<Image>();spinImage.enabled=true;spinImage.color=Color.white;spin.targetGraphic=spinImage;
            var hudOwner=GetComponentInParent<WoolHud>();var home=hudOwner?hudOwner.GetComponentInChildren<WoolHomePresentation>(true):null;
            var homeFace=home&&home.levelButton?home.levelButton.GetComponent<Image>():null;
            if(homeFace){spinImage.sprite=homeFace.sprite;spinImage.type=homeFace.type;availableButton=homeFace.sprite;unavailableButton=homeFace.sprite;}
            var homeStyle=home&&home.levelButton?home.levelButton.GetComponent<WoolGlossyButton>():null;
            if(homeStyle){if(!style)style=spin.gameObject.AddComponent<WoolGlossyButton>();style.tone=homeStyle.tone;style.enabled=true;}
            if(!disabledFace){disabledFace=spin.transform.Find("Silver unavailable face") as RectTransform;if(!disabledFace){disabledFace=new GameObject("Silver unavailable face",typeof(RectTransform),typeof(WoolSpinSurface)).GetComponent<RectTransform>();disabledFace.SetParent(spin.transform,false);}}
            disabledFace.anchorMin=Vector2.zero;disabledFace.anchorMax=Vector2.one;disabledFace.offsetMin=disabledFace.offsetMax=Vector2.zero;
            var silver=disabledFace.GetComponent<WoolSpinSurface>();silver.kind=WoolSpinSurface.Kind.DisabledButton;silver.raycastTarget=false;
            ArtworkText(buttonLabel,512,1275,640,82,54,Color.white);
            buttonLabel.fontSharedMaterial=buttonLabel.font.material;buttonLabel.fontStyle=FontStyles.Normal;buttonLabel.characterSpacing=0;buttonLabel.fontWeight=FontWeight.Regular;
            buttonLabel.outlineWidth=.10f;buttonLabel.outlineColor=new Color32(70,74,67,255);
            ArtworkText(streakLabel,560,913,630,56,39,WoolArt.Hex("68320C"));
            ArtworkText(timerLabel,759,1127,280,70,57,WoolArt.Hex("68320C"));
            ArtworkText(progressLabel,574,1409,620,60,35,WoolArt.Hex("68320C"));
            ArtworkText(milestoneCounter,525,1013,600,46,31,Color.white);
            if(milestoneFill)milestoneFill.gameObject.SetActive(false);
            artProgressFill=Surface("Exact progress fill",WoolSpinSurface.Kind.ProgressFill,525,1013,620,38);artProgressFill.gameObject.SetActive(true);
            if(milestoneCounter)milestoneCounter.transform.SetAsLastSibling();
            ArtworkText(resultLabel,512,1480,830,40,26,WoolArt.Hex("68320C"));
            if(lamps.Length!=16){var bulbs=new WoolSpinLamp[16];for(int i=0;i<16;i++){if(i<lamps.Length)bulbs[i]=lamps[i];else{var go=new GameObject("Reference bulb "+i,typeof(RectTransform),typeof(WoolSpinLamp));go.transform.SetParent(card,false);bulbs[i]=go.GetComponent<WoolSpinLamp>();}}lamps=bulbs;}
            for(int i=0;i<lamps.Length;i++)if(lamps[i]){lamps[i].gameObject.SetActive(i!=0);float a=i*22.5f*Mathf.Deg2Rad;Place(lamps[i].transform,512+Mathf.Sin(a)*323,520-Mathf.Cos(a)*323,29,29);lamps[i].transform.SetAsLastSibling();lamps[i].raycastTarget=false;}
            var oldPointer=card.Find("Exact fixed pointer");if(oldPointer)oldPointer.gameObject.SetActive(false);
            var pointer=Surface("Complete fixed pointer",WoolSpinSurface.Kind.Pointer,512,216,90,90);pointer.gameObject.SetActive(true);pointer.SetAsLastSibling();
            close.transform.SetAsLastSibling();
        }
        void LayoutReference()
        {
            var panel=card.GetComponent<WoolSpinSurface>();if(!panel)panel=card.gameObject.AddComponent<WoolSpinSurface>();
            panel.kind=WoolSpinSurface.Kind.ReferencePanel;panel.raycastTarget=false;
            var rays=Surface("Reference sunburst",WoolSpinSurface.Kind.Sunburst,500,505,950,1000);rays.SetAsFirstSibling();
            if(!card.GetComponent<RectMask2D>())card.gameObject.AddComponent<RectMask2D>();
            Place(card.Find("Orange header"),500,72,620,140);
            Place(card.Find("Title"),500,72,600,110);
            Place(close.transform,930,100,112,112);if(info)info.gameObject.SetActive(false);
            Place(card.Find("Golden wheel rim"),500,505,724,724);
            Place(wheel,500,505,642,642);
            // Scale the authored reward composition together with its disk.
            wheel.sizeDelta=new Vector2(810,810);wheel.localScale=Vector3.one*(642f/810);
            Place(card.Find("Fixed pointer"),500,185,90,100);
            for(int i=0;i<lamps.Length;i++)if(lamps[i]){
                float a=(15+i*30)*Mathf.Deg2Rad;
                Place(lamps[i].transform,500+Mathf.Sin(a)*344,505-Mathf.Cos(a)*344,34,34);lamps[i].SetLit(i%2==((int)Time.unscaledTime%2));
            }
            for(int slot=0;slot<8;slot++){
                var text=wheel.Find("Prize label "+slot);var coins=wheel.Find("Duck coins "+slot);
                if(text)text.gameObject.SetActive(true);if(coins)coins.gameObject.SetActive(WoolSpinState.Coins[slot]>0);
                float a=PrizeAngle(slot)*Mathf.Deg2Rad;
                float tx=405+Mathf.Sin(a)*250,ty=405-Mathf.Cos(a)*250-(WoolSpinState.Coins[slot]>0?38:0);
                if(slot==0){tx=405;ty=130;}
                if(text){Place(text,tx,ty,210,92); // Wheel has an 810-unit local coordinate system.
                    var rt=(RectTransform)text;rt.anchoredPosition=new Vector2(tx-405,-ty);
                    var label=text.GetComponent<TMP_Text>();label.text=slot==0?"DAY 30\nONLY":WoolSpinState.Coins[slot]>0?WoolSpinState.Coins[slot].ToString():"TRY\nAGAIN";
                    label.enableAutoSizing=false;label.fontSize=slot==0?40:48;label.textWrappingMode=TextWrappingModes.NoWrap;label.overflowMode=TextOverflowModes.Overflow;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;label.fontSharedMaterial=buttonLabel.font.material;label.outlineWidth=0;label.color=WoolSpinState.Coins[slot]>0||slot==0?Color.white:WoolArt.Hex("68320C");
                }
                if(coins){float cx=405+Mathf.Sin(a)*250,cy=405-Mathf.Cos(a)*250+34;Place(coins,cx,cy,130,88);var coinImage=coins.GetComponent<Image>();if(referenceCoins&&coinImage){coinImage.sprite=referenceCoins;coinImage.preserveAspect=true;}((RectTransform)coins).anchoredPosition=new Vector2(cx-405,-cy);}
            }
            var ads=wheel.Find("No Ads reward");if(ads){Place(ads,405,230,100,100);((RectTransform)ads).anchoredPosition=new Vector2(0,-230);}
            var hub=wheel.Find("Original flower hub");if(hub)hub.gameObject.SetActive(false);
            var hubRing=Surface("Duck hub rim",WoolSpinSurface.Kind.Hub,500,505,160,160);
            var hubDuck=card.Find("Duck hub portrait") as RectTransform;
            if(!hubDuck){hubDuck=new GameObject("Duck hub portrait",typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();hubDuck.SetParent(card,false);}
            foreach(string oldName in new[]{"Daily rule","Daily bonus subtitle","Wheel golden aura"}){var old=card.Find(oldName);if(old)old.gameObject.SetActive(false);}
            foreach(string oldName in new[]{"Unlock Slot reward","Duck coins 2","Duck coins 4","Duck coins 6"}){var old=wheel.Find(oldName);if(old)old.gameObject.SetActive(false);}
            var homeHud=GetComponentInParent<WoolHud>();var map=homeHud?homeHud.GetComponentInChildren<WoolSkywayMap>(true):null;
            var portrait=hubDuck.GetComponent<Image>();if(referenceDuck)portrait.sprite=referenceDuck;else if(map&&map.happyFrames!=null&&map.happyFrames.Length>0)portrait.sprite=map.happyFrames[0];portrait.preserveAspect=true;portrait.raycastTarget=false;Place(hubDuck,500,505,145,145);
            Place(card.Find("Streak card"),500,965,870,185);
            ReferenceIcon("Calendar icon",referenceCalendar,170,940,130,130);ReferenceIcon("Clock icon",referenceClock,165,1130,110,110);ReferenceIcon("Gift icon",referenceGift,160,1420,110,110);
            Place(streakLabel.transform,570,915,620,52);streakLabel.fontSize=38;
            if(guaranteeLabel){Place(guaranteeLabel.transform,570,961,620,42);guaranteeLabel.fontSize=29;}
            Place(card.Find("30 day progress track"),530,1015,650,43);

            if(milestoneCounter){var cr=milestoneCounter.rectTransform;cr.anchorMin=Vector2.zero;cr.anchorMax=Vector2.one;cr.offsetMin=cr.offsetMax=Vector2.zero;milestoneCounter.fontSize=28;milestoneCounter.color=Color.white;}
            Place(card.Find("Countdown card"),500,1130,870,120);
            Place(card.Find("Timer title"),420,1112,360,42);
            Place(card.Find("UTC reset"),420,1152,360,36);
            Surface("Countdown gold pill",WoolSpinSurface.Kind.Banner,745,1130,340,80).SetSiblingIndex(timerLabel.transform.GetSiblingIndex());
            Place(timerLabel.transform,745,1130,340,72);timerLabel.transform.SetAsLastSibling();timerLabel.enableAutoSizing=false;timerLabel.fontSize=48;timerLabel.textWrappingMode=TextWrappingModes.NoWrap;timerLabel.alignment=TextAlignmentOptions.Center;
            Place(spin.transform,500,1285,740,145);
            var hud=GetComponentInParent<WoolHud>();var home=hud?hud.GetComponentInChildren<WoolHomePresentation>(true):null;
            var source=home&&home.levelButton?home.levelButton.GetComponent<Image>():null;
            var face=spin.GetComponent<Image>();if(source&&face){availableButton=source.sprite;face.sprite=source.sprite;face.type=source.type;}
            var faceRect=spin.transform.Find("Glossy spin face") as RectTransform;
            if(!faceRect){faceRect=new GameObject("Glossy spin face",typeof(RectTransform),typeof(WoolSpinSurface)).GetComponent<RectTransform>();faceRect.SetParent(spin.transform,false);}
            faceRect.SetAsFirstSibling();faceRect.anchorMin=Vector2.zero;faceRect.anchorMax=Vector2.one;faceRect.offsetMin=faceRect.offsetMax=Vector2.zero;
            var spinFace=faceRect.GetComponent<WoolSpinSurface>();spinFace.kind=WoolSpinSurface.Kind.SpinButton;spinFace.raycastTarget=true;
            var softStyle=spin.GetComponent<WoolGlossyButton>();if(softStyle)softStyle.enabled=false;if(face)face.enabled=false;spin.targetGraphic=spinFace;
            var buttonRect=buttonLabel.rectTransform;buttonRect.anchorMin=Vector2.zero;buttonRect.anchorMax=Vector2.one;buttonRect.offsetMin=new Vector2(160,10);buttonRect.offsetMax=new Vector2(-30,-10);buttonLabel.enableAutoSizing=true;buttonLabel.fontSizeMin=36;buttonLabel.fontSizeMax=65;buttonLabel.fontSize=65;
            Surface("Free spin pill",WoolSpinSurface.Kind.CreamCard,500,1420,850,90).SetSiblingIndex(progressLabel.transform.GetSiblingIndex());
            progressLabel.transform.SetAsLastSibling();Place(progressLabel.transform,565,1420,680,60);progressLabel.color=WoolArt.Hex("68320C");
            Place(resultLabel.transform,500,1500,850,50);resultLabel.color=WoolArt.Hex("68320C");
        }
        void Update()
        {
            if(!root||WoolHud.CurrentSafeArea!=lastSafe||root.rect.size!=lastSize||(canvasRect&&canvasRect.rect.size!=lastCanvas))Fit();
            int phase=(int)Time.unscaledTime%2;
            if(lampPhase!=phase){lampPhase=phase;for(int i=0;i<lamps.Length;i++)if(lamps[i])lamps[i].SetLit(i%2==phase);}
            float pulse=spin&&spin.interactable&&!animating?1+.025f*Mathf.Sin(Time.unscaledTime*2.4f):1;
            if(spin){spin.transform.localScale=Vector3.one*pulse;if(buttonLabel)buttonLabel.transform.localScale=referenceBackdrop?Vector3.one:Vector3.one/pulse;}
            if(Time.unscaledTime>=nextRefresh){nextRefresh=Time.unscaledTime+.25f;Refresh();}
        }
        public void Refresh()
        {
            if(!spin)return;
            bool ready=WoolTrustedClock.Ready;int available=WoolProgress.AvailableSpins;
            spin.interactable=ready&&available>0&&!animating;
            if(close)close.interactable=!animating;
            if(referenceBackdrop&&disabledFace){disabledFace.gameObject.SetActive(false);var style=spin.GetComponent<WoolGlossyButton>();if(style)style.enabled=true;var face=spin.GetComponent<Image>();if(face)face.enabled=true;}
            if(!buttonGraphic)buttonGraphic=spin.GetComponent<Image>();if(availableButton&&unavailableButton)buttonGraphic.sprite=spin.interactable||animating?availableButton:unavailableButton;
            buttonLabel.text=animating?"SPINNING…":available>0?"SPIN NOW":"NO SPINS LEFT";
            if(!ready)timerLabel.text="VERIFYING TIME";
            else{var left=WoolProgress.SpinResetIn;timerLabel.text=((int)left.TotalHours).ToString("00")+":"+left.Minutes.ToString("00")+":"+left.Seconds.ToString("00");}
            int streak=WoolProgress.SpinStreak;
            streakLabel.text="DAILY STREAK · "+streak+" DAYS";
            if(guaranteeLabel)guaranteeLabel.text="Every 30 daily spins: 24h No Ads";
            int days=WoolProgress.SpinMilestoneProgress;
            if(referenceBackdrop&&artProgressFill){float filled=620*Mathf.Clamp01(days/30f);artProgressFill.gameObject.SetActive(filled>0);Place(artProgressFill,215+filled*.5f,1013,filled,38);}
            else if(milestoneFill)milestoneFill.fillAmount=days/30f;
            if(milestoneCounter)milestoneCounter.text=days+" / 30 DAYS";
            progressLabel.text=WoolProgress.SpinLevelProgress+" / 3 new levels  ·  Earn 1 free spin";
            if(!ready)resultLabel.text=WoolTrustedClock.Status;
            else if(!wasVerified)resultLabel.text="";
            wasVerified=ready;
        }
        void Spin()
        {
            if(animating)return;
            animating=true;Refresh();
            int slot=WoolProgress.SpinDaily();if(slot<0){animating=false;Refresh();return;}
            pendingSlot=slot;
            // SpinDaily commits and saves the payout atomically. Show the same
            // balance immediately, including if this panel is closed externally.
            walletChanged?.Invoke();
            StartCoroutine(Animate(slot));
        }
        IEnumerator Animate(int slot)
        {
            resultLabel.text="Good luck!";
            var audio=FindFirstObjectByType<WoolGameAudio>();if(audio)audio.Play(WoolGameAudio.Cue.Spin);
            float start=wheel.localEulerAngles.z;
            float travel=1800+Mathf.Repeat(PrizeAngle(slot)-start,360);
            for(float t=0;t<3.5f;t+=Time.unscaledDeltaTime)
            {wheel.localRotation=Quaternion.Euler(0,0,start+travel*(1-Mathf.Pow(1-t/3.5f,4)));yield return null;}
            wheel.localRotation=Quaternion.Euler(0,0,PrizeAngle(slot));
            walletChanged?.Invoke();
            resultLabel.text=WoolProgress.LastSpinMilestone?"30 DAYS! 24h No Ads + 1 bonus spin!":WoolProgress.LastSpinReward<=0?"Try again on your next spin!":"You won "+WoolProgress.LastSpinReward+" coins!";
            if(audio&&WoolProgress.LastSpinReward>0)audio.Play(WoolGameAudio.Cue.Coin);
            pendingSlot=-1;animating=false;Refresh();
        }
    }
}
