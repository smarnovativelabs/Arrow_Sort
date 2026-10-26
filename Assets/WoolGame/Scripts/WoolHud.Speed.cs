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
        public void SetGameplaySpeed(int multiplier)
        {
            if(gameplaySpeedLabel)gameplaySpeedLabel.text=multiplier==2?"×2":"×1";
        }
        void StyleGameplaySpeed()
        {
            var legacy=header.Find("Label");
            if(legacy){var text=legacy.GetComponent<TMP_Text>();if(text&&text.text.StartsWith(">>"))legacy.gameObject.SetActive(false);}
            var face=gameplaySpeedButton.GetComponent<UnityEngine.UI.Image>();
            if(face){face.sprite=null;face.color=Color.clear;face.raycastTarget=true;}
            gameplaySpeedButton.transition=Selectable.Transition.None;
            if(gameplaySpeedLabel)
            {
                gameplaySpeedLabel.textWrappingMode=TextWrappingModes.NoWrap;
                gameplaySpeedLabel.enableAutoSizing=false;gameplaySpeedLabel.fontSize=46;
                gameplaySpeedLabel.color=new Color(.2f,.27f,.12f,1);gameplaySpeedLabel.outlineColor=Color.white;
                gameplaySpeedLabel.outlineWidth=.1f;gameplaySpeedLabel.raycastTarget=false;
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
