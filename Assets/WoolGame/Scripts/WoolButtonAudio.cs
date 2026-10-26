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
        float amount=1,velocity,submitUntil;
        public void Initialize(UnityEngine.UI.Button target,WoolGameAudio audio)
        {
            button=target;audioFeedback=audio;
            if(button.targetGraphic&&!button.targetGraphic.GetComponent<UnityEngine.UI.Shadow>())
            {
                var shadow=button.targetGraphic.gameObject.AddComponent<UnityEngine.UI.Shadow>();
                shadow.effectColor=new Color(.08f,.16f,.2f,.18f);shadow.effectDistance=new Vector2(0,-3);
            }
            if(!ready){restingScale=lastApplied=transform.localScale;ready=true;}
            var colors=button.colors;colors.fadeDuration=.1f;
            colors.highlightedColor=new Color(1,.98f,.9f);colors.pressedColor=new Color(.8f,.86f,.9f);
            colors.selectedColor=colors.highlightedColor;button.colors=colors;
        }
        bool CanInteract=>button&&button.IsActive()&&button.IsInteractable();
        void Play(){if(CanInteract&&audioFeedback)audioFeedback.Play(WoolGameAudio.Cue.Click);}
        void LateUpdate()
        {
            if(!ready)return;
            // Portrait layout can change the base scale between frames.
            var currentScale=transform.localScale;
            if((currentScale-lastApplied).sqrMagnitude>.00001f)restingScale=currentScale;
            float target=CanInteract?(pressed||Time.unscaledTime<submitUntil?.94f:hovered||selected?1.035f:1):1;
            // Settle exactly so idle buttons stop dirtying their Canvas transforms.
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
