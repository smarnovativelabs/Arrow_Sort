using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    // Rounded vector gradients keep the interactive surfaces sharp at every portrait size.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoolDailySurface : MaskableGraphic
    {
        public Color top=Color.white,bottom=Color.white,border=Color.clear;
        public float radius=28,stroke=3;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;
            Rounded(vh,r.width,r.height,radius,border,border);
            Rounded(vh,r.width-stroke*2,r.height-stroke*2,Mathf.Max(0,radius-stroke),top,bottom);
        }
        void Rounded(VertexHelper vh,float w,float h,float radius,Color top,Color bottom)
        {
            if(w<=0||h<=0)return;
            float r=Mathf.Min(radius,Mathf.Min(w,h)*.5f);int n=vh.currentVertCount;vh.AddVert(Vector3.zero,Color.Lerp(bottom,top,.5f)*color,Vector2.zero);
            for(int corner=0;corner<4;corner++)for(int step=0;step<=12;step++)
            {
                float a=(corner*90+step*7.5f)*Mathf.Deg2Rad;
                var c=new Vector2(corner==0||corner==3?w/2-r:-w/2+r,corner<2?h/2-r:-h/2+r);
                var p=c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r;int i=corner*13+step;
                vh.AddVert(p,Color.Lerp(bottom,top,(p.y+h*.5f)/h)*color,Vector2.zero);if(i>0)vh.AddTriangle(n,n+i,n+i+1);
            }
            vh.AddTriangle(n,n+52,n+1);
        }
    }
}
