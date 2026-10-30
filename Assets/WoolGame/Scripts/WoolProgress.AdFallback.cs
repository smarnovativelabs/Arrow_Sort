using System;
namespace WoolGame {
 public static partial class WoolProgress {
  public const int UnavailableAdDailyLimit=3;
  static void RefreshUnavailableAdDay(){
   EnsureLoaded();long day=WoolTrustedClock.LocalDay(WoolTrustedClock.UnixNow);
   if(day>data.unavailableAdRewardDay){data.unavailableAdRewardDay=day;data.unavailableAdRewardClaims=0;}
  }
  public static int UnavailableAdRewardsRemaining {get{RefreshUnavailableAdDay();return Math.Max(0,UnavailableAdDailyLimit-data.unavailableAdRewardClaims);}}
  public static int UnavailableAdCoinAmount(int requestedValue)=>(int)((long)Math.Max(0,requestedValue)*30/100);
  public static int GrantUnavailableAdCoins(int requestedValue){
   RefreshUnavailableAdDay();int before=Coins;
   int amount=(int)Math.Min(int.MaxValue-(long)before,UnavailableAdCoinAmount(requestedValue));
   if(UnavailableAdRewardsRemaining==0||amount<=0)return 0;
   data.unavailableAdRewardClaims++;SetCoins(before+amount);return amount;
  }
 }
}
