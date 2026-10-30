using System.Collections.Generic;
using UnityEngine;
namespace WoolGame
{
    // Prefab variants are warmed once and returned to their queues between levels.
    public sealed class WoolPrefabVehicles : MonoBehaviour
    {
        public GameObject vehiclePrefab;
        public Transform[] templates; // size * 10 + color, references inside Vehicles.prefab
        public Mesh smallDirectionArrow;
        public Mesh mediumDirectionArrow,largeDirectionArrow;
        public Material smallArrowOutline,smallArrowFace;
        readonly Queue<Transform>[] available=new Queue<Transform>[30];
        readonly Transform[] rented=new Transform[WoolEndlessGenerator.MaxVehicles];
        readonly int[] keys=new int[WoolEndlessGenerator.MaxVehicles];
        bool ready;
        Transform Create(int key)
        {
            var instance=Instantiate(templates[key],transform);instance.name=templates[key].name+" pooled";
            WoolSceneRig.SetViewLayer(instance,"Vehicle View");instance.localPosition=Vector3.zero;instance.localRotation=Quaternion.identity;instance.localScale=Vector3.one;
            var directionArrow=key<10?smallDirectionArrow:key<20?mediumDirectionArrow:largeDirectionArrow;
            if(directionArrow&&smallArrowOutline&&smallArrowFace)
            {
                var body=instance.GetComponent<MeshFilter>().sharedMesh.bounds;
                foreach(var filter in instance.GetComponentsInChildren<MeshFilter>(true))
                {
                    if(filter.transform==instance||!filter.name.Contains("Arrow"))continue;
                    filter.sharedMesh=directionArrow;
                    filter.transform.localScale=Vector3.one;
                    filter.transform.localRotation=Quaternion.identity;
                    filter.transform.localPosition=new Vector3(body.center.x,body.max.y-body.size.y*.088f,body.center.z);
                    filter.GetComponent<MeshRenderer>().sharedMaterials=new[]{smallArrowOutline,smallArrowFace};
                }
            }
            instance.gameObject.SetActive(false);return instance;
        }
        public void Warm()
        {
            if(ready)return;ready=true;
            for(int key=0;key<30;key++){available[key]=new Queue<Transform>();for(int n=0;n<3;n++)available[key].Enqueue(Create(key));}
        }
        public Bounds Show(int index,WoolTile tile,Transform actor)
        {
            Warm();int key=(int)tile.size*10+tile.color;
            if(rented[index]&&keys[index]!=key)Release(index);
            if(!rented[index]){rented[index]=available[key].Count>0?available[key].Dequeue():Create(key);keys[index]=key;}
            var visual=rented[index];visual.SetParent(actor,false);var bounds=visual.GetComponent<MeshFilter>().sharedMesh.bounds;
            visual.localPosition=-bounds.center;visual.localRotation=Quaternion.identity;visual.localScale=Vector3.one;visual.gameObject.SetActive(true);
            return new Bounds(Vector3.zero,bounds.size);
        }
        public void FitProportions(int index,Vector3 actorScale)
        {
            var visual=rented[index];if(!visual)return;
            // Cancel the tile collider's independent axis scales for the art only.
            // The original small/medium/large meshes retain their authored proportions.
            float scale=Mathf.Min(actorScale.x,actorScale.z)*.88f;
            // Slightly enlarge only the small FBX visual for phone readability.
            if(keys[index]/10==(int)WoolSize.Small)scale*=1.10f;
            visual.localScale=new Vector3(scale/actorScale.x,scale/actorScale.y,scale/actorScale.z);
            visual.localPosition=-Vector3.Scale(visual.GetComponent<MeshFilter>().sharedMesh.bounds.center,visual.localScale);
        }
        public void Release(int index)
        {
            var item=rented[index];if(!item)return;if(blockedShakes[index]!=null){StopCoroutine(blockedShakes[index]);blockedShakes[index]=null;item.localPosition=shakeHome[index];}item.gameObject.SetActive(false);item.SetParent(transform,false);available[keys[index]].Enqueue(item);rented[index]=null;
        }
        readonly Coroutine[] blockedShakes=new Coroutine[WoolEndlessGenerator.MaxVehicles];
        readonly Vector3[] shakeHome=new Vector3[WoolEndlessGenerator.MaxVehicles];
        public void ShakeBlocked(int index)
        {
            if(index<0||index>=rented.Length||!rented[index])return;
            var visual=rented[index];
            if(blockedShakes[index]!=null){StopCoroutine(blockedShakes[index]);visual.localPosition=shakeHome[index];}
            shakeHome[index]=visual.localPosition;blockedShakes[index]=StartCoroutine(Shake(index,visual));
        }
        System.Collections.IEnumerator Shake(int index,Transform visual)
        {
            var home=shakeHome[index];float elapsed=0;
            while(elapsed<.28f&&rented[index]==visual&&visual&&visual.gameObject.activeInHierarchy)
            {
                float p=elapsed/.28f;visual.localPosition=home+Vector3.right*(Mathf.Sin(p*Mathf.PI*8)*.13f*(1-p));
                elapsed+=Time.unscaledDeltaTime;yield return null;
            }
            if(visual&&rented[index]==visual)visual.localPosition=home;blockedShakes[index]=null;
        }
        public void ResetPool(){for(int i=0;i<rented.Length;i++)Release(i);}
    }
}

