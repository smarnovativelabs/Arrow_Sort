using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WoolGame.Editor
{
    public static class WoolAdPassBuilder
    {
        [MenuItem("Wool Crush/Author Ad Pass offer")]
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode first.");
            const string path="Assets/WoolGame/Designer/UI/AdPass/OfferPanel.png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=2048;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;
            importer.SaveAndReimport();
            var green=AssetDatabase.LoadAllAssetsAtPath("Assets/WoolGame/Designer/UI/FantasyHome/InterfaceLayers.png").OfType<Sprite>().Single(s=>s.name=="InterfaceLayers_08");
            var hud=UnityEngine.Object.FindFirstObjectByType<WoolHud>();
            Undo.RegisterFullObjectHierarchyUndo(hud.gameObject,"Author Ad Pass popup");
            var result=hud.AuthorAdPassOffer(AssetDatabase.LoadAssetAtPath<Sprite>(path),green);
            EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);EditorSceneManager.SaveScene(hud.gameObject.scene);AssetDatabase.SaveAssets();return result;
        }
    }
}
