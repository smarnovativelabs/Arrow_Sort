using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] Sprite[] skin;
        [SerializeField] Button rewardButton,freeCoinButton,profileClose;
        [SerializeField] TMP_Text rewardLabel,profileName;
        [SerializeField] TMP_Text[] profileValues;
        [SerializeField] GameObject profilePanel;
        public void ShowReward(string title,string body,string first,Action action,string second,Action other,string reward,Action watch)
        {
            Show(title,body,first,action,second,other);
            modalCard.sizeDelta=new Vector2(620,700);
            modalTitle.rectTransform.anchoredPosition=new Vector2(0,245);
            modalBody.rectTransform.anchoredPosition=new Vector2(0,115);
            primary.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-30);
            secondary.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-125);
            if(rewardButton){rewardButton.gameObject.SetActive(reward!=null);rewardLabel.text=reward;rewardButton.onClick.RemoveAllListeners();if(watch!=null)rewardButton.onClick.AddListener(()=>watch());}
            ApplyModalSkin();
        }
        void BindPolishedUI()
        {
            if(freeCoinButton){freeCoinButton.onClick.RemoveAllListeners();freeCoinButton.onClick.AddListener(()=>FreeCoinsPressed?.Invoke());}
            if(profileClose){profileClose.onClick.RemoveAllListeners();profileClose.onClick.AddListener(()=>profilePanel.SetActive(false));}
        }
        void OpenProfile()
        {
            if(!profilePanel)return;
            profilePanel.SetActive(true);profilePanel.transform.SetAsLastSibling();FitAuthoredPanels();profileName.text="Player   ·   Level "+WoolProgress.CurrentLevel;
            int[] values={WoolProgress.FirstTryWins,WoolProgress.CompletedCount,WoolProgress.WinningStreak,WoolProgress.BestStreak};
            for(int i=0;i<profileValues.Length;i++)profileValues[i].text=values[i].ToString();
        }
        void ApplyModalSkin()
        {
            if(skin==null||skin.Length<9)return;
            Paint(modalCard.GetComponent<Image>(),0);Paint(primary.GetComponent<Image>(),1);Paint(secondary.GetComponent<Image>(),2);Paint(close.GetComponent<Image>(),3);
            modalTitle.color=WoolArt.Hex("59361E");modalBody.color=WoolArt.Hex("74513C");
        }
        void Paint(Image image,int sprite){if(!image||skin==null||skin.Length<9)return;image.sprite=skin[sprite];image.type=Image.Type.Sliced;image.color=Color.white;}
#if UNITY_EDITOR
        public void AuthorPolish(Sprite[] sprites)
        {
            skin=sprites;font=UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/WoolGame/Fonts/LilitaOne-TMP.asset");
            foreach(var text in GetComponentsInChildren<TMP_Text>(true))
            {
                if(font)text.font=font;text.fontStyle=FontStyles.Normal;
                if(text.fontSize>=26){text.outlineWidth=.1f;text.outlineColor=new Color(.25f,.11f,.025f,.75f);}
            }
            foreach(var button in GetComponentsInChildren<Button>(true))
            {
                Paint(button.GetComponent<Image>(),button.name=="Purchase"||button.name=="Play level"?1:8);
                button.GetComponent<Image>().type=Image.Type.Simple;
                var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=Color.white;colors.pressedColor=new Color(.85f,.8f,.65f);button.colors=colors;
            }
            foreach(var rect in GetComponentsInChildren<RectTransform>(true))
            {
                if(rect.name=="Footer"||rect.name=="Brand")rect.gameObject.SetActive(false);
                if(rect.name=="Product ribbon")Paint(rect.GetComponent<Image>(),2);
                if(rect.name.EndsWith("Bundle")||rect.name=="Remove Forced Ads")Paint(rect.GetComponent<Image>(),0);
            }
            header.GetComponent<Image>().enabled=false;header.sizeDelta=new Vector2(680,130);header.anchoredPosition=new Vector2(0,-80);
            level.rectTransform.anchoredPosition=new Vector2(-266,-45);level.rectTransform.sizeDelta=new Vector2(175,45);level.fontSize=25;level.color=Color.white;
            coins.rectTransform.anchoredPosition=new Vector2(222,20);coins.rectTransform.sizeDelta=new Vector2(175,55);coins.fontSize=30;coins.color=WoolArt.Hex("9A4F26");
            if(!header.Find("Coin plate")){var plate=Panel("Coin plate",header,new Vector2(.5f,.5f),new Vector2(250,66),new Vector2(195,20),Color.white);Paint(plate.GetComponent<Image>(),8);plate.SetAsFirstSibling();var icon=Icon(plate,9,new Vector2(60,60),new Vector2(-98,0));icon.sprite=skin[5];}
            var pause=header.Find("Pause").GetComponent<Button>();pause.GetComponent<RectTransform>().anchoredPosition=new Vector2(-268,20);pause.GetComponent<RectTransform>().sizeDelta=new Vector2(92,92);pause.GetComponentInChildren<TMP_Text>().text="";pause.GetComponent<Image>().enabled=false;
            if(!pause.transform.Find("Gear art")){var gear=Icon(pause.transform,9,new Vector2(86,86),Vector2.zero);gear.name="Gear art";}
            meter.gameObject.SetActive(false);progress.gameObject.SetActive(false);
            footer.GetComponent<Image>().enabled=false;footer.sizeDelta=new Vector2(680,116);footer.anchoredPosition=new Vector2(0,82);
            for(int i=0;i<4;i++)
            {
                var button=footer.Find(WoolBoosters.Names[i]);button.GetComponent<RectTransform>().sizeDelta=new Vector2(157,112);button.GetComponent<RectTransform>().anchoredPosition=new Vector2(-252+168*i,0);
                Paint(button.GetComponent<Image>(),8);button.GetComponent<Image>().type=Image.Type.Simple;var label=button.GetComponentInChildren<TMP_Text>();label.fontSize=16;label.color=WoolArt.Hex("8B491C");label.rectTransform.anchoredPosition=new Vector2(0,-37);
                var icon=button.Find("Icon "+i).GetComponent<RectTransform>();icon.sizeDelta=new Vector2(84,78);icon.anchoredPosition=new Vector2(0,10);
            }
            if(!rewardButton){rewardButton=ButtonAt("Reward video",modalCard,"WATCH VIDEO",new Vector2(500,84),new Vector2(0,-235),null);rewardLabel=rewardButton.GetComponentInChildren<TMP_Text>();Paint(rewardButton.GetComponent<Image>(),2);rewardLabel.color=Color.white;rewardLabel.fontSize=24;rewardButton.gameObject.SetActive(false);}
            var coinObject=homeCoins.transform.parent.gameObject;freeCoinButton=coinObject.GetComponent<Button>();if(!freeCoinButton)freeCoinButton=coinObject.AddComponent<Button>();Paint(coinObject.GetComponent<Image>(),8);
            if(!coinObject.transform.Find("Coin art")){var coin=Icon(coinObject.transform,9,new Vector2(70,70),new Vector2(-103,0));coin.name="Coin art";coin.sprite=skin[5];}
            if(!profilePanel)
            {
                var overlay=Rect("Player profile",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);profilePanel=overlay.gameObject;overlay.gameObject.AddComponent<Image>().color=new Color(.04f,.07f,.11f,.86f);
                var panel=Panel("Profile card",overlay,new Vector2(.5f,.5f),new Vector2(662,970),Vector2.zero,Color.white);Paint(panel.GetComponent<Image>(),7);
                var title=Panel("Profile title",panel,new Vector2(.5f,1),new Vector2(650,115),new Vector2(0,-30),Color.white);Paint(title.GetComponent<Image>(),6);Label("Title",title,"Profile",42,new Vector2(450,70),Vector2.zero,Color.white);
                profileClose=ButtonAt("Close profile",title,"X",new Vector2(75,75),new Vector2(275,15),null);Paint(profileClose.GetComponent<Image>(),3);profileClose.GetComponentInChildren<TMP_Text>().color=Color.white;
                Icon(panel,15,new Vector2(130,130),new Vector2(-210,300));profileName=Label("Player name",panel,"Player",30,new Vector2(390,120),new Vector2(80,305),Color.white);
                Label("Stats heading",panel,"General Stats",36,new Vector2(550,70),new Vector2(0,178),Color.white);
                string[] names={"First Try Wins","Collectibles","Winning Streak","Duck Families Rescued"};int[] symbols={6,8,14,15};profileValues=new TMP_Text[4];
                for(int i=0;i<4;i++)
                {
                    var card=Panel(names[i],panel,new Vector2(.5f,.5f),new Vector2(284,270),new Vector2(i%2==0?-152:152,2-(i/2)*295),Color.white);Paint(card.GetComponent<Image>(),0);
                    Icon(card,symbols[i],new Vector2(100,100),new Vector2(0,60));Label("Label",card,names[i],23,new Vector2(255,54),new Vector2(0,-28),WoolArt.Hex("8B491C"));profileValues[i]=Label("Value",card,"0",37,new Vector2(220,60),new Vector2(0,-84),WoolArt.Hex("703817"));
                }
                profilePanel.SetActive(false);
            }
            Paint(tutorialCard.GetComponent<Image>(),0);ApplyModalSkin();UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
