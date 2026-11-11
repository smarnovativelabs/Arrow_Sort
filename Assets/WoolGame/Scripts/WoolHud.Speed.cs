using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] Button gameplaySpeedButton;
        [SerializeField] TMP_Text gameplaySpeedLabel;
        public event Action SpeedPressed;
        void BindSpeedControl()
        {
            if(!gameplaySpeedButton)return;
            StyleGameplaySpeed();
            gameplaySpeedButton.onClick.RemoveAllListeners();
            gameplaySpeedButton.onClick.AddListener(()=>SpeedPressed?.Invoke());
            SetGameplaySpeed(1);
        }
        WoolSpeedButtonFace speedFace;WoolRewardVideoIcon speedVideoIcon;bool speedNeedsAd,speedUnlocked;int shownSpeed=1;
        public void SetSpeedUnlocked(bool value){speedUnlocked=value;SetGameplaySpeed(shownSpeed);}
        public void SetSpeedAdRequired(bool value){speedNeedsAd=value;SetGameplaySpeed(shownSpeed);}
        public void SetGameplaySpeed(int multiplier)
        {
            shownSpeed=multiplier;if(speedFace){speedFace.active=multiplier==2;speedFace.SetVerticesDirty();}if(speedVideoIcon)speedVideoIcon.gameObject.SetActive(speedNeedsAd&&!speedUnlocked);if(gameplaySpeedLabel){gameplaySpeedLabel.rectTransform.offsetMin=new Vector2(speedNeedsAd&&!speedUnlocked?76:8,5);gameplaySpeedLabel.rectTransform.offsetMax=new Vector2(-8,-5);}if(gameplaySpeedLabel){gameplaySpeedLabel.text="2×";gameplaySpeedLabel.alignment=TextAlignmentOptions.Center;}if(gameplaySpeedButton){var face=gameplaySpeedButton.GetComponent<Image>();if(face)face.color=multiplier==2?new Color(.68f,.55f,.34f):Color.white;}
        }
        TMP_Text speedTrialTimer;int shownTrialSeconds=-1;
        public void SetSpeedTrialCountdown(float remaining,bool visible){
            if(!gameplaySpeedButton)return;
            if(!speedTrialTimer){
                var go=new GameObject("Speed trial countdown",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(gameplaySpeedButton.transform,false);
                speedTrialTimer=go.GetComponent<TextMeshProUGUI>();speedTrialTimer.font=gameplaySpeedLabel.font;speedTrialTimer.fontSize=28;speedTrialTimer.alignment=TextAlignmentOptions.Center;
                speedTrialTimer.color=WoolArt.Hex("713B19");speedTrialTimer.outlineColor=WoolArt.Hex("FFF1BD");speedTrialTimer.outlineWidth=.12f;speedTrialTimer.raycastTarget=false;
                var rt=speedTrialTimer.rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(.5f,0);rt.pivot=new Vector2(.5f,1);rt.anchoredPosition=new Vector2(0,-6);rt.sizeDelta=new Vector2(180,40);
            }
            speedTrialTimer.gameObject.SetActive(visible);
            int seconds=Mathf.Max(0,Mathf.CeilToInt(remaining));if(seconds==shownTrialSeconds)return;shownTrialSeconds=seconds;
            speedTrialTimer.text=(seconds/60).ToString("00")+":"+(seconds%60).ToString("00");
        }
        void StyleGameplaySpeed()
        {
            var legacy=header.Find("Label");
            if(legacy){var text=legacy.GetComponent<TMP_Text>();if(text&&text.text.StartsWith(">>"))legacy.gameObject.SetActive(false);}
            var face=gameplaySpeedButton.GetComponent<UnityEngine.UI.Image>();
            if(face){face.sprite=Resources.Load<Sprite>("WoolButtons/Amber");face.type=Image.Type.Sliced;face.color=Color.white;face.raycastTarget=true;}
                                    gameplaySpeedButton.transition=Selectable.Transition.None;
            if(face)face.enabled=false;
            if(!speedFace){var go=new GameObject("Raised cream speed face",typeof(RectTransform),typeof(WoolSpeedButtonFace));go.transform.SetParent(gameplaySpeedButton.transform,false);go.transform.SetAsFirstSibling();speedFace=go.GetComponent<WoolSpeedButtonFace>();var rt=(RectTransform)go.transform;rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;}gameplaySpeedButton.targetGraphic=speedFace;
            if(!speedVideoIcon){var go=new GameObject("Rewarded video badge",typeof(RectTransform),typeof(WoolRewardVideoIcon));go.transform.SetParent(gameplaySpeedButton.transform,false);speedVideoIcon=go.GetComponent<WoolRewardVideoIcon>();speedVideoIcon.raycastTarget=false;var rt=(RectTransform)go.transform;rt.anchorMin=rt.anchorMax=new Vector2(0,.5f);rt.pivot=new Vector2(.5f,.5f);rt.anchoredPosition=new Vector2(44,0);rt.sizeDelta=new Vector2(53,49);go.SetActive(speedNeedsAd&&!speedUnlocked);}
            if(gameplaySpeedLabel)
            {
                gameplaySpeedLabel.textWrappingMode=TextWrappingModes.NoWrap;
                gameplaySpeedLabel.enableAutoSizing=false;gameplaySpeedLabel.fontSize=48;
                gameplaySpeedLabel.color=WoolArt.Hex("713B19");gameplaySpeedLabel.outlineColor=WoolArt.Hex("FFF1BD");
                gameplaySpeedLabel.outlineWidth=.1f;gameplaySpeedLabel.raycastTarget=false;gameplaySpeedButton.GetComponent<RectTransform>().sizeDelta=new Vector2(190,100);gameplaySpeedLabel.rectTransform.anchorMin=Vector2.zero;gameplaySpeedLabel.rectTransform.anchorMax=Vector2.one;gameplaySpeedLabel.rectTransform.offsetMin=Vector2.one*5;gameplaySpeedLabel.rectTransform.offsetMax=-Vector2.one*5;
            }
        }
#if UNITY_EDITOR
        public void AuthorSpeedControl()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Author the speed button in Edit Mode.");
            if(!header)throw new InvalidOperationException("The gameplay header is missing.");
            if(!gameplaySpeedButton)
            {
                var existing=header.Find("Gameplay speed");
                gameplaySpeedButton=existing?existing.GetComponent<Button>():ButtonAt("Gameplay speed",header,">>  x1",new Vector2(118,70),new Vector2(270,-80),null);
            }
            gameplaySpeedLabel=gameplaySpeedButton.GetComponentInChildren<TMP_Text>(true);
            var rect=gameplaySpeedButton.GetComponent<RectTransform>();rect.anchoredPosition=new Vector2(270,-80);rect.sizeDelta=new Vector2(118,70);
            gameplaySpeedLabel.color=Color.white;gameplaySpeedLabel.fontSize=25;gameplaySpeedLabel.rectTransform.sizeDelta=new Vector2(110,54);
            gameplaySpeedLabel.outlineColor=WoolArt.Hex("96551E");gameplaySpeedLabel.outlineWidth=.1f;
            StyleGameplaySpeed();SetGameplaySpeed(1);UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
