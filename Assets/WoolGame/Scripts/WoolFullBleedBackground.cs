using UnityEngine;
using UnityEngine.UI;

namespace WoolGame
{
    // Art covers the physical canvas; hit targets remain in the HUD's safe rectangle.
    public static class WoolCanvasArt
    {
        static readonly Vector3[] Corners=new Vector3[4];
        public static void FillCanvas(RectTransform art)
        {
            if(!art)return;var canvas=art.GetComponentInParent<Canvas>();var parent=art.parent as RectTransform;
            if(!canvas||!parent)return;var root=(RectTransform)canvas.rootCanvas.transform;root.GetWorldCorners(Corners);
            Vector2 min=parent.InverseTransformPoint(Corners[0]),max=parent.InverseTransformPoint(Corners[2]);
            art.anchorMin=art.anchorMax=Vector2.zero;art.pivot=Vector2.zero;art.localScale=Vector3.one;
            art.anchoredPosition=min-parent.rect.min;art.sizeDelta=max-min;
        }
        public static void Cover(RectTransform art, float aspect, float overscan=1.015f)
        {
            if(!art||aspect<=0)return;
            var canvas=art.GetComponentInParent<Canvas>();
            if(!canvas||!(art.parent is RectTransform parent))return;
            var root=(RectTransform)canvas.rootCanvas.transform;
            root.GetWorldCorners(Corners);
            var min=(Vector2)parent.InverseTransformPoint(Corners[0]);
            var max=(Vector2)parent.InverseTransformPoint(Corners[2]);
            var size=max-min;
            float height=Mathf.Max(size.y,size.x/aspect)*overscan;
            art.anchorMin=art.anchorMax=new Vector2(.5f,.5f);art.pivot=new Vector2(.5f,.5f);
            art.localScale=Vector3.one;
            art.position=parent.TransformPoint((min+max)*.5f);
            art.sizeDelta=new Vector2(height*aspect,height);
        }
    }

    // Serialized and authored once, never clones textures or rebuilds hierarchy during play.
    [DefaultExecutionOrder(200)]
    public sealed class WoolFullBleedBackground : MonoBehaviour
    {
        public UnityEngine.UI.Image surface;
        public UnityEngine.UI.Image[] sources;
        public GameObject menuRoot;
        Vector2 lastSize;Sprite lastSprite;
        void LateUpdate()
        {
            if(!surface)return;
            UnityEngine.UI.Image source=null;
            if(sources!=null)foreach(var candidate in sources)
                if(candidate&&candidate.gameObject.activeInHierarchy&&(candidate.sprite||candidate.color.a>.9f))source=candidate;
            bool visible=source;
            // The scrolling map already fills the physical canvas. A second, separately
            // scaled copy behind it makes its paths diverge in the notch and bottom margins.
            if(source&&source.GetComponentInParent<WoolSkywayMap>())visible=false;
            surface.enabled=visible;
            if(!visible)return;
            var rect=(RectTransform)surface.transform.parent;
            if(surface.sprite!=source.sprite){surface.sprite=source.sprite;lastSize=Vector2.zero;}
            surface.color=source.color;
            if(lastSize!=rect.rect.size||lastSprite!=surface.sprite)
            {
                lastSize=rect.rect.size;lastSprite=surface.sprite;
                float aspect=surface.sprite?surface.sprite.rect.width/surface.sprite.rect.height:rect.rect.width/Mathf.Max(1,rect.rect.height);
                WoolCanvasArt.Cover(surface.rectTransform,aspect);
            }
        }
    }
}
