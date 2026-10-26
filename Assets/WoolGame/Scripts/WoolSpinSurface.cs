using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    // Native vector surfaces stay sharp when the portrait canvas scales.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoolSpinSurface : MaskableGraphic
    {
        public enum Kind { Banner, CreamCard, GoldRing, PrizeDisk }
        public Kind kind;
        static Color Hex(string s)=>WoolArt.Hex(s);
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();float w=rectTransform.rect.width,h=rectTransform.rect.height;
            if(kind==Kind.GoldRing)
            {
                float r=Mathf.Min(w,h)*.5f;
                Circle(vh,r,new Color(.15f,.045f,0,.45f),new Vector2(0,-10));
                Circle(vh,r,Hex("BD5605"));Circle(vh,r-6,Hex("FFE9A1"));
                Circle(vh,r-13,Hex("FFB916"));Circle(vh,r-28,Hex("F48B08"));Circle(vh,r-34,Hex("FFF4B9"));
                return;
            }
            if(kind==Kind.PrizeDisk)
            {
                float r=Mathf.Min(w,h)*.5f;
                Circle(vh,r,Hex("D8B56B"));
                for(int sector=0;sector<8;sector++)
                {
                    Color outer=sector==0?Hex("C065FF"):sector%2==0?Hex("FFF5DB"):Hex("24699C");
                    Color inner=sector==0?Hex("7E2BD0"):sector%2==0?Hex("FFD88C"):Hex("123B66");
                    for(int step=0;step<32;step++)
                    {
                        float a=(sector*45-22.35f+44.7f*step/32)*Mathf.Deg2Rad,b=(sector*45-22.35f+44.7f*(step+1)/32)*Mathf.Deg2Rad;
                        int n=vh.currentVertCount;vh.AddVert(Vector3.zero,inner,Vector2.zero);
                        vh.AddVert(new Vector3(Mathf.Sin(a)*r,Mathf.Cos(a)*r),outer,Vector2.zero);
                        vh.AddVert(new Vector3(Mathf.Sin(b)*r,Mathf.Cos(b)*r),outer,Vector2.zero);vh.AddTriangle(n,n+1,n+2);
                    }
                }
                return;
            }
            Rounded(vh,w,h,48,Hex("A94D04"),Hex("A94D04"),new Vector2(0,-7));
            Rounded(vh,w,h,48,Hex("FFF3AF"),Hex("F9AC12"));
            Rounded(vh,w-10,h-10,43,Hex("FFD75B"),Hex("E48109"));
            Rounded(vh,w-25,h-25,36,kind==Kind.Banner?Hex("FFB52D"):Hex("FFF8E8"),kind==Kind.Banner?Hex("F58B11"):Hex("FFE6BB"));
        }
        static void Circle(VertexHelper vh,float radius,Color c,Vector2 offset=default)
        {
            int n=vh.currentVertCount;vh.AddVert(offset,c,Vector2.zero);
            for(int i=0;i<=128;i++){float a=i*Mathf.PI*2/128;vh.AddVert(offset+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,c,Vector2.zero);if(i>0)vh.AddTriangle(n,n+i,n+i+1);}
        }
        static void Rounded(VertexHelper vh,float w,float h,float radius,Color top,Color bottom,Vector2 offset=default)
        {
            float r=Mathf.Min(radius,Mathf.Min(w,h)/2);int n=vh.currentVertCount;vh.AddVert(offset,Color.Lerp(bottom,top,.5f),Vector2.zero);
            for(int corner=0;corner<4;corner++)for(int step=0;step<=16;step++)
            {
                float a=(corner*90+step*90f/16)*Mathf.Deg2Rad;
                Vector2 center=new Vector2(corner==0||corner==3?w/2-r:-w/2+r,corner<2?h/2-r:-h/2+r);
                var p=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r;
                vh.AddVert(p+offset,Color.Lerp(bottom,top,(p.y+h/2)/h),Vector2.zero);
                int i=corner*17+step;if(i>0)vh.AddTriangle(n,n+i,n+i+1);
            }
            vh.AddTriangle(n,n+68,n+1);
        }
    }
}
