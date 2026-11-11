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
        public Sprite profilePencilSprite;
        public Button[] choices;
        public TMP_InputField nameInput;
        public const int MaxNameLength=WoolProgress.MaxPlayerNameLength;
        public TMP_Text nameLimitMessage;
        public Button namePencil;
        void LimitMessage(bool visible){if(nameLimitMessage){nameLimitMessage.text="Maximum 15 characters allowed";nameLimitMessage.gameObject.SetActive(visible);}}
        char ValidateNameCharacter(string current,int position,char added){if(current.Length>=MaxNameLength){LimitMessage(true);return '\0';}return added;}
        void NameEdited(string value){if(value.Length>MaxNameLength){nameInput.SetTextWithoutNotify(value.Substring(0,MaxNameLength));LimitMessage(true);}else if(value.Length<MaxNameLength)LimitMessage(false);}
        void SaveDraft(){if(nameInput.text.Length>MaxNameLength){LimitMessage(true);return;}WoolProgress.SetProfile(nameInput.text,draft);if(draftAvatarChosen)WoolProgress.RecordProfileAvatar();editScreen.SetActive(false);mainScreen.SetActive(true);Refresh();changed?.Invoke();}
        public CanvasGroup fade;
        int draft;bool draftAvatarChosen;
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
            for(int i=0;i<choices.Length;i++){int index=i;Wire(choices[i],()=>{draft=index;draftAvatarChosen=true;AvatarSelectionVersion++;RefreshSelection();});}
        }
        void OnEnable(){shownAt=Time.unscaledTime;Fit();nextRefresh=0;}
        public void Show(){WoolProgress.RecordProfileVisit();mainScreen.SetActive(true);editScreen.SetActive(false);gameObject.SetActive(true);transform.SetAsLastSibling();shownAt=Time.unscaledTime;Refresh();Fit();}
        public void Refresh()
        {
            nameLabel.text=WoolProgress.PlayerName;levelLabel.text=WoolProgress.CurrentLevel.ToString();portrait.sprite=avatars[WoolProgress.AvatarIndex];
            if(idLabel)idLabel.text="ID: "+WoolProgress.UserId;

            values[0].text=WoolProgress.FirstTryWins.ToString();values[1].text=WoolTrustedClock.Ready?WoolProgress.TodayCompletedCount.ToString():"--";
            values[2].text=WoolProgress.WinningStreak.ToString();values[3].text=WoolProgress.BestStreak.ToString();
            values[4].text=WoolProgress.CompletedCount.ToString();values[5].text=WoolCoinText.Format(WoolProgress.CoinsCollected);
        }
        void Edit(){draftAvatarChosen=false;draft=WoolProgress.AvatarIndex;var current=WoolProgress.PlayerName==WoolProgress.UserId?"":WoolProgress.PlayerName;nameInput.SetTextWithoutNotify(current.Substring(0,Math.Min(MaxNameLength,current.Length)));LimitMessage(false);if(editorIdLabel)editorIdLabel.text="Player ID: "+WoolProgress.UserId;mainScreen.SetActive(false);editScreen.SetActive(true);RefreshSelection();}
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
        public void ApplyEditorStyle()
        {
            var hud=GetComponentInParent<WoolHud>();var source=hud?hud.GetComponentInChildren<WoolUnlockPresentation>(true):null;
            if(source&&editScreen){
                var ribbon=source.heading.transform.parent.GetComponent<Image>();var banner=editScreen.transform.Find("Editor banner");
                var title=editScreen.transform.Find("Editor title");
                if(banner){var image=banner.GetComponent<Image>();if(image&&ribbon){image.sprite=ribbon.sprite;image.type=ribbon.type;image.color=Color.white;}HeaderRect((RectTransform)banner,new Vector2(0,-38),new Vector2(650,145));}
                if(title){HeaderRect((RectTransform)title,new Vector2(0,-38),new Vector2(600,100));var text=title.GetComponent<TMP_Text>();text.text="Make It Yours";text.font=source.heading.font;text.fontSharedMaterial=source.heading.fontSharedMaterial;text.fontSize=52;text.alignment=TextAlignmentOptions.Center;}
                if(cancel)HeaderRect((RectTransform)cancel.transform,new Vector2(425,-46),new Vector2(84,84));
                var frame=source.card.GetComponent<Image>();
                if(frame)for(int i=0;i<choices.Length;i++){var border=editScreen.transform.Find("Avatar card "+i);if(border){var image=border.GetComponent<Image>();image.sprite=frame.sprite;image.type=Image.Type.Sliced;image.color=Color.white;}}
            }
            if(nameLabel){var r=nameLabel.rectTransform;r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(517.5f,-308);r.sizeDelta=new Vector2(275,82);}
            if(idLabel){var r=idLabel.rectTransform;r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(517.5f,-373);r.sizeDelta=new Vector2(275,36);}
            FitPortrait(portrait);
            if(portrait){var mask=portrait.transform.parent as RectTransform;mask.sizeDelta=new Vector2(190,184);FitPortrait(portrait);}
            if(editScreen){
                var border=editScreen.transform.Find("Name field border");var frame=card.Find("Shared popup frame");
                if(border&&frame){var image=border.GetComponent<Image>();var sourceImage=frame.GetComponent<Image>();if(image&&sourceImage){image.sprite=sourceImage.sprite;image.type=Image.Type.Sliced;image.color=Color.white;}}
                foreach(var choice in choices)if(choice){
                    var mask=choice.transform.Find("Rounded portrait") as RectTransform;
                    if(mask){mask.anchorMin=Vector2.zero;mask.anchorMax=Vector2.one;mask.pivot=Vector2.one*.5f;mask.offsetMin=new Vector2(6,6);mask.offsetMax=new Vector2(-6,-6);var image=mask.GetComponentInChildren<Image>(true);FitPortrait(image);}
                }
            }
            if(pencil){
                pencil.transform.SetAsLastSibling();
                if(portrait){var mask=(RectTransform)portrait.transform.parent;var rt=(RectTransform)pencil.transform;rt.position=mask.TransformPoint(new Vector3(mask.rect.xMax-4,mask.rect.yMin+24,0));rt.sizeDelta=new Vector2(80,80);}

                var style=pencil.GetComponent<WoolGlossyButton>();if(style)style.enabled=false;
                var face=pencil.GetComponent<Image>();if(face&&profilePencilSprite){face.sprite=profilePencilSprite;face.color=Color.white;face.preserveAspect=true;face.enabled=true;face.raycastTarget=true;pencil.targetGraphic=face;}
                var icon=pencil.transform.Find("Plain pencil");if(icon)icon.gameObject.SetActive(false);
            }
        }
        static void FitPortrait(Image image)
        {
            if(!image||!image.sprite)return;var mask=image.transform.parent as RectTransform;if(!mask)return;
            var rt=image.rectTransform;rt.anchorMin=rt.anchorMax=rt.pivot=Vector2.one*.5f;rt.anchoredPosition=Vector2.zero;
            var size=mask.rect.size;if(size.x<=0||size.y<=0)return;
            float scale=Mathf.Max(size.x/image.sprite.rect.width,size.y/image.sprite.rect.height);
            rt.sizeDelta=image.sprite.rect.size*scale;image.preserveAspect=false;image.maskable=true;image.raycastTarget=false;
        }
        static void HeaderRect(RectTransform rt,Vector2 position,Vector2 size){rt.anchorMin=rt.anchorMax=new Vector2(.5f,1);rt.pivot=Vector2.one*.5f;rt.anchoredPosition=position;rt.sizeDelta=size;}
        public void Fit()
        {
            if(!card||!dimmer)return;root=(RectTransform)transform;ApplyEditorStyle();
            var shared=card.Find("Shared popup frame");
            if(shared){
                var source=shared.GetComponent<Image>();var target=editScreen?editScreen.transform.Find("Editor card"):null;
                var background=target?target.GetComponent<Image>():null;
                if(source&&background){background.sprite=source.sprite;background.type=source.type;background.color=Color.white;
                    var rt=background.rectTransform;rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;rt.SetAsFirstSibling();}
                shared.gameObject.SetActive(false);
            }
            var canvas=GetComponentInParent<Canvas>();if(!canvas)return;canvasRect=(RectTransform)canvas.transform;
            var size=root.rect.size;if(size.x<1||size.y<1)return;
            card.anchorMin=card.anchorMax=new Vector2(.5f,.5f);card.anchoredPosition=Vector2.zero;card.sizeDelta=new Vector2(941,1672);
            card.localScale=Vector3.one*Mathf.Min(size.x*.94f/941,size.y*.94f/1672);
            dimmer.rectTransform.anchorMin=dimmer.rectTransform.anchorMax=new Vector2(.5f,.5f);dimmer.rectTransform.position=canvasRect.TransformPoint(canvasRect.rect.center);dimmer.rectTransform.sizeDelta=canvasRect.rect.size;
            lastSize=size;lastCanvas=canvasRect.rect.size;
        }
    }
}
