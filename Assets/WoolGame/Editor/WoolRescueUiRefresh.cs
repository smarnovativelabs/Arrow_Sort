using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace WoolGame.Editor
{
    public static class WoolRescueUiRefresh
    {
        const string Atlas="Assets/WoolGame/Designer/UI/RescueUIAtlas-20261019.png";

        [MenuItem("Wool Crush/Apply duck rescue UI refresh")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before applying UI art.");
            var rig=UnityEngine.Object.FindFirstObjectByType<WoolSceneRig>();
            if(!rig||!rig.hud||!rig.authoredLayout)throw new InvalidOperationException("Open the authored Wool Crush scene.");
            string backup="Docs/Backups/UiRefresh-"+DateTime.Now.ToString("yyyyMMdd-HHmmss");
            System.IO.Directory.CreateDirectory(backup);
            EditorSceneManager.SaveScene(rig.gameObject.scene,backup+"/Before.unity",true);
            var sprites=PrepareAtlas();
            Undo.RegisterFullObjectHierarchyUndo(rig.hud.gameObject,"Refresh duck rescue UI");
            Undo.RegisterFullObjectHierarchyUndo(rig.authoredLayout.environment,"Align rescue gameplay zones");
            rig.hud.AuthorRescueTheme(sprites);
            AlignBackground(rig);
            float center=rig.slots.Average(s=>s.position.x);
            foreach(var slot in rig.slots)
            {
                var p=slot.position;p.x-=center;slot.position=p;
            }
            rig.ApplyLayout(false);
            rig.FitCamera();
            EditorUtility.SetDirty(rig);EditorUtility.SetDirty(rig.hud);
            EditorSceneManager.MarkSceneDirty(rig.gameObject.scene);
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(rig.gameObject.scene);
            Debug.Log("Duck rescue UI saved: 16 original sprites, eight docks on the grey rail, green environment and white vehicle board.");
        }

        public static Sprite[] PrepareAtlas()
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(Atlas);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.isReadable=true;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;
            importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;
            importer.filterMode=FilterMode.Bilinear;importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Atlas);var pixels=texture.GetPixels32();
            var factory=new SpriteDataProviderFactories();factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var previous=provider.GetSpriteRects();var rects=new SpriteRect[16];
            for(int i=0;i<16;i++)
            {
                int left=Mathf.RoundToInt((i%4)*texture.width/4f),right=Mathf.RoundToInt((i%4+1)*texture.width/4f);
                int bottom=Mathf.RoundToInt((3-i/4)*texture.height/4f),top=Mathf.RoundToInt((4-i/4)*texture.height/4f);
                int minX=right,minY=top,maxX=left,maxY=bottom;
                for(int y=bottom;y<top;y++)for(int x=left;x<right;x++)if(pixels[y*texture.width+x].a>160)
                {minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);}
                minX=Mathf.Max(left,minX-2);maxX=Mathf.Min(right-1,maxX+2);minY=Mathf.Max(bottom,minY-2);maxY=Mathf.Min(top-1,maxY+2);
                var rect=new Rect(minX,minY,maxX-minX+1,maxY-minY+1);
                Vector4 border=Vector4.zero;
                if(i==0||i==7)border=new Vector4(rect.width*.17f,rect.height*.17f,rect.width*.17f,rect.height*.17f);
                if(i==1||i==2||i==6)border=new Vector4(rect.width*.22f,rect.height*.4f,rect.width*.22f,rect.height*.4f);
                string name="RescueUI_"+i.ToString("00");var old=previous.FirstOrDefault(s=>s.name==name);
                rects[i]=new SpriteRect{name=name,rect=rect,alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f),border=border,spriteID=old!=null?old.spriteID:GUID.Generate()};
            }
            provider.SetSpriteRects(rects);
            var names=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if(names!=null)names.SetNameFileIdPairs(rects.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
            provider.Apply();importer.SaveAndReimport();
            importer.isReadable=false;importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(Atlas).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
        }

        static void AlignBackground(WoolSceneRig rig)
        {
            var background=rig.authoredLayout.environment.GetComponentsInChildren<MeshRenderer>(true).First(r=>r.name=="Game_Background_Grass_NoPath");
            float railZ=rig.slots.Average(s=>s.position.z);
            // The texture is green above v=.67, grey between .67 and .55, and white below.
            // Map the existing artwork to the gameplay coordinates instead of moving the vehicle rules.
            float[] xs={-60,-11,11,60};float[] us={.9995f,.9995f,.0005f,.0005f};
            float[] zs={-60,-19,railZ-1.25f,railZ+1.25f,28,60};
            float[] vs={.9995f,.9995f,.67f,.55f,.0005f,.0005f};
            var verts=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
            for(int z=0;z<zs.Length;z++)for(int x=0;x<xs.Length;x++)
            {verts.Add(new Vector3(xs[x],-.42f,zs[z]));uv.Add(new Vector2(us[x],vs[z]));}
            for(int z=0;z<zs.Length-1;z++)for(int x=0;x<xs.Length-1;x++)
            {int n=z*xs.Length+x;triangles.AddRange(new[]{n,n+xs.Length,n+1,n+1,n+xs.Length,n+xs.Length+1});}
            const string path="Assets/WoolGame/Designer/UI/RescueGameplayBackground.asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(!mesh){mesh=new Mesh{name="Rescue gameplay zones"};AssetDatabase.CreateAsset(mesh,path);}
            mesh.Clear();mesh.SetVertices(verts);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
            background.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);background.transform.localScale=Vector3.one;
            background.GetComponent<MeshFilter>().sharedMesh=mesh;
            background.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;background.receiveShadows=false;
            foreach(var slot in rig.slots)
                foreach(var renderer in slot.GetComponentsInChildren<MeshRenderer>(true))
                    if(renderer.name=="Round slot")renderer.sharedMaterial=SlotMaterial(renderer.sharedMaterial,"RescueSlotSlate",WoolArt.Hex("7A879C"));
        }

        static Material SlotMaterial(Material source,string name,Color color)
        {
            string path="Assets/WoolGame/Designer/UI/"+name+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!material){material=new Material(source);material.name=name;AssetDatabase.CreateAsset(material,path);}
            if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",color);
            if(material.HasProperty("_Color"))material.SetColor("_Color",color);
            EditorUtility.SetDirty(material);return material;
        }
    }
}
