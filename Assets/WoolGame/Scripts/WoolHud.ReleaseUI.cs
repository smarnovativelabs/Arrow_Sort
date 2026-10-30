using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] RectTransform releaseHeaderArt;
        readonly Vector3[] releaseCanvasCorners=new Vector3[4];
        void FitReleaseUI()
        {
            if(releaseHeaderArt&&header&&canvas&&releaseHeaderArt.gameObject.activeSelf&&header.lossyScale.sqrMagnitude>.000001f)
            {
                ((RectTransform)canvas.rootCanvas.transform).GetWorldCorners(releaseCanvasCorners);
                var min=header.InverseTransformPoint(releaseCanvasCorners[0]);
                var max=header.InverseTransformPoint(releaseCanvasCorners[2]);
                float bottom=header.rect.yMin-12;
                releaseHeaderArt.anchorMin=releaseHeaderArt.anchorMax=new Vector2(.5f,.5f);
                releaseHeaderArt.pivot=new Vector2(.5f,.5f);
                releaseHeaderArt.localPosition=new Vector3((min.x+max.x)*.5f,(bottom+max.y)*.5f,0);
                releaseHeaderArt.sizeDelta=new Vector2(max.x-min.x+4,max.y-bottom+4);
            }
            if(!modalCard||!modal)return;
            var area=(RectTransform)modal.transform;
            if(area.rect.width<=0||area.rect.height<=0)return;
            modalCard.localScale=Vector3.one*Mathf.Min(1.25f,
                Mathf.Min((area.rect.width-64)/Mathf.Max(1,modalCard.sizeDelta.x),
                          (area.rect.height-80)/Mathf.Max(1,modalCard.sizeDelta.y)));
        }

#if UNITY_EDITOR
        const string ReleaseArt="Assets/WoolGame/Designer/UI/Release/";
        static Sprite ReleaseSprite(string name,bool sliced=false)
        {
            string path=ReleaseArt+name+".png";
            UnityEditor.AssetDatabase.ImportAsset(path);
            var importer=(UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(path);
            importer.textureType=UnityEditor.TextureImporterType.Sprite;
            importer.spriteImportMode=UnityEditor.SpriteImportMode.Single;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
            importer.textureCompression=UnityEditor.TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=1024;importer.filterMode=FilterMode.Bilinear;
            importer.spriteBorder=sliced?new Vector4(48,48,48,48):Vector4.zero;
            importer.SaveAndReimport();
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        public string AuthorReleaseUI()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Author UI outside Play Mode.");
            var newIcons=new[]{ReleaseSprite("AbilityUnlock"),ReleaseSprite("AbilityRemove"),ReleaseSprite("AbilitySort"),ReleaseSprite("AbilityFog")};
            var cream=ReleaseSprite("CreamPanel",true);
            var green=ReleaseSprite("GreenButton",true);
            var blue=ReleaseSprite("BlueButton",true);
            var abilityTile=ReleaseSprite("AbilityTile");
            var closeArt=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/ProfileEditor/Close.asset");
            var replacements=new Dictionary<Sprite,Sprite>();
            for(int k=0;k<4;k++)
            {
                if(icons[k])replacements[icons[k]]=newIcons[k];
                if(unlockPresentation&&unlockPresentation.abilityIcons[k])replacements[unlockPresentation.abilityIcons[k]]=newIcons[k];
                var oldLayer=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/Unlock/LayeredIcon"+k+".asset");
                if(oldLayer)replacements[oldLayer]=newIcons[k];
            }
            int replaced=0,preserved=0;
            foreach(var image in GetComponentsInChildren<UnityEngine.UI.Image>(true))
            {
                if(image.sprite&&replacements.TryGetValue(image.sprite,out var replacement))
                {image.sprite=replacement;image.type=UnityEngine.UI.Image.Type.Simple;image.preserveAspect=true;image.color=Color.white;replaced++;}
                // Photos, icons, characters and baked panels keep their native proportions.
                if(image.sprite&&image.type==UnityEngine.UI.Image.Type.Simple)
                {image.preserveAspect=true;preserved++;}
                if(image.sprite&&image.type==UnityEngine.UI.Image.Type.Sliced&&image.sprite.border==Vector4.zero)
                {image.type=UnityEngine.UI.Image.Type.Simple;image.preserveAspect=true;}
                UnityEditor.EditorUtility.SetDirty(image);
            }
            for(int k=0;k<4;k++)icons[k]=newIcons[k];
            if(unlockPresentation)
            {
                unlockPresentation.abilityIcons=newIcons;
                unlockPresentation.titles=new[]{"Unlock Slot","Remove Vehicle","Sort Colors","Pierce Fog"};
                unlockPresentation.descriptions=new[]{"Open one extra crossbow slot for this level.","Choose a vehicle and remove it from the board.","Rearrange the snake colors for a better match.","Let your crossbows shoot through the fog."};
                UnityEditor.EditorUtility.SetDirty(unlockPresentation);
            }
            if(skin!=null&&skin.Length>=9){skin[0]=cream;skin[1]=green;skin[2]=blue;}
            void Face(UnityEngine.UI.Image image,Sprite sprite)
            {if(!image)return;image.sprite=sprite;image.type=UnityEngine.UI.Image.Type.Sliced;image.preserveAspect=false;image.color=Color.white;UnityEditor.EditorUtility.SetDirty(image);}
            Face(modalCard.GetComponent<UnityEngine.UI.Image>(),cream);
            if(!releaseHeaderArt)releaseHeaderArt=Panel("Unified gameplay header",header,new Vector2(.5f,.5f),new Vector2(1080,220),Vector2.zero,Color.white);
            releaseHeaderArt.SetAsFirstSibling();Face(releaseHeaderArt.GetComponent<UnityEngine.UI.Image>(),cream);
            releaseHeaderArt.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            if(bannerArea){bannerArea.GetComponent<UnityEngine.UI.Image>().color=Color.clear;var placeholder=bannerArea.Find("Banner placeholder");if(placeholder)placeholder.gameObject.SetActive(false);}
            Face(primary.GetComponent<UnityEngine.UI.Image>(),green);Face(secondary.GetComponent<UnityEngine.UI.Image>(),blue);
            if(utilityPanel)
            {
                Face(utilityPanel.card.GetComponent<UnityEngine.UI.Image>(),cream);
                utilityPanel.greenButton=green;
                foreach(var b in new[]{utilityPanel.music,utilityPanel.sound,utilityPanel.daily,utilityPanel.video,utilityPanel.shop})
                    if(b)Face(b.GetComponent<UnityEngine.UI.Image>(),b==utilityPanel.shop?blue:green);
                UnityEditor.EditorUtility.SetDirty(utilityPanel);
            }
            string[] captions={"UNLOCK","REMOVE","SORT","PIERCE FOG"};
            for(int k=0;k<4;k++)
            {
                var button=footer.Find(WoolBoosters.Names[k]);if(!button)continue;
                var rt=(RectTransform)button;rt.sizeDelta=new Vector2(146,146);rt.anchoredPosition=new Vector2(-252+168*k,0);
                Face(button.GetComponent<UnityEngine.UI.Image>(),cream);
                var tile=button.GetComponent<WoolAbilityTileGraphic>();
                if(tile){tile.tile=abilityTile;tile.SetAllDirty();button.GetComponent<UnityEngine.UI.Button>().targetGraphic=tile;UnityEditor.EditorUtility.SetDirty(tile);}
                var icon=button.Find("Icon "+k) as RectTransform;
                if(icon){icon.gameObject.SetActive(true);icon.sizeDelta=new Vector2(105,105);icon.anchoredPosition=new Vector2(0,17);icon.GetComponent<UnityEngine.UI.Image>().sprite=newIcons[k];}
                var caption=button.Find("Label").GetComponent<TMP_Text>();caption.text=captions[k];caption.color=WoolArt.Hex("59361E");caption.fontSize=18;caption.fontSizeMax=18;caption.fontSizeMin=15;caption.enableAutoSizing=true;
                caption.gameObject.SetActive(true);
                caption.rectTransform.sizeDelta=new Vector2(138,28);caption.rectTransform.anchoredPosition=new Vector2(0,-47);
                var old=button.Find("Video option");if(old)old.gameObject.SetActive(false);
                var quantity=button.Find("Owned quantity");if(quantity){var badge=quantity.GetComponent<UnityEngine.UI.Image>();if(badge)badge.color=WoolArt.Hex("348F61");}
            }
            if(font)
            foreach(var text in GetComponentsInChildren<TMP_Text>(true))
            {
                // Keep custom title materials/illustrated logos and their intended colors.
                if(text.font!=font){text.font=font;text.fontSharedMaterial=font.material;}
                text.raycastTarget=false;text.extraPadding=true;
                if(text.text.Length>3&&text.fontSize>=18)
                {text.enableAutoSizing=true;text.fontSizeMax=Mathf.Max(18,text.fontSize);text.fontSizeMin=Mathf.Min(text.fontSizeMax,Mathf.Max(16,text.fontSize*.72f));}
                UnityEditor.EditorUtility.SetDirty(text);
            }
            foreach(var button in GetComponentsInChildren<UnityEngine.UI.Button>(true))
            {
                if(closeArt&&(button.name.StartsWith("Close",StringComparison.OrdinalIgnoreCase)||button.name.StartsWith("Dismiss",StringComparison.OrdinalIgnoreCase)||button==close))
                {
                    var image=button.GetComponent<UnityEngine.UI.Image>();
                    if(image){image.sprite=closeArt;image.type=UnityEngine.UI.Image.Type.Simple;image.preserveAspect=true;image.color=Color.white;}
                    var rect=(RectTransform)button.transform;float edge=Mathf.Min(rect.sizeDelta.x,rect.sizeDelta.y);
                    if(edge>0)rect.sizeDelta=Vector2.one*Mathf.Max(72,edge);
                    foreach(var label in button.GetComponentsInChildren<TMP_Text>(true))if(label.text=="×"||label.text=="x"||label.text=="X")label.gameObject.SetActive(false);
                }
                if(button.transition==Selectable.Transition.None)continue;
                var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1,.98f,.91f);
                colors.pressedColor=new Color(.82f,.86f,.77f);colors.disabledColor=new Color(.62f,.64f,.62f,.72f);colors.fadeDuration=.1f;button.colors=colors;
                UnityEditor.EditorUtility.SetDirty(button);
            }
            var layer=transform.Find("Full screen UI artwork") as RectTransform;
            if(!layer)layer=Rect("Full screen UI artwork",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            layer.SetAsFirstSibling();
            var full=layer.GetComponent<WoolFullBleedBackground>();if(!full)full=layer.gameObject.AddComponent<WoolFullBleedBackground>();
            var picture=layer.Find("Uncropped aspect cover") as RectTransform;
            if(!picture)picture=Rect("Uncropped aspect cover",layer,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.one,Vector2.zero);
            var background=picture.GetComponent<UnityEngine.UI.Image>();if(!background)background=picture.gameObject.AddComponent<UnityEngine.UI.Image>();
            background.raycastTarget=false;background.type=UnityEngine.UI.Image.Type.Simple;background.preserveAspect=true;
            var village=homePage.Find("Village background").GetComponent<UnityEngine.UI.Image>();
            var sources=new List<UnityEngine.UI.Image>{village};
            if(shopPage)sources.Add(shopPage.GetComponent<UnityEngine.UI.Image>());
            var shopArt=shopPage?shopPage.Find("Premium shop backdrop"):null;
            if(shopArt)
            {
                var image=shopArt.GetComponent<UnityEngine.UI.Image>();if(image&&image.sprite)sources.Add(image);
                var fitter=shopArt.GetComponent<AspectRatioFitter>();if(fitter)fitter.enabled=false;
                if(!shopArt.GetComponent<WoolCoverArtwork>())shopArt.gameObject.AddComponent<WoolCoverArtwork>();
            }
            if(mapPage)sources.Add(mapPage.GetComponent<UnityEngine.UI.Image>());
            if(skywayMap&&skywayMap.artwork)
            {
                var pictureSource=skywayMap.artwork.Find("Floating sky islands");
                if(pictureSource)sources.Add(pictureSource.GetComponent<UnityEngine.UI.Image>());
            }
            if(dailyTasksView&&dailyTasksView.fullBackground)sources.Add(dailyTasksView.fullBackground);
            full.menuRoot=menu;full.sources=sources.ToArray();full.surface=background;
            background.sprite=village.sprite;
            var mask=homePage.GetComponent<RectMask2D>();if(mask)mask.enabled=false;
            // Shop has a solid cream canvas; match its edge outside the physical safe area.
            if(shopPage)shopPage.GetComponent<UnityEngine.UI.Image>().color=WoolArt.Hex("FFF2D6");
            // Extend translucent screen dimmers only into the unsafe margins, avoiding double opacity.
            foreach(Transform child in transform)
            {
                var source=child.GetComponent<UnityEngine.UI.Image>();
                if(!source||child==layer)continue;
                bool overlay=child.gameObject==modal||child.gameObject==launch||child.gameObject==dailyPanel||child.gameObject==spinPanel||child.gameObject==profilePanel||(utilityPanel&&child.gameObject==utilityPanel.gameObject)||child.name=="Guided ability spotlight";
                if(!overlay)
                {
                    var obsolete=child.Find("Safe area backdrop extension");if(obsolete)UnityEditor.Undo.DestroyObjectImmediate(obsolete.gameObject);
                    continue;
                }
                var margin=child.Find("Safe area backdrop extension") as RectTransform;
                if(!margin)margin=Rect("Safe area backdrop extension",child,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
                margin.SetAsFirstSibling();
                if(!margin.GetComponent<CanvasRenderer>())margin.gameObject.AddComponent<CanvasRenderer>();
                var graphic=margin.GetComponent<WoolSafeAreaBackdrop>();if(!graphic)graphic=margin.gameObject.AddComponent<WoolSafeAreaBackdrop>();
                graphic.source=source;graphic.raycastTarget=false;UnityEditor.EditorUtility.SetDirty(graphic);
            }
            var overlayImages=GetComponentsInChildren<UnityEngine.UI.Image>(true);
            foreach(var source in overlayImages)
            {
                string name=source.name.ToLowerInvariant();
                bool dimmer=name.Contains("dimmer")||name.Contains("dimmed")||name=="black background"||name=="victory full screen background";
                if(source.gameObject==modal||source.gameObject==launch||source.gameObject==dailyPanel||source.gameObject==spinPanel||source.gameObject==profilePanel)dimmer=true;
                if(utilityPanel&&source.gameObject==utilityPanel.gameObject)dimmer=true;
                if(!dimmer)continue;
                var margin=source.transform.Find("Safe area backdrop extension") as RectTransform;
                if(!margin)margin=Rect("Safe area backdrop extension",source.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
                margin.SetAsFirstSibling();
                if(!margin.GetComponent<CanvasRenderer>())margin.gameObject.AddComponent<CanvasRenderer>();
                var graphic=margin.GetComponent<WoolSafeAreaBackdrop>();if(!graphic)graphic=margin.gameObject.AddComponent<WoolSafeAreaBackdrop>();
                graphic.source=source;graphic.raycastTarget=false;UnityEditor.EditorUtility.SetDirty(graphic);
            }
            portraitWidth=0;RefreshSafeArea();ConfigureInputTargets();FitReleaseUI();
            UnityEditor.EditorUtility.SetDirty(full);UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            UnityEditor.AssetDatabase.SaveAssets();UnityEditor.SceneManagement.EditorSceneManager.SaveScene(gameObject.scene);
            return "Unified UI saved; "+replaced+" ability images replaced; "+preserved+" images preserve aspect; physical-screen background authored.";
        }
#endif
    }
}
