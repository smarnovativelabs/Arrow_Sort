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
        [SerializeField,Min(1)] float amplifySeconds=15;
        [SerializeField,Min(.1f),Tooltip("Maximum yarn reach in board units. Only matching exposed segments inside this range are eligible.")] float collectionReach=8.5f;
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
        Transform projectileTarget;
        int projectileColor;
        float shotCooldown;
        readonly float[] bumpTime=new float[WoolEndlessGenerator.MaxVehicles];
        readonly Vector3[] bumpStart=new Vector3[WoolEndlessGenerator.MaxVehicles],bumpDirection=new Vector3[WoolEndlessGenerator.MaxVehicles];
        Vector3 kittenStart,dragonScale,kittenScale;
        int money,total,tutorialStep,unlockedSlotMask,sortUses;
        int tutorialShotSlot=-1;
        float tutorialFeedbackUntil;
        float headDistance,elapsed,amplifyUntil,grace,entryTarget;
        bool entering;
        float nextDailySave;
        public float ApproachSpeedMultiplier=>entering?2:1;
        bool paused,ended,transition,removeMode,winBonusClaimed,revived,attemptStarted,newCompletion;
        int gameplaySpeed=1;
        public int GameplaySpeed=>gameplaySpeed;
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
            public Vector3 start,exit,corridor;
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
            hud.InitializeAuthored();gameAudio=GetComponent<WoolGameAudio>();if(!gameAudio)gameAudio=gameObject.AddComponent<WoolGameAudio>();gameAudio.Initialize(rig,audioEnabled);sound=rig.audioSource;hud.PausePressed+=TogglePause;hud.BoosterPressed+=OfferBooster;hud.SpeedPressed+=ToggleGameplaySpeed;
            hud.PlayPressed+=PlaySaved;hud.FreeCoinsPressed+=OfferFreeCoins;hud.LevelSelected+=OpenCampaignLevel;
            WoolProgress.Load(WoolEndlessGenerator.MaxLevel);WoolProgress.InitializeOnboarding();money=WoolProgress.Coins;
            if(!automatedInput){var ads=WoolServices.EnsureAds();ads.AdVisibilityChanged+=hud.SetAdVisible;}
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
            if(rig.arrowPool)rig.arrowPool.ResetPool();
            hud.SetDanger(0,0);if(flame)flame.ResetBreath();
            if(attemptStarted&&!ended)WoolProgress.MarkFailed(level.Number);
            if(gameAudio)gameAudio.SetGameplayMusic(false);
            StopAllCoroutines();paused=false;removeMode=false;phase=Phase.Menu;world.gameObject.SetActive(false);if(rig.authoredLayout)rig.authoredLayout.environment.SetActive(false);
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
            gameAudio.Play(WoolGameAudio.Cue.Start);StopAllCoroutines();if(flame)flame.ResetBreath();rig.kitten.gameObject.SetActive(true);hud.EndLaunch();hud.SetDanger(0,0);definition=ResolveLevel(number);definition.advanceSpeed*=1.35f;
            lessonKind=-1;lessonTarget=-1;hud.HideAbilityGuide();hud.HideAbilityEffect();WoolProgress.GrantAbilityGifts(number);
            money=WoolProgress.Coins;level=new WoolLevel(definition);total=level.Yarn.Count;for(int c=0;c<displayedCounts.Length;c++)displayedCounts[c]=-99;winBonusClaimed=revived=attemptStarted=newCompletion=false;simultaneousCollectors=0;
            if(total>rig.yarn.Length||level.Tiles.Count>rig.vehicles.Length)throw new InvalidOperationException("Level exceeds its authored pool capacity.");
            rig.ResetPool();if(rig.arrowPool)rig.arrowPool.ResetPool();projectileTarget=null;shotCooldown=0;Array.Clear(bumpTime,0,bumpTime.Length);world.gameObject.SetActive(true);hud.CloseMenu();
            Array.Clear(active,0,active.Length);yarn.Clear();yarnRenderers.Clear();yarnAmounts.Clear();tileViews.Clear();rig.ApplyEnvironment(definition);BuildPath();
            definition.advanceSpeed*=Mathf.Lerp(1f,1.3f,Mathf.InverseLerp(24,65,rig.route.Length));
            elapsed=amplifyUntil=0;collectionSequence=0;grace=0;paused=ended=transition=removeMode=false;
            tutorialShotSlot=-1;tutorialFeedbackUntil=0;
            pendingRemoveOwned=false;pendingRemoveCost=0;
            tutorialStep=sortUses=0;unlockedSlotMask=15|(rig.vipSlotEntitled?16:0);pendingSlot=-1;returningDragon=false;dragonAttackTime=2f;dragonNearDuck=dragonFireStarted=false;gameplaySpeed=1;hud.SetGameplaySpeed(1);phase=Phase.Ready;
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
            BeginAttempt();
            for(int i=0;i<total;i++){yarn.Add(rig.yarn[i]);yarnRenderers.Add(rig.yarn[i].GetComponent<Renderer>());yarnAmounts.Add(1);rig.SetSectionModel(i,level.Yarn[i]);}
            for(int i=0;i<level.Tiles.Count;i++)
            {
                rig.SetVehicle(i,level.Tiles[i],definition.boardRotation,definition.boardScale);
                tileViews.Add(rig.vehicles[i]);
            }
            if(!vehicleSources)vehicleSources=gameObject.GetComponent<WoolVehicleSources>()??gameObject.AddComponent<WoolVehicleSources>();
            vehicleSources.Load(rig,level,definition);
            rig.CompactParking(vehicleSources);rig.ApplyLayout(definition.tutorial);
            rig.kitten.localPosition=OnPath(rig.route.Length-.7f)+new Vector3(0,0,-.4f);
            kittenStart=rig.kitten.localPosition;dragonHead.gameObject.SetActive(true);dragonHead.localScale=dragonScale;
            if(rig.duckRescue){rig.kitten.gameObject.SetActive(false);rig.duckRescue.ResetForLevel(rig.route,world,rig.authoredLayout);}
            if(rig.fog)rig.fog.gameObject.SetActive(rig.authoredLayout||!definition.tutorial);
            tutorialHand.gameObject.SetActive(definition.tutorial);
            Physics.SyncTransforms();
            PositionYarn();hud.Hide();hud.Status(level.Number,money,RemainingYarn,total);hud.Tutorial(definition.tutorial);UpdateCrossbowCounts();UpdateTutorial();rig.CapturePortraitBounds(definition);rig.FitCamera();
            if(!definition.tutorial)hud.Hint("Can't collect wool in the fog. Choose your colors carefully.");
            if(definition.containerPositions!=null&&definition.containerPositions.Length>0&&!WoolProgress.WarehouseTutorialSeen)hud.ShowWarehouseTutorial();
            hud.SetAbilityLevels(number);StartCoroutine(BeginAbilityLesson());
        }
        void Update()
        {
            bool suspended=paused||transition||(hud&&(hud.ModalVisible||hud.AdVisible||hud.AbilityGuideVisible));
            if(phase==Phase.Won&&!transition&&rig.duckRescue&&!hud.AdVisible)rig.duckRescue.Celebrate(Mathf.Min(Time.unscaledDeltaTime,.1f));
            if(!suspended&&rig.arrowPool)rig.arrowPool.TickPresentation(Mathf.Min(Time.deltaTime,.1f)*gameplaySpeed);
            UpdateDragonAnimation(suspended);
            gameAudio.SetFireActive(flame&&flame.Bursting&&rig.DragonMouthOpen&&!suspended);
            if(level==null||phase==Phase.Menu)return;rig.FitCamera();
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
                else { grace-=dt;if(grace<=0)headDistance+=dt*definition.advanceSpeed; }
                if(definition.tutorial)headDistance=Mathf.Min(headDistance,rig.route.Length-1.5f);
                if(rig.duckRescue)rig.duckRescue.Tick(dt,headDistance,definition.advanceSpeed);
                if(!definition.tutorial&&(rig.duckRescue?rig.duckRescue.BothCaught:headDistance>=rig.route.Length-.7f)){Lose("The dragon caught the duck family at the end of the path. Try a different box order.");return;}
            }
            if(vehicleSources)vehicleSources.Tick(dt);
            PositionYarn();simultaneousCollectors=0;CollectingColorMask=0;float collected=0;
            for(int i=0;i<tileViews.Count;i++)if(bumpTime[i]>0)
            {
                bumpTime[i]=Mathf.Max(0,bumpTime[i]-dt);
                tileViews[i].localPosition=bumpStart[i]+bumpDirection[i]*Mathf.Sin((1-bumpTime[i]/.28f)*Mathf.PI)*.12f;
            }
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
                if(crossbow.dock<crossbow.travelDuration)continue;
                if(!crossbow.smoking)
                {
                    crossbow.smoking=true;var trail=vehicle.GetComponent<TrailRenderer>();if(trail)trail.emitting=false;vehicle.gameObject.SetActive(false);
                    rig.arrowPool.Dock(i,level.Tiles[crossbow.index].color,world.TransformPoint(Slot(i)));
                }
                if(crossbow.dock<crossbow.travelDuration+.22f)continue;
                crossbow.arrived=true;crossbow.arrivalOrder=++collectionSequence;rig.arrowPool.Reveal(i);
            }
            if(projectileTarget&&InFog(projectileTarget)){rig.arrowPool.Cancel();projectileTarget=null;}
            int hit=rig.arrowPool.Tick(dt);
            if(hit>=0&&active[hit]!=null&&projectileTarget)
            {
                int target=yarn.IndexOf(projectileTarget);var crossbow=active[hit];
                if(target>=0&&!InFog(yarn[target])&&level.Yarn[target]==projectileColor&&yarnAmounts[target]>0)
                {
                    yarnAmounts[target]=0;crossbow.ammo-=1;collected=1;
                    if(definition.tutorial&&hit==tutorialShotSlot){tutorialShotSlot=-1;tutorialFeedbackUntil=elapsed+1.2f;hud.Hint("Direct hit! Matching arrows damage the dragon.");}
                    WoolProgress.RecordDailyWool(projectileColor,1);
                    if(crossbow.ammo<=0){rig.arrowPool.Release(hit);active[hit]=null;}
                }
                projectileTarget=null;shotCooldown=0; // The 0.08 second bow draw replaces the old idle shot delay.
            }
            shotCooldown=Mathf.Max(0,shotCooldown-dt);
            if(!rig.arrowPool.Busy&&shotCooldown<=0)
            {
                int chosen=-1,target=-1;
                for(int i=0;i<active.Length;i++)
                {
                    var crossbow=active[i];if(crossbow==null||!crossbow.arrived||crossbow.ammo<=0)continue;
                    int candidate=FindTarget(crossbow,out float distance);if(candidate<0)continue;
                    if(chosen<0||CollectorPrecedes(i,chosen))
                    {chosen=i;target=candidate;}
                }
                if(chosen>=0)
                {
                    projectileTarget=yarn[target];projectileColor=level.Tiles[active[chosen].index].color;
                    rig.arrowPool.Launch(chosen,projectileColor,projectileTarget);
                }
            }
            if(rig.arrowPool.Busy){simultaneousCollectors=1;CollectingColorMask=1<<projectileColor;}
            if(collected>0)headDistance=Mathf.Max(rig.authoredLayout?0:definition.tutorial?1.5f:entryTarget,headDistance-collected*.22f);
            if(entering&&!definition.tutorial&&collected>0){entering=false;phase=Phase.Playing;}
            bool removed=false;
            for(int i=yarn.Count-1;i>=0;i--)if(yarnAmounts[i]<=.00001f)
            {yarn[i].gameObject.SetActive(false);yarn.RemoveAt(i);yarnRenderers.RemoveAt(i);yarnAmounts.RemoveAt(i);level.Yarn.RemoveAt(i);removed=true;}
            if(removed)hud.Status(level.Number,money,RemainingYarn,total);
            PositionYarn();UpdateCrossbowCounts();TickFlame(Mathf.Min(Time.deltaTime,.1f));
            hud.SetDanger(Mathf.InverseLerp(rig.route.Length*.73f,rig.route.Length-1.25f,headDistance),elapsed);

            if(definition.tutorial&&tutorialShotSlot<0&&tutorialFeedbackUntil>0&&elapsed>=tutorialFeedbackUntil){tutorialFeedbackUntil=0;UpdateTutorial();}
            if(definition.tutorial&&tutorialHand.gameObject.activeSelf&&tutorialStep<level.Solution.Count)tutorialHand.localPosition=tileViews[level.Solution[tutorialStep]].localPosition+new Vector3(.35f,-.45f+Mathf.Sin(elapsed*5)*.08f,-1);
            if(RemainingYarn==0){Win();return;}
            if(!automatedInput&&Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)Pointer(Mouse.current.position.ReadValue(),-1);
            else if(!automatedInput&&Touchscreen.current!=null&&Touchscreen.current.primaryTouch.press.wasPressedThisFrame)Pointer(Touchscreen.current.primaryTouch.position.ReadValue(),Touchscreen.current.primaryTouch.touchId.ReadValue());
        }
        void Pointer(Vector2 screen,int pointerId)
        {
            if(hud.ModalVisible||EventSystem.current&&EventSystem.current.IsPointerOverGameObject(pointerId))return;
            if(rig.vehicleCamera&&!rig.vehicleCamera.pixelRect.Contains(screen))return;
            if(Physics.Raycast(rig.CameraForScreenPoint(screen).ScreenPointToRay(screen),out var hit,100,1<<LayerMask.NameToLayer("Vehicle View"),QueryTriggerInteraction.Ignore))
            {
                var slot=hit.collider.GetComponent<WoolSlot>();if(slot){if(!SlotOpen(slot.Index))OfferBoosterForSlot(0,slot.Index);return;}
                for(int i=0;i<tileViews.Count;i++)
                    if(hit.transform==tileViews[i]||hit.transform.IsChildOf(tileViews[i])){SelectTile(i);return;}
            }
        }
        public bool SelectTile(int index)
        {
            if(level==null||index<0||index>=level.Tiles.Count||ended||paused||transition||hud.ModalVisible||hud.AdVisible||hud.AbilityGuideVisible||phase==Phase.Menu)return false;
            if(level.Tiles[index].selected||level.Tiles[index].hidden||bumpTime[index]>0)return false;
            if(removeMode){RemoveBox(index);return true;}
            if(definition.tutorial){if(entering||tutorialShotSlot>=0||tutorialFeedbackUntil>0)return false;if(tutorialStep<level.Solution.Count&&index!=level.Solution[tutorialStep]){hud.Hint("Follow the pointing hand. Tap that vehicle to try this lesson.");return false;}}
            if(!level.CanSelect(index))
            {
                var blocked=level.Tiles[index];bumpStart[index]=tileViews[index].localPosition;
                var dir=blocked.direction==WoolDirection.Up?Vector3.up:blocked.direction==WoolDirection.Down?Vector3.down:blocked.direction==WoolDirection.Left?Vector3.left:Vector3.right;
                bumpDirection[index]=Quaternion.Euler(0,0,definition.boardRotation)*dir;bumpTime[index]=.28f;
                gameAudio.Play(WoolGameAudio.Cue.Blocked);hud.Hint("Another vehicle blocks this exit.");return false;
            }
            var tile=level.Tiles[index];Vector3 direction=tile.direction==WoolDirection.Up?Vector3.up:tile.direction==WoolDirection.Right?Vector3.right:tile.direction==WoolDirection.Down?Vector3.down:Vector3.left;
            direction=Quaternion.Euler(0,0,definition.boardRotation)*direction;
            Physics.SyncTransforms();
            bool left=direction.x<0||(Mathf.Abs(direction.x)<.5f&&tileViews[index].localPosition.x<0);
            int slot=ReserveSide(left);
            if(slot<0){gameAudio.Play(WoolGameAudio.Cue.Blocked);hud.Hint("Need to unlock more slots. Wait for a matching crossbow to finish.");return false;}
            BeginAttempt();tile.selected=true;
            var crossbow=new ActiveCrossbow {index=index,slot=slot,ammo=tile.ammo,initialAmmo=tile.ammo};
            crossbow.start=tileViews[index].localPosition;
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
            crossbow.travelDuration=Mathf.Max(.7f,crossbow.pathLength/10);
            active[slot]=crossbow;
            var trail=tileViews[index].GetComponent<TrailRenderer>();if(trail)trail.emitting=true;
            phase=Phase.Playing;if(audioEnabled&&rig.departureSound)sound.PlayOneShot(rig.departureSound,.18f);
            if(definition.tutorial){tutorialStep++;tutorialShotSlot=slot;tutorialHand.gameObject.SetActive(false);hud.Hint("Watch: the vehicle docks, becomes a crossbow, and fires matching arrows.");}return true;
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
            if(!definition.tutorial)return;
            string[] prompts={
                "1/6  Tap the purple vehicle. Its crossbow fires at purple dragon sections.",
                "2/6  Follow the arrow: tap the red vehicle to clear the blue vehicle's exit.",
                "3/6  The blue vehicle now has a clear path. Tap it to launch its crossbow.",
                "4/6  Bigger vehicles carry more arrows: small 4, medium 6, large 10.",
                "5/6  Rescue the duck family before the dragon reaches them. You're safe while learning.",
                "6/6  Tap the last vehicle. Clear every color to finish the rescue!",
                "All vehicles launched! Watch your crossbows finish the rescue."
            };
            hud.Hint(prompts[Mathf.Min(tutorialStep,prompts.Length-1)]);
            tutorialHand.gameObject.SetActive(!entering&&tutorialShotSlot<0&&tutorialFeedbackUntil<=0&&tutorialStep<level.Solution.Count);
        }
        bool CollectorPrecedes(int candidate,int current)
        {
            var a=active[candidate];var b=active[current];
            return a.ammo!=b.ammo?a.ammo<b.ammo:a.arrivalOrder<b.arrivalOrder;
        }
        int FindTarget(ActiveCrossbow crossbow,out float distanceSquared)
        {
            int color=level.Tiles[crossbow.index].color,target=-1;
            distanceSquared=collectionReach*collectionReach*(elapsed<amplifyUntil?4:1);
            for(int i=0;i<level.Yarn.Count;i++)
            {
                if(yarnAmounts[i]<=.00001f||level.Yarn[i]!=color||!yarn[i].gameObject.activeSelf)continue;
                if(InFog(yarn[i]))continue;
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
                rig.counts[i].text=value==-3?"VIP":value==-2?"+":value<0?"":value.ToString();
                rig.RefreshSlotLabelLayout();
            }
        }
        public void OfferBooster(int kind)=>OfferBoosterForSlot(kind,-1);
        void OfferBoosterForSlot(int kind,int requestedSlot)
        {
            if(level==null||kind<0||kind>=4||ended||paused||transition||phase==Phase.Menu)return;
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
                WatchReward(()=>ActivateBooster(kind,0),()=>{paused=false;hud.Hide();OfferBoosterForSlot(kind,requestedSlot);});
               },()=>{paused=false;hud.Hide();});
        }
        void ActivateBooster(int kind,int cost,bool spendUnlockPower=false)
        {
            money=WoolProgress.Coins;
            if(kind==0&&(pendingSlot<4||pendingSlot>=8||SlotOpen(pendingSlot)))return;
            if(money<cost){hud.Show("MORE COINS NEEDED","This power needs "+cost+" coins. You have "+money+".","BACK",()=>{paused=false;hud.Hide();OfferBoosterForSlot(kind,kind==0?pendingSlot:-1);},"HOME",OpenHome);return;}
            if(spendUnlockPower&&kind!=1&&!WoolProgress.UseAbility(kind)){paused=false;hud.Hide();return;}
            paused=false;hud.Hide();
            rig.arrowPool.Cancel();projectileTarget=null;
            if(kind==1)
            {
                removeMode=true;pendingRemoveCost=cost;pendingRemoveOwned=spendUnlockPower;hud.Hint("Remove any box — tap your choice.");
                hud.SelectionMode(true,()=>{removeMode=false;pendingRemoveOwned=false;hud.SelectionMode(false,null);hud.Hint("Removal cancelled.");});if(lessonKind==1)ShowRemovalLessonTarget();return;
            }
            BeginAttempt();money-=cost;Save();gameAudio.PlayAbility(kind);
            int effectSlot=pendingSlot;
            if(kind==0){WoolProgress.RecordDailyUnlock();unlockedSlotMask|=1<<pendingSlot;pendingSlot=-1;UpdateCrossbowCounts();hud.Hint("A new crossbow slot is ready.");}
            else if(kind==2){level.SortColors(++sortUses,yarnAmounts);WoolProgress.RecordDailyAbility(2);SyncYarnViews();hud.Hint("Dragon body colors refreshed.");}
            else{amplifyUntil=elapsed+amplifySeconds;WoolProgress.RecordDailyAbility(3);hud.Hint("Amplify active — all dragon colors can be collected.");}
            hud.Status(level.Number,money,RemainingYarn,total);
            if(kind==2)BringActiveColorForward();CompleteAbilityLesson(kind);StartCoroutine(AbilityEffect(kind,effectSlot));
        }
        void RemoveBox(int index)
        {
            money=WoolProgress.Coins;
            if(!removeMode||money<pendingRemoveCost||(pendingRemoveOwned&&WoolProgress.AbilityCount(1)<=0)||!level.RemoveBox(index,yarnAmounts))return;
            if(pendingRemoveOwned)WoolProgress.UseAbility(1);pendingRemoveOwned=false;hud.RefreshAbilityInventory();
            WoolProgress.RecordDailyAbility(1);BeginAttempt();money-=pendingRemoveCost;pendingRemoveCost=0;removeMode=false;Save();gameAudio.PlayAbility(1);
            SyncYarnViews();UpdateGrid();hud.SelectionMode(false,null);CompleteAbilityLesson(1);StartCoroutine(RemoveVehicleEffect(index));
            hud.Status(level.Number,money,RemainingYarn,total);hud.Hint("Box removed and its matching wool cleared.");
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
        void RequestRestart(){if(transition)return;paused=true;hud.Show("TRY A NEW APPROACH?","Restart this level with all your crossbows restored.","RESTART LEVEL",()=>{if(attemptStarted)WoolProgress.MarkFailed(level.Number);LoadLevel(level.Number);},"KEEP PLAYING",()=>{paused=false;hud.Hide();});}
        void Win()
        {
            if(ended)return;ended=true;phase=Phase.Won;rig.SetDragonAnimation(0);hud.SetDanger(0,0);if(flame)flame.ResetBreath();
            winCoinsPresented=false;newCompletion=WoolProgress.CompleteLevel(level.Number);money=WoolProgress.Coins;
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
        bool winCoinsPresented;
        public void ResumeOnboardingReward()
        {
            LoadLevel(1);ended=true;phase=Phase.Won;entering=false;
            newCompletion=false;winCoinsPresented=true;ShowWinReward();
        }
        void ShowWinReward()
        {
            if(level.Number==1&&WoolProgress.OnboardingStep==WoolOnboardingStep.Gameplay)WoolProgress.AdvanceOnboarding(WoolOnboardingStep.Coins);
            money=WoolProgress.Coins;
            int earned=newCompletion&&!winCoinsPresented?50:0;
            Action map=()=>{if(WoolProgress.OnboardingStep==WoolOnboardingStep.Coins)WoolProgress.AdvanceOnboarding(WoolOnboardingStep.Map);OpenHome();hud.ShowMap();};
            Action<Action> collect=destination=>{
                if(transition)return;
                transition=true;winCoinsPresented=true;
                if(earned>0)gameAudio.Play(WoolGameAudio.Cue.Coin);
                hud.FlyWinCoins(earned,WoolProgress.Coins,()=>{transition=false;destination();});
            };
            hud.ShowLevelComplete(earned,money,newCompletion&&!winBonusClaimed,
                ()=>collect(map),
                ()=>{
                    if(transition||winBonusClaimed||!newCompletion)return;
                    WatchReward(()=>{
                        if(WoolProgress.ClaimCompletionBonus(level.Number)){
                            winBonusClaimed=true;winCoinsPresented=true;money=WoolProgress.Coins;transition=true;
                            gameAudio.Play(WoolGameAudio.Cue.Coin);
                            hud.Status(level.Number,money,RemainingYarn,total);
                            hud.FlyWinCoins(earned+50,money,()=>{transition=false;map();},true);
                        }else ShowWinReward();
                    },ShowWinReward);
                },()=>collect(map));
        }
        void OfferFreeCoins()
        {
            if(transition)return;
            hud.ShowReward("A LITTLE GIFT","Watch a short video to receive 50 coins.","BACK",hud.Hide,null,null,"WATCH VIDEO  ·  +50",()=>
            {
                if(transition)return;
                WatchReward(()=>{money=WoolProgress.Coins+50;Save();gameAudio.Play(WoolGameAudio.Cue.Coin);hud.Home(WoolProgress.CurrentLevel,money);hud.Show("GIFT RECEIVED","50 coins added to your wallet.","CONTINUE",hud.Hide);});
                  });
        }
        void Lose(string reason)
        {
            if(ended){ShowDefeat(reason);return;}
            if(attemptStarted)WoolProgress.MarkFailed(level.Number);ended=true;defeatDistance=headDistance;phase=Phase.Lost;gameAudio.PlayOutcome(false);
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
            Vector3 direction=(OnPath(headDistance+.08f)-OnPath(headDistance-.08f)).normalized;
            Vector3 mouth=world.TransformPoint(dragonHead.localPosition+direction*.65f+Vector3.back*.3f);
            if(rig.models)mouth=dragonHead.TransformPoint(rig.models.mouthOffset);
            flame.Tick(dt,mouth,world.TransformDirection(direction),false);
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
            hud.BindLoseCoins(()=>{
                if(transition||revived||phase!=Phase.Lost)return;
                money=WoolProgress.Coins;
                money=WoolProgress.Coins;
                if(money<WoolLosePresentation.ClearCost){ShowDefeat(reason);return;}
                money-=WoolLosePresentation.ClearCost;Save();
                BeginAnimatedRevive();
            });
            hud.ShowLose(!revived,()=>{
                if(transition||revived)return;
                WatchReward(BeginAnimatedRevive);
            },()=>{if(transition)return;LoadLevel(level.Number);},OpenHome);
        }
        void BeginAnimatedRevive()
        {
            if(transition||revived||phase!=Phase.Lost)return;
            transition=true;hud.Hide();StartCoroutine(AnimateRevive());
        }
        IEnumerator AnimateRevive()
        {
            // All slot contents remain authoritative until the presentation finishes.
            var starts=new Vector3[active.Length];var scales=new Vector3[active.Length];
            for(int i=0;i<active.Length;i++)if(active[i]!=null){starts[i]=tileViews[active[i].index].localPosition;scales[i]=tileViews[active[i].index].localScale;}
            for(float t=0;t<1.25f;t+=Time.unscaledDeltaTime){
                for(int i=0;i<active.Length;i++)if(active[i]!=null){float p=Mathf.Clamp01((t-i*.07f)/.72f);var v=tileViews[active[i].index];v.localPosition=starts[i]+new Vector3(Mathf.Sin(p*Mathf.PI)*.5f,p*p*3,0);v.localScale=scales[i]*Mathf.Lerp(1,0,Mathf.SmoothStep(0,1,p));v.localRotation=Quaternion.Euler(0,0,p*90);}
                yield return null;
            }
            yield return ReturnDragon(false);
            float emerge=headDistance;
            for(float t=0;t<.8f;t+=Time.unscaledDeltaTime){headDistance=Mathf.Lerp(emerge,.8f,Mathf.SmoothStep(0,1,t/.8f));PositionYarn();yield return null;}
            ResumeAfterDefeat();
        }
        void ResumeAfterDefeat()
        {
            if(revived||phase!=Phase.Lost)return;
            StopAllCoroutines();
            if(rig.arrowPool)rig.arrowPool.ResetPool();
            projectileTarget=null;shotCooldown=0;
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
            revived=true;ended=paused=transition=removeMode=false;phase=Phase.Playing;grace=5;
            entering=false;returningDragon=false;dragonNearDuck=dragonFireStarted=false;dragonAttackTime=2;
            headDistance=Mathf.Min(defeatDistance,entryTarget);
            // Resume at the first exposed point inside an unlocked slot's actual shooting reach.
            for(float d=.8f;d<rig.route.Length-2;d+=.1f){
                Vector3 point=OnPath(d);bool fog=rig.authoredLayout?rig.authoredLayout.IsInFog(world.TransformPoint(point)):!definition.tutorial&&point.y>=definition.fogAbove;
                if(fog)continue;bool inRange=false;
                for(int slot=0;slot<active.Length;slot++)if(SlotOpen(slot)&&((Vector2)(point-Slot(slot))).sqrMagnitude<collectionReach*collectionReach*.85f){inRange=true;break;}
                if(inRange){headDistance=Mathf.Min(d+.7f,rig.route.Length-2);break;}
            }
            if(flame)flame.ResetBreath();hud.SetDanger(0,0);
            rig.kitten.localScale=kittenScale;rig.kitten.gameObject.SetActive(true);
            if(rig.duckRescue){rig.kitten.gameObject.SetActive(false);rig.duckRescue.ResetForLevel(rig.route,world,rig.authoredLayout);headDistance=Mathf.Min(headDistance,rig.duckRescue.MotherDistance-1.5f);}
            tutorialShotSlot=-1;tutorialFeedbackUntil=0;SyncYarnViews();UpdateCrossbowCounts();UpdateGrid();hud.Hide();hud.SelectionMode(false,null);UpdateTutorial();
            hud.Status(level.Number,money,RemainingYarn,total);gameAudio.Play(WoolGameAudio.Cue.Booster);
            if(RemainingYarn==0)Win();
        }
        void WatchReward(Action earned,Action returnToOffer=null)
        {
            transition=true;bool settled=false;
            var ads=WoolServices.EnsureAds();
            ads.ShowRewardedVideo(message=>
            {
                if(!this||settled)return;settled=true;transition=false;WoolProgress.RecordDailyVideo();earned();
            },message=>
            {
                if(!this||settled)return;settled=true;transition=false;
                hud.Show("VIDEO UNAVAILABLE",message,"BACK",()=>{if(returnToOffer!=null)returnToOffer();else if(phase==Phase.Won)ShowWinReward();else if(phase==Phase.Lost)ShowDefeat("Try again or continue after watching a video.");else hud.Hide();});
            });
        }
        void Save(){WoolProgress.SetCoins(money);}
        void OnApplicationPause(bool suspended){if(suspended&&WoolProgress.IsLoaded)WoolProgress.Flush();}
        void OnApplicationQuit(){if(WoolProgress.IsLoaded)WoolProgress.Flush();}
        void OnDestroy()
        {
            if(hud&&AdsMediation.AdsMediationManager.instance)AdsMediation.AdsMediationManager.instance.AdVisibilityChanged-=hud.SetAdVisible;
            if(hud){hud.PausePressed-=TogglePause;hud.BoosterPressed-=OfferBooster;hud.SpeedPressed-=ToggleGameplaySpeed;hud.PlayPressed-=PlaySaved;hud.FreeCoinsPressed-=OfferFreeCoins;hud.LevelSelected-=OpenCampaignLevel;}
            if(generatedDefinition)Destroy(generatedDefinition);
            if(automatedInputValue||inspectorPreview){automatedInputValue=false;inspectorPreview=false;WoolProgress.EndPreview();}
        }
    }
}



