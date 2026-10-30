using UnityEngine;
namespace WoolGame
{
    // Direct references keep the original Sounds folder and its asset GUIDs intact.
    public sealed class WoolSoundLibrary : ScriptableObject
    {
        public AudioClip background, bonus, fire, coin, end, spin, win;
        [Header("Pixabay presentation audio")]
        public AudioClip menuMusic, uiClick, victory, defeat;
        public AudioClip unlockSlot, removeVehicle, sortColors, pierceFog;
        [Header("Rescue gameplay feedback")]
        public AudioClip arrowShot, arrowImpact, vehicleMove, vehicleDock, blocked, levelStart;
        public AudioClip duckQuack;
    }
}
