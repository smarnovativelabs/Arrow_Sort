using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace WoolGame.Editor
{
    public static class WoolPortraitValidation
    {
        static readonly int[] Devices={18,12,0,32,76};
        static readonly string[] Sizes={"750x1334","1170x2532","1536x2048","960x2658","2048x2732"};
        static readonly int[] Levels={1,2,5,8,10,11,16,21,40,71,110};
        static int[] levelSet=Levels;
        static readonly BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
        static readonly StringBuilder report=new StringBuilder();
        static WoolGameController game;static int current,stage,retries;static double since;static bool loaded;
        public static string Result="Idle";
        public static string Begin(bool quick=false)
        {
            if(!EditorApplication.isPlaying)throw new Exception("Enter Play Mode with isolated QA progress first.");
            game=UnityEngine.Object.FindFirstObjectByType<WoolGameController>();
            levelSet=quick?new[]{9,12,71}:Levels;
            WoolProgress.MarkRailTutorialSeen();WoolProgress.MarkWarehouseTutorialSeen();
            current=stage=retries=0;report.Clear();Result="Running";Application.runInBackground=true;
            EditorApplication.update-=Tick;EditorApplication.update+=Tick;
            return "Checking "+Devices.Length*levelSet.Length+" normal level/device combinations";
        }
        static IEnumerator LoadInPlayer(int level)
        {
            yield return null;
            game.automatedInput=true;
            try{game.LoadLevel(level);}finally{game.automatedInput=false;}
            loaded=true;
        }
        static void Tick()
        {
            try
            {
                if(!EditorApplication.isPlaying){Finish("Interrupted");return;}
                int device=current/levelSet.Length,level=levelSet[current%levelSet.Length];
                if(stage==0)
                {
                    WoolReleaseValidation.SetDevice(Devices[device]);game.enabled=false;Time.timeScale=0;
                    since=EditorApplication.timeSinceStartup;stage=1;EditorApplication.QueuePlayerLoopUpdate();return;
                }
                if(stage==1)
                {
                    if(EditorApplication.timeSinceStartup-since<.6)return;
                    loaded=false;game.enabled=true;Time.timeScale=1;game.StartCoroutine(LoadInPlayer(level));
                    stage=2;since=EditorApplication.timeSinceStartup;return;
                }
                if(!loaded||EditorApplication.timeSinceStartup-since<3.1){EditorApplication.QueuePlayerLoopUpdate();return;}
                Time.timeScale=0;game.enabled=false;
                string metrics=ValidateFrame();
                string capture=WoolReleaseValidation.Capture("release-device"+Devices[device]+"-level"+level);
                if(!capture.EndsWith(Sizes[device])){if(retries++<2){stage=0;return;}throw new Exception("Wrong simulator size: "+capture);}
                retries=0;
                report.AppendLine("PASS device="+Devices[device]+" level="+level+" "+metrics);
                Directory.CreateDirectory("Builds/QA");File.WriteAllText("Builds/QA/release-portrait-report.txt",report.ToString());
                current++;stage=0;
                if(current==Devices.Length*levelSet.Length)Finish("PASS "+Devices.Length*levelSet.Length+" normal level/device combinations");
            }
            catch(Exception e){Finish("FAIL "+e);Debug.LogException(e);}
        }
        public static string ValidateFrame()
        {
            var rig=game.rig;var definition=(WoolLevelDefinition)FGet("definition");
            int visible=0,clipped=0;float smallest=100000;
            for(int i=0;i<game.VehicleCount;i++)
            {
                var actor=rig.vehicles[i];if(!actor.gameObject.activeInHierarchy)continue;visible++;
                var b=actor.GetComponent<BoxCollider>().bounds;var min=Vector2.one*100000;var max=-min;bool outside=false;
                for(int c=0;c<8;c++)
                {
                    var p=rig.vehicleCamera.WorldToViewportPoint(b.center+Vector3.Scale(b.extents,new Vector3((c&1)==0?-1:1,(c&2)==0?-1:1,(c&4)==0?-1:1)));
                    min=Vector2.Min(min,p);max=Vector2.Max(max,p);outside|=p.x<0||p.x>1||p.y<0||p.y>1;
                }
                if(outside)clipped++;
                smallest=Mathf.Min(smallest,Mathf.Min((max.x-min.x)*rig.vehicleCamera.pixelWidth,(max.y-min.y)*rig.vehicleCamera.pixelHeight));
            }
            if(clipped!=0)throw new Exception("Clipped vehicles "+clipped+" at level "+game.CurrentLevel);
            foreach(var path in rig.authoredLayout.paths)if(path.road.activeSelf)
                foreach(var p in path.FramingPoints)
                {
                    var screen=rig.gameCamera.WorldToViewportPoint(path.road.transform.TransformPoint(p));
                    if(screen.x<0||screen.x>1||screen.y<0||screen.y>1)throw new Exception("Main road clipped at "+p+" => "+screen);
                }
            var spawn=rig.gameCamera.WorldToViewportPoint(rig.world.TransformPoint(rig.route.Position(.35f)));
            if(spawn.x>=0&&spawn.x<=1&&spawn.y>=-.15f&&spawn.y<=1.15f)throw new Exception("Spawn is onscreen: "+spawn);
            if(game.CurrentLevel<=10&&game.VehicleCount!=(game.CurrentLevel==1?6:22+(game.CurrentLevel-2)*3))throw new Exception("Opening count changed");
            var rules=new WoolLevel(definition);
            foreach(int i in rules.Solution){rules.Tiles[i].hidden=false;if(!rules.CanSelect(i))throw new Exception("Blocked solution");rules.Tiles[i].selected=true;}
            var banner=UnityEngine.Object.FindFirstObjectByType<WoolDifficultyAnnouncement>();
            if(banner&&banner.IsVisible)throw new Exception("Difficulty banner stayed open");
            if(UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Any(b=>b.name=="Switch Test Level"))throw new Exception("Test UI leaked into normal game");
            return "total="+game.VehicleCount+" visible="+visible+" minPixels="+smallest.ToString("F1")+" spawn="+spawn.ToString("F2")+" "+WoolReleaseValidation.LayoutMetrics();
        }
        static object FGet(string name)=>typeof(WoolGameController).GetField(name,F).GetValue(game);
        static void Finish(string result)
        {
            EditorApplication.update-=Tick;Time.timeScale=1;if(game)game.enabled=true;
            Result=result;report.AppendLine(result);Directory.CreateDirectory("Builds/QA");File.WriteAllText("Builds/QA/release-portrait-report.txt",report.ToString());
        }
    }
}
