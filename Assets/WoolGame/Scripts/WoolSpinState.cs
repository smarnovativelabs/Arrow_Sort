using System;
namespace WoolGame
{
    [Serializable]
    public sealed class WoolSpinState
    {
        public long lastDailyDay=-1;
        public int streak,extraSpins,levelProgress,unlockPowers,lastSlot=-1,lastCoins;
        public long noAdsUntil;
        public static readonly int[] Coins={0,1000,50,500,0,0,100,400};
        public bool DailyReady(long utc)=>utc>0&&WoolTrustedClock.LocalDay(utc)>lastDailyDay;
        public int Available(long utc)=>utc<=0?0:Math.Max(0,extraSpins)+(DailyReady(utc)?1:0);
        public int CurrentStreak(long utc)=>utc<=0?streak:WoolTrustedClock.LocalDay(utc)>lastDailyDay+1?0:streak;
        public int Spin(long utc,int randomSlot)
        {
            if(Available(utc)==0)return -1;
            bool milestone=false;
            if(DailyReady(utc))
            {
                long day=WoolTrustedClock.LocalDay(utc);
                streak=lastDailyDay==day-1?streak+1:1;lastDailyDay=day;
                milestone=streak%30==0;
                if(milestone)extraSpins++;
            }
            else extraSpins--;
            lastSlot=milestone?0:Math.Max(0,Math.Min(7,randomSlot));
            lastCoins=Coins[lastSlot];
            if(lastSlot==4)unlockPowers++;
            if(lastSlot==0)noAdsUntil=Math.Max(utc,noAdsUntil)+86400;
            return lastSlot;
        }
        public void CompleteLevel(){if(++levelProgress>=3){levelProgress=0;extraSpins++;}}
    }
}
