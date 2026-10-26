using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Coffee.UIEffects;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] RectTransform referenceHome,referenceProfile,referenceSpin;
        void FitReferencePages()
        {
            var animatedHome=referenceHome?referenceHome.GetComponent<WoolHomePresentation>():null;
            if(animatedHome)animatedHome.Fit();else FitReference(referenceHome,homePage,0);
            FitReference(referenceProfile,profilePanel?profilePanel.transform as RectTransform:null,32);
            FitReference(referenceSpin,spinPanel?spinPanel.transform as RectTransform:null,32);
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
            BindNav(actions,"Ad Pass",ShowNoAdsStatus);BindNav(navigation,"Shop",ShowShop);
            BindNav(navigation,"Home",()=>Navigate(2));BindNav(navigation,"Map",ShowMap);
            FitReferencePages();
        }
#if UNITY_EDITOR
        const string RefPath="Assets/WoolGame/Designer/UI/Reference/";
        static Sprite ReferenceSprite(string name)
        {
            string path=RefPath+name+".png";
            var importer=UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
            importer.textureType=UnityEditor.TextureImporterType.Sprite;importer.spriteImportMode=UnityEditor.SpriteImportMode.Single;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=2048;
            importer.textureCompression=UnityEditor.TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        // Reference coordinates are top-left pixels on a 941 x 1672 design canvas.
        static void ReferencePlace(RectTransform rt,float x,float y,float w,float h)
        {rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.pivot=new Vector2(.5f,.5f);rt.sizeDelta=new Vector2(w,h);rt.anchoredPosition=new Vector2(x+w*.5f-470.5f,836-y-h*.5f);rt.localScale=Vector3.one;}
        RectTransform ReferenceRoot(string name,Transform parent,Sprite sprite)
        {
            var existing=parent.Find(name) as RectTransform;
            var rt=existing?existing:Rect(name,parent,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(941,1672),Vector2.zero);
            rt.sizeDelta=new Vector2(941,1672);rt.anchoredPosition=Vector2.zero;
            var img=rt.GetComponent<Image>();if(!img)img=rt.gameObject.AddComponent<Image>();img.sprite=sprite;img.type=Image.Type.Simple;img.color=Color.white;img.raycastTarget=false;
            return rt;
        }
        static Sprite ReferenceCrop(Sprite source,string name,float x,float y,float w,float h)
        {
            string path=RefPath+name+".asset";
            var sprite=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite)return sprite;
            float sx=source.texture.width/941f,sy=source.texture.height/1672f;
            sprite=Sprite.Create(source.texture,new Rect(x*sx,source.texture.height-(y+h)*sy,w*sx,h*sy),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            sprite.name=name;UnityEditor.AssetDatabase.CreateAsset(sprite,path);return sprite;
        }
        Button ReferenceButton(Transform parent,string name,float x,float y,float w,float h,Sprite art=null)
        {
            var t=parent.Find(name);var b=t?t.GetComponent<Button>():ButtonAt(name,parent,"",new Vector2(w,h),Vector2.zero,null);
            ReferencePlace(b.GetComponent<RectTransform>(),x,y,w,h);
            var image=b.GetComponent<Image>();image.sprite=art;image.type=Image.Type.Simple;image.color=art?Color.white:Color.clear;image.raycastTarget=true;
            b.transition=art?Selectable.Transition.ColorTint:Selectable.Transition.None;
            foreach(var text in b.GetComponentsInChildren<TMP_Text>(true))text.gameObject.SetActive(false);
            return b;
        }
        TMP_Text ReferenceText(Transform parent,string name,float x,float y,float w,float h,int size,Color color)
        {
            var t=parent.Find(name);var label=t?t.GetComponent<TMP_Text>():Label(name,parent,"",size,new Vector2(w,h),Vector2.zero,color);
            ReferencePlace(label.rectTransform,x,y,w,h);label.fontSize=size;label.fontSizeMax=size;label.fontSizeMin=size*.55f;label.enableAutoSizing=true;label.color=color;label.raycastTarget=false;label.gameObject.SetActive(true);
            return label;
        }
        static void Shine(GameObject target,float delay)
        {
            var shine=target.GetComponent<UIShiny>();if(!shine)shine=target.AddComponent<UIShiny>();
            shine.width=.12f;shine.softness=.65f;shine.brightness=.28f;shine.gloss=.35f;shine.rotation=25;
            shine.effectPlayer.play=true;shine.effectPlayer.loop=true;shine.effectPlayer.duration=1.2f;
            shine.effectPlayer.initialPlayDelay=delay;shine.effectPlayer.loopDelay=5;shine.effectPlayer.updateMode=AnimatorUpdateMode.UnscaledTime;
            UnityEditor.EditorUtility.SetDirty(shine);
        }
        public string AuthorReferencePages()
        {
            var home=ReferenceSprite("Home");var profile=ReferenceSprite("Profile");var spin=ReferenceSprite("Spin");var wheel=ReferenceSprite("Wheel");
            foreach(Transform t in homePage)if(t.name!="Reference home")t.gameObject.SetActive(false);
            referenceHome=ReferenceRoot("Reference home",homePage,home);
            homePage.GetComponent<RectMask2D>().enabled=false;menu.GetComponent<Image>().color=WoolArt.Hex("8BCDDC");
            ReferenceButton(referenceHome,"Profile",35,20,130,135);ReferenceButton(referenceHome,"Settings",808,25,110,120);
            ReferenceButton(referenceHome,"Wallet",490,35,305,95);ReferenceButton(referenceHome,"Tasks",42,1010,206,207);
            ReferenceButton(referenceHome,"Lucky Spin",267,1010,200,207);ReferenceButton(referenceHome,"Rescue Shop",482,1010,203,207);
            ReferenceButton(referenceHome,"Ad Pass",699,1010,205,207);
            var play=ReferenceButton(referenceHome,"Play",163,1232,614,189,ReferenceCrop(home,"HomePlay",163,1232,614,189));Shine(play.gameObject,1);
            homeLevel=ReferenceText(referenceHome,"Live level",213,1260,514,125,84,Color.white);homeLevel.outlineWidth=.18f;homeLevel.outlineColor=WoolArt.Hex("226B05");
            homeCoins=ReferenceText(referenceHome,"Live coins",578,49,148,65,48,WoolArt.Hex("65300A"));
            homeDifficulty=ReferenceText(referenceHome,"Live difficulty",220,1415,500,52,30,Color.white);
            ReferenceButton(referenceHome,"Shop",60,1490,245,165);ReferenceButton(referenceHome,"Home",330,1490,285,165);ReferenceButton(referenceHome,"Map",640,1490,245,165);
            // Keep existing panel owners and data fields; replace only their visual children.
            var oldProfile=profilePanel.transform.Find("Profile card");if(oldProfile)oldProfile.gameObject.SetActive(false);
            referenceProfile=ReferenceRoot("Reference profile",profilePanel.transform,profile);
            profilePanel.GetComponent<Image>().color=new Color(.09f,.05f,.02f,.72f);
            profileClose=ReferenceButton(referenceProfile,"Close profile",774,70,140,145);
            profileName=ReferenceText(referenceProfile,"Live player",437,370,411,95,43,WoolArt.Hex("65300A"));
            profileValues=new TMP_Text[4];
            for(int i=0;i<4;i++)profileValues[i]=ReferenceText(referenceProfile,"Live stat "+i,i%2==0?104:512,i<2?962:1404,327,90,77,WoolArt.Hex("542000"));
            var oldSpin=spinPanel.transform.Find("Spin card");if(oldSpin)oldSpin.gameObject.SetActive(false);
            referenceSpin=ReferenceRoot("Reference spin",spinPanel.transform,spin);
            spinPanel.GetComponent<Image>().color=new Color(.09f,.05f,.02f,.72f);
            spinClose=ReferenceButton(referenceSpin,"Close spin",784,138,131,140);
            var wheelRect=referenceSpin.Find("Rotating wheel") as RectTransform;
            if(!wheelRect)wheelRect=Rect("Rotating wheel",referenceSpin,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);
            ReferencePlace(wheelRect,60,326,820,820);spinWheel=wheelRect;spinWheel.localRotation=Quaternion.identity;
            var wi=wheelRect.GetComponent<Image>();if(!wi)wi=wheelRect.gameObject.AddComponent<Image>();wi.sprite=wheel;wi.raycastTarget=false;
            var pointer=referenceSpin.Find("Fixed pointer") as RectTransform;
            if(!pointer)pointer=Rect("Fixed pointer",referenceSpin,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);
            ReferencePlace(pointer,415,310,110,114);var pg=pointer.GetComponent<WoolWheelGraphic>();if(!pg)pg=pointer.gameObject.AddComponent<WoolWheelGraphic>();pg.pointer=true;pg.raycastTarget=false;
            spinButton=ReferenceButton(referenceSpin,"Spin",98,1173,744,193,ReferenceCrop(spin,"SpinButton",98,1173,744,193));Shine(spinButton.gameObject,2.7f);spinButton.transition=Selectable.Transition.None;
            var label=spinButton.GetComponentInChildren<TMP_Text>(true);label.gameObject.SetActive(true);label.font=font;label.fontSharedMaterial=font.material;label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;label.rectTransform.offsetMin=new Vector2(30,32);label.rectTransform.offsetMax=new Vector2(-30,-32);label.enableAutoSizing=true;label.fontSizeMin=28;label.fontSizeMax=62;label.color=Color.white;
            spinMessage=ReferenceText(referenceSpin,"Live spin status",112,1103,718,66,26,WoolArt.Hex("7A4020"));
            // Carry the same glossy finish through the existing shop, map, tasks and dialogs.
            foreach(var button in GetComponentsInChildren<Button>(true))
            {
                if(button.transform.IsChildOf(referenceHome)||button.transform.IsChildOf(referenceProfile)||button.transform.IsChildOf(referenceSpin))continue;
                if(button.name=="Purchase"||button.name=="Claim"||button.name=="Primary"||button.name=="Reward video")Shine(button.gameObject,1.8f);
            }
            foreach(var title in new[]{modalCard, dailyPanel.transform.Find("Daily card") as RectTransform})if(title)Shine(title.gameObject,3.5f);
            FitReferencePages();ConfigureInputTargets();UnityEditor.EditorUtility.SetDirty(this);
            return "Reference Home/Profile/Spin artwork, live data, interactive targets and UIShiny configured.";
        }
#endif
    }
}

