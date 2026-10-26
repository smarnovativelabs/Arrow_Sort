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
        public WoolSpinLamp[] lamps;
        public Sprite availableButton,unavailableButton;Image buttonGraphic;
        bool animating;
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
            info.onClick.RemoveAllListeners();info.onClick.AddListener(()=>resultLabel.text="Daily reset: this device local midnight. Every 3 new levels: +1 spin. Day 30: 24h No Ads +1 spin.");
        }
        void OnEnable(){WoolTrustedClock.Ensure();nextRefresh=0;Fit();Refresh();}
        void OnDisable(){StopAllCoroutines();animating=false;if(spin)spin.transform.localScale=Vector3.one;}
        public void Show(){gameObject.SetActive(true);transform.SetAsLastSibling();Fit();Refresh();}
        public void Fit()
        {
            if(!card||!dimmer)return;
            root=(RectTransform)transform;
            var canvas=GetComponentInParent<Canvas>();if(!canvas)return;canvasRect=(RectTransform)canvas.transform;
            var size=root.rect.size;if(size.x<1||size.y<1)return;
            var safe=Screen.safeArea;lastSafe=safe;
            float top=Screen.height>0?(Screen.height-safe.yMax)/Screen.height*size.y:0,bottom=Screen.height>0?safe.yMin/Screen.height*size.y:0;
            card.anchorMin=card.anchorMax=new Vector2(.5f,.5f);card.anchoredPosition=new Vector2(0,(bottom-top)*.5f);
            card.sizeDelta=new Vector2(1000,1740);card.localScale=Vector3.one*Mathf.Min(size.x*.94f/1000,(size.y-top-bottom-40)/1740);
            dimmer.rectTransform.anchorMin=dimmer.rectTransform.anchorMax=new Vector2(.5f,.5f);
            dimmer.rectTransform.position=canvasRect.TransformPoint(canvasRect.rect.center);
            dimmer.rectTransform.sizeDelta=canvasRect.rect.size;dimmer.rectTransform.localScale=Vector3.one;
            lastSize=size;lastCanvas=canvasRect.rect.size;
        }
        void Update()
        {
            if(!root||Screen.safeArea!=lastSafe||root.rect.size!=lastSize||(canvasRect&&canvasRect.rect.size!=lastCanvas))Fit();
            int phase=(int)Time.unscaledTime%2;
            if(lampPhase!=phase){lampPhase=phase;for(int i=0;i<lamps.Length;i++)if(lamps[i])lamps[i].SetLit(i%2==phase);}
            float pulse=1+.017f*Mathf.Sin(Time.unscaledTime*2.6f);
            if(spin){spin.transform.localScale=Vector3.one*pulse;if(buttonLabel)buttonLabel.transform.localScale=Vector3.one/pulse;}
            if(Time.unscaledTime>=nextRefresh){nextRefresh=Time.unscaledTime+.25f;Refresh();}
        }
        public void Refresh()
        {
            if(!spin)return;
            bool ready=WoolTrustedClock.Ready;int available=WoolProgress.AvailableSpins;
            spin.interactable=ready&&available>0&&!animating;
            if(!buttonGraphic)buttonGraphic=spin.GetComponent<Image>();if(availableButton&&unavailableButton)buttonGraphic.sprite=spin.interactable||animating?availableButton:unavailableButton;
            buttonLabel.text=animating?"SPINNING…":!ready?"VERIFY TIME":available>0?"SPIN  ("+available+")":"NO SPINS YET";
            if(!ready)timerLabel.text="VERIFYING TIME";
            else{var left=WoolProgress.SpinResetIn;timerLabel.text=((int)left.TotalHours).ToString("00")+":"+left.Minutes.ToString("00")+":"+left.Seconds.ToString("00");}
            int streak=WoolProgress.SpinStreak;
            streakLabel.text="Daily spin streak: "+streak+" days";
            if(guaranteeLabel)guaranteeLabel.text="Day 30 guarantees 24h No Ads";
            progressLabel.text=WoolProgress.SpinLevelProgress+" / 3 new levels  ·  Earn 1 free spin";
            if(!ready)resultLabel.text=WoolTrustedClock.Status;
            else if(!wasVerified)resultLabel.text="";
            wasVerified=ready;
        }
        void Spin()
        {
            if(animating)return;
            int slot=WoolProgress.SpinDaily();if(slot<0){Refresh();return;}
            animating=true;Refresh();StartCoroutine(Animate(slot));
        }
        IEnumerator Animate(int slot)
        {
            resultLabel.text="Good luck!";
            var audio=FindFirstObjectByType<WoolGameAudio>();if(audio)audio.Play(WoolGameAudio.Cue.Spin);
            float start=wheel.localEulerAngles.z;
            float travel=1800+Mathf.Repeat(slot*45-start,360);
            for(float t=0;t<3.5f;t+=Time.unscaledDeltaTime)
            {wheel.localRotation=Quaternion.Euler(0,0,start+travel*(1-Mathf.Pow(1-t/3.5f,4)));yield return null;}
            wheel.localRotation=Quaternion.Euler(0,0,slot*45);
            animating=false;walletChanged?.Invoke();
            resultLabel.text=slot==0?"24 hours without forced ads!":slot==4?"You won 1 Unlock Slot power!":slot==5?"Try again on your next spin!":"You won "+WoolProgress.LastSpinReward+" coins!";
            if(audio&&slot!=5)audio.Play(WoolGameAudio.Cue.Coin);Refresh();
        }
    }
}
