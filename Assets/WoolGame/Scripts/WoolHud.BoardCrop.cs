using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] RectTransform boardCropLeft,boardCropRight;
        public void ConfigureBoardCrop()
        {
            if(!boardCropLeft)boardCropLeft=CreateBoardCurtain("Board left margin");
            if(!boardCropRight)boardCropRight=CreateBoardCurtain("Board right margin");
        }
        RectTransform CreateBoardCurtain(string name)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            var rt=(RectTransform)go.transform;rt.SetParent(transform,false);rt.SetAsFirstSibling();
            var image=go.GetComponent<Image>();image.color=WoolArt.Hex("F5F1E8");image.raycastTarget=false;
            return rt;
        }
        public void ApplyBoardCrop(float left,float right,float top,int width,int height)
        {
            if(!boardCropLeft||!boardCropRight)return;
            // Full-screen environment replaces the legacy cream side curtains.
            bool visible=false;
            boardCropLeft.gameObject.SetActive(visible);boardCropRight.gameObject.SetActive(visible);
            if(!visible)return;
            boardCropLeft.anchorMin=Vector2.zero;boardCropLeft.anchorMax=new Vector2(left,top);
            boardCropRight.anchorMin=new Vector2(right,0);boardCropRight.anchorMax=new Vector2(1,top);
            boardCropLeft.offsetMin=boardCropLeft.offsetMax=Vector2.zero;
            boardCropRight.offsetMin=boardCropRight.offsetMax=Vector2.zero;
            if(footer)
            {
                float scale=PortraitScale(width,height);
                footer.localScale=Vector3.one*Mathf.Min(1f,((right-left)*width/scale-24)/680f);
            }
        }
    }
}
