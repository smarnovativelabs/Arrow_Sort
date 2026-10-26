using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WoolGame.Editor
{
    public static class WoolFantasyHomeValidation
    {
        const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
        static string playerLoopReport="No player-loop check requested.";
        public static string CheckAfterFrame(int top=0,int bottom=0,int left=0,int right=0)
        {
            playerLoopReport="Waiting for player frame.";
            UnityEngine.Object.FindFirstObjectByType<WoolHud>().StartCoroutine(CheckInPlayerLoop(top,bottom,left,right));
            return playerLoopReport;
        }
        static IEnumerator CheckInPlayerLoop(int top,int bottom,int left,int right)
        {
            yield return null;
            // GraphicRaycaster reads Screen dimensions; evaluate from the player loop, not an Editor GUI eval.
            playerLoopReport=Check(top,bottom,left,right);
        }
        public static string PlayerLoopReport()=>playerLoopReport;
        public static string SetResolution(int width,int height)
        {
            WoolHud.EditorSafeArea=null;
            var asm=typeof(UnityEditor.Editor).Assembly;
            var sizesType=asm.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes=singleton.GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null);
            var groupType=sizesType.GetProperty("currentGroupType",Flags).GetValue(sizes);
            var group=sizesType.GetMethod("GetGroup",Flags).Invoke(sizes,new[]{groupType});
            var sizeType=asm.GetType("UnityEditor.GameViewSize");
            var modeType=asm.GetType("UnityEditor.GameViewSizeType");
            string label="Fantasy home QA "+width+"x"+height;
            int builtin=(int)group.GetType().GetMethod("GetBuiltinCount",Flags).Invoke(group,null);
            int count=(int)group.GetType().GetMethod("GetCustomCount",Flags).Invoke(group,null);
            int index=-1;
            for(int i=0;i<count;i++)
            {
                var s=group.GetType().GetMethod("GetGameViewSize",Flags).Invoke(group,new object[]{builtin+i});
                if((int)sizeType.GetProperty("width",Flags).GetValue(s)==width&&(int)sizeType.GetProperty("height",Flags).GetValue(s)==height){index=builtin+i;break;}
            }
            if(index<0)
            {
                var ctor=sizeType.GetConstructor(Flags,null,new[]{modeType,typeof(int),typeof(int),typeof(string)},null);
                var size=ctor.Invoke(new[]{Enum.Parse(modeType,"FixedResolution"),(object)width,height,label});
                group.GetType().GetMethod("AddCustomSize",Flags).Invoke(group,new[]{size});index=builtin+count;
            }
            var playView=asm.GetType("UnityEditor.PlayModeView");
            var view=(EditorWindow)playView.GetMethod("GetMainPlayModeView",BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public).Invoke(null,null);
            var select=view.GetType().GetMethod("SizeSelectionCallback",Flags);
            select.Invoke(view,new object[]{index,null});
            view.Repaint();EditorApplication.QueuePlayerLoopUpdate();
            var selected=view.GetType().GetProperty("currentGameViewSize",Flags).GetValue(view);
            return "Game view requested "+width+"x"+height+" group="+groupType+" index="+index+" selected="+sizeType.GetProperty("width",Flags).GetValue(selected)+"x"+sizeType.GetProperty("height",Flags).GetValue(selected)+". Confirm render target on next call.";
        }
        public static string Check(int top=0,int bottom=0,int left=0,int right=0)
        {
            var target=TargetSize();int w=(int)target.x,h=(int)target.y;
            Rect safe=new Rect(left,bottom,w-left-right,h-top-bottom);WoolHud.SetEditorSafeArea(safe,w,h);
            var hud=UnityEngine.Object.FindFirstObjectByType<WoolHud>();
            hud.ApplyPortraitFrame(w,h,safe);Canvas.ForceUpdateCanvases();
            var home=hud.GetComponentInChildren<WoolHomePresentation>(true);home.Fit();Canvas.ForceUpdateCanvases();
            var failures=new List<string>();var bounds=new List<string>();
            var corners=new Vector3[4];
            foreach(var b in home.GetComponentsInChildren<Button>())
            {
                var rt=(RectTransform)b.transform;rt.GetWorldCorners(corners);
                var min=RectTransformUtility.WorldToScreenPoint(null,corners[0]);var max=RectTransformUtility.WorldToScreenPoint(null,corners[2]);
                if(min.x<safe.xMin-1||max.x>safe.xMax+1||min.y<safe.yMin-1||max.y>safe.yMax+1)failures.Add(b.name+" outside safe area");
                var data=new PointerEventData(EventSystem.current){position=(min+max)*.5f};
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
                var hit=hits.Count>0?hits[0].gameObject.GetComponentInParent<Button>():null;
                if(hit!=b)failures.Add(b.name+" touch intercepted by "+(hit?hit.name:"none"));
                bounds.Add(b.name+": "+min+" - "+max);
            }
            var level=home.levelName.GetComponent<TMPro.TMP_Text>();
            if(string.IsNullOrEmpty(level.text)||level.textInfo.characterCount==0)failures.Add("Level text empty");
            home.dragon.GetWorldCorners(corners);
            float dragonTop=corners.Max(c=>RectTransformUtility.WorldToScreenPoint(null,c).y);
            var settings=(RectTransform)home.topBar.Find("Settings");settings.GetWorldCorners(corners);
            float settingsBottom=corners.Min(c=>RectTransformUtility.WorldToScreenPoint(null,c).y);
            if(dragonTop>settingsBottom-4)failures.Add("Dragon overlaps settings reserved area");
            bounds.Add("Dragon/header gap: "+(settingsBottom-dragonTop).ToString("F2")+" px");
            bounds.Add("Duck pose: integrated sprite; separate wings active="+((home.leftWing&&home.leftWing.gameObject.activeSelf)||(home.rightWing&&home.rightWing.gameObject.activeSelf)));
            if((home.leftWing&&home.leftWing.gameObject.activeSelf)||(home.rightWing&&home.rightWing.gameObject.activeSelf))failures.Add("Legacy wing still active");
            bounds.Add("Level font: "+level.font.name+" "+level.font.atlasWidth+"x"+level.font.atlasHeight);
            string report=w+"x"+h+" safe="+safe+" buttons="+home.GetComponentsInChildren<Button>().Length+" frame="+Time.frameCount+" animation="+home.AnimationTime+" failures="+failures.Count+"\n"+string.Join("\n",failures.Concat(bounds));
            Directory.CreateDirectory("Docs/FantasyHome20261021");File.WriteAllText("Docs/FantasyHome20261021/"+w+"x"+h+"-safe-"+top+"-"+bottom+".txt",report);
            return report;
        }
        public static string CheckInteractions()
        {
            var hud=UnityEngine.Object.FindFirstObjectByType<WoolHud>();
            var home=hud.GetComponentInChildren<WoolHomePresentation>(true);
            string[] names={"Profile","Settings","Tasks","Lucky Spin","Rescue Shop","Ad Pass","Shop","Map","Home"};
            var results=new List<string>();
            foreach(string name in names)
            {
                hud.Hide();hud.Home(WoolProgress.CurrentLevel,WoolProgress.Coins);
                var button=home.GetComponentsInChildren<Button>(true).First(b=>b.name==name);
                button.onClick.Invoke();
                bool passed=false;
                if(name=="Settings")passed=hud.ModalVisible;
                else if(name=="Ad Pass")passed=hud.ModalVisible||hud.GetComponentsInChildren<RectTransform>(true).Any(t=>t.name=="Shop"&&t.gameObject.activeInHierarchy);
                else if(name=="Home")passed=home.gameObject.activeInHierarchy;
                else
                {
                    string target=name=="Profile"?"Profile":name=="Tasks"?"Daily tasks":name=="Lucky Spin"?"Daily spin":name=="Map"?"Map":"Shop";
                    passed=hud.GetComponentsInChildren<RectTransform>(true).Any(t=>t.name==target&&t.gameObject.activeInHierarchy);
                }
                results.Add(name+": "+(passed?"PASS":"CHECK TARGET"));
            }
            hud.Hide();hud.Home(WoolProgress.CurrentLevel,WoolProgress.Coins);
            string report=string.Join("\n",results);File.WriteAllText("Docs/FantasyHome20261021/interactions.txt",report);return report;
        }
        public static string Restore()
        {WoolHud.EditorSafeArea=null;return "Physical safe area restored on the next player frame.";}
        static Vector2 TargetSize()
        {
            // Screen.width in an Editor eval can describe the Inspector, not the player target.
            var type=typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.PlayModeView");
            return (Vector2)type.GetMethod("GetMainPlayModeViewTargetSize",BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public).Invoke(null,null);
        }
        public static string PreviewSafeArea(int top,int bottom,int left=0,int right=0)
        {
            var size=TargetSize();WoolHud.SetEditorSafeArea(new Rect(left,bottom,size.x-left-right,size.y-top-bottom),(int)size.x,(int)size.y);
            return "Preview safe area set for target "+size+". Inspect after the next player frame.";
        }
    }
}
