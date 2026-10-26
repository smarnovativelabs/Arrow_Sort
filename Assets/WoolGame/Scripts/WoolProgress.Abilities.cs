using System;
namespace WoolGame {
public static partial class WoolProgress {
 public static int AbilityCount(int kind){EnsureLoaded();switch(kind){case 0:return UnlockPowerCount;case 1:return Math.Max(0,data.removePowers);case 2:return Math.Max(0,data.sortPowers);case 3:return Math.Max(0,data.boostPowers);default:return 0;}}
 public static void AwardAbilities(int kind,int count){if(kind<0||kind>3||count<=0)return;EnsureLoaded();int value=(int)Math.Min(int.MaxValue,(long)AbilityCount(kind)+count);switch(kind){case 0:if(data.spin==null)data.spin=new WoolSpinState();data.spin.unlockPowers=value;break;case 1:data.removePowers=value;break;case 2:data.sortPowers=value;break;case 3:data.boostPowers=value;break;}Save();}
 public static bool UseAbility(int kind){if(AbilityCount(kind)<=0)return false;if(kind==0)return UseUnlockPower();switch(kind){case 1:data.removePowers--;break;case 2:data.sortPowers--;break;case 3:data.boostPowers--;break;default:return false;}Save();return true;}
}}
