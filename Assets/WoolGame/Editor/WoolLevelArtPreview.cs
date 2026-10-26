using TMPro;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Rendering;

namespace WoolGame.Editor
{
    // Render-only objects in Unity's private preview scene. No gameplay components or saves.
    internal sealed class WoolLevelArtPreview : IDisposable
    {
        PreviewRenderUtility utility;
        GameObject root;
        Texture2D curve;
        readonly System.Collections.Generic.List<UnityEngine.Mesh> previewMeshes=new System.Collections.Generic.List<UnityEngine.Mesh>();
        Spline spline;
        Vector3[] points;
        float[] distances;
        public float Length {get;private set;}
        void SourcePreview(GameObject prefab,Vector3 position)
        {
            if(!prefab)return;var instance=UnityEngine.Object.Instantiate(prefab,root.transform);instance.transform.localPosition=position;
        }
        public Vector3 Position(float distance)
        {
            if(points==null)return Vector3.zero;
            distance=Mathf.Clamp(distance,0,Length);int lo=0,hi=points.Length-1;
            while(hi-lo>1){int m=(lo+hi)/2;if(distances[m]<distance)lo=m;else hi=m;}
            return Vector3.Lerp(points[lo],points[hi],Mathf.InverseLerp(distances[lo],distances[hi],distance));
        }
        public void Rebuild(WoolSceneRig rig,WoolLevelDefinition data,float headProgress)
        {
            Dispose();if(!rig||!data||data.route==null||data.route.Length<2)return;
            utility=new PreviewRenderUtility();root=new GameObject("Level editor art preview"){hideFlags=HideFlags.HideAndDontSave};utility.AddSingleGO(root);
            spline=new Spline();foreach(var p in data.route)spline.Add(new Unity.Mathematics.float3(p.x,p.y,p.z),TangentMode.AutoSmooth);
            points=new Vector3[1025];distances=new float[1025];
            for(int i=0;i<points.Length;i++){points[i]=(Vector3)SplineUtility.EvaluatePosition(spline,i/1024f);if(i>0)distances[i]=distances[i-1]+Vector3.Distance(points[i],points[i-1]);}
            Length=distances[1024];
            curve=new Texture2D(1024,1,TextureFormat.RGBAFloat,false,true){hideFlags=HideFlags.HideAndDontSave,wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
            var pixels=new Color[1024];for(int i=0;i<pixels.Length;i++){var p=Position(Length*i/1023f);pixels[i]=new Color(p.x,p.y,p.z,1);}curve.SetPixels(pixels);curve.Apply();
            if(data.tutorial&&rig.tutorialEnvironment)CopyMeshes(rig.tutorialEnvironment.transform,Vector3.zero);
            if(!data.tutorial&&rig.standardEnvironment)
            {
                var meadow=rig.standardEnvironment.transform.Find("Knitted meadow");if(meadow)CopyMeshes(meadow,Vector3.zero);
                var original=rig.standardEnvironment.GetComponentInChildren<LineRenderer>(true);
                if(original){var road=new GameObject("Road").AddComponent<LineRenderer>();road.transform.SetParent(root.transform);road.sharedMaterial=original.sharedMaterial;road.useWorldSpace=true;road.widthMultiplier=original.widthMultiplier;road.numCapVertices=12;road.numCornerVertices=8;road.positionCount=321;for(int i=0;i<321;i++){var p=Position(Length*i/320f);p.z=.6f;road.SetPosition(i,p);}}
            }
            for(int i=0;i<Math.Min(8,rig.slots.Length);i++)
            {
                var position=i<4?new Vector3((i-1.5f)*2.1f,data.tutorial?-.9f:2.25f,.4f):new Vector3((i-5.5f)*2.1f,data.tutorial?-1.62f:1.35f,.4f);
                CopyMeshes(rig.slots[i],position-rig.slots[i].position);
            }
            if(data.vehicles!=null)foreach(var tile in data.vehicles)
            {
                if(tile!=null&&((tile.source==WoolVehicleSource.Rail&&tile.sourceOrder>=6)||(tile.source==WoolVehicleSource.Container&&tile.sourceOrder>0)))continue;
                if(tile==null||!rig.authoredLayout||!rig.authoredLayout.vehiclePool)continue;
                var template=rig.authoredLayout.vehiclePool.templates[(int)tile.size*10+tile.color];
                var mesh=template.GetComponent<MeshFilter>().sharedMesh;if(!mesh)continue;
                var rotation=Quaternion.Euler(0,0,-(int)tile.direction*90+data.boardRotation)*Quaternion.Euler(-90,0,0);
                var size=mesh.bounds.size;var scale=new Vector3(tile.width*data.boardScale/size.x,.7f/size.y,tile.length*data.boardScale/size.z);
                var position=BoardPoint(data,new Vector2(tile.x,tile.y))-rotation*Vector3.Scale(mesh.bounds.center,scale);
                Mesh("Vehicle",mesh,template.GetComponent<Renderer>().sharedMaterial,position,rotation,scale);
            }
            if(rig.authoredLayout)
            {
                if(data.hasRail)SourcePreview(rig.authoredLayout.railPrefab,new Vector3(0,WoolEndlessGenerator.RailY,0));
                if(data.containerPositions!=null)foreach(var depot in data.containerPositions)SourcePreview(rig.authoredLayout.tunnelPrefab,new Vector3(depot.x,depot.y,0));
            }
            float head=Mathf.Clamp01(headProgress)*Length;
            if(data.yarn!=null&&rig.yarn.Length>0)
            {
                var mesh=rig.yarn[0].GetComponent<MeshFilter>().sharedMesh;
                for(int i=0;i<Math.Min(data.yarn.Length,rig.yarn.Length);i++)
                {
                    float d=head-.23f-(i+.5f)*.43f;if(d<=0)break;int color=Mathf.Clamp(data.yarn[i],0,rig.woolMaterials.Length-1);
                    bool imported=rig.models&&rig.models.tailMeshes!=null&&color<rig.models.tailMeshes.Length&&rig.models.tailMeshes[color];
                    var direction=Position(d+.02f)-Position(d-.02f);
                    var renderer=Mesh("Wool",imported?rig.models.tailMeshes[color]:mesh,imported?rig.models.tailMaterial:rig.woolMaterials[color],Position(d)+Vector3.back*.20f,imported?Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg+180f):Quaternion.identity,new Vector3(.43f*1.45f,.82f,.65f));
                    var block=new MaterialPropertyBlock();block.SetFloat("_FollowSpline",1);block.SetVector("_SplineSection",new Vector4(d,.43f*1.45f,.82f,.65f));renderer.SetPropertyBlock(block);
                }
            }
            float headPos=head+.35f;
            var tangent=Position(headPos+.08f)-Position(headPos-.08f);
            var face=Quaternion.Euler(0,0,Mathf.Atan2(tangent.y,tangent.x)*Mathf.Rad2Deg);
            CopyMeshes(rig.dragon,Position(headPos)+Vector3.back*.08f+Vector3.back*.2f-rig.dragon.position,face*Quaternion.Inverse(rig.dragon.rotation));
            var kitten=data.tutorial?new Vector3(2.3f,.85f,-.4f):Position(Length-.7f)+Vector3.back*.4f;
            CopyMeshes(rig.kitten,kitten-rig.kitten.position);
            utility.camera.orthographic=true;utility.camera.transform.SetPositionAndRotation(new Vector3(0,0,-30),Quaternion.identity);
            utility.camera.nearClipPlane=.1f;utility.camera.farClipPlane=100;utility.camera.clearFlags=CameraClearFlags.SolidColor;utility.camera.backgroundColor=WoolArt.Hex("FAE8C8");
            utility.lights[0].intensity=1.2f;utility.lights[0].transform.rotation=Quaternion.Euler(40,-25,0);utility.lights[1].intensity=.4f;utility.ambientColor=new Color(.7f,.7f,.7f);
        }
        void PreviewBox(WoolSceneRig rig,string name,Vector3 p,Vector3 size,Color color)
        {
            var cube=GameObject.CreatePrimitive(PrimitiveType.Cube);cube.name=name;cube.transform.SetParent(root.transform);cube.transform.localPosition=p;cube.transform.localScale=size;
            UnityEngine.Object.DestroyImmediate(cube.GetComponent<Collider>());var renderer=cube.GetComponent<Renderer>();renderer.sharedMaterial=rig.mechanismMaterial;var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",color);block.SetColor("_Color",color);renderer.SetPropertyBlock(block);
        }
        static Color ColorFor(int index)=>WoolArt.Palette[Mathf.Clamp(index,0,WoolArt.Palette.Length-1)];
        public static Vector3 BoardPoint(WoolLevelDefinition data,Vector2 p)=>new Vector3(0,-3.2f,0)+Quaternion.Euler(0,0,data.boardRotation)*(new Vector3(p.x,p.y+3.2f,0)*data.boardScale);
        public static Vector2 LogicalPoint(WoolLevelDefinition data,Vector3 p)
        {var q=Quaternion.Euler(0,0,-data.boardRotation)*(p-new Vector3(0,-3.2f,0))/Mathf.Max(.01f,data.boardScale);return new Vector2(q.x,q.y-3.2f);}
        MeshRenderer Mesh(string name,UnityEngine.Mesh mesh,Material material,Vector3 p,Quaternion q,Vector3 scale)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root.transform);go.transform.SetPositionAndRotation(p,q);go.transform.localScale=scale;
            go.GetComponent<MeshFilter>().sharedMesh=mesh;var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;return r;
        }
        void CopyMeshes(Transform source,Vector3 offset,Quaternion? rotate=null)
        {
            if(!source)return;
            foreach(var filter in source.GetComponentsInChildren<MeshFilter>(true))
            {
                var renderer=filter.GetComponent<Renderer>();if(!filter.sharedMesh||!renderer||filter.GetComponent<TMP_Text>())continue;
                var copy=Mesh(filter.name,filter.sharedMesh,renderer.sharedMaterial,source.position+offset+(rotate??Quaternion.identity)*(filter.transform.position-source.position),(rotate??Quaternion.identity)*filter.transform.rotation,filter.transform.lossyScale);
                copy.sharedMaterials=renderer.sharedMaterials;var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block);copy.SetPropertyBlock(block);
            }
            foreach(var skinned in source.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var baked=new UnityEngine.Mesh();skinned.BakeMesh(baked);previewMeshes.Add(baked);
                var copy=Mesh(skinned.name,baked,skinned.sharedMaterial,source.position+offset+(rotate??Quaternion.identity)*(skinned.transform.position-source.position),(rotate??Quaternion.identity)*skinned.transform.rotation,skinned.transform.lossyScale);copy.sharedMaterials=skinned.sharedMaterials;
            }
            foreach(var original in source.GetComponentsInChildren<SpriteRenderer>(true))
            {
                var go=new GameObject(original.name,typeof(SpriteRenderer));go.transform.SetParent(root.transform);
                go.transform.SetPositionAndRotation(source.position+offset+(rotate??Quaternion.identity)*(original.transform.position-source.position),(rotate??Quaternion.identity)*original.transform.rotation);go.transform.localScale=original.transform.lossyScale;
                var copy=go.GetComponent<SpriteRenderer>();copy.sprite=original.sprite;copy.sharedMaterial=original.sharedMaterial;copy.color=original.color;copy.drawMode=original.drawMode;copy.size=original.size;copy.sortingLayerID=original.sortingLayerID;copy.sortingOrder=original.sortingOrder;
            }
        }
        public Texture Render(Rect rect,Vector3 center,float viewWidth)
        {
            if(utility==null)return null;
            var old=Shader.GetGlobalTexture("_KnotSplinePositions");float length=Shader.GetGlobalFloat("_KnotSplineLength"),texel=Shader.GetGlobalFloat("_KnotSplineTexelSize");
            try
            {
                Shader.SetGlobalTexture("_KnotSplinePositions",curve);Shader.SetGlobalFloat("_KnotSplineLength",Length);Shader.SetGlobalFloat("_KnotSplineTexelSize",1f/1024);
                utility.BeginPreview(rect,GUIStyle.none);utility.camera.aspect=rect.width/rect.height;utility.camera.orthographicSize=viewWidth*.5f/utility.camera.aspect;utility.camera.transform.position=new Vector3(center.x,center.y,-30);
                utility.Render(true);return utility.EndPreview();
            }
            finally{Shader.SetGlobalTexture("_KnotSplinePositions",old);Shader.SetGlobalFloat("_KnotSplineLength",length);Shader.SetGlobalFloat("_KnotSplineTexelSize",texel);}
        }
        public void Dispose(){if(utility!=null){utility.Cleanup();utility=null;}if(curve)UnityEngine.Object.DestroyImmediate(curve);curve=null;root=null;points=null;distances=null;Length=0;}
    }
}
