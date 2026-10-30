using System.Collections.Generic;
using UnityEngine;
namespace WoolGame {
public sealed partial class WoolGameController {
 void StoreRewardedAbility(int kind){WoolProgress.AwardAbilities(kind,1);paused=false;hud.Hide();hud.RefreshAbilityInventory();gameAudio.Play(WoolGameAudio.Cue.Coin);hud.Hint("+1 "+WoolBoosters.Names[kind]+" saved. Tap its button whenever you want to use it.");}
 bool SectionOnScreen(int i){if(i<0||i>=yarn.Count||!yarn[i].gameObject.activeInHierarchy||yarnAmounts[i]<=0)return false;var screen=view.WorldToScreenPoint(yarn[i].position);return screen.z>0&&view.pixelRect.Contains(screen)&&!(rig.authoredLayout&&rig.authoredLayout.IsInFog(yarn[i].position));}
 int VisibleAbilitySections(){int count=0;for(int i=0;i<yarn.Count;i++)if(SectionOnScreen(i))count++;return count;}
 List<int> RemovalTargetOrder(int color){var result=new List<int>();for(int pass=0;pass<2;pass++)for(int i=0;i<level.Yarn.Count;i++)if(level.Yarn[i]==color&&yarnAmounts[i]>0&&SectionOnScreen(i)==(pass==0))result.Add(i);return result;}
 void SortForReadyVehicles(){
  var slots=new List<int>();for(int i=0;i<active.Length;i++)if(active[i]!=null&&active[i].arrived&&active[i].ammo>0)slots.Add(i);
  slots.Sort((a,b)=>CollectorPrecedes(a,b)?-1:CollectorPrecedes(b,a)?1:0);var priority=new List<int>();foreach(int slot in slots){int color=level.Tiles[active[slot].index].color;if(!priority.Contains(color))priority.Add(color);}
  if(priority.Count==0)for(int i=0;i<level.Tiles.Count;i++){var tile=level.Tiles[i];if(!tile.hidden&&!tile.selected&&!tile.probing&&level.CanSelect(i)&&!priority.Contains(tile.color))priority.Add(tile.color);}
  level.SortColors(++sortUses,yarnAmounts,priority);
 }
}
}
