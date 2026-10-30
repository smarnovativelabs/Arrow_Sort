using System;
using System.Collections;
using UnityEngine;
namespace WoolGame {
public sealed partial class WoolGameController {
 int lessonKind=-1,lessonTarget=-1;
 int pointingTile=-1;Action pointingAction;Collider pointingCollider;
 void UpdateTutorialPointer(){
  if(!hud)return;
  bool active=level!=null&&phase!=Phase.Menu&&world.gameObject.activeInHierarchy&&TutorialGuidance&&!ended&&!hud.AdVisible;
  if(!active){hud.PointLevelTutorialAt(Vector2.zero,false);return;}
  bool visible=!paused&&!transition&&!hud.ModalVisible&&tutorialHand&&tutorialHand.gameObject.activeSelf&&tutorialStep<level.Solution.Count;
  if(!visible){hud.BlockLevelTutorial();return;}
  int index=level.Solution[tutorialStep];
  if(pointingTile!=index||pointingAction==null){pointingTile=index;pointingAction=()=>SelectTile(index);pointingCollider=tileViews[index].GetComponent<Collider>();}
  Vector2 center=rig.vehicleCamera.WorldToScreenPoint(tileViews[index].position),size=Vector2.one*150;
  if(pointingCollider){var bounds=pointingCollider.bounds;var min=Vector2.one*float.MaxValue;var max=Vector2.one*float.MinValue;
   for(int corner=0;corner<8;corner++){var offset=new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1);Vector2 screen=rig.vehicleCamera.WorldToScreenPoint(bounds.center+Vector3.Scale(bounds.extents,offset));min=Vector2.Min(min,screen);max=Vector2.Max(max,screen);}center=(min+max)*.5f;size=max-min;
  }
  hud.PointLevelTutorialAt(center,true,size,pointingAction);
 }
 IEnumerator BeginAbilityLesson(){int kind=Array.IndexOf(WoolBoosters.UnlockLevels,level.Number);if(kind<0||WoolProgress.AbilityLessonSeen(kind)||WoolProgress.HasCompleted(level.Number))yield break;
 if(WoolProgress.AbilityCount(kind)<=0){WoolProgress.CompleteAbilityLesson(kind);if(kind==1)WoolProgress.SetRemovalLessonTargetPending(false);yield break;}
 while(entering||hud.ModalVisible||(kind==2&&VisibleAbilitySections()<Mathf.Min(8,level.Yarn.Count))){if(ended||phase==Phase.Menu)yield break;yield return null;}
 if(WoolProgress.AbilityCount(kind)<=0){WoolProgress.CompleteAbilityLesson(kind);yield break;}lessonKind=kind;
 if(ended)yield break;
 if(kind==1)lessonTarget=FindLessonBlocker();
 paused=true;
 if(kind==1&&WoolProgress.RemovalLessonTargetPending){paused=false;ActivateBooster(1,0,true);paused=true;ShowRemovalLessonTarget();yield break;}
 string[] copy={"Tap to add a slot","Tap to remove a vehicle","Match colours to ready vehicles","Attack wool through the fog"};
 hud.ShowAbilityGuide(kind,copy[kind],()=>{hud.HideAbilityGuide();paused=false;if(kind==0)pendingSlot=4;ActivateBooster(kind,0,true);if(kind==1){WoolProgress.SetRemovalLessonTargetPending(true);paused=true;ShowRemovalLessonTarget();}});
 }
 bool AnyVehicleMovingForLesson(){foreach(var bow in active)if(bow!=null&&!bow.arrived)return true;return false;}
 void DockLessonVehicle(int index,int slot){if(index<0||index>=level.Tiles.Count)return;var tile=level.Tiles[index];tile.selected=true;active[slot]=new ActiveCrossbow{index=index,slot=slot,ammo=tile.ammo,initialAmmo=tile.ammo,arrived=true,arrivalOrder=++collectionSequence};tileViews[index].gameObject.SetActive(false);rig.arrowPool.Dock(slot,tile.color,world.TransformPoint(Slot(slot)));rig.arrowPool.Reveal(slot);UpdateCrossbowCounts();}
 int FindLessonBlocker(){for(int i=0;i<level.Tiles.Count;i++)for(int j=0;j<level.Tiles.Count;j++)if(i!=j&&WoolLevel.BlocksPath(level.Tiles[j],level.Tiles[i],level.Tiles[j].direction))return i;return level.Solution[0];}
 void ShowRemovalLessonTarget(){hud.ShowWorldAbilityGuide(tileViews[lessonTarget],rig.vehicleCamera,"Tap this vehicle",()=>{paused=false;hud.HideAbilityGuide();RemoveBox(lessonTarget);});}
 void CompleteAbilityLesson(int kind){if(lessonKind!=kind)return;WoolProgress.CompleteAbilityLesson(kind);if(kind==1)WoolProgress.SetRemovalLessonTargetPending(false);lessonKind=-1;lessonTarget=-1;hud.HideAbilityGuide();}
 void MoveColorToTail(int color){for(int i=0;i<level.Yarn.Count;i++)if(level.Yarn[i]==color){int j=level.Yarn.Count-1;while(j>i&&level.Yarn[j]==color)j--;if(j<=i)break;SwapWool(i,j);}}
 void SwapWool(int a,int b){int c=level.Yarn[a];level.Yarn[a]=level.Yarn[b];level.Yarn[b]=c;float amount=yarnAmounts[a];yarnAmounts[a]=yarnAmounts[b];yarnAmounts[b]=amount;}
 void BringActiveColorForward(){
 for(int slot=0;slot<active.Length;slot++)if(active[slot]!=null){
  int color=level.Tiles[active[slot].index].color;var colors=new System.Collections.Generic.List<int>();var amounts=new System.Collections.Generic.List<float>();
  for(int pass=0;pass<2;pass++)for(int i=0;i<level.Yarn.Count;i++)if((level.Yarn[i]==color)==(pass==0)){colors.Add(level.Yarn[i]);amounts.Add(yarnAmounts[i]);}
  level.Yarn.Clear();level.Yarn.AddRange(colors);yarnAmounts.Clear();yarnAmounts.AddRange(amounts);SyncYarnViews();break;
 }
 }

 MaterialPropertyBlock sortProperties;
 void SetSortWhite(float amount){if(sortProperties==null)sortProperties=new MaterialPropertyBlock();foreach(var renderer in yarnRenderers)White(renderer,amount);}

 void White(Renderer renderer,float amount){if(!renderer)return;renderer.GetPropertyBlock(sortProperties);sortProperties.SetFloat("_SortWhite",amount);
 if(renderer.sharedMaterial&&renderer.sharedMaterial.HasProperty("_BaseMap")&&!renderer.sharedMaterial.HasProperty("_SortWhite")){sortProperties.SetTexture("_BaseMap",amount>=.7f?Texture2D.whiteTexture:renderer.sharedMaterial.GetTexture("_BaseMap"));if(renderer.sharedMaterial.HasProperty("_BaseColor"))sortProperties.SetColor("_BaseColor",Color.Lerp(renderer.sharedMaterial.GetColor("_BaseColor"),Color.white,amount));}
 renderer.SetPropertyBlock(sortProperties);}
 IEnumerator AbilityEffect(int kind,int slot){if(kind==2){
 transition=true;PositionYarn();
 for(float t=0;t<.3f;t+=Time.unscaledDeltaTime){SetSortWhite(Mathf.Clamp01(t/.3f));yield return null;}
 SetSortWhite(1);yield return new WaitForSecondsRealtime(.35f);
 SortForReadyVehicles();SyncYarnViews();PositionYarn();SetSortWhite(1);
 hud.ShowAbilityEffect(2,new Vector2(Screen.width*.5f,Screen.height*.72f));
 yield return new WaitForSecondsRealtime(.2f);
 for(float t=0;t<.65f;t+=Time.unscaledDeltaTime){SetSortWhite(1-Mathf.SmoothStep(0,1,t/.65f));yield return null;}
 SetSortWhite(0);yield return new WaitForSecondsRealtime(.25f);
 hud.HideAbilityEffect();transition=false;hud.Hint("Dragon colours grouped for ready vehicles.");hud.RefreshAbilityInventory();yield break;
 }transition=true;hud.ShowAbilityEffect(kind,kind==0&&slot>=0?rig.SlotCamera.WorldToScreenPoint(rig.slots[slot].position):new Vector3(Screen.width*.5f,Screen.height*.65f,0));
 var target=kind==0&&slot>=0?rig.slots[slot]:dragonHead;var original=target.localScale;
 for(float t=0;t<1.65f;t+=Time.unscaledDeltaTime){float p=t/1.65f;target.localScale=original*(1+Mathf.Sin(p*Mathf.PI)*.16f);if(kind==2){PositionYarn();for(int i=0;i<yarn.Count;i++){float wave=Mathf.Clamp01(1-Mathf.Abs(p-(float)i/Mathf.Max(1,yarn.Count))/.15f);yarn[i].localScale*=1+wave*.3f;}}yield return null;}
 target.localScale=original;PositionYarn();hud.HideAbilityEffect();transition=false;hud.RefreshAbilityInventory();
 }
 IEnumerator RemoveVehicleEffect(int index){
  transition=true;var vehicle=tileViews[index];var pos=vehicle.localPosition;var scale=vehicle.localScale;var rotation=vehicle.localRotation;
  var order=RemovalTargetOrder(level.Tiles[index].color);var targets=new System.Collections.Generic.List<Vector2>();
  foreach(int i in order){if(targets.Count>=Mathf.Min(6,level.Tiles[index].ammo))break;targets.Add(SectionOnScreen(i)?(Vector2)view.WorldToScreenPoint(yarn[i].position):new Vector2(Screen.width*(.35f+.06f*targets.Count),Screen.height+160));}
  if(targets.Count==0)targets.Add(new Vector2(Screen.width*.5f,Screen.height+160));
  yield return hud.PlayRemovalVolley(rig.vehicleCamera.WorldToScreenPoint(vehicle.position),targets,WoolArt.Palette[level.Tiles[index].color]);
  if(!level.RemoveBox(index,yarnAmounts,order)){transition=false;yield break;}
  SyncYarnViews();UpdateGrid();if(vehicleSources)vehicleSources.NotifyRemoved(index);hud.Status(level.Number,money,RemainingYarn,total);SaveTutorialState();
  for(float t=0;t<.35f;t+=Time.unscaledDeltaTime){float p=t/.35f;vehicle.localScale=scale*(1-p);vehicle.localPosition=pos+Vector3.up*p*.5f;yield return null;}
  vehicle.gameObject.SetActive(false);vehicle.localPosition=pos;vehicle.localScale=scale;vehicle.localRotation=rotation;transition=false;hud.RefreshAbilityInventory();hud.Hint("Vehicle and matching dragon colour cleared.");if(RemainingYarn==0)Win();
 }
}
}
