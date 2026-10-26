#if UNITY_EDITOR
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WoolGame
{
    public sealed partial class WoolHud
    {
        public void AuthorRescueTheme(Sprite[] art)
        {
            if(Application.isPlaying || art == null || art.Length != 16)
                throw new InvalidOperationException("Author the sixteen rescue sprites in Edit Mode.");
            var previous=skin;
            skin=new[]{art[0],art[1],art[2],art[3],art[4],art[5],art[6],art[0],art[7]};
            var ink=WoolArt.Hex("58442F");
            foreach(var image in GetComponentsInChildren<Image>(true))
            {
                if(previous!=null)for(int i=0;i<previous.Length&&i<skin.Length;i++)
                    if(image.sprite==previous[i]){SetRescueArt(image,skin[i],i!=3&&i!=4&&i!=5);break;}
            }
            for(int i=0;i<4;i++)icons[i]=art[8+i];
            icons[9]=art[4];icons[10]=art[14];icons[11]=art[13];icons[13]=art[15];icons[15]=art[12];
            foreach(var image in GetComponentsInChildren<Image>(true))
            {
                int index;
                if(image.name.StartsWith("Icon ")&&int.TryParse(image.name.Substring(5),out index)&&index>=0&&index<icons.Length)
                    SetRescueArt(image,icons[index],false);
                if(image.name=="Gear art")SetRescueArt(image,art[4],false);
                if(image.name=="Coin art"||image.name=="Coin"||image.transform.parent&&image.transform.parent.name=="Coin plate")
                    SetRescueArt(image,art[5],false);
                if(image.name=="Coin plate"||image.name=="Shop wallet"||image.name=="Coins")SetRescueArt(image,art[7],true);
                if(image.name=="Profile title"||image.name=="Shop awning"||image.name=="Map header"||image.name=="Game title")
                    SetRescueArt(image,art[6],true);
                if(image.name=="Duck family art"||image.name=="Launch mascot")SetRescueArt(image,art[12],false);
            }
            foreach(var text in GetComponentsInChildren<TMP_Text>(true))
            {
                text.fontStyle=FontStyles.Normal;text.color=ink;text.outlineWidth=0;
                var legacyOutline=text.GetComponent<Outline>();if(legacyOutline)legacyOutline.enabled=false;
            }
            foreach(var button in GetComponentsInChildren<Button>(true))
            {
                bool cross=button.name=="Close"||button.name.StartsWith("Close ");
                bool positive=button.name=="Primary"||button.name=="Play level"||button.name=="Purchase"||button.name=="Spin"||button.name.StartsWith("Claim");
                var graphic=button.GetComponent<Image>();
                SetRescueArt(graphic,cross?art[3]:positive?art[1]:art[7],!cross);
                var label=button.GetComponentInChildren<TMP_Text>(true);
                if(label){label.color=positive?Color.white:ink;if(cross)label.text="";if(positive){label.outlineWidth=.1f;label.outlineColor=WoolArt.Hex("34782B");}}
                var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=Color.white;colors.pressedColor=new Color(.84f,.91f,.95f);colors.disabledColor=new Color(.62f,.66f,.68f,.8f);button.colors=colors;
                var edge=button.GetComponent<Outline>();if(edge)edge.enabled=false;
            }
            SetRescueArt(primary.GetComponent<Image>(),art[1],true);
            SetRescueArt(secondary.GetComponent<Image>(),art[2],true);
            if(rewardButton)SetRescueArt(rewardButton.GetComponent<Image>(),art[2],true);
            SetRescueArt(header.Find("Pause").GetComponent<Image>(),art[4],false);
            var gear=header.Find("Pause/Gear art");if(gear)gear.gameObject.SetActive(false);
            header.Find("Pause").GetComponentInChildren<TMP_Text>(true).text="";
            header.GetComponent<Image>().enabled=false;
            SetRescueArt(tutorialCard.GetComponent<Image>(),art[0],true);
            SetRescueArt(boosterPicture.GetComponent<Image>(),art[7],true);
            SetRescueArt(modalCard.GetComponent<Image>(),art[0],true);
            level.fontSize=34;level.color=ink;level.rectTransform.sizeDelta=new Vector2(240,64);
            var levelPlate=header.Find("Level plate") as RectTransform;
            if(!levelPlate)levelPlate=Panel("Level plate",header,new Vector2(.5f,1),new Vector2(270,90),new Vector2(0,-66),Color.white);
            SetRescueArt(levelPlate.GetComponent<Image>(),art[6],true);levelPlate.SetAsFirstSibling();
            coins.fontSize=33;coins.color=ink;
            meter.GetComponent<Image>().sprite=rounded;meter.GetComponent<Image>().color=WoolArt.Hex("D4E0C8");
            progressFill.sprite=rounded;progressFill.color=WoolArt.Hex("73C957");
            progress.fontSize=24;progress.color=WoolArt.Hex("486659");
            footer.GetComponent<Image>().enabled=false;
            for(int i=0;i<4;i++)
            {
                var button=footer.Find(WoolBoosters.Names[i]);
                SetRescueArt(button.GetComponent<Image>(),art[7],true);
                var rt=(RectTransform)button;rt.sizeDelta=new Vector2(154,124);rt.anchoredPosition=new Vector2(-252+168*i,0);
                var icon=button.Find("Icon "+i) as RectTransform;icon.sizeDelta=new Vector2(84,84);icon.anchoredPosition=new Vector2(0,15);
                var label=button.GetComponentInChildren<TMP_Text>(true);label.rectTransform.anchoredPosition=new Vector2(0,-43);label.fontSize=17;
            }
            var homeArt=homePage.Find("Sailing with friends").GetComponent<RawImage>();
            homeArt.texture=null;homeArt.color=WoolArt.Hex("D9F0F3");homeArt.uvRect=new Rect(0,0,1,1);
            homePage.GetComponent<RectMask2D>().enabled=true;
            homePage.Find("Game title/Title").GetComponent<TMP_Text>().text="WOOL CRUSH";
            homePage.Find("Game title/Title").GetComponent<TMP_Text>().fontSize=47;
            var hero=homePage.Find("Duck family art") as RectTransform;
            hero.sizeDelta=new Vector2(590,590);Place(hero,new Vector2(.5f,.55f),Vector2.zero);
            var subtitle=homePage.Find("Rescue subtitle").GetComponent<TMP_Text>();subtitle.text="A LITTLE COLOR. A BIG RESCUE.";subtitle.fontSize=23;
            SetRescueArt(homePage.Find("Settings").GetComponent<Image>(),art[4],false);
            var settingsIcon=homePage.Find("Settings/Icon 9");if(settingsIcon)settingsIcon.gameObject.SetActive(false);
            var nav=menu.transform.Find("Bottom navigation");SetRescueArt(nav.GetComponent<Image>(),art[0],true);
            shopPage.GetComponent<Image>().color=WoolArt.Hex("EAF2E7");
            launch.GetComponent<Image>().color=WoolArt.Hex("D9F0F3");launchTitle.text="WOOL CRUSH";
            launchTitle.color=ink;launchTitle.fontSize=54;
            if(bannerArea){var caption=bannerArea.Find("Banner placeholder");if(caption)caption.gameObject.SetActive(false);bannerArea.GetComponent<Image>().color=WoolArt.Hex("F6F2E9");}
            safe.gameObject.SetActive(true);canvas.enabled=true;profilePanel.SetActive(false);modal.SetActive(false);launch.SetActive(false);menu.SetActive(false);
            meter.gameObject.SetActive(true);progress.gameObject.SetActive(true);
            ConfigureInputTargets();portraitWidth=0;ApplyPortraitFrame(Screen.width,Screen.height,Screen.safeArea);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        static void SetRescueArt(Image image,Sprite sprite,bool sliced)
        {
            if(!image||!sprite)return;
            image.sprite=sprite;image.color=Color.white;image.enabled=true;
            image.type=sliced?Image.Type.Sliced:Image.Type.Simple;image.preserveAspect=!sliced;
            image.raycastTarget=image.GetComponent<Button>()!=null;
        }
    }
}
#endif
