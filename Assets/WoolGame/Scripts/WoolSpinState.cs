using System;
using System.Collections.Generic;
namespace WoolGame
{
    [Serializable]
    public sealed class WoolSpinState
    {
        public const int RewardPolicy=2,DaysRequired=30;
        public long lastDailyDay=-1;
        public int streak,extraSpins,levelProgress,unlockPowers,lastSlot=-1,lastCoins;
        public long noAdsUntil;
        public int rewardPolicy;
        public long lastSpinUtc,lastMilestoneDay=-1,noAdsGrantedUtc;
        public List<long> qualifyingDays=new List<long>(),noAdsEvidence=new List<long>();
        public bool lastSpinMilestone;
        public static readonly int[] Coins={0,1000,0,500,0,250,0,400};
        public bool UpgradePolicy()
        {
            if(rewardPolicy==RewardPolicy){if(qualifyingDays==null)qualifyingDays=new List<long>();if(noAdsEvidence==null)noAdsEvidence=new List<long>();return false;}
            // Old streaks/random No Ads grants have no daily evidence. Preserve used daily spins,
            // earned bonus tickets and purchased inventory, but start guarded eligibility fresh.
            rewardPolicy=RewardPolicy;streak=0;qualifyingDays=new List<long>();noAdsEvidence=new List<long>();
            noAdsUntil=noAdsGrantedUtc=lastSpinUtc=0;lastMilestoneDay=-1;lastSpinMilestone=false;return true;
        }
        bool TimeAllowed(long utc)=>utc>0&&utc<=253402300799L&&utc>=lastSpinUtc&&WoolTrustedClock.LocalDay(utc)>=lastDailyDay;
        public bool DailyReady(long utc)=>TimeAllowed(utc)&&WoolTrustedClock.LocalDay(utc)>lastDailyDay;
        public int Available(long utc)=>!TimeAllowed(utc)?0:(int)Math.Min(int.MaxValue,(long)Math.Max(0,extraSpins)+(DailyReady(utc)?1:0));
        public int CurrentStreak(long utc)=>utc<=0?0:WoolTrustedClock.LocalDay(utc)>lastDailyDay+1?0:Math.Max(0,streak);
        public int CycleProgress(long utc)=>CurrentStreak(utc)==0?0:Math.Min(DaysRequired-1,qualifyingDays?.Count??0);
        static bool Consecutive(List<long> days,long end)
        {
            if(days==null||days.Count!=DaysRequired)return false;
            for(int i=0;i<DaysRequired;i++)if(days[i]!=end-DaysRequired+1+i)return false;
            return true;
        }
        public bool HasNoAds(long utc)=>rewardPolicy==RewardPolicy&&utc>0&&noAdsGrantedUtc>0&&utc>=noAdsGrantedUtc&&utc<noAdsUntil&&noAdsUntil-noAdsGrantedUtc==86400&&lastMilestoneDay==WoolTrustedClock.LocalDay(noAdsGrantedUtc)&&Consecutive(noAdsEvidence,lastMilestoneDay);
        public int Spin(long utc,int randomSlot)
        {
            UpgradePolicy();if(Available(utc)==0)return -1;
            bool milestone=false;lastSpinMilestone=false;
            if(DailyReady(utc))
            {
                long day=WoolTrustedClock.LocalDay(utc);
                bool consecutive=lastDailyDay==day-1;
                if(!consecutive){qualifyingDays.Clear();streak=0;}
                if(qualifyingDays.Count>0&&qualifyingDays[qualifyingDays.Count-1]!=day-1)qualifyingDays.Clear();
                streak=streak==int.MaxValue?streak:streak+1;lastDailyDay=day;qualifyingDays.Add(day);
                milestone=Consecutive(qualifyingDays,day)&&(lastMilestoneDay<0||day-lastMilestoneDay>=DaysRequired);
                if(milestone)
                {
                    noAdsEvidence=new List<long>(qualifyingDays);qualifyingDays.Clear();
                    lastMilestoneDay=day;noAdsGrantedUtc=utc;noAdsUntil=utc+86400;
                    if(extraSpins<int.MaxValue)extraSpins++;lastSpinMilestone=true;
                }
            }
            else extraSpins--;
            lastSpinUtc=utc;
            // Slot zero is exclusive to the fully validated 30-day milestone.
            lastSlot=milestone?0:randomSlot<=0?6:Math.Min(7,randomSlot);
            lastCoins=Coins[lastSlot];
            return lastSlot;
        }
        public void CompleteLevel(){if(++levelProgress>=3){levelProgress=0;if(extraSpins<int.MaxValue)extraSpins++;}}
    }
}
