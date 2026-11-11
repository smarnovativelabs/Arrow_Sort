using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Coffee.UIEffects;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] RectTransform referenceHome;
        void FitReferencePages()
        {
            var animatedHome=referenceHome?referenceHome.GetComponent<WoolHomePresentation>():null;
            if(animatedHome)animatedHome.Fit();else FitReference(referenceHome,homePage,0);
            var nav=menu?menu.transform.Find("Bottom navigation"):null;
            if(menuNavigation)FitUnifiedNavigation();else if(nav&&referenceHome)nav.gameObject.SetActive(!homePage.gameObject.activeSelf&&!(skywayMap&&skywayMap.gameObject.activeInHierarchy));
        }
        static void FitReference(RectTransform page,RectTransform parent,float margin)
        {
            if(!page||!parent)return;
            page.localScale=Vector3.one*Mathf.Min((parent.rect.width-margin)/941f,(parent.rect.height-margin)/1672f);
        }
        void BindReferencePages()
        {
            if(!referenceHome)return;
            var top=referenceHome.Find("Header controls")??referenceHome;
            var actions=referenceHome.Find("Action cards")??referenceHome;
            var levelArea=referenceHome.Find("Level area")??referenceHome;
            var navigation=referenceHome.Find("Navigation")??referenceHome;
            BindNav(top,"Profile",ShowProfile);BindNav(top,"Settings",ShowMenuSettings);
            BindNav(top,"Wallet",ShowCoinOptions);
            BindNav(levelArea,"Play",()=>PlayPressed?.Invoke());BindNav(actions,"Tasks",ShowDailyTasks);
            BindNav(actions,"Lucky Spin",ShowDailySpin);BindNav(actions,"Rescue Shop",ShowSpecialOffer);BindNav(actions,"Special Offer",ShowSpecialOffer);
            BindNav(actions,"Ad Pass",ShowNoAdsStatus);var removeAds=actions.Find("Ad Pass");if(removeAds)foreach(var text in removeAds.GetComponentsInChildren<TMP_Text>(true))if((text.text.Replace("\n"," ").Trim().Equals("Ad Pass",StringComparison.OrdinalIgnoreCase)||text.text.Replace("\n"," ").Trim().Equals("Remove Ads",StringComparison.OrdinalIgnoreCase)))text.text="REMOVE\nADS";BindNav(navigation,"Shop",ShowShop);
            BindNav(navigation,"Home",()=>Navigate(2));BindNav(navigation,"Map",ShowMap);
            FitReferencePages();
        }
#if UNITY_EDITOR
        RectTransform ReferenceRoot(string name,Transform parent,Sprite sprite)
        {
            var existing=parent.Find(name) as RectTransform;
            var rt=existing?existing:Rect(name,parent,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(941,1672),Vector2.zero);
            rt.sizeDelta=new Vector2(941,1672);rt.anchoredPosition=Vector2.zero;
            var img=rt.GetComponent<Image>();if(!img)img=rt.gameObject.AddComponent<Image>();img.sprite=sprite;img.type=Image.Type.Simple;img.color=Color.white;img.raycastTarget=false;
            return rt;
        }
#endif
    }
}
