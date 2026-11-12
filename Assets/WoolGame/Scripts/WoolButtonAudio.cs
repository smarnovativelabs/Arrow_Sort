using UnityEngine;
using UnityEngine.EventSystems;
namespace WoolGame
{
    // Pointer/keyboard feedback survives onClick listener rebinding.
    public sealed class WoolButtonAudio : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        UnityEngine.UI.Button button;
        WoolGameAudio audioFeedback;
        Vector3 restingScale;
        Vector3 lastApplied;
        bool pressed,hovered,selected,ready;
        float amount=1,velocity,submitUntil;TMPro.TMP_Text[] captions;UnityEngine.UI.Graphic[] rewardIcons;bool[] videoIcons;
        public void Initialize(UnityEngine.UI.Button target,WoolGameAudio audio)
        {
            button=target;audioFeedback=audio;captions=GetComponentsInChildren<TMPro.TMP_Text>(true);rewardIcons=GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            videoIcons=new bool[rewardIcons.Length];
            for(int i=0;i<rewardIcons.Length;i++){
                var icon=rewardIcons[i];if(!icon)continue;var iconName=icon.name;
                videoIcons[i]=icon is WoolRewardVideoGraphic||icon is WoolRewardVideoIcon||
                    (Contains(iconName,"video")&&(Contains(iconName,"icon")||Contains(iconName,"reward")));
            }
            if(button.targetGraphic)foreach(var shadow in button.targetGraphic.GetComponents<UnityEngine.UI.Shadow>())
                if(shadow.GetType()==typeof(UnityEngine.UI.Shadow))shadow.enabled=false;
            if(!ready){restingScale=lastApplied=transform.localScale;ready=true;}
            var colors=button.colors;colors.fadeDuration=.1f;
            colors.highlightedColor=new Color(1,.98f,.9f);colors.pressedColor=new Color(.8f,.86f,.9f);
            colors.selectedColor=colors.highlightedColor;button.colors=colors;
        }
        bool CanInteract=>button&&button.IsActive()&&button.IsInteractable();
        static bool Contains(string text,string value)=>text.IndexOf(value,System.StringComparison.OrdinalIgnoreCase)>=0;
        void Play(){if(CanInteract&&audioFeedback)audioFeedback.Play(WoolGameAudio.Cue.Click);}
        void LateUpdate()
        {
            if(!ready)return;
            // Portrait layout can change the base scale between frames.
            var currentScale=transform.localScale;
            if((currentScale-lastApplied).sqrMagnitude>.00001f)restingScale=currentScale;
            float target=CanInteract?(pressed||Time.unscaledTime<submitUntil?.94f:hovered||selected?1.035f:1):1;
                        bool adButton=false;
            if(captions!=null)foreach(var caption in captions){if(!caption||!caption.gameObject.activeInHierarchy)continue;var text=caption.text;if(Contains(text,"watch")||Contains(text,"video")||Contains(text,"2x coins")){adButton=true;break;}}
                        if(!adButton&&rewardIcons!=null)for(int i=0;i<rewardIcons.Length;i++){
                var icon=rewardIcons[i];
                if(!icon||!icon.enabled||!icon.gameObject.activeInHierarchy)continue;
                if(videoIcons[i]){adButton=true;break;}
            }
            if(CanInteract&&adButton&&!pressed&&Time.unscaledTime>=submitUntil)target=(hovered||selected?1.02f:1)+.055f*Mathf.Sin(Time.unscaledTime*3f);
            // Settle idle buttons when no animation is needed.
            if(Mathf.Abs(amount-target)<.0001f&&Mathf.Abs(velocity)<.0001f){amount=target;velocity=0;}
            else amount=Mathf.SmoothDamp(amount,target,ref velocity,.065f,Mathf.Infinity,Time.unscaledDeltaTime);
            lastApplied=restingScale*amount;
            if(!currentScale.Equals(lastApplied))transform.localScale=lastApplied;
        }
        void OnDisable(){pressed=hovered=selected=false;submitUntil=0;amount=1;velocity=0;if(ready)transform.localScale=lastApplied=restingScale;}
        public void OnPointerDown(PointerEventData data){if(data.button!=PointerEventData.InputButton.Left||!CanInteract)return;pressed=true;Play();}
        public void OnPointerUp(PointerEventData data){pressed=false;}
        public void OnPointerEnter(PointerEventData data){hovered=true;}
        public void OnPointerExit(PointerEventData data){hovered=false;pressed=false;}
        public void OnSelect(BaseEventData data){selected=true;}
        public void OnDeselect(BaseEventData data){selected=false;pressed=false;}
        public void OnSubmit(BaseEventData data){if(!CanInteract)return;submitUntil=Time.unscaledTime+.12f;Play();}
    }
}
