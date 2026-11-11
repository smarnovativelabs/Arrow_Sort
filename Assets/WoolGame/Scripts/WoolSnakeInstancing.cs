using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace WoolGame
{
    // Reuses the existing section transforms and colliders; only rendering is grouped.
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(300)]
    public sealed class WoolSnakeInstancing:MonoBehaviour
    {
        public WoolSceneRig rig;
        Matrix4x4[][] matrices;
        int[] counts;
        MeshFilter[] filters;
        Renderer[] renderers;
        public int VisibleSections {get;private set;}
        public int SubmittedBatches {get;private set;}
        void Start()
        {
            if(!rig)rig=GetComponent<WoolSceneRig>();
            if(!rig||!rig.models||!SystemInfo.supportsInstancing){enabled=false;return;}
            rig.models.tailMaterial.enableInstancing=true;
            RefreshVehicles();
            int groups=rig.models.tailMeshes.Length;
            matrices=new Matrix4x4[groups][];counts=new int[groups];
            for(int i=0;i<groups;i++)matrices[i]=new Matrix4x4[1023];
            filters=new MeshFilter[rig.yarn.Length];renderers=new Renderer[rig.yarn.Length];
            for(int i=0;i<filters.Length;i++){filters[i]=rig.yarn[i].GetComponent<MeshFilter>();renderers[i]=rig.yarn[i].GetComponent<Renderer>();}
        }
        sealed class VehicleGroup {
            public Mesh mesh;public Material material;public int layer,count;
            public readonly Matrix4x4[] matrices=new Matrix4x4[1023];
            public readonly List<MeshRenderer> renderers=new List<MeshRenderer>();
        }
        readonly List<VehicleGroup> vehicleGroups=new List<VehicleGroup>();
        public void RefreshVehicles(){
            foreach(var group in vehicleGroups)foreach(var renderer in group.renderers)if(renderer)renderer.forceRenderingOff=false;
            vehicleGroups.Clear();
            if(SystemInfo.supportsInstancing&&rig)PrepareVehicles();
        }
        void PrepareVehicles(){
            foreach(var vehicle in rig.vehicles){
                if(!vehicle)continue;
                foreach(var renderer in vehicle.GetComponentsInChildren<MeshRenderer>(true)){
                    var filter=renderer.GetComponent<MeshFilter>();var material=renderer.sharedMaterial;
                    if(!filter||!filter.sharedMesh||filter.sharedMesh.subMeshCount!=1||renderer.sharedMaterials.Length!=1||!material||!material.shader.isSupported||renderer.HasPropertyBlock())continue;
                    // These shaders support instance transforms; preserve every other renderer.
                    string shader=material.shader.name;
                    if(shader!="WoolGame/Reference Block Vehicle"&&shader!="Universal Render Pipeline/Unlit")continue;
                    material.enableInstancing=true;
                    VehicleGroup group=vehicleGroups.Find(g=>g.mesh==filter.sharedMesh&&g.material==material&&g.layer==renderer.gameObject.layer);
                    if(group==null){group=new VehicleGroup{mesh=filter.sharedMesh,material=material,layer=renderer.gameObject.layer};vehicleGroups.Add(group);}
                    group.renderers.Add(renderer);
                }
            }
        }
        void DrawVehicles(){
            foreach(var group in vehicleGroups){
                group.count=0;
                foreach(var renderer in group.renderers){
                    if(!renderer)continue;
                    bool supported=renderer.gameObject.layer==group.layer&&renderer.sharedMaterial==group.material&&renderer.GetComponent<MeshFilter>().sharedMesh==group.mesh&&!renderer.HasPropertyBlock();
                    renderer.forceRenderingOff=supported;
                    if(!supported||!renderer.enabled||!renderer.gameObject.activeInHierarchy)continue;
                    group.matrices[group.count++]=renderer.localToWorldMatrix;
                }
                if(group.count>0)Graphics.DrawMeshInstanced(group.mesh,0,group.material,group.matrices,group.count,null,ShadowCastingMode.Off,false,group.layer,null,LightProbeUsage.Off);
            }
        }
        void LateUpdate()
        {
            if(matrices==null)return;
            DrawVehicles();
            WoolVehicleContactShadow.DrawBatched();
            System.Array.Clear(counts,0,counts.Length);VisibleSections=SubmittedBatches=0;
            for(int i=0;i<filters.Length;i++)
            {
                var renderer=renderers[i];
                if(!renderer)continue;
                int group=System.Array.IndexOf(rig.models.tailMeshes,filters[i].sharedMesh);
                bool supported=group>=0&&renderer.sharedMaterial==rig.models.tailMaterial&&rig.models.tailMaterial.enableInstancing;
                renderer.forceRenderingOff=supported;
                if(!supported||!renderer.enabled||!renderer.gameObject.activeInHierarchy)continue;
                matrices[group][counts[group]++]=filters[i].transform.localToWorldMatrix;VisibleSections++;
            }
            for(int i=0;i<counts.Length;i++)if(counts[i]>0)
            {
                Graphics.DrawMeshInstanced(rig.models.tailMeshes[i],0,rig.models.tailMaterial,matrices[i],counts[i],null,ShadowCastingMode.Off,false,0,rig.gameCamera,LightProbeUsage.Off);
                SubmittedBatches++;
            }
        }
        void OnDisable()
        {
            WoolVehicleContactShadow.RestoreRendering();
            foreach(var group in vehicleGroups)foreach(var renderer in group.renderers)if(renderer)renderer.forceRenderingOff=false;
            if(renderers!=null)foreach(var renderer in renderers)if(renderer)renderer.forceRenderingOff=false;
        }
    }
}
