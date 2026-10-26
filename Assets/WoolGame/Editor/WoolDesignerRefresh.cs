using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WoolGame.Editor
{
    public static class WoolDesignerRefresh
    {
        const string Output="Docs/DesignerRefresh20261016";
        static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        public static string Apply()
        {
            Require(!EditorApplication.isPlaying,"Exit Play Mode before authoring.");
            var rig=UnityEngine.Object.FindFirstObjectByType<WoolSceneRig>();
            Require(rig&&rig.authoredLayout,"Designer scene is required.");
            Directory.CreateDirectory(Output);
            var layout=rig.authoredLayout;
            if(rig.vehicles.Length<WoolEndlessGenerator.MaxVehicles)
            {
                Undo.RecordObject(rig,"Expand vehicle pool to 70");
                var expanded=new Transform[WoolEndlessGenerator.MaxVehicles];Array.Copy(rig.vehicles,expanded,rig.vehicles.Length);
                for(int i=rig.vehicles.Length;i<expanded.Length;i++)
                {
                    var actor=UnityEngine.Object.Instantiate(rig.vehicles[0],rig.vehicles[0].parent);actor.name="Vehicle "+i;actor.gameObject.SetActive(false);
                    Undo.RegisterCreatedObjectUndo(actor.gameObject,"Add vehicle pool entry");expanded[i]=actor;
                }
                rig.vehicles=expanded;EditorUtility.SetDirty(rig);
            }
            Undo.RecordObject(layout,"Bind designer tunnels and tail");
            var tunnel=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Fbx/TUNNEL .fbx");
            Require(tunnel,"Designer tunnel FBX is missing.");
            var filters=tunnel.GetComponentsInChildren<MeshFilter>(true);
            layout.railPrefab=MakePrefab(filters.First(f=>f.name.Trim()=="Long Tunnel"),"DesignerRail",new Vector3(9.5f,2.1f,1.25f),Quaternion.Euler(0,0,-90)*Quaternion.Euler(-90,0,0));
            layout.tunnelPrefab=MakePrefab(filters.First(f=>f.name.Trim()=="Tunnel"),"DesignerTunnel",new Vector3(1.65f,1.75f,.85f),Quaternion.Euler(0,0,180)*Quaternion.Euler(-90,0,0));
            if(!layout.tailTip)
            {
                var root=new GameObject("Designer dragon tail tip");Undo.RegisterCreatedObjectUndo(root,"Add designer tail tip");
                root.transform.SetParent(rig.world,false);layout.tailTip=root.transform;
                var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Fbx/Tail_Tip.fbx");
                var source=asset.GetComponentInChildren<MeshFilter>(true);
                var visual=new GameObject("Tail_Tip");visual.transform.SetParent(root.transform,false);
                visual.AddComponent<MeshFilter>().sharedMesh=source.sharedMesh;
                visual.AddComponent<MeshRenderer>().sharedMaterials=source.GetComponent<MeshRenderer>().sharedMaterials;
                visual.transform.localRotation=Quaternion.Euler(0,90,270);
                var b=RotatedBounds(source.sharedMesh.bounds,visual.transform.localRotation);
                visual.transform.localScale=new Vector3(.55f,.55f,.55f);
                // The attachment end faces forward, with the point extending behind the final segment.
                visual.transform.localPosition=new Vector3(-b.max.x*.55f+.10f,-b.center.y*.55f,0);
                root.SetActive(false);
            }
            var head=rig.dragon.GetChild(0);Undo.RecordObject(head,"Attach head to body height");
            head.localPosition=new Vector3(0,0,.65f);
            foreach(var actor in rig.vehicles)
            {
                var trail=actor.GetComponent<TrailRenderer>();
                if(!trail)trail=Undo.AddComponent<TrailRenderer>(actor.gameObject);
                Undo.RecordObject(trail,"Configure color departure trail");
                var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/WoolGame/Designer/DepartureTrail.mat");
                if(!material)
                {
                    material=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));material.name="DepartureTrail";
                    material.SetFloat("_Surface",1);material.SetFloat("_Blend",0);material.SetFloat("_ZWrite",0);
                    material.SetFloat("_SrcBlend",5);material.SetFloat("_DstBlend",10);material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.renderQueue=3000;
                    AssetDatabase.CreateAsset(material,"Assets/WoolGame/Designer/DepartureTrail.mat");
                }
                trail.sharedMaterial=material;trail.emitting=false;trail.time=.28f;trail.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;trail.receiveShadows=false;
            }
            EditorUtility.SetDirty(layout);EditorSceneManager.MarkSceneDirty(rig.gameObject.scene);
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(rig.gameObject.scene);
            return "Designer rail, tunnel, tail tip and trails bound; head height aligned; scene saved.";
        }
        static Bounds RotatedBounds(Bounds b,Quaternion rotation)
        {
            var result=new Bounds(rotation*b.center,Vector3.zero);
            for(int i=0;i<8;i++)result.Encapsulate(rotation*(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1))));
            return result;
        }
        public static string BindBowAnimation()
        {
            Require(!EditorApplication.isPlaying,"Exit Play Mode before authoring bows.");
            var rig=UnityEngine.Object.FindFirstObjectByType<WoolSceneRig>();var layout=rig.authoredLayout;
            layout.bowAttack=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Fbx/Bow Aniamtion.fbx").OfType<AnimationClip>().First(c=>c.name=="Armature|Armature|ArmatureAction");
            string[] files={"Red Bow","Cyan Bow","Yellow Bow","Purple Bow","Green Bow","Orange Bow","pink bow ","Blue Bow","Lime Bow","Light Pink Bow"};
            int count=0;
            var atlas=AssetDatabase.LoadAssetAtPath<Material>("Assets/WoolGame/Designer/Crossbow/DesignerBowAtlas.mat");Require(atlas,"Designer bow atlas is missing.");
            foreach(var slot in layout.slots)for(int c=0;c<10;c++)
            {
                var v=slot.visuals[c];
                if(v.animationRoot)
                {
                    v.animationRoot.transform.localScale=new Vector3(1.53f,2,1.72f);
                    foreach(var renderer in v.animationRoot.GetComponentsInChildren<Renderer>(true))renderer.sharedMaterial=atlas;
                    layout.bowAttack.SampleAnimation(v.animationRoot,0);continue;
                }
                var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Fbx/"+files[c]+".fbx");Require(source,"Missing bow "+files[c]);
                Undo.RecordObject(layout,"Bind designer bow animations");
                foreach(Transform child in v.frame.Cast<Transform>().ToArray())Undo.DestroyObjectImmediate(child.gameObject);
                v.bowstring=null;
                var instance=UnityEngine.Object.Instantiate(source,v.frame);instance.name="Designer animated "+files[c];
                Undo.RegisterCreatedObjectUndo(instance,"Add designer bow rig");
                foreach(var animator in instance.GetComponentsInChildren<Animator>(true))UnityEngine.Object.DestroyImmediate(animator);
                foreach(Transform child in instance.transform)if(child.name.EndsWith("Armature"))child.name="Armature";
                foreach(var renderer in instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    renderer.sharedMaterial=atlas;
                    if(renderer.name.Contains("Arrow")){renderer.gameObject.SetActive(false);continue;}
                    renderer.updateWhenOffscreen=true;
                }
                instance.transform.localPosition=new Vector3(0,.02f,.12f);
                instance.transform.localRotation=Quaternion.identity;instance.transform.localScale=new Vector3(1.53f,2,1.72f);
                v.animationRoot=instance;layout.bowAttack.SampleAnimation(instance,0);count++;
            }
            rig.arrowPool.ResetPool();
            EditorUtility.SetDirty(layout);EditorSceneManager.MarkSceneDirty(rig.gameObject.scene);EditorSceneManager.SaveScene(rig.gameObject.scene);
            return "Bound designer animation to "+count+" slot/color bow rigs.";
        }
        static GameObject MakePrefab(MeshFilter source,string name,Vector3 size,Quaternion rotation)
        {
            var root=new GameObject(name);var visual=new GameObject(source.name);visual.transform.SetParent(root.transform,false);
            visual.AddComponent<MeshFilter>().sharedMesh=source.sharedMesh;visual.AddComponent<MeshRenderer>().sharedMaterials=source.GetComponent<MeshRenderer>().sharedMaterials;
            visual.transform.localRotation=rotation;
            var b=RotatedBounds(source.sharedMesh.bounds,rotation);
            root.transform.localScale=new Vector3(size.x/b.size.x,size.y/b.size.y,size.z/b.size.z);
            visual.transform.localPosition=new Vector3(-b.center.x,-b.center.y,-b.max.z);
            var prefab=PrefabUtility.SaveAsPrefabAsset(root,"Assets/WoolGame/Designer/"+name+".prefab");UnityEngine.Object.DestroyImmediate(root);return prefab;
        }
        public static string ValidateLevels(int count=500)
        {
            var rig=UnityEngine.Object.FindFirstObjectByType<WoolSceneRig>();int min=10000,max=0;
            for(int n=1;n<=count;n++)
            {
                var data=WoolEndlessGenerator.Generate(n,rig.levels[1].route);
                try
                {
                    Require(data.yarn.Length>=250&&data.yarn.Length<=400,"Segment budget level "+n);
                    Require(data.vehicles.Length>=50&&data.vehicles.Length<=70,"Vehicle pool level "+n);
                    int color=(n%10)*7%10;int cars=data.vehicles.Count(t=>t.color==color);
                    Require(cars>=2&&cars<=3,"Scarce cars level "+n);
                    Require(Array.IndexOf(data.yarn,color)<data.yarn.Length/10&&Array.LastIndexOf(data.yarn,color)>data.yarn.Length*.9f,"Separated scarce color level "+n);
                    for(int c=0;c<10;c++)Require(data.yarn.Count(v=>v==c)==data.vehicles.Where(t=>t.color==c).Sum(t=>WoolLevel.Capacity(t.size)),"Ammo conservation level "+n);
                    var rules=new WoolLevel(data);
                    foreach(int index in data.tutorialOrder)
                    {
                        var tile=rules.Tiles[index];
                        if(tile.source!=WoolVehicleSource.Board)tile.hidden=false;
                        Require(rules.CanSelect(index),"Blocked solution level "+n+" tile "+index);
                        tile.selected=true;
                    }
                    min=Math.Min(min,data.yarn.Length);max=Math.Max(max,data.yarn.Length);
                }
                finally{UnityEngine.Object.DestroyImmediate(data);}
            }
            string report=count+" levels passed: segment range "+min+"-"+max+", 2-3 scarce vehicles, separated color, exact ammunition totals, legal complete exit order and pool bounds.";
            Directory.CreateDirectory(Output);File.WriteAllText(Output+"/level-validation.txt",report);return report;
        }
        public static string Capture(string name,int width=720,int height=1280)
        {
            var rig=UnityEngine.Object.FindFirstObjectByType<WoolSceneRig>();var camera=rig.gameCamera;
            var previous=camera.targetTexture;var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(width,height,24);
            var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            try{camera.targetTexture=rt;rig.FitCamera();camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();Directory.CreateDirectory(Output);File.WriteAllBytes(Output+"/"+name+".png",texture.EncodeToPNG());}
            finally{camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(texture);rig.FitCamera();}
            return Output+"/"+name+".png";
        }
        static int[] playLevels;static int playLevelIndex,playOrderIndex,playSteps;static WoolGameController game;
        static System.Reflection.MethodInfo update;
        static readonly List<string> playResults=new List<string>();
        public static string BeginPlayValidation()
        {
            Require(EditorApplication.isPlaying,"Play Mode required.");
            game=UnityEngine.Object.FindFirstObjectByType<WoolGameController>();game.automatedInput=true;
            playLevels=new[]{1,2,3,4,5,6,7,8,10,25,50};playLevelIndex=playOrderIndex=playSteps=0;playResults.Clear();
            update=typeof(WoolGameController).GetMethod("Update",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            game.LoadLevel(playLevels[0]);return "Play validation started";
        }
        public static string StepPlayValidation(int steps=1000)
        {
            for(int s=0;s<steps&&playLevelIndex<playLevels.Length;s++)
            {
                if(game.CurrentPhase=="Lost")throw new InvalidOperationException("Live solver lost level "+game.CurrentLevel+" at order "+playOrderIndex+" remaining "+game.RemainingYarn);
                if(game.RemainingYarn==0)
                {
                    playResults.Add("Level "+game.CurrentLevel+" completed with four normal slots; "+playSteps+" simulation frames.");
                    playLevelIndex++;playOrderIndex=playSteps=0;
                    File.WriteAllLines(Output+"/play-validation.txt",playResults);
                    if(playLevelIndex==playLevels.Length)return string.Join("\n",playResults);
                    game.LoadLevel(playLevels[playLevelIndex]);
                }
                while(playOrderIndex<game.SolutionOrder.Count&&game.SelectTile(game.SolutionOrder[playOrderIndex]))playOrderIndex++;
                update.Invoke(game,null);playSteps++;
                Require(playSteps<100000,"Live solver stalled level "+game.CurrentLevel);
            }
            return playLevelIndex>=playLevels.Length?"ALL PLAY LEVELS PASSED":"Passed "+playLevelIndex+" levels; running "+game.CurrentLevel+", remaining "+game.RemainingYarn+", order "+playOrderIndex+", frames "+playSteps;
        }
    }
}
