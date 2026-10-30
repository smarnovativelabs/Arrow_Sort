using System;
using UnityEngine;
namespace WoolGame
{
    [Serializable] public sealed class WoolAuthoredPath
    {
        public GameObject road;
        // Centerline in the road's own coordinates: moving its GameObject moves the route too.
        public Vector3[] points;
        public float entryTailLength;
        public System.Collections.Generic.IEnumerable<Vector3> FramingPoints
        { get { for(int i=entryTailLength>0?1:0;i<points.Length;i++)yield return points[i]; } }
    }
    [Serializable] public sealed class WoolAuthoredSlot
    {
        public Transform slot;
        public ParticleSystem arrival;
        public Transform[] colors;
        public Transform[] arrows;
        public WoolBowVisual[] visuals;
    }
    [Serializable] public sealed class WoolBowVisual
    {
        public Transform frame;
        public LineRenderer bowstring;
        public TrailRenderer trail;
        public Vector3 loadedPosition;
        public Vector3 loadedScale=Vector3.one;
        public Quaternion restRotation=Quaternion.identity;
        public GameObject animationRoot;
    }
    public sealed class WoolAuthoredLayout : MonoBehaviour
    {
        public GameObject environment;
        public WoolAuthoredPath[] paths;
        public WoolAuthoredSlot[] slots;
        public Transform fogBoundary;
        public float cameraSize=18;
        public WoolPrefabVehicles vehiclePool;
        public GameObject railPrefab,tunnelPrefab;
        public Transform tailTip;
        public AnimationClip bowAttack;
        public Color[] vehicleTrailColors=(Color[])WoolArt.Palette.Clone();
        public bool IsInFog(Vector3 position)=>fogBoundary&&position.z<=fogBoundary.position.z;

        public void SelectPath(int number,GroundSplinePath route,Transform gameplayWorld,WoolDifficulty difficulty=WoolDifficulty.Easy)
        {
            var ranked=new int[paths.Length];var lengths=new float[paths.Length];
            for(int i=0;i<paths.Length;i++)
            {
                ranked[i]=i;
                for(int n=1;n<paths[i].points.Length;n++)lengths[i]+=Vector3.Distance(paths[i].road.transform.TransformPoint(paths[i].points[n-1]),paths[i].road.transform.TransformPoint(paths[i].points[n]));
            }
            Array.Sort(ranked,(a,b)=>lengths[a].CompareTo(lengths[b]));
            int shortCount=Mathf.Max(1,paths.Length/3);
            int selected;
            if(number<=8)selected=difficulty==WoolDifficulty.Hard?ranked[(number/5)%shortCount]:ranked[shortCount+(number-1)%(paths.Length-shortCount)];
            else {
                // Visit every authored path equally, alternating shorter and longer routes.
                // Shift each cycle so hard levels are not tied to one route-length group.
                int offset=number-9,cycle=offset/paths.Length;
                int turn=(offset%paths.Length+cycle)%paths.Length;
                int rank=turn%2==0?turn/2:paths.Length-1-turn/2;
                selected=ranked[rank];
            }
            for(int i=0;i<paths.Length;i++)paths[i].road.SetActive(i==selected);
            var path=paths[selected];var points=new Vector3[path.points.Length];var distances=new float[points.Length];
            for(int i=0;i<points.Length;i++)
            {
                points[i]=gameplayWorld.InverseTransformPoint(path.road.transform.TransformPoint(path.points[i]));
                if(i>0)distances[i]=distances[i-1]+Vector3.Distance(points[i-1],points[i]);
            }
            route.UseBaked(points,distances,path.entryTailLength>0?Vector3.Distance(points[0],points[1]):0);
        }
    }
}
