#if UNITY_EDITOR
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace WoolGame
{
    public static class WoolSkywayAuthoring
    {
        const string Folder="Assets/WoolGame/Designer/UI/Skyway/";
        static Sprite ImportSprite(string path)
        {
            AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.maxTextureSize=2048;
            importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        public static string BakeAndAuthor()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Author in Edit mode only");
            var rig=UnityEngine.Object.FindFirstObjectByType<WoolSceneRig>();
            var d=rig.duckRescue;Directory.CreateDirectory(Folder);
            string original=Folder+"SkywayReference.png";
            if(!File.Exists(original))File.Copy("C:/Users/Muneeb Nazir/Downloads/Duck and Dragon Skyway Map.png",original);
            var reference=ImportSprite(original);var background=ImportSprite(Folder+"SkywayBackground.jpg");
            string navPath=Folder+"MapNavigation.asset";
            var nav=AssetDatabase.LoadAssetAtPath<Sprite>(navPath);
            if(!nav)
            {
                float h=reference.texture.height*(214f/1672f);
                nav=Sprite.Create(reference.texture,new Rect(0,0,reference.texture.width,h),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
                nav.name="Map selected navigation";AssetDatabase.CreateAsset(nav,navPath);
            }
            string atlasPath=Folder+"DuckMapAnimation.png";
            if(!File.Exists(atlasPath))BakeDuck(d,atlasPath);
            var atlas=ImportSprite(atlasPath);var frames=new Sprite[48];
            for(int i=0;i<frames.Length;i++)
            {
                string path=Folder+"DuckFrame"+i.ToString("D2")+".asset";
                frames[i]=AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if(!frames[i])
                {
                    frames[i]=Sprite.Create(atlas.texture,new Rect((i%8)*256,(i/8)*256,256,256),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
                    frames[i].name="Duck "+(i<16?"idle":i<32?"walk":"happy")+" "+i%16;AssetDatabase.CreateAsset(frames[i],path);
                }
            }
            var idle=new Sprite[16];var walk=new Sprite[16];var happy=new Sprite[16];
            Array.Copy(frames,0,idle,0,16);Array.Copy(frames,16,walk,0,16);Array.Copy(frames,32,happy,0,16);
            var map=rig.hud.AuthorSkyway(background,nav,idle,walk,happy);
            EditorUtility.SetDirty(rig.hud);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(rig.gameObject.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(rig.gameObject.scene);AssetDatabase.SaveAssets();
            return "Skyway scene authored; 11 numbered nodes, map navigation and 48 duck animation frames.";
        }
        public static string RebakeDuckAtlas(){var rig=UnityEngine.Object.FindFirstObjectByType<WoolSceneRig>();BakeDuck(rig.duckRescue,Folder+"DuckMapAnimation.png");ImportSprite(Folder+"DuckMapAnimation.png");return "Duck atlas rebaked with CPU skinning per frame";}
        static void BakeDuck(WoolDuckRescue source,string path)
        {
            var root=new GameObject("Temporary duck sprite bake");root.transform.position=new Vector3(1000,1000,1000);
            var mother=UnityEngine.Object.Instantiate(source.motherModel,root.transform,false);
            var baby=UnityEngine.Object.Instantiate(source.babyModel,root.transform,false);
            mother.SetActive(true);baby.SetActive(true);
            Vector3 ms=source.motherModel.transform.localScale,bs=source.babyModel.transform.localScale;
            foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
            foreach(var animator in root.GetComponentsInChildren<Animator>(true))animator.enabled=false;
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true)){renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;}
            foreach(var skin in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=true;
            var go=new GameObject("Temporary sprite camera");var cam=go.AddComponent<Camera>();
            cam.orthographic=true;cam.cullingMask=1<<31;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.clear;
            cam.nearClipPlane=.05f;cam.farClipPlane=30;cam.aspect=1;cam.allowHDR=false;cam.allowMSAA=false;
            var data=go.AddComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=false;data.renderShadows=false;
            var lightGo=new GameObject("Temporary duck light");var light=lightGo.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.1f;light.cullingMask=1<<31;light.shadows=LightShadows.None;light.transform.rotation=Quaternion.Euler(35,-35,0);
            var skins=root.GetComponentsInChildren<SkinnedMeshRenderer>(true);var poses=new Mesh[skins.Length];
            for(int k=0;k<skins.Length;k++){poses[k]=new Mesh();poses[k].MarkDynamic();var pose=new GameObject("Baked pose",typeof(MeshFilter),typeof(MeshRenderer));pose.layer=31;pose.transform.SetParent(skins[k].transform,false);pose.GetComponent<MeshFilter>().sharedMesh=poses[k];var renderer=pose.GetComponent<MeshRenderer>();renderer.sharedMaterials=skins[k].sharedMaterials;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;skins[k].forceRenderingOff=true;}
            var rt=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);var atlas=new Texture2D(2048,1536,TextureFormat.RGBA32,false);var active=RenderTexture.active;
            try
            {
                Sample(source,mother,baby,0,0,ms,bs);
                for(int k=0;k<skins.Length;k++)skins[k].BakeMesh(poses[k]);
                var bounds=new Bounds(root.transform.position,Vector3.zero);bool initialized=false;
                foreach(var renderer in root.GetComponentsInChildren<Renderer>()){if(!initialized){bounds=renderer.bounds;initialized=true;}else bounds.Encapsulate(renderer.bounds);}
                var center=bounds.center+Vector3.up*bounds.size.y*.1f;
                cam.orthographicSize=Mathf.Max(bounds.extents.y*1.6f,bounds.extents.x*1.25f);
                cam.transform.position=center+new Vector3(2.4f,1.7f,3.2f).normalized*8;
                cam.transform.LookAt(center);cam.targetTexture=rt;
                for(int i=0;i<48;i++)
                {
                    Sample(source,mother,baby,i/16,(i%16)/16f,ms,bs);
                    for(int k=0;k<skins.Length;k++)skins[k].BakeMesh(poses[k]);
                    cam.Render();RenderTexture.active=rt;
                    atlas.ReadPixels(new Rect(0,0,256,256),(i%8)*256,(i/8)*256,false);
                }
                atlas.Apply();File.WriteAllBytes(path,atlas.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active=active;cam.targetTexture=null;rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(atlas);
                UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(lightGo);UnityEngine.Object.DestroyImmediate(root);
                foreach(var pose in poses)UnityEngine.Object.DestroyImmediate(pose);
            }
        }
        static void Sample(WoolDuckRescue source,GameObject mother,GameObject baby,int state,float fraction,Vector3 ms,Vector3 bs)
        {
            var a=state==0?source.motherEat:state==1?source.motherWalk:source.motherJump;
            var b=state==0?source.babyEat:state==1?source.babyWalk:source.babyJump;
            if(a)a.SampleAnimation(mother,fraction*a.length);
            if(b)b.SampleAnimation(baby,Mathf.Repeat(fraction+.1f,1)*b.length);
            mother.transform.localPosition=new Vector3(.28f,0,0);baby.transform.localPosition=new Vector3(-.48f,0,.12f);
            mother.transform.localRotation=baby.transform.localRotation=Quaternion.Euler(0,-45,0);
            mother.transform.localScale=ms;baby.transform.localScale=bs;
        }
    }
}
#endif
