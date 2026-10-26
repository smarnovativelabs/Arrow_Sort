using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    // Continue a popup dimmer into the notch/home-indicator margins without darkening it twice.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoolSafeAreaBackdrop : MaskableGraphic
    {
        public UnityEngine.UI.Image source;
        Color lastColor;Vector2 lastSize;bool lastEnabled;
        readonly Vector3[] corners=new Vector3[4];
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(!source||!source.enabled||source.color.a<=0)return;
            var root=canvas?(RectTransform)canvas.rootCanvas.transform:null;if(!root)return;
            root.GetWorldCorners(corners);
            Vector2 min=rectTransform.InverseTransformPoint(corners[0]),max=rectTransform.InverseTransformPoint(corners[2]);
            var hole=rectTransform.rect;
            void Quad(float x0,float y0,float x1,float y1)
            {
                if(x1<=x0||y1<=y0)return;int n=vh.currentVertCount;
                vh.AddVert(new Vector3(x0,y0),source.color,Vector2.zero);vh.AddVert(new Vector3(x0,y1),source.color,Vector2.zero);
                vh.AddVert(new Vector3(x1,y1),source.color,Vector2.zero);vh.AddVert(new Vector3(x1,y0),source.color,Vector2.zero);
                vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
            }
            Quad(min.x,min.y,max.x,hole.yMin);Quad(min.x,hole.yMax,max.x,max.y);
            Quad(min.x,hole.yMin,hole.xMin,hole.yMax);Quad(hole.xMax,hole.yMin,max.x,hole.yMax);
        }
        void LateUpdate()
        {
            if(!source)return;
            var root=canvas?(RectTransform)canvas.rootCanvas.transform:null;var size=root?root.rect.size:Vector2.zero;
            if(lastColor!=source.color||lastSize!=size||lastEnabled!=source.enabled)
            {lastColor=source.color;lastSize=size;lastEnabled=source.enabled;SetVerticesDirty();}
        }
    }
}
