using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WoolGame.Editor
{
    public static class WoolDarkGlossAuthoring
    {
        const string Root="Assets/WoolGame/Designer/SharedSnakePalette";
        const string BackupRoot="Docs/Backups/DarkGlossPalette";
        static readonly string[] Hex={"E53935","00D5E8","FFD600","7B28C9","159447","FF8C00","FF76B5","2454E6","86451F","30343B"};
        static readonly string[] SourceHex={"C3190C","04C4E0","F3C916","A428E3","03A14E","F57D08","C51A73","1851C3","72B50E","F66F92"};

        [MenuItem("Tools/Wool Game/Apply Dark Gloss Palette")]
        public static void ApplyMenu(){Debug.Log(Apply());}

        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            var rig=UnityEngine.Object.FindFirstObjectByType<WoolSceneRig>();
            if(!rig||!rig.authoredLayout)throw new InvalidOperationException("Open the authored gameplay scene.");
            Directory.CreateDirectory(Root);Directory.CreateDirectory(BackupRoot);
            Backup("Assets/Scenes/Wool Crush.unity");
            Backup("Assets/WoolGame/Scripts/WoolArt.cs");
            Backup(AssetDatabase.GetAssetPath(rig.models.tailMaterial));
            Backup("Assets/Art/Materials/FogPortraitEntrance.mat");
            var colors=Hex.Select(Parse).ToArray();
            var reference=SourceHex.Select(Parse).ToArray();
            var ramp=new Texture2D(128,10,TextureFormat.RGBA32,false);
            for(int y=0;y<10;y++)for(int x=0;x<128;x++)ramp.SetPixel(x,y,Shade(colors[y],x/127f));
            ramp.Apply();Write(ramp,Root+"/DarkColorRamp.png",false);
            UnityEngine.Object.DestroyImmediate(ramp);

            var source=new Texture2D(2,2,TextureFormat.RGBA32,false);
            source.LoadImage(File.ReadAllBytes("Assets/Art/Texture/Dragon_Tail_Pieces_Atlas.png"));
            var pixels=source.GetPixels32();int width=source.width,height=source.height;
            var tags=new sbyte[width*height];for(int i=0;i<tags.Length;i++)tags[i]=-1;
            for(int row=0;row<10;row++)TagMesh(rig.models.tailMeshes[row],row,tags,width,height);
            var hue=new float[10];var sat=new float[10];var value=new float[10];
            for(int i=0;i<10;i++)Color.RGBToHSV(reference[i],out hue[i],out sat[i],out value[i]);
            var counts=new int[10];
            for(int i=0;i<pixels.Length;i++)
            {
                Color original=pixels[i];float h,s,v;Color.RGBToHSV(original,out h,out s,out v);
                if(v<.04f||s<.08f)continue;
                int row=tags[i];
                if(row<0){float nearest=2;for(int c=0;c<10;c++){float d=Mathf.Abs(h-hue[c]);d=Mathf.Min(d,1-d);if(d<nearest){nearest=d;row=c;}}}
                float diffuse=Mathf.Clamp01(v/Mathf.Max(.1f,value[row]));
                float gloss=Mathf.Max(Mathf.Clamp01((v-value[row])/Mathf.Max(.04f,1-value[row])),Mathf.Clamp01((sat[row]-s)/Mathf.Max(.1f,sat[row])));
                float shade=.5f*diffuse+.5f*Mathf.Pow(gloss,1.5f);
                var recolored=Shade(colors[row],Mathf.Clamp01(shade));recolored.a=original.a;
                pixels[i]=recolored;counts[row]++;
            }
            source.SetPixels32(pixels);source.Apply();Write(source,Root+"/DarkDragonTailAtlas.png",true);
            UnityEngine.Object.DestroyImmediate(source);
            var tail=rig.models.tailMaterial;
            tail.shader=Shader.Find("Universal Render Pipeline/Unlit");tail.shaderKeywords=new string[0];
            tail.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/DarkDragonTailAtlas.png"));
            tail.SetColor("_BaseColor",Color.white);tail.enableInstancing=true;EditorUtility.SetDirty(tail);
            var head=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Dragon_Head_Atlas.mat");
            if(head){head.shader=Shader.Find("Universal Render Pipeline/Unlit");head.shaderKeywords=new string[0];EditorUtility.SetDirty(head);}

            var common=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/DarkColorRamp.png");
            foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{Root}))
            {
                var m=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if(!m||m.shader.name!="WoolGame/Shared Snake Gloss")continue;
                Backup(AssetDatabase.GetAssetPath(m));m.SetTexture("_TailRamp",common);m.SetFloat("_Shine",.9f);
                m.SetFloat("_NeutralBrightness",m.name.Contains("Bow")?.58f:1f);m.enableInstancing=true;EditorUtility.SetDirty(m);
            }
            rig.authoredLayout.vehicleTrailColors=(Color[])colors.Clone();rig.arrowPool.shotColors=(Color[])colors.Clone();
            foreach(var slot in rig.authoredLayout.slots)for(int i=0;i<10;i++)
            {
                var trail=slot.visuals[i].trail;if(!trail)continue;
                trail.startColor=new Color(colors[i].r,colors[i].g,colors[i].b,.75f);
                trail.endColor=new Color(colors[i].r,colors[i].g,colors[i].b,0);EditorUtility.SetDirty(trail);
            }
            string art="Assets/WoolGame/Scripts/WoolArt.cs";string text=File.ReadAllText(art);
            int start=text.IndexOf("public static readonly Color[] Palette = {");int end=text.IndexOf("};",start);
            if(start<0||end<0)throw new InvalidOperationException("Palette anchor missing");
            File.WriteAllText(art,text.Substring(0,start)+"public static readonly Color[] Palette = {\n            "+string.Join(", ",Hex.Select(x=>"Hex(\""+x+"\")"))+"\n        "+text.Substring(end));
            File.WriteAllText(Root+"/Palette.json",JsonUtility.ToJson(new PaletteRecord{hex=Hex},true));
            foreach(var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if(light.type!=LightType.Directional)continue;
                light.transform.rotation=Quaternion.Euler(45,-30,0);light.color=Color.white;light.intensity=1.1f;
                EditorUtility.SetDirty(light);EditorUtility.SetDirty(light.transform);
            }
            // The entrance is a transparent overlay, never an opaque lit surface.
            var fog=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/FogPortraitEntrance.mat");
            if(fog){fog.shader=Shader.Find("WoolGame/SoftFog");fog.shaderKeywords=new string[0];fog.SetOverrideTag("RenderType","Transparent");fog.renderQueue=3000;EditorUtility.SetDirty(fog);}
            foreach(var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if(renderer.sharedMaterial!=fog)continue;
                renderer.enabled=true;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;EditorUtility.SetDirty(renderer);
            }
            EditorUtility.SetDirty(rig.authoredLayout);EditorUtility.SetDirty(rig.arrowPool);
            EditorSceneManager.MarkSceneDirty(rig.gameObject.scene);EditorSceneManager.SaveScene(rig.gameObject.scene);
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            return "Applied ten shared dark colors. Tail texture pixels per color: "+string.Join(",",counts);
        }

        static Color Shade(Color color,float value)
        {
            if(value<=.5f)return Color.Lerp(color*.42f,color,value*2);
            var highlight=Color.Lerp(color,Color.white,.62f);
            return Color.Lerp(color,highlight,Mathf.Pow((value-.5f)*2,1.35f));
        }
        static Color Parse(string hex){Color c;ColorUtility.TryParseHtmlString("#"+hex,out c);return c;}
        static void TagMesh(Mesh mesh,int row,sbyte[] tags,int width,int height)
        {
            var uv=mesh.uv;var tris=mesh.triangles;
            for(int i=0;i<tris.Length;i+=3)
            {
                var a=Vector2.Scale(uv[tris[i]],new Vector2(width,height));
                var b=Vector2.Scale(uv[tris[i+1]],new Vector2(width,height));
                var c=Vector2.Scale(uv[tris[i+2]],new Vector2(width,height));
                float area=Cross(b-a,c-a);if(Mathf.Abs(area)<.00001f)continue;
                int x0=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.x,Mathf.Min(b.x,c.x))),0,width-1);
                int x1=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.x,Mathf.Max(b.x,c.x))),0,width-1);
                int y0=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.y,Mathf.Min(b.y,c.y))),0,height-1);
                int y1=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.y,Mathf.Max(b.y,c.y))),0,height-1);
                for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)
                {
                    var p=new Vector2(x+.5f,y+.5f);float u=Cross(p-a,c-a)/area,v=Cross(b-a,p-a)/area;
                    if(u>=0&&v>=0&&u+v<=1)tags[y*width+x]=(sbyte)row;
                }
            }
        }
        static float Cross(Vector2 a,Vector2 b){return a.x*b.y-a.y*b.x;}
        static void Write(Texture2D texture,string path,bool mipmaps)
        {
            File.WriteAllBytes(path,texture.EncodeToPNG());AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;importer.alphaIsTransparency=false;
            importer.npotScale=TextureImporterNPOTScale.None;importer.mipmapEnabled=mipmaps;
            importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;importer.SaveAndReimport();
        }
        static void Backup(string path){if(File.Exists(path)){var target=BackupRoot+"/"+Path.GetFileName(path);if(!File.Exists(target))File.Copy(path,target);}}
        [Serializable] class PaletteRecord{public string[] hex;}
    }
}
