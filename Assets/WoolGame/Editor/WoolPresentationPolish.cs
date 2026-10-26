using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace WoolGame.Editor
{
    public static class WoolPresentationPolish
    {
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            var rig=UnityEngine.Object.FindFirstObjectByType<WoolSceneRig>();
            Undo.RegisterFullObjectHierarchyUndo(rig.gameObject,"Polish rescue presentation");
            var eating=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Fbx/duck eating .fbx").OfType<AnimationClip>().ToArray();
            rig.duckRescue.motherEat=eating.Single(c=>c.name=="Duck_Mother_Rig|Duck_Eating");
            rig.duckRescue.babyEat=eating.Single(c=>c.name=="Duck_Baby|Duck_Baby_Eating");
            var plus=AssetDatabase.LoadAllAssetsAtPath("Assets/newCompleteSprite.png").OfType<Sprite>().Single(s=>s.name=="newCompleteSprite_22");
            var gold=Material("SlotWarmGold",new Color(.94f,.66f,.22f));
            var inset=Material("SlotDeepTeal",new Color(.15f,.30f,.32f));
            var ring=Ring();
            for(int i=0;i<rig.slots.Length;i++)
            {
                var slot=rig.slots[i];
                var disk=slot.Find("Round slot");disk.GetComponent<Renderer>().sharedMaterial=inset;
                var rim=slot.Find("Cream rim");rim.localScale=Vector3.one;rim.localPosition=Vector3.zero;
                rim.GetComponent<MeshFilter>().sharedMesh=ring;rim.GetComponent<Renderer>().sharedMaterial=gold;
                if(!rig.slotUnlockIcons[i]){rig.slotUnlockIcons[i]=new GameObject("Unlock slot plus",typeof(SpriteRenderer));rig.slotUnlockIcons[i].transform.SetParent(slot,false);}
                var icon=rig.slotUnlockIcons[i];var sr=icon.GetComponent<SpriteRenderer>();sr.sprite=plus;sr.color=Color.white;
                icon.transform.localPosition=new Vector3(0,0,-.25f);icon.transform.localRotation=Quaternion.identity;
                icon.transform.localScale=Vector3.one*(1.96f/plus.bounds.size.x);
                icon.SetActive(i>=4);rig.counts[i].text="";
            }
            rig.hud.AuthorPresentationPolish();
            EditorUtility.SetDirty(rig.duckRescue);EditorUtility.SetDirty(rig);EditorUtility.SetDirty(rig.hud);
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(rig.gameObject.scene);EditorSceneManager.SaveScene(rig.gameObject.scene);
            return "Eating clips, eight gold-rimmed slots, full-size unlock icons and readable ability cards saved.";
        }
        static Material Material(string name,Color color)
        {
            string path="Assets/WoolGame/Designer/UI/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Unlit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);EditorUtility.SetDirty(m);return m;
        }
        static Mesh Ring()
        {
            const int n=64;var vertices=new Vector3[n*2];var triangles=new int[n*6];
            for(int i=0;i<n;i++)
            {
                float a=i*Mathf.PI*2/n;var d=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);
                vertices[i*2]=d*.98f+Vector3.back*.17f;vertices[i*2+1]=d*.80f+Vector3.back*.20f;
                int j=(i+1)%n*2,k=i*6;triangles[k]=i*2;triangles[k+1]=j+1;triangles[k+2]=j;
                triangles[k+3]=i*2;triangles[k+4]=i*2+1;triangles[k+5]=j+1;
            }
            const string path="Assets/WoolGame/Designer/UI/SlotBevelRing.asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!mesh){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}
            mesh.Clear();mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);return mesh;
        }
    }
}
