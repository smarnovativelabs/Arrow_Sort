using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace WoolGame.Editor
{
    public sealed class WoolLevelEditorWindow : EditorWindow
    {
        [SerializeField] WoolLevelDefinition asset,draft;
        [SerializeField] int selectedVehicle=-1,selectedPoint=-1,mode,zoom,requestedLevel=51;
        [SerializeField] float headProgress=.55f,snap=.1f;
        [SerializeField] bool balanceOnVehicleEdit=true;
        [SerializeField] Vector2 scroll;
        WoolLevelDefinition[] levels=Array.Empty<WoolLevelDefinition>();
        WoolSceneRig rig;
        WoolLevelArtPreview art;
        bool artDirty=true,dragging;
        Vector2 dragOffset;
        string status="Choose a level. Drag boxes or route points directly in the preview.";
        List<string> problems=new List<string>();
        const string Recovery="Library/WoolLevelEditorDraft.json";
        [Serializable] class RecoveryData {public string guid,json;}
        [MenuItem("Wool Crush/Level Editor")]
        public static void Open(){GetWindow<WoolLevelEditorWindow>("Wool Level Editor").Show();}
        public static void OpenLevel(WoolLevelDefinition level)
        {var window=GetWindow<WoolLevelEditorWindow>("Wool Level Editor");window.Show();window.SwitchTo(level);}
        [OnOpenAsset(1)] static bool OnAsset(int id,int line)
        {var level=Resources.FindObjectsOfTypeAll<WoolLevelDefinition>().FirstOrDefault(item=>item.GetInstanceID()==id);if(!level)return false;OpenLevel(level);return true;}
        void OnEnable()
        {
            minSize=new Vector2(850,500);saveChangesMessage="Save your manual level changes? Invalid drafts are kept in Library/WoolLevelEditorDraft.json.";
            Undo.undoRedoPerformed+=OnUndo;EditorApplication.playModeStateChanged+=OnPlayMode;
            FindLevels();RestoreDraft();artDirty=true;
        }
        void OnDisable(){StashDraft();Undo.undoRedoPerformed-=OnUndo;EditorApplication.playModeStateChanged-=OnPlayMode;DisposeArt();}
        void OnDestroy(){if(draft)DestroyImmediate(draft);}
        void OnPlayMode(PlayModeStateChange state){DisposeArt();artDirty=true;Repaint();}
        void DisposeArt(){art?.Dispose();art=null;}
        void OnUndo(){if(draft&&asset){hasUnsavedChanges=JsonUtility.ToJson(draft)!=JsonUtility.ToJson(asset);artDirty=true;ValidateDraft();StashDraft();Repaint();}}
        void FindLevels()
        {
            rig=UnityEngine.Object.FindFirstObjectByType<WoolSceneRig>();
            levels=rig&&rig.levels!=null?rig.levels.Where(x=>x).ToArray():AssetDatabase.FindAssets("t:WoolLevelDefinition").Select(g=>AssetDatabase.LoadAssetAtPath<WoolLevelDefinition>(AssetDatabase.GUIDToAssetPath(g))).Where(x=>x).OrderBy(x=>x.number).ToArray();
        }
        public void SwitchTo(WoolLevelDefinition next)
        {
            if(!next||next==asset&&draft)return;
            if(hasUnsavedChanges)
            {
                int choice=EditorUtility.DisplayDialogComplex("Unsaved level","Save changes to "+asset.name+" before switching?","Save","Cancel","Discard");
                if(choice==1)return;if(choice==0&&!SaveDraft())return;
            }
            if(draft)DestroyImmediate(draft);asset=next;draft=rig&&asset.number>=3&&!asset.manuallyAuthored?WoolEndlessGenerator.Generate(asset.number,rig.levels[1].route):Instantiate(asset);draft.name=asset.name;draft.hideFlags=HideFlags.HideAndDontSave;
            selectedVehicle=selectedPoint=-1;hasUnsavedChanges=false;headProgress=draft.tutorial?.8f:Mathf.Max(.45f,draft.initialHeadProgress);artDirty=true;ValidateDraft();ClearRecovery();Repaint();
        }
        public override void SaveChanges(){if(SaveDraft())base.SaveChanges();else StashDraft();}
        public override void DiscardChanges(){hasUnsavedChanges=false;ClearRecovery();if(asset){var keep=asset;asset=null;SwitchTo(keep);}base.DiscardChanges();}
        bool SaveDraft()
        {
            if(!asset||!draft)return true;
            var candidate=Instantiate(draft);
            try
            {
                WoolManualLevelUtility.UpdateDerivedData(candidate);candidate.manuallyAuthored=true;candidate.hideFlags=asset.hideFlags;
                var errors=WoolManualLevelUtility.Validate(candidate,rig?rig.vehicles.Length:70,rig?rig.yarn.Length:400);
                if(errors.Count>0){status="Cannot save: "+errors[0]+" Fix the highlighted issues; your draft is retained.";problems=errors;StashDraft();Repaint();return false;}
                Undo.RecordObject(asset,"Save manual Wool level");EditorUtility.CopySerialized(candidate,asset);asset.name=Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(asset));EditorUtility.SetDirty(asset);AssetDatabase.SaveAssetIfDirty(asset);
                EditorUtility.CopySerialized(asset,draft);draft.hideFlags=HideFlags.HideAndDontSave;hasUnsavedChanges=false;ClearRecovery();status="Saved "+asset.name+". Manual layout is protected from campaign regeneration.";artDirty=true;ValidateDraft();Repaint();return true;
            }
            finally{DestroyImmediate(candidate);}
        }
        void Record(string description){Undo.RecordObject(draft,description);}
        void Changed(bool vehicles=false)
        {
            if(vehicles){WoolManualLevelUtility.UpdateDerivedData(draft);if(balanceOnVehicleEdit){if(draft.vehicles.Length>0)WoolManualLevelUtility.BalanceYarn(draft);else draft.yarn=Array.Empty<int>();}}
            hasUnsavedChanges=true;artDirty=true;ValidateDraft();StashDraft();Repaint();
        }
        void ValidateDraft(){problems=draft?WoolManualLevelUtility.Validate(draft,rig?rig.vehicles.Length:70,rig?rig.yarn.Length:400):new List<string>();}
        void StashDraft(){if(hasUnsavedChanges&&asset&&draft)File.WriteAllText(Recovery,JsonUtility.ToJson(new RecoveryData{guid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset)),json=EditorJsonUtility.ToJson(draft)}));}
        void ClearRecovery(){if(File.Exists(Recovery))File.Delete(Recovery);}
        void RestoreDraft()
        {
            if(draft)return;
            if(File.Exists(Recovery))try{var record=JsonUtility.FromJson<RecoveryData>(File.ReadAllText(Recovery));var saved=AssetDatabase.LoadAssetAtPath<WoolLevelDefinition>(AssetDatabase.GUIDToAssetPath(record.guid));if(saved){asset=saved;draft=Instantiate(saved);draft.hideFlags=HideFlags.HideAndDontSave;EditorJsonUtility.FromJsonOverwrite(record.json,draft);hasUnsavedChanges=true;status="Recovered your unsaved manual draft.";ValidateDraft();return;}}catch(Exception e){Debug.LogWarning("Could not restore level draft: "+e.Message);}
            if(asset){var saved=asset;asset=null;SwitchTo(saved);}else if(levels.Length>0)SwitchTo(levels[0]);
        }
        void OnGUI()
        {
            using(new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if(GUILayout.Button("Refresh",EditorStyles.toolbarButton,GUILayout.Width(60)))FindLevels();
                int index=Array.IndexOf(levels,asset);
                using(new EditorGUI.DisabledScope(levels.Length==0))
                {
                    if(GUILayout.Button("<",EditorStyles.toolbarButton,GUILayout.Width(24))&&index>0)SwitchTo(levels[index-1]);
                    EditorGUI.BeginChangeCheck();int pick=EditorGUILayout.Popup(Mathf.Max(0,index),levels.Select(x=>"Level "+x.number+" — "+x.displayName).ToArray(),EditorStyles.toolbarPopup,GUILayout.Width(245));if(EditorGUI.EndChangeCheck()&&pick!=index&&pick>=0&&pick<levels.Length)SwitchTo(levels[pick]);
                    if(GUILayout.Button(">",EditorStyles.toolbarButton,GUILayout.Width(24))&&index+1<levels.Length)SwitchTo(levels[index+1]);
                }
                requestedLevel=EditorGUILayout.IntField(requestedLevel,GUILayout.Width(65));if(GUILayout.Button("Open #",EditorStyles.toolbarButton,GUILayout.Width(60)))OpenGenerated();
                GUILayout.FlexibleSpace();
                if(GUILayout.Button("Select asset",EditorStyles.toolbarButton,GUILayout.Width(85))&&asset){Selection.activeObject=asset;EditorGUIUtility.PingObject(asset);}
                using(new EditorGUI.DisabledScope(!hasUnsavedChanges||EditorApplication.isPlayingOrWillChangePlaymode))
                {if(GUILayout.Button("Save Level",EditorStyles.toolbarButton,GUILayout.Width(90)))SaveDraft();if(GUILayout.Button("Revert",EditorStyles.toolbarButton,GUILayout.Width(55)))DiscardChanges();}
            }
            if(EditorApplication.isPlayingOrWillChangePlaymode){EditorGUILayout.HelpBox("Stop Play Mode to edit levels. This tool previews art without running gameplay.",MessageType.Info);return;}
            if(!draft){EditorGUILayout.HelpBox("No level assets found. Open the Wool Crush scene and refresh.",MessageType.Info);return;}
            if(!rig)EditorGUILayout.HelpBox("Open the Wool Crush scene for the game-art preview. Diagram editing is still available.",MessageType.Info);
            var previous=mode;mode=GUILayout.Toolbar(mode,new[]{"Vehicles","Snake path","Wool sequence"});if(previous!=mode){dragging=false;zoom=mode==0?1:mode==1?2:0;Repaint();}
            zoom=GUILayout.Toolbar(zoom,new[]{"Entire level","Board close-up","Path close-up"});
            using(new EditorGUILayout.HorizontalScope())
            {
                using(new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                {
                    EditorGUILayout.LabelField("Edit Mode preview • click to select, drag to move",EditorStyles.miniLabel);
                    var area=GUILayoutUtility.GetRect(200,10000,300,10000,GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true));DrawCanvas(area);
                    EditorGUI.BeginChangeCheck();headProgress=EditorGUILayout.Slider("Snake preview position",headProgress,.02f,.97f);if(EditorGUI.EndChangeCheck()){artDirty=true;Repaint();}
                    EditorGUILayout.LabelField("Preview position does not change the level start or player progress.",EditorStyles.miniLabel);
                }
                using(new EditorGUILayout.VerticalScope(GUILayout.Width(330)))
                {
                    scroll=EditorGUILayout.BeginScrollView(scroll);
                    DrawSettings();if(mode==0)DrawVehicle();else if(mode==1)DrawPath();else DrawWool();
                    EditorGUILayout.Space();EditorGUILayout.LabelField("Layout checks",EditorStyles.boldLabel);
                    if(problems.Count==0)EditorGUILayout.HelpBox("Geometry and wool quantities are consistent. Timed difficulty still needs a playthrough.",MessageType.Info);
                    else foreach(var problem in problems.Take(12))EditorGUILayout.HelpBox(problem,MessageType.Warning);
                    EditorGUILayout.EndScrollView();
                }
            }
            EditorGUILayout.HelpBox(status,MessageType.None);
        }
        void OpenGenerated()
        {
            if(!rig||requestedLevel<1)return;
            var existing=levels.FirstOrDefault(l=>l.number==requestedLevel);if(existing){SwitchTo(existing);return;}
            string folder="Assets/Resources/LevelOverrides",path=folder+"/Level"+requestedLevel+".asset";
            var target=AssetDatabase.LoadAssetAtPath<WoolLevelDefinition>(path);
            if(!target){Directory.CreateDirectory(folder);target=WoolEndlessGenerator.Generate(requestedLevel,rig.levels[1].route);target.hideFlags=HideFlags.None;AssetDatabase.CreateAsset(target,path);AssetDatabase.SaveAssets();}
            SwitchTo(target);
        }
        void DrawSettings()
        {
            EditorGUILayout.LabelField("Level "+draft.number+(draft.manuallyAuthored?" • Manual":""),EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();string name=EditorGUILayout.TextField("Name",draft.displayName);var difficulty=(WoolDifficulty)EditorGUILayout.EnumPopup("Difficulty",draft.difficulty);
            float rotation=EditorGUILayout.Slider("Board rotation",draft.boardRotation,-180,180),scale=EditorGUILayout.Slider("Board scale",draft.boardScale,.4f,1);
            float speed=EditorGUILayout.FloatField("Dragon speed",draft.advanceSpeed),rate=EditorGUILayout.FloatField("Winding rate",draft.windingRate),fog=EditorGUILayout.FloatField("Fog boundary Y",draft.fogAbove);
            if(EditorGUI.EndChangeCheck()){Record("Edit level settings");draft.displayName=name;draft.difficulty=difficulty;draft.boardRotation=rotation;draft.boardScale=scale;draft.advanceSpeed=Mathf.Max(.01f,speed);draft.windingRate=Mathf.Max(.01f,rate);draft.fogAbove=fog;Changed();}
            EditorGUI.BeginChangeCheck();var environment=(WoolEnvironmentDefinition)EditorGUILayout.ObjectField("Environment",draft.environment,typeof(WoolEnvironmentDefinition),false);
            if(EditorGUI.EndChangeCheck()){Record("Change environment");draft.environment=environment;Changed();}
            EditorGUI.BeginChangeCheck();bool rail=EditorGUILayout.Toggle("Moving rail",draft.hasRail);int depots=EditorGUILayout.IntSlider("Containers",draft.containerPositions.Length,0,2);
            if(EditorGUI.EndChangeCheck()){Record("Edit vehicle sources");draft.hasRail=rail;Array.Resize(ref draft.containerPositions,depots);for(int c=0;c<depots;c++)if(draft.containerPositions[c]==Vector2.zero)draft.containerPositions[c]=new Vector2(c==0?-2.35f:2.35f,-5.95f);Changed();}
            for(int c=0;c<draft.containerPositions.Length;c++){EditorGUI.BeginChangeCheck();var p=EditorGUILayout.Vector2Field("Container "+(c+1),draft.containerPositions[c]);if(EditorGUI.EndChangeCheck()){Record("Move container");draft.containerPositions[c]=p;foreach(var v in draft.vehicles)if(v.source==WoolVehicleSource.Container&&v.sourceId==c){v.x=p.x;v.y=p.y;}Changed(true);}}
            EditorGUILayout.LabelField("Starting slots", "4 + 3 rewarded unlocks");snap=EditorGUILayout.Slider("Drag snap",snap,0,.5f);
            balanceOnVehicleEdit=EditorGUILayout.ToggleLeft("Keep wool quantities balanced when editing boxes",balanceOnVehicleEdit);
            EditorGUILayout.Space();
        }
        void DrawVehicle()
        {
            EditorGUILayout.LabelField("Vehicles ("+draft.vehicles.Length+")",EditorStyles.boldLabel);
            var labels=draft.vehicles.Select((t,i)=>"#"+i+"  "+WoolArt.ColorNames[Mathf.Clamp(t.color,0,8)]+" "+t.size+" / "+WoolLevel.Capacity(t.size)).ToArray();
            if(labels.Length>0)selectedVehicle=EditorGUILayout.Popup("Selected",Mathf.Clamp(selectedVehicle,0,labels.Length-1),labels);
            if(selectedVehicle>=0&&selectedVehicle<draft.vehicles.Length)
            {
                var t=draft.vehicles[selectedVehicle];EditorGUI.BeginChangeCheck();int color=EditorGUILayout.Popup("Color",t.color,WoolArt.ColorNames);var size=(WoolSize)EditorGUILayout.EnumPopup("Size",t.size);var direction=(WoolDirection)EditorGUILayout.EnumPopup("Arrow",t.direction);var pos=EditorGUILayout.Vector2Field("Logical position",new Vector2(t.x,t.y));
                float width=EditorGUILayout.FloatField("Width",t.width),length=EditorGUILayout.FloatField("Length",t.length);
                if(EditorGUI.EndChangeCheck()){Record("Edit vehicle");if(size!=t.size){width=.87f;length=size==WoolSize.Small?.87f:size==WoolSize.Medium?1.35f:1.83f;}t.color=color;t.size=size;t.direction=direction;t.x=pos.x;t.y=pos.y;t.width=Mathf.Max(.1f,width);t.length=Mathf.Max(.1f,length);Changed(true);}
                EditorGUILayout.LabelField("Capacity",WoolLevel.Capacity(t.size)+" wool units");
                EditorGUI.BeginChangeCheck();var source=(WoolVehicleSource)EditorGUILayout.EnumPopup("Vehicle source",t.source);int sourceId=EditorGUILayout.IntSlider("Container index",t.sourceId,0,1),queue=EditorGUILayout.IntField("Queue order",t.sourceOrder);
                if(EditorGUI.EndChangeCheck()){Record("Edit source queue");t.source=source;t.sourceId=sourceId;t.sourceOrder=Mathf.Max(0,queue);Changed(true);}
                if(t.source!=WoolVehicleSource.Board)EditorGUILayout.HelpBox("Queued vehicles share a source. Only the front / visible rail vehicles appear during play.",MessageType.Info);
                using(new EditorGUILayout.HorizontalScope())
                {
                    if(GUILayout.Button("Duplicate")){Record("Duplicate vehicle");var copy=JsonUtility.FromJson<WoolTile>(JsonUtility.ToJson(t));copy.x+=1;draft.vehicles=draft.vehicles.Concat(new[]{copy}).ToArray();selectedVehicle=draft.vehicles.Length-1;Changed(true);}
                    if(GUILayout.Button("Delete")){Record("Delete vehicle");draft.vehicles=draft.vehicles.Where((x,i)=>i!=selectedVehicle).ToArray();selectedVehicle=Mathf.Min(selectedVehicle,draft.vehicles.Length-1);Changed(true);}
                }
            }
            if(GUILayout.Button("Add vehicle")){Record("Add vehicle");draft.vehicles=draft.vehicles.Concat(new[]{new WoolTile{x=0,y=-3.2f,width=.87f,length=.87f,color=0,size=WoolSize.Small,ammo=4}}).ToArray();selectedVehicle=draft.vehicles.Length-1;Changed(true);}
        }
        void DrawPath()
        {
            EditorGUILayout.LabelField("Snake route • entry to kitten",EditorStyles.boldLabel);
            if(draft.route.Length==0&&GUILayout.Button("Create route")){Record("Create route");draft.route=new[]{new Vector3(-4,8.5f,0),new Vector3(3,8,0),new Vector3(4,5,0),new Vector3(-2,4,0)};selectedPoint=0;Changed();}
            if(draft.route.Length>0)
            {
                selectedPoint=EditorGUILayout.IntSlider("Selected point",Mathf.Clamp(selectedPoint,0,draft.route.Length-1),0,draft.route.Length-1);
                EditorGUI.BeginChangeCheck();var p=EditorGUILayout.Vector2Field("Position",draft.route[selectedPoint]);if(EditorGUI.EndChangeCheck()){Record("Move route point");draft.route[selectedPoint]=new Vector3(p.x,p.y,0);Changed();}
                if(GUILayout.Button("Insert point after selection")){Record("Insert route point");var list=draft.route.ToList();list.Insert(selectedPoint+1,selectedPoint+1<list.Count?(list[selectedPoint]+list[selectedPoint+1])*.5f:list[selectedPoint]+Vector3.right);draft.route=list.ToArray();selectedPoint++;Changed();}
                using(new EditorGUI.DisabledScope(draft.route.Length<=2))if(GUILayout.Button("Delete selected point")){Record("Delete route point");draft.route=draft.route.Where((p,i)=>i!=selectedPoint).ToArray();selectedPoint=Mathf.Min(selectedPoint,draft.route.Length-1);Changed();}
            }
            EditorGUILayout.HelpBox("Point 0 is the entry. The kitten sits near the last point. Drag numbered dots in the preview. The tutorial uses a painted background; its route changes may need matching background art.",MessageType.Info);
        }
        void DrawWool()
        {
            EditorGUILayout.LabelField("Wool • head to tail",EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();int batch=EditorGUILayout.IntSlider("Batch size",Mathf.Max(1,draft.snakeBatchSize),1,7);if(EditorGUI.EndChangeCheck()){Record("Set wool batch size");draft.snakeBatchSize=batch;Changed();}
            if(GUILayout.Button("Rebuild sequence from exit order")){Record("Rebuild wool");try{WoolManualLevelUtility.RebuildYarn(draft);Changed();}catch(Exception e){status=e.Message;}}
            if(GUILayout.Button("Balance quantities, keep order")){Record("Balance wool");try{WoolManualLevelUtility.BalanceYarn(draft);Changed();}catch(Exception e){status=e.Message;}}
            var runs=new List<Vector2Int>();foreach(int color in draft.yarn){if(runs.Count>0&&runs[runs.Count-1].x==color){var r=runs[runs.Count-1];r.y++;runs[runs.Count-1]=r;}else runs.Add(new Vector2Int(color,1));}
            EditorGUILayout.LabelField(draft.yarn.Length+" units in "+runs.Count+" color runs",EditorStyles.miniLabel);
            for(int i=0;i<runs.Count;i++)
            {
                using(new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label((i+1).ToString(),GUILayout.Width(24));EditorGUI.BeginChangeCheck();int color=EditorGUILayout.Popup(runs[i].x,WoolArt.ColorNames),count=EditorGUILayout.IntField(runs[i].y,GUILayout.Width(42));
                    if(EditorGUI.EndChangeCheck()){Record("Edit wool run");runs[i]=new Vector2Int(color,Mathf.Clamp(count,1,512));SetRuns(runs);break;}
                    using(new EditorGUI.DisabledScope(i==0))if(GUILayout.Button("↑",GUILayout.Width(24))){Record("Move wool run");var r=runs[i-1];runs[i-1]=runs[i];runs[i]=r;SetRuns(runs);break;}
                    using(new EditorGUI.DisabledScope(i==runs.Count-1))if(GUILayout.Button("↓",GUILayout.Width(24))){Record("Move wool run");var r=runs[i+1];runs[i+1]=runs[i];runs[i]=r;SetRuns(runs);break;}
                }
            }
        }
        void SetRuns(List<Vector2Int> runs){draft.yarn=runs.SelectMany(r=>Enumerable.Repeat(r.x,r.y)).ToArray();Changed();}
        Rect portrait;
        float ViewWidth=>zoom==2?10.4f:9.7f;
        Vector3 ViewCenter=>zoom==1?new Vector3(0,-3.2f,0):zoom==2?new Vector3(0,6,0):Vector3.zero;
        Vector2 ScreenPoint(Vector3 p)=>new Vector2(portrait.center.x+(p.x-ViewCenter.x)*portrait.width/ViewWidth,portrait.center.y-(p.y-ViewCenter.y)*portrait.width/ViewWidth);
        Vector3 WorldPoint(Vector2 p)=>ViewCenter+new Vector3((p.x-portrait.center.x)*ViewWidth/portrait.width,(portrait.center.y-p.y)*ViewWidth/portrait.width,0);
        void DrawCanvas(Rect area)
        {
            EditorGUI.DrawRect(area,new Color(.1f,.12f,.15f));float aspect=zoom==0?.45f:zoom==1?1.2f:1.5f;float width=Mathf.Min(area.width,area.height*aspect);portrait=new Rect(area.center.x-width/2,area.center.y-width/aspect/2,width,width/aspect);
            if(Event.current.type==EventType.Repaint)
            {
                if(artDirty){art??=new WoolLevelArtPreview();art.Rebuild(rig,draft,headProgress);artDirty=false;}
                var texture=art?.Render(portrait,ViewCenter,ViewWidth);if(texture)GUI.DrawTexture(portrait,texture,ScaleMode.StretchToFill);else EditorGUI.DrawRect(portrait,WoolArt.Hex("F8E9CE"));
                var absolutePortrait=portrait;GUI.BeginGroup(portrait);portrait=new Rect(0,0,portrait.width,portrait.height);
                Handles.BeginGUI();
                if(mode==0)for(int i=0;i<draft.vehicles.Length;i++)
                {
                    var t=draft.vehicles[i];var center=WoolLevelArtPreview.BoardPoint(draft,new Vector2(t.x,t.y));bool vertical=t.direction==WoolDirection.Up||t.direction==WoolDirection.Down;
                    float w=(vertical?t.width:t.length)*draft.boardScale,h=(vertical?t.length:t.width)*draft.boardScale;var q=Quaternion.Euler(0,0,draft.boardRotation);
                    var corners=new[]{new Vector3(-w,-h),new Vector3(-w,h),new Vector3(w,h),new Vector3(w,-h),new Vector3(-w,-h)}.Select(p=>(Vector3)ScreenPoint(center+q*p*.5f)).ToArray();
                    Handles.color=i==selectedVehicle?Color.white:new Color(.1f,.1f,.1f,.35f);Handles.DrawAAPolyLine(i==selectedVehicle?3:1,corners);
                    if(!rig){Handles.color=WoolArt.Palette[Mathf.Clamp(t.color,0,8)];Handles.DrawAAConvexPolygon(corners.Take(4).ToArray());}
                    if(i==selectedVehicle)GUI.Label(new Rect(ScreenPoint(center)+new Vector2(-20,-28),new Vector2(70,20)),"#"+i+" / "+WoolLevel.Capacity(t.size),EditorStyles.whiteMiniLabel);
                }
                if(mode==1)
                {
                    Handles.color=new Color(1,.35f,.05f,1);Handles.DrawAAPolyLine(2,draft.route.Select(p=>(Vector3)ScreenPoint(p)).ToArray());
                    for(int i=0;i<draft.route.Length;i++){var p=ScreenPoint(draft.route[i]);EditorGUI.DrawRect(new Rect(p-Vector2.one*5,Vector2.one*10),i==selectedPoint?Color.white:Color.cyan);GUI.Label(new Rect(p+new Vector2(6,-6),new Vector2(40,20)),i.ToString(),EditorStyles.whiteMiniLabel);}
                }
                Handles.EndGUI();
                for(int i=0;i<7;i++)
                {
                    var slot=i<4?new Vector3((i-1.5f)*2.1f,draft.tutorial?-.9f:2.25f,0):new Vector3((i-5)*2.1f,draft.tutorial?-1.62f:1.35f,0);
                    var p=ScreenPoint(slot);var style=new GUIStyle(EditorStyles.miniLabel){alignment=TextAnchor.MiddleCenter};style.normal.textColor=Color.white;
                    GUI.Label(new Rect(p+new Vector2(-35,-8),new Vector2(70,16)),i<4?"Slot "+(i+1):"Locked +",style);
                }
                float fogY=ScreenPoint(new Vector3(0,draft.fogAbove)).y;if(!draft.tutorial&&fogY>portrait.y)EditorGUI.DrawRect(new Rect(portrait.x,portrait.y,portrait.width,Mathf.Min(portrait.height,fogY-portrait.y)),new Color(.95f,1,.95f,.24f));
                GUI.EndGroup();portrait=absolutePortrait;
            }
            HandleDrag();
        }
        void HandleDrag()
        {
            var e=Event.current;if(e.button!=0||mode==2)return;
            if(e.type==EventType.MouseDown&&portrait.Contains(e.mousePosition))
            {
                int hit=-1;var world=WorldPoint(e.mousePosition);
                if(mode==0)
                {
                    var logical=WoolLevelArtPreview.LogicalPoint(draft,world);
                    for(int i=draft.vehicles.Length-1;i>=0;i--){var t=draft.vehicles[i];bool v=t.direction==WoolDirection.Up||t.direction==WoolDirection.Down;if(Mathf.Abs(logical.x-t.x)<(v?t.width:t.length)*.5f&&Mathf.Abs(logical.y-t.y)<(v?t.length:t.width)*.5f){hit=i;dragOffset=new Vector2(t.x,t.y)-logical;break;}}
                    selectedVehicle=hit;
                }
                else{for(int i=0;i<draft.route.Length;i++)if(Vector2.Distance(ScreenPoint(draft.route[i]),e.mousePosition)<12){hit=i;dragOffset=(Vector2)draft.route[i]-(Vector2)world;break;}selectedPoint=hit;}
                if(hit>=0){Undo.IncrementCurrentGroup();Undo.RegisterCompleteObjectUndo(draft,mode==0?"Drag vehicle":"Drag route point");dragging=true;e.Use();Repaint();}
            }
            else if(e.type==EventType.MouseDrag&&dragging)
            {
                Vector2 p=mode==0?WoolLevelArtPreview.LogicalPoint(draft,WorldPoint(e.mousePosition)):(Vector2)WorldPoint(e.mousePosition);p+=dragOffset;if(snap>.001f)p=new Vector2(Mathf.Round(p.x/snap)*snap,Mathf.Round(p.y/snap)*snap);
                if(mode==0){draft.vehicles[selectedVehicle].x=p.x;draft.vehicles[selectedVehicle].y=p.y;}else draft.route[selectedPoint]=new Vector3(p.x,p.y,0);
                Changed(mode==0);e.Use();
            }
            else if(e.type==EventType.MouseUp&&dragging){dragging=false;Undo.FlushUndoRecordObjects();e.Use();}
        }
    }

    [CustomEditor(typeof(WoolLevelDefinition))]
    public sealed class WoolLevelDefinitionInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if(GUILayout.Button("Open Visual Level Editor",GUILayout.Height(30)))WoolLevelEditorWindow.OpenLevel((WoolLevelDefinition)target);
            EditorGUILayout.HelpBox("Use the visual editor for safe drag editing, balanced capacities, preview, and manual-save protection. Raw fields below are advanced settings.",MessageType.Info);
            DrawDefaultInspector();
        }
    }
}
