using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        public bool AdVisible { get; private set; }
        bool adDisplayed,adLoading;
        GameObject adLoadingOverlay;
        public void SetAdVisible(bool visible){adDisplayed=visible;RefreshAdState();}
        public void SetAdLoading(bool loading)
        {
            adLoading=loading;
            if(loading&&!adLoadingOverlay&&canvas)
            {
                var root=Rect("Loading reward video",canvas.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
                var dim=root.gameObject.AddComponent<Image>();dim.color=new Color(0,0,0,.6f);dim.raycastTarget=true;
                var panel=Panel("Video loading card",root,new Vector2(.5f,.5f),new Vector2(520,180),Vector2.zero,Color.white);
                var face=panel.GetComponent<Image>();face.sprite=Resources.Load<Sprite>("WoolButtons/Card");face.type=Image.Type.Sliced;face.raycastTarget=false;
                var caption=Label("Loading video caption",panel,"Loading video...",38,new Vector2(480,140),Vector2.zero,new Color(.13f,.23f,.22f));caption.raycastTarget=false;
                adLoadingOverlay=root.gameObject;
            }
            if(adLoadingOverlay){adLoadingOverlay.SetActive(loading);if(loading)adLoadingOverlay.transform.SetAsLastSibling();}
            RefreshAdState();
        }
        void RefreshAdState(){AdVisible=adDisplayed||adLoading;if(canvas)canvas.enabled=!adDisplayed;if(gameAudio)gameAudio.SetAdVisible(AdVisible);}
        void BindMotion()
        {
            AddMotion(modalCard);
            if(dailyPanel)AddMotion(dailyPanel.transform.Find("Daily card"));
            if(spinPanel)AddMotion(spinPanel.transform.Find("Spin card"));
            if(profilePanel)AddMotion(profilePanel.transform.Find("Profile card"));
            AddMotion(homePage);AddMotion(shopPage);AddMotion(cardsPage);AddMotion(mapPage);
        }
        static void AddMotion(Transform target)
        {
            if(target&&!target.GetComponent<WoolPanelMotion>())target.gameObject.AddComponent<WoolPanelMotion>();
        }
    }
}
