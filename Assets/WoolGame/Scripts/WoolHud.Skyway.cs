using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] WoolSkywayMap skywayMap;
        public Sprite SkywayPanelSprite=>rounded;
        public void OpenSkywayLevel(int number){if(WoolProgress.IsUnlocked(number))LevelSelected?.Invoke(number);}
        public void SkywayHome()=>Navigate(2);
        public void SkywayShop()=>ShowShop();
        void ShowSkywayMap()
        {
            CloseEngagement();Hide();if(profilePanel)profilePanel.SetActive(false);Tutorial(false);
            header.gameObject.SetActive(false);footer.gameObject.SetActive(false);
            meter.gameObject.SetActive(false);progress.gameObject.SetActive(false);hint.gameObject.SetActive(false);
            if(bannerArea)bannerArea.gameObject.SetActive(false);if(abilityBackdrop)abilityBackdrop.gameObject.SetActive(false);
            menu.SetActive(true);homePage.gameObject.SetActive(false);shopPage.gameObject.SetActive(false);
            if(cardsPage)cardsPage.gameObject.SetActive(false);
            mapPage.gameObject.SetActive(true);skywayMap.gameObject.SetActive(true);
            SelectTab("Map");var nav=menu.transform.Find("Bottom navigation");if(nav)nav.gameObject.SetActive(false);
            skywayMap.OpenMap();
        }
#if UNITY_EDITOR
        public WoolSkywayMap AuthorSkyway(Sprite background,Sprite navigation,Sprite[] idle,Sprite[] walk,Sprite[] happy)
        {
            foreach(Transform child in mapPage)child.gameObject.SetActive(false);
            var go=new GameObject("Animated skyway map",typeof(RectTransform));
            go.transform.SetParent(mapPage,false);skywayMap=go.AddComponent<WoolSkywayMap>();
            skywayMap.Author(this,background,navigation,idle,walk,happy,font);
            mapPage.GetComponent<Image>().color=new Color(.30f,.73f,.96f,1);
            UnityEditor.EditorUtility.SetDirty(this);return skywayMap;
        }
#endif
    }
}
