using System;
using UnityEngine;
namespace WoolGame
{
    // Versioned, value-only resume data. Old campaigns simply have no checkpoint.
    [Serializable]
    public sealed class WoolTutorialCheckpoint
    {
        public int version=1,level=1,layoutHash,step,total,waitingShot=-1,sequence,unlockedMask=15,sortCount;
        public float head,elapsed,feedbackRemaining,grace,amplifyRemaining;
        public int[] colors,bowTiles,bowInitial,bowOrder;
        public float[] amounts,bowAmmo;
        public bool[] selected;
        public bool Valid(int hash,int tiles,int segments,int slots)
        {
            if(version!=1||level<1||layoutHash!=hash||step<0||step>tiles||total!=segments||
                colors==null||amounts==null||colors.Length!=amounts.Length||colors.Length>segments||
                selected==null||selected.Length!=tiles||bowTiles==null||bowTiles.Length!=slots||
                bowAmmo==null||bowAmmo.Length!=slots||bowInitial==null||bowInitial.Length!=slots||
                bowOrder==null||bowOrder.Length!=slots||waitingShot< -1||waitingShot>=slots||
                !Finite(head)||!Finite(elapsed)||!Finite(feedbackRemaining)||!Finite(grace)||!Finite(amplifyRemaining)||head<0||elapsed<0||sequence<0||feedbackRemaining<0||amplifyRemaining<0||sortCount<0||unlockedMask<15||unlockedMask>255||(unlockedMask&15)!=15)return false;
            int picked=0;foreach(bool value in selected)if(value)picked++;
            if(level==1&&picked!=step)return false;
            for(int i=0;i<colors.Length;i++)if(colors[i]<0||colors[i]>=10||!Finite(amounts[i])||amounts[i]<=0||amounts[i]>1)return false;
            var seen=new bool[tiles];
            for(int i=0;i<slots;i++)
            {
                int tile=bowTiles[i];if(tile== -1)continue;
                if(tile<0||tile>=tiles||seen[tile]||!selected[tile]||!Finite(bowAmmo[i])||bowAmmo[i]<=0||bowAmmo[i]>bowInitial[i]||(unlockedMask&(1<<i))==0)return false;
                seen[tile]=true;
            }
            return waitingShot<0||bowTiles[waitingShot]>=0;
        }
        static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
    }
    public static partial class WoolProgress
    {
        public static WoolTutorialCheckpoint TutorialCheckpoint
        {
            get { EnsureLoaded();return !data.hasTutorialCheckpoint||data.tutorialCheckpoint==null?null:JsonUtility.FromJson<WoolTutorialCheckpoint>(JsonUtility.ToJson(data.tutorialCheckpoint)); }
        }
        public static void SaveTutorialCheckpoint(WoolTutorialCheckpoint checkpoint)
        {
            EnsureLoaded();if(checkpoint==null){ClearTutorialCheckpoint();return;}if(HasCompleted(checkpoint.level))return;
            data.hasTutorialCheckpoint=true;data.tutorialCheckpoint=JsonUtility.FromJson<WoolTutorialCheckpoint>(JsonUtility.ToJson(checkpoint));Save();
        }
        public static void ClearTutorialCheckpoint(){EnsureLoaded();if(!data.hasTutorialCheckpoint)return;data.hasTutorialCheckpoint=false;data.tutorialCheckpoint=null;Save();}
        public static bool RemovalLessonTargetPending{get{EnsureLoaded();return data.removalLessonTargetPending;}}
        public static void SetRemovalLessonTargetPending(bool pending){EnsureLoaded();data.removalLessonTargetPending=pending;Save();}
    }
}
