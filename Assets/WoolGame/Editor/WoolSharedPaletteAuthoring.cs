using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace WoolGame.Editor {
 public static class WoolSharedPaletteAuthoring {
 const string Root="Assets/WoolGame/Designer/SharedSnakePalette";
 static Dictionary<string,Mesh> meshes;
 static Dictionary<Material,Material> materials;
 static Texture2D ramp;static int changed;
 public static string Apply(){
 if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
 var rig=Resources.FindObjectsOfTypeAll<WoolSceneRig>().First(x=>x.gameObject.scene.IsValid());
 Directory.CreateDirectory(Root+"/Meshes");Directory.CreateDirectory("Docs/Backups/SharedSnakePalette");
 Backup("Assets/Scenes/Wool Crush.unity");Backup("Assets/Prefabs/Vehicles.prefab");Backup("Assets/WoolGame/Scripts/WoolArt.cs");Backup("Assets/WoolGame/Scripts/WoolAuthoredLayout.cs");
 var source=(Texture2D)rig.models.tailMaterial.GetTexture("_BaseMap");var pixels=Read(source);
 var palette=new Color[10];var image=new Texture2D(128,10,TextureFormat.RGBA32,false);
 for(int color=0;color<10;color++){
 var mesh=rig.models.tailMeshes[color];var triangles=mesh.triangles;var uv=mesh.uv;var vertices=mesh.vertices;
 var sums=new Vector3[128];var weights=new float[128];var hist=new Dictionary<int,float>();var colors=new Dictionary<int,Vector4>();
 for(int t=0;t<triangles.Length;t+=3){int a=triangles[t],b=triangles[t+1],c=triangles[t+2];float area=Vector3.Cross(vertices[b]-vertices[a],vertices[c]-vertices[a]).magnitude;
 for(int j=1;j<8;j++)for(int k=1;k<8-j;k++){float u=j/8f,v=k/8f;var q=uv[a]*(1-u-v)+uv[b]*u+uv[c]*v;var col=pixels.GetPixelBilinear(q.x,q.y);float h,s,value;Color.RGBToHSV(col,out h,out s,out value);if(s<.28f||value<.12f)continue;
 int index=Mathf.Clamp(Mathf.RoundToInt(value*127),0,127);sums[index]+=new Vector3(col.r,col.g,col.b)*area;weights[index]+=area;
 int key=((int)(col.r*15)<<8)|((int)(col.g*15)<<4)|(int)(col.b*15);if(!hist.ContainsKey(key)){hist[key]=0;colors[key]=Vector4.zero;}hist[key]+=area;colors[key]+=new Vector4(col.r,col.g,col.b,1)*area;
 }}
 int best=hist.OrderByDescending(x=>x.Value).First().Key;var dominant=colors[best]/hist[best];palette[color]=new Color(dominant.x,dominant.y,dominant.z,1);
 for(int x=0;x<128;x++){int left=x,right=x;while(left>=0&&weights[left]==0)left--;while(right<128&&weights[right]==0)right++;if(left<0)left=right;if(right>=128)right=left;var l=sums[left]/weights[left];var r=sums[right]/weights[right];var value=Vector3.Lerp(l,r,left==right?0:(x-left)/(float)(right-left));image.SetPixel(x,color,new Color(value.x,value.y,value.z,Mathf.Max(palette[color].r,palette[color].g,palette[color].b)));}
 }
 UnityEngine.Object.DestroyImmediate(pixels);image.Apply();File.WriteAllBytes(Root+"/SnakeColorRamp.png",image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);AssetDatabase.ImportAsset(Root+"/SnakeColorRamp.png");
 var ti=(TextureImporter)AssetImporter.GetAtPath(Root+"/SnakeColorRamp.png");ti.textureType=TextureImporterType.Default;ti.npotScale=TextureImporterNPOTScale.None;ti.mipmapEnabled=false;ti.wrapMode=TextureWrapMode.Clamp;ti.filterMode=FilterMode.Bilinear;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.SaveAndReimport();ramp=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/SnakeColorRamp.png");
 meshes=new Dictionary<string,Mesh>();materials=new Dictionary<Material,Material>();changed=0;
 string prefabPath=AssetDatabase.GetAssetPath(rig.authoredLayout.vehiclePool.vehiclePrefab);if(string.IsNullOrEmpty(prefabPath))prefabPath="Assets/Prefabs/Vehicles.prefab";
 var names=rig.authoredLayout.vehiclePool.templates.Select(x=>x.name).ToArray();var prefab=PrefabUtility.LoadPrefabContents(prefabPath);
 try{var all=prefab.GetComponentsInChildren<Transform>(true);for(int i=0;i<30;i++)ApplyTree(all.Single(x=>x.name==names[i]),i%10);PrefabUtility.SaveAsPrefabAsset(prefab,prefabPath);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
 for(int slot=0;slot<rig.authoredLayout.slots.Length;slot++)for(int color=0;color<10;color++){ApplyTree(rig.authoredLayout.slots[slot].colors[color],color);ApplyTree(rig.authoredLayout.slots[slot].arrows[color],color);}
 rig.authoredLayout.vehicleTrailColors=palette;rig.arrowPool.shotColors=palette;
 for(int slot=0;slot<rig.authoredLayout.slots.Length;slot++)for(int color=0;color<10;color++){var trail=rig.authoredLayout.slots[slot].visuals[color].trail;if(trail){trail.startColor=new Color(palette[color].r,palette[color].g,palette[color].b,.75f);trail.endColor=new Color(palette[color].r,palette[color].g,palette[color].b,0);}}
 string[] hex=palette.Select(ColorUtility.ToHtmlStringRGB).ToArray();File.WriteAllText(Root+"/Palette.json",JsonUtility.ToJson(new PaletteRecord{hex=hex},true));
 // Keep rules, effects and authoring defaults on the same ten-color ordering.
 string artPath="Assets/WoolGame/Scripts/WoolArt.cs";string text=File.ReadAllText(artPath);int start=text.IndexOf("public static readonly Color[] Palette = {");int end=text.IndexOf("};",start);if(start<0||end<0)throw new InvalidOperationException("Palette anchor missing");string replacement="public static readonly Color[] Palette = {\n            "+string.Join(", ",hex.Select(x=>"Hex(\""+x+"\")"))+"\n        ";File.WriteAllText(artPath,text.Substring(0,start)+replacement+text.Substring(end));
 EditorUtility.SetDirty(rig.authoredLayout);EditorUtility.SetDirty(rig.arrowPool);EditorSceneManager.MarkSceneDirty(rig.gameObject.scene);EditorSceneManager.SaveScene(rig.gameObject.scene);AssetDatabase.SaveAssets();AssetDatabase.Refresh();return "Updated "+changed+" renderers using "+meshes.Count+" persistent meshes and "+materials.Count+" shared atlas materials. Palette: "+string.Join(",",hex);
 }
 static void ApplyTree(Transform root,int color){foreach(var renderer in root.GetComponentsInChildren<Renderer>(true)){
 Mesh mesh=null;var skin=renderer as SkinnedMeshRenderer;var filter=renderer.GetComponent<MeshFilter>();if(skin)mesh=skin.sharedMesh;else if(filter&&renderer is MeshRenderer)mesh=filter.sharedMesh;if(!mesh)continue;
 var source=renderer.sharedMaterials;if(source.Length==0||source.Any(x=>!x||!x.HasProperty("_BaseMap")))continue;
 string guid;long file;AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mesh,out guid,out file);string key=guid+"_"+file+"_"+color;
 Mesh painted;if(!meshes.TryGetValue(key,out painted)){string path=Root+"/Meshes/"+key+".asset";painted=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!painted){painted=UnityEngine.Object.Instantiate(mesh);painted.name=mesh.name+" SnakePalette "+color;painted.SetUVs(2,Enumerable.Repeat(new Vector2((color+.5f)/10f,0),painted.vertexCount).ToList());AssetDatabase.CreateAsset(painted,path);}PaintCoordinates(painted);meshes[key]=painted;}
 if(skin)skin.sharedMesh=painted;else filter.sharedMesh=painted;
 var mapped=new Material[source.Length];for(int i=0;i<source.Length;i++){Material material;if(!materials.TryGetValue(source[i],out material)){string name=source[i].name.Replace(' ','_');string path=Root+"/"+name+".mat";material=AssetDatabase.LoadAssetAtPath<Material>(path);if(!material){material=new Material(Shader.Find("WoolGame/Shared Snake Gloss"));material.name=name+" Snake Gloss";AssetDatabase.CreateAsset(material,path);}material.SetTexture("_BaseMap",source[i].GetTexture("_BaseMap"));material.SetTexture("_TailRamp",ramp);material.SetFloat("_Shine",.7f);material.enableInstancing=true;EditorUtility.SetDirty(material);materials[source[i]]=material;}mapped[i]=material;}
 renderer.sharedMaterials=mapped;renderer.SetPropertyBlock(null);EditorUtility.SetDirty(renderer);if(filter)EditorUtility.SetDirty(filter);changed++;
 }}
 public static string RefineHighlights(){
 foreach(string guid in AssetDatabase.FindAssets("t:Mesh",new[]{Root+"/Meshes"})){var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(guid));PaintCoordinates(mesh);EditorUtility.SetDirty(mesh);}
 var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/SnakeColorRamp.png");var readable=Read(texture);var record=JsonUtility.FromJson<PaletteRecord>(File.ReadAllText(Root+"/Palette.json"));for(int y=0;y<10;y++){Color color;ColorUtility.TryParseHtmlString("#"+record.hex[y],out color);for(int x=0;x<128;x++){var p=readable.GetPixel(x,y);p.a=Mathf.Max(color.r,color.g,color.b);readable.SetPixel(x,y,p);}}readable.Apply();File.WriteAllBytes(Root+"/SnakeColorRamp.png",readable.EncodeToPNG());UnityEngine.Object.DestroyImmediate(readable);AssetDatabase.ImportAsset(Root+"/SnakeColorRamp.png");AssetDatabase.SaveAssets();return "Flat surfaces now receive a painted gloss spot with snake-matched base brightness";
 }
 static void PaintCoordinates(Mesh mesh){var bounds=mesh.bounds;var uv=mesh.vertices.Select(v=>new Vector2(Mathf.InverseLerp(bounds.min.x,bounds.max.x,v.x),Mathf.InverseLerp(bounds.min.z,bounds.max.z,v.z))).ToList();mesh.SetUVs(3,uv);}
 static Texture2D Read(Texture2D src){var rt=RenderTexture.GetTemporary(src.width,src.height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);Graphics.Blit(src,rt);var old=RenderTexture.active;RenderTexture.active=rt;var result=new Texture2D(src.width,src.height,TextureFormat.RGBA32,false);result.ReadPixels(new Rect(0,0,src.width,src.height),0,0);result.Apply();RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);return result;}
 static void Backup(string path){if(File.Exists(path)){string target="Docs/Backups/SharedSnakePalette/"+Path.GetFileName(path);if(!File.Exists(target))File.Copy(path,target);}}
 [Serializable] class PaletteRecord {public string[] hex;}
 }
}
