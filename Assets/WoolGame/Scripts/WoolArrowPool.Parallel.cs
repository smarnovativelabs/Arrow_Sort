using UnityEngine;
namespace WoolGame {public sealed partial class WoolArrowPool {
 readonly WoolArrowPool[] parallelShooters=new WoolArrowPool[8];bool parallelWorker;
 bool AnyParallelBusy(bool flight){foreach(var worker in parallelShooters)if(worker&&(flight?worker.stage==ShotStage.Travel:worker.SerialBusy))return true;return false;}
 int ParallelCount(bool hits){int sum=0;foreach(var worker in parallelShooters)if(worker)sum+=hits?worker.localHits:worker.localShots;return sum;}
 public bool SlotBusy(int slot)=>slot==0?SerialBusy:parallelShooters[slot]&&parallelShooters[slot].SerialBusy;
 public int TickSlot(int slot,float dt){var worker=slot==0?this:parallelShooters[slot];if(!worker)return -1;worker.AllowFogTargets=AllowFogTargets;return worker.TickSerial(dt);}
 public void CancelSlot(int slot){var worker=slot==0?this:parallelShooters[slot];if(worker)worker.CancelLocal();}
 WoolArrowPool Worker(int slot){
  if(parallelShooters[slot])return parallelShooters[slot];
  var root=new GameObject("Independent crossbow slot "+(slot+1));root.transform.SetParent(transform,false);var worker=root.AddComponent<WoolArrowPool>();
  JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(this),worker);worker.parallelWorker=true;worker.impactProperties=new MaterialPropertyBlock();
  if(hitEffect){worker.hitEffect=Instantiate(hitEffect,root.transform);worker.hitEffect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
  if(muzzleEffect){worker.muzzleEffect=Instantiate(muzzleEffect,root.transform);worker.muzzleEffect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
  if(impactSegment){worker.impactSegment=Instantiate(impactSegment,root.transform);worker.impactRenderer=worker.impactSegment.GetComponent<MeshRenderer>();worker.impactSegment.gameObject.SetActive(false);}
  parallelShooters[slot]=worker;return worker;
 }
}}
