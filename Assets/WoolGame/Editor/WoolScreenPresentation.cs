using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WoolGame.Editor
{
    public static class WoolScreenPresentation
    {
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before authoring.");
            var rig=UnityEngine.Object.FindFirstObjectByType<WoolSceneRig>();
            var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/WoolGame/Designer/Crossbow/DesignerBowAtlas.mat");
            var shader=Shader.Find("Universal Render Pipeline/Unlit");
            if(!rig||!material||!shader)throw new InvalidOperationException("Required scene, atlas or URP shader missing.");
            material.shader=shader;material.shaderKeywords=Array.Empty<string>();
            material.SetColor("_BaseColor",Color.white);material.SetFloat("_Surface",0);
            material.SetFloat("_Cull",2);material.renderQueue=-1;EditorUtility.SetDirty(material);
            WoolDesignerRefresh.BindBowAnimation();

            // Continue the background's edge texels beyond its authored rectangle.
            // The original center, slots and all gameplay geometry remain undistorted.
            var background=rig.authoredLayout.environment.GetComponentsInChildren<MeshRenderer>(true)
                .First(r=>r.name=="Game_Background_Grass_NoPath");
            var b=background.bounds;
            float[] xs={b.min.x-30,b.min.x,b.max.x,b.max.x+30};
            float[] zs={b.min.z-30,b.min.z,b.max.z,b.max.z+30};
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
            for(int z=0;z<4;z++)for(int x=0;x<4;x++)
            {
                vertices.Add(new Vector3(xs[x],b.center.y,zs[z]));
                uv.Add(new Vector2(x<2?1f-.0005f:.0005f,z<2?1f-.0005f:z==2?.0005f:.25f));
            }
            for(int z=0;z<3;z++)for(int x=0;x<3;x++)
            {
                if(x==1&&z==1)continue;
                int i=z*4+x;triangles.AddRange(new[]{i,i+4,i+1,i+1,i+4,i+5});
            }
            const string path="Assets/WoolGame/Designer/BackgroundEdges.asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(!mesh){mesh=new Mesh{name="Background edges"};AssetDatabase.CreateAsset(mesh,path);}
            mesh.Clear();mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
            var root=rig.authoredLayout.environment.transform.Find("Screen background edges");
            if(!root)
            {
                root=new GameObject("Screen background edges",typeof(MeshFilter),typeof(MeshRenderer)).transform;
                root.SetParent(rig.authoredLayout.environment.transform,true);
            }
            root.GetComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=root.GetComponent<MeshRenderer>();renderer.sharedMaterial=background.sharedMaterial;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
            var fog=rig.authoredLayout.environment.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="Fog");
            if(fog){var scale=fog.localScale;scale.x=82;fog.localScale=scale;}
            // Keep the top composition fixed and reserve clear space below the rail for boosters.
            float extraHeight=23.8f-rig.authoredLayout.cameraSize;
            rig.gameCamera.transform.position-=rig.gameCamera.transform.up*extraHeight;
            rig.authoredLayout.cameraSize=23.8f;EditorUtility.SetDirty(rig.authoredLayout);
            rig.hud.ConfigureBottomLayout();EditorUtility.SetDirty(rig.hud);
            rig.FitCamera();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(rig.gameObject.scene);EditorSceneManager.SaveScene(rig.gameObject.scene);
            return "Full-screen background, slot-sized bows and unlit atlas saved.";
        }
    }
}
