using System;
using UnityEngine;
namespace WoolGame
{
    public static partial class WoolProgress
    {
        static string TaskDay=>DateTime.Now.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture);
        static WoolDailyTaskState DailyTasks
        {
            get
            {
                EnsureLoaded();string today=TaskDay;
                if(data.dailyTasks==null||string.CompareOrdinal(today,data.dailyTasks.day)>0)data.dailyTasks=new WoolDailyTaskState{day=today,version=2};
                data.dailyTasks.Upgrade();return data.dailyTasks;
            }
        }
        public static int DailyCredits=>DailyTasks.Points;
        public static bool DailyChestCollected {get{EnsureLoaded();return data.dailyChestCollected;}}
        public static int DailyChestCredits {get{EnsureLoaded();return Mathf.Max(0,data.dailyChestCredits);}}
        public static bool DailyGiftClaimed(int gift)=>DailyTasks.GiftClaimed(gift);
        public static bool ClaimDailyGift(int gift)
        {
            if(!DailyTasks.ClaimGift(gift))return false;
            int amount=WoolDailyTaskState.GiftCoins[gift];RecordDailyCoins(amount);data.coins+=amount;data.collectedCoins+=amount;Save();return true;
        }
        public static bool ClaimDailyChest()
        {
            if(DailyChestCredits<700)return false;
            data.dailyChestCredits-=700;data.dailyChestCollected=true;RecordDailyCoins(500);data.coins+=500;data.collectedCoins+=500;Save();return true;
        }
        public static void RecordDailyCoins(int amount){if(amount>0)DailyTasks.coins=(int)Math.Min(500L,(long)DailyTasks.coins+amount);}
        public static void RecordDailyUnlock(){RecordDailyAbility(0);}
        public static void RecordDailyAbility(int kind){var state=DailyTasks;switch(kind){case 0:state.unlocks=1;break;case 1:state.removes=1;break;case 2:state.sorts=1;break;case 3:state.amplifies=1;break;default:return;}Save();}
        public static bool HasDailyRewardToCollect {get{for(int i=0;i<DailyTitles.Length;i++)if(!DailyClaimed(i)&&DailyCount(i)>=DailyTargets[i])return true;for(int i=0;i<4;i++)if(!DailyGiftClaimed(i)&&DailyCredits>=(i+1)*25)return true;return DailyChestCredits>=700;}}
        public static void RecordDailySpinOpened(){DailyTasks.spins=1;Save();}
        public static void RecordDailyVideo(){DailyTasks.videos=1;Save();}
        static void RecordDailyCompletion(){DailyTasks.wins=Math.Min(2,DailyTasks.wins+1);}
#if UNITY_EDITOR
        public static string ValidateDailyTaskRules()
        {
            BeginPreview();int configured=totalLevels;
            try
            {
                data=NewCampaign(0);records.Clear();totalLevels=10;
                if(ClaimDailyTask(2)||ClaimDailyGift(0)||ClaimDailyChest())throw new Exception("Premature claim");
                var s=DailyTasks;s.wins=2;s.coins=500;s.unlocks=s.removes=s.sorts=s.amplifies=1;
                for(int i=0;i<DailyTitles.Length;i++)if(!ClaimDailyTask(i)||ClaimDailyTask(i))throw new Exception("Task single claim");
                if(DailyCredits!=100||DailyChestCredits!=100||Coins!=0)throw new Exception("Credit sum or coin separation");
                for(int i=0;i<4;i++)if(!ClaimDailyGift(i)||ClaimDailyGift(i))throw new Exception("Gift single claim");
                if(Coins!=200)throw new Exception("Gift rewards must sum to 200");
                data.dailyTasks.day="2000-01-01";
                if(DailyCredits!=0||DailyChestCredits!=100||DailyCount(2)!=0)throw new Exception("Daily reset preserves chest");
                data.dailyChestCredits=715;
                if(!ClaimDailyChest()||ClaimDailyChest()||Coins!=700||DailyChestCredits!=15)throw new Exception("Chest amount, overflow or duplicate");
                string raw=JsonUtility.ToJson(data);data=Read(raw);
                if(Coins!=700||DailyChestCredits!=15)throw new Exception("Persistence");
                data.dailyTasks.day="2999-01-01";data.dailyTasks.claims=255;
                if(DailyCredits!=100)throw new Exception("Clock rollback reset");
                var legacy=new WoolDailyTaskState{claims=(1<<1)|(1<<3)};legacy.Upgrade();if(legacy.Points!=30||!legacy.Claimed(1)||legacy.Claimed(3))throw new Exception("Six-task save migration");
                var boundary=new WoolDailyTaskState{version=2,wins=2,coins=500,unlocks=1,removes=1,sorts=1,amplifies=1};boundary.Claim(0);if(boundary.ClaimGift(0))throw new Exception("10 < 25 boundary");boundary.Claim(1);if(!boundary.ClaimGift(0)||boundary.ClaimGift(1))throw new Exception("25 boundary");
                return "PASS: premature/duplicate claims; 100 daily credits; gifts 20/30/50/100; 700 chest -> 500 coins; overflow; midnight reset; rollback; JSON reload; 25-credit boundary.";
            }
            finally{totalLevels=configured;EndPreview();}
        }
#endif
    }
}
