using UnityEngine;
namespace WoolGame
{
    // Rotation is independent of responsive position and scale fitting.
    public sealed class WoolHeroMotion:MonoBehaviour
    {
        void Update(){transform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.unscaledTime*1.25f)*1.5f);}
        void OnDisable(){transform.localRotation=Quaternion.identity;}
    }
}
