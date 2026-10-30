using UnityEngine;
namespace WoolGame
{
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    [AddComponentMenu("Arrow Sort/Test Build Options")]
    public sealed class WoolTestBuildOptions : MonoBehaviour
    {
        [Tooltip("Set before building: ON unlocks every level in this build. OFF uses normal saved progression. This option is not shown inside the game.")]
        public bool unlockAllLevelsInBuild;
        static WoolTestBuildOptions active;
        public static bool AllLevelsUnlocked=>active&&active.isActiveAndEnabled&&active.unlockAllLevelsInBuild;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSession(){active=null;}
        void OnEnable(){active=this;}
        void OnDisable(){if(active==this)active=null;}
    }
}