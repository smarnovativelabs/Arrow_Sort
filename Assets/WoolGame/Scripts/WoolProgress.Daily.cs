using System;
using UnityEngine;
namespace WoolGame
{
    public static partial class WoolProgress
    {
        [Serializable] sealed class DailyRecord
        {
            public string day;
            public int wins,colorMask,claims;
            public float wool;
            public bool spun;
            public int spinReward;
            public int spinSlot=-1;
        }
        public static readonly string[] DailyTitles=WoolDailyTaskState.Titles;
        public static readonly int[] DailyTargets=WoolDailyTaskState.Targets;
        public static readonly int[] DailyRewards=WoolDailyTaskState.Credits;
        static DailyRecord Daily
        {
            get
            {
                EnsureLoaded();string today=DateTime.Now.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture);
                // Do not re-grant rewards when the device clock moves backwards.
                if(data.daily==null||string.CompareOrdinal(today,data.daily.day)>0)data.daily=new DailyRecord{day=today};
                return data.daily;
            }
        }
        public static int DailyCount(int task)=>DailyTasks.Count(task);
        public static bool DailyClaimed(int task)=>DailyTasks.Claimed(task);
        public static bool ClaimDailyTask(int task)
        {
            int before=DailyCredits;if(!DailyTasks.Claim(task))return false;
            data.dailyChestCollected=false;data.dailyChestCredits+=DailyCredits-before;Save();return true;
        }
        public static void RecordDailyWool(int color,float amount)
        {
            if(amount<=0||color<0||color>=WoolArt.Palette.Length)return;
            Daily.wool+=amount;Daily.colorMask|=1<<color;
        }
        public static TimeSpan DailyResetIn=>DateTime.Now.Date.AddDays(1)-DateTime.Now;
        public static bool NoAdsActive { get { EnsureLoaded();return data.noAdsUntilUnix>0&&(!WoolTrustedClock.Ready||WoolTrustedClock.UnixNow<data.noAdsUntilUnix); } }
        public static TimeSpan NoAdsRemaining { get { EnsureLoaded();return TimeSpan.FromSeconds(WoolTrustedClock.Ready?Math.Max(0,data.noAdsUntilUnix-WoolTrustedClock.UnixNow):0); } }
        public static void Flush(){if(data!=null)Save();}
    }
}

