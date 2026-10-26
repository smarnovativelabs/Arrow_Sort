using System;
namespace WoolGame {
[Serializable] public sealed class WoolDailyTaskState {
 public string day;
 public int version,creditOffset,wins,coins,unlocks,removes,sorts,amplifies,spins,videos,claims,gifts;
 public static readonly string[] Titles={"Login daily","Collect 500 coins","Clear 2 levels today","Rescue duck family 1 time","Use \"Extra Slot\" 1 time","Use \"Remove Vehicle\" 1 time","Use \"Rearrange Colors\" 1 time","Use \"Amplify\" 1 time"};
 public static readonly int[] Targets={1,500,2,1,1,1,1,1};
 public static readonly int[] Credits={10,15,15,20,10,10,10,10};
 public static readonly int[] GiftCoins={20,30,50,100};
 public void Upgrade(){if(version>=2)return;int old=0;int[] legacy={15,20,15,10,25,15};for(int i=0;i<6;i++)if((claims&(1<<i))!=0)old+=legacy[i];int mask=0;if((claims&1)!=0)mask|=1<<2;if((claims&2)!=0)mask|=1<<1;if((claims&4)!=0)mask|=1<<4;if((claims&16)!=0)mask|=1<<3;claims=mask;version=2;int mapped=0;for(int i=0;i<8;i++)if(Claimed(i))mapped+=Credits[i];creditOffset=old-mapped;}
 public int Count(int task){switch(task){case 0:return 1;case 1:return coins;case 2:return wins;case 3:return wins;case 4:return unlocks;case 5:return removes;case 6:return sorts;case 7:return amplifies;default:return 0;}}
 public bool Claimed(int task)=>task>=0&&task<Titles.Length&&(claims&(1<<task))!=0;
 public int Points {get{int n=creditOffset;for(int i=0;i<Titles.Length;i++)if(Claimed(i))n+=Credits[i];return Math.Max(0,Math.Min(100,n));}}
 public bool Claim(int task){if(task<0||task>=Titles.Length||Claimed(task)||Count(task)<Targets[task])return false;claims|=1<<task;return true;}
 public bool GiftClaimed(int gift)=>gift>=0&&gift<4&&(gifts&(1<<gift))!=0;
 public bool ClaimGift(int gift){if(gift<0||gift>=4||GiftClaimed(gift)||Points<(gift+1)*25)return false;gifts|=1<<gift;return true;}
}}
