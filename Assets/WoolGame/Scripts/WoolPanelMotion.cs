using UnityEngine;
namespace WoolGame
{
    // Brief fade keeps hit targets at their final positions from the first frame.
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class WoolPanelMotion : MonoBehaviour
    {
        CanvasGroup group;
        float elapsed;
        void Awake(){group=GetComponent<CanvasGroup>();}
        void OnEnable(){if(!group)group=GetComponent<CanvasGroup>();elapsed=0;group.alpha=.85f;}
        void Update(){if(elapsed>=.1f)return;elapsed+=Time.unscaledDeltaTime;group.alpha=Mathf.Lerp(.85f,1,Mathf.Clamp01(elapsed/.1f));}
        void OnDisable(){if(group)group.alpha=1;}
    }
}
