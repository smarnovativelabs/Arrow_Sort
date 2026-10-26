using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WoolMapNodeGraphic:MaskableGraphic
    {
        public Color face=new Color(.4f,.8f,.2f,1);
        public bool ring;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;
            if(ring){Ring(vh,r.center,r.width*.48f,r.height*.46f,r.width*.40f,r.height*.38f,color);return;}
            Ellipse(vh,r.center+Vector2.down*r.height*.10f,r.width*.48f,r.height*.32f,new Color(.2f,.09f,.02f,.18f));
            Ellipse(vh,r.center+Vector2.down*r.height*.04f,r.width*.47f,r.height*.31f,new Color(.90f,.45f,.02f,1)*color);
            Ellipse(vh,r.center+Vector2.up*r.height*.02f,r.width*.46f,r.height*.30f,new Color(1,.78f,.20f,1)*color);
            Ellipse(vh,r.center+Vector2.up*r.height*.07f,r.width*.41f,r.height*.265f,new Color(.95f,.97f,1,1)*color);
            Ellipse(vh,r.center+Vector2.up*r.height*.09f,r.width*.385f,r.height*.235f,face*color);
            Ring(vh,r.center+Vector2.up*r.height*.095f,r.width*.375f,r.height*.225f,r.width*.35f,r.height*.20f,new Color(1,1,1,.23f)*color);
        }
        static void Ellipse(VertexHelper vh,Vector2 c,float rx,float ry,Color col)
        {
            int start=vh.currentVertCount;vh.AddVert(c,col,Vector2.zero);
            for(int i=0;i<=48;i++){float a=i*Mathf.PI*2/48;vh.AddVert(c+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry),col,Vector2.zero);if(i>0)vh.AddTriangle(start,start+i,start+i+1);}
        }
        static void Ring(VertexHelper vh,Vector2 c,float rx,float ry,float ix,float iy,Color col)
        {
            int start=vh.currentVertCount;
            for(int i=0;i<=48;i++){float a=i*Mathf.PI*2/48;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));vh.AddVert(c+Vector2.Scale(d,new Vector2(rx,ry)),col,Vector2.zero);vh.AddVert(c+Vector2.Scale(d,new Vector2(ix,iy)),col,Vector2.zero);if(i>0){int n=start+i*2;vh.AddTriangle(n-2,n,n-1);vh.AddTriangle(n,n+1,n-1);}}
        }
    }
}
