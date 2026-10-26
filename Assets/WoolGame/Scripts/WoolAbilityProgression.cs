using System;
using System.Collections.Generic;
using UnityEngine;
namespace WoolGame {
public static partial class WoolProgress {
 public static void GrantAbilityGifts(int stage){EnsureLoaded();for(int k=0;k<4;k++)if(stage>=WoolBoosters.UnlockLevels[k]&&(data.abilityGiftMask&(1<<k))==0){data.abilityGiftMask|=1<<k;AwardAbilities(k,1);}}
 public static bool AbilityLessonSeen(int kind){EnsureLoaded();return (data.abilityTutorialMask&(1<<kind))!=0;}
 public static void CompleteAbilityLesson(int kind){EnsureLoaded();data.abilityTutorialMask|=1<<kind;Save();}
}
public static partial class WoolEndlessGenerator {
 public static bool MechanicsAllowed(WoolLevelDefinition d){if(d.number<21&&d.hasRail)return false;if(d.number<16&&d.containerPositions!=null&&d.containerPositions.Length>0)return false;foreach(var t in d.vehicles)if(t.source==WoolVehicleSource.Rail&&d.number<21||t.source==WoolVehicleSource.Container&&d.number<16)return false;return true;}
 static void ConfigureAbilityLesson(WoolLevelDefinition d){int kind=Array.IndexOf(WoolBoosters.UnlockLevels,d.number);if(kind<0)return;d.startingGraceSeconds=12;d.difficulty=DifficultyFor(d.number);d.displayName=WoolBoosters.Names[kind]+" lesson";d.difficultyNote="One free "+WoolBoosters.Names[kind]+" power. Learn it in a guided rescue situation.";
 if(kind==0){var reserve=new HashSet<int>();for(int i=0;i<Math.Min(4,d.tutorialOrder.Length);i++){int index=d.tutorialOrder[i];reserve.Add(index);d.vehicles[index].color=1;}int colorIndex=0;int[] lessonColors={3,4,8,6};foreach(var t in d.vehicles)if(!reserve.Contains(Array.IndexOf(d.vehicles,t)))t.color=lessonColors[(colorIndex++)%lessonColors.Length];d.colorCount=5;
 var colors=new List<int>();foreach(var t in d.vehicles)if(t.color!=1)for(int n=0;n<WoolLevel.Capacity(t.size);n++)colors.Add(t.color);foreach(var t in d.vehicles)if(t.color==1)for(int n=0;n<WoolLevel.Capacity(t.size);n++)colors.Add(t.color);d.yarn=colors.ToArray();}
 }
}
}
