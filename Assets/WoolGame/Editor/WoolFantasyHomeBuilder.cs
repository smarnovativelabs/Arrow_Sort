using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using TMPro;
using UnityEngine.TextCore.LowLevel;

namespace WoolGame.Editor
{
    public static class WoolFantasyHomeBuilder
    {
        const string Folder="Assets/WoolGame/Designer/UI/FantasyHome/";
        [MenuItem("Wool Crush/Author layered fantasy home")]
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before authoring the home screen.");
            var ui=Slice("InterfaceLayers",new[]{
                new Rect(16,24,277,284),new Rect(309,124,426,141),new Rect(744,74,217,216),new Rect(966,67,278,250),
                new Rect(35,366,244,274),new Rect(328,359,276,284),new Rect(659,360,259,283),new Rect(952,377,292,231),
                new Rect(6,739,375,160),new Rect(381,755,372,143),new Rect(750,724,215,173),new Rect(964,710,280,202),
                new Rect(27,981,274,219),new Rect(316,977,314,220),new Rect(618,940,379,306),new Rect(1005,963,183,225)});
            var chars=Slice("CharacterLayers",new[]{new Rect(0,0,594,657),new Rect(594,0,660,657),new Rect(0,675,625,579),new Rect(625,675,629,579)});
            var wings=Slice("ProtectiveWings",new[]{new Rect(0,0,627,1254),new Rect(627,0,627,1254)});
            chars[2]=wings[0];chars[3]=wings[1];
            Slice("DuckFamilyFixed",new[]{new Rect(0,0,1254,1254)});
            PrepareHomeFont();
            var importer=(TextureImporter)AssetImporter.GetAtPath(Folder+"VillageCloudless.png");
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.npotScale=TextureImporterNPOTScale.None;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            var hud=UnityEngine.Object.FindFirstObjectByType<WoolHud>();
            Undo.RegisterFullObjectHierarchyUndo(hud.gameObject,"Author layered fantasy home");
            string result=hud.AuthorFantasyHome(AssetDatabase.LoadAssetAtPath<Sprite>(Folder+"VillageCloudless.png"),ui,chars);
            EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);EditorSceneManager.SaveScene(hud.gameObject.scene);
            AssetDatabase.SaveAssets();return result;
        }
        static void PrepareHomeFont()
        {
            const string path="Assets/WoolGame/Fonts/LilitaOne-HomeSharp.asset";
            var sharp=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if(sharp)return;
            var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/WoolGame/Fonts/LilitaOne-Regular.ttf");
            sharp=TMP_FontAsset.CreateFontAsset(source,128,16,GlyphRenderMode.SDFAA,2048,2048,AtlasPopulationMode.Dynamic,true);
            sharp.name="LilitaOne Home Sharp";
            string glyphs="";for(int i=32;i<127;i++)glyphs+=(char)i;
            sharp.TryAddCharacters(glyphs+"—•×");
            AssetDatabase.CreateAsset(sharp,path);AssetDatabase.AddObjectToAsset(sharp.material,sharp);
            foreach(var atlas in sharp.atlasTextures){atlas.filterMode=FilterMode.Bilinear;AssetDatabase.AddObjectToAsset(atlas,sharp);}
            sharp.material.SetFloat("_PerspectiveFilter",0);sharp.material.SetFloat("_Sharpness",.15f);
            EditorUtility.SetDirty(sharp);EditorUtility.SetDirty(sharp.material);AssetDatabase.SaveAssets();
        }
        static Sprite[] Slice(string name,Rect[] regions)
        {
            string path=Folder+name+".png";var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.isReadable=true;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;
            importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var pixels=texture.GetPixels32();
            var factory=new SpriteDataProviderFactories();factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var previous=provider.GetSpriteRects();var rects=new SpriteRect[regions.Length];
            for(int i=0;i<regions.Length;i++)
            {
                var area=regions[i];float sx=texture.width/1254f,sy=texture.height/1254f;
                int left=Mathf.Clamp(Mathf.RoundToInt(area.x*sx),0,texture.width-1),right=Mathf.Clamp(Mathf.RoundToInt(area.xMax*sx),1,texture.width);
                int bottom=Mathf.Clamp(Mathf.RoundToInt(texture.height-area.yMax*sy),0,texture.height-1),top=Mathf.Clamp(Mathf.RoundToInt(texture.height-area.y*sy),1,texture.height);
                int minX=right,minY=top,maxX=left,maxY=bottom;
                for(int y=bottom;y<top;y++)for(int x=left;x<right;x++)if(pixels[y*texture.width+x].a>96)
                {minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);}
                minX=Mathf.Max(left,minX-2);minY=Mathf.Max(bottom,minY-2);maxX=Mathf.Min(right-1,maxX+2);maxY=Mathf.Min(top-1,maxY+2);
                string spriteName=name+"_"+i.ToString("00");var old=previous.FirstOrDefault(s=>s.name==spriteName);
                rects[i]=new SpriteRect{name=spriteName,rect=new Rect(minX,minY,maxX-minX+1,maxY-minY+1),pivot=new Vector2(.5f,.5f),alignment=SpriteAlignment.Center,spriteID=old!=null?old.spriteID:GUID.Generate()};
            }
            provider.SetSpriteRects(rects);var names=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if(names!=null)names.SetNameFileIdPairs(rects.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
            provider.Apply();importer.SaveAndReimport();importer.isReadable=false;importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
        }
    }
}
