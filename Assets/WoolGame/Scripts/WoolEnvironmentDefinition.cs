using UnityEngine;
namespace WoolGame
{
    [CreateAssetMenu(menuName="Wool Crush/Environment",fileName="Environment")]
    public sealed class WoolEnvironmentDefinition : ScriptableObject
    {
        [Tooltip("Art prefab in board coordinates. Keep gameplay vehicles, slots and snake out of this prefab.")]
        public GameObject prefab;
        public Color cameraBackground=new Color(.14f,.25f,.31f);
    }
    // Optional binding on an environment prefab, so road selection is explicit.
}
