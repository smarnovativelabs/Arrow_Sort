using UnityEngine;
using UnityEngine.UI;

namespace WoolGame
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoolTouchFeedback : MaskableGraphic
    {
        struct Stamp { public Vector2 point; public float born, angle; public bool impact; }
        readonly Stamp[] stamps = new Stamp[24];
        int cursor;
        bool hadLive;
        public int TapCount { get; private set; }
        public int ImpactCount { get; private set; }
        public void ClearFeedback(){for(int i=0;i<stamps.Length;i++)stamps[i].born=-100;hadLive=false;SetVerticesDirty();}
        static readonly Vector2[] foot = RoundedFoot();

        static Vector2[] RoundedFoot()
        {
            // Three rounded toes joined by duck webbing, with a small rounded heel.
            var corners = new[] { new Vector2(-9,-27),new Vector2(-13,-11),new Vector2(-35,16),new Vector2(-32,29),new Vector2(-22,30),new Vector2(-11,20),new Vector2(-6,37),new Vector2(0,41),new Vector2(6,37),new Vector2(11,20),new Vector2(22,30),new Vector2(32,29),new Vector2(35,16),new Vector2(13,-11),new Vector2(9,-27),new Vector2(0,-31) };
            var points = new Vector2[corners.Length * 5];
            for (int i=0;i<corners.Length;i++)
            {
                var a=Vector2.Lerp(corners[i],corners[(i+corners.Length-1)%corners.Length],.3f);
                var b=corners[i];var c=Vector2.Lerp(b,corners[(i+1)%corners.Length],.3f);
                for(int k=0;k<5;k++){float t=k/4f;points[i*5+k]=(1-t)*(1-t)*a+2*(1-t)*t*b+t*t*c;}
            }
            return points;
        }
        protected override void Awake()
        {
            base.Awake();raycastTarget=false;
            for(int i=0;i<stamps.Length;i++)stamps[i].born=-100;
        }
        public void SpawnScreen(Vector2 screen,bool impact=false)
        {
            var camera=canvas&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,screen,camera,out var point);
            stamps[cursor]=new Stamp{point=point,born=Time.unscaledTime,angle=impact?0:(TapCount%2==0?-10:10),impact=impact};
            cursor=(cursor+1)%stamps.Length;if(impact)ImpactCount++;else TapCount++;SetVerticesDirty();
        }
        void Update()
        {
            bool live=false;foreach(var s in stamps)if(Time.unscaledTime-s.born<.6f){live=true;break;}
            // Clear the final faded mesh too, so no faint footprint remains on screen.
            if(live||hadLive)SetVerticesDirty();hadLive=live;
        }
        void LateUpdate(){rectTransform.anchorMin=Vector2.zero;rectTransform.anchorMax=Vector2.one;rectTransform.offsetMin=rectTransform.offsetMax=Vector2.zero;}
        void Triangle(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Color color)
        {
            int i=vh.currentVertCount;vh.AddVert(a,color,Vector2.zero);vh.AddVert(b,color,Vector2.zero);vh.AddVert(c,color,Vector2.zero);vh.AddTriangle(i,i+1,i+2);
        }
        Vector2 Point(Stamp s,Vector2 p,float scale)
        {
            float a=s.angle*Mathf.Deg2Rad;return s.point+new Vector2(p.x*Mathf.Cos(a)-p.y*Mathf.Sin(a),p.x*Mathf.Sin(a)+p.y*Mathf.Cos(a))*scale;
        }
        void FootRing(VertexHelper vh,Stamp s,float inside,float outside,float innerAlpha,float outerAlpha)
        {
            for(int n=0;n<foot.Length;n++)
            {
                int next=(n+1)%foot.Length,i=vh.currentVertCount;
                vh.AddVert(Point(s,foot[n],inside),new Color(1,1,1,innerAlpha),Vector2.zero);
                vh.AddVert(Point(s,foot[next],inside),new Color(1,1,1,innerAlpha),Vector2.zero);
                vh.AddVert(Point(s,foot[next],outside),new Color(1,1,1,outerAlpha),Vector2.zero);
                vh.AddVert(Point(s,foot[n],outside),new Color(1,1,1,outerAlpha),Vector2.zero);
                vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);
            }
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            foreach(var s in stamps)
            {
                float age=Time.unscaledTime-s.born;if(age<0||age>=.6f)continue;
                float alpha=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.14f,.6f,age));
                if(!s.impact)
                {
                    float size=Mathf.Lerp(.65f,1.2f,Mathf.SmoothStep(0,1,age/.09f));
                    for(int n=0;n<foot.Length;n++)Triangle(vh,s.point,Point(s,foot[n],size*.88f),Point(s,foot[(n+1)%foot.Length],size*.88f),new Color(1,1,1,alpha*.20f));
                    FootRing(vh,s,size*.88f,size*.96f,alpha*.20f,alpha*.82f);
                    FootRing(vh,s,size*.96f,size*1.04f,alpha*.82f,alpha*.14f);
                    FootRing(vh,s,size*1.04f,size*1.22f,alpha*.14f,0);
                    continue;
                }
                float p=age/.6f,r=Mathf.Lerp(7,64,p);
                for(int n=0;n<12;n++)
                {
                    float a=n*Mathf.PI/6,b=(n+1)*Mathf.PI/6;
                    var inner=s.point+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(r-3);
                    var outer=s.point+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r;
                    var next=s.point+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*r;
                    Triangle(vh,inner,outer,next,new Color(1,.82f,.22f,alpha*(1-p)));
                }
                for(int n=0;n<8;n++)
                {
                    float a=n*Mathf.PI/4;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));var side=new Vector2(-d.y,d.x);
                    float center=Mathf.Lerp(10,66,p),length=18*(1-p);
                    Triangle(vh,s.point+d*(center+length),s.point+d*center+side*4*(1-p),s.point+d*center-side*4*(1-p),new Color(1,.92f,.55f,alpha));
                }
                float z=20*(1-p);
                Triangle(vh,s.point+Vector2.up*z,s.point+Vector2.left*z*.4f,s.point+Vector2.down*z,new Color(1,1,1,alpha));
                Triangle(vh,s.point+Vector2.left*z,s.point+Vector2.up*z*.4f,s.point+Vector2.right*z,new Color(1,1,.85f,alpha));
            }
        }
    }
    public sealed partial class WoolHud
    {
        WoolTouchFeedback touchFeedback;
        public void EnsureTouchFeedback()
        {
            if(touchFeedback)return;
            var root=Rect("Duck footsteps and vehicle contact sparks",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            var layer=root.gameObject.AddComponent<Canvas>();layer.overrideSorting=true;layer.sortingOrder=1000;
            touchFeedback=root.gameObject.AddComponent<WoolTouchFeedback>();touchFeedback.raycastTarget=false;
        }
        public void PlayVehicleContact(Vector2 screen){EnsureTouchFeedback();touchFeedback.SpawnScreen(screen,true);}
        public void PlayVehicleTap(Vector2 screen){EnsureTouchFeedback();touchFeedback.SpawnScreen(screen);}
        public void ClearVehicleFeedback(){if(touchFeedback)touchFeedback.ClearFeedback();}
    }
}
