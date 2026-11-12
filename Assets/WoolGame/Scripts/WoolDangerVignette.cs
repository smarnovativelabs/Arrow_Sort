using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    // A screen-edge warning with a transparent center; independent of sprite padding/aspect.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoolDangerVignette : MaskableGraphic
    {
        readonly Vector2[] outer=new Vector2[4],inner=new Vector2[4];
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;
            float feather=Mathf.Min(r.width,r.height)*.065f;
            outer[0]=new Vector2(r.xMin,r.yMin);outer[1]=new Vector2(r.xMin,r.yMax);outer[2]=new Vector2(r.xMax,r.yMax);outer[3]=new Vector2(r.xMax,r.yMin);
            inner[0]=outer[0]+new Vector2(feather,feather);inner[1]=outer[1]+new Vector2(feather,-feather);inner[2]=outer[2]+new Vector2(-feather,-feather);inner[3]=outer[3]+new Vector2(-feather,feather);
            var clear=color;clear.a=0;
            for(int i=0;i<4;i++)
            {
                int j=(i+1)%4,n=vh.currentVertCount;
                vh.AddVert(outer[i],color,Vector2.zero);vh.AddVert(outer[j],color,Vector2.zero);
                vh.AddVert(inner[j],clear,Vector2.zero);vh.AddVert(inner[i],clear,Vector2.zero);
                vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
            }
        }
    }
}
