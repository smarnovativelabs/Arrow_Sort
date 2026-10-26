using UnityEngine;
namespace WoolGame
{
    public sealed class WoolCoverArtwork : MonoBehaviour
    {
        UnityEngine.UI.Image image;Vector2 size;Vector3 parentScale;
        void OnEnable(){image=GetComponent<UnityEngine.UI.Image>();size=Vector2.zero;}
        void LateUpdate()
        {
            if(!image||!image.sprite)return;
            var canvas=GetComponentInParent<Canvas>();if(!canvas)return;
            var root=(RectTransform)canvas.rootCanvas.transform;
            if(size==root.rect.size&&parentScale==transform.parent.lossyScale)return;
            size=root.rect.size;parentScale=transform.parent.lossyScale;
            WoolCanvasArt.Cover((RectTransform)transform,image.sprite.rect.width/image.sprite.rect.height);
        }
    }
}
