using UnityEngine;
using UnityEngine.Rendering;
namespace WoolGame
{
    // Reuses the existing section transforms and colliders; only rendering is grouped.
    [DisallowMultipleComponent]
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
            int groups=rig.models.tailMeshes.Length;
            matrices=new Matrix4x4[groups][];counts=new int[groups];
            for(int i=0;i<groups;i++)matrices[i]=new Matrix4x4[1023];
            filters=new MeshFilter[rig.yarn.Length];renderers=new Renderer[rig.yarn.Length];
            for(int i=0;i<filters.Length;i++){filters[i]=rig.yarn[i].GetComponent<MeshFilter>();renderers[i]=rig.yarn[i].GetComponent<Renderer>();}
        }
        void LateUpdate()
        {
            if(matrices==null)return;
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
            if(renderers!=null)foreach(var renderer in renderers)if(renderer)renderer.forceRenderingOff=false;
        }
    }
}
