using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] Image dangerBorder;
        public void SetDanger(float intensity,float time)
        {
            if(!dangerBorder)return;
            dangerBorder.gameObject.SetActive(intensity>.01f);
            dangerBorder.color=new Color(1,1,1,Mathf.Clamp01(intensity)*(.22f+.42f*(.5f+.5f*Mathf.Sin(time*4))));
        }
#if UNITY_EDITOR
        public void AuthorDangerBorder(Sprite sprite)
        {
            if(!dangerBorder)
            {
                var rect=Rect("Kitten danger border",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
                dangerBorder=rect.gameObject.AddComponent<Image>();dangerBorder.raycastTarget=false;
            }
            dangerBorder.sprite=sprite;dangerBorder.type=Image.Type.Simple;
            dangerBorder.transform.SetAsLastSibling();dangerBorder.gameObject.SetActive(false);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
