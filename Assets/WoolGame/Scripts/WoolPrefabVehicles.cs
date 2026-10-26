using System.Collections.Generic;
using UnityEngine;
namespace WoolGame
{
    // Prefab variants are warmed once and returned to their queues between levels.
    public sealed class WoolPrefabVehicles : MonoBehaviour
    {
        public GameObject vehiclePrefab;
        public Transform[] templates; // size * 10 + color, references inside Vehicles.prefab
        readonly Queue<Transform>[] available=new Queue<Transform>[30];
        readonly Transform[] rented=new Transform[WoolEndlessGenerator.MaxVehicles];
        readonly int[] keys=new int[WoolEndlessGenerator.MaxVehicles];
        bool ready;
        Transform Create(int key)
        {
            var instance=Instantiate(templates[key],transform);instance.name=templates[key].name+" pooled";
            WoolSceneRig.SetViewLayer(instance,"Vehicle View");instance.localPosition=Vector3.zero;instance.localRotation=Quaternion.identity;instance.localScale=Vector3.one;
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
        public void Release(int index)
        {
            var item=rented[index];if(!item)return;item.gameObject.SetActive(false);item.SetParent(transform,false);available[keys[index]].Enqueue(item);rented[index]=null;
        }
        public void ResetPool(){for(int i=0;i<rented.Length;i++)Release(i);}
    }
}

