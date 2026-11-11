using UnityEngine;
using System.Collections.Generic;
namespace WoolGame{
public sealed class WoolVehicleContactShadow:MonoBehaviour{
 static Material shared;Transform shadow;BoxCollider body;MeshRenderer shadowRenderer;
 static readonly List<WoolVehicleContactShadow> instances=new List<WoolVehicleContactShadow>();
 static readonly Matrix4x4[] batch=new Matrix4x4[1023];
 static Mesh quad;
 public static void DrawBatched(){
  if(!SystemInfo.supportsInstancing||!shared||!quad)return;
  shared.enableInstancing=true;
  for(int layer=0;layer<32;layer++){
   int count=0;
   foreach(var instance in instances){
    if(!instance||!instance.shadowRenderer)continue;
    var renderer=instance.shadowRenderer;renderer.forceRenderingOff=true;
    if(renderer.gameObject.layer==layer&&renderer.enabled&&renderer.gameObject.activeInHierarchy)batch[count++]=renderer.localToWorldMatrix;
   }
   if(count>0)Graphics.DrawMeshInstanced(quad,0,shared,batch,count,null,UnityEngine.Rendering.ShadowCastingMode.Off,false,layer,null,UnityEngine.Rendering.LightProbeUsage.Off);
  }
 }
 public static void RestoreRendering(){foreach(var instance in instances)if(instance&&instance.shadowRenderer)instance.shadowRenderer.forceRenderingOff=false;}

 Vector3 previousPosition;Quaternion previousRotation;int stillFrames;
 void Awake(){instances.Add(this);body=GetComponent<BoxCollider>();foreach(var renderer in GetComponentsInChildren<Renderer>(true))renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}
 void OnEnable(){previousPosition=transform.position;previousRotation=transform.rotation;stillFrames=0;if(shadow)shadow.gameObject.SetActive(false);}
 void LateUpdate(){if(!body||!transform.parent)return;
 bool moving=(transform.position-previousPosition).sqrMagnitude>.00000001f||Quaternion.Angle(transform.rotation,previousRotation)>.05f;
 previousPosition=transform.position;previousRotation=transform.rotation;
 stillFrames=moving?0:Mathf.Min(stillFrames+1,2);
 if(moving||stillFrames<2){if(shadow)shadow.gameObject.SetActive(false);return;}
 if(shadow&&shadow.gameObject.activeSelf&&shadow.parent==transform.parent)return;
 if(!shadow){var go=GameObject.CreatePrimitive(PrimitiveType.Quad);go.name="Soft vehicle contact shadow";Destroy(go.GetComponent<Collider>());go.layer=gameObject.layer;shadow=go.transform;shadow.SetParent(transform.parent,false);if(!shared)shared=new Material(Shader.Find("WoolGame/Vehicle Contact Shadow"));var renderer=go.GetComponent<MeshRenderer>();shadowRenderer=renderer;quad=go.GetComponent<MeshFilter>().sharedMesh;renderer.sharedMaterial=shared;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;}
 if(shadow.parent!=transform.parent)shadow.SetParent(transform.parent,false);shadow.gameObject.SetActive(true);
 // Axis-aligned collider bounds keep only horizontal and vertical shadow silhouettes.
 var bounds=body.bounds;Vector3 min=Vector3.one*float.MaxValue,max=Vector3.one*float.MinValue;for(int i=0;i<8;i++){var point=transform.parent.InverseTransformPoint(bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));min=Vector3.Min(min,point);max=Vector3.Max(max,point);}var center=(min+max)*.5f;shadow.localPosition=new Vector3(center.x+.025f,center.y-.045f,max.z+.025f);shadow.localRotation=Quaternion.identity;shadow.localScale=new Vector3((max.x-min.x)*1.16f,(max.y-min.y)*1.16f,1);}
 void OnDisable(){if(shadow)shadow.gameObject.SetActive(false);}
 void OnDestroy(){instances.Remove(this);if(shadow)Destroy(shadow.gameObject);}
}}
