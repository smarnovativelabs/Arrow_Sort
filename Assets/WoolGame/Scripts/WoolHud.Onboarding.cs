using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        public WoolWinPresentation TutorialWin=>winPresentation;
        public WoolSkywayMap TutorialMap=>skywayMap;
        public WoolDailyTasks TutorialTasks=>dailyTasksView;
        public WoolLuckySpin TutorialSpin=>luckySpin;
        public WoolProfileView TutorialProfile=>profileView;
        public WoolShopView TutorialShop=>shopPage.GetComponent<WoolShopView>();
        public Button TutorialHomeTab=>menuNavigation.tabs[1];
        public Button TutorialTasksButton=>referenceHome.GetComponentsInChildren<Button>(true)[0];
        public Button FindHomeTutorialButton(string buttonName)
        {
            foreach(var b in referenceHome.GetComponentsInChildren<Button>(true))if(b.name==buttonName)return b;
            return null;
        }
        public void ShowTutorialProfile(){CloseEngagement();ShowProfile();}
    }
}
