using System;
using UnityEngine;

namespace WoolGame
{
    public enum WoolDifficulty { Easy, Medium, Hard }

    [CreateAssetMenu(menuName="Wool Crush/Level",fileName="Level")]
    public sealed class WoolLevelDefinition : ScriptableObject
    {
        [Min(1)] public int number=1;
        [Tooltip("Saved by the visual level editor. Campaign generation preserves this asset.")] public bool manuallyAuthored;
        public WoolEnvironmentDefinition environment;
        public WoolDifficulty difficulty;   
        public string displayName;
        public string chapterName;
        public string layoutStyle;
        [TextArea] public string difficultyNote;
        [Min(0)] public float startingGraceSeconds=3;
        [Range(.1f,.8f)] public float initialHeadProgress=.27f;
        [Min(1)] public int colorCount=3;
        [Min(1)] public int initialLegalMoves=1;
        public WoolTile[] vehicles=Array.Empty<WoolTile>();
        public int[] yarn=Array.Empty<int>();
        public int[] tutorialOrder=Array.Empty<int>();
        public Vector3[] route=Array.Empty<Vector3>();
        [HideInInspector] public int openSlots=4;
        public float boardRotation;
        [Range(.4f,1)] public float boardScale=1;
        public int snakeBatchSize=1;
        [Min(.01f)] public float advanceSpeed=.25f;
        [Min(.01f)] public float windingRate=5;
        public float fogAbove=4.5f;
        public bool tutorial;
        public bool hasRail;
        public Vector2[] containerPositions=Array.Empty<Vector2>();
        public int generatorRevision;
        [TextArea] public string evidence;
    }
}
