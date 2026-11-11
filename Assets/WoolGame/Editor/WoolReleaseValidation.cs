using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace WoolGame.Editor {
public static class WoolReleaseValidation {
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
 static void StartPreview(){if(!SessionState.GetBool("WoolReleaseQA",false))return;SessionState.SetBool("WoolReleaseQA",false);Application.runInBackground=true;WoolProgress.Load(WoolEndlessGenerator.MaxLevel);WoolProgress.BeginPreview();WoolProgress.InitializeOnboarding();WoolProgress.AdvanceOnboarding(WoolOnboardingStep.Complete);WoolProgress.MarkGameplayRulesTutorialSeen();for(int i=0;i<4;i++)WoolProgress.CompleteAbilityLesson(i);}
 static readonly BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static readonly List<string> rows=new List<string>{"level,vehicles,requiredSlots,peakSlots,legalWitness,difficulty"};
 public static string ValidateLevels(int start,int end){
  var rig=UnityEngine.Object.FindFirstObjectByType<WoolSceneRig>();var route=rig.levels[1].route;
  for(int n=start;n<=end;n++){
   var d=WoolEndlessGenerator.Generate(n,route);try{
    int slotLimit=n>10?5:4;
    if(d.requiredSlots>slotLimit)throw new Exception("Exceeds designed slot limit at "+n);
    var rules=new WoolLevel(d);var active=new List<int>();var ammo=d.vehicles.Select(t=>WoolLevel.Capacity(t.size)).ToArray();int next=0,peak=0;
    foreach(int color in d.yarn){
     int best=-1;foreach(int i in active)if(d.vehicles[i].color==color&&(best<0||ammo[i]<ammo[best]))best=i;
     while(best<0){if(next>=d.tutorialOrder.Length)throw new Exception("No matching vehicle at "+n);
      int index=d.tutorialOrder[next++];var tile=rules.Tiles[index];tile.hidden=false;
      if(!rules.CanSelect(index))throw new Exception("Blocked witness level "+n+" vehicle "+index);
      tile.selected=true;active.Add(index);peak=Math.Max(peak,active.Count);if(peak>slotLimit)throw new Exception("Exceeds designed slot limit at level "+n);
      foreach(int i in active)if(d.vehicles[i].color==color&&(best<0||ammo[i]<ammo[best]))best=i;
     }
     if(--ammo[best]==0)active.Remove(best);
    }
    if(ammo.Any(a=>a!=0))throw new Exception("Ammo mismatch at "+n);
    var expected=n%11==0?WoolDifficulty.SuperHard:n%5==0?WoolDifficulty.Hard:n>=2?WoolDifficulty.Medium:WoolDifficulty.Easy;
    if(d.difficulty!=expected)throw new Exception("Difficulty schedule mismatch");
    rows.Add(n+","+d.vehicles.Length+","+d.requiredSlots+","+peak+",true,"+d.difficulty);
   }finally{UnityEngine.Object.DestroyImmediate(d);}
  }
  Directory.CreateDirectory("Builds/QA");File.WriteAllLines("Builds/QA/level-solvability.csv",rows);return "PASS levels "+start+"-"+end+": legal witness, balanced ammo, opening <=4 slots, challenges <=5 slots";
 }
 static EditorWindow simulatorWindow;
 public static object SimulatorMain(){var type=Type.GetType("UnityEditor.DeviceSimulation.SimulatorWindow,UnityEditor.DeviceSimulatorModule");if(!simulatorWindow)simulatorWindow=EditorWindow.GetWindow(type);simulatorWindow.Show();simulatorWindow.Focus();return type.GetField("m_Main",F).GetValue(simulatorWindow);}
 public static string Devices(){var main=SimulatorMain();var devices=(Array)main.GetType().GetProperty("devices",F).GetValue(main);return string.Join("\n",devices.Cast<UnityEngine.Object>().Select((d,i)=>i+":"+d.name));}
 public static string SetDevice(int index){var main=SimulatorMain();main.GetType().GetProperty("deviceIndex",F).SetValue(main,index);return "Simulator device "+index;}
 public static string Capture(string name){
  var main=SimulatorMain();var view=main.GetType().GetProperty("playModeView",F).GetValue(main);var type=view.GetType();FieldInfo field=null;while(type!=null&&field==null){field=type.GetField("m_TargetTexture",F);type=type.BaseType;}var rt=(RenderTexture)field.GetValue(view);if(!rt)throw new Exception("Simulator has no rendered frame");
  var old=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();if(SystemInfo.graphicsUVStartsAtTop){var pixels=image.GetPixels32();var flipped=new Color32[pixels.Length];for(int y=0;y<rt.height;y++)Array.Copy(pixels,y*rt.width,flipped,(rt.height-1-y)*rt.width,rt.width);image.SetPixels32(flipped);image.Apply();}RenderTexture.active=old;Directory.CreateDirectory("Builds/QA");var path="Builds/QA/"+name+".png";File.WriteAllBytes(path,image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);return Path.GetFullPath(path)+" "+rt.width+"x"+rt.height;
 }
 static int caseStage,caseLevel;static string caseName;static double caseAt;public static string CaseResult="Idle";
 public static string RunDeviceCase(int index,int level,string name){SetDevice(index);caseLevel=level;caseName=name;caseStage=1;caseAt=EditorApplication.timeSinceStartup;CaseResult="Running "+name;EditorApplication.update-=DeviceTick;EditorApplication.update+=DeviceTick;return CaseResult;}
 static void DeviceTick(){try{
  if(EditorApplication.timeSinceStartup-caseAt<1)return;
  if(caseStage==1){var g=UnityEngine.Object.FindFirstObjectByType<WoolGameController>();var auto=typeof(WoolGameController).GetProperty("automatedInput",F);bool previous=(bool)auto.GetValue(g);auto.SetValue(g,true);try{g.LoadLevel(caseLevel);}finally{auto.SetValue(g,previous);}if(g.CurrentLevel!=caseLevel)throw new Exception("Requested level did not load");SimulatorMain();caseStage=2;caseAt=EditorApplication.timeSinceStartup;return;}
  if(caseStage==2){Capture(caseName+"-banner");var game=UnityEngine.Object.FindFirstObjectByType<WoolGameController>();bool accepted=game.SelectTile(game.SolutionOrder[0]);File.AppendAllText("Builds/QA/device-checks.txt",caseName+" firstMove="+accepted+"\n");var metrics=LayoutMetrics();File.AppendAllText("Builds/QA/device-checks.txt",caseName+" "+metrics+"\n");caseStage=3;caseAt=EditorApplication.timeSinceStartup;return;}
  if(caseStage==3&&EditorApplication.timeSinceStartup-caseAt>2.5){Capture(caseName+"-game");var banner=UnityEngine.Object.FindFirstObjectByType<WoolDifficultyAnnouncement>();if(banner&&banner.IsVisible)throw new Exception("Banner did not disappear");CaseResult="PASS "+caseName;EditorApplication.update-=DeviceTick;}
 }catch(Exception e){CaseResult="FAIL "+caseName+": "+e.Message;EditorApplication.update-=DeviceTick;Debug.LogException(e);}}
 public static string LayoutMetrics(){var rig=UnityEngine.Object.FindFirstObjectByType<WoolSceneRig>();var cam=rig.SlotCamera;var r=rig.slots[0].GetComponent<Renderer>();if(!r)r=rig.slots[0].GetComponentInChildren<Renderer>();var b=r.bounds;float minX=1e10f,maxX=-1e10f,minY=1e10f,maxY=-1e10f;for(int i=0;i<8;i++){var p=cam.WorldToScreenPoint(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));minX=Mathf.Min(minX,p.x);maxX=Mathf.Max(maxX,p.x);minY=Mathf.Min(minY,p.y);maxY=Mathf.Max(maxY,p.y);}float ratio=(maxX-minX)/(maxY-minY);if(!float.IsFinite(ratio)||Mathf.Abs(ratio-1)>.08f)throw new Exception("Slot oval: "+ratio);return "Screen "+Screen.width+"x"+Screen.height+" safe="+Screen.safeArea+" slotAspect="+ratio+" rescue="+rig.gameCamera.rect;}
}
}
