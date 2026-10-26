using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] WoolProfileView profileView;
        [SerializeField] Image homeAvatar;
        void BindProfileView(){if(profileView){profileView.Bind(RefreshHomeAvatar);RefreshHomeAvatar();}}
        void RefreshHomeAvatar(){if(homeAvatar&&profileView&&profileView.avatars.Length==6)homeAvatar.sprite=profileView.avatars[WoolProgress.AvatarIndex];}
#if UNITY_EDITOR
        public string AuthorProfileView(Sprite art,Sprite[] avatars,Sprite green,Sprite pencilSprite)
        {
            var overlay=transform.Find("Duck profile") as RectTransform;
            if(!overlay)overlay=Rect("Duck profile",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            profileView=overlay.GetComponent<WoolProfileView>();if(!profileView)profileView=overlay.gameObject.AddComponent<WoolProfileView>();
            profileView.avatars=avatars;
            profileView.dimmer=FantasyImage("Dimmed background",overlay,null,0,0,941,1672,false).GetComponent<Image>();profileView.dimmer.color=new Color(0,0,0,.8f);
            var card=overlay.Find("Profile canvas") as RectTransform;if(!card)card=new GameObject("Profile canvas",typeof(RectTransform),typeof(CanvasGroup)).GetComponent<RectTransform>();
            card.SetParent(overlay,false);profileView.card=card;profileView.fade=card.GetComponent<CanvasGroup>();
            var main=FantasyImage("Profile illustration",card,art,0,0,941,1672,false);profileView.mainScreen=main.gameObject;
            var avatarMask=main.Find("Rounded avatar") as RectTransform;
            if(!avatarMask)avatarMask=new GameObject("Rounded avatar",typeof(RectTransform),typeof(CanvasRenderer),typeof(WoolRoundedClip),typeof(Mask)).GetComponent<RectTransform>();
            avatarMask.SetParent(main,false);FantasyPlace(avatarMask,94,232,190,180);avatarMask.GetComponent<Mask>().showMaskGraphic=false;avatarMask.GetComponent<WoolRoundedClip>().raycastTarget=false;
            profileView.portrait=FantasyImage("Selected avatar",avatarMask,avatars[0],0,0,190,180,false).GetComponent<Image>();
            var brown=WoolArt.Hex("70280E");
            profileView.nameLabel=FantasyText("Player name",main,330,267,325,82,43,Color.white);
            profileView.nameLabel.fontSizeMin=18;
            profileView.idLabel=FantasyText("Player ID",main,330,355,320,36,22,WoolArt.Hex("263887"));
            var outline=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/WoolGame/Designer/UI/SpecialOffer/OfferText.mat");if(outline)profileView.nameLabel.fontSharedMaterial=outline;
            profileView.levelLabel=FantasyText("Current level",main,705,265,110,82,70,WoolArt.Hex("FFF4CD"));if(outline)profileView.levelLabel.fontSharedMaterial=outline;

            profileView.values=new TMP_Text[6];float[] xs={112,515},ys={810,1126,1440};
            for(int i=0;i<6;i++)profileView.values[i]=FantasyText("Live stat "+i,main,xs[i%2],ys[i/2],308,72,55,brown);
            profileView.close=FantasyButton("Close profile",main,null,778,55,113,113);
            profileView.pencil=FantasyButton("Edit avatar",main,pencilSprite,235,350,90,87);
            profileView.nameButton=FantasyButton("Edit name",main,null,330,267,325,82);
            var edit=card.Find("Edit profile") as RectTransform;if(!edit)edit=new GameObject("Edit profile",typeof(RectTransform)).GetComponent<RectTransform>();edit.SetParent(card,false);FantasyPlace(edit,0,0,941,1672);profileView.editScreen=edit.gameObject;
            RectTransform Surface(string name,WoolSpinSurface.Kind kind,float x,float y,float w,float h)
            {
                var rt=edit.Find(name) as RectTransform;if(!rt)rt=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(WoolSpinSurface)).GetComponent<RectTransform>();rt.SetParent(edit,false);FantasyPlace(rt,x,y,w,h);var g=rt.GetComponent<WoolSpinSurface>();g.kind=kind;g.raycastTarget=false;g.SetVerticesDirty();return rt;
            }
            Surface("Editor card",WoolSpinSurface.Kind.CreamCard,35,105,871,1495);
            Surface("Editor banner",WoolSpinSurface.Kind.Banner,90,60,761,145);
            var title=FantasyText("Editor title",edit,120,90,701,85,58,Color.white);title.text="Make It Yours";if(outline)title.fontSharedMaterial=outline;
            var previewMask=edit.Find("Avatar preview") as RectTransform;
            if(!previewMask)previewMask=new GameObject("Avatar preview",typeof(RectTransform),typeof(CanvasRenderer),typeof(WoolRoundedClip),typeof(Mask)).GetComponent<RectTransform>();
            previewMask.SetParent(edit,false);FantasyPlace(previewMask,385,240,170,170);previewMask.GetComponent<Mask>().showMaskGraphic=false;previewMask.GetComponent<WoolRoundedClip>().raycastTarget=false;
            profileView.selectedPreview=FantasyImage("Preview portrait",previewMask,avatars[0],0,0,170,170,false).GetComponent<Image>();
            profileView.selectionName=FantasyText("Selected duck name",edit,200,430,541,45,35,brown);
            profileView.editorIdLabel=FantasyText("Immutable player ID",edit,120,485,701,35,22,brown);
            var nameTitle=FantasyText("Name title",edit,115,545,711,42,31,brown);nameTitle.text="Your name";
            Surface("Name field border",WoolSpinSurface.Kind.CreamCard,105,590,731,120);
            var inputRT=edit.Find("Name input") as RectTransform;
            if(!inputRT)inputRT=new GameObject("Name input",typeof(RectTransform),typeof(Image),typeof(TMP_InputField)).GetComponent<RectTransform>();inputRT.SetParent(edit,false);FantasyPlace(inputRT,115,600,711,100);
            var inputImage=inputRT.GetComponent<Image>();inputImage.color=Color.clear;
            profileView.nameInput=inputRT.GetComponent<TMP_InputField>();profileView.nameInput.characterLimit=0;profileView.nameInput.lineType=TMP_InputField.LineType.SingleLine;
            var text=FantasyText("Input text",inputRT,24,7,663,86,42,brown);text.alignment=TextAlignmentOptions.MidlineLeft;text.richText=false;
            profileView.nameInput.textViewport=inputRT;profileView.nameInput.textComponent=text;profileView.nameInput.targetGraphic=inputImage;
            var choose=FantasyText("Choose avatar title",edit,100,745,741,60,39,brown);choose.text="Choose an avatar";
            profileView.choices=new Button[6];profileView.selectedLabels=new TMP_Text[6];
            profileView.choiceHighlights=new GameObject[6];
            var obsoleteName=edit.Find("Duck name");if(obsoleteName)obsoleteName.gameObject.SetActive(false);
            for(int i=0;i<6;i++)
            {
                float x=125+(i%3)*245,y=830+(i/3)*280;
                profileView.choiceHighlights[i]=Surface("Selected frame "+i,WoolSpinSurface.Kind.Banner,x-13,y-13,226,259).gameObject;
                var baseCard=Surface("Avatar card "+i,WoolSpinSurface.Kind.CreamCard,x-6,y-6,212,243);
                profileView.choices[i]=FantasyButton("Avatar "+i,edit,null,x,y,200,200);
                var rounded=profileView.choices[i].transform.Find("Rounded portrait") as RectTransform;
                if(!rounded)rounded=new GameObject("Rounded portrait",typeof(RectTransform),typeof(CanvasRenderer),typeof(WoolRoundedClip),typeof(Mask)).GetComponent<RectTransform>();
                rounded.SetParent(profileView.choices[i].transform,false);FantasyPlace(rounded,0,0,200,200);rounded.GetComponent<Mask>().showMaskGraphic=false;rounded.GetComponent<WoolRoundedClip>().raycastTarget=false;
                FantasyImage("Duck portrait",rounded,avatars[i],0,0,200,200,false);
                var avatarName=FantasyText("Duck name "+i,edit,x-5,y+205,210,37,25,brown);avatarName.text=new[]{"Royal","Explorer","Wizard","Pilot","Chef","Garden"}[i];
                profileView.selectedLabels[i]=FantasyText("Selected",profileView.choices[i].transform,16,165,168,33,21,Color.white);profileView.selectedLabels[i].text="SELECTED";if(outline)profileView.selectedLabels[i].fontSharedMaterial=outline;
                profileView.selectedLabels[i].transform.SetAsLastSibling();
            }
            profileView.save=FantasyButton("Save profile",edit,green,190,1435,561,145);var saveText=FantasyText("Save caption",profileView.save.transform,20,20,521,105,59,Color.white);saveText.text="SAVE PROFILE";
            var levelMaterial=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/WoolGame/Designer/UI/FantasyHome/LevelText.mat");if(levelMaterial)saveText.fontSharedMaterial=levelMaterial;
            profileView.cancel=FantasyButton("Cancel editing",edit,skin[3],817,62,90,90);
            var backgrounds=new System.Collections.Generic.List<Transform>();
            foreach(Transform child in edit)if(child.GetComponent<WoolSpinSurface>())backgrounds.Add(child);
            for(int i=backgrounds.Count-1;i>=0;i--)backgrounds[i].SetAsFirstSibling();
            edit.Find("Editor card").SetAsFirstSibling();
            // Mirror the saved avatar in the existing home portrait button.
            var top=referenceHome?referenceHome.Find("Header controls/Profile"):null;
            if(top)
            {
                var legacyBackdrop=top.GetComponent<Image>();if(legacyBackdrop){legacyBackdrop.sprite=null;legacyBackdrop.color=Color.clear;}
                var oldIcon=top.Find("Saved duck avatar");
                var mask=oldIcon as RectTransform;if(!mask)mask=new GameObject("Saved duck avatar",typeof(RectTransform),typeof(CanvasRenderer),typeof(WoolSpinLamp),typeof(Mask)).GetComponent<RectTransform>();
                mask.SetParent(top,false);FantasyPlace(mask,4,7.5f,120,120);mask.GetComponent<Mask>().showMaskGraphic=false;mask.GetComponent<WoolSpinLamp>().raycastTarget=false;
                homeAvatar=FantasyImage("Portrait",mask,avatars[0],0,0,120,120,false).GetComponent<Image>();
            }
            BindProfileView();ConfigureInputTargets();profileView.Fit();edit.gameObject.SetActive(false);overlay.gameObject.SetActive(false);
            UnityEditor.EditorUtility.SetDirty(profileView);UnityEditor.EditorUtility.SetDirty(this);return "Profile, editable name, six-avatar picker and live stats saved.";
        }
#endif
    }
}
