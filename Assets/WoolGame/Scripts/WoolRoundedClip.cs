using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoolRoundedClip : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();float w=rectTransform.rect.width,h=rectTransform.rect.height,r=Mathf.Min(w,h)*.16f;
            vh.AddVert(Vector3.zero,Color.white,Vector2.zero);
            for(int c=0;c<4;c++)for(int step=0;step<=16;step++)
            {
                float a=(c*90+step*90f/16)*Mathf.Deg2Rad;
                var p=new Vector2(c==0||c==3?w/2-r:-w/2+r,c<2?h/2-r:-h/2+r)+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r;
                vh.AddVert(p,Color.white,Vector2.zero);int i=c*17+step;if(i>0)vh.AddTriangle(0,i,i+1);
            }
            vh.AddTriangle(0,68,1);
        }
    }
}
