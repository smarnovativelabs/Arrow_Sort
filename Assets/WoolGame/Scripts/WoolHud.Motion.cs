using UnityEngine;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        public bool AdVisible { get; private set; }
        public void SetAdVisible(bool visible){AdVisible=visible;if(canvas)canvas.enabled=!visible;if(gameAudio)gameAudio.SetAdVisible(visible);}
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
