using System;
namespace WoolGame
{
    [Serializable]
    public sealed class WoolDailyPlayState
    {
        public long utcDay=-1;
        public int count,pending;
        public int Count(long utc)=>utc>0&&utc/86400==utcDay?count:0;
        public void Record(long utc){if(utc<=0){pending++;return;}Resolve(utc);count++;}
        public void Resolve(long utc){if(utc<=0)return;long day=utc/86400;if(day!=utcDay){utcDay=day;count=0;}count+=pending;pending=0;}
    }
}
