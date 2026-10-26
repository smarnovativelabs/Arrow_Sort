using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;

namespace WoolGame
{
    [RequireComponent(typeof(Image))]
    public sealed class WoolHomeFlame : BaseMeshEffect
    {
        float clock;
        public float FlowTime=>clock;
        void Update(){clock+=Time.unscaledDeltaTime;if(graphic)graphic.SetVerticesDirty();}
        public override void ModifyMesh(VertexHelper vh)
        {
            if(!IsActive())return;
            var image=graphic as Image;if(!image||!image.sprite)return;
            var rect=graphic.rectTransform.rect;var uv=DataUtility.GetOuterUV(image.sprite);
            vh.Clear();const int steps=14;
            for(int y=0;y<=steps;y++)for(int x=0;x<=steps;x++)
            {
                float u=(float)x/steps,v=(float)y/steps;
                // Mouth is the top-right corner. The free flame tip ripples downstream.
                float travel=(2-u-v)*.5f;
                float ripple=Mathf.Sin(travel*20-clock*17)*5*travel;
                var color=graphic.color;color.a*=.86f+.14f*Mathf.Sin(clock*12+travel*15);
                var p=new Vector3(Mathf.Lerp(rect.xMin,rect.xMax,u)+ripple,Mathf.Lerp(rect.yMin,rect.yMax,v)-ripple,0);
                vh.AddVert(p,color,new Vector2(Mathf.Lerp(uv.x,uv.z,u),Mathf.Lerp(uv.y,uv.w,v)));
            }
            for(int y=0;y<steps;y++)for(int x=0;x<steps;x++)
            {int n=y*(steps+1)+x;vh.AddTriangle(n,n+steps+1,n+1);vh.AddTriangle(n+1,n+steps+1,n+steps+2);}
        }
    }
}
