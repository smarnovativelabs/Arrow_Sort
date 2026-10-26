#if UNITY_EDITOR
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Coffee.UIEffects;

namespace WoolGame
{
    public sealed partial class WoolHud
    {
        public string AuthorFantasyHome(Sprite background,Sprite[] ui,Sprite[] characters)
        {
            if(Application.isPlaying||ui.Length!=16||characters.Length!=4)
                throw new InvalidOperationException("Author home layers in Edit Mode with all sprites supplied.");
            foreach(Transform child in homePage)child.gameObject.SetActive(false);
            referenceHome=ReferenceRoot("Reference home",homePage,null);
            referenceHome.GetComponent<Image>().enabled=false;
            referenceHome.gameObject.SetActive(true);
            foreach(Transform child in referenceHome)child.gameObject.SetActive(false);
            var pageMask=homePage.GetComponent<RectMask2D>();if(pageMask)pageMask.enabled=false;
            var motion=referenceHome.GetComponent<WoolHomePresentation>();
            if(!motion)motion=referenceHome.gameObject.AddComponent<WoolHomePresentation>();
            motion.background=FantasyImage("Village background",homePage,background,0,0,941,1672,false);
            motion.background.SetAsFirstSibling();
            motion.topBar=FantasyGroup("Header controls",referenceHome,941,180);
            motion.hero=FantasyGroup("Living rescue scene",referenceHome,941,900);
            motion.actions=FantasyGroup("Action cards",referenceHome,941,220);
            motion.levelArea=FantasyGroup("Level area",referenceHome,941,230);
            motion.navigation=FantasyGroup("Navigation",referenceHome,941,190);
            // Decorative layers never intercept touches.
            motion.clouds=new RectTransform[5];
            for(int i=0;i<5;i++)
            {
                motion.clouds[i]=FantasyImage("Moving cloud "+i,referenceHome,ui[13],0,0,400-i*30,210-i*15,false);
                motion.clouds[i].anchorMin=motion.clouds[i].anchorMax=new Vector2(.5f,1);
                motion.clouds[i].GetComponent<Image>().color=new Color(1,1,1,.78f-i*.05f);
                motion.clouds[i].SetAsFirstSibling();
            }
            // Reuse existing named Buttons, so the established BindReferencePages owns actions.
            FantasyButton("Profile",motion.topBar,ui[0],35,20,128,135);
            FantasyButton("Wallet",motion.topBar,ui[1],500,34,292,98);
            FantasyButton("Settings",motion.topBar,ui[2],808,25,110,120);
            homeCoins=FantasyText("Live coins",motion.topBar,580,47,145,66,48,WoolArt.Hex("65300A"));
            homeCoins.text="0";
            motion.dragon=FantasyImage("Dragon",motion.hero,characters[1],560,-5,500,500,false);
            motion.dragon.anchorMin=motion.dragon.anchorMax=new Vector2(.5f,.5f);
            motion.dragon.anchoredPosition=new Vector2(365,170);
            var fire=FantasyImage("Dragon fire",motion.dragon,ui[14],-200,435,350,300,false);
            // This mesh animates the plume, instead of moving the entire background screenshot.
            if(!fire.GetComponent<WoolHomeFlame>())fire.gameObject.AddComponent<WoolHomeFlame>();
            motion.mother=FantasyGroup("Duck family",motion.hero,941,900);
            motion.mother.anchorMin=motion.mother.anchorMax=new Vector2(.5f,.5f);
            motion.mother.anchoredPosition=Vector2.zero;
            if(motion.leftWing)motion.leftWing.gameObject.SetActive(false);
            if(motion.rightWing)motion.rightWing.gameObject.SetActive(false);
            var fixedFamily=UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/WoolGame/Designer/UI/FantasyHome/DuckFamilyFixed.png");
            Sprite familySprite=null;foreach(var asset in fixedFamily)if(asset is Sprite sprite){familySprite=sprite;break;}
            if(!familySprite)throw new InvalidOperationException("Missing integrated duck-family sprite.");
            FantasyImage("Mother and babies",motion.mother,familySprite,100,370,620,540,false).SetAsFirstSibling();
            string[] names={"Tasks","Lucky Spin","Rescue Shop","Ad Pass"};
            string[] captions={"DAILY\nTASKS","LUCKY\nSPIN","RESCUE\nSHOP","AD\nPASS"};
            motion.actionIcons=new RectTransform[4];
            motion.actionCards=new RectTransform[4];
            for(int i=0;i<4;i++)
            {
                float x=45+i*220;
                var button=FantasyButton(names[i],motion.actions,ui[3],x,10,195,200);
                motion.actionCards[i]=(RectTransform)button.transform;
                motion.actionIcons[i]=FantasyImage("Animated icon",button.transform,ui[4+i],28,-4,140,130,false);
                var label=FantasyText("Caption",button.transform,12,119,171,76,30,WoolArt.Hex("582407"));
                label.text=captions[i];label.lineSpacing=-10;
            }
            motion.wheel=motion.actionIcons[1];
            FantasyImage("Fixed wheel pointer",motion.wheel.parent,ui[15],83,-8,28,46,false);
            var play=FantasyButton("Play",motion.levelArea,ui[8],163,7,614,174);
            motion.levelButton=(RectTransform)play.transform;
            homeLevel=FantasyText("Live level",play.transform,35,20,544,130,83,Color.white);
            homeLevel.text="Level "+Mathf.Max(1,WoolProgress.CurrentLevel);
            homeLevel.fontStyle=FontStyles.Normal;
            // An independent material prevents outline changes from touching other UI text.
            const string materialPath="Assets/WoolGame/Designer/UI/FantasyHome/LevelText.mat";
            var textMaterial=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(!textMaterial){textMaterial=new Material(homeLevel.font.material);UnityEditor.AssetDatabase.CreateAsset(textMaterial,materialPath);}
            textMaterial.CopyPropertiesFromMaterial(homeLevel.font.material);
            textMaterial.SetColor("_OutlineColor",WoolArt.Hex("226B05"));textMaterial.SetFloat("_OutlineWidth",.18f);
            textMaterial.SetFloat("_FaceDilate",0);textMaterial.EnableKeyword("OUTLINE_ON");
            textMaterial.SetFloat("_PerspectiveFilter",0);textMaterial.SetFloat("_Sharpness",.15f);
            homeLevel.fontSharedMaterial=textMaterial;UnityEditor.EditorUtility.SetDirty(textMaterial);
            motion.levelName=homeLevel.rectTransform;
            homeDifficulty=FantasyText("Live difficulty",motion.levelArea,220,185,500,45,30,Color.white);
            homeDifficulty.text="A fresh challenge";
            FantasyText("Left flourish",motion.levelArea,175,186,45,43,28,Color.white).text="—";
            FantasyText("Right flourish",motion.levelArea,722,186,45,43,28,Color.white).text="—";
            var shine=play.GetComponent<UIShiny>();if(!shine)shine=play.gameObject.AddComponent<UIShiny>();
            shine.width=.10f;shine.softness=.55f;shine.brightness=.8f;shine.gloss=.6f;shine.rotation=25;
            shine.effectPlayer.play=true;shine.effectPlayer.loop=true;shine.effectPlayer.duration=.65f;
            shine.effectPlayer.initialPlayDelay=1.35f;shine.effectPlayer.loopDelay=1.35f;
            shine.effectPlayer.updateMode=AnimatorUpdateMode.UnscaledTime;
            FantasyImage("Bottom panel",motion.navigation,ui[9],5,4,931,180,false);
            var shop=FantasyButton("Shop",motion.navigation,null,60,5,240,176);
            FantasyImage("Shop icon",shop.transform,ui[10],59,9,122,100,false);
            FantasyText("Caption",shop.transform,12,113,216,52,35,WoolArt.Hex("582407")).text="Shop";
            var home=FantasyButton("Home",motion.navigation,null,330,0,282,180);
            FantasyImage("Home icon",home.transform,ui[11],2,1,278,164,false);
            var ht=FantasyText("Caption",home.transform,30,119,222,51,35,Color.white);ht.text="Home";ht.fontStyle=FontStyles.Bold;
            var map=FantasyButton("Map",motion.navigation,null,640,5,240,176);
            FantasyImage("Map icon",map.transform,ui[12],56,10,127,101,false);
            FantasyText("Caption",map.transform,12,113,216,52,35,WoolArt.Hex("582407")).text="Map";
            referenceHome.SetAsLastSibling();
            canvas.pixelPerfect=true;motion.Fit();ConfigureInputTargets();
            menu.SetActive(true);homePage.gameObject.SetActive(true);shopPage.gameObject.SetActive(false);
            if(mapPage)mapPage.gameObject.SetActive(false);if(cardsPage)cardsPage.gameObject.SetActive(false);
            if(profilePanel)profilePanel.SetActive(false);if(dailyPanel)dailyPanel.SetActive(false);if(spinPanel)spinPanel.SetActive(false);
            modal.SetActive(false);launch.SetActive(false);safe.gameObject.SetActive(false);
            var oldNav=menu.transform.Find("Bottom navigation");if(oldNav)oldNav.gameObject.SetActive(false);
            BindReferencePages();portraitWidth=0;ApplyPortraitFrame(Screen.width,Screen.height,Screen.safeArea);
            UnityEditor.EditorUtility.SetDirty(this);UnityEditor.EditorUtility.SetDirty(motion);
            homeLevel.ForceMeshUpdate(true);
            return "Home saved: dragon below header, integrated static duck wings, stable high-resolution text, moving clouds and two-second UIShiny.";
        }
        RectTransform FantasyGroup(string name,Transform parent,float width,float height)
        {
            var t=parent.Find(name) as RectTransform;
            if(!t)t=Rect(name,parent,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(width,height),Vector2.zero);
            t.sizeDelta=new Vector2(width,height);t.gameObject.SetActive(true);t.localScale=Vector3.one;return t;
        }
        static void FantasyPlace(RectTransform rt,float x,float y,float w,float h)
        {
            rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(.5f,.5f);
            rt.sizeDelta=new Vector2(w,h);rt.anchoredPosition=new Vector2(x+w*.5f,-y-h*.5f);
            rt.localScale=Vector3.one;
        }
        RectTransform FantasyImage(string name,Transform parent,Sprite sprite,float x,float y,float w,float h,bool preserve)
        {
            var rt=parent.Find(name) as RectTransform;
            if(!rt)rt=Rect(name,parent,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero);
            rt.gameObject.SetActive(true);FantasyPlace(rt,x,y,w,h);
            var im=rt.GetComponent<Image>();if(!im)im=rt.gameObject.AddComponent<Image>();
            im.sprite=sprite;im.type=Image.Type.Simple;im.preserveAspect=preserve;im.color=Color.white;im.raycastTarget=false;
            return rt;
        }
        Button FantasyButton(string name,Transform parent,Sprite sprite,float x,float y,float w,float h)
        {
            // Locate existing reference buttons before moving them into responsive groups.
            var t=parent.Find(name)??referenceHome.Find(name);
            var b=t?t.GetComponent<Button>():ButtonAt(name,parent,"",new Vector2(w,h),Vector2.zero,null);
            b.transform.SetParent(parent,false);b.gameObject.SetActive(true);
            FantasyPlace((RectTransform)b.transform,x,y,w,h);
            var im=b.GetComponent<Image>();im.enabled=true;im.sprite=sprite;im.color=sprite?Color.white:Color.clear;
            im.type=Image.Type.Simple;im.preserveAspect=false;im.raycastTarget=true;b.targetGraphic=im;
            b.transition=Selectable.Transition.ColorTint;b.interactable=true;
            var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=Color.white;
            colors.pressedColor=new Color(.88f,.9f,.94f,1);b.colors=colors;
            foreach(var text in b.GetComponentsInChildren<TMP_Text>(true))text.gameObject.SetActive(false);
            return b;
        }
        TMP_Text FantasyText(string name,Transform parent,float x,float y,float w,float h,int size,Color color)
        {
            var t=parent.Find(name);var text=t?t.GetComponent<TMP_Text>():Label(name,parent,"",size,new Vector2(w,h),Vector2.zero,color);
            FantasyPlace(text.rectTransform,x,y,w,h);text.gameObject.SetActive(true);
            var sharpFont=UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/WoolGame/Fonts/LilitaOne-HomeSharp.asset");
            text.font=sharpFont?sharpFont:font;
            text.fontSharedMaterial=text.font.material;text.fontSize=size;text.fontSizeMax=size;text.fontSizeMin=size*.65f;
            text.extraPadding=true;text.isOrthographic=true;
            text.enableAutoSizing=true;text.color=color;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
            return text;
        }
        static void SetWingPivot(RectTransform wing,Vector2 pivot)
        {
            var delta=Vector2.Scale(pivot-wing.pivot,wing.sizeDelta);wing.pivot=pivot;wing.anchoredPosition+=delta;
        }
    }
}
#endif
