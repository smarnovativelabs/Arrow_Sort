using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] GameObject menu;
        [SerializeField] RectTransform homePage,shopPage,cardsPage;
        [SerializeField] TMP_Text homeLevel,homeCoins,shopCoins,homeLock;
        [SerializeField] RectTransform cardList;
        [SerializeField] Texture2D homeTexture;
        int unlockedLevel;

        void BuildMenu()
        {
            var root=Rect("Main menu",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            menu=root.gameObject;menu.AddComponent<Image>().color=WoolArt.Hex("30B8E0");
            // Keep all menus below the shared modal, regardless of creation order.
            root.SetSiblingIndex(modal.transform.GetSiblingIndex());
            homePage=Rect("Home",root,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            homePage.gameObject.AddComponent<RectMask2D>();
            homeTexture=Resources.Load<Texture2D>("WoolHome");
            var picture=Rect("Sailing with friends",homePage,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);
            var art=picture.gameObject.AddComponent<RawImage>();art.texture=homeTexture;art.raycastTarget=false;
            var fitter=picture.gameObject.AddComponent<AspectRatioFitter>();fitter.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;fitter.aspectRatio=homeTexture?(float)homeTexture.width/homeTexture.height:9f/16;

            var coinBar=Panel("Coins",homePage,new Vector2(.5f,1),new Vector2(230,68),new Vector2(125,-77),WoolArt.Hex("FFF0D6"));
            homeCoins=Label("Balance",coinBar,"0",37,new Vector2(190,60),Vector2.zero,WoolArt.Hex("AF4939"));
            var settings=ButtonAt("Settings",homePage,"",new Vector2(74,72),Vector2.zero,ShowMenuSettings);Icon(settings.transform,9,new Vector2(76,76),Vector2.zero);
            Place(settings.GetComponent<RectTransform>(),new Vector2(.5f,1),new Vector2(292,-77));
            var profile=ButtonAt("Profile",homePage,"",new Vector2(85,88),Vector2.zero,ShowProfile);Icon(profile.transform,15,new Vector2(91,94),Vector2.zero);
            Place(profile.GetComponent<RectTransform>(),new Vector2(.5f,1),new Vector2(-282,-80));
            var locked=Panel("Upcoming feature",homePage,new Vector2(.5f,1),new Vector2(630,80),new Vector2(0,-195),WoolArt.Hex("6D432B"));
            homeLock=Label("Unlock level",locked,"Unlock at Level 17",27,new Vector2(580,64),Vector2.zero,Color.white);
            MenuBadge(homePage,"TASKS",10,-278,-335,()=>Show("LEVEL REWARDS","Rescue a duck family to earn 50 coins.\nComplete levels to grow your collection.","BACK",Hide));
            MenuBadge(homePage,"OFFERS",11,278,-335,ShowShop);
            MenuBadge(homePage,"Lv.16",13,-278,-490,()=>Show("LOCKED","Keep completing levels to reach Level 16.","BACK",Hide));
            MenuBadge(homePage,"NO ADS",12,278,-490,ShowShop);
            MenuBadge(homePage,"Lv.25",14,278,-645,()=>Show("LOCKED","Keep completing levels to reach Level 25.","BACK",Hide));
            var play=MenuButton("Play level",homePage,"Level 1",new Vector2(345,120),Vector2.zero,()=>PlayPressed?.Invoke(),WoolArt.Hex("61CB00"));
            Place(play.GetComponent<RectTransform>(),new Vector2(.5f,.205f),Vector2.zero);homeLevel=play.GetComponentInChildren<TMP_Text>();homeLevel.fontSize=44;

            shopPage=Rect("Shop",root,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);shopPage.gameObject.AddComponent<Image>().color=WoolArt.Hex("304657");
            var shopHeader=Panel("Shop awning",shopPage,new Vector2(.5f,1),new Vector2(1500,160),new Vector2(0,-70),WoolArt.Hex("F79A20"));
            Label("Shop title",shopHeader,"Shop",49,new Vector2(320,90),new Vector2(-100,0),Color.white);
            shopCoins=Label("Shop coins",shopHeader,"0",36,new Vector2(200,64),new Vector2(223,0),Color.white);
            var viewport=Rect("Shop viewport",shopPage,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            viewport.offsetMin=new Vector2(0,144);viewport.offsetMax=new Vector2(0,-150);viewport.gameObject.AddComponent<RectMask2D>();
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.viewport=viewport;
            var content=Rect("Shop products",viewport,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(650,2030),Vector2.zero);content.pivot=new Vector2(.5f,1);scroll.content=content;
            Product(content,0,"Remove Forced Ads","No automatic pop-up ads\nNo bottom banner ads\nReward videos stay optional\n+ 5000 coins","Rs2,250.00",WoolArt.Hex("F83068"));
            Product(content,1,"Starter Bundle","2000 coins\nEach booster x1","Rs550.00",WoolArt.Hex("E98922"));
            Product(content,2,"Superior Bundle","2000 coins  ·  Each booster x2\nVIP 30 minutes","Rs2,850.00",WoolArt.Hex("177CEC"));
            Product(content,3,"Large Bundle","6000 coins  ·  Each booster x4\nVIP 1 hour","Rs5,700.00",WoolArt.Hex("177CEC"));
            Product(content,4,"Cat Bundle","14000 coins  ·  Each booster x10\nVIP 2 hours","Rs10,100.00",WoolArt.Hex("177CEC"));
            Product(content,5,"Advanced Bundle","20000 coins  ·  Each booster x16\nVIP 3 hours","Rs14,200.00",WoolArt.Hex("177CEC"));

            cardsPage=Rect("Collection",root,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);cardsPage.gameObject.AddComponent<Image>().color=WoolArt.Hex("7ECDE7");
            var collectionTitle=Label("Collection title",cardsPage,"HOME COLLECTION",42,new Vector2(660,110),Vector2.zero,Color.white);Place(collectionTitle.rectTransform,new Vector2(.5f,1),new Vector2(0,-120));
            cardList=Rect("Cards",cardsPage,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(640,760),Vector2.zero);
            var nav=Panel("Bottom navigation",root,new Vector2(.5f,0),new Vector2(1600,132),new Vector2(0,66),WoolArt.Hex("F5A325"));
            string[] names={"Shop","Rank","Home","Club","Cards"};
            for(int i=0;i<5;i++)
            {
                int tab=i;var button=MenuButton(names[i],nav,names[i],new Vector2(133,106),new Vector2((i-2)*139,0),()=>Navigate(tab),WoolArt.Hex("FFC34F"));
                var label=button.GetComponentInChildren<TMP_Text>();label.fontSize=21;label.rectTransform.anchoredPosition=new Vector2(0,-36);label.rectTransform.sizeDelta=new Vector2(130,30);
                Icon(button.transform,i+4,new Vector2(80,78),new Vector2(0,14));
                if(i==1||i==3){button.interactable=false;label.text="Locked";}
            }
            BuildCards();menu.SetActive(false);
        }

        void MenuBadge(Transform parent,string text,int icon,float x,float y,Action action)
        {
            var b=MenuButton(text,parent,text,new Vector2(106,112),Vector2.zero,action,WoolArt.Hex("E49A23"));
            var caption=b.GetComponentInChildren<TMP_Text>();caption.fontSize=20;caption.rectTransform.sizeDelta=new Vector2(104,28);caption.rectTransform.anchoredPosition=new Vector2(0,-41);
            Icon(b.transform,icon,new Vector2(99,94),new Vector2(0,15));
            Place(b.GetComponent<RectTransform>(),new Vector2(.5f,1),new Vector2(x,y));
        }
        Button MenuButton(string name,Transform parent,string text,Vector2 size,Vector2 position,Action action,Color color)
        {
            var b=ButtonAt(name,parent,text,size,position,action);b.GetComponent<Image>().color=color;
            var label=b.GetComponentInChildren<TMP_Text>();label.color=Color.white;label.fontSize=27;
            var outline=label.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.35f,.17f,.04f,.85f);outline.effectDistance=new Vector2(1.5f,-1.5f);
            var edge=b.gameObject.AddComponent<Outline>();edge.effectColor=WoolArt.Hex("FFF1C2");edge.effectDistance=new Vector2(3,-3);return b;
        }
        void Product(Transform parent,int index,string title,string contents,string price,Color color)
        {
            var card=Panel(title,parent,new Vector2(.5f,1),new Vector2(620,312),new Vector2(0,-170-index*335),WoolArt.Hex("FFEDCC"));
            var border=card.gameObject.AddComponent<Outline>();border.effectColor=WoolArt.Hex("FBA21F");border.effectDistance=new Vector2(5,-5);
            if(index>0)for(int k=0;k<4;k++)Icon(card,k,new Vector2(58,58),new Vector2(-90+k*60,108));
            else Icon(card,12,new Vector2(110,110),new Vector2(-222,55));
            Label("Contents",card,contents,index==0?23:27,new Vector2(index==0?430:560,130),new Vector2(index==0?63:0,index==0?44:26),WoolArt.Hex("95532F"));
            var ribbon=Panel("Product ribbon",card,new Vector2(.5f,0),new Vector2(620,92),new Vector2(0,46),color);
            Label("Name",ribbon,title,28,new Vector2(350,80),new Vector2(-120,0),Color.white);
            MenuButton("Purchase",ribbon,price,new Vector2(216,68),new Vector2(189,0),()=>Show("STORE UNAVAILABLE","Purchases are currently unavailable.\nNo payment has been taken.","BACK TO SHOP",Hide),WoolArt.Hex("58C500"));
        }
        void Navigate(int index){CloseEngagement();if(index==0)ShowShop();else if(index==2){if(mapPage)mapPage.gameObject.SetActive(false);if(profilePanel)profilePanel.SetActive(false);homePage.gameObject.SetActive(true);shopPage.gameObject.SetActive(false);if(cardsPage)cardsPage.gameObject.SetActive(false);SelectTab("Home");}else if(index==4)ShowMap();}
        public void Home(int stage,int balance)
        {
            ClearCompactGameplayFrame();
            Tutorial(false);meter.gameObject.SetActive(false);progress.gameObject.SetActive(false);
            header.gameObject.SetActive(false);footer.gameObject.SetActive(false);hint.gameObject.SetActive(false);if(bannerArea)bannerArea.gameObject.SetActive(false);
            unlockedLevel=WoolProgress.CurrentLevel;menu.SetActive(true);homeLevel.text="Level "+unlockedLevel;homeCoins.text=shopCoins.text=balance.ToString();
            if(homeLock)homeLock.gameObject.SetActive(false);RefreshDifficulty(unlockedLevel);Navigate(2);
        }
        public void CloseMenu(){safe.gameObject.SetActive(true);CloseEngagement();menu.SetActive(false);if(profilePanel)profilePanel.SetActive(false);header.gameObject.SetActive(true);footer.gameObject.SetActive(true);hint.gameObject.SetActive(false);if(bannerArea)bannerArea.gameObject.SetActive(false);}
        public void ShowShop(){CloseEngagement();if(mapPage)mapPage.gameObject.SetActive(false);if(profilePanel)profilePanel.SetActive(false);homePage.gameObject.SetActive(false);if(cardsPage)cardsPage.gameObject.SetActive(false);shopPage.gameObject.SetActive(true);SelectTab("Shop");}
        void ShowCards()
        {
            homePage.gameObject.SetActive(false);shopPage.gameObject.SetActive(false);cardsPage.gameObject.SetActive(true);
        }
        void BuildCards()
        {
            string[] titles={"First rescue","Forest friends","Color explorer","Yarn keeper","Ocean adventure","Dragon friend"};
            for(int i=0;i<titles.Length;i++)
            {
                bool earned=unlockedLevel>i*3+1;var card=Panel("Card "+i,cardList,new Vector2(.5f,.5f),new Vector2(280,210),new Vector2((i%2==0?-1:1)*153,250-(i/2)*240),WoolArt.Hex(earned?"FFF0CF":"81A7B4"));
                Label("Card title",card,titles[i]+"\n\n"+(earned?"Collected":"Complete Level "+(i*3+1)),26,new Vector2(250,185),Vector2.zero,earned?WoolArt.Hex("8B552E"):Color.white);
            }
        }
        void ShowMenuSettings()
        {
            ShowAudioSettings(Hide);
        }
    }
}
