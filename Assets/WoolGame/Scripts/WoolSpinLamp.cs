using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoolSpinLamp : MaskableGraphic
    {
        bool lit;
        public bool IsLit=>lit;
        public void SetLit(bool value){if(lit==value)return;lit=value;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();float radius=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.5f;
            if(lit){Glow(vh,radius*1.8f,new Color(1,.90f,.08f,.3f));Glow(vh,radius*1.35f,new Color(1,.95f,.25f,.6f));}
            Disc(vh,radius,new Color(1,.73f,.02f));
            Disc(vh,radius*.82f,lit?new Color(1,.95f,.25f):new Color(.85f,.52f,.06f));
            Disc(vh,radius*.58f,lit?new Color(1,1,.90f):new Color(1,.77f,.24f));
        }
        static void Glow(VertexHelper vh,float radius,Color center)
        {
            int n=vh.currentVertCount;vh.AddVert(Vector3.zero,center,Vector2.zero);var edge=center;edge.a=0;
            for(int i=0;i<=32;i++){float a=i*Mathf.PI/16;vh.AddVert(new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius),edge,Vector2.zero);if(i>0)vh.AddTriangle(n,n+i,n+i+1);}
        }
        static void Disc(VertexHelper vh,float radius,Color c)
        {
            int n=vh.currentVertCount;vh.AddVert(Vector3.zero,c,Vector2.zero);
            for(int i=0;i<=32;i++){float a=i*Mathf.PI*2/32;vh.AddVert(new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius),c,Vector2.zero);if(i>0)vh.AddTriangle(n,n+i,n+i+1);}
        }
    }
}
