using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoolSpinLamp : MaskableGraphic
    {
        bool lit;
        public void SetLit(bool value){if(lit==value)return;lit=value;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();float radius=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.5f;
            Disc(vh,radius,new Color(.86f,.44f,.025f));
            Disc(vh,radius*.82f,lit?new Color(1,.95f,.25f):new Color(.57f,.26f,.015f));
            Disc(vh,radius*.58f,lit?new Color(1,1,.90f):new Color(.78f,.44f,.055f));
        }
        static void Disc(VertexHelper vh,float radius,Color c)
        {
            int n=vh.currentVertCount;vh.AddVert(Vector3.zero,c,Vector2.zero);
            for(int i=0;i<=32;i++){float a=i*Mathf.PI*2/32;vh.AddVert(new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius),c,Vector2.zero);if(i>0)vh.AddTriangle(n,n+i,n+i+1);}
        }
    }
}
