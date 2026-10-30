using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    // Corner brackets keep the real target visible and do not rotate away from its bounds.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoolTutorialFrame : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;float length=Mathf.Min(42,Mathf.Min(r.width,r.height)*.26f),stroke=6;
            for(int x=0;x<2;x++)for(int y=0;y<2;y++)
            {
                float left=x==0?r.xMin:r.xMax-length,bottom=y==0?r.yMin:r.yMax-stroke;
                Quad(left,bottom,left+length,bottom+stroke);
                left=x==0?r.xMin:r.xMax-stroke;bottom=y==0?r.yMin:r.yMax-length;
                Quad(left,bottom,left+stroke,bottom+length);
            }
            void Quad(float x0,float y0,float x1,float y1)
            {
                int n=vh.currentVertCount;
                vh.AddVert(new Vector2(x0,y0),color,Vector2.zero);vh.AddVert(new Vector2(x0,y1),color,Vector2.zero);
                vh.AddVert(new Vector2(x1,y1),color,Vector2.zero);vh.AddVert(new Vector2(x1,y0),color,Vector2.zero);
                vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
            }
        }
    }
}
