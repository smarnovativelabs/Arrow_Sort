using UnityEngine;
namespace WoolGame
{
    public static partial class WoolProgress
    {
        public static int MapDestinationLevel
        {
            get
            {
                return CurrentLevel;
            }
        }
        public static int MapPresentedLevel {get{EnsureLoaded();return data.mapPresentationKnown?Mathf.Clamp(data.mapPresentedLevel,1,totalLevels):MapDestinationLevel;}}
        public static void MarkMapPresented(int level)
        {
            EnsureLoaded();data.mapPresentedLevel=Mathf.Clamp(level,1,totalLevels);data.mapPresentationKnown=true;Save();
        }
    }
}
