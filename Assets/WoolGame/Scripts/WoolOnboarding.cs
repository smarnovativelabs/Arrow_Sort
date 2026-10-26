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
        float pollAt,stepAt,mapArrivalAt=-1;
        bool suspended;
        int gameplayIntro,avatarSelectionAt;
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
            if(overlay&&overlay.gameObject.activeSelf)Layout();
        }
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
                    {if(mapArrivalAt<0)mapArrivalAt=Time.unscaledTime;if(Time.unscaledTime-mapArrivalAt>1.2f){target=(RectTransform)hud.TutorialHomeTab.transform;message.text="Your duck reached level 2! Tap Home to find your daily rewards.";}}
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
                        message.text="Your first rescue completed a task too! Tap its Collect button for 20 more credits.";
                        target=(RectTransform)hud.TutorialTasks.claims[3].transform;
                    }
                    else if(WoolProgress.DailyCredits>=25&&!WoolProgress.DailyGiftClaimed(0))
                    {
                        message.text="You reached the first gift! Tap it, then Collect to put 20 coins in your wallet.";
                        target=(RectTransform)(hud.TutorialTasks.preview.activeSelf?hud.TutorialTasks.previewCollect:hud.TutorialTasks.gifts[0]).transform;
                    }
                    else
                    {
                        target=null;message.text="Task credits fill the 100-credit meter. Gifts at 25, 50, 75 and 100 give 20, 30, 50 and 100 coins. Save 700 credits for the 500-coin chest. Tasks refresh daily.";
                        SetNext("TRY THE DAILY SPIN",()=>Advance(WoolOnboardingStep.Spin));
                    }
                    break;
                case WoolOnboardingStep.Spin:
                    if(WoolProgress.OnboardingSpinTaken&&!hud.TutorialSpin.IsAnimating&&Time.unscaledTime-stepAt>1f)
                    {message.text="Nice spin! Come back daily for another chance, and clear 3 new levels for an extra spin. Let's visit your profile next.";target=null;SetNext("MY PROFILE",()=>Advance(WoolOnboardingStep.Profile));}
                    else if(!WoolTrustedClock.Ready)
                    {message.text="Your free daily spin needs an internet time check. You can return when connected; let's explore your profile meanwhile.";SetNext("CONTINUE THE TOUR",()=>Advance(WoolOnboardingStep.Profile));}
                    else if(WoolProgress.AvailableSpins<=0&&!hud.TutorialSpin.IsAnimating)
                    {target=null;message.text="Today's spin is used. Come back after the countdown, or earn a spin by clearing 3 new levels.";SetNext("MY PROFILE",()=>Advance(WoolOnboardingStep.Profile));}
                    else if(!WoolProgress.OnboardingSpinTaken)
                    {message.text="Tap SPIN and watch the wheel reveal your reward.";next.gameObject.SetActive(false);}
                    break;
                case WoolOnboardingStep.Shop:
                    if(!WoolProgress.CanClaimShopDailyGift&&Time.unscaledTime-stepAt>.5f)
                    {target=null;message.text="Your free 50 coins are in the wallet! Come back every day for this gift. Purchases and reward videos are optional.";SetNext("FINISH MY TOUR",()=>Advance(WoolOnboardingStep.Finished));}
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
                        bool valid=!string.IsNullOrWhiteSpace(profile.nameInput.text)&&profile.nameInput.text.Trim()!=WoolProgress.UserId;
                        target=profile.nameInput.GetComponent<RectTransform>();
                        message.text=valid?"Great name! Next, choose the duck avatar that feels like you.":"Tap the name field and enter your name (up to 15 characters).";
                        if(valid)SetNext("CHOOSE MY AVATAR",()=>{WoolProgress.SetProfile(profile.nameInput.text,WoolProgress.AvatarIndex);profile.nameInput.DeactivateInputField();Advance(WoolOnboardingStep.Avatar);});else next.gameObject.SetActive(false);
                    }
                    break;
                case WoolOnboardingStep.Avatar:
                    var editor=hud.TutorialProfile;
                    if(!editor.editScreen.activeInHierarchy&&editor.AvatarSelectionVersion<=avatarSelectionAt){hud.ShowTutorialProfile();editor.nameButton.onClick.Invoke();}
                    if(editor.AvatarSelectionVersion>avatarSelectionAt)
                    {
                        if(!editor.editScreen.activeInHierarchy){Advance(WoolOnboardingStep.Shop);break;}
                        target=(RectTransform)editor.save.transform;message.text="Looking good! Tap SAVE PROFILE to keep your name and avatar.";
                    }
                    break;
            }
        }
        void Advance(WoolOnboardingStep step){WoolProgress.AdvanceOnboarding(step);Present();}
        void ShowGameplayIntro()
        {
            overlay.gameObject.SetActive(true);game.SetOnboardingPause(true);
            if(gameplayIntro==0)
            {
                heading.text="A little rescue, together";
                message.text="The dragon is carrying colourful wool toward your ducks. Tap a vehicle to load a crossbow. It collects matching wool. Clear the wool to rescue the family!";
                SetNext("HOW DO SLOTS WORK?",()=>{gameplayIntro=1;ShowGameplayIntro();});
            }
            else if(gameplayIntro==1)
            {
                heading.text="Four slots to start";
                message.text="Only the first four slots are open. A loaded crossbow uses one slot until its arrows run out. The + slots are locked: use the Unlock ability or choose a rewarded video to open more.";
                SetNext("SHOW ME THE CONTROLS",()=>{gameplayIntro=2;ShowGameplayIntro();});
            }
            else
            {
                heading.text="Small moves, smart rescues";
                message.text="Arrows show each vehicle's exit direction. The gear opens settings; ×1 changes speed. Bottom powers unlock slots, remove blockers, sort colours or pierce fog. New powers get a hands-on lesson when unlocked.";
                SetNext("LET'S RESCUE THE DUCKS",()=>{gameplayIntro=3;overlay.gameObject.SetActive(false);game.SetOnboardingPause(false);});
            }
            Layout();
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
                    heading.text="Your first rescue!";message.text="Tap Continue to collect your level reward and watch the coins fly to your wallet.";
                    target=(RectTransform)hud.TutorialWin.collect.transform;break;
                case WoolOnboardingStep.Map:
                    mapArrivalAt=-1;heading.text="Your rescue journey";message.text="Each rescue unlocks the next level. Watch your duck walk from level 1 to level 2.";
                    if(!hud.TutorialMap.gameObject.activeInHierarchy)hud.ShowMap();break;
                case WoolOnboardingStep.HomeTasks:
                    heading.text="Welcome home · Daily Tasks";message.text="Tap Tasks. Play, rescue ducks and use powers to complete goals, then collect their rewards.";
                    target=(RectTransform)hud.FindHomeTutorialButton("Tasks").transform;break;
                case WoolOnboardingStep.Login:
                    hud.ShowDailyTasks();hud.TutorialTasks.taskScroll.verticalNormalizedPosition=1;
                    heading.text="Claim your login credits";message.text="Tap Collect on Login daily. Credits fill the gift meter; the gifts award coins.";
                    target=(RectTransform)hud.TutorialTasks.claims[0].transform;break;
                case WoolOnboardingStep.TasksMeter:
                    hud.ShowDailyTasks();heading.text="Credits become gifts";message.text="Collect completed tasks to fill your gift meter.";break;
                case WoolOnboardingStep.Spin:
                    hud.ShowDailySpin();heading.text="A free spin every day";message.text="Tap SPIN to reveal a reward. Daily spins and spins earned through levels are free.";
                    target=(RectTransform)hud.TutorialSpin.spin.transform;break;
                case WoolOnboardingStep.Shop:
                    hud.CloseEngagement();hud.ShowShop();Canvas.ForceUpdateCanvases();hud.TutorialShop.Fit(true);
                    hud.TutorialShop.scroll.verticalNormalizedPosition=0;
                    heading.text="One last daily gift";message.text="The shop has a free daily gift. Tap CLAIM FREE for 50 coins; no purchase or video is required.";
                    target=(RectTransform)hud.TutorialShop.dailyClaim.transform;break;
                case WoolOnboardingStep.Profile:
                    hud.ShowTutorialProfile();heading.text="Your rescue story";message.text="Your profile tracks rescued levels, first-try wins, streaks and collected coins. Tap your name to edit it, then choose an avatar.";
                    target=(RectTransform)hud.TutorialProfile.nameButton.transform;break;
                case WoolOnboardingStep.Name:
                    hud.ShowTutorialProfile();hud.TutorialProfile.nameButton.onClick.Invoke();
                    heading.text="Choose your name";message.text="Enter your name (up to 15 characters), then we'll choose an avatar.";
                    target=hud.TutorialProfile.nameInput.GetComponent<RectTransform>();break;
                case WoolOnboardingStep.Avatar:
                    if(!hud.TutorialProfile.editScreen.activeInHierarchy){hud.ShowTutorialProfile();hud.TutorialProfile.nameButton.onClick.Invoke();}
                    avatarSelectionAt=hud.TutorialProfile.AvatarSelectionVersion;
                    heading.text="Pick your duck";message.text="Tap your favourite avatar. You can change your name or avatar again at any time.";
                    target=hud.TutorialProfile.choices[0].transform.parent as RectTransform;break;
                case WoolOnboardingStep.Finished:
                    heading.text="You're ready to rescue!";message.text="Tap vehicles to match wool, keep an eye on your four open slots, and rescue the ducks. Your daily tasks, spins, profile and free shop gift are always here.";
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
            bubble=Rect("Tutorial message",overlay);var panel=bubble.gameObject.AddComponent<Image>();panel.sprite=hud.SkywayPanelSprite;panel.type=UnityEngine.UI.Image.Type.Sliced;panel.color=new Color(.12f,.16f,.25f,.98f);panel.raycastTarget=true;
            heading=Text("Step",bubble,30,new Color(1,.85f,.37f));message=Text("Instruction",bubble,29,Color.white);
            var button=Image("Tutorial continue",bubble,new Color(.20f,.58f,.34f),true);button.sprite=hud.SkywayPanelSprite;button.type=UnityEngine.UI.Image.Type.Sliced;next=button.gameObject.AddComponent<Button>();next.targetGraphic=button;
            next.gameObject.AddComponent<WoolButtonAudio>().Initialize(next,FindFirstObjectByType<WoolGameAudio>());
            nextText=Text("Caption",button.transform,27,Color.white);nextText.alignment=TextAlignmentOptions.Center;
            arrow=Text("Tap cue",overlay,25,new Color(1,.9f,.4f));arrow.text="TAP HERE";arrow.alignment=TextAlignmentOptions.Center;
        }
        void Layout()
        {
            var safe=WoolHud.CurrentSafeArea;
            if(Screen.width>0&&Screen.height>0){overlay.anchorMin=safe.min/new Vector2(Screen.width,Screen.height);overlay.anchorMax=safe.max/new Vector2(Screen.width,Screen.height);overlay.offsetMin=overlay.offsetMax=Vector2.zero;}
            var r=overlay.rect;float w=r.width,h=r.height;if(w<1||h<1)return;
            // Work in the already-safe HUD rect; never subtract the device notch twice.
            Rect hole=new Rect(w*.5f,h*.5f,0,0);
            bool hasTarget=target&&target.gameObject.activeInHierarchy;
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
            float scale=Mathf.Min(1.25f,w/1000f,h/1450f),bw=Mathf.Min(w-24,880*scale),bh=(next.gameObject.activeSelf?360:280)*scale;
            float y=hasTarget&&hole.center.y>h*.55f?24:h-bh-24;
            if(hasTarget&&y<hole.yMax+14&&y+bh>hole.yMin-14)y=hole.yMin>bh+38?hole.yMin-bh-14:Mathf.Min(h-bh-12,hole.yMax+14);
            Place(bubble,(w-bw)*.5f,y,bw,bh);
            Place(heading.rectTransform,22*scale,bh-58*scale,bw-44*scale,42*scale);heading.fontSize=30*scale;
            Place(message.rectTransform,22*scale,next.gameObject.activeSelf?104*scale:18*scale,bw-44*scale,190*scale);message.fontSize=28*scale;
            Place((RectTransform)next.transform,24*scale,18*scale,bw-48*scale,70*scale);Stretch(nextText.rectTransform);nextText.fontSize=27*scale;
            arrow.gameObject.SetActive(hasTarget&&Step!=WoolOnboardingStep.Name);
            if(hasTarget){Place(arrow.rectTransform,Mathf.Clamp(hole.center.x-95*scale,0,w-190*scale),Mathf.Max(0,hole.yMin-38*scale),190*scale,30*scale);arrow.fontSize=25*scale;}
        }
        static RectTransform Rect(string name,Transform parent){var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rt.SetParent(parent,false);return rt;}
        static Image Image(string name,Transform parent,Color color,bool raycast){var rt=Rect(name,parent);var image=rt.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=raycast;return image;}
        static TMP_Text Text(string name,Transform parent,float size,Color color){var rt=Rect(name,parent);var text=rt.gameObject.AddComponent<TextMeshProUGUI>();text.font=TMP_Settings.defaultFontAsset;text.fontSize=size;text.color=color;text.raycastTarget=false;text.textWrappingMode=TextWrappingModes.Normal;text.alignment=TextAlignmentOptions.MidlineLeft;return text;}
        static void Place(RectTransform rt,float x,float y,float w,float h){rt.anchorMin=rt.anchorMax=Vector2.zero;rt.pivot=Vector2.zero;rt.anchoredPosition=new Vector2(x,y);rt.sizeDelta=new Vector2(Mathf.Max(0,w),Mathf.Max(0,h));}
        static void Stretch(RectTransform rt){rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;}
        void OnDestroy(){if(overlay)Destroy(overlay.gameObject);}
    }
}
