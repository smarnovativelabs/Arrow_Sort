using UnityEngine;
namespace WoolGame
{
    [CreateAssetMenu(menuName="Wool Crush/Dragon Body Art",fileName="DragonBodyArt")]
    public sealed class WoolModelSet : ScriptableObject
    {
        public Mesh[] tailMeshes;
        public Material tailMaterial;
        [Tooltip("Mouth position relative to the head movement pivot.")]
        public Vector3 mouthOffset=new Vector3(.65f,0,-.3f);
    }
}
