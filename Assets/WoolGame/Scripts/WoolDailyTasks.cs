using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    public sealed class WoolDailyTasks : MonoBehaviour
    {
        public RectTransform card,duck,pencil,writingHand,notebook;
        public Image fullBackground;
        public Image duckImage;
        public Sprite[] duckFrames;
        public Image chestImage;
        public Sprite chestClosed,chestOpen;
        public GameObject chestGlow;
        public Sprite[] giftClosed,giftOpen;
        public GameObject[] giftGlows;
        public ScrollRect taskScroll;
        public VerticalLayoutGroup taskLayout;
        public RectTransform taskFrame;
        public RectTransform[] rows;
        public Graphic[] rowArt;
        public Image[] fills,giftArt;
        public TMP_Text[] counts,claimLabels,giftLabels;
        public Button[] claims,gifts;
        public Button close,info,chest;
        public Image dailyFill;
        public RectTransform creditTarget;
        public TMP_Text dailyTotal,chestTotal,timer,message;
        public GameObject preview;
        public RectTransform previewCard;
        public TMP_Text previewAmount;
        public TMP_Text previewState;
        public Button previewCollect,previewClose;
        public Sprite creditSprite,coinSprite;
        public TMP_FontAsset font;
        public Image[] ink;
        Action closeAction,spinAction,playAction,walletChanged,shopAction;
        bool collecting;
        public bool IsCollecting=>collecting;
        float nextRefresh;
        readonly bool[] giftPulseReady=new bool[4];
        int shownCredits,shownChest,previewGift=-1;
        Coroutine writing;
        Vector2 lastSize;
        float taskRowHeight=168;
        public void Bind(Action closePanel,Action openSpin,Action play,Action wallet,Action openShop=null)
        {
            closeAction=closePanel;spinAction=openSpin;playAction=play;walletChanged=wallet;shopAction=openShop;
            close.onClick.RemoveAllListeners();close.onClick.AddListener(()=>closeAction?.Invoke());
            if(info){info.onClick.RemoveAllListeners();}
            for(int i=0;i<rows.Length;i++){int task=i;claims[i].onClick.RemoveAllListeners();claims[i].onClick.AddListener(()=>TaskPressed(task));}
            for(int i=0;i<4;i++){int gift=i;gifts[i].onClick.RemoveAllListeners();gifts[i].onClick.AddListener(()=>Preview(gift));}
            chest.onClick.RemoveAllListeners();chest.onClick.AddListener(()=>Preview(4));
            if(previewClose)previewClose.gameObject.SetActive(false);
            var dismissArt=preview.GetComponent<UnityEngine.UI.Image>();
            if(!dismissArt)dismissArt=preview.AddComponent<UnityEngine.UI.Image>();
            dismissArt.color=Color.clear;dismissArt.raycastTarget=true;
            var dismiss=preview.GetComponent<UnityEngine.UI.Button>();
            if(!dismiss)dismiss=preview.AddComponent<UnityEngine.UI.Button>();
            dismiss.targetGraphic=dismissArt;dismiss.transition=UnityEngine.UI.Selectable.Transition.None;
            dismiss.onClick.RemoveAllListeners();dismiss.onClick.AddListener(()=>preview.SetActive(false));
            previewCollect.onClick.RemoveAllListeners();previewCollect.onClick.AddListener(CollectPreview);
        }
        void OnEnable(){collecting=false;shownCredits=WoolProgress.DailyCredits;shownChest=WoolProgress.DailyChestCredits;nextRefresh=0;if(preview)preview.SetActive(false);if(message)message.text="Saved across daily resets";Fit();Refresh();}
        void OnDisable()
        {
            StopAllCoroutines();writing=null;collecting=false;
            if(taskLayout)taskLayout.enabled=true;
            if(pencil){pencil.gameObject.SetActive(false);pencil.localRotation=Quaternion.Euler(0,0,-28);}
            if(writingHand)writingHand.gameObject.SetActive(false);
            if(duck)duck.localRotation=Quaternion.identity;
            if(duckImage&&duckFrames!=null&&duckFrames.Length>0)duckImage.sprite=duckFrames[0];
            if(ink!=null)foreach(var line in ink)if(line)line.gameObject.SetActive(false);
            if(card)for(int i=card.childCount-1;i>=0;i--)if(card.GetChild(i).name=="Flying credit")Destroy(card.GetChild(i).gameObject);
        }
        public void Fit()
        {
            if(!card)return;var root=(RectTransform)transform;var size=root.rect.size;if(size.x<1||size.y<1)return;
            card.anchorMin=card.anchorMax=new Vector2(.5f,.5f);card.anchoredPosition=Vector2.zero;
            float scale=Mathf.Min((size.x-24)/941f,(size.y-28)/1450f);
            card.localScale=Vector3.one*scale;
            float height=(size.y-28)/scale;card.sizeDelta=new Vector2(941,height);lastSize=size;
            var previewRect=preview?(RectTransform)preview.transform:null;
            if(previewRect){previewRect.anchorMin=Vector2.zero;previewRect.anchorMax=Vector2.one;previewRect.offsetMin=previewRect.offsetMax=Vector2.zero;}
            if(taskScroll)
            {
                float viewportHeight=Mathf.Max(300,height-816);
                var scrollRect=(RectTransform)taskScroll.transform;scrollRect.sizeDelta=new Vector2(941,viewportHeight);scrollRect.anchoredPosition=new Vector2(470.5f,-800-viewportHeight*.5f);
                taskScroll.viewport.anchorMin=Vector2.zero;taskScroll.viewport.anchorMax=Vector2.one;taskScroll.viewport.offsetMin=taskScroll.viewport.offsetMax=Vector2.zero;
                if(taskFrame){taskFrame.sizeDelta=new Vector2(921,height-796);taskFrame.anchoredPosition=new Vector2(470.5f,-786-(height-796)*.5f);}
                taskRowHeight=Mathf.Clamp((viewportHeight-20*(rows.Length-1))/rows.Length,168,220);
                foreach(var row in rows)
                {
                    var element=row.GetComponent<LayoutElement>();float delta=(taskRowHeight-element.preferredHeight)*.5f;element.preferredHeight=taskRowHeight;
                    foreach(RectTransform child in row)
                    {
                        if(child.name=="Task cream"){child.anchorMin=Vector2.zero;child.anchorMax=Vector2.one;child.offsetMin=child.offsetMax=Vector2.zero;}
                        else if(child.name=="Credit award panel"){child.anchorMin=new Vector2(1,0);child.anchorMax=Vector2.one;child.sizeDelta=new Vector2(240,0);child.anchoredPosition=new Vector2(-120,0);}
                        else child.anchoredPosition+=Vector2.down*delta;
                    }
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(taskScroll.content);
            }
            if(fullBackground&&fullBackground.sprite)WoolCanvasArt.Cover(fullBackground.rectTransform,fullBackground.sprite.rect.width/fullBackground.sprite.rect.height);
        }
        void Update()
        {
            if(((RectTransform)transform).rect.size!=lastSize)Fit();
            if(Time.unscaledTime>=nextRefresh){nextRefresh=Time.unscaledTime+.2f;Refresh();}
            for(int i=0;i<4;i++)if(giftArt[i])giftArt[i].transform.localScale=Vector3.one*(giftPulseReady[i]?1+.08f*Mathf.Sin(Time.unscaledTime*5):1);
        }
        public void Refresh()
        {
            if(!dailyTotal)return;
            if(!collecting){shownCredits=WoolProgress.DailyCredits;shownChest=WoolProgress.DailyChestCredits;}
            dailyTotal.text=shownCredits+"/100";dailyFill.fillAmount=shownCredits/100f;
            creditTarget.anchoredPosition=new Vector2(112+shownCredits*7.35f,-706);
            chestTotal.text=Mathf.Min(700,shownChest)+" / 700";
            if(chestImage)chestImage.sprite=WoolProgress.DailyChestCollected?chestOpen:chestClosed;
            if(chestGlow)chestGlow.SetActive(WoolProgress.DailyChestCollected);
            chest.interactable=!collecting;
            var left=WoolProgress.DailyResetIn;timer.text=left.Hours.ToString("00")+":"+left.Minutes.ToString("00")+":"+left.Seconds.ToString("00");
            int slot=0;
            for(int pass=0;pass<3;pass++)for(int i=0;i<rows.Length;i++)
            {
                bool done=WoolProgress.DailyClaimed(i);bool ready=!done&&WoolProgress.DailyCount(i)>=WoolProgress.DailyTargets[i];if((done?2:ready?0:1)!=pass)continue;
                var pos=rows[i].anchoredPosition;float target=-taskRowHeight*.5f-slot*(taskRowHeight+20);
                if(!collecting){if(rows[i].GetSiblingIndex()!=slot)rows[i].SetSiblingIndex(slot);rows[i].anchoredPosition=new Vector2(pos.x,target);}slot++;
                int count=Mathf.Min(WoolProgress.DailyCount(i),WoolProgress.DailyTargets[i]);
                counts[i].text=count+"/"+WoolProgress.DailyTargets[i];fills[i].fillAmount=count/(float)WoolProgress.DailyTargets[i];
                rowArt[i].color=done?new Color(.66f,.66f,.72f):Color.white;
                fills[i].color=done?new Color(.24f,.45f,.28f):new Color(.15f,1,.03f);
                claimLabels[i].text=done?"Collected":count>=WoolProgress.DailyTargets[i]?"Collect":"Go";
                claims[i].interactable=!done&&!collecting;
            }
            for(int i=0;i<4;i++)
            {
                bool done=WoolProgress.DailyGiftClaimed(i),ready=shownCredits>=(i+1)*25;
                gifts[i].interactable=!collecting;
                if(giftClosed!=null&&giftClosed.Length==4)giftArt[i].sprite=done?giftOpen[i]:giftClosed[i];
                if(giftGlows!=null&&giftGlows.Length==4)giftGlows[i].SetActive(done);
                giftArt[i].color=ready||done?Color.white:new Color(.88f,.9f,.97f);
                giftLabels[i].text=((i+1)*25).ToString();
                giftPulseReady[i]=ready&&!done;
            }
        }
        void TaskPressed(int task)
        {
            if(collecting)return;
            preview.SetActive(false);
            if(WoolProgress.DailyCount(task)>=WoolProgress.DailyTargets[task])
            {
                int before=WoolProgress.DailyCredits,weekly=WoolProgress.DailyChestCredits;
                if(WoolProgress.ClaimDailyTask(task)){collecting=true;StartCoroutine(CollectTask(task,before,weekly));}return;
            }
            if(task==1&&shopAction!=null){shopAction();return;}
            playAction?.Invoke();
        }
        void Preview(int gift)
        {
            if(collecting)return;
            if(preview.activeSelf&&previewGift==gift){preview.SetActive(false);return;}
            previewGift=gift;previewAmount.text="x"+(gift==4?500:WoolDailyTaskState.GiftCoins[gift]);
            bool done=gift<4?WoolProgress.DailyGiftClaimed(gift):WoolProgress.DailyChestCollected;
            bool ready=gift==4?WoolProgress.DailyChestCredits>=700:WoolProgress.DailyCredits>=(gift+1)*25&&!done;
            previewCollect.interactable=ready;
            if(previewState)previewState.text=done?"Collected":ready?"Tap to collect":gift==4?"At 700 credits":"At "+((gift+1)*25)+" credits";
            float x=gift==4?651:296+gift*184;
            previewCard.anchoredPosition=new Vector2(Mathf.Clamp(x,100,841),gift==4?-130:-495);
            preview.SetActive(true);preview.transform.SetAsLastSibling();
        }
        void CollectPreview()
        {
            if(!previewCollect.interactable)return;
            int gift=previewGift;preview.SetActive(false);
            bool success=gift==4?WoolProgress.ClaimDailyChest():WoolProgress.ClaimDailyGift(gift);if(!success)return;
            walletChanged?.Invoke();PlayCoin();message.text="Collected "+(gift==4?500:WoolDailyTaskState.GiftCoins[gift])+" coins!";
            Write();Refresh();
        }
        IEnumerator CollectTask(int task,int before,int weekly)
        {
            shownCredits=before;shownChest=weekly;Refresh();
            Write();
            var start=card.InverseTransformPoint(claims[task].transform.position);
            int amount=WoolProgress.DailyCredits-before;
            for(int i=0;i<5;i++)StartCoroutine(Fly(start,i*.065f));
            yield return new WaitForSecondsRealtime(.84f);
            for(float t=0;t<.35f;t+=Time.unscaledDeltaTime)
            {
                float p=Mathf.SmoothStep(0,1,t/.35f);shownCredits=Mathf.RoundToInt(Mathf.Lerp(before,before+amount,p));shownChest=Mathf.RoundToInt(Mathf.Lerp(weekly,weekly+amount,p));Refresh();yield return null;
            }
            shownCredits=before+amount;shownChest=weekly+amount;PlayCoin();
            var from=new Vector2[rows.Length];var to=new Vector2[rows.Length];for(int i=0;i<rows.Length;i++)from[i]=rows[i].anchoredPosition;
            if(taskLayout)taskLayout.enabled=false;
            int slot=0;for(int pass=0;pass<3;pass++)for(int i=0;i<rows.Length;i++)if(WoolProgress.DailyClaimed(i)==(pass==1))to[i]=new Vector2(from[i].x,-taskRowHeight*.5f-slot++*(taskRowHeight+20));
            for(float t=0;t<.32f;t+=Time.unscaledDeltaTime){for(int i=0;i<rows.Length;i++)rows[i].anchoredPosition=Vector2.Lerp(from[i],to[i],Mathf.SmoothStep(0,1,t/.32f));yield return null;}
            collecting=false;if(taskLayout)taskLayout.enabled=true;Refresh();
        }
        IEnumerator Fly(Vector3 start,float delay)
        {
            // RectTransform positions here are in the card's centred local coordinates.
            var end=new Vector3(-358.5f+WoolProgress.DailyCredits*7.35f,card.rect.height*.5f-706,0);
            yield return new WaitForSecondsRealtime(delay);
            var go=new GameObject("Flying credit",typeof(RectTransform),typeof(Image));var rt=(RectTransform)go.transform;rt.SetParent(card,false);
            rt.sizeDelta=new Vector2(53,60);var im=go.GetComponent<Image>();im.sprite=creditSprite;im.preserveAspect=true;im.raycastTarget=false;
            var middle=(start+end)*.5f+new Vector3(-100,160,0);
            for(float t=0;t<.5f;t+=Time.unscaledDeltaTime){float p=t/.5f;rt.localPosition=(1-p)*(1-p)*start+2*(1-p)*p*middle+p*p*end;rt.localScale=Vector3.one*(1-.45f*p);yield return null;}
            Destroy(go);creditTarget.localScale=Vector3.one*1.18f;yield return new WaitForSecondsRealtime(.09f);creditTarget.localScale=Vector3.one;
        }
        void Write(){if(writing!=null)StopCoroutine(writing);writing=StartCoroutine(WriteInNotebook());}
        IEnumerator WriteInNotebook()
        {
            if(duckImage&&duckFrames!=null&&duckFrames.Length>=4)
            {
                for(float t=0;t<1.4f;t+=Time.unscaledDeltaTime){duckImage.sprite=duckFrames[1+(int)(t*7)%3];yield return null;}
                duckImage.sprite=duckFrames[0];writing=null;yield break;
            }
            if(!pencil)yield break;
            pencil.gameObject.SetActive(true);if(writingHand)writingHand.gameObject.SetActive(true);
            Vector2 home=new Vector2(340.5f,-381.5f);Vector2 handHome=new Vector2(362,-397.5f);
            for(float t=0;t<1.3f;t+=Time.unscaledDeltaTime)
            {
                pencil.anchoredPosition=home+new Vector2(Mathf.Sin(t*31)*9,-t*8+Mathf.Sin(t*23)*3);
                pencil.localRotation=Quaternion.Euler(0,0,-28+Mathf.Sin(t*31)*9);
                if(writingHand)writingHand.anchoredPosition=handHome+new Vector2(Mathf.Sin(t*31)*4,Mathf.Sin(t*23)*2);
                if(duck)duck.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*10)*.65f);
                if(ink!=null)for(int i=0;i<ink.Length;i++)ink[i].gameObject.SetActive(t>(i+1)*.2f);
                yield return null;
            }
            pencil.anchoredPosition=home;pencil.gameObject.SetActive(false);if(writingHand){writingHand.anchoredPosition=handHome;writingHand.gameObject.SetActive(false);}if(duck)duck.localRotation=Quaternion.identity;
            if(ink!=null)foreach(var line in ink)line.gameObject.SetActive(false);
            writing=null;
        }
        void PlayCoin(){var audio=FindFirstObjectByType<WoolGameAudio>();if(audio)audio.Play(WoolGameAudio.Cue.Coin);}
    }
}


