using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace WoolGame.Editor
{
    public static class WoolTileEnvironmentAuthoring
    {
        public const string MainScene="Assets/Scenes/Arrow Sort.unity";
        public const string TexturePath="Assets/Art/Texture/Dragon_Game_Background_Tiles.png";
        const string MaterialPath="Assets/WoolGame/Designer/TiledGameplayBackground.mat";
        const string FloorSourcePath="Assets/Art/Texture/ReferenceFloorSource.png";
        [MenuItem("Arrow Sort/Environment/Apply approved tiles to main game")]
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode first.");
            var current=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(current.path==MainScene&&current.isDirty)throw new InvalidOperationException("Save the main scene before applying environment art.");
            if(current.path!=MainScene)EditorSceneManager.OpenScene(MainScene);
            AssetDatabase.ImportAsset(TexturePath,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(TexturePath);
            importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;
            importer.isReadable=false;importer.mipmapEnabled=false;importer.alphaSource=TextureImporterAlphaSource.None;
            importer.maxTextureSize=2048;importer.npotScale=TextureImporterNPOTScale.None;
            importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
            importer.textureCompression=TextureImporterCompression.Compressed;
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=2048,format=TextureImporterFormat.ETC2_RGB4,compressionQuality=50});
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="iPhone",overridden=true,maxTextureSize=2048,format=TextureImporterFormat.ASTC_6x6,compressionQuality=50});
            importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            var rig=UnityEngine.Object.FindFirstObjectByType<WoolSceneRig>();
            var ground=rig.authoredLayout.environment.transform.Find("Game_Background_Grass_NoPath").GetComponent<Renderer>();
            var material=AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if(!material){material=new Material(ground.sharedMaterial);AssetDatabase.CreateAsset(material,MaterialPath);}
            material.name="Approved tiled gameplay background";
            material.shader=Shader.Find("WoolGame/Tiled Environment");
            material.SetTexture("_BaseMap",texture);material.SetColor("_BaseColor",Color.white);
            ConfigureReferenceFloor(material);
            ground.sharedMaterial=material;rig.gameplayBoardTexture=texture;
            EditorUtility.SetDirty(material);EditorUtility.SetDirty(rig);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(MainScene,true)};
            EditorSceneManager.MarkSceneDirty(rig.gameObject.scene);EditorSceneManager.SaveScene(rig.gameObject.scene);
            foreach(string asset in new[]{"Assets/Scenes/Two Environment Preview.unity","Assets/WoolGame/TemporaryPreview","Assets/WoolGame/Shaders/EnvironmentPreview.shader","Assets/Builds"})
                if(!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(asset)))AssetDatabase.DeleteAsset(asset);
            AssetDatabase.SaveAssets();
            return "Main campaign restored; approved tile image assigned; test scene, materials, levels and shader removed.";
        }
        public static void ConfigureReferenceFloor(Material material)
        {
            AssetDatabase.ImportAsset(FloorSourcePath,ImportAssetOptions.ForceSynchronousImport);
            var sourceImporter=(TextureImporter)AssetImporter.GetAtPath(FloorSourcePath);
            sourceImporter.textureType=TextureImporterType.Default;sourceImporter.sRGBTexture=true;
            sourceImporter.isReadable=false;sourceImporter.mipmapEnabled=false;
            sourceImporter.alphaSource=TextureImporterAlphaSource.None;sourceImporter.maxTextureSize=512;
            sourceImporter.npotScale=TextureImporterNPOTScale.None;
            sourceImporter.textureCompression=TextureImporterCompression.Uncompressed;
            sourceImporter.wrapMode=TextureWrapMode.Clamp;sourceImporter.filterMode=FilterMode.Bilinear;
            sourceImporter.SaveAndReimport();
            var source=AssetDatabase.LoadAssetAtPath<Texture2D>(FloorSourcePath);
            if(!source)throw new InvalidOperationException("Reference floor image must finish importing before assigning the material.");
            material.SetTexture("_TileMap",source);
            // Original screenshot pixels: a clean 36 x 33 two-row repeat, with no UI.
            material.SetVector("_TileRect",new Vector4(20f/source.width,(source.height-42f)/source.height,36f/source.width,33f/source.height));
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
        }
        [MenuItem("Arrow Sort/Environment/Build main game Android APK")]
        public static void BuildMainAndroid()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Stop Play Mode and finish compilation first.");
            var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).ToArray();
            if(scenes.Length!=1||scenes[0].path!=MainScene)throw new InvalidOperationException("Build must contain the main game scene only.");
            Directory.CreateDirectory("Builds/MainTiles");
            bool bundle=EditorUserBuildSettings.buildAppBundle,export=EditorUserBuildSettings.exportAsGoogleAndroidProject,signing=PlayerSettings.Android.useCustomKeystore;
            try
            {
                EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;PlayerSettings.Android.useCustomKeystore=false;
                File.WriteAllText("Builds/MainTiles/BuildStarted.txt",DateTime.UtcNow.ToString("O")+"\n"+MainScene);
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{MainScene},locationPathName="Builds/MainTiles/ArrowSort-Main-Tiles.apk",target=BuildTarget.Android,options=BuildOptions.None});
                File.WriteAllText("Builds/MainTiles/BuildResult.txt",report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nBytes: "+report.summary.totalSize);
                if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Main game APK build failed: "+report.summary.result);
            }
            finally{EditorUserBuildSettings.buildAppBundle=bundle;EditorUserBuildSettings.exportAsGoogleAndroidProject=export;PlayerSettings.Android.useCustomKeystore=signing;}
        }
    }
}
