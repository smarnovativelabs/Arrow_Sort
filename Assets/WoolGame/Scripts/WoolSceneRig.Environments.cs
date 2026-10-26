using UnityEngine;
namespace WoolGame
{
    public sealed partial class WoolSceneRig
    {
        [Tooltip("Automatic environment rotation for levels without an explicit environment. Empty uses the original scene art.")]
        public WoolEnvironmentDefinition[] environments;
        [Min(1)] public int levelsPerEnvironment=10;
        GameObject environmentInstance;WoolEnvironmentDefinition currentEnvironment;
        public GameObject ActiveEnvironment {get;private set;}
        public void ApplyEnvironment(WoolLevelDefinition level)
        {
            if(authoredLayout){authoredLayout.environment.SetActive(true);ActiveEnvironment=authoredLayout.environment;return;}
            var chosen=level.environment;
            if(!chosen&&!level.tutorial&&environments!=null&&environments.Length>0)
                chosen=environments[((level.number-1)/Mathf.Max(1,levelsPerEnvironment))%environments.Length];
            if(chosen!=currentEnvironment)
            {
                if(environmentInstance){environmentInstance.SetActive(false);Destroy(environmentInstance);environmentInstance=null;}
                currentEnvironment=chosen;
                if(chosen&&chosen.prefab)environmentInstance=Instantiate(chosen.prefab,world);
            }
            if(tutorialEnvironment)tutorialEnvironment.SetActive(!environmentInstance&&level.tutorial);
            if(standardEnvironment)standardEnvironment.SetActive(!environmentInstance&&!level.tutorial);
            ActiveEnvironment=environmentInstance?environmentInstance:level.tutorial?tutorialEnvironment:standardEnvironment;
            if(chosen)gameCamera.backgroundColor=chosen.cameraBackground;
        }
    }
}
