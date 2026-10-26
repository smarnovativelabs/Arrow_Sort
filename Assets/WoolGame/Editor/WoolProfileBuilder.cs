using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace WoolGame.Editor
{
    public static class WoolProfileBuilder
    {
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode first.");
            Sprite Import(string path)
            {
                AssetDatabase.ImportAsset(path);var i=(TextureImporter)AssetImporter.GetAtPath(path);
                i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.mipmapEnabled=false;i.alphaIsTransparency=true;i.maxTextureSize=2048;i.textureCompression=TextureImporterCompression.Uncompressed;i.npotScale=TextureImporterNPOTScale.None;i.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            const string folder="Assets/WoolGame/Designer/UI/Profile/";
            var panel=Import(folder+"ProfilePanel.png");var sheet=Import(folder+"DuckAvatars.png");var avatars=new Sprite[6];
            for(int i=0;i<6;i++)
            {
                string path=folder+"Avatar"+i+".asset";avatars[i]=AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if(!avatars[i]){float w=sheet.texture.width/3f,h=sheet.texture.height/2f;avatars[i]=Sprite.Create(sheet.texture,new Rect(i%3*w,(1-i/3)*h,w,h),new Vector2(.5f,.5f),100);avatars[i].name=new[]{"Royal Duck","Explorer Duck","Wizard Duck","Pilot Duck","Chef Duck","Garden Duck"}[i];AssetDatabase.CreateAsset(avatars[i],path);}
            }
            const string pencilPath=folder+"Pencil.asset";var pencil=AssetDatabase.LoadAssetAtPath<Sprite>(pencilPath);
            if(!pencil){float sx=panel.texture.width/941f,sy=panel.texture.height/1672f;pencil=Sprite.Create(panel.texture,new Rect(235*sx,panel.texture.height-437*sy,90*sx,87*sy),new Vector2(.5f,.5f),100);pencil.name="Edit avatar pencil";AssetDatabase.CreateAsset(pencil,pencilPath);}
            var green=AssetDatabase.LoadAllAssetsAtPath("Assets/WoolGame/Designer/UI/FantasyHome/InterfaceLayers.png").OfType<Sprite>().Single(s=>s.name=="InterfaceLayers_08");
            var hud=UnityEngine.Object.FindFirstObjectByType<WoolHud>();Undo.RegisterFullObjectHierarchyUndo(hud.gameObject,"Author player profile");
            string result=hud.AuthorProfileView(panel,avatars,green,pencil);
            // Repair the previous wheel's custom graphics, which need explicit CanvasRenderers.
            foreach(var g in hud.GetComponentsInChildren<WoolSpinSurface>(true)){if(!g.GetComponent<CanvasRenderer>())g.gameObject.AddComponent<CanvasRenderer>();g.SetAllDirty();}
            foreach(var g in hud.GetComponentsInChildren<WoolSpinLamp>(true)){if(!g.GetComponent<CanvasRenderer>())g.gameObject.AddComponent<CanvasRenderer>();g.SetAllDirty();}
            foreach(var image in hud.GetComponentsInChildren<UnityEngine.UI.Image>(true))
            {
                string name=image.name;
                if(name!="Fixed pointer"&&name!="No Ads reward"&&name!="Original flower hub"&&name!="Information")continue;
                if(!image.transform.GetComponentInParent<WoolLuckySpin>())continue;
                var shape=image.GetComponent<WoolSpriteSilhouette>();if(!shape)shape=image.gameObject.AddComponent<WoolSpriteSilhouette>();
                shape.shape=name=="Fixed pointer"?WoolSpriteSilhouette.Shape.Pointer:WoolSpriteSilhouette.Shape.Circle;shape.Apply();EditorUtility.SetDirty(shape);
            }
            EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);EditorSceneManager.SaveScene(hud.gameObject.scene);AssetDatabase.SaveAssets();return result;
        }
        public static string Validate()
        {
            void Check(bool b,string message){if(!b)throw new Exception(message);}
            var daily=new WoolDailyPlayState();long t=20000L*86400+86399;daily.Record(t);daily.Record(t);Check(daily.Count(t)==2,"Daily attempts count");Check(daily.Count(t+1)==0,"Midnight UTC reset");daily.Record(t+1);Check(daily.Count(t+1)==1,"First attempt after reset");
            var restored=JsonUtility.FromJson<WoolDailyPlayState>(JsonUtility.ToJson(daily));Check(restored.Count(t+1)==1,"Persisted daily count");
            WoolProgress.BeginPreview();try{WoolProgress.SetProfile("  Duck Captain  ",5);Check(WoolProgress.PlayerName=="Duck Captain"&&WoolProgress.AvatarIndex==5,"Profile editing");WoolProgress.SetProfile("<bad>\n",99);Check(WoolProgress.PlayerName=="bad"&&WoolProgress.AvatarIndex==5,"Input sanitization and avatar limit");}finally{WoolProgress.EndPreview();}
            return "PASS: attempts count, UTC midnight reset, daily save/reload, name/avatar editing and validation using nonpersistent preview state.";
        }
    }
}
