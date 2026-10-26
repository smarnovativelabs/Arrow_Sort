using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoolWheelGraphic : Graphic
    {
        public bool pointer;
        static void Triangle(VertexHelper vh,float r,Color top,Color bottom,float offset)
        {
            int n=vh.currentVertCount;
            vh.AddVert(new Vector3(-r,r*.5f+offset,0),top,Vector2.zero);
            vh.AddVert(new Vector3(r,r*.5f+offset,0),top,Vector2.zero);
            vh.AddVert(new Vector3(0,-r*.8f+offset,0),bottom,Vector2.zero);vh.AddTriangle(n,n+1,n+2);
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();float radius=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.5f;
            if(pointer)
            {
                Triangle(vh,radius,new Color(.56f,.23f,.02f),new Color(.73f,.31f,.01f),-4);
                Triangle(vh,radius*.96f,new Color(1,.96f,.53f),new Color(1,.59f,.02f),0);
                Triangle(vh,radius*.79f,new Color(.99f,.19f,.13f),new Color(.72f,.02f,.015f),1);
                int n=vh.currentVertCount;
                vh.AddVert(new Vector3(-radius*.55f,radius*.29f,0),new Color(1,1,1,.85f),Vector2.zero);
                vh.AddVert(new Vector3(radius*.5f,radius*.29f,0),new Color(1,1,1,.65f),Vector2.zero);
                vh.AddVert(new Vector3(radius*.32f,radius*.15f,0),new Color(1,1,1,0),Vector2.zero);
                vh.AddVert(new Vector3(-radius*.48f,radius*.15f,0),new Color(1,1,1,.3f),Vector2.zero);
                vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);return;
            }
            Color[] colors={new Color(1,.80f,.30f),new Color(.32f,.48f,.58f),new Color(1,.67f,.22f),new Color(.25f,.40f,.51f),new Color(1,.87f,.40f),new Color(.32f,.48f,.58f),new Color(1,.71f,.25f),new Color(.60f,.28f,.80f)};
            for(int sector=0;sector<8;sector++)for(int step=0;step<16;step++)
            {
                float a=(sector*45-22+44*step/16f)*Mathf.Deg2Rad,b=(sector*45-22+44*(step+1)/16f)*Mathf.Deg2Rad;
                int index=vh.currentVertCount;vh.AddVert(Vector3.zero,colors[sector],Vector2.zero);
                vh.AddVert(new Vector3(Mathf.Sin(a)*radius,Mathf.Cos(a)*radius,0),colors[sector],Vector2.zero);
                vh.AddVert(new Vector3(Mathf.Sin(b)*radius,Mathf.Cos(b)*radius,0),colors[sector],Vector2.zero);vh.AddTriangle(index,index+1,index+2);
            }
        }
    }
}
