from pathlib import Path
p=Path('Docs/DailyTasks20261022/WoolProgress.Tasks.cs');s=p.read_text(encoding='utf-8')
s=s.replace('new WoolDailyTaskState{day=today}','new WoolDailyTaskState{day=today,version=2}').replace('return data.dailyTasks;','data.dailyTasks.Upgrade();return data.dailyTasks;')
s=s.replace('public static void RecordDailyUnlock(){DailyTasks.unlocks=1;Save();}','public static void RecordDailyUnlock(){RecordDailyAbility(0);}\n        public static void RecordDailyAbility(int kind){var state=DailyTasks;switch(kind){case 0:state.unlocks=1;break;case 1:state.removes=1;break;case 2:state.sorts=1;break;case 3:state.amplifies=1;break;default:return;}Save();}\n        public static bool HasDailyRewardToCollect {get{for(int i=0;i<DailyTitles.Length;i++)if(!DailyClaimed(i)&&DailyCount(i)>=DailyTargets[i])return true;for(int i=0;i<4;i++)if(!DailyGiftClaimed(i)&&DailyCredits>=(i+1)*25)return true;return DailyChestCredits>=700;}}')
s=s.replace('if(ClaimDailyTask(0)||ClaimDailyGift(0)||ClaimDailyChest())','if(ClaimDailyTask(2)||ClaimDailyGift(0)||ClaimDailyChest())').replace('s.unlocks=s.spins=s.videos=1;','s.unlocks=s.removes=s.sorts=s.amplifies=1;').replace('for(int i=0;i<6;i++)','for(int i=0;i<DailyTitles.Length;i++)').replace('DailyCount(0)!=0','DailyCount(2)!=0').replace('data.dailyTasks.claims=63','data.dailyTasks.claims=255')
a=s.index('                var boundary=');b=s.index('                return "PASS:',a)
s=s[:a]+'''                var legacy=new WoolDailyTaskState{claims=(1<<1)|(1<<3)};legacy.Upgrade();if(legacy.Points!=30||!legacy.Claimed(1)||legacy.Claimed(3))throw new Exception("Six-task save migration");
                var boundary=new WoolDailyTaskState{version=2,wins=2,coins=500,unlocks=1,removes=1,sorts=1,amplifies=1};boundary.Claim(0);if(boundary.ClaimGift(0))throw new Exception("10 < 25 boundary");boundary.Claim(1);if(!boundary.ClaimGift(0)||boundary.ClaimGift(1))throw new Exception("25 boundary");
'''+s[b:]
p.write_text(s,encoding='utf-8')
p=Path('Docs/DailyTasks20261022/WoolDailyTasks.cs');s=p.read_text(encoding='utf-8')
s=s.replace('public Image duckImage;','public Image fullBackground;\n        public Image duckImage;').replace('Action closeAction,spinAction,playAction,walletChanged;','Action closeAction,spinAction,playAction,walletChanged,shopAction;').replace('Action play,Action wallet)','Action play,Action wallet,Action openShop=null)').replace('walletChanged=wallet;','walletChanged=wallet;shopAction=openShop;')
s=s.replace('for(int i=0;i<6;i++)','for(int i=0;i<rows.Length;i++)').replace('new Vector2[6]','new Vector2[rows.Length]')
s=s.replace('if(task==3){spinAction?.Invoke();return;}\n            if(task==5){WatchVideo();return;}','if(task==1&&shopAction!=null){shopAction();return;}')
s=s.replace('int amount=WoolProgress.DailyRewards[task];','int amount=WoolProgress.DailyCredits-before;')
s=s.replace('card.localScale=Vector3.one*Mathf.Min((size.x-24)/941f,(size.y-28)/1672f);lastSize=size;','''card.localScale=Vector3.one*Mathf.Min((size.x-24)/941f,(size.y-28)/1672f);lastSize=size;
            if(fullBackground){var canvas=(RectTransform)GetComponentInParent<Canvas>().transform;fullBackground.rectTransform.position=canvas.TransformPoint(canvas.rect.center);fullBackground.rectTransform.sizeDelta=canvas.rect.size;fullBackground.rectTransform.localScale=Vector3.one;}''')
p.write_text(s,encoding='utf-8')
