using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace WoolGame
{
    public sealed partial class WoolGameController : MonoBehaviour
    {
        // Retained only for existing scene serialization; campaign data owns startup.
        [SerializeField] bool audioEnabled=true;
        [SerializeField] public WoolSceneRig rig;
        [Header("Level preview")]
        [SerializeField,Min(1),Tooltip("Enable Open Inspector Level On Play to start this level. During Play use the component menu: Open Inspector Level.")] int inspectorLevel=1;
        [SerializeField] bool openInspectorLevelOnPlay;
        bool inspectorPreview;
        [ContextMenu("Open Inspector Level")]
        public void OpenInspectorLevel()
        {
            if(!Application.isPlaying){Debug.Log("Enable Open Inspector Level On Play, then press Play to open the selected level.",this);return;}
            if(!hud)return;
            if(!inspectorPreview){WoolProgress.BeginPreview();inspectorPreview=true;}
            automatedInputValue=true;
            LoadLevel(Mathf.Clamp(inspectorLevel,1,WoolEndlessGenerator.MaxLevel));
            automatedInputValue=false; // Keep preview save isolation while allowing normal input.
        }
        [SerializeField,Min(1)] float amplifySeconds=12;
        WoolLevel level;
        WoolLevelDefinition definition,generatedDefinition;
        WoolVehicleSources vehicleSources;
        WoolHud hud;
        WoolDragonFlame flame;
        Camera view;
        Transform world,dragonHead,tutorialHand;
        AudioSource sound;
        WoolGameAudio gameAudio;
        readonly List<Transform> yarn=new List<Transform>(512),tileViews=new List<Transform>(WoolEndlessGenerator.MaxVehicles);
        readonly List<Renderer> yarnRenderers=new List<Renderer>(512);
        MaterialPropertyBlock yarnProperties;
        static readonly int FollowId=Shader.PropertyToID("_FollowSpline"),SectionId=Shader.PropertyToID("_SplineSection");
        readonly List<float> yarnAmounts=new List<float>(512);
        readonly ActiveCrossbow[] active=new ActiveCrossbow[WoolLevel.Slots];
        int collectionSequence;
        readonly Transform[] projectileTargets=new Transform[8];
        readonly int[] projectileColors=new int[8];
        Vector3 kittenStart,dragonScale,kittenScale;
        int money,total,tutorialStep,unlockedSlotMask,sortUses;
        int tutorialShotSlot=-1;
        float tutorialFeedbackUntil;
        float headDistance,elapsed,amplifyUntil,grace,entryTarget;
        bool entering;
        float nextDailySave;
        public float ApproachSpeedMultiplier=>entering?2:1;
        public const float VehicleSpeedMultiplier=.9f;
        bool paused,ended,transition,removeMode,winBonusClaimed,revived,attemptStarted,newCompletion;
        int gameplaySpeed=1;
        public int GameplaySpeed=>gameplaySpeed;
        public float SnakeDangerSpeedMultiplier
        {
            get
            {
                if(!rig||!rig.duckRescue||rig.duckRescue.BothCaught)return 1;
                var ducks=rig.duckRescue;
                float nearest=ducks.MotherCaught?ducks.BabyDistance:ducks.MotherDistance;
                float gap=nearest-headDistance-.35f;
                return Mathf.Lerp(.45f,1,Mathf.SmoothStep(0,1,Mathf.InverseLerp(.5f,3.5f,gap)));
            }
        }
        int simultaneousCollectors;
        public int SimultaneousCollectors => simultaneousCollectors;
        public int CollectingColorMask {get;private set;}
        int pendingRemoveCost,pendingSlot=-1;
        bool pendingRemoveOwned;
        bool returningDragon;
        float defeatDistance;
        float dragonAttackTime;
        bool dragonNearDuck,dragonFireStarted;
        enum Phase { Ready,Playing,Won,Lost,Menu }
        Phase phase;
        sealed class ActiveCrossbow
        {
            public int index,slot,initialAmmo;
            public float ammo,dock;
            public bool arrived,smoking;
            public int arrivalOrder;
            public Vector3 start,exit,corridor,startScale;
            public float travelDuration;
            public Vector3[] path;
            public float pathLength;
        }
        public System.Collections.Generic.IReadOnlyList<int> SolutionOrder=>level.Solution;
        public int CurrentLevel=>level==null?0:level.Number;
        public int RemainingYarn=>level==null?0:level.Yarn.Count;
        public string CurrentPhase=>phase.ToString();
        public int VehicleCount=>level==null?0:level.Tiles.Count;
        public bool IsBusy=>transition||paused;
        public void SetOnboardingPause(bool value){paused=value;}
        public float Elapsed=>elapsed;

        bool automatedInputValue;
        public bool automatedInput
        {
            get=>automatedInputValue;
            set
            {
                if(automatedInputValue==value)return;
                automatedInputValue=value;
                if(value)WoolProgress.BeginPreview();
                else { WoolProgress.EndPreview();money=WoolProgress.IsLoaded?WoolProgress.Coins:Mathf.Max(0,PlayerPrefs.GetInt("WoolRescue.Coins",0)); }
            }
        }

        IEnumerator Start()
        {
            if(!rig||!rig.hud||rig.levels.Length==0){Debug.LogError("The core scene must be authored before Play.",this);enabled=false;yield break;}
            // Gameplay pause is owned by paused/modal state; clear stale Editor or SDK time freezes at startup.
            Time.timeScale=1f;
            if(rig.arrowPool)rig.arrowPool.sceneRig=rig;rig.PrepareSnakeModels();flame=GetComponent<WoolDragonFlame>();
            QualitySettings.vSyncCount=0;Application.targetFrameRate=40;
            if(Application.isMobilePlatform)Application.targetFrameRate=40;Screen.orientation=ScreenOrientation.Portrait;AudioListener.volume=1;
            yarnProperties=new MaterialPropertyBlock();world=rig.world;view=rig.gameCamera;hud=rig.hud;dragonHead=rig.dragon;tutorialHand=rig.hand;sound=rig.audioSource;
            kittenStart=rig.kitten.localPosition;kittenScale=rig.kitten.localScale;dragonScale=dragonHead.localScale;
            hud.InitializeAuthored();hud.EnsureTouchFeedback();gameAudio=GetComponent<WoolGameAudio>();if(!gameAudio)gameAudio=gameObject.AddComponent<WoolGameAudio>();gameAudio.Initialize(rig,audioEnabled);sound=rig.audioSource;hud.PausePressed+=TogglePause;hud.BoosterPressed+=OfferBooster;hud.SpeedPressed+=ToggleGameplaySpeed;
            hud.PlayPressed+=PlaySaved;hud.FreeCoinsPressed+=OfferFreeCoins;hud.LevelSelected+=OpenCampaignLevel;
            WoolProgress.Load(WoolEndlessGenerator.MaxLevel);WoolProgress.InitializeOnboarding();money=WoolProgress.Coins;
            if(rig.duckRescue)rig.duckRescue.DuckCaught+=PlayDuckCaught;
            if(!automatedInput){var ads=WoolServices.EnsureAds();ads.AdVisibilityChanged+=hud.SetAdVisible;ads.AdLoadingChanged+=hud.SetAdLoading;}
            phase=Phase.Menu;transition=true;world.gameObject.SetActive(false);if(rig.authoredLayout)rig.authoredLayout.environment.SetActive(false);hud.Hide();hud.CloseMenu();
            hud.Launch("Arrow Sort Puzzle: Rescue Ducks",WoolArt.Hex("F39B22"),0);
            // Startup imports and SDK initialization can stall the first frame for seconds.
            // Bound presentation time per rendered frame so that stall cannot skip the bar.
            hud.SetLaunchProgress(0,"Getting your rescue ready.");
            yield return null;
            const float loadingPresentationSeconds=2.5f;
            for(float time=0;time<loadingPresentationSeconds;time+=Mathf.Min(Time.unscaledDeltaTime,1f/30f))
            {
                hud.SetLaunchProgress(Mathf.Clamp01(time/loadingPresentationSeconds),"Every little move makes a big rescue.");
                yield return null;
            }
            hud.SetLaunchProgress(1,"Your next adventure is ready.");
            yield return null;
            yield return new WaitForSecondsRealtime(.35f);
            WoolProgress.MarkStarted();transition=false;hud.EndLaunch();
            #if UNITY_EDITOR
            if(openInspectorLevelOnPlay){OpenInspectorLevel();yield break;}
#endif
            if(!automatedInput&&WoolProgress.OnboardingStep!=WoolOnboardingStep.Complete)
            {
                var guide=GetComponent<WoolOnboarding>();if(!guide)guide=gameObject.AddComponent<WoolOnboarding>();
                guide.Begin(this,hud);
            }
            else OpenHome();
        }
        void PlaySaved()=>OpenCampaignLevel(WoolProgress.CurrentLevel);
        public void OpenCampaignLevel(int number)
        {
            if(transition||!WoolProgress.IsUnlocked(number))return;
            LoadLevel(number);
        }
        public void ResetCampaign()
        {
            if(transition)return;
            WoolProgress.ResetProgress(true);WoolProgress.MarkStarted();money=WoolProgress.Coins;LoadLevel(1);
        }
        public void OpenHome()
        {
            if(transition)return;
            hud.ClearVehicleFeedback();
            SaveTutorialState();
            hud.PointLevelTutorialAt(Vector2.zero,false);if(tutorialHand)tutorialHand.gameObject.SetActive(false);hud.HideAbilityGuide();
            if(rig.arrowPool)rig.arrowPool.ResetPool();
            hud.SetDanger(0,0);if(flame)flame.ResetBreath();
            if(attemptStarted&&!ended&&!TutorialCheckpointActive)WoolProgress.MarkFailed(level.Number);
            if(gameAudio)gameAudio.SetGameplayMusic(false);
            StopAllCoroutines();hud.HideGameplayRule();SetSortWhite(0);paused=false;removeMode=false;phase=Phase.Menu;world.gameObject.SetActive(false);if(rig.authoredLayout)rig.authoredLayout.environment.SetActive(false);
            money=WoolProgress.Coins;hud.EndLaunch();hud.SelectionMode(false,null);hud.Hide();hud.Home(WoolProgress.CurrentLevel,money);
        }
        void BeginAttempt()
        {
            if(attemptStarted)return;
            attemptStarted=true;WoolProgress.BeginLevel(level.Number);
        }

        void BuildPath()=>rig.authoredLayout.SelectPath(level.Number,rig.route,world,definition.difficulty);
        Vector3 OnPath(float distance)=>rig.route.Position(distance);
        Vector3 Slot(int i)=>world.InverseTransformPoint(rig.slots[i].position)+new Vector3(0,0,-.35f);
        WoolLevelDefinition ResolveLevel(int number)
        {
            if(generatedDefinition){Destroy(generatedDefinition);generatedDefinition=null;}
            if(number<=3||number==4||number==6||number==8){generatedDefinition=WoolEndlessGenerator.Generate(number,rig.levels[1].route);return generatedDefinition;}
            if(number<=rig.levels.Length&&rig.levels[number-1]&&WoolEndlessGenerator.MechanicsAllowed(rig.levels[number-1])&&rig.levels[number-1].manuallyAuthored&&rig.levels[number-1].vehicles.Length>=50&&rig.levels[number-1].vehicles.Length<=70)
            {generatedDefinition=Instantiate(rig.levels[number-1]);WoolEndlessGenerator.ApplyChallenge(generatedDefinition);return generatedDefinition;}
            var manual=Resources.Load<WoolLevelDefinition>("LevelOverrides/Level"+number);
            if(manual&&WoolEndlessGenerator.MechanicsAllowed(manual)&&manual.vehicles.Length>=50&&manual.vehicles.Length<=70){generatedDefinition=Instantiate(manual);WoolEndlessGenerator.ApplyChallenge(generatedDefinition);return generatedDefinition;}
            generatedDefinition=WoolEndlessGenerator.Generate(number,rig.levels[1].route);
            return generatedDefinition;
        }
        public void LoadLevel(int number)
        {
            if(gameAudio)gameAudio.SetGameplayMusic(true);
            if(!Application.isPlaying||!hud||!rig||rig.levels.Length==0||number<1||number>WoolEndlessGenerator.MaxLevel)return;
            if(!automatedInput&&!WoolProgress.IsUnlocked(number))return;
            gameAudio.Play(WoolGameAudio.Cue.Start);StopAllCoroutines();hud.HideGameplayRule();SetSortWhite(0);if(flame)flame.ResetBreath();rig.kitten.gameObject.SetActive(true);hud.EndLaunch();hud.SetDanger(0,0);definition=ResolveLevel(number);definition.boardRotation=0;definition.advanceSpeed*=1.1475f*(number>=5?1.16f:1);
            // Keep the generated packing compact without stretching vehicle models.
            // Definitions are generated or cloned by ResolveLevel; source assets stay intact.
            if(!definition.tutorial)
            {
                const float horizontalSpacing=1.08f;
                const float rowSpacing=1.12f;
                definition.boardRotation=0;
                float bottom=float.MaxValue;
                foreach(var tile in definition.vehicles)if(tile.source==WoolVehicleSource.Board)bottom=Mathf.Min(bottom,tile.y);
                if(bottom==float.MaxValue)bottom=0;
                foreach(var tile in definition.vehicles)if(tile.source!=WoolVehicleSource.Rail){tile.x*=horizontalSpacing;tile.y=bottom+(tile.y-bottom)*rowSpacing;}
                for(int i=0;i<definition.containerPositions.Length;i++){var p=definition.containerPositions[i];definition.containerPositions[i]=new Vector2(p.x*horizontalSpacing,bottom+(p.y-bottom)*rowSpacing);}
            }
            lessonKind=-1;lessonTarget=-1;hud.PointLevelTutorialAt(Vector2.zero,false);hud.HideAbilityGuide();hud.HideAbilityEffect();WoolProgress.GrantAbilityGifts(number);
            money=WoolProgress.Coins;level=new WoolLevel(definition);total=level.Yarn.Count;for(int c=0;c<displayedCounts.Length;c++)displayedCounts[c]=-99;winBonusClaimed=revived=attemptStarted=newCompletion=false;simultaneousCollectors=0;
            if(total>rig.yarn.Length||level.Tiles.Count>rig.vehicles.Length)throw new InvalidOperationException("Level exceeds its authored pool capacity.");
            rig.ResetPool();if(rig.arrowPool)rig.arrowPool.ResetPool();ClearProjectileTargets();ResetContactMoves();hud.ClearVehicleFeedback();world.gameObject.SetActive(true);hud.CloseMenu();
            Array.Clear(active,0,active.Length);yarn.Clear();yarnRenderers.Clear();yarnAmounts.Clear();tileViews.Clear();rig.ApplyEnvironment(definition);BuildPath();
            definition.advanceSpeed*=Mathf.Lerp(1f,1.3f,Mathf.InverseLerp(24,65,rig.route.PlayableLength));
            elapsed=amplifyUntil=0;collectionSequence=0;grace=0;paused=ended=transition=removeMode=false;
            tutorialShotSlot=-1;tutorialFeedbackUntil=0;
            pendingRemoveOwned=false;pendingRemoveCost=0;
            tutorialStep=sortUses=0;unlockedSlotMask=15|(rig.vipSlotEntitled?16:0);pendingSlot=-1;returningDragon=false;dragonAttackTime=2f;dragonNearDuck=dragonFireStarted=false;gameplaySpeed=1;hud.SetGameplaySpeed(1);phase=Phase.Ready;
            challengeSupportCompleted=number<=4||rig.vipSlotEntitled;
            entryTarget=definition.tutorial?rig.route.Length-1.25f:rig.route.Length*definition.initialHeadProgress;
            if(!definition.tutorial)
            {
                // Find the first exposed collecting section on this level's actual path.
                for(float d=.8f;d<rig.route.Length-2;d+=.1f)
                    if(OnPath(d).y<definition.fogAbove-.35f){entryTarget=Mathf.Max(entryTarget,d+1.4f);break;}
            }
            if(rig.authoredLayout){entryTarget=.6f;for(float d=0;d<rig.route.Length-1;d+=.1f)if(!rig.authoredLayout.IsInFog(world.TransformPoint(OnPath(d)))){entryTarget=d+.7f;break;}}
            headDistance=rig.authoredLayout?0:definition.tutorial?.55f:Mathf.Max(.8f,entryTarget-2.4f);entering=true;
            if(rig.entrancePortal){rig.entrancePortal.gameObject.SetActive(!rig.authoredLayout&&definition.tutorial);rig.entrancePortal.localPosition=OnPath(0)+Vector3.back*.1f;}
            var resume=TutorialCheckpointActive?WoolProgress.TutorialCheckpoint:null;
            if(ValidTutorialState(resume))attemptStarted=true;else BeginAttempt();
            for(int i=0;i<total;i++){yarn.Add(rig.yarn[i]);yarnRenderers.Add(rig.yarn[i].GetComponent<Renderer>());yarnAmounts.Add(1);rig.SetSectionModel(i,level.Yarn[i]);}
            for(int i=0;i<level.Tiles.Count;i++)
            {
                rig.SetVehicle(i,level.Tiles[i],definition.boardRotation,definition.tutorial?1.15f:definition.boardScale);
                tileViews.Add(rig.vehicles[i]);
            }
            if(!vehicleSources)vehicleSources=gameObject.GetComponent<WoolVehicleSources>()??gameObject.AddComponent<WoolVehicleSources>();
            vehicleSources.Load(rig,level,definition);
            rig.CompactParking(vehicleSources,definition.tutorial);rig.ApplyLayout(definition.tutorial);
            rig.kitten.localPosition=OnPath(rig.route.Length-.7f)+new Vector3(0,0,-.4f);
            kittenStart=rig.kitten.localPosition;dragonHead.gameObject.SetActive(true);dragonHead.localScale=dragonScale;
            if(rig.duckRescue){rig.kitten.gameObject.SetActive(false);rig.duckRescue.ResetForLevel(rig.route,world,rig.authoredLayout);}
            if(rig.fog)rig.fog.gameObject.SetActive(rig.authoredLayout||!definition.tutorial);
            tutorialHand.gameObject.SetActive(TutorialGuidance);
            foreach(var renderer in tutorialHand.GetComponentsInChildren<Renderer>(true))renderer.enabled=false;
            Physics.SyncTransforms();
            PositionYarn();hud.Hide();hud.Status(level.Number,money,RemainingYarn,total);hud.Tutorial(TutorialGuidance);UpdateCrossbowCounts();UpdateTutorial();rig.CapturePortraitBounds(definition);rig.FitCamera();
            RestoreTutorialState();
            if(!definition.tutorial)hud.Hint("Can't collect wool in the fog. Choose your colors carefully.");
            if(definition.containerPositions!=null&&definition.containerPositions.Length>0&&!WoolProgress.WarehouseTutorialSeen)hud.ShowWarehouseTutorial();
            else if(definition.hasRail&&!WoolProgress.RailTutorialSeen)hud.ShowRailTutorial();
            hud.SetAbilityLevels(number);StartCoroutine(BeginGameplayRulesThenAbility());
        }
        void Update()
        {
            if(WoolProgress.IsLoaded)money=WoolProgress.Coins;
            bool suspended=paused||transition||removeMode||(hud&&(hud.ModalVisible||hud.AdVisible||hud.AbilityGuideVisible));
            if(hud&&(suspended||ended||level==null||phase==Phase.Menu))hud.SetDanger(0,elapsed);
            if(rig.arrowPool)rig.arrowPool.AllowFogTargets=elapsed<amplifyUntil;
            if(hud)hud.SetAmplifyCountdown(Mathf.Max(0,amplifyUntil-elapsed));
            UpdateTutorialPointer();
            if(phase==Phase.Won&&!transition&&rig.duckRescue&&!hud.AdVisible)rig.duckRescue.Celebrate(Mathf.Min(Time.unscaledDeltaTime,.1f));
            if(!suspended&&rig.arrowPool)rig.arrowPool.TickPresentation(Mathf.Min(Time.deltaTime,.1f)*gameplaySpeed);
            UpdateDragonAnimation(suspended);
            gameAudio.SetFireActive(flame&&flame.Bursting&&rig.DragonMouthOpen&&!suspended);
            if(level==null||phase==Phase.Menu)return;rig.FitCamera();
            if(removeMode&&!paused&&!transition&&!hud.ModalVisible&&!hud.AdVisible&&!hud.AbilityGuideVisible){PollVehiclePointer();return;}
            if(phase==Phase.Menu||suspended||ended)return;
            // Entry retains its authored two-times approach; user speed applies after entry.
            float dt=Mathf.Min(Time.deltaTime,.1f)*(entering?1:gameplaySpeed);elapsed+=dt;
            if(Time.unscaledTime>=nextDailySave){WoolProgress.Flush();nextDailySave=Time.unscaledTime+10;}
            if(phase==Phase.Ready||phase==Phase.Playing)
            {
                if(entering)
                {
                    headDistance=Mathf.MoveTowards(headDistance,entryTarget,dt*Mathf.Max(4.5f,entryTarget/2.4f));
                    if(headDistance>=entryTarget){entering=false;phase=Phase.Playing;UpdateTutorial();}
                }
                else { grace-=dt;if(grace<=0)headDistance+=dt*definition.advanceSpeed*SnakeDangerSpeedMultiplier; }
                if(definition.tutorial)headDistance=Mathf.Min(headDistance,rig.route.Length-1.5f);
                if(rig.duckRescue)rig.duckRescue.Tick(dt,headDistance,definition.advanceSpeed*SnakeDangerSpeedMultiplier,.35f+Mathf.Abs(dragonHead.localScale.x)*(rig.models?rig.models.mouthOffset.x:.65f));
                if(!definition.tutorial&&(rig.duckRescue?rig.duckRescue.BothCaught:headDistance>=rig.route.Length-.7f)){Lose("The dragon caught the duck family at the end of the path. Try a different box order.");return;}
            }
            if(vehicleSources)vehicleSources.Tick(dt,HasFreeVehicleSlot);
            PositionYarn();simultaneousCollectors=0;CollectingColorMask=0;float collected=0;
            TickContactMoves(dt);
            for(int i=0;i<active.Length;i++)
            {
                var crossbow=active[i];if(crossbow==null||crossbow.arrived)continue;
                crossbow.dock+=dt;var vehicle=tileViews[crossbow.index];
                float distance=Mathf.Clamp01(crossbow.dock/crossbow.travelDuration)*crossbow.pathLength;
                int leg=1;
                for(;leg<crossbow.path.Length-1;leg++){float length=Vector3.Distance(crossbow.path[leg-1],crossbow.path[leg]);if(distance<=length)break;distance-=length;}
                Vector3 from=crossbow.path[leg-1],to=crossbow.path[leg];
                vehicle.localPosition=Vector3.Lerp(from,to,distance/Mathf.Max(.001f,Vector3.Distance(from,to)));
                var direction=to-from;
                if(direction.sqrMagnitude>.001f)vehicle.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg+(rig.authoredLayout?-90:90))*Quaternion.Euler(-90,0,0);
                UpdateVehicleOffset(crossbow.index);var logical=Quaternion.Euler(0,0,-definition.boardRotation)*direction;
                rig.PresentDockingVehicle(vehicle,from,to,distance/Mathf.Max(.001f,Vector3.Distance(from,to)),crossbow.startScale,leg==crossbow.path.Length-1);
                level.Tiles[crossbow.index].transitDirection=Mathf.Abs(logical.x)>Mathf.Abs(logical.y)?(logical.x>0?WoolDirection.Right:WoolDirection.Left):(logical.y>0?WoolDirection.Up:WoolDirection.Down);
                if(crossbow.dock<crossbow.travelDuration)continue;
                if(!crossbow.smoking)
                {
                    crossbow.smoking=true;var trail=vehicle.GetComponent<TrailRenderer>();if(trail){trail.emitting=false;trail.Clear();}vehicle.gameObject.SetActive(false);
                    rig.arrowPool.Dock(i,level.Tiles[crossbow.index].color,world.TransformPoint(Slot(i)));
                }
                if(crossbow.dock<crossbow.travelDuration+.22f)continue;
                vehicle.gameObject.SetActive(false);crossbow.arrived=true;level.Tiles[crossbow.index].leaving=false;crossbow.arrivalOrder=++collectionSequence;rig.arrowPool.Reveal(i);
            }
            for(int slot=0;slot<active.Length;slot++)
            {
                var targetView=projectileTargets[slot];
                if(targetView&&InFog(targetView)){rig.arrowPool.CancelSlot(slot);projectileTargets[slot]=null;}
                int hit=rig.arrowPool.TickSlot(slot,dt);
                if(hit>=0&&active[slot]!=null&&projectileTargets[slot])
                {
                    int target=yarn.IndexOf(projectileTargets[slot]);var crossbow=active[slot];int color=projectileColors[slot];
                    if(target>=0&&!InFog(yarn[target])&&level.Yarn[target]==color&&yarnAmounts[target]>0)
                    {
                        yarnAmounts[target]=0;crossbow.ammo--;collected++;
                        if(TutorialGuidance&&slot==tutorialShotSlot){tutorialShotSlot=-1;tutorialFeedbackUntil=elapsed+.7f;hud.Hint("Great match!");}
                        WoolProgress.RecordDailyWool(color,1);
                        if(crossbow.ammo<=0){rig.arrowPool.Release(slot);active[slot]=null;}
                    }
                    projectileTargets[slot]=null;
                }
                if(!rig.arrowPool.SlotBusy(slot))projectileTargets[slot]=null;
            }
            for(int slot=0;slot<active.Length;slot++)
            {
                var crossbow=active[slot];if(crossbow==null||!crossbow.arrived||crossbow.ammo<=0||rig.arrowPool.SlotBusy(slot))continue;
                int target=FindTarget(crossbow,out _);if(target<0)continue;
                projectileTargets[slot]=yarn[target];projectileColors[slot]=level.Tiles[crossbow.index].color;
                rig.arrowPool.Launch(slot,projectileColors[slot],projectileTargets[slot]);
                if(!rig.arrowPool.SlotBusy(slot))projectileTargets[slot]=null;
            }
            for(int slot=0;slot<active.Length;slot++)if(rig.arrowPool.SlotBusy(slot)){simultaneousCollectors++;CollectingColorMask|=1<<projectileColors[slot];}
            if(collected>0)headDistance=Mathf.Max(rig.authoredLayout?0:definition.tutorial?1.5f:entryTarget,headDistance-collected*.22f);
            if(entering&&!definition.tutorial&&collected>0){entering=false;phase=Phase.Playing;}
            bool removed=false;
            for(int i=yarn.Count-1;i>=0;i--)if(yarnAmounts[i]<=.00001f)
            {yarn[i].gameObject.SetActive(false);yarn.RemoveAt(i);yarnRenderers.RemoveAt(i);yarnAmounts.RemoveAt(i);level.Yarn.RemoveAt(i);removed=true;}
            if(removed){hud.Status(level.Number,money,RemainingYarn,total);SaveTutorialState();}
            PositionYarn();UpdateCrossbowCounts();TickFlame(Mathf.Min(Time.deltaTime,.1f));
            float dangerEnd=rig.duckRescue?rig.duckRescue.MotherDistance-rig.duckRescue.approachDistance-.35f:rig.route.Length-1.25f;
            hud.SetDanger(!ended&&!suspended?Mathf.InverseLerp(dangerEnd-5f,dangerEnd,headDistance):0,elapsed);

            if(definition.tutorial&&tutorialShotSlot<0&&tutorialFeedbackUntil>0&&elapsed>=tutorialFeedbackUntil){tutorialFeedbackUntil=0;UpdateTutorial();}
            if(definition.tutorial&&tutorialHand.gameObject.activeSelf&&tutorialStep<level.Solution.Count)tutorialHand.localPosition=tileViews[level.Solution[tutorialStep]].localPosition+new Vector3(.35f,-.45f+Mathf.Sin(elapsed*5)*.08f,-1);
            if(RemainingYarn==0){Win();return;}
            PollVehiclePointer();
        }
        void PollVehiclePointer(){if(!automatedInput&&Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)Pointer(Mouse.current.position.ReadValue(),-1);else if(!automatedInput&&Touchscreen.current!=null&&Touchscreen.current.primaryTouch.press.wasPressedThisFrame)Pointer(Touchscreen.current.primaryTouch.position.ReadValue(),Touchscreen.current.primaryTouch.touchId.ReadValue());}
        readonly List<RaycastResult> pointerUIHits=new List<RaycastResult>();
        bool InteractiveUIAt(Vector2 screen)
        {
            if(!EventSystem.current)return false;
            var pointer=new PointerEventData(EventSystem.current){position=screen};
            pointerUIHits.Clear();EventSystem.current.RaycastAll(pointer,pointerUIHits);
            foreach(var hit in pointerUIHits)
                if(hit.gameObject.GetComponentInParent<UnityEngine.UI.Selectable>()||hit.gameObject.GetComponentInParent<UnityEngine.UI.ScrollRect>())return true;
            return false;
        }
        public bool TryGetLockedSlotAtScreenPoint(Vector2 screen,out int slot)
        {
            slot=-1;if(level==null||!rig.SlotCamera||!rig.SlotCamera.pixelRect.Contains(screen))return false;
            var camera=rig.SlotCamera;
            float spacing=Vector2.Distance(camera.WorldToScreenPoint(rig.slots[4].position),camera.WorldToScreenPoint(rig.slots[5].position));
            float halfWidth=spacing*.46f,halfHeight=Mathf.Min(camera.pixelHeight*.48f,spacing*.46f);
            for(int i=4;i<rig.slots.Length;i++)if(!SlotOpen(i))
            {
                Vector2 center=camera.WorldToScreenPoint(rig.slots[i].position);
                if(Mathf.Abs(screen.x-center.x)<=halfWidth&&Mathf.Abs(screen.y-center.y)<=halfHeight){slot=i;return true;}
            }
            return false;
        }
        void Pointer(Vector2 screen,int pointerId)
        {
            if(hud.ModalVisible||InteractiveUIAt(screen))return;
            if(rig.vehicleCamera&&!rig.vehicleCamera.pixelRect.Contains(screen)&&(!rig.slotCamera||!rig.slotCamera.pixelRect.Contains(screen)))return;
            if(TryGetLockedSlotAtScreenPoint(screen,out int requestedSlot)){OfferBoosterForSlot(0,requestedSlot);return;}
            if(Physics.Raycast(rig.CameraForScreenPoint(screen).ScreenPointToRay(screen),out var hit,100,(1<<LayerMask.NameToLayer("Vehicle View"))|(1<<LayerMask.NameToLayer("Slot View")),QueryTriggerInteraction.Ignore))
            {
                var slot=hit.collider.GetComponentInParent<WoolSlot>();if(slot){if(!SlotOpen(slot.Index))OfferBoosterForSlot(0,slot.Index);return;}
                for(int i=0;i<tileViews.Count;i++)
                    if(hit.transform==tileViews[i]||hit.transform.IsChildOf(tileViews[i])){SelectTile(i);return;}
            }
        }
        void BlockedVehicle(int index,string message)
        {
            gameAudio.Play(WoolGameAudio.Cue.Blocked);ShowVehicleTap(index);hud.Hint(message);
            if(rig.authoredLayout&&rig.authoredLayout.vehiclePool)rig.authoredLayout.vehiclePool.ShakeBlocked(index);
        }
        public bool SelectTile(int index)
        {
            if(level==null||index<0||index>=level.Tiles.Count||ended||paused||transition||hud.ModalVisible||hud.AdVisible||hud.AbilityGuideVisible||phase==Phase.Menu)return false;
            if(level.Tiles[index].selected||level.Tiles[index].hidden||(!removeMode&&VehicleIsProbing(index)))return false;
            if(removeMode){RemoveBox(index);return true;}
            if(TutorialGuidance){if(entering||tutorialShotSlot>=0||tutorialFeedbackUntil>0)return false;if(tutorialStep<level.Solution.Count&&index!=level.Solution[tutorialStep]){hud.Hint("Tap the glowing vehicle");return false;}}
            if(level.Tiles[index].source==WoolVehicleSource.Rail&&!HasFreeVehicleSlot){BlockedVehicle(index,"Wait for a free slot");return false;}
            if(level.Tiles[index].source==WoolVehicleSource.Rail&&((vehicleSources&&!vehicleSources.RailExitClear(index))||!level.CanSelect(index))){BlockedVehicle(index,"Wait for a clear exit");return false;}
            if(!level.CanSelect(index))
            {
                if(level.TryGetBlocker(index,out int blocker,out _)&&blocker>=0&&level.Tiles[blocker].probing){BlockedVehicle(index,"Wait for that vehicle to return. Clear its path first.");return false;}
                // Belt vehicles keep their shake above; stationary vehicles approach
                // the blocker and return through the existing contact movement.
                BeginContactMove(index);hud.Hint("Clear the path first");return false;
            }
            var tile=level.Tiles[index];Vector3 direction=tile.direction==WoolDirection.Up?Vector3.up:tile.direction==WoolDirection.Right?Vector3.right:tile.direction==WoolDirection.Down?Vector3.down:Vector3.left;
            direction=Quaternion.Euler(0,0,definition.boardRotation)*direction;
            Physics.SyncTransforms();
            bool left=direction.x<0||(Mathf.Abs(direction.x)<.5f&&tileViews[index].localPosition.x<0);
            int slot=ReserveSide(left);
            if(slot<0){gameAudio.Play(WoolGameAudio.Cue.Blocked);hud.Hint("Need to unlock more slots. Wait for a matching crossbow to finish.");return false;}
            BeginAttempt();tile.selected=true;tile.leaving=true;tile.carried=false;tile.transitDirection=tile.direction;
            ShowVehicleTap(index);
            var crossbow=new ActiveCrossbow {index=index,slot=slot,ammo=tile.ammo,initialAmmo=tile.ammo};
            crossbow.start=tileViews[index].localPosition;
            crossbow.startScale=tileViews[index].localScale;
            // Cross below the slot row, then approach only the reserved slot vertically.
            float laneY=rig.TravelTop;
            float sideX=left?-rig.TravelLeft:rig.TravelRight;
            float exitDistance=Mathf.Abs(direction.x)>.5f?(sideX-Mathf.Sign(direction.x)*crossbow.start.x)/Mathf.Abs(direction.x):((direction.y>0?laneY:rig.TravelBottom)-crossbow.start.y)/direction.y;
            crossbow.exit=crossbow.start+direction*Mathf.Max(.2f,exitDistance);
            crossbow.corridor=new Vector3(left?-sideX:sideX,laneY,crossbow.start.z);
            var approach=new Vector3(Slot(slot).x,laneY,crossbow.start.z);
            var corner=new Vector3(crossbow.corridor.x,crossbow.exit.y,crossbow.start.z);
            crossbow.path=direction.y>.5f
                ?new[]{crossbow.start,crossbow.exit,approach,Slot(slot)}
                :new[]{crossbow.start,crossbow.exit,corner,crossbow.corridor,approach,Slot(slot)};
            for(int p=1;p<crossbow.path.Length;p++)crossbow.pathLength+=Vector3.Distance(crossbow.path[p-1],crossbow.path[p]);
            crossbow.travelDuration=Mathf.Clamp(crossbow.pathLength/28f,.55f,.78f)/VehicleSpeedMultiplier;
            active[slot]=crossbow;
            if(vehicleSources)vehicleSources.NotifySlotAssigned(index,slot);
            var trail=tileViews[index].GetComponent<TrailRenderer>();if(trail)trail.emitting=true;
            phase=Phase.Playing;if(audioEnabled&&rig.departureSound)sound.PlayOneShot(rig.departureSound,.55f);
            if(TutorialGuidance){tutorialStep++;tutorialShotSlot=slot;tutorialHand.gameObject.SetActive(false);hud.Hint("Watch the colours match!");}SaveTutorialState();return true;
        }
        bool SlotOpen(int i)=>(unlockedSlotMask&(1<<i))!=0;
        int ReserveSide(bool left)
        {
            for(int n=0;n<4;n++){int i=left?n:3-n;if(active[i]==null)return i;}
            for(int n=0;n<4;n++){int i=left?4+n:7-n;if(SlotOpen(i)&&active[i]==null)return i;}
            return -1;
        }
        void UpdateTutorial()
        {
            if(!TutorialGuidance)return;
            string[] prompts={
                "1 / 6   Tap the glowing vehicle",
                "2 / 6   Match the next colour",
                "3 / 6   Tap to clear the way",
                "4 / 6   More arrows, more matches!",
                "5 / 6   Almost there!",
                "6 / 6   One last vehicle!",
                "Great work! Watch the rescue"
            };
            hud.Hint(prompts[Mathf.Min(tutorialStep,prompts.Length-1)]);
            tutorialHand.gameObject.SetActive(!entering&&tutorialShotSlot<0&&tutorialFeedbackUntil<=0&&tutorialStep<level.Solution.Count);
        }
        void ClearProjectileTargets(){Array.Clear(projectileTargets,0,projectileTargets.Length);}
        bool TargetReserved(Transform target,int slot){for(int i=0;i<projectileTargets.Length;i++)if(i!=slot&&projectileTargets[i]==target)return true;return false;}
        bool CollectorPrecedes(int candidate,int current)
        {
            var a=active[candidate];var b=active[current];
            return a.ammo!=b.ammo?a.ammo<b.ammo:a.arrivalOrder<b.arrivalOrder;
        }
        int FindTarget(ActiveCrossbow crossbow,out float distanceSquared)
        {
            int color=level.Tiles[crossbow.index].color,target=-1;
            // Exposed matching segments are eligible anywhere on the route.
            distanceSquared=float.PositiveInfinity;
            for(int i=0;i<level.Yarn.Count;i++)
            {
                if(yarnAmounts[i]<=.00001f||level.Yarn[i]!=color||!yarn[i].gameObject.activeSelf)continue;
                if(InFog(yarn[i])||TargetReserved(yarn[i],crossbow.slot))continue;
                Vector2 delta=(Vector2)(yarn[i].localPosition-Slot(crossbow.slot));
                float distance=delta.sqrMagnitude;
                if(distance>=distanceSquared)continue;
                distanceSquared=distance;target=i;
            }
            return target;
        }
        bool InFog(Transform segment)=>elapsed>=amplifyUntil&&(rig.authoredLayout?rig.authoredLayout.IsInFog(segment.position):!definition.tutorial&&segment.localPosition.y>=definition.fogAbove);
        void PositionYarn()
        {
            if(yarnProperties==null)yarnProperties=new MaterialPropertyBlock();
            float offset=.23f;
            for(int i=0;i<yarn.Count;i++)
            {
                float length=.43f*yarnAmounts[i],d=headDistance-offset-length*.5f;offset+=length;
                bool visible=d>0;yarn[i].gameObject.SetActive(visible);if(!visible)continue;
                Vector3 point=OnPath(d)+Vector3.back*.20f,tangent=OnPath(d+.02f)-OnPath(d-.02f);
                yarn[i].localPosition=point;yarn[i].localRotation=Quaternion.Euler(0,0,Mathf.Atan2(tangent.y,tangent.x)*Mathf.Rad2Deg+180f);
                yarn[i].localScale=new Vector3(length*1.45f,.82f,.65f);
                if(yarnRenderers[i].sharedMaterial.HasProperty(FollowId)){yarnProperties.SetFloat(FollowId,1);yarnProperties.SetVector(SectionId,new Vector4(d,length*1.45f,.82f,.65f));yarnRenderers[i].SetPropertyBlock(yarnProperties);}
            }
            float headPos=headDistance+.35f;
            dragonHead.gameObject.SetActive(headPos>0);
            dragonHead.localPosition=OnPath(headPos)+Vector3.back*.08f;
            Vector3 direction=OnPath(headPos+.08f)-OnPath(headPos-.08f);
            dragonHead.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
            if(rig.authoredLayout&&rig.authoredLayout.tailTip)
            {
                var tip=rig.authoredLayout.tailTip;float d=headDistance-(yarn.Count>0?offset:.8f);
                tip.gameObject.SetActive((yarn.Count>0||returningDragon)&&d>0);
                if(d>0){tip.localPosition=OnPath(d)+Vector3.back*.20f;var tangent=rig.route.Tangent(d);tip.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(tangent.y,tangent.x)*Mathf.Rad2Deg);}
            }
        }
        void UpdateGrid(){} // Blocked pieces retain their authored dimensions.
        readonly int[] displayedCounts=new int[WoolLevel.Slots];
        void UpdateCrossbowCounts()
        {
            for(int i=0;i<rig.counts.Length;i++)
            {
                int value=!SlotOpen(i)?-2:active[i]==null?-1:Mathf.CeilToInt(active[i].ammo);
                if(displayedCounts[i]==value)continue;displayedCounts[i]=value;
                rig.counts[i].fontSize=value<=-2?16f:12f;
                bool plus=rig.slotUnlockIcons!=null&&i<rig.slotUnlockIcons.Length&&rig.slotUnlockIcons[i];
                if(plus)rig.slotUnlockIcons[i].SetActive(false);
                rig.counts[i].text=value==-3?"VIP":value==-2?"AD +":value<0?"":value.ToString();
                rig.RefreshSlotLabelLayout();
            }
        }
        public void OfferBooster(int kind)=>OfferBoosterForSlot(kind,-1);
        void OfferBoosterForSlot(int kind,int requestedSlot)
        {
            if(level==null||kind<0||kind>=4||ended||paused||transition||removeMode||phase==Phase.Menu)return;
            if(!WoolBoosters.Available(kind,level.Number)){hud.Hint(WoolBoosters.Names[kind]+" unlocks at level "+WoolBoosters.UnlockLevels[kind]+".");return;}
            if(definition.tutorial){hud.Hint("First, learn to rescue the family with the highlighted vehicles.");return;}
            if(kind==0&&unlockedSlotMask==255){hud.Hint("All eight slots are unlocked.");return;}
            if(kind==3&&elapsed<amplifyUntil){hud.Hint("Amplify is already active.");return;}
            if(kind==1&&level.Tiles.TrueForAll(tile=>tile.selected)){hud.Hint("All boxes have already left the board.");return;}
            removeMode=false;paused=true;
            if(kind==0)
            {
                pendingSlot=requestedSlot;
                if(pendingSlot<0)for(int i=4;i<8;i++)if(!SlotOpen(i)){pendingSlot=i;break;}
                if(pendingSlot<0||SlotOpen(pendingSlot)){paused=false;return;}
            }
            if(WoolProgress.AbilityCount(kind)>0)
            {
                Action cancel=()=>{paused=false;hud.Hide();};
                hud.Show(WoolBoosters.Names[kind],WoolBoosters.Descriptions[kind],"USE POWER ("+WoolProgress.AbilityCount(kind)+")",()=>ActivateBooster(kind,0,true),"CANCEL",cancel,cancel);return;
            }
            hud.Show(WoolBoosters.Names[kind],(kind==0?"Unlock slot "+(pendingSlot+1)+" for this level.":WoolBoosters.Descriptions[kind])+"\nWatch a video or use coins.",WoolBoosters.Prices[kind]+" COINS",()=>ActivateBooster(kind,WoolBoosters.Prices[kind]),"WATCH VIDEO",()=>
            {
                WatchReward(()=>StoreRewardedAbility(kind),()=>{paused=false;hud.Hide();OfferBoosterForSlot(kind,requestedSlot);},WoolBoosters.Prices[kind],WoolBoosters.Names[kind]);
               },()=>{paused=false;hud.Hide();});

        }
        void ActivateBooster(int kind,int cost,bool spendUnlockPower=false)
        {
            money=WoolProgress.Coins;
            if(kind==0&&(pendingSlot<4||pendingSlot>=8||SlotOpen(pendingSlot)))return;
            if(money<cost){int returnSlot=kind==0?pendingSlot:-1;hud.ShowGameplayCoinShop(()=>{paused=false;hud.Hide();OfferBoosterForSlot(kind,returnSlot);});return;}
            if(spendUnlockPower&&kind!=1&&!WoolProgress.UseAbility(kind)){paused=false;hud.Hide();return;}
            paused=false;hud.Hide();
            rig.arrowPool.Cancel();ClearProjectileTargets();
            if(kind==1)
            {
                removeMode=true;pendingRemoveCost=cost;pendingRemoveOwned=spendUnlockPower;hud.Hint("Remove any box — tap your choice.");
                hud.SelectionMode(true,()=>{removeMode=false;pendingRemoveOwned=false;pendingRemoveCost=0;paused=false;hud.HideAbilityGuide();if(lessonKind==1){WoolProgress.SetRemovalLessonTargetPending(false);lessonKind=-1;lessonTarget=-1;}hud.SelectionMode(false,null);hud.Hint("Removal cancelled.");});return;
            }
            BeginAttempt();money-=cost;Save();gameAudio.PlayAbility(kind);
            if(kind==0)challengeSupportCompleted=true;
            int effectSlot=pendingSlot;
            if(kind==0){WoolProgress.RecordDailyUnlock();unlockedSlotMask|=1<<pendingSlot;pendingSlot=-1;UpdateCrossbowCounts();hud.Hint("A new crossbow slot is ready.");}
            else if(kind==2){WoolProgress.RecordDailyAbility(2);hud.Hint("Sorting dragon colours...");}
            else{amplifyUntil=elapsed+amplifySeconds;rig.arrowPool.AllowFogTargets=true;WoolProgress.RecordDailyAbility(3);hud.Hint("Amplify active — all dragon colors can be collected.");}
            hud.Status(level.Number,money,RemainingYarn,total);
            CompleteAbilityLesson(kind);SaveTutorialState();StartCoroutine(AbilityEffect(kind,effectSlot));
        }
        void RemoveBox(int index)
        {
            money=WoolProgress.Coins;
            if(!removeMode||transition||(pendingRemoveOwned&&WoolProgress.AbilityCount(1)<=0)||index<0||index>=level.Tiles.Count||level.Tiles[index].selected)return;
            if(money<pendingRemoveCost){bool wasPaused=paused;paused=true;hud.ShowGameplayCoinShop(()=>{paused=wasPaused;});return;}
            float available=0;for(int i=0;i<level.Yarn.Count;i++)if(level.Yarn[i]==level.Tiles[index].color)available+=yarnAmounts[i];if(available+.0001f<level.Tiles[index].ammo)return;
            if(pendingRemoveOwned)WoolProgress.UseAbility(1);pendingRemoveOwned=false;hud.RefreshAbilityInventory();
            contactMoves[index]=null;level.Tiles[index].probing=false;
            WoolProgress.RecordDailyAbility(1);BeginAttempt();money-=pendingRemoveCost;pendingRemoveCost=0;removeMode=false;Save();gameAudio.PlayAbility(1);
            hud.SelectionMode(false,null);CompleteAbilityLesson(1);StartCoroutine(RemoveVehicleEffect(index));
        }
        void SyncYarnViews()
        {
            // The rule mutation already compacted colors and quantities together.
            // Reuse the view pool independently; never trim a second amount here.
            while(yarn.Count>level.Yarn.Count){int last=yarn.Count-1;yarn[last].gameObject.SetActive(false);yarn.RemoveAt(last);yarnRenderers.RemoveAt(last);}
            if(yarnAmounts.Count!=level.Yarn.Count)throw new InvalidOperationException("Wool quantity bookkeeping is out of sync.");
            for(int i=0;i<yarn.Count;i++)rig.ColorSection(yarn[i],level.Yarn[i]);
            PositionYarn();
        }
        void ToggleGameplaySpeed()
        {
            if(level==null||ended||paused||transition||phase==Phase.Menu)return;
            gameplaySpeed=gameplaySpeed==1?2:1;hud.SetGameplaySpeed(gameplaySpeed);
        }
        void TogglePause(){if(ended||transition)return;paused=true;hud.ShowAudioSettings(()=>{paused=false;hud.Hide();},OpenHome);}
        void RequestRestart(){if(transition)return;paused=true;hud.Show("TRY A NEW APPROACH?","Restart this level with all your crossbows restored.","RESTART LEVEL",()=>{if(attemptStarted)WoolProgress.MarkFailed(level.Number);RestartCurrentLevel();},"KEEP PLAYING",()=>{paused=false;hud.Hide();});}
        void Win()
        {
            if(ended)return;ended=true;phase=Phase.Won;rig.SetDragonAnimation(0);hud.SetDanger(0,0);if(flame)flame.ResetBreath();

            newCompletion=WoolProgress.CompleteLevel(level.Number);money=WoolProgress.Coins;
            var checkpoint=WoolProgress.TutorialCheckpoint;if(checkpoint!=null&&checkpoint.level==level.Number)WoolProgress.ClearTutorialCheckpoint();
            winRewardGranted=WoolProgress.CanClaimCompletionReward(level.Number);
            winBonusClaimed=WoolProgress.HasClaimedCompletionBonus(level.Number);
            gameAudio.PlayOutcome(true);
            hud.Status(level.Number,money,RemainingYarn,total);StartCoroutine(Celebrate());
        }
        IEnumerator Celebrate()
        {
            hud.Hint("Duck family saved!");if(rig.confetti)rig.confetti.Play(true);
            transition=true;
            yield return ReturnDragon(true);
            rig.kitten.localPosition=kittenStart;
            transition=false;ShowWinReward();
            if(newCompletion&&level.Number>=3&&!automatedInput&&AdsMediation.AdsMediationManager.instance)AdsMediation.AdsMediationManager.instance.ShowInterstitial();
        }
        bool winRewardGranted;
        public void ResumeOnboardingReward()
        {
            LoadLevel(1);ended=true;phase=Phase.Won;entering=false;
            newCompletion=false;winRewardGranted=false;ShowWinReward();
        }
        void ShowWinReward()
        {
            if(level.Number==1&&WoolProgress.OnboardingStep==WoolOnboardingStep.Gameplay)WoolProgress.AdvanceOnboarding(WoolOnboardingStep.Coins);
            money=WoolProgress.Coins;
            int earned=WoolProgress.CanClaimCompletionReward(level.Number)?50:0;
            Action map=()=>{if(WoolProgress.OnboardingStep==WoolOnboardingStep.Coins)WoolProgress.AdvanceOnboarding(WoolOnboardingStep.Map);OpenHome();hud.ShowMap();};
            Action<Action> collect=destination=>{
                if(transition)return;
                transition=true;
                int collected=WoolProgress.ClaimCompletionReward(level.Number)?50:0;
                money=WoolProgress.Coins;
                if(collected>0)gameAudio.Play(WoolGameAudio.Cue.Coin);
                hud.FlyWinCoins(collected,WoolProgress.Coins,()=>{transition=false;destination();});
            };
            hud.ShowLevelComplete(earned,money,earned>0,
                ()=>collect(map),
                ()=>{
                    if(transition||!WoolProgress.CanClaimCompletionReward(level.Number))return;
                    WatchReward(()=>{
                        if(WoolProgress.ClaimCompletionBonus(level.Number)){
                            winBonusClaimed=true;money=WoolProgress.Coins;transition=true;
                            gameAudio.Play(WoolGameAudio.Cue.Coin);
                            hud.Status(level.Number,money,RemainingYarn,total);
                            hud.FlyWinCoins(earned+50,money,()=>{transition=false;map();},true);
                        }else ShowWinReward();
                    },ShowWinReward);
                },()=>{if(!transition)map();});
        }
        void OfferFreeCoins()
        {
            if(transition)return;
            // WATCH is already the player's consent. Go straight to MAX and keep the current page.
            WatchReward(()=>{money=(int)Math.Min(int.MaxValue,(long)WoolProgress.Coins+50);Save();gameAudio.Play(WoolGameAudio.Cue.Coin);hud.ShowCoinOptions();},()=>{hud.Hide();hud.ShowCoinOptions();});
        }
        void Lose(string reason)
        {
            if(ended)return;
            if(attemptStarted)WoolProgress.MarkFailed(level.Number);ended=true;hud.SetDanger(0,elapsed);defeatDistance=headDistance;phase=Phase.Lost;gameAudio.PlayOutcome(false);
            KittenCaught(reason);
        }
        void UpdateDragonAnimation(bool suspended)
        {
            if(phase==Phase.Won)return;
            var state=WoolDragonAnimationState.EyesAndMouthDemo;
            bool playing=phase==Phase.Ready||phase==Phase.Playing;
            float duckDistance=float.PositiveInfinity;
            if(playing&&rig.duckRescue)duckDistance=rig.duckRescue.MotherDistance-(headDistance+.35f);
            float attackDistance=rig.duckRescue?rig.duckRescue.approachDistance+1.25f:0;
            bool nearDuck=playing&&rig.duckRescue&&duckDistance<=attackDistance;
            bool approaching=playing&&rig.duckRescue&&duckDistance<=attackDistance+5f;
            if(nearDuck&&!dragonNearDuck)dragonAttackTime=0;
            else if(!nearDuck&&dragonNearDuck)
            {
                dragonAttackTime=1.75f;dragonFireStarted=false;
                if(flame)flame.ResetBreath();
            }
            dragonNearDuck=nearDuck;
            if(playing)
            {
                if(!suspended)dragonAttackTime+=Mathf.Min(Time.deltaTime,.1f)*gameplaySpeed;
                float period=nearDuck?1.8f:5f;
                float cycleTime=Mathf.Repeat(dragonAttackTime,period);
                float closeEnd=nearDuck?period:1.75f;
                if(cycleTime<1.15f)
                {
                    state=WoolDragonAnimationState.MouthOpen;
                    if(cycleTime>=.25f&&!dragonFireStarted&&!suspended)
                    {
                        if(flame)flame.Burst();
                        dragonFireStarted=true;
                    }
                }
                else if(cycleTime<closeEnd)
                {
                    state=WoolDragonAnimationState.MouthClosed;
                    if(dragonFireStarted){if(flame)flame.ResetBreath();dragonFireStarted=false;}
                }
                else
                {
                    state=approaching?WoolDragonAnimationState.MouthOpenClose:WoolDragonAnimationState.EyesAndMouthDemo;
                    if(dragonFireStarted){if(flame)flame.ResetBreath();dragonFireStarted=false;}
                }
            }
            else if(dragonFireStarted){if(flame)flame.ResetBreath();dragonFireStarted=false;}
            rig.AnimateDragon(state,suspended);
        }
        void TickFlame(float dt)
        {
            if(!flame)return;
            // The visible head is ahead of headDistance. Use its actual pose at bends.
            Vector3 mouth=dragonHead.TransformPoint(rig.models?rig.models.mouthOffset:new Vector3(.65f,0,-.3f));
            flame.Tick(dt,mouth,dragonHead.TransformDirection(Vector3.right),false);
        }
        void KittenCaught(string reason)
        {
            transition=true;hud.Hint("The dragon caught the duck family!");
            if(flame)flame.ResetBreath();
            rig.kitten.gameObject.SetActive(false);rig.kitten.localScale=kittenScale;
            transition=false;ShowDefeat(reason);
        }
        IEnumerator ReturnDragon(bool rescued)
        {
            returningDragon=true;
            float start=headDistance;
            if(rescued&&start<1.2f)
            {
                const float leadIn=.55f;
                for(float t=0;t<leadIn;t+=Time.deltaTime)
                {
                    headDistance=Mathf.Lerp(start,1.2f,Mathf.SmoothStep(0,1,t/leadIn));
                    PositionYarn();
                    if(rig.duckRescue)rig.duckRescue.Celebrate(Time.deltaTime);
                    yield return null;
                }
                headDistance=1.2f;PositionYarn();
            }
            start=headDistance;
            float duration=Mathf.Clamp(start/12f,1.8f,4f);
            for(float t=0;t<duration;t+=Time.deltaTime)
            {
                headDistance=Mathf.Lerp(start,-1f,Mathf.SmoothStep(0,1,t/duration));
                PositionYarn();
                if(rescued&&rig.duckRescue)rig.duckRescue.Celebrate(Time.deltaTime);
                yield return null;
            }
            headDistance=-1;PositionYarn();returningDragon=false;
            if(rig.authoredLayout&&rig.authoredLayout.tailTip)rig.authoredLayout.tailTip.gameObject.SetActive(false);
        }
        void ShowDefeat(string reason)
        {
            if(phase!=Phase.Lost)return;
            hud.BindLoseCoins(()=>{
                if(transition||revived||phase!=Phase.Lost)return;
                money=WoolProgress.Coins;
                money=WoolProgress.Coins;
                if(money<WoolLosePresentation.ClearCost){hud.ShowGameplayCoinShop(()=>ShowDefeat(reason));return;}
                money-=WoolLosePresentation.ClearCost;Save();
                BeginAnimatedRevive();
            });
            hud.ShowLose(!revived,()=>{
                if(transition||revived)return;
                WatchReward(BeginAnimatedRevive,null,WoolLosePresentation.ClearCost,"Continue");
            },()=>{if(transition)return;LoadLevel(level.Number);},OpenHome);
        }
        void BeginAnimatedRevive()
        {
            if(transition||revived||phase!=Phase.Lost)return;
            transition=true;hud.Hide();StartCoroutine(AnimateRevive());
        }
        IEnumerator AnimateRevive()
        {
            if(rig.duckRescue)rig.duckRescue.RestoreLastSafePoint();
            float retreat=Mathf.Min(defeatDistance,entryTarget+2.4f);if(rig.duckRescue)retreat=Mathf.Min(retreat,rig.duckRescue.MotherDistance-rig.duckRescue.approachDistance-1);
            retreat=Mathf.Max(0,retreat);float startDistance=headDistance,duration=Mathf.Clamp(Mathf.Abs(startDistance-retreat)/15,.8f,1.8f);
            // All slot contents remain authoritative until the presentation finishes.
            var starts=new Vector3[active.Length];var scales=new Vector3[active.Length];
            for(int i=0;i<active.Length;i++)if(active[i]!=null){starts[i]=tileViews[active[i].index].localPosition;scales[i]=tileViews[active[i].index].localScale;}
            for(float t=0;t<duration;t+=Time.unscaledDeltaTime){
                headDistance=Mathf.Lerp(startDistance,retreat,Mathf.SmoothStep(0,1,t/duration));PositionYarn();
                for(int i=0;i<active.Length;i++)if(active[i]!=null){float p=Mathf.Clamp01((t-i*.07f)/.72f);var v=tileViews[active[i].index];v.localPosition=starts[i]+new Vector3(Mathf.Sin(p*Mathf.PI)*.5f,p*p*3,0);v.localScale=scales[i]*Mathf.Lerp(1,0,Mathf.SmoothStep(0,1,p));v.localRotation=Quaternion.Euler(0,0,p*90);}
                yield return null;
            }
            headDistance=retreat;PositionYarn();
            ResumeAfterDefeat();
        }
        void ResumeAfterDefeat()
        {
            if(revived||phase!=Phase.Lost)return;
            StopAllCoroutines();
            if(rig.arrowPool)rig.arrowPool.ResetPool();
            ClearProjectileTargets();
            for(int slot=0;slot<active.Length;slot++){
                var bow=active[slot];if(bow==null)continue;
                int color=level.Tiles[bow.index].color;
                float remaining=Mathf.Max(0,bow.ammo);
                for(int i=level.Yarn.Count-1;i>=0&&remaining>.00001f;i--){
                    if(level.Yarn[i]!=color)continue;
                    float take=Mathf.Min(remaining,yarnAmounts[i]);
                    remaining-=take;yarnAmounts[i]-=take;
                    if(yarnAmounts[i]<=.00001f){level.Yarn.RemoveAt(i);yarnAmounts.RemoveAt(i);}
                }
                tileViews[bow.index].gameObject.SetActive(false);
                active[slot]=null;
            }
            revived=true;ended=paused=transition=removeMode=false;phase=Phase.Playing;grace=0;
            entering=false;returningDragon=false;dragonNearDuck=dragonFireStarted=false;dragonAttackTime=2;
            if(flame)flame.ResetBreath();hud.SetDanger(0,0);
            rig.kitten.localScale=kittenScale;rig.kitten.gameObject.SetActive(true);
            if(rig.duckRescue)rig.kitten.gameObject.SetActive(false);
            tutorialShotSlot=-1;tutorialFeedbackUntil=0;SyncYarnViews();UpdateCrossbowCounts();UpdateGrid();hud.Hide();hud.SelectionMode(false,null);UpdateTutorial();
            hud.Status(level.Number,money,RemainingYarn,total);gameAudio.Play(WoolGameAudio.Cue.Booster);
            if(RemainingYarn==0)Win();
        }
        void WatchReward(Action earned,Action returnToOffer=null,int rewardCoinValue=50,string rewardPurpose="reward")
        {
            transition=true;bool settled=false,challengeFallback=false;
            var ads=WoolServices.EnsureAds();
            ads.ShowRewardedVideo(message=>
            {
                if(!this||settled)return;settled=true;transition=false;earned();
            },message=>
            {
                if(!this||settled)return;settled=true;transition=false;
                hud.Show("VIDEO CLOSED",message,"BACK",BackToRewardOffer);
            },message=>
            {
                if(!this||settled)return;settled=true;transition=false;
                int coins=WoolProgress.GrantUnavailableAdCoins(rewardCoinValue);money=WoolProgress.Coins;
                challengeFallback=AllowChallengeAdFallback();
                if(coins>0)gameAudio.Play(WoolGameAudio.Cue.Coin);
                hud.Status(level==null?WoolProgress.CurrentLevel:level.Number,money,RemainingYarn,total);
                string fallbackMessage=coins>0?"Here are "+coins+" coins (30% of the "+rewardPurpose+" value).\nFree rewards left today: "+WoolProgress.UnavailableAdRewardsRemaining+" / "+WoolProgress.UnavailableAdDailyLimit:"Today's 3 free no-video rewards are used.\nCome back tomorrow or try a video later.";
                hud.Show(coins>0?"NO VIDEO AVAILABLE":"VIDEO UNAVAILABLE",fallbackMessage,coins>0?"COLLECT":"OK",BackToRewardOffer);
            });
            void BackToRewardOffer(){if(challengeFallback){paused=false;hud.Hide();hud.Hint("No video available. Keep playing this challenge.");}else if(returnToOffer!=null)returnToOffer();else if(phase==Phase.Won)ShowWinReward();else if(phase==Phase.Lost)ShowDefeat("Try again or continue.");else hud.Hide();}
        }
        void PlayDuckCaught(){gameAudio.Play(WoolGameAudio.Cue.DuckQuack);}
        void Save(){WoolProgress.SetCoins(money);}
        void OnApplicationPause(bool suspended){if(suspended&&WoolProgress.IsLoaded){SaveTutorialState();WoolProgress.Flush();}}
        void OnApplicationQuit(){if(WoolProgress.IsLoaded){SaveTutorialState();WoolProgress.Flush();}}
        void OnDestroy()
        {
            if(rig&&rig.duckRescue)rig.duckRescue.DuckCaught-=PlayDuckCaught;
            if(hud&&AdsMediation.AdsMediationManager.instance){AdsMediation.AdsMediationManager.instance.AdVisibilityChanged-=hud.SetAdVisible;AdsMediation.AdsMediationManager.instance.AdLoadingChanged-=hud.SetAdLoading;}
            if(hud){hud.PausePressed-=TogglePause;hud.BoosterPressed-=OfferBooster;hud.SpeedPressed-=ToggleGameplaySpeed;hud.PlayPressed-=PlaySaved;hud.FreeCoinsPressed-=OfferFreeCoins;hud.LevelSelected-=OpenCampaignLevel;}
            if(generatedDefinition)Destroy(generatedDefinition);
            if(automatedInputValue||inspectorPreview){automatedInputValue=false;inspectorPreview=false;WoolProgress.EndPreview();}
        }
    }
}

