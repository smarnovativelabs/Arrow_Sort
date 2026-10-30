using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WoolGame.Editor
{
    public static class WoolDuckAuthoring
    {
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before authoring ducks.");
            var rig=UnityEngine.Object.FindFirstObjectByType<WoolSceneRig>();
            const string path="Assets/Art/Fbx/Duck_Baby_Duck.fbx";
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(!source||!rig)throw new InvalidOperationException("Duck FBX or scene rig missing.");
            var clips=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
            if(!rig.duckRescue)
            {
                var root=new GameObject("Duck rescue family");Undo.RegisterCreatedObjectUndo(root,"Add duck rescue family");
                root.transform.SetParent(rig.authoredLayout.environment.transform,true);
                rig.duckRescue=root.AddComponent<WoolDuckRescue>();
            }
            var rescue=rig.duckRescue;
            if(!rescue.mother)rescue.mother=Create(source,rescue.transform,true,out rescue.motherModel);
            if(!rescue.baby)rescue.baby=Create(source,rescue.transform,false,out rescue.babyModel);
            rescue.motherJump=clips.Single(c=>c.name=="Duck_Mother_Rig|Duck_Happy_Jump");
            rescue.motherWalk=clips.Single(c=>c.name=="Duck_Mother_Rig|Duck_Walk");
            rescue.babyJump=clips.Single(c=>c.name=="Duck_Baby|Duck_Happy_Jump");
            rescue.babyWalk=clips.Single(c=>c.name=="Duck_Baby|Duck_Walk");
            var eating=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Fbx/duck eating .fbx").OfType<AnimationClip>().ToArray();
            rescue.motherEat=eating.Single(c=>c.name=="Duck_Mother_Rig|Duck_Eating");
            rescue.babyEat=eating.Single(c=>c.name=="Duck_Baby|Duck_Baby_Eating");
            BindAnimator(rescue.motherModel,"MotherDuck",rescue.motherWalk,rescue.motherEat,rescue.motherJump);
            BindAnimator(rescue.babyModel,"BabyDuck",rescue.babyWalk,rescue.babyEat,rescue.babyJump);
            const string materialPath="Assets/WoolGame/Designer/DuckAtlas.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(!material)
            {
                material=new Material(source.GetComponentInChildren<Renderer>(true).sharedMaterial){name="DuckAtlas"};
                material.shader=Shader.Find("Universal Render Pipeline/Unlit");material.shaderKeywords=Array.Empty<string>();
                AssetDatabase.CreateAsset(material,materialPath);
            }
            foreach(var renderer in rescue.GetComponentsInChildren<Renderer>(true))renderer.sharedMaterial=material;
            rig.kitten.gameObject.SetActive(false);rescue.gameObject.SetActive(false);
            EditorUtility.SetDirty(rescue);EditorUtility.SetDirty(rig);
            foreach(var text in rig.hud.GetComponentsInChildren<TMPro.TMP_Text>(true))
                if(text.text.Contains("Kittens")||text.text.Contains("KITTENS")||text.text.Contains("kitten"))
                {text.text=text.text.Replace("Kittens Rescued","Duck Families Rescued").Replace("KITTENS","DUCK FAMILY").Replace("kitten","duck family");EditorUtility.SetDirty(text);}
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(rig.gameObject.scene);EditorSceneManager.SaveScene(rig.gameObject.scene);
            return "Mother and baby rigs bound to their supplied walk/jump clips; scene saved.";
        }
        static Transform Create(GameObject source,Transform parent,bool mother,out GameObject model)
        {
            var holder=new GameObject(mother?"Mother duck":"Baby duck").transform;holder.SetParent(parent,false);
            model=UnityEngine.Object.Instantiate(source,holder);model.name=mother?"Mother animated model":"Baby animated model";
            foreach(var animator in model.GetComponentsInChildren<Animator>(true))UnityEngine.Object.DestroyImmediate(animator);
            foreach(var child in model.transform.Cast<Transform>().ToArray())
                if(mother?!child.name.StartsWith("Duck_Mother"):!child.name.StartsWith("Duck_Baby"))UnityEngine.Object.DestroyImmediate(child.gameObject);
            var skeleton=model.transform.Find(mother?"Duck_Mother_Rig":"Duck_Baby");
            float scale=mother?.75f:1.3f;var rotation=Quaternion.Euler(0,180,0);
            model.transform.localRotation=rotation;model.transform.localScale=Vector3.one*scale;
            model.transform.localPosition=-(rotation*skeleton.localPosition)*scale;
            foreach(var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))renderer.updateWhenOffscreen=true;
            return holder;
        }
        public static void BindAnimator(GameObject model,string name,AnimationClip walk,AnimationClip feed,AnimationClip jump)
        {
            const string folder="Assets/WoolGame/Animations/Ducks";
            System.IO.Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            string path=folder+"/"+name+".controller";
            var controller=AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(path);
            if(!controller)controller=UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(path);
            var machine=controller.layers[0].stateMachine;
            string[] names={"Feed","Walk","Jump"};AnimationClip[] motions={feed,walk,jump};
            for(int i=0;i<names.Length;i++){
                var state=machine.states.Select(s=>s.state).FirstOrDefault(s=>s.name==names[i]);
                if(!state)state=machine.AddState(names[i],new Vector3(250,i*80));
                state.motion=motions[i];state.writeDefaultValues=true;if(i==0)machine.defaultState=state;
            }
            var animator=model.GetComponent<Animator>();if(!animator)animator=model.AddComponent<Animator>();
            animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;animator.enabled=true;
            if(!model.GetComponent<WoolDuckAnimationDriver>())model.AddComponent<WoolDuckAnimationDriver>();
            foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=true;
            EditorUtility.SetDirty(controller);EditorUtility.SetDirty(animator);EditorUtility.SetDirty(model);
        }
    }
}
