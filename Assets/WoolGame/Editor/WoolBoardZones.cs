using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace WoolGame.Editor
{
    public static class WoolBoardZones
    {
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Author outside Play Mode.");
            var rig=Object.FindFirstObjectByType<WoolSceneRig>();
            var background=GameObject.Find("Game_Background_Grass_NoPath");
            var filter=background.GetComponent<MeshFilter>();
            // Align texture transitions to the existing eight-slot row. Preserve path and rule coordinates.
            float[] xs={-41,-11,11,41};float[] zs={-44,-22,-4.5f,-1.4f,26,50};
            float[] vs={.9995f,.9995f,.674f,.55f,.0005f,.25f};
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
            for(int z=0;z<zs.Length;z++)for(int x=0;x<xs.Length;x++)
            {vertices.Add(background.transform.InverseTransformPoint(new Vector3(xs[x],-.32f,zs[z])));uv.Add(new Vector2(x<2?.9995f:.0005f,vs[z]));}
            for(int z=0;z<zs.Length-1;z++)for(int x=0;x<xs.Length-1;x++){int i=z*4+x;triangles.AddRange(new[]{i,i+4,i+1,i+1,i+4,i+5});}
            const string path="Assets/WoolGame/Designer/AlignedBoardZones.asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!mesh){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}
            mesh.name="Green environment - eight grey slots - white vehicles";mesh.Clear();mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();filter.sharedMesh=mesh;EditorUtility.SetDirty(mesh);
            var edges=rig.authoredLayout.environment.transform.Find("Screen background edges");if(edges)edges.gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(rig.gameObject.scene);AssetDatabase.SaveAssets();
            return "Green ends at z=-4.5; grey strip -4.5 to -1.4 contains all eight slots at z=-3.05; white board below.";
        }
    }
}
