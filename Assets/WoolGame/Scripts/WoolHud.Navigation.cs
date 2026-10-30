using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] WoolMenuNavigation menuNavigation;
        void FitUnifiedNavigation()
        {
            if(!menuNavigation)return;
            var old=menu.transform.Find("Bottom navigation");if(old)old.gameObject.SetActive(false);
            var homeMotion=referenceHome?referenceHome.GetComponent<WoolHomePresentation>():null;
            if(homeMotion&&homeMotion.navigation)homeMotion.navigation.gameObject.SetActive(false);
            if(skywayMap&&skywayMap.navigation)skywayMap.navigation.gameObject.SetActive(false);
            menuNavigation.SetVisible(!walletShopOpen&&menu.activeSelf&&(homePage.gameObject.activeSelf||shopPage.gameObject.activeSelf||(mapPage&&mapPage.gameObject.activeSelf)));
        }
#if UNITY_EDITOR
        public string AuthorUnifiedNavigation(Sprite[] states,Sprite headerFrame)
        {
            var old=menu.transform.Find("Bottom navigation");if(old)old.gameObject.SetActive(false);
            var homeMotion=referenceHome.GetComponent<WoolHomePresentation>();if(homeMotion.navigation)homeMotion.navigation.gameObject.SetActive(false);
            if(skywayMap.navigation)skywayMap.navigation.gameObject.SetActive(false);
            var go=new GameObject("Unified bottom navigation",typeof(RectTransform));go.transform.SetParent(menu.transform,false);
            menuNavigation=go.AddComponent<WoolMenuNavigation>();
            menuNavigation.Author(this,states,shopPage.Find("Shop viewport") as RectTransform);
            var caption=skywayMap.pageLabel.transform.parent.GetComponent<Image>();caption.sprite=headerFrame;caption.type=Image.Type.Sliced;caption.color=Color.white;
            foreach(var button in new[]{skywayMap.previous,skywayMap.next})
            {
                var image=button.GetComponent<Image>();image.sprite=headerFrame;image.type=Image.Type.Sliced;image.color=Color.white;
                var colors=button.colors;colors.normalColor=colors.highlightedColor=Color.white;colors.pressedColor=new Color(.9f,.93f,.95f,1);button.colors=colors;button.transition=Selectable.Transition.ColorTint;
                var label=button.GetComponentInChildren<TMPro.TMP_Text>();label.color=WoolArt.Hex("63310B");label.fontSize=48;
            }
            skywayMap.pageLabel.color=WoolArt.Hex("63310B");skywayMap.status.color=WoolArt.Hex("8B5427");
            skywayMap.pageLabel.enableAutoSizing=skywayMap.status.enableAutoSizing=true;
            skywayMap.pageLabel.fontSizeMax=30;skywayMap.pageLabel.fontSizeMin=20;
            skywayMap.status.fontSizeMax=19;skywayMap.status.fontSizeMin=14;
            var a=skywayMap.pageLabel.rectTransform;a.anchorMin=new Vector2(0,.45f);a.anchorMax=new Vector2(1,.94f);a.offsetMin=new Vector2(16,0);a.offsetMax=new Vector2(-16,0);
            a=skywayMap.status.rectTransform;a.anchorMin=new Vector2(0,.1f);a.anchorMax=new Vector2(1,.43f);a.offsetMin=new Vector2(16,0);a.offsetMax=new Vector2(-16,0);
            ConfigureInputTargets();FitUnifiedNavigation();menuNavigation.Select("Home");
            UnityEditor.EditorUtility.SetDirty(this);UnityEditor.EditorUtility.SetDirty(skywayMap);
            return "One shared navigation bar authored for Home, Shop and Map.";
        }
#endif
    }
}
