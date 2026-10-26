using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    // Clip the Image mesh; imported/shared Sprite assets remain untouched.
    [RequireComponent(typeof(Image))]
    public sealed class WoolSpriteSilhouette : BaseMeshEffect
    {
        public enum Shape { Circle, Pointer, Unlock }
        public Shape shape;
        protected override void OnEnable(){base.OnEnable();Apply();}
        public void Apply()
        {
            var image=GetComponent<Image>();if(!image)return;
            image.type=Image.Type.Simple;image.useSpriteMesh=false;image.SetVerticesDirty();
        }
        public override void ModifyMesh(VertexHelper vh)
        {
            if(!IsActive()||vh.currentVertCount<4||shape==Shape.Unlock)return;
            UIVertex sample=default;vh.PopulateUIVertex(ref sample,0);
            float x0=sample.position.x,x1=x0,y0=sample.position.y,y1=y0;
            float u0=sample.uv0.x,u1=u0,v0=sample.uv0.y,v1=v0;
            for(int i=1;i<vh.currentVertCount;i++){
                UIVertex v=default;vh.PopulateUIVertex(ref v,i);
                x0=Mathf.Min(x0,v.position.x);x1=Mathf.Max(x1,v.position.x);y0=Mathf.Min(y0,v.position.y);y1=Mathf.Max(y1,v.position.y);
                u0=Mathf.Min(u0,v.uv0.x);u1=Mathf.Max(u1,v.uv0.x);v0=Mathf.Min(v0,v.uv0.y);v1=Mathf.Max(v1,v.uv0.y);
            }
            vh.Clear();
            if(shape==Shape.Circle){
                Add(vh,sample,.5f,.5f,x0,x1,y0,y1,u0,u1,v0,v1);
                for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;Add(vh,sample,.5f+Mathf.Cos(a)*.485f,.5f+Mathf.Sin(a)*.485f,x0,x1,y0,y1,u0,u1,v0,v1);}
                for(int i=0;i<64;i++)vh.AddTriangle(0,i+1,(i+1)%64+1);
            }else{
                Add(vh,sample,.025f,.905f,x0,x1,y0,y1,u0,u1,v0,v1);Add(vh,sample,.975f,.905f,x0,x1,y0,y1,u0,u1,v0,v1);
                Add(vh,sample,.95f,.69f,x0,x1,y0,y1,u0,u1,v0,v1);Add(vh,sample,.5875f,.036f,x0,x1,y0,y1,u0,u1,v0,v1);
                Add(vh,sample,.4125f,.036f,x0,x1,y0,y1,u0,u1,v0,v1);Add(vh,sample,.05f,.69f,x0,x1,y0,y1,u0,u1,v0,v1);
                for(int i=1;i<5;i++)vh.AddTriangle(0,i,i+1);
            }
        }
        static void Add(VertexHelper vh,UIVertex vertex,float x,float y,float x0,float x1,float y0,float y1,float u0,float u1,float v0,float v1)
        {
            vertex.position=new Vector3(Mathf.Lerp(x0,x1,x),Mathf.Lerp(y0,y1,y),vertex.position.z);
            vertex.uv0=new Vector4(Mathf.Lerp(u0,u1,x),Mathf.Lerp(v0,v1,y),0,0);vh.AddVert(vertex);
        }
    }
}
