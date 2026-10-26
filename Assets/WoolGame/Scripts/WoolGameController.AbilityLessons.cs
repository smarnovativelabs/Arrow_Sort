using System;
using System.Collections;
using UnityEngine;
namespace WoolGame {
public sealed partial class WoolGameController {
 int lessonKind=-1,lessonTarget=-1;
 IEnumerator BeginAbilityLesson(){int kind=Array.IndexOf(WoolBoosters.UnlockLevels,level.Number);if(kind<0||WoolProgress.AbilityLessonSeen(kind))yield break;
 while(entering||hud.ModalVisible)yield return null;
 lessonKind=kind;
 if(kind==2||kind==3){MoveColorToTail(level.Tiles[level.Solution[0]].color);SyncYarnViews();}
 // Players load vehicles through the real selection/movement path.
 int practiceCount=kind==0?4:kind==1?0:1;
 for(int n=0;n<practiceCount;n++){
  int index=level.Solution[n];bool tapped=false;
  var screen=rig.vehicleCamera.WorldToScreenPoint(tileViews[index].position);
  hud.ShowWorldAbilityGuide(screen,kind==0?"This color is near the dragon's tail. Load this vehicle and watch its remaining arrows.":"Load this matching vehicle to see where its color is on the dragon.",()=>{
   hud.HideAbilityGuide();tapped=SelectTile(index);
  });
  while(!tapped&&!ended)yield return null;
  while((transition||AnyVehicleMovingForLesson())&&!ended)yield return null;
 }
 if(ended)yield break;
 if(kind==1)lessonTarget=FindLessonBlocker();
 paused=true;
 string[] copy={"All four slots are occupied.\nUse your FREE Unlock power to add a slot.","A vehicle blocks the way.\nUse your FREE Remove power to clear it.","Your loaded color is buried in the dragon.\nUse your FREE Sort power to bring it forward.","Matching wool is hidden in the smoke.\nUse your FREE Amplify power to reach it."};
 hud.ShowAbilityGuide(kind,copy[kind],()=>{hud.HideAbilityGuide();paused=false;if(kind==0)pendingSlot=4;ActivateBooster(kind,0,true);});
 }
 bool AnyVehicleMovingForLesson(){foreach(var bow in active)if(bow!=null&&!bow.arrived)return true;return false;}
 void DockLessonVehicle(int index,int slot){if(index<0||index>=level.Tiles.Count)return;var tile=level.Tiles[index];tile.selected=true;active[slot]=new ActiveCrossbow{index=index,slot=slot,ammo=tile.ammo,initialAmmo=tile.ammo,arrived=true,arrivalOrder=++collectionSequence};tileViews[index].gameObject.SetActive(false);rig.arrowPool.Dock(slot,tile.color,world.TransformPoint(Slot(slot)));rig.arrowPool.Reveal(slot);UpdateCrossbowCounts();}
 int FindLessonBlocker(){for(int i=0;i<level.Tiles.Count;i++)for(int j=0;j<level.Tiles.Count;j++)if(i!=j&&WoolLevel.BlocksPath(level.Tiles[j],level.Tiles[i],level.Tiles[j].direction))return i;return level.Solution[0];}
 void ShowRemovalLessonTarget(){var screen=rig.vehicleCamera.WorldToScreenPoint(tileViews[lessonTarget].position);hud.ShowWorldAbilityGuide(screen,"Tap this blocking vehicle to remove it.",()=>{hud.HideAbilityGuide();RemoveBox(lessonTarget);});}
 void CompleteAbilityLesson(int kind){if(lessonKind!=kind)return;WoolProgress.CompleteAbilityLesson(kind);lessonKind=-1;lessonTarget=-1;hud.HideAbilityGuide();}
 void MoveColorToTail(int color){for(int i=0;i<level.Yarn.Count;i++)if(level.Yarn[i]==color){int j=level.Yarn.Count-1;while(j>i&&level.Yarn[j]==color)j--;if(j<=i)break;SwapWool(i,j);}}
 void SwapWool(int a,int b){int c=level.Yarn[a];level.Yarn[a]=level.Yarn[b];level.Yarn[b]=c;float amount=yarnAmounts[a];yarnAmounts[a]=yarnAmounts[b];yarnAmounts[b]=amount;}
 void BringActiveColorForward(){for(int slot=0;slot<active.Length;slot++)if(active[slot]!=null){int color=level.Tiles[active[slot].index].color;for(int i=1;i<level.Yarn.Count;i++)if(level.Yarn[i]==color&&yarnAmounts[i]>0){SwapWool(0,i);break;}SyncYarnViews();break;}}
 IEnumerator AbilityEffect(int kind,int slot){transition=true;hud.ShowAbilityEffect(kind,kind==0&&slot>=0?rig.vehicleCamera.WorldToScreenPoint(rig.slots[slot].position):new Vector3(Screen.width*.5f,Screen.height*.65f,0));
 var target=kind==0&&slot>=0?rig.slots[slot]:dragonHead;var original=target.localScale;
 for(float t=0;t<1.15f;t+=Time.unscaledDeltaTime){float p=t/1.15f;target.localScale=original*(1+Mathf.Sin(p*Mathf.PI)*.16f);if(kind==2){PositionYarn();for(int i=0;i<yarn.Count;i++){float wave=Mathf.Clamp01(1-Mathf.Abs(p-(float)i/Mathf.Max(1,yarn.Count))/.15f);yarn[i].localScale*=1+wave*.3f;}}yield return null;}
 target.localScale=original;PositionYarn();hud.HideAbilityEffect();transition=false;hud.RefreshAbilityInventory();
 }
 IEnumerator RemoveVehicleEffect(int index){transition=true;var vehicle=tileViews[index];var pos=vehicle.localPosition;var scale=vehicle.localScale;var rotation=vehicle.localRotation;hud.ShowAbilityEffect(1,rig.vehicleCamera.WorldToScreenPoint(vehicle.position));
 for(float t=0;t<.8f;t+=Time.unscaledDeltaTime){float p=t/.8f;vehicle.localPosition=pos+Vector3.up*(p*p*2);vehicle.localScale=scale*(1-Mathf.SmoothStep(0,1,p));vehicle.localRotation=Quaternion.Euler(0,0,p*120)*rotation;yield return null;}
 vehicle.gameObject.SetActive(false);vehicle.localScale=scale;hud.HideAbilityEffect();transition=false;
 }
}
}
