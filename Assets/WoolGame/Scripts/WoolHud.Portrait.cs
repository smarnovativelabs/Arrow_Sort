using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        public const float PortraitAspect=720f/1600f;
        
        int portraitWidth,portraitHeight;
        Rect portraitSafe;
        float compactDesignHeight;
        public void ApplyCompactGameplayFrame(Rect frame,float designHeight,int width,int height)
        {
            compactDesignHeight=designHeight;
            if(bannerArea)bannerArea.gameObject.SetActive(false);
            safe.anchorMin=safe.anchorMax=frame.center/new Vector2(width,height);
            safe.pivot=new Vector2(.5f,.5f);safe.anchoredPosition=Vector2.zero;
            safe.sizeDelta=new Vector2(1080,designHeight);
            safe.localScale=Vector3.one*(frame.width/(1080*Mathf.Max(.001f,canvas.scaleFactor)));
            float frameScale=Mathf.Max(.001f,frame.width/1080f);
            // Outer space extends the environment; edge controls still follow
            // the device safe area rather than drifting inward on tall phones.
            header.anchoredPosition=new Vector2(0,(portraitSafe.yMax-frame.yMax)/frameScale);
            footer.localScale=Vector3.one*1.45f;
            footer.anchoredPosition=new Vector2(0,115-(frame.yMin-portraitSafe.yMin)/frameScale);
            // Gameplay controls float over the scene, as in the portrait reference.
            if(releaseHeaderArt)releaseHeaderArt.gameObject.SetActive(false);
            var headerImage=header.GetComponent<UnityEngine.UI.Image>();if(headerImage)headerImage.enabled=false;
            EdgeControl(header,"Pause",new Vector2(0,1),new Vector2(92,-110));
            EdgeControl(header,"Level",new Vector2(0,1),new Vector2(135,-205));
            var levelPlate=header.Find("Level plate");if(levelPlate)levelPlate.gameObject.SetActive(false);
            foreach(string name in new[]{"Coin plate","Coins"}){var item=header.Find(name);if(item)item.gameObject.SetActive(false);}
            var speed=header.Find("Gameplay speed");if(speed){speed.gameObject.SetActive(true);EdgeControl(header,"Gameplay speed",Vector2.one,new Vector2(-100,-110));}
            meter.gameObject.SetActive(false);progress.gameObject.SetActive(false);
            if(boardCropLeft)boardCropLeft.gameObject.SetActive(false);if(boardCropRight)boardCropRight.gameObject.SetActive(false);
            FitReleaseUI();
        }
        public void ClearCompactGameplayFrame(){compactDesignHeight=0;if(safe)safe.localScale=Vector3.one;portraitWidth=0;RefreshSafeArea();HideAbilityGuide();HideAbilityEffect();}
        [SerializeField] RectTransform bannerArea,abilityBackdrop;
        public void ConfigureBottomLayout()
        {
            if(!bannerArea)
            {
                bannerArea=Panel("Banner ad area",safe,new Vector2(.5f,0),new Vector2(720,110),Vector2.zero,WoolArt.Hex("A9A59D"));
                Label("Banner placeholder",bannerArea,"ADVERTISEMENT",18,new Vector2(500,45),Vector2.zero,WoolArt.Hex("615F59"));
            }
            if(!abilityBackdrop)abilityBackdrop=Panel("Ability strip",safe,new Vector2(.5f,0),new Vector2(720,170),Vector2.zero,WoolArt.Hex("FAE9CC"));
            bannerArea.anchorMin=new Vector2(0,0);bannerArea.anchorMax=new Vector2(1,0);bannerArea.pivot=new Vector2(.5f,0);
            bannerArea.sizeDelta=new Vector2(0,110);bannerArea.anchoredPosition=Vector2.zero;
            abilityBackdrop.anchorMin=new Vector2(0,0);abilityBackdrop.anchorMax=new Vector2(1,0);abilityBackdrop.pivot=new Vector2(.5f,0);
            abilityBackdrop.sizeDelta=new Vector2(0,170);abilityBackdrop.anchoredPosition=new Vector2(0,110);
            bannerArea.GetComponent<Image>().sprite=null;abilityBackdrop.GetComponent<Image>().sprite=null;
            abilityBackdrop.gameObject.SetActive(false);
            bannerArea.SetAsFirstSibling();abilityBackdrop.SetAsFirstSibling();
            footer.anchoredPosition=new Vector2(0,192);
        }

        public static Rect PortraitPixels(int width,int height,Rect safeArea)
        {
            return UnityEngine.Rect.MinMaxRect(Mathf.Clamp(safeArea.xMin,0,width),Mathf.Clamp(safeArea.yMin,0,height),
                Mathf.Clamp(safeArea.xMax,0,width),Mathf.Clamp(safeArea.yMax,0,height));
        }
        public static float PortraitScale(int width,int height)
        {
            return Mathf.Max(.001f,Mathf.Min(width/1080f,height/1920f));
        }
        public void ApplyPortraitFrame(int width,int height,Rect safeArea)
        {
            if(!canvas||width<=0||height<=0)return;
            if(portraitWidth==width&&portraitHeight==height&&portraitSafe==safeArea)return;
            portraitWidth=width;portraitHeight=height;portraitSafe=safeArea;
            var scaler=canvas.GetComponent<CanvasScaler>();
            if(scaler)
            {
                scaler.enabled=true;
                scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution=new Vector2(1080,1920);
                scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            }
            float scale=PortraitScale(width,height);
            canvas.scaleFactor=scale;
            Rect frame=PortraitPixels(width,height,safeArea);
            if(frame.width<=0||frame.height<=0)frame=new Rect(0,0,width,height);
            var min=frame.min/new Vector2(width,height);
            var max=frame.max/new Vector2(width,height);
            foreach(Transform child in transform)
            {
                var rt=child as RectTransform;
                if(!rt)continue;
                if(rt.GetComponent<WoolFullBleedBackground>())
                {rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;continue;}
                rt.anchorMin=min;rt.anchorMax=max;rt.offsetMin=rt.offsetMax=Vector2.zero;
            }
            // Keep artwork full bleed while interactive controls stay inside the safe area.
            var background=transform.Find("Main menu/Home/Sailing with friends") as RectTransform;
            if(background)
            {
                background.offsetMin=new Vector2(-frame.xMin,-frame.yMin)/scale;
                background.offsetMax=new Vector2(width-frame.xMax,height-frame.yMax)/scale;
            }
            if(header)
            {
                header.anchorMin=new Vector2(0,1);header.anchorMax=Vector2.one;
                header.pivot=new Vector2(.5f,1);header.sizeDelta=new Vector2(0,200);
                header.anchoredPosition=Vector2.zero;
                EdgeControl(header,"Pause",new Vector2(0,1),new Vector2(70,-65));
                EdgeControl(header,"Level",new Vector2(.5f,1),new Vector2(0,-66));
                EdgeControl(header,"Level plate",new Vector2(.5f,1),new Vector2(0,-66));
                EdgeControl(header,"Level difficulty",new Vector2(0,1),new Vector2(100,-172));
                EdgeControl(header,"Coin plate",Vector2.one,new Vector2(-185,-65));
                EdgeControl(header,"Coins",Vector2.one,new Vector2(-158,-65));
                EdgeControl(header,"Gameplay speed",Vector2.one,new Vector2(-100,-167));
                foreach(string name in new[]{"Coin plate","Coins"}){var item=header.Find(name);if(item)item.gameObject.SetActive(true);}
                foreach(string name in new[]{"Level difficulty","Gameplay speed"}){var item=header.Find(name);if(item)item.gameObject.SetActive(false);}
                meter.anchorMin=meter.anchorMax=new Vector2(.5f,1);meter.anchoredPosition=new Vector2(0,-139);meter.sizeDelta=new Vector2(480,18);
                progress.rectTransform.anchorMin=progress.rectTransform.anchorMax=new Vector2(.5f,1);progress.rectTransform.anchoredPosition=new Vector2(0,-178);progress.rectTransform.sizeDelta=new Vector2(650,34);
            }
            if(hint&&hint.transform.parent==safe)
            {
                var rt=hint.rectTransform;
                rt.anchorMin=rt.anchorMax=new Vector2(.5f,1);
                rt.anchoredPosition=new Vector2(0,-220);
                rt.sizeDelta=new Vector2(Mathf.Min(720,frame.width/scale-80),70);
                if(hint.transform.parent==safe)hint.gameObject.SetActive(false);
            }
            if(footer)
            {
                footer.anchorMin=footer.anchorMax=new Vector2(.5f,0);
                footer.anchoredPosition=new Vector2(0,190);
                footer.localScale=Vector3.one*Mathf.Min(1.5f,(frame.width/scale-32)/680f);
            }
            ResponsiveMenus(frame.width/scale,frame.height/scale);
            FitReleaseUI();
            lastWidth=width;lastHeight=height;lastSafe=safeArea;
        }

        public void PositionTutorial(Vector2 normalizedSlotBottom)
        {
            if(!tutorialCard||!tutorialCard.gameObject.activeSelf||!safe)return;
            float width=Mathf.Max(1,safe.rect.width),height=Mathf.Max(1,safe.rect.height);
            tutorialCard.anchorMin=tutorialCard.anchorMax=new Vector2(.5f,Mathf.Clamp(normalizedSlotBottom.y,.32f,.72f));
            tutorialCard.pivot=new Vector2(.5f,1);tutorialCard.anchoredPosition=new Vector2(0,-36);
            tutorialCard.sizeDelta=new Vector2(Mathf.Min(880,width-72),130);tutorialCard.localScale=Vector3.one;
            hint.rectTransform.anchorMin=Vector2.zero;hint.rectTransform.anchorMax=Vector2.one;
            hint.rectTransform.offsetMin=new Vector2(20,12);hint.rectTransform.offsetMax=new Vector2(-20,-12);
            hint.fontSize=29;hint.enableAutoSizing=true;hint.fontSizeMin=23;hint.fontSizeMax=29;hint.gameObject.SetActive(true);
            tutorialTip.rectTransform.anchorMin=tutorialTip.rectTransform.anchorMax=new Vector2(.5f,0);
            tutorialTip.rectTransform.anchoredPosition=new Vector2(0,205);
            tutorialTip.rectTransform.sizeDelta=new Vector2(Mathf.Min(900,width-60),90);
            tutorialTip.fontSize=24;
            tutorialTip.text="No rush. Follow the hand—your duck family is safe while learning.";
        }
        static void EdgeControl(RectTransform parent,string name,Vector2 anchor,Vector2 position)
        {
            var rt=parent.Find(name) as RectTransform;
            if(!rt)return;
            rt.anchorMin=rt.anchorMax=anchor;rt.anchoredPosition=position;
        }
        public void SetCaptureFrame(bool capture)
        {
            portraitWidth=0;
            ApplyPortraitFrame(Screen.width,Screen.height,capture?new Rect(0,0,Screen.width,Screen.height):Screen.safeArea);
        }
    }
}
