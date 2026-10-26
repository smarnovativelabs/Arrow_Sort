#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        public void AuthorPresentationPolish()
        {
            for(int i=0;i<4;i++)
            {
                var button=footer.Find(WoolBoosters.Names[i]);var rt=(RectTransform)button;
                rt.sizeDelta=new Vector2(146,134);rt.anchoredPosition=new Vector2(-252+168*i,0);
                var image=button.GetComponent<Image>();image.sprite=rounded;image.type=Image.Type.Sliced;image.preserveAspect=false;image.color=WoolArt.Hex("FFF4DA");
                var edge=button.GetComponent<Outline>();if(!edge)edge=button.gameObject.AddComponent<Outline>();edge.enabled=true;edge.effectColor=WoolArt.Hex("C99A40");edge.effectDistance=new Vector2(2,-3);
                var icon=button.Find("Icon "+i) as RectTransform;icon.sizeDelta=new Vector2(76,76);icon.anchoredPosition=new Vector2(0,22);
                var caption=button.Find("Label").GetComponent<TMP_Text>();caption.rectTransform.anchoredPosition=new Vector2(0,-25);caption.rectTransform.sizeDelta=new Vector2(136,28);
                caption.fontSize=21;caption.color=WoolArt.Hex("294C52");caption.fontStyle=FontStyles.Bold;caption.enableAutoSizing=true;caption.fontSizeMin=18;caption.fontSizeMax=21;
                var tag=button.Find("Video option");
                if(!tag)tag=Label("Video option",button,"VIDEO / COINS",12,new Vector2(132,22),new Vector2(0,-49),WoolArt.Hex("58736F")).transform;
                var video=tag.GetComponent<TMP_Text>();video.raycastTarget=false;video.fontSize=12;video.enableAutoSizing=false;video.fontStyle=FontStyles.Normal;
            }
            foreach(var button in GetComponentsInChildren<Button>(true))
            {
                foreach(var label in button.GetComponentsInChildren<TMP_Text>(true))
                {
                    if(label.text.Length<2||label.name=="Video option")continue;
                    label.enableAutoSizing=true;label.fontSizeMax=Mathf.Max(label.fontSize,18);label.fontSizeMin=Mathf.Min(label.fontSizeMax,16);
                }
            }
            var picture=boosterPicture.GetComponent<Image>();picture.sprite=rounded;picture.type=Image.Type.Sliced;picture.preserveAspect=false;picture.color=WoolArt.Hex("F5E8C9");
            modalTitle.enableAutoSizing=true;modalTitle.fontSizeMin=24;modalTitle.fontSizeMax=38;
            modalBody.enableAutoSizing=true;modalBody.fontSizeMin=18;modalBody.fontSizeMax=26;
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
}
#endif
