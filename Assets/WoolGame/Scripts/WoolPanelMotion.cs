using UnityEngine;
namespace WoolGame
{
    // Additive slide and fade; responsive fitting retains ownership of scale.
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class WoolPanelMotion : MonoBehaviour
    {
        CanvasGroup group;
        float elapsed;
        RectTransform rect;
        Vector2 origin,lastPosition;
        void Awake(){group=GetComponent<CanvasGroup>();}
        void OnEnable(){if(!group)group=GetComponent<CanvasGroup>();rect=transform as RectTransform;if(rect)origin=lastPosition=rect.anchoredPosition;elapsed=0;group.alpha=0;}
        void Update(){if(elapsed>=.3f)return;elapsed+=Time.unscaledDeltaTime;float ease=1-Mathf.Pow(1-Mathf.Clamp01(elapsed/.3f),3);group.alpha=ease;if(rect){if((rect.anchoredPosition-lastPosition).sqrMagnitude>.001f)origin=rect.anchoredPosition;rect.anchoredPosition=lastPosition=origin+Vector2.down*(24*(1-ease));}}
        void OnDisable(){if(group)group.alpha=1;if(rect){if((rect.anchoredPosition-lastPosition).sqrMagnitude>.001f)origin=rect.anchoredPosition;rect.anchoredPosition=origin;}}
    }
}
