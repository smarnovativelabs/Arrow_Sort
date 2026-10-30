#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
namespace WoolGame.Editor {
// Bake only the eight FBX road entry caps. No mesh vertex access is needed in a player.
public sealed class WoolPathEntryPostprocessor:AssetPostprocessor {
 public override uint GetVersion(){return 3;}
 void OnPostprocessModel(GameObject root){
  if(assetPath!="Assets/Art/Fbx/Level 1 to 8.fbx")return;
  foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true)){
   Extend(filter.sharedMesh,"Dragon_Path_L1",932,new Vector3(0f,-1.46929036E-09f,-0.0336134434f),new[]{0,1,458,459,460,461,462,463,465,466,566,567,699,700,702,704});
   Extend(filter.sharedMesh,"Dragon_Path_L2",936,new Vector3(0.00371975568f,-1.46292367E-09f,-0.033467792f),new[]{0,1,2,3,4,5,458,459,460,461,462,463,464,465,466,467,468,469,470,471,585,586,587,588,702,703,704,705,706,708,710,712});
   Extend(filter.sharedMesh,"Dragon_Path_L3",1472,new Vector3(0f,-1.46929036E-09f,-0.0336134434f),new[]{0,1,730,731,732,733,734,735,737,738,921,922,1105,1106,1108,1110});
   Extend(filter.sharedMesh,"Dragon_Path_L4",1472,new Vector3(0f,-1.46929036E-09f,-0.0336134434f),new[]{0,1,730,731,732,733,734,735,737,738,921,922,1105,1106,1108,1110});
   Extend(filter.sharedMesh,"Dragon_Path_L5",1000,new Vector3(0f,-1.46929036E-09f,-0.0336134434f),new[]{1,2,251,252,502,504,506,508});
   ExtrudeEntry(filter.sharedMesh,"Dragon_Path_L6",new Vector3(.009806991f,-1.424446E-09f,-.0325875245f));
   ExtrudeEntry(filter.sharedMesh,"Dragon_Path_L7",new Vector3(.009807431f,-1.4244419E-09f,-.03258743f));
   Extend(filter.sharedMesh,"Dragon_Path_L8",700,new Vector3(0.00369092752f,-1.46302193E-09f,-0.033470042f),new[]{1,2,176,177,352,354,356,358});
  }
 }
 static void Extend(Mesh mesh,string expected,int count,Vector3 delta,int[] indices){
  if(!mesh||mesh.name!=expected)return;
  if(mesh.vertexCount!=count)throw new System.InvalidOperationException("Road FBX topology changed; update entry-cap indices for "+expected);
  var vertices=mesh.vertices;foreach(int i in indices)vertices[i]+=delta;mesh.vertices=vertices;mesh.RecalculateBounds();
 }
 // L6 and L7 have long bottom faces that must not be stretched:
 // retain the original corner and extrude the four edges of its end cap instead.
 static void ExtrudeEntry(Mesh mesh,string expected,Vector3 delta){
  if(!mesh||mesh.name!=expected)return;
  if(mesh.vertexCount!=1000||mesh.subMeshCount!=1)throw new System.InvalidOperationException("Road FBX topology changed; review the entry cap for "+expected);
  var source=mesh.vertices;var normals=new List<Vector3>(mesh.normals);var vertices=new List<Vector3>(source);
  var sourceIndex=new List<int>();for(int i=0;i<source.Length;i++)sourceIndex.Add(i);
  float end=float.PositiveInfinity;foreach(var v in source)end=Mathf.Min(end,v.z);
  var triangles=new List<int>();var cap=new List<int>();var edges=new List<Vector2Int>();
  void Edge(int a,int b){int reverse=edges.FindIndex(e=>e.x==b&&e.y==a);if(reverse>=0)edges.RemoveAt(reverse);else edges.Add(new Vector2Int(a,b));}
  var original=mesh.triangles;
  for(int i=0;i<original.Length;i+=3){int a=original[i],b=original[i+1],c=original[i+2];
   if(Mathf.Abs(source[a].z-end)<.000001f&&Mathf.Abs(source[b].z-end)<.000001f&&Mathf.Abs(source[c].z-end)<.000001f){cap.Add(a);cap.Add(b);cap.Add(c);Edge(a,b);Edge(b,c);Edge(c,a);}
   else{triangles.Add(a);triangles.Add(b);triangles.Add(c);}
  }
  if(cap.Count!=6||edges.Count!=4)throw new System.InvalidOperationException("Expected a two-triangle, four-edge entry cap for "+expected);
  void Triangle(int a,Vector3 pa,int b,Vector3 pb,int c,Vector3 pc){
   var normal=Vector3.Cross(pb-pa,pc-pa).normalized;int start=vertices.Count;
   vertices.Add(pa);vertices.Add(pb);vertices.Add(pc);sourceIndex.Add(a);sourceIndex.Add(b);sourceIndex.Add(c);
   normals.Add(normal);normals.Add(normal);normals.Add(normal);triangles.Add(start);triangles.Add(start+1);triangles.Add(start+2);
  }
  for(int i=0;i<cap.Count;i+=3){int a=cap[i],b=cap[i+1],c=cap[i+2];Triangle(a,source[a]+delta,b,source[b]+delta,c,source[c]+delta);}
  foreach(var e in edges){int a=e.x,b=e.y;Triangle(a,source[a],b,source[b],b,source[b]+delta);Triangle(a,source[a],b,source[b]+delta,a,source[a]+delta);}
  // Preserve all imported channels, including hard normals on the existing road.
  var uv=new List<Vector4>[8];for(int channel=0;channel<8;channel++){var values=new List<Vector4>();mesh.GetUVs(channel,values);if(values.Count==source.Length){uv[channel]=values;for(int i=source.Length;i<vertices.Count;i++)values.Add(values[sourceIndex[i]]);}}
  var tangents=new List<Vector4>(mesh.tangents);if(tangents.Count==source.Length)for(int i=source.Length;i<vertices.Count;i++)tangents.Add(tangents[sourceIndex[i]]);
  var colors=new List<Color>(mesh.colors);if(colors.Count==source.Length)for(int i=source.Length;i<vertices.Count;i++)colors.Add(colors[sourceIndex[i]]);
  mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetTriangles(triangles,0);
  for(int channel=0;channel<8;channel++)if(uv[channel]!=null)mesh.SetUVs(channel,uv[channel]);
  if(tangents.Count==vertices.Count)mesh.SetTangents(tangents);if(colors.Count==vertices.Count)mesh.SetColors(colors);mesh.RecalculateBounds();
 }
}
}
#endif
