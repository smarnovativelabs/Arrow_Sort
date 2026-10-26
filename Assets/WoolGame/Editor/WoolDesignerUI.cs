using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace WoolGame.Editor
{
    public static class WoolDesignerUI
    {
        const string Atlas="Assets/newCompleteSprite.png";
        public static string PrepareAtlas()
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(Atlas);
            var factory=new SpriteDataProviderFactories();factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var capability=provider.GetDataProvider<ISpriteFrameEditCapability>();
            if(capability==null||!capability.GetEditCapability().HasCapability(EEditCapability.EditBorder))
                throw new InvalidOperationException("Atlas importer does not support safe border editing.");
            var rects=provider.GetSpriteRects();
            foreach(var rect in rects)
            {
                int id=int.Parse(rect.name.Substring(rect.name.LastIndexOf('_')+1));
                if(id==4||id==2)rect.border=new Vector4(55,35,55,35);
                if(id==5)rect.border=new Vector4(30,22,30,22);
                if(id==76)rect.border=new Vector4(40,30,40,30);
                if(id==29)rect.border=new Vector4(24,24,24,24);
                if(id==20||id==92||id==104)rect.border=new Vector4(24,24,24,24);
                if(id==69)rect.border=new Vector4(32,32,32,32);
                if(id==83)rect.border=new Vector4(32,32,32,105);
                if(id==61)rect.border=new Vector4(100,45,100,45);
            }
            provider.SetSpriteRects(rects);provider.Apply();
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;
            importer.alphaIsTransparency=true;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
            return "Atlas borders updated; original sprite IDs preserved.";
        }
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before styling.");
            var rig=UnityEngine.Object.FindFirstObjectByType<WoolSceneRig>();
            var sprites=AssetDatabase.LoadAllAssetsAtPath(Atlas).OfType<Sprite>().ToArray();
            Sprite S(int id)=>sprites.Single(s=>s.name=="newCompleteSprite_"+id);
            rig.hud.AuthorDesignerTheme(sprites);
            if(rig.slotUnlockIcons==null||rig.slotUnlockIcons.Length!=8)rig.slotUnlockIcons=new GameObject[8];
            for(int i=5;i<8;i++)
            {
                if(!rig.slotUnlockIcons[i])
                {
                    rig.slotUnlockIcons[i]=new GameObject("Unlock slot plus",typeof(SpriteRenderer));
                    rig.slotUnlockIcons[i].transform.SetParent(rig.slots[i],true);
                }
                var icon=rig.slotUnlockIcons[i];var renderer=icon.GetComponent<SpriteRenderer>();renderer.sprite=S(22);renderer.color=Color.white;
                icon.transform.position=rig.slots[i].position+Vector3.up*.5f;
                icon.transform.rotation=rig.gameCamera.transform.rotation;
                icon.transform.localScale=Vector3.one*(1.25f/S(22).bounds.size.x);
                icon.SetActive(true);rig.counts[i].text="";
            }
            EditorUtility.SetDirty(rig);EditorUtility.SetDirty(rig.hud);
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(rig.gameObject.scene);EditorSceneManager.SaveScene(rig.gameObject.scene);
            return "Designer atlas applied across UI; three slot plus icons saved.";
        }
    }
}
