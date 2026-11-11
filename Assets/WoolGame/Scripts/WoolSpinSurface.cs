using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    // Native vector surfaces stay sharp when the portrait canvas scales.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoolSpinSurface : MaskableGraphic
    {
        public enum Kind { Banner, CreamCard, GoldRing, PrizeDisk, ReferencePanel, Sunburst, Hub, SpinButton, DisabledButton, ProgressFill, Pointer, TutorialTeal }
        public Kind kind;
        static Color Hex(string s)=>WoolArt.Hex(s);
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();float w=rectTransform.rect.width,h=rectTransform.rect.height;
            if(kind==Kind.TutorialTeal){
                bool green=false;float radius=Mathf.Min(h*.48f,80);
                Rounded(vh,w,h,radius,green?Hex("267600"):Hex("123D46"),green?Hex("155000"):Hex("082C35"),new Vector2(0,-7));
                Rounded(vh,w,h,radius,green?Hex("D8FF91"):Hex("E7FFFF"),green?Hex("387A04"):Hex("52838A"));
                Rounded(vh,w-8,h-8,radius-4,green?Hex("79EC00"):Hex("337581"),green?Hex("24A000"):Hex("063C49"));
                Rounded(vh,w-30,h*.29f,Mathf.Min(28,h*.14f),new Color(1,1,1,.25f),new Color(1,1,1,.015f),new Vector2(0,h*.27f));return;
            }
            if(kind==Kind.Pointer){
                Triangle(vh,w,h,Hex("B84C00"),new Vector2(0,-4));
                Triangle(vh,w-5,h-5,Hex("FFD973"),Vector2.zero);
                Triangle(vh,w-16,h-17,Hex("FF7C27"),new Vector2(0,1));
                Triangle(vh,w-25,h-26,Hex("F32918"),new Vector2(0,2));return;
            }
            if(kind==Kind.ProgressFill){Rounded(vh,w,h,22,Hex("85F000"),Hex("259A00"));Rounded(vh,w-8,h-8,18,Hex("9EFF21"),Hex("36B800"));return;}
            if(kind==Kind.DisabledButton){
                Rounded(vh,w,h,44,Hex("737379"),Hex("626268"),new Vector2(0,-6));
                Rounded(vh,w,h,44,Hex("ECECEE"),Hex("B5B5BA"));
                Rounded(vh,w-8,h-8,40,Hex("BDBDC2"),Hex("929298"));
                Rounded(vh,w-26,4,2,new Color(1,1,1,.60f),new Color(1,1,1,.25f),new Vector2(0,h*.36f));return;
            }
            if(kind==Kind.SpinButton){
                Rounded(vh,w,h,48,Hex("467E00"),Hex("286000"),new Vector2(0,-8));
                Rounded(vh,w,h,48,Hex("C4FF52"),Hex("75B600"));
                Rounded(vh,w-12,h-12,43,Hex("9DE500"),Hex("43A500"));
                Rounded(vh,w-26,h-24,36,Hex("8AE000"),Hex("39A000"));
                Rounded(vh,w-72,12,6,new Color(1,1,1,.55f),new Color(1,1,1,.12f),new Vector2(0,h*.36f));
                int n=vh.currentVertCount;float x=-w*.28f;
                vh.AddVert(new Vector2(x-18,-27),Color.white,Vector2.zero);vh.AddVert(new Vector2(x-18,27),Color.white,Vector2.zero);vh.AddVert(new Vector2(x+26,0),Color.white,Vector2.zero);vh.AddTriangle(n,n+1,n+2);return;
            }
            if(kind==Kind.Sunburst){
                float radius=Mathf.Min(w,h)*.5f;for(int i=0;i<32;i++){
                    float a=i*Mathf.PI/16,b=a+Mathf.PI/32;int n=vh.currentVertCount;
                    vh.AddVert(Vector2.zero,new Color(1,.72f,.16f,.35f),Vector2.zero);
                    vh.AddVert(new Vector2(Mathf.Sin(a),Mathf.Cos(a))*radius,new Color(1,.72f,.16f,0),Vector2.zero);
                    vh.AddVert(new Vector2(Mathf.Sin(b),Mathf.Cos(b))*radius,new Color(1,.72f,.16f,0),Vector2.zero);vh.AddTriangle(n,n+1,n+2);
                }return;
            }
            if(kind==Kind.Hub){float r=Mathf.Min(w,h)*.5f;Circle(vh,r,Hex("BD6500"));Circle(vh,r-5,Hex("FFF27C"));Circle(vh,r-13,Hex("FFB900"));return;}
            if(kind==Kind.ReferencePanel){
                Rounded(vh,w,h,90,Hex("FF9D37"),Hex("F6650D"));
                Rounded(vh,w-18,h-18,82,Hex("FFF8CD"),Hex("FFE6A5"));
                Rounded(vh,w-30,h-30,75,Hex("F89525"),Hex("FF9D37"));
                Rounded(vh,w-40,h-40,70,Hex("FFF0CB"),Hex("FFE8BA"));return;
            }
            if(kind==Kind.GoldRing)
            {
                float r=Mathf.Min(w,h)*.5f;
                Circle(vh,r,new Color(.15f,.045f,0,.45f),new Vector2(0,-10));
                Circle(vh,r,Hex("B85D00"));
                Circle(vh,r-3,Hex("FFCC18"));Circle(vh,r-6,Hex("FFF4A0"));
                Circle(vh,r-10,Hex("FFD72D"));Circle(vh,r-15,Hex("FFBF00"));
                Circle(vh,r-21,Hex("FFAF00"));Circle(vh,r-27,Hex("F79700"));
                Circle(vh,r-32,Hex("E78000"));Circle(vh,r-36,Hex("FFDC53"));
                Circle(vh,r-39,Hex("FFF1A0"));Circle(vh,r-42,Hex("C76A00"));
                return;
            }
            if(kind==Kind.PrizeDisk)
            {
                float r=Mathf.Min(w,h)*.5f;
                Circle(vh,r,Hex("D8B56B"));
                for(int sector=0;sector<8;sector++)
                {
                    Color outer=sector==0?Hex("C065FF"):sector%2==0?Hex("FFF5DB"):Hex("0865BB");
                    Color inner=sector==0?Hex("7E2BD0"):sector%2==0?Hex("FFD88C"):Hex("03488D");
                    for(int step=0;step<32;step++)
                    {
                        float a=(sector*45-22.5f+45f*step/32)*Mathf.Deg2Rad,b=(sector*45-22.5f+45f*(step+1)/32)*Mathf.Deg2Rad;
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
        static void Triangle(VertexHelper vh,float w,float h,Color col,Vector2 offset)
        {
            Vector2[] points={new Vector2(-w*.40f,h*.43f),new Vector2(w*.40f,h*.43f),new Vector2(w*.48f,h*.30f),new Vector2(w*.065f,-h*.46f),new Vector2(-w*.065f,-h*.46f),new Vector2(-w*.48f,h*.30f)};
            int n=vh.currentVertCount;vh.AddVert(offset,col,Vector2.zero);
            for(int i=0;i<points.Length;i++){var c=Color.Lerp(col,Color.white,Mathf.Max(0,points[i].y/h)*.3f);vh.AddVert(points[i]+offset,c,Vector2.zero);}
            for(int i=0;i<points.Length;i++)vh.AddTriangle(n,n+1+i,n+1+(i+1)%points.Length);
        }
        static void Circle(VertexHelper vh,float radius,Color c,Vector2 offset=default)
        {
            int n=vh.currentVertCount;vh.AddVert(offset,c,Vector2.zero);
            for(int i=0;i<=128;i++){float a=i*Mathf.PI*2/128;var tint=c;
                if(c.a>.9f){float light=Mathf.Sin(a)*.10f+Mathf.Cos(a)*.035f;tint=light>0?Color.Lerp(c,Color.white,light):Color.Lerp(c,Hex("9C4500"),-light);}
                vh.AddVert(offset+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,tint,Vector2.zero);if(i>0)vh.AddTriangle(n,n+i,n+i+1);}
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
