// [UNITY-SKILL:SPRITEATLAS]
// [TYPE:PREBUILD] Core UI is delivered inside the APK.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
namespace WoolGame.Editor
{
    public sealed class WoolSpriteAtlasPipeline : IPreprocessBuildWithReport
    {
        const string AtlasPath="Assets/WoolGame/Atlases/CoreUI.spriteatlasv2";
        public int callbackOrder=>-100;
        public void OnPreprocessBuild(BuildReport report){Generate();}
        public static string Generate()
        {
            EditorSettings.spritePackerMode=SpritePackerMode.SpriteAtlasV2;
            if(EditorSettings.spritePackerMode==SpritePackerMode.Disabled)throw new BuildFailedException("Sprite packing is disabled.");
            var sprites=new HashSet<Sprite>();
            foreach(var component in Resources.FindObjectsOfTypeAll<Component>())
            {
                if(!component||!component.gameObject.scene.IsValid())continue;
                var serialized=new SerializedObject(component);var property=serialized.GetIterator();
                while(property.NextVisible(true))
                    if(property.propertyType==SerializedPropertyType.ObjectReference&&property.objectReferenceValue is Sprite sprite&&EditorUtility.IsPersistent(sprite))sprites.Add(sprite);
            }
            // Backgrounds and runtime pixel-sliced source sheets keep their original texture.
            // Packing only referenced small sprites avoids importing unused sheets into the APK.
            var largeSources=new HashSet<Texture2D>(sprites.Where(s=>s.rect.width>512||s.rect.height>512).Select(OriginalTexture));
            var icons=sprites.Where(s=>AssetDatabase.GetAssetPath(s).StartsWith("Assets/WoolGame/",StringComparison.Ordinal)&&!AssetDatabase.GetAssetPath(OriginalTexture(s)).Contains("/Resources/")&&!largeSources.Contains(OriginalTexture(s))&&s.rect.width<=512&&s.rect.height<=512).OrderBy(AssetDatabase.GetAssetPath).Cast<UnityEngine.Object>().ToArray();
            if(icons.Length==0)return "No eligible UI sprites";
            Directory.CreateDirectory(Path.GetDirectoryName(AtlasPath));
            var atlas=File.Exists(AtlasPath)?SpriteAtlasAsset.Load(AtlasPath):new SpriteAtlasAsset();
            if(File.Exists(AtlasPath))
            {
                var runtime=AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AtlasPath);
                if(runtime)atlas.Remove(runtime.GetPackables());
            }
            atlas.Add(icons);SpriteAtlasAsset.Save(atlas,AtlasPath);AssetDatabase.ImportAsset(AtlasPath);
            var importer=(SpriteAtlasImporter)AssetImporter.GetAtPath(AtlasPath);
            var packing=importer.packingSettings;packing.padding=4;packing.enableRotation=false;packing.enableTightPacking=false;packing.enableAlphaDilation=true;importer.packingSettings=packing;
            var texture=importer.textureSettings;texture.generateMipMaps=false;texture.readable=false;texture.filterMode=FilterMode.Bilinear;importer.textureSettings=texture;
            importer.includeInBuild=true;
            importer.SetPlatformSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=2048,format=TextureImporterFormat.ASTC_4x4,compressionQuality=100});
            importer.SaveAndReimport();AssetDatabase.SaveAssets();
            var message="SpriteAtlasV2: "+icons.Length+" referenced UI sprites, no rotation, Android ASTC 4x4";
            Directory.CreateDirectory("Builds/QA");File.WriteAllText("Builds/QA/sprite-atlas-report.txt",message);
            return message;
        }
        public static string PackPreview()
        {
            var atlas=AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AtlasPath);
            if(!atlas)throw new Exception("Generate the core UI atlas first.");
            SpriteAtlasUtility.PackAtlases(new[]{atlas},EditorUserBuildSettings.activeBuildTarget,false);
            return "Packed core UI preview: "+atlas.spriteCount+" sprites";
        }
        static Texture2D OriginalTexture(Sprite sprite)=>UnityEditor.Sprites.SpriteUtility.GetSpriteTexture(sprite,false);
    }
}
