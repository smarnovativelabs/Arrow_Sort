using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace WoolGame.Editor
{
    public static class WoolLuckySpinBuilder
    {
        [MenuItem("Wool Crush/Author Lucky Spin popup")]
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode first.");
            Sprite Import(string path)
            {
                AssetDatabase.ImportAsset(path);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=2048;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;importer.SaveAndReimport();
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            var coins=Import("Assets/WoolGame/Designer/UI/LuckySpin/DuckCoins.png");
            var unlockSource=Import("Assets/WoolGame/Designer/UI/LuckySpin/UnlockPower.png");
            const string unlockPath="Assets/WoolGame/Designer/UI/LuckySpin/UnlockCutout.asset";
            var unlock=AssetDatabase.LoadAssetAtPath<Sprite>(unlockPath);
            if(!unlock)
            {
                unlock=Sprite.Create(unlockSource.texture,new Rect(0,0,273,259),new Vector2(.5f,.5f),100);unlock.name="Unlock Slot power";
                Vector2[] outline={new Vector2(128,4),new Vector2(167,8),new Vector2(174,50),new Vector2(207,54),new Vector2(220,76),new Vector2(215,115),new Vector2(198,144),new Vector2(254,164),new Vector2(270,205),new Vector2(247,242),new Vector2(197,257),new Vector2(68,257),new Vector2(16,238),new Vector2(5,206),new Vector2(22,171),new Vector2(64,145),new Vector2(62,117),new Vector2(49,98),new Vector2(55,64),new Vector2(90,55),new Vector2(99,21)};
                // Triangulate the concave plus/podium outline rather than showing the source's cream tile.
                var vertices=outline.Select(p=>new Vector2((p.x-136.5f)/100,(129.5f-p.y)/100)).ToArray();
                var contour=new System.Collections.Generic.List<int>(Enumerable.Range(0,vertices.Length));
                var triangles=new System.Collections.Generic.List<ushort>();
                float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
                int guard=0;
                while(contour.Count>2&&guard++<500)
                {
                    bool clipped=false;
                    for(int i=0;i<contour.Count;i++)
                    {
                        int ia=contour[(i+contour.Count-1)%contour.Count],ib=contour[i],ic=contour[(i+1)%contour.Count];
                        var a=vertices[ia];var b=vertices[ib];var c=vertices[ic];
                        if(Cross(b-a,c-b)>=0)continue;
                        bool contains=false;
                        foreach(int k in contour){if(k==ia||k==ib||k==ic)continue;var p=vertices[k];if(Cross(b-a,p-a)<=0&&Cross(c-b,p-b)<=0&&Cross(a-c,p-c)<=0){contains=true;break;}}
                        if(contains)continue;
                        triangles.Add((ushort)ia);triangles.Add((ushort)ib);triangles.Add((ushort)ic);contour.RemoveAt(i);clipped=true;break;
                    }
                    if(!clipped)throw new Exception("Unable to triangulate Unlock Slot icon");
                }
                unlock.OverrideGeometry(vertices,triangles.ToArray());AssetDatabase.CreateAsset(unlock,unlockPath);
            }
            var reference=Import("Assets/WoolGame/Designer/UI/LuckySpin/Reference.png");
            const string hubPath="Assets/WoolGame/Designer/UI/LuckySpin/OriginalFlower.asset";
            var hub=AssetDatabase.LoadAssetAtPath<Sprite>(hubPath);
            if(!hub)
            {
                var tex=reference.texture;
                hub=Sprite.Create(tex,new Rect(439,tex.height-675,146,146),new Vector2(.5f,.5f),100);
                hub.name="Original flower hub";
                var vertices=new Vector2[65];var triangles=new ushort[64*3];
                for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;vertices[i+1]=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.72f;triangles[i*3]=0;triangles[i*3+1]=(ushort)(i+1);triangles[i*3+2]=(ushort)((i+1)%64+1);}
                hub.OverrideGeometry(vertices,triangles);AssetDatabase.CreateAsset(hub,hubPath);
            }
            var ui=AssetDatabase.LoadAllAssetsAtPath("Assets/WoolGame/Designer/UI/FantasyHome/InterfaceLayers.png").OfType<Sprite>().ToArray();
            Sprite Crop(string name,Rect topRect,bool circle=false,bool triangle=false)
            {
                string path="Assets/WoolGame/Designer/UI/LuckySpin/"+name+".asset";
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite)return sprite;
                var tex=reference.texture;
                sprite=Sprite.Create(tex,new Rect(topRect.x,tex.height-topRect.y-topRect.height,topRect.width,topRect.height),new Vector2(.5f,.5f),100);sprite.name=name;
                if(circle)
                {
                    var v=new Vector2[65];var tri=new ushort[192];float r=topRect.width*.49f/100;
                    for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;v[i+1]=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r;tri[i*3]=0;tri[i*3+1]=(ushort)(i+1);tri[i*3+2]=(ushort)((i+1)%64+1);}
                    sprite.OverrideGeometry(v,tri);
                }
                if(triangle)sprite.OverrideGeometry(new[]{new Vector2(-.38f,.34f),new Vector2(.38f,.34f),new Vector2(.36f,.16f),new Vector2(.07f,-.39f),new Vector2(-.07f,-.39f),new Vector2(-.36f,.16f)},new ushort[]{0,1,2,0,2,3,0,3,4,0,4,5});
                AssetDatabase.CreateAsset(sprite,path);return sprite;
            }
            var green=ui.Single(s=>s.name=="InterfaceLayers_08");
            var pointer=Crop("OriginalPointer",new Rect(474,255,80,84),false,true);
            var noAds=Crop("NoAdsIcon",new Rect(464,404,103,103),true);
            var info=Crop("OriginalInfo",new Rect(44,231,88,88),true);
            var hud=UnityEngine.Object.FindFirstObjectByType<WoolHud>();Undo.RegisterFullObjectHierarchyUndo(hud.gameObject,"Author Lucky Spin popup");
            string result=hud.AuthorLuckySpin(coins,unlock,green,pointer,hub,noAds,info);
            EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);EditorSceneManager.SaveScene(hud.gameObject.scene);AssetDatabase.SaveAssets();return result;
        }
        public static string ValidateRules()
        {
            void Check(bool condition,string message){if(!condition)throw new Exception(message);}
            long day=new DateTimeOffset(new DateTime(2026,1,1,0,0,0,DateTimeKind.Local)).ToUnixTimeSeconds();
            var s=new WoolSpinState();Check(s.Available(0)==0,"Invalid time must block spins");
            Check(s.Available(day)==1,"Initial daily spin");Check(s.Spin(day,2)==2&&s.lastCoins==0,"Try Again consumes a spin, no coins");
            Check(s.Spin(day+86399,1)==-1,"Cannot reclaim within same local day");
            Check(s.Available(day+86400)==1,"Local midnight reset while game closed");
            for(int i=1;i<30;i++)s.Spin(day+i*86400,1);
            Check(s.streak==30&&s.lastSlot==0&&s.noAdsUntil==day+30*86400,"Day 30 guarantees 24h No Ads");
            Check(s.extraSpins==1,"Day 30 awards exactly one extra spin");
            Check(s.Spin(day+29*86400,5)==5&&s.lastCoins==250&&s.unlockPowers==0&&s.streak==30&&s.extraSpins==0,"Extra spin grants coins without inflating streak");
            Check(s.Spin(day+29*86400,0)==-1,"No duplicate milestone grant");
            Check(s.Spin(day+32*86400,1)==1&&s.streak==1,"Missed days reset streak");
            s.CompleteLevel();s.CompleteLevel();Check(s.extraSpins==0,"No early level spin");s.CompleteLevel();Check(s.extraSpins==1&&s.levelProgress==0,"Three completions grant one spin");
            var copy=JsonUtility.FromJson<WoolSpinState>(JsonUtility.ToJson(s));Check(copy.lastDailyDay==s.lastDailyDay&&copy.extraSpins==1&&copy.unlockPowers==0,"Save/reload keeps timer, spins and powers");
            return "PASS: local reset, closed-game deadline, invalid time, one claim/day, Try Again, exclusive 30-day reward and extra spin, unlock reward, streak gap, level-earned spin, save/reload.";
        }
    }
}

