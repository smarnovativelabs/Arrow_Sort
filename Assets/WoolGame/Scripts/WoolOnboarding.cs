using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WoolGame
{
    // One saved journey; rewards and navigation remain owned by their existing screens.
    public sealed class WoolOnboarding : MonoBehaviour
    {
        WoolGameController game;
        WoolHud hud;
        WoolOnboardingStep shown=(WoolOnboardingStep)(-1);
        RectTransform overlay,bubble;
        readonly Image[] blockers=new Image[4],edges=new Image[4];
        readonly Vector3[] corners=new Vector3[4];
        TMP_Text heading,message,arrow;
        Button next;
        TMP_Text nextText;
        RectTransform target;
        RectTransform pointingHand;
        float pollAt,stepAt,mapArrivalAt=-1;
        float lastNameEditAt;
        string observedNameText;
        bool suspended;
        int avatarSelectionAt;
        public WoolOnboardingStep Step=>WoolProgress.OnboardingStep;

        public void Begin(WoolGameController controller,WoolHud owner)
        {
            game=controller;hud=owner;suspended=false;
            if(!overlay)Build();
            shown=(WoolOnboardingStep)(-1);
            if(Step==WoolOnboardingStep.Gameplay)
            {
                if(WoolProgress.HasCompleted(1)){WoolProgress.AdvanceOnboarding(WoolOnboardingStep.Coins);game.ResumeOnboardingReward();}
                else game.LoadLevel(1);
            }
            else if(Step==WoolOnboardingStep.Coins)game.ResumeOnboardingReward();
            else game.OpenHome();
            Present();
        }

        void Update()
        {
            if(!hud||suspended||Step==WoolOnboardingStep.Complete)return;
            if(Time.unscaledTime>=pollAt){pollAt=Time.unscaledTime+.1f;Tick();}

        }
        void LateUpdate(){if(overlay&&overlay.gameObject.activeSelf)Layout();}
        void Tick()
        {
            if(shown!=Step)Present();
            switch(Step)
            {
                case WoolOnboardingStep.Gameplay: break;
                case WoolOnboardingStep.Coins:
                    if(hud.TutorialWin.gameObject.activeInHierarchy&&!hud.TutorialWin.Busy)target=(RectTransform)hud.TutorialWin.collect.transform;
                    break;
                case WoolOnboardingStep.Map:
                    if(!hud.TutorialMap.IsTravelling&&hud.TutorialMap.PresentedLevel>=2)
                    {if(mapArrivalAt<0)mapArrivalAt=Time.unscaledTime;if(Time.unscaledTime-mapArrivalAt>1.2f){target=(RectTransform)hud.TutorialHomeTab.transform;message.text="Next stop: Home!";}}
                    if(hud.TutorialHomeTab.gameObject.activeInHierarchy&&!hud.TutorialMap.gameObject.activeInHierarchy)Advance(WoolOnboardingStep.HomeTasks);
                    break;
                case WoolOnboardingStep.HomeTasks:
                    if(hud.TutorialTasks.gameObject.activeInHierarchy)Advance(WoolOnboardingStep.Login);
                    break;
                case WoolOnboardingStep.Login:
                    if(WoolProgress.DailyClaimed(0)&&!hud.TutorialTasks.IsCollecting&&Time.unscaledTime-stepAt>.7f)Advance(WoolOnboardingStep.TasksMeter);
                    break;
                case WoolOnboardingStep.TasksMeter:
                    if(hud.TutorialTasks.IsCollecting)break;
                    if(WoolProgress.DailyCount(3)>=1&&!WoolProgress.DailyClaimed(3))
                    {
                        message.text="Tap Collect for your rescue reward";
                        target=(RectTransform)hud.TutorialTasks.claims[3].transform;
                    }
                    else if(WoolProgress.DailyCredits>=25&&!WoolProgress.DailyGiftClaimed(0))
                    {
                        message.text="Tap your gift to collect coins";
                        target=(RectTransform)(hud.TutorialTasks.preview.activeSelf?hud.TutorialTasks.previewCollect:hud.TutorialTasks.gifts[0]).transform;
                    }
                    else
                    {
                        target=null;message.text="Play daily. Earn more gifts!";
                        SetNext("TRY A SPIN",()=>Advance(WoolOnboardingStep.Spin));
                    }
                    break;
                case WoolOnboardingStep.Spin:
                    if(WoolProgress.OnboardingSpinTaken&&!hud.TutorialSpin.IsAnimating&&Time.unscaledTime-stepAt>1f)
                    {message.text="Nice! Your reward is saved";target=null;SetNext("MY PROFILE",()=>Advance(WoolOnboardingStep.Profile));}
                    else if(!WoolTrustedClock.Ready)
                    {message.text="Connect later for your free spin";SetNext("CONTINUE",()=>Advance(WoolOnboardingStep.Profile));}
                    else if(WoolProgress.AvailableSpins<=0&&!hud.TutorialSpin.IsAnimating)
                    {target=null;message.text="A new spin tomorrow!";SetNext("MY PROFILE",()=>Advance(WoolOnboardingStep.Profile));}
                    else if(!WoolProgress.OnboardingSpinTaken)
                    {message.text="Tap SPIN for a reward";next.gameObject.SetActive(false);}
                    break;
                case WoolOnboardingStep.Shop:
                    if(!WoolProgress.CanClaimShopDailyGift&&Time.unscaledTime-stepAt>.5f)
                    {target=null;message.text="50 coins! All yours";SetNext("LET'S PLAY",()=>Advance(WoolOnboardingStep.Finished));}
                    break;
                case WoolOnboardingStep.Profile:
                    if(hud.TutorialProfile.editScreen.activeInHierarchy)Advance(WoolOnboardingStep.Name);
                    break;
                case WoolOnboardingStep.Name:
                    var profile=hud.TutorialProfile;
                    if(!profile.editScreen.activeInHierarchy)
                    {
                        profile.nameButton.onClick.Invoke();target=profile.nameInput.GetComponent<RectTransform>();
                    }
                    else
                    {
                        if(observedNameText!=profile.nameInput.text)
                        {observedNameText=profile.nameInput.text;lastNameEditAt=Time.unscaledTime;}
                        bool valid=!string.IsNullOrWhiteSpace(profile.nameInput.text)&&profile.nameInput.text.Trim()!=WoolProgress.UserId;
                        target=profile.nameInput.GetComponent<RectTransform>();
                        message.text=valid?"Your name looks great!":"Type your name";
                        if(valid)SetNext("COMPLETE",()=>{WoolProgress.SetProfile(profile.nameInput.text,WoolProgress.AvatarIndex);profile.nameInput.DeactivateInputField();Advance(WoolOnboardingStep.Avatar);});else next.gameObject.SetActive(false);
                    }
                    break;
                case WoolOnboardingStep.Avatar:
                    var editor=hud.TutorialProfile;
                    if(!editor.editScreen.activeInHierarchy&&editor.AvatarSelectionVersion<=avatarSelectionAt){hud.ShowTutorialProfile();editor.nameButton.onClick.Invoke();}
                    if(editor.AvatarSelectionVersion>avatarSelectionAt)
                    {
                        if(!editor.editScreen.activeInHierarchy){Advance(WoolOnboardingStep.Shop);break;}
                        target=(RectTransform)editor.save.transform;message.text="Tap SAVE PROFILE";
                    }
                    break;
            }
        }
        void Advance(WoolOnboardingStep step){WoolProgress.AdvanceOnboarding(step);Present();}
        void ShowGameplayIntro()
        {
            overlay.gameObject.SetActive(false);game.SetOnboardingPause(false);
        }
        void Present()
        {
            shown=Step;stepAt=Time.unscaledTime;target=null;next.gameObject.SetActive(false);
            overlay.gameObject.SetActive(Step!=WoolOnboardingStep.Gameplay&&Step!=WoolOnboardingStep.Complete);
            switch(Step)
            {
                case WoolOnboardingStep.Gameplay:
                    ShowGameplayIntro();break;
                case WoolOnboardingStep.Coins:
                    heading.text="Your first rescue!";message.text="Tap Continue. Collect your coins!";
                    target=(RectTransform)hud.TutorialWin.collect.transform;break;
                case WoolOnboardingStep.Map:
                    mapArrivalAt=-1;heading.text="Your rescue journey";message.text="Look! Your next rescue is ready";
                    if(!hud.TutorialMap.gameObject.activeInHierarchy)hud.ShowMap();break;
                case WoolOnboardingStep.HomeTasks:
                    heading.text="A gift every day";message.text="Tap Tasks for daily gifts";
                    target=(RectTransform)hud.FindHomeTutorialButton("Tasks").transform;break;
                case WoolOnboardingStep.Login:
                    hud.ShowDailyTasks();hud.TutorialTasks.taskScroll.verticalNormalizedPosition=1;
                    heading.text="Today's gift";message.text="Tap Collect for today's gift";
                    target=(RectTransform)hud.TutorialTasks.claims[0].transform;break;
                case WoolOnboardingStep.TasksMeter:
                    hud.ShowDailyTasks();heading.text="Bonus unlocked!";message.text="Collect rewards. Unlock gifts!";break;
                case WoolOnboardingStep.Spin:
                    hud.ShowDailySpin();heading.text="Try your luck";message.text="Tap SPIN. Win a free reward!";
                    target=(RectTransform)hud.TutorialSpin.spin.transform;break;
                case WoolOnboardingStep.Shop:
                    hud.CloseEngagement();hud.ShowShop();Canvas.ForceUpdateCanvases();hud.TutorialShop.Fit(true);
                    hud.TutorialShop.scroll.verticalNormalizedPosition=0;
                    heading.text="One last daily gift";message.text="Tap CLAIM FREE for 50 coins";
                    target=(RectTransform)hud.TutorialShop.dailyClaim.transform;break;
                case WoolOnboardingStep.Profile:
                    hud.ShowTutorialProfile();heading.text="Your rescue story";message.text="Make it yours. Tap your name";
                    target=(RectTransform)hud.TutorialProfile.nameButton.transform;break;
                case WoolOnboardingStep.Name:
                    hud.ShowTutorialProfile();hud.TutorialProfile.nameButton.onClick.Invoke();
                    heading.text="Choose your name";message.text="Type your name";
                    observedNameText=hud.TutorialProfile.nameInput.text;lastNameEditAt=Time.unscaledTime;
                    target=hud.TutorialProfile.nameInput.GetComponent<RectTransform>();break;
                case WoolOnboardingStep.Avatar:
                    if(!hud.TutorialProfile.editScreen.activeInHierarchy){hud.ShowTutorialProfile();hud.TutorialProfile.nameButton.onClick.Invoke();}
                    avatarSelectionAt=hud.TutorialProfile.AvatarSelectionVersion;
                    heading.text="Pick your duck";message.text="Pick your favourite duck";
                    target=hud.TutorialProfile.choices[0].transform.parent as RectTransform;break;
                case WoolOnboardingStep.Finished:
                    heading.text="You're ready to rescue!";message.text="Your next rescue awaits!";
                    SetNext("LET'S PLAY",()=>{WoolProgress.AdvanceOnboarding(WoolOnboardingStep.Complete);overlay.gameObject.SetActive(false);hud.CloseEngagement();game.OpenHome();enabled=false;});break;
                case WoolOnboardingStep.Complete: enabled=false;break;
            }
            if(overlay.gameObject.activeSelf)Layout();
        }
        void Suspend(){suspended=true;overlay.gameObject.SetActive(false);hud.CloseEngagement();game.OpenHome();}
        void SetNext(string caption,UnityEngine.Events.UnityAction action)
        {
            if(next.gameObject.activeSelf&&nextText.text==caption)return;
            nextText.text=caption;next.onClick.RemoveAllListeners();next.onClick.AddListener(action);next.gameObject.SetActive(true);
        }
        void Build()
        {
            overlay=Rect("First rescue tutorial",hud.transform);overlay.anchorMin=Vector2.zero;overlay.anchorMax=Vector2.one;overlay.offsetMin=overlay.offsetMax=Vector2.zero;
            // A nested canvas keeps the guide above every existing modal without rebuilding the HUD.
            var canvas=overlay.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=300;overlay.gameObject.AddComponent<GraphicRaycaster>();
            for(int i=0;i<4;i++){blockers[i]=Image("Tutorial dimmer "+i,overlay,new Color(.035f,.035f,.08f,.72f),true);edges[i]=Image("Highlight "+i,overlay,new Color(1,.83f,.24f),false);}
            bubble=Rect("Tutorial message",overlay);var panel=bubble.gameObject.AddComponent<Image>();panel.sprite=hud.SkywayPanelSprite;panel.type=UnityEngine.UI.Image.Type.Sliced;panel.color=WoolArt.Hex("FFF1CE");panel.raycastTarget=true;
            var border=bubble.gameObject.AddComponent<Outline>();border.effectColor=WoolArt.Hex("C99332");border.effectDistance=new Vector2(3,-3);
            heading=Text("Step",bubble,30,WoolArt.Hex("59361E"));heading.fontStyle=FontStyles.Bold;message=Text("Instruction",bubble,29,WoolArt.Hex("74513C"));
            var button=Image("Tutorial continue",bubble,new Color(.20f,.58f,.34f),true);button.sprite=hud.SkywayPanelSprite;button.type=UnityEngine.UI.Image.Type.Sliced;next=button.gameObject.AddComponent<Button>();next.targetGraphic=button;
            next.gameObject.AddComponent<WoolButtonAudio>().Initialize(next,FindFirstObjectByType<WoolGameAudio>());
            nextText=Text("Caption",button.transform,27,Color.white);nextText.alignment=TextAlignmentOptions.Center;
            arrow=Text("Tap cue",overlay,25,new Color(1,.9f,.4f));arrow.text="TAP HERE";arrow.alignment=TextAlignmentOptions.Center;
            pointingHand=Rect("Point at the required button",overlay);var hand=pointingHand.gameObject.AddComponent<WoolWarehouseHand>();hand.color=WoolArt.Hex("30DFFF");hand.raycastTarget=false;
        }
        void Layout()
        {
            var safe=WoolHud.CurrentSafeArea;
            // Block the entire display; only the message and hand respect the notch insets.
            overlay.anchorMin=Vector2.zero;overlay.anchorMax=Vector2.one;overlay.offsetMin=overlay.offsetMax=Vector2.zero;
            var r=overlay.rect;float w=r.width,h=r.height;if(w<1||h<1)return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay,safe.min,null,out var safeMin);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay,safe.max,null,out var safeMax);
            safeMin-=r.min;safeMax-=r.min;
            Rect hole=new Rect(w*.5f,h*.5f,0,0);
            bool hasTarget=target&&target.gameObject.activeInHierarchy&&(!next.gameObject.activeSelf||Step==WoolOnboardingStep.Name);
            if(hasTarget)
            {
                target.GetWorldCorners(corners);var a=(Vector2)overlay.InverseTransformPoint(corners[0])-r.min;var b=(Vector2)overlay.InverseTransformPoint(corners[2])-r.min;
                if(Step==WoolOnboardingStep.Avatar&&hud.TutorialProfile.AvatarSelectionVersion<=avatarSelectionAt)
                {
                    a=Vector2.one*float.MaxValue;b=Vector2.one*float.MinValue;
                    foreach(var choice in hud.TutorialProfile.choices)
                    {
                        ((RectTransform)choice.transform).GetWorldCorners(corners);
                        a=Vector2.Min(a,(Vector2)overlay.InverseTransformPoint(corners[0])-r.min);
                        b=Vector2.Max(b,(Vector2)overlay.InverseTransformPoint(corners[2])-r.min);
                    }
                }
                hole=UnityEngine.Rect.MinMaxRect(Mathf.Clamp(a.x-10,0,w),Mathf.Clamp(a.y-10,0,h),Mathf.Clamp(b.x+10,0,w),Mathf.Clamp(b.y+10,0,h));
            }
            Place(blockers[0].rectTransform,0,0,hole.xMin,h);Place(blockers[1].rectTransform,hole.xMax,0,w-hole.xMax,h);
            Place(blockers[2].rectTransform,hole.xMin,0,hole.width,hole.yMin);Place(blockers[3].rectTransform,hole.xMin,hole.yMax,hole.width,h-hole.yMax);
            for(int i=0;i<4;i++)edges[i].gameObject.SetActive(hasTarget);
            if(hasTarget){Place(edges[0].rectTransform,hole.xMin,hole.yMin,hole.width,4);Place(edges[1].rectTransform,hole.xMin,hole.yMax-4,hole.width,4);Place(edges[2].rectTransform,hole.xMin,hole.yMin,4,hole.height);Place(edges[3].rectTransform,hole.xMax-4,hole.yMin,4,hole.height);}
            float scale=Mathf.Min(1.25f,(safeMax.x-safeMin.x)/1000f,(safeMax.y-safeMin.y)/1450f),bw=Mathf.Min(safeMax.x-safeMin.x-32,920*scale),bh=(next.gameObject.activeSelf?300:198)*scale;
            float y=hasTarget&&hole.center.y>h*.55f?24:h-bh-24;
            if(hasTarget&&y<hole.yMax+14&&y+bh>hole.yMin-14)y=hole.yMin>bh+38?hole.yMin-bh-14:Mathf.Min(h-bh-12,hole.yMax+14);
            y=Mathf.Clamp(y,safeMin.y+(next.gameObject.activeSelf?150*scale:16),safeMax.y-bh-16);
            Place(bubble,(safeMin.x+safeMax.x-bw)*.5f,y,bw,bh);
            Place(heading.rectTransform,28*scale,bh-56*scale,bw-56*scale,44*scale);heading.fontSize=36*scale;
            Place(message.rectTransform,28*scale,next.gameObject.activeSelf?106*scale:22*scale,bw-56*scale,112*scale);message.fontSize=52*scale;message.fontStyle=FontStyles.Bold;
            Place((RectTransform)next.transform,24*scale,18*scale,bw-48*scale,70*scale);Stretch(nextText.rectTransform);nextText.fontSize=40*scale;
            arrow.gameObject.SetActive(false);
            bool showHand=(Step!=WoolOnboardingStep.Name||Time.unscaledTime-lastNameEditAt>=3f)&&(hasTarget||next.gameObject.activeSelf);
            pointingHand.gameObject.SetActive(showHand);
            if(showHand){
                bool fromBelow=hole.center.y>h*.4f;
                var tip=new Vector2(hole.center.x,fromBelow?hole.yMin-16:hole.yMax+16);
                if(next.gameObject.activeSelf&&(!hasTarget||Step==WoolOnboardingStep.Name)){((RectTransform)next.transform).GetWorldCorners(corners);tip=(Vector2)overlay.InverseTransformPoint((corners[0]+corners[3])*.5f)-r.min+Vector2.down*16;fromBelow=true;}
                pointingHand.anchorMin=pointingHand.anchorMax=Vector2.zero;pointingHand.pivot=new Vector2(.5f,.5f);pointingHand.sizeDelta=new Vector2(165,185);pointingHand.anchoredPosition=tip;

                // Keep the fingertip at the actual target center when changing approach direction.
                pointingHand.anchoredPosition=tip;pointingHand.localRotation=Quaternion.Euler(0,0,fromBelow?0:180);
                float pulse=.72f*scale*(1+.05f*Mathf.Sin(Time.unscaledTime*4));float flip=fromBelow?(tip.x>w-160*scale?-1:1):(tip.x<160*scale?-1:1);pointingHand.localScale=new Vector3(pulse*flip,pulse,1);
            }
        }
        static RectTransform Rect(string name,Transform parent){var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rt.SetParent(parent,false);return rt;}
        static Image Image(string name,Transform parent,Color color,bool raycast){var rt=Rect(name,parent);var image=rt.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=raycast;return image;}
        static TMP_Text Text(string name,Transform parent,float size,Color color){var rt=Rect(name,parent);var text=rt.gameObject.AddComponent<TextMeshProUGUI>();text.font=TMP_Settings.defaultFontAsset;text.fontSize=size;text.color=color;text.raycastTarget=false;text.textWrappingMode=TextWrappingModes.Normal;text.alignment=TextAlignmentOptions.MidlineLeft;return text;}
        static void Place(RectTransform rt,float x,float y,float w,float h){rt.anchorMin=rt.anchorMax=Vector2.zero;rt.pivot=Vector2.zero;rt.anchoredPosition=new Vector2(x,y);rt.sizeDelta=new Vector2(Mathf.Max(0,w),Mathf.Max(0,h));}
        static void Stretch(RectTransform rt){rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;}
        void OnDisable(){if(overlay)overlay.gameObject.SetActive(false);}
        void OnDestroy(){if(overlay)Destroy(overlay.gameObject);}
    }
}
