using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoolAbilitySpotlight : MaskableGraphic,ICanvasRaycastFilter
    {
        public Rect opening;
        public bool passThroughOpening;
        public bool IsRaycastLocationValid(Vector2 screen,Camera camera){if(!passThroughOpening)return true;RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,screen,camera,out var local);return !opening.Contains(local);}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;var h=opening;
            Quad(r.xMin,r.yMin,h.xMin,r.yMax);Quad(h.xMax,r.yMin,r.xMax,r.yMax);
            Quad(h.xMin,r.yMin,h.xMax,h.yMin);Quad(h.xMin,h.yMax,h.xMax,r.yMax);
            void Quad(float x0,float y0,float x1,float y1)
            {
                if(x1<=x0||y1<=y0)return;int n=vh.currentVertCount;
                vh.AddVert(new Vector2(x0,y0),color,Vector2.zero);vh.AddVert(new Vector2(x0,y1),color,Vector2.zero);
                vh.AddVert(new Vector2(x1,y1),color,Vector2.zero);vh.AddVert(new Vector2(x1,y0),color,Vector2.zero);
                vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
            }
        }
    }
}
