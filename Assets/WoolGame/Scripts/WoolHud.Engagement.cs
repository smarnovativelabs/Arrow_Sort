using TMPro;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] GameObject dailyPanel,spinPanel;
        [SerializeField] WoolDailyTasks dailyTasksView;
        [SerializeField] Button dailyClose,spinClose,spinButton;
        [SerializeField] Button[] dailyClaims;
        [SerializeField] TMP_Text[] dailyCounts;
        [SerializeField] Image[] dailyFills;
        [SerializeField] TMP_Text dailyClock,spinMessage;
        [SerializeField] RectTransform spinWheel;
        bool wheelAnimating;
        void BindEngagementUI()
        {
            if(dailyTasksView)dailyTasksView.Bind(CloseEngagement,ShowDailySpin,()=>{CloseEngagement();PlayPressed?.Invoke();},RefreshWalletLabels,ShowShop);
            BindNav(homePage,"TASKS",ShowDailyTasks);BindNav(homePage,"Lv.16",ShowDailySpin);BindNav(homePage,"NO ADS",ShowNoAdsStatus);
            if(dailyClose){dailyClose.onClick.RemoveAllListeners();dailyClose.onClick.AddListener(CloseEngagement);}
            if(spinClose){spinClose.onClick.RemoveAllListeners();spinClose.onClick.AddListener(CloseEngagement);}
            if(dailyClaims!=null)for(int i=0;i<dailyClaims.Length;i++){int task=i;dailyClaims[i].onClick.RemoveAllListeners();dailyClaims[i].onClick.AddListener(()=>{if(WoolProgress.ClaimDailyTask(task)){if(gameAudio)gameAudio.Play(WoolGameAudio.Cue.Coin);RefreshWalletLabels();RefreshDaily();}});}
            if(spinButton){spinButton.onClick.RemoveAllListeners();spinButton.onClick.AddListener(()=>{if(wheelAnimating)return;int slot=WoolProgress.SpinDaily();if(slot>=0)StartCoroutine(AnimateSpin(slot));});}
        }
        void OnRectTransformDimensionsChange(){FitAuthoredPanels();}
        void FitAuthoredPanels()
        {
            FitReferencePages();
            var root=transform as RectTransform;if(!root)return;
            float height=root.rect.height-60;
            if(dailyTasksView)dailyTasksView.Fit();else if(dailyPanel)FitPanel(dailyPanel.transform.Find("Daily card"),height,980);
            if(spinPanel)FitPanel(spinPanel.transform.Find("Spin card"),height,920);
            if(profilePanel)FitPanel(profilePanel.transform.Find("Profile card"),height,1020);
        }
        void FitPanel(Transform panel,float available,float height){if(panel)panel.localScale=Vector3.one*Mathf.Clamp(available/height,.35f,1);}
        void ShowNoAdsStatus()
        {
            if(adPassOffer){ShowAdPassOffer();return;}
            if(!WoolProgress.NoAdsActive){ShowShop();return;}
            var left=WoolProgress.NoAdsRemaining;
            Show("NO ADS ACTIVE",((int)left.TotalHours)+"h "+left.Minutes+"m remaining.\nPop-up and banner ads are paused.\nReward videos remain optional.","BACK",Hide);
        }
        void RefreshWalletLabels(){homeCoins.text=shopCoins.text=WoolProgress.Coins.ToString();}
        public void CloseEngagement(){if(utilityPanel)utilityPanel.Hide();if(dailyPanel)dailyPanel.SetActive(false);if(spinPanel)spinPanel.SetActive(false);if(adPassOffer)adPassOffer.Hide();if(specialOffer)specialOffer.Hide();if(luckySpin)luckySpin.gameObject.SetActive(false);if(profileView)profileView.gameObject.SetActive(false);}
        public void ShowDailyTasks()
        {
            CloseEngagement();Hide();if(profilePanel)profilePanel.SetActive(false);if(!dailyPanel)return;
            dailyPanel.SetActive(true);dailyPanel.transform.SetAsLastSibling();FitAuthoredPanels();RefreshDaily();
        }
        void RefreshDaily()
        {
            if(dailyTasksView){dailyTasksView.Refresh();return;}
            var remaining=WoolProgress.DailyResetIn;dailyClock.text="New tasks in "+remaining.Hours.ToString("00")+":"+remaining.Minutes.ToString("00")+"  ·  resets at midnight";
            for(int i=0;i<3;i++)
            {
                int count=Mathf.Min(WoolProgress.DailyCount(i),WoolProgress.DailyTargets[i]);bool claimed=WoolProgress.DailyClaimed(i);
                dailyCounts[i].text=count+" / "+WoolProgress.DailyTargets[i];dailyFills[i].fillAmount=count/(float)WoolProgress.DailyTargets[i];
                dailyClaims[i].interactable=!claimed&&count==WoolProgress.DailyTargets[i];dailyClaims[i].GetComponentInChildren<TMP_Text>().text=claimed?"CLAIMED":count==WoolProgress.DailyTargets[i]?"CLAIM  +"+WoolProgress.DailyRewards[i]:"+"+WoolProgress.DailyRewards[i]+" COINS";
            }
        }
        public void ShowDailySpin()
        {
            WoolProgress.RecordDailySpinOpened();
            if(luckySpin){CloseEngagement();Hide();if(profilePanel)profilePanel.SetActive(false);luckySpin.Show();return;}
            CloseEngagement();Hide();if(profilePanel)profilePanel.SetActive(false);if(!spinPanel)return;
            spinPanel.SetActive(true);spinPanel.transform.SetAsLastSibling();FitAuthoredPanels();RefreshSpin();
        }
        void RefreshSpin()
        {
            bool used=WoolProgress.DailySpinUsed;spinButton.interactable=!used&&!wheelAnimating;
            spinButton.GetComponentInChildren<TMP_Text>().text=used?"COME BACK TOMORROW":"FREE DAILY SPIN";
            int streak=WoolProgress.SpinStreak,until=30-streak%30;
            string result=used?(WoolProgress.LastSpinSlot==7?"24 hours without forced ads!":WoolProgress.LastSpinReward>0?"You won "+WoolProgress.LastSpinReward+" coins!":"Try again tomorrow!"):"Your free spin is ready.";
            spinMessage.text=result+"\nStreak: "+streak+" days  ·  "+until+" to 24h No Ads";
        }
        IEnumerator AnimateSpin(int slot)
        {
            if(gameAudio)gameAudio.Play(WoolGameAudio.Cue.Spin);
            wheelAnimating=true;spinButton.interactable=false;spinMessage.text="Round and round…";
            float start=spinWheel.localEulerAngles.z,target=1440+Mathf.Repeat(slot*45-start,360);
            for(float time=0;time<3;time+=Time.unscaledDeltaTime){spinWheel.localRotation=Quaternion.Euler(0,0,start+target*(1-Mathf.Pow(1-time/3,4)));yield return null;}
            spinWheel.localRotation=Quaternion.Euler(0,0,slot*45);
            wheelAnimating=false;if(gameAudio&&WoolProgress.LastSpinReward>0)gameAudio.Play(WoolGameAudio.Cue.Coin);RefreshWalletLabels();RefreshSpin();
        }
#if UNITY_EDITOR
        void AuthorWheelSegments()
        {
            var image=spinWheel.GetComponent<Image>();if(image)UnityEditor.Undo.DestroyObjectImmediate(image);
            if(!spinWheel.GetComponent<WoolWheelGraphic>())spinWheel.gameObject.AddComponent<WoolWheelGraphic>();
            spinWheel.sizeDelta=new Vector2(380,380);spinWheel.localRotation=Quaternion.identity;
            if(!spinWheel.Find("Prize 0"))
            {
                for(int i=0;i<8;i++)
                {
                    float angle=i*45*Mathf.Deg2Rad;var p=new Vector2(Mathf.Sin(angle)*130,Mathf.Cos(angle)*130);
                    string caption=WoolProgress.WheelCoinReward(i)>0?WoolProgress.WheelCoinReward(i).ToString():i==7?"NO ADS\n24H":"TRY\nAGAIN";
                    var text=Label("Prize "+i,spinWheel,caption,WoolProgress.WheelCoinReward(i)>0?29:19,new Vector2(94,67),p,WoolProgress.WheelCoinReward(i)==0?Color.white:WoolArt.Hex("74401F"));
                    text.rectTransform.localRotation=Quaternion.Euler(0,0,-i*45);
                }
                var center=Icon(spinWheel,9,new Vector2(79,79),Vector2.zero);center.sprite=skin[5];
            }
            for(int i=0;i<8;i++)
            {
                var label=spinWheel.Find("Prize "+i).GetComponent<TMP_Text>();int reward=WoolProgress.WheelCoinReward(i);
                label.text=reward>0?reward.ToString():i==7?"NO ADS\n24H":"TRY\nAGAIN";
                label.fontSize=reward>0?29:19;label.color=reward>0?WoolArt.Hex("74401F"):Color.white;
            }
            spinWheel.GetComponent<WoolWheelGraphic>().SetVerticesDirty();
            var oldPointer=spinWheel.parent.Find("Wheel pointer");if(oldPointer&&oldPointer.GetComponent<TMP_Text>())UnityEditor.Undo.DestroyObjectImmediate(oldPointer.gameObject);
            if(!spinWheel.parent.Find("Wheel pointer"))
            {
                var pointer=Rect("Wheel pointer",spinWheel.parent,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(52,52),new Vector2(0,293));
                if(!pointer.GetComponent<CanvasRenderer>())pointer.gameObject.AddComponent<CanvasRenderer>();
                var shape=pointer.gameObject.AddComponent<WoolWheelGraphic>();shape.pointer=true;shape.raycastTarget=false;
            }
            var rules=spinWheel.parent.Find("Wheel rules");
            if(rules)rules.GetComponent<TMP_Text>().text="4 coin prizes · 3 try again · 1 No Ads\nEvery 30th consecutive spin guarantees No Ads.";
            if(!rules)Label("Wheel rules",spinWheel.parent,"4 coin prizes · 3 try again · 1 No Ads\nEvery 30th consecutive spin guarantees No Ads.",19,new Vector2(540,65),new Vector2(0,-390),WoolArt.Hex("805936"));
        }
        public void AuthorEngagement()
        {
            foreach(string name in new[]{"TASKS","Lv.16","NO ADS"}){var badge=homePage.Find(name);if(badge)badge.gameObject.SetActive(true);}
            if(!dailyPanel)
            {
                var overlay=Rect("Daily tasks",menu.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);dailyPanel=overlay.gameObject;overlay.gameObject.AddComponent<Image>().color=new Color(.06f,.08f,.17f,.78f);
                var panel=Panel("Daily card",overlay,new Vector2(.5f,.5f),new Vector2(650,980),Vector2.zero,Color.white);Paint(panel.GetComponent<Image>(),0);
                Label("Title",panel,"Daily Tasks",48,new Vector2(500,80),new Vector2(0,391),WoolArt.Hex("8B491C"));
                dailyClock=Label("Reset",panel,"",23,new Vector2(570,72),new Vector2(0,307),WoolArt.Hex("916344"));
                dailyClose=MenuButton("Close daily",panel,"X",new Vector2(68,68),new Vector2(273,425),null,Color.white);Paint(dailyClose.GetComponent<Image>(),3);
                dailyClaims=new Button[3];dailyCounts=new TMP_Text[3];dailyFills=new Image[3];
                for(int i=0;i<3;i++)
                {
                    var card=Panel("Task "+i,panel,new Vector2(.5f,.5f),new Vector2(568,193),new Vector2(0,158-i*221),WoolArt.Hex("FFE3A6"));
                    Label("Goal",card,WoolProgress.DailyTitles[i],29,new Vector2(510,55),new Vector2(0,57),WoolArt.Hex("8B491C"));
                    var track=Panel("Task progress",card,new Vector2(.5f,.5f),new Vector2(487,18),new Vector2(0,12),WoolArt.Hex("DBBA7B"));
                    var fill=Panel("Fill",track,new Vector2(.5f,.5f),new Vector2(487,18),Vector2.zero,WoolArt.Hex("8CBD35"));dailyFills[i]=fill.GetComponent<Image>();dailyFills[i].type=Image.Type.Filled;dailyFills[i].fillMethod=Image.FillMethod.Horizontal;dailyFills[i].fillAmount=0;
                    dailyCounts[i]=Label("Count",card,"0 / 3",26,new Vector2(195,49),new Vector2(-140,-50),WoolArt.Hex("8B491C"));
                    dailyClaims[i]=MenuButton("Claim",card,"CLAIM",new Vector2(260,70),new Vector2(118,-49),null,Color.white);Paint(dailyClaims[i].GetComponent<Image>(),1);dailyClaims[i].GetComponent<Image>().type=Image.Type.Simple;dailyClaims[i].GetComponentInChildren<TMP_Text>().fontSize=24;
                }
                dailyPanel.SetActive(false);
            }
            if(!spinPanel)
            {
                var overlay=Rect("Daily spin",menu.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);spinPanel=overlay.gameObject;overlay.gameObject.AddComponent<Image>().color=new Color(.06f,.08f,.17f,.78f);
                var panel=Panel("Spin card",overlay,new Vector2(.5f,.5f),new Vector2(650,920),Vector2.zero,Color.white);Paint(panel.GetComponent<Image>(),0);
                Label("Title",panel,"Lucky Spin",49,new Vector2(500,80),new Vector2(0,356),WoolArt.Hex("8B491C"));
                spinClose=MenuButton("Close spin",panel,"X",new Vector2(68,68),new Vector2(273,397),null,Color.white);Paint(spinClose.GetComponent<Image>(),3);
                spinWheel=Icon(panel,13,new Vector2(355,355),new Vector2(0,97)).rectTransform;
                spinMessage=Label("Spin message",panel,"",27,new Vector2(510,125),new Vector2(0,-149),WoolArt.Hex("8B491C"));
                spinButton=MenuButton("Spin",panel,"FREE DAILY SPIN",new Vector2(493,95),new Vector2(0,-302),null,Color.white);Paint(spinButton.GetComponent<Image>(),1);spinButton.GetComponent<Image>().type=Image.Type.Simple;
                spinPanel.SetActive(false);
            }
            AuthorWheelSegments();
            var spinBadge=homePage.Find("Lv.16");if(spinBadge)spinBadge.GetComponentInChildren<TMP_Text>(true).text="SPIN";
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
