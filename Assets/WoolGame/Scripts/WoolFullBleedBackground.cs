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
        RawImage mapContinuation;
        readonly Vector3[] sourceCorners=new Vector3[4];
        void ContinueMap(Image source)
        {
            // This transform defines the map coordinates; the continuation below is
            // the sole renderer, including the notch and the area behind the buttons.
            source.enabled=false;
            if(!mapContinuation)
            {
                var existing=transform.Find("Continuous map through safe edges");
                if(existing)mapContinuation=existing.GetComponent<RawImage>();
                else
                {
                    var go=new GameObject("Continuous map through safe edges",typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage));
                    go.transform.SetParent(transform,false);mapContinuation=go.GetComponent<RawImage>();
                }
                mapContinuation.raycastTarget=false;
            }
            var map=source.GetComponentInParent<WoolSkywayMap>();
            if(mapContinuation.transform.parent!=map.transform){mapContinuation.transform.SetParent(map.transform,false);mapContinuation.transform.SetAsFirstSibling();}
            mapContinuation.enabled=true;
            WoolCanvasArt.FillCanvas(mapContinuation.rectTransform);
            mapContinuation.texture=source.sprite.texture;
            mapContinuation.texture.wrapMode=TextureWrapMode.Clamp;
            mapContinuation.color=source.color;
            source.rectTransform.GetWorldCorners(sourceCorners);
            Vector2 min=mapContinuation.rectTransform.InverseTransformPoint(sourceCorners[0]);
            Vector2 max=mapContinuation.rectTransform.InverseTransformPoint(sourceCorners[2]);
            Vector2 span=max-min;
            if(span.x<=0||span.y<=0)return;
            var rect=mapContinuation.rectTransform.rect;
            Vector2 low=(rect.min-min)/span,high=(rect.max-min)/span;
            var uv=UnityEngine.Sprites.DataUtility.GetOuterUV(source.sprite);
            mapContinuation.uvRect=Rect.MinMaxRect(uv.x+low.x*(uv.z-uv.x),uv.y+low.y*(uv.w-uv.y),uv.x+high.x*(uv.z-uv.x),uv.y+high.y*(uv.w-uv.y));
        }
        void LateUpdate()
        {
            if(!surface)return;
            UnityEngine.UI.Image source=null;
            if(sources!=null)foreach(var candidate in sources)
                if(candidate&&candidate.gameObject.activeInHierarchy&&(candidate.sprite||candidate.color.a>.9f))source=candidate;
            if(source&&source.sprite&&source.GetComponentInParent<WoolSkywayMap>())
            {
                surface.enabled=false;ContinueMap(source);return;
            }
            if(mapContinuation)mapContinuation.enabled=false;
            bool visible=source;
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
