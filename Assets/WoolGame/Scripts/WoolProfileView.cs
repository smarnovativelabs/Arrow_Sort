using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    public sealed class WoolProfileView : MonoBehaviour
    {
        public RectTransform card;
        public Image dimmer,portrait;
        public GameObject mainScreen,editScreen;
        public TMP_Text nameLabel,levelLabel;
        [HideInInspector] public TMP_Text dateLabel;
        public TMP_Text idLabel,editorIdLabel,selectionName;
        public Image selectedPreview;
        public GameObject[] choiceHighlights;
        public TMP_Text[] values,selectedLabels;
        public Sprite[] avatars;
        public Button pencil,nameButton,close,save,cancel;
        public Button[] choices;
        public TMP_InputField nameInput;
        public const int MaxNameLength=WoolProgress.MaxPlayerNameLength;
        public TMP_Text nameLimitMessage;
        public Button namePencil;
        void LimitMessage(bool visible){if(nameLimitMessage){nameLimitMessage.text="Maximum 15 characters allowed";nameLimitMessage.gameObject.SetActive(visible);}}
        char ValidateNameCharacter(string current,int position,char added){if(current.Length>=MaxNameLength){LimitMessage(true);return '\0';}return added;}
        void NameEdited(string value){if(value.Length>MaxNameLength){nameInput.SetTextWithoutNotify(value.Substring(0,MaxNameLength));LimitMessage(true);}else if(value.Length<MaxNameLength)LimitMessage(false);}
        void SaveDraft(){if(nameInput.text.Length>MaxNameLength){LimitMessage(true);return;}WoolProgress.SetProfile(nameInput.text,draft);editScreen.SetActive(false);mainScreen.SetActive(true);Refresh();changed?.Invoke();}
        public CanvasGroup fade;
        int draft;
        public int AvatarSelectionVersion {get;private set;}
        float shownAt,nextRefresh;
        Action changed;
        RectTransform root,canvasRect;
        Vector2 lastSize,lastCanvas;
        public void Bind(Action onChanged)
        {
            changed=onChanged;if(dateLabel)dateLabel.gameObject.SetActive(false);
            void Wire(Button b,UnityEngine.Events.UnityAction action){b.onClick.RemoveAllListeners();b.onClick.AddListener(action);}
            Wire(close,()=>gameObject.SetActive(false));Wire(pencil,Edit);Wire(nameButton,Edit);
            Wire(cancel,()=>{editScreen.SetActive(false);mainScreen.SetActive(true);});
            Wire(save,SaveDraft);
            nameInput.characterLimit=0;nameInput.onValidateInput=ValidateNameCharacter;nameInput.onValueChanged.RemoveListener(NameEdited);nameInput.onValueChanged.AddListener(NameEdited);
            if(namePencil)Wire(namePencil,()=>{nameInput.Select();nameInput.ActivateInputField();});
            for(int i=0;i<choices.Length;i++){int index=i;Wire(choices[i],()=>{draft=index;AvatarSelectionVersion++;RefreshSelection();});}
        }
        void OnEnable(){shownAt=Time.unscaledTime;Fit();nextRefresh=0;}
        public void Show(){mainScreen.SetActive(true);editScreen.SetActive(false);gameObject.SetActive(true);transform.SetAsLastSibling();shownAt=Time.unscaledTime;Refresh();Fit();}
        public void Refresh()
        {
            nameLabel.text=WoolProgress.PlayerName;levelLabel.text=WoolProgress.CurrentLevel.ToString();portrait.sprite=avatars[WoolProgress.AvatarIndex];
            if(idLabel)idLabel.text="ID: "+WoolProgress.UserId;

            values[0].text=WoolProgress.FirstTryWins.ToString();values[1].text=WoolTrustedClock.Ready?WoolProgress.TodayCompletedCount.ToString():"--";
            values[2].text=WoolProgress.WinningStreak.ToString();values[3].text=WoolProgress.BestStreak.ToString();
            values[4].text=WoolProgress.CompletedCount.ToString();values[5].text=WoolCoinText.Format(WoolProgress.CoinsCollected);
        }
        void Edit(){draft=WoolProgress.AvatarIndex;var current=WoolProgress.PlayerName==WoolProgress.UserId?"":WoolProgress.PlayerName;nameInput.SetTextWithoutNotify(current.Substring(0,Math.Min(MaxNameLength,current.Length)));LimitMessage(false);if(editorIdLabel)editorIdLabel.text="Player ID: "+WoolProgress.UserId;mainScreen.SetActive(false);editScreen.SetActive(true);RefreshSelection();}
        void RefreshSelection()
        {
            for(int i=0;i<choices.Length;i++){var image=choices[i].GetComponent<Image>();image.color=image.sprite?Color.white:Color.clear;if(selectedLabels[i].transform.parent.name=="Selected ribbon"){selectedLabels[i].gameObject.SetActive(true);selectedLabels[i].transform.parent.gameObject.SetActive(i==draft);}else selectedLabels[i].gameObject.SetActive(i==draft);if(choiceHighlights!=null&&i<choiceHighlights.Length)choiceHighlights[i].SetActive(i==draft);}
            if(selectedPreview)selectedPreview.sprite=avatars[draft];
            if(selectionName)selectionName.text=new[]{"Royal Duck","Explorer Duck","Wizard Duck","Pilot Duck","Chef Duck","Garden Duck"}[draft];
        }
        void Update()
        {
            if(!root||root.rect.size!=lastSize||(canvasRect&&canvasRect.rect.size!=lastCanvas))Fit();
            if(fade)fade.alpha=Mathf.Clamp01((Time.unscaledTime-shownAt)/.22f);
            if(Time.unscaledTime>=nextRefresh){nextRefresh=Time.unscaledTime+1;if(mainScreen.activeSelf)Refresh();}
        }
        public void Fit()
        {
            if(!card||!dimmer)return;root=(RectTransform)transform;
            var canvas=GetComponentInParent<Canvas>();if(!canvas)return;canvasRect=(RectTransform)canvas.transform;
            var size=root.rect.size;if(size.x<1||size.y<1)return;
            card.anchorMin=card.anchorMax=new Vector2(.5f,.5f);card.anchoredPosition=Vector2.zero;card.sizeDelta=new Vector2(941,1672);
            card.localScale=Vector3.one*Mathf.Min(size.x*.94f/941,size.y*.94f/1672);
            dimmer.rectTransform.anchorMin=dimmer.rectTransform.anchorMax=new Vector2(.5f,.5f);dimmer.rectTransform.position=canvasRect.TransformPoint(canvasRect.rect.center);dimmer.rectTransform.sizeDelta=canvasRect.rect.size;
            lastSize=size;lastCanvas=canvasRect.rect.size;
        }
    }
}
