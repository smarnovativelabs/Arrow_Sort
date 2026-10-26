using System;
namespace WoolGame
{
    public static partial class WoolProgress
    {
        public const int MaxPlayerNameLength=15;
        static string CreateUserId()=>BitConverter.ToUInt64(Guid.NewGuid().ToByteArray(),0).ToString("D20",System.Globalization.CultureInfo.InvariantCulture);
        public static string UserId {get{EnsureLoaded();return data.userId;}}
        public static string PlayerName {get{EnsureLoaded();return string.IsNullOrWhiteSpace(data.playerName)?data.userId:data.playerName;}}
        public static int AvatarIndex {get{EnsureLoaded();return Math.Max(0,Math.Min(5,data.avatarIndex));}}
        public static long CoinsCollected {get{EnsureLoaded();return data.collectedCoins;}}
        public static int TodayPlayCount {get{ResolveProfilePlays();return data.profilePlays.Count(WoolTrustedClock.UnixNow);}}
        public static int TodayCompletedCount {get{ResolveProfilePlays();return data.profileCompletions.Count(WoolTrustedClock.UnixNow);}}
        public static void SetProfile(string name,int avatar)
        {
            EnsureLoaded();name=(name??"").Trim();
            name=System.Text.RegularExpressions.Regex.Replace(name,@"[\p{C}<>]","");
            if(name.Length>MaxPlayerNameLength)name=name.Substring(0,MaxPlayerNameLength);
            data.playerName=string.IsNullOrWhiteSpace(name)?data.userId:name;data.avatarIndex=Math.Max(0,Math.Min(5,avatar));Save();
        }
        public static void ResolveProfilePlays()
        {
            EnsureLoaded();if(data.profilePlays==null)data.profilePlays=new WoolDailyPlayState();
            if(data.profileCompletions==null)data.profileCompletions=new WoolDailyPlayState();
            if(WoolTrustedClock.Ready&&(data.profilePlays.pending>0||data.profileCompletions.pending>0)){data.profilePlays.Resolve(WoolTrustedClock.UnixNow);data.profileCompletions.Resolve(WoolTrustedClock.UnixNow);Save();}
        }
        static void RecordProfilePlay()
        {
            if(data.profilePlays==null)data.profilePlays=new WoolDailyPlayState();
            data.profilePlays.Record(WoolTrustedClock.UnixNow);
        }
        static void RecordProfileCompletion()
        {
            if(data.profileCompletions==null)data.profileCompletions=new WoolDailyPlayState();
            data.profileCompletions.Record(WoolTrustedClock.UnixNow);
        }
#if UNITY_EDITOR
        public static string ValidateProfileLifetimeRules()
        {
            int configured=totalLevels;BeginPreview();
            try
            {
                data=NewCampaign(0);records.Clear();totalLevels=10;
                string id=UserId;
                if(id.Length!=20||!System.Linq.Enumerable.All(id,char.IsDigit)||PlayerName!=id)throw new Exception("Numeric ID/default name");
                SetProfile("Duck Captain",3);if(UserId!=id||PlayerName!="Duck Captain")throw new Exception("Stable ID with editable name");
                BeginLevel(1);CompleteLevel(1);CompleteLevel(1);
                if(data.profileCompletions.count+data.profileCompletions.pending!=1)throw new Exception("One completion per attempt");
                BeginLevel(1);CompleteLevel(1);
                if(data.profileCompletions.count+data.profileCompletions.pending!=2)throw new Exception("Completed replay counts today");
                if(CoinsCollected!=50)throw new Exception("Replay must not award duplicate coins");
                SetCoins(10);if(CoinsCollected!=50)throw new Exception("Spending must not reduce lifetime coins");
                SetCoins(30);if(CoinsCollected!=70)throw new Exception("Income accumulates in lifetime coins");
                SetProfile("",0);if(PlayerName!=id)throw new Exception("Empty name defaults to user ID");
                return "PASS: numeric user ID/default name, stable ID, completed levels/replays counted once, lifetime earnings survive spending, no duplicate coin grants.";
            }
            finally{EndPreview();totalLevels=configured;}
        }
#endif
    }
}
