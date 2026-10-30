using System;
using UnityEngine;
namespace WoolGame
{
    public static partial class WoolProgress
    {
        static WoolSpinState SpinState
        {
            get
            {
                EnsureLoaded();
                if(!data.spinMigrated&&WoolTrustedClock.Ready)
                {
                    if(data.spin==null)data.spin=new WoolSpinState();
                    // Existing used spins stay used on upgrade; device-dated future streaks grant no reward.
                    DateTime last;
                    if(DateTime.TryParse(data.lastSpinDay,out last))
                    {
                        long day=new DateTimeOffset(DateTime.SpecifyKind(last.Date,DateTimeKind.Utc)).ToUnixTimeSeconds()/86400;
                        long today=WoolTrustedClock.LocalDay(WoolTrustedClock.UnixNow);
                        data.spin.lastDailyDay=Math.Min(day,today);
                        data.spin.streak=day<=today&&day>=today-1?Math.Max(0,data.spinStreak):0;
                    }
                    data.spinMigrated=true;Save();
                }
                if(data.spin==null)data.spin=new WoolSpinState();
                if(data.spin.UpgradePolicy()){data.noAdsUntilUnix=0;Save();}
                return data.spin;
            }
        }
        public static int ReadySpinNotifications=>Math.Max(AvailableSpins,SpinState.extraSpins);
        public static int AvailableSpins=>SpinState?.Available(WoolTrustedClock.UnixNow)??0;
        public static int SpinStreak=>SpinState?.CurrentStreak(WoolTrustedClock.UnixNow)??0;
        public static int SpinLevelProgress=>SpinState?.levelProgress??0;
        public static int SpinMilestoneProgress=>SpinState.CycleProgress(WoolTrustedClock.UnixNow);
        public static bool LastSpinMilestone=>SpinState.lastSpinMilestone;
        public static int UnlockPowerCount {get{EnsureLoaded();return data.spin?.unlockPowers??0;}}
        public static bool DailySpinUsed=>!WoolTrustedClock.Ready||!(SpinState?.DailyReady(WoolTrustedClock.UnixNow)??false);
        public static int LastSpinSlot=>SpinState?.lastSlot??-1;
        public static int LastSpinReward=>SpinState?.lastCoins??0;
        public static int WheelCoinReward(int slot)=>slot>=0&&slot<8?WoolSpinState.Coins[slot]:0;
        public static TimeSpan SpinResetIn=>WoolTrustedClock.UntilLocalMidnight;
        public static int SpinDaily()
        {
            if(!WoolTrustedClock.Ready)return -1;
            var state=SpinState;int slot=state.Spin(WoolTrustedClock.UnixNow,UnityEngine.Random.Range(1,8));
            if(slot<0)return -1;
            if(OnboardingStep==WoolOnboardingStep.Spin)data.onboardingSpinTaken=true;
            RecordDailyCoins(state.lastCoins);data.coins+=state.lastCoins;data.collectedCoins+=state.lastCoins;data.noAdsUntilUnix=state.noAdsUntil;Save();
            if(slot==0&&AdsMediation.AdsMediationManager.instance)AdsMediation.AdsMediationManager.instance.RemoveBannerAd();
            return slot;
        }
        public static bool UseUnlockPower()
        {
            EnsureLoaded();if(data.spin==null||data.spin.unlockPowers<=0)return false;
            data.spin.unlockPowers--;Save();return true;
        }
        static void RecordSpinLevel(){EnsureLoaded();if(data.spin==null)data.spin=new WoolSpinState();data.spin.CompleteLevel();}
    }
}
