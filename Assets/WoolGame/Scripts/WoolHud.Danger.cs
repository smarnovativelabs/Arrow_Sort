using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] Image dangerBorder;
        public void SetDanger(float intensity,float time)
        {
            if(!dangerBorder)return;
            dangerBorder.gameObject.SetActive(intensity>.01f);
            if(intensity<=.01f)return;
            WoolCanvasArt.FillCanvas(dangerBorder.rectTransform);
            dangerBorder.raycastTarget=false;dangerBorder.preserveAspect=false;
            dangerBorder.enabled=false;
            var vignette=dangerBorder.GetComponentInChildren<WoolDangerVignette>(true);
            if(!vignette)
            {
                var edge=new GameObject("Full screen danger glow",typeof(RectTransform),typeof(CanvasRenderer));
                var rect=(RectTransform)edge.transform;rect.SetParent(dangerBorder.transform,false);
                rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
                vignette=edge.AddComponent<WoolDangerVignette>();
            }
            vignette.raycastTarget=false;
            vignette.color=new Color(.95f,.035f,.02f,Mathf.Lerp(.7f,1f,Mathf.Clamp01(intensity))*(.85f+.15f*Mathf.Sin(time*5)));
            dangerBorder.transform.SetAsLastSibling();
        }
#if UNITY_EDITOR
        public void AuthorDangerBorder(Sprite sprite)
        {
            if(!dangerBorder)
            {
                var rect=Rect("Kitten danger border",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
                dangerBorder=rect.gameObject.AddComponent<Image>();dangerBorder.raycastTarget=false;
            }
            dangerBorder.sprite=sprite;dangerBorder.type=Image.Type.Simple;
            dangerBorder.transform.SetAsLastSibling();dangerBorder.gameObject.SetActive(false);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
