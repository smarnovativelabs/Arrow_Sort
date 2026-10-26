#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WoolGame
{
    public sealed partial class WoolHud
    {
        public void AuthorDesignerTheme(Sprite[] sprites)
        {
            Sprite S(int id)=>sprites.Single(s=>s.name=="newCompleteSprite_"+id);
            void Art(Image image,Sprite sprite,bool sliced=true)
            {if(!image)return;image.sprite=sprite;image.type=sliced?Image.Type.Sliced:Image.Type.Simple;image.color=Color.white;image.preserveAspect=!sliced;}
            skin=new[]{S(69),S(4),S(76),S(54),S(17),S(18),S(61),S(69),S(29)};
            var ink=WoolArt.Hex("493923");
            foreach(var image in GetComponentsInChildren<Image>(true))
            {
                if(image.sprite&&image.sprite.name.StartsWith("UISkin-"))
                {
                    int index=int.Parse(image.sprite.name.Substring(7));if(index<skin.Length)Art(image,skin[index]);
                }
                if(image.name=="Coin art"||(image.transform.parent&&image.transform.parent.name=="Coin plate"))Art(image,S(18),false);
                if(image.name=="Gear art"||(image.name=="Icon 9"&&image.transform.parent.name=="Settings"))Art(image,S(21),false);
                if(image.name=="Icon 12")Art(image,S(84),false);
                if(image.name=="Product ribbon")image.enabled=false;
                if(image.name.StartsWith("Task ")&&!image.name.Contains("progress"))Art(image,S(92));
            }
            foreach(var text in GetComponentsInChildren<TMP_Text>(true))
            {
                text.fontStyle=FontStyles.Normal;text.outlineWidth=0;text.text=text.text.Replace("familys","families");
                if(text.color.a>.8f)text.color=ink;
            }
            foreach(var button in GetComponentsInChildren<Button>(true))
            {
                bool action=button.name=="Purchase"||button.name=="Primary"||button.name=="Play level"||button.name=="Claim"||button.name=="Spin";
                bool cross=button.name=="Close"||button.name.StartsWith("Close ");
                Art(button.GetComponent<Image>(),cross?S(54):action?S(4):S(29),!cross);
                var label=button.GetComponentInChildren<TMP_Text>(true);
                if(label){label.color=action?Color.white:ink;if(cross)label.text="";if(action){label.outlineWidth=.12f;label.outlineColor=WoolArt.Hex("287909");}}
                var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1,1,.96f);colors.pressedColor=new Color(.83f,.9f,.8f);colors.disabledColor=new Color(.65f,.67f,.64f,.7f);button.colors=colors;
            }
            // Keep existing controls and callbacks, replacing only their presentation.
            Art(modalCard.GetComponent<Image>(),S(69));Art(primary.GetComponent<Image>(),S(4));Art(secondary.GetComponent<Image>(),S(76));
            Art(close.GetComponent<Image>(),S(54),false);modalTitle.color=modalBody.color=ink;
            foreach(var image in GetComponentsInChildren<Image>(true))
                if(image.name=="Profile title"){Art(image,S(61));foreach(var t in image.GetComponentsInChildren<TMP_Text>(true))if(t.text!="")t.color=Color.white;}
            if(tutorialCard)Art(tutorialCard.GetComponent<Image>(),S(20));
            Art(header.Find("Coin plate").GetComponent<Image>(),S(5));coins.color=ink;
            level.color=Color.white;level.outlineWidth=.15f;level.outlineColor=ink;
            hint.fontSize=23;hint.color=ink;
            if(gameDifficulty){gameDifficulty.rectTransform.anchoredPosition=new Vector2(-260,-140);gameDifficulty.fontSize=19;}
            hint.rectTransform.anchoredPosition=new Vector2(0,-195);hint.rectTransform.sizeDelta=new Vector2(610,66);
            foreach(var button in footer.GetComponentsInChildren<Button>(true))Art(button.GetComponent<Image>(),S(29));

            // Shop: framed cards, clear title bands, consistent contents and price placement.
            shopPage.GetComponent<Image>().color=WoolArt.Hex("E6F0DB");
            var shopHeader=shopPage.Find("Shop awning").GetComponent<RectTransform>();
            Art(shopHeader.GetComponent<Image>(),S(61));shopHeader.sizeDelta=new Vector2(660,126);shopHeader.anchoredPosition=new Vector2(0,-80);
            var title=shopHeader.Find("Shop title").GetComponent<TMP_Text>();title.text="RESCUE SHOP";title.color=Color.white;title.fontSize=42;title.rectTransform.anchoredPosition=Vector2.zero;title.rectTransform.sizeDelta=new Vector2(530,80);title.outlineWidth=.12f;
            shopCoins.transform.SetParent(shopPage,false);Place(shopCoins.rectTransform,new Vector2(.5f,1),new Vector2(226,-184));shopCoins.fontSize=28;shopCoins.color=ink;
            var wallet=shopPage.Find("Shop wallet");
            if(!wallet)wallet=Panel("Shop wallet",shopPage,new Vector2(.5f,1),new Vector2(235,58),new Vector2(205,-184),Color.white);
            Art(wallet.GetComponent<Image>(),S(5));wallet.SetSiblingIndex(shopCoins.transform.GetSiblingIndex());shopCoins.transform.SetAsLastSibling();shopCoins.gameObject.SetActive(true);
            var coin=wallet.Find("Coin");if(!coin){var icon=Icon(wallet,9,new Vector2(48,48),new Vector2(-80,0));icon.name="Coin";coin=icon.transform;}Art(coin.GetComponent<Image>(),S(18),false);
            var viewport=shopPage.Find("Shop viewport").GetComponent<RectTransform>();viewport.offsetMax=new Vector2(0,-235);viewport.offsetMin=new Vector2(0,190);
            var content=viewport.Find("Shop products").GetComponent<RectTransform>();content.sizeDelta=new Vector2(650,6*344+20);
            int productIndex=0;
            foreach(Transform card in content)
            {
                var rect=card.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(620,320);rect.anchoredPosition=new Vector2(0,-170-productIndex*344);
                Art(card.GetComponent<Image>(),S(83));var outline=card.GetComponent<Outline>();if(outline)outline.enabled=false;
                var ribbon=card.Find("Product ribbon").GetComponent<RectTransform>();ribbon.anchorMin=ribbon.anchorMax=new Vector2(.5f,1);ribbon.anchoredPosition=new Vector2(0,-44);ribbon.sizeDelta=new Vector2(580,70);ribbon.GetComponent<Image>().enabled=false;
                var name=ribbon.Find("Name").GetComponent<TMP_Text>();name.rectTransform.anchoredPosition=Vector2.zero;name.rectTransform.sizeDelta=new Vector2(550,64);name.fontSize=29;name.color=Color.white;name.text=name.text.Replace("Cat Bundle","Duck Family Bundle");
                var purchase=card.GetComponentsInChildren<Button>(true).First(b=>b.name=="Purchase");purchase.transform.SetParent(card,false);var pr=purchase.GetComponent<RectTransform>();pr.anchorMin=pr.anchorMax=new Vector2(.5f,0);pr.anchoredPosition=new Vector2(163,50);pr.sizeDelta=new Vector2(260,70);Art(purchase.GetComponent<Image>(),S(4));purchase.GetComponentInChildren<TMP_Text>().fontSize=24;
                var contents=card.Find("Contents").GetComponent<TMP_Text>();contents.color=ink;contents.fontSize=productIndex==0?21:24;contents.rectTransform.anchoredPosition=productIndex==0?new Vector2(65,5):new Vector2(0,-20);contents.rectTransform.sizeDelta=productIndex==0?new Vector2(420,130):new Vector2(560,82);
                foreach(Transform child in card)if(child.name.StartsWith("Icon ")){var cr=child.GetComponent<RectTransform>();if(productIndex==0){cr.anchoredPosition=new Vector2(-220,12);cr.sizeDelta=new Vector2(108,108);}else{int index=int.Parse(child.name.Substring(5));cr.anchoredPosition=new Vector2(-105+70*index,52);cr.sizeDelta=new Vector2(58,58);}}
                productIndex++;
            }
            // Home now uses the game's green environment and duck family, not the old sailing scene.
            var picture=homePage.Find("Sailing with friends").GetComponent<RawImage>();
            picture.texture=UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Texture/Dragon_Game_Background_Grass.png");picture.uvRect=new Rect(0,.67f,1,.33f);
            picture.GetComponent<AspectRatioFitter>().aspectRatio=1;
            var banner=homePage.Find("Game title");if(!banner)banner=Panel("Game title",homePage,new Vector2(.5f,.81f),new Vector2(650,125),Vector2.zero,Color.white);
            Art(banner.GetComponent<Image>(),S(61));var heading=banner.Find("Title");if(!heading)heading=Label("Title",banner,"WOOL CRUSH",48,new Vector2(520,80),Vector2.zero,Color.white).transform;heading.GetComponent<TMP_Text>().color=Color.white;
            var subtitle=homePage.Find("Rescue subtitle");if(!subtitle)subtitle=Label("Rescue subtitle",homePage,"SAVE THE DUCK FAMILY",26,new Vector2(600,55),Vector2.zero,ink).transform;Place(subtitle.GetComponent<RectTransform>(),new Vector2(.5f,.75f),Vector2.zero);
            var hero=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/DuckFamily.png");
            if(hero)
            {
                var portrait=homePage.Find("Duck family art");if(!portrait)portrait=Panel("Duck family art",homePage,new Vector2(.5f,.56f),new Vector2(620,450),Vector2.zero,Color.white);
                portrait.GetComponent<RectTransform>().sizeDelta=new Vector2(960,685);
                Art(portrait.GetComponent<Image>(),hero,false);portrait.GetComponent<Image>().raycastTarget=false;
                foreach(var image in GetComponentsInChildren<Image>(true))if(image.name=="Launch mascot"||image.name=="Icon 15")Art(image,hero,false);
                var mascot=launch.transform.Find("Launch mascot") as RectTransform;if(mascot)mascot.sizeDelta=new Vector2(650,465);
            }
            string[] badges={"TASKS","Lv.16","OFFERS","NO ADS"};string[] captions={"TASKS","DAILY SPIN","OFFERS","NO ADS"};
            for(int i=0;i<badges.Length;i++){var badge=homePage.Find(badges[i]);if(!badge)continue;Place(badge.GetComponent<RectTransform>(),new Vector2(.5f,.34f),new Vector2(-240+160*i,0));badge.GetComponent<RectTransform>().sizeDelta=new Vector2(138,116);var label=badge.GetComponentInChildren<TMP_Text>();label.text=captions[i];label.fontSize=19;}
            if(homePage.Find("Lv.25"))homePage.Find("Lv.25").gameObject.SetActive(false);
            Place(homePage.Find("Play level").GetComponent<RectTransform>(),new Vector2(.5f,.21f),Vector2.zero);
            var nav=menu.transform.Find("Bottom navigation");Art(nav.GetComponent<Image>(),S(20));
            Art(homePage.Find("Coins").GetComponent<Image>(),S(5));
            Art(homePage.Find("Settings").GetComponent<Image>(),S(17));
            if(mapPage)mapPage.GetComponent<Image>().color=WoolArt.Hex("E6F0DB");
            if(mapPage&&mapPage.Find("Map header")){var mapHeader=mapPage.Find("Map header");Art(mapHeader.GetComponent<Image>(),S(61));foreach(var text in mapHeader.GetComponentsInChildren<TMP_Text>(true))text.color=Color.white;}
            if(cardsPage)cardsPage.GetComponent<Image>().color=WoolArt.Hex("E6F0DB");
            ConfigureBottomLayout();UnityEditor.EditorUtility.SetDirty(this);
        }
    }
}
#endif
