using TMPro;
using UnityEngine;

namespace WoolGame
{
    // Serialized scene references only. WoolGameController remains the game owner.
    public sealed partial class WoolSceneRig : MonoBehaviour
    {
        public Camera gameCamera;
        [Tooltip("Set from a verified VIP entitlement.")] public bool vipSlotEntitled;
        public WoolHud hud;
        public WoolAuthoredLayout authoredLayout;
        public WoolArrowPool arrowPool;
        public WoolDuckRescue duckRescue;
        public GameObject[] slotUnlockIcons=new GameObject[8];
        public Transform world,dragon,kitten,hand;
        public GroundSplinePath route;
        public Transform[] vehicles,yarn,slots;
        public TMP_Text[] counts;
        public Mesh[] vehicleMeshes;
        public Material[] vehicleMaterials,woolMaterials;
        [UnityEngine.Serialization.FormerlySerializedAs("spoolCoreMaterial")] public Material mechanismMaterial;
        public BoxCollider leftEdge,rightEdge,bottomEdge,topEdge;
        public ParticleSystem confetti;
        public AudioSource audioSource;
        public AudioClip departureSound,winSound;
        public WoolLevelDefinition[] levels;
        public Transform fog,entrancePortal;
        public GameObject tutorialEnvironment,standardEnvironment;
        public Vector2 worldBounds=new Vector2(9.7f,20);

        public static readonly Vector3 ParkingOffset = new Vector3(0, -1.15f, 0);

        public void SetVehicle(int index,WoolTile tile,float boardRotation=0,float boardScale=1)
        {
            var t=vehicles[index];t.gameObject.SetActive(true);
            SetViewLayer(t,"Vehicle View");
            var bounds=authoredLayout.vehiclePool.Show(index,tile,t);
            t.GetComponent<Renderer>().enabled=false;
            t.localScale=new Vector3(tile.width*boardScale/bounds.size.x,.7f/bounds.size.y,tile.length*boardScale/bounds.size.z);
            authoredLayout.vehiclePool.FitProportions(index,t.localScale);
            t.localPosition=ParkingPresentationOffset+ParkingOffset+new Vector3(0,-3.2f,0)+Quaternion.Euler(0,0,boardRotation)*(new Vector3(tile.x,tile.y+3.2f,0)*boardScale)+Vector3.back*.3f;
            t.localRotation=Quaternion.Euler(0,0,-(int)tile.direction*90+boardRotation)*Quaternion.Euler(-90,0,0);
            var collider=t.GetComponent<BoxCollider>();collider.center=Vector3.zero;collider.size=bounds.size;collider.enabled=true;
            var oldTrail=t.GetComponent<TrailRenderer>();if(oldTrail)
            {
                oldTrail.enabled=true;oldTrail.autodestruct=false;oldTrail.emitting=false;oldTrail.Clear();oldTrail.time=.28f;
                oldTrail.startWidth=tile.width*.65f*world.lossyScale.magnitude/Mathf.Sqrt(3);oldTrail.startWidth=Mathf.Abs(oldTrail.startWidth);oldTrail.endWidth=.015f;
                var color=authoredLayout.vehicleTrailColors[tile.color];oldTrail.startColor=new Color(color.r,color.g,color.b,.7f);oldTrail.endColor=new Color(color.r,color.g,color.b,0);
                oldTrail.minVertexDistance=.035f;oldTrail.numCornerVertices=4;oldTrail.numCapVertices=4;
            }
        }
        public void ResetPool()
        {
            ParkingPresentationOffset=Vector3.zero;ParkingTravelReady=false;
            if(authoredLayout)authoredLayout.vehiclePool.ResetPool();
            foreach(var t in vehicles){var trail=t.GetComponent<TrailRenderer>();if(trail){trail.emitting=false;trail.Clear();}t.gameObject.SetActive(false);}
            foreach(var t in yarn)t.gameObject.SetActive(false);
            if(authoredLayout&&authoredLayout.tailTip)authoredLayout.tailTip.gameObject.SetActive(false);
            if(confetti)confetti.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        public void ApplyLayout(bool tutorial)
        {
            for(int i=0;i<slots.Length;i++)counts[i].transform.position=slots[i].position+new Vector3(0,.3f,.6f);
        }
        public void FitCamera()
        {
            int width=gameCamera.targetTexture?gameCamera.targetTexture.width:Screen.width;
            int height=gameCamera.targetTexture?gameCamera.targetTexture.height:Screen.height;
            if(width<=0||height<=0)return;
            Rect safe=gameCamera.targetTexture?new Rect(0,0,width,height):WoolHud.CurrentSafeArea;
            FitPortraitCamera(width,height,safe);
        }
    }
}



