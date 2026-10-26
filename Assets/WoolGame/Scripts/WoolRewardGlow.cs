using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoolRewardGlow : MaskableGraphic
    {
        void Update(){rectTransform.localRotation=Quaternion.Euler(0,0,Time.unscaledTime*13);color=new Color(1,1,1,.65f+.2f*Mathf.Sin(Time.unscaledTime*3));}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();float r=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.5f;
            for(int i=0;i<12;i++)
            {
                float a=i*Mathf.PI/6,b=a+.11f,c=a-.11f;int n=vh.currentVertCount;
                vh.AddVert(Vector3.zero,new Color(1,.9f,.35f,.8f)*color,Vector2.zero);
                vh.AddVert(new Vector2(Mathf.Cos(b),Mathf.Sin(b))*r,new Color(1,.9f,.35f,0)*color,Vector2.zero);
                vh.AddVert(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r*.82f,new Color(1,1,.65f,.6f)*color,Vector2.zero);
                vh.AddVert(new Vector2(Mathf.Cos(c),Mathf.Sin(c))*r,new Color(1,.9f,.35f,0)*color,Vector2.zero);
                vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
            }
        }
    }
}
