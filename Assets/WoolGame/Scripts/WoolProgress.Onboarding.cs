using UnityEngine;
namespace WoolGame
{
    // Existing numeric IDs are saved in player campaigns; append new steps.
    public enum WoolOnboardingStep { Gameplay, Coins, Map, HomeTasks, Login, Spin, Shop, Profile, Name, Finished, Complete, TasksMeter, Avatar }

    public static partial class WoolProgress
    {
        public const int DailyTasksUnlockLevel=5,LuckySpinUnlockLevel=10;
        public static bool DailyTasksUnlocked=>HasCompleted(DailyTasksUnlockLevel);
        public static bool LuckySpinUnlocked=>HasCompleted(LuckySpinUnlockLevel);
        public static bool FeatureTutorialOnly {get{EnsureLoaded();return data.featureTutorialOnly;}}
        public static bool TryBeginFeatureTutorial()
        {
            EnsureLoaded();if(OnboardingStep!=WoolOnboardingStep.Complete)return false;
            if(HasCompleted(3)&&!data.profileIntroDone){
                var remaining=RemainingProfileStep;
                if(remaining!=WoolOnboardingStep.Complete){data.onboardingStep=(int)remaining;data.featureTutorialOnly=true;Save();return true;}
                data.profileIntroDone=true;Save();
            }
            if(DailyTasksUnlocked&&!data.tasksIntroDone)data.onboardingStep=(int)WoolOnboardingStep.HomeTasks;
            else if(LuckySpinUnlocked&&!data.spinIntroDone)data.onboardingStep=(int)WoolOnboardingStep.Spin;
            else return false;
            data.featureTutorialOnly=true;Save();return true;
        }
        public static void FinishFeatureTutorial(){EnsureLoaded();data.featureTutorialOnly=false;data.onboardingStep=(int)WoolOnboardingStep.Complete;Save();}
        public static WoolOnboardingStep OnboardingStep { get { EnsureLoaded();return System.Enum.IsDefined(typeof(WoolOnboardingStep),data.onboardingStep)?(WoolOnboardingStep)data.onboardingStep:WoolOnboardingStep.Complete; } }
        public static bool OnboardingSpinTaken { get { EnsureLoaded();return data.onboardingSpinTaken; } }
        public static void InitializeOnboarding()
        {
            EnsureLoaded();
            if(!data.profileTutorialPolicyKnown){
                data.profileTutorialPolicyKnown=true;
                // Old completed introductions already included the profile steps.
                if(data.onboardingKnown&&(OnboardingStep==WoolOnboardingStep.Complete||OnboardingStep==WoolOnboardingStep.Shop||OnboardingStep==WoolOnboardingStep.Finished))data.profileIntroDone=true;
                Save();
            }
            if(data.onboardingKnown)return;
            // Missing fields in an established save must not enroll an existing player.
            data.onboardingKnown=true;
            data.onboardingStep=(int)(data.hasStarted||data.levels.Count>0?WoolOnboardingStep.Complete:WoolOnboardingStep.Gameplay);
            if(data.onboardingStep==0){data.mapPresentationKnown=true;data.mapPresentedLevel=1;}
            Save();
        }
        public static void AdvanceOnboarding(WoolOnboardingStep step)
        {
            EnsureLoaded();if(!data.onboardingKnown||Order(step)<=Order(OnboardingStep))return;
            if(OnboardingStep==WoolOnboardingStep.TasksMeter)data.tasksIntroDone=true;
            if(OnboardingStep==WoolOnboardingStep.Spin)data.spinIntroDone=true;
            data.onboardingStep=(int)step;Save();
        }
        static int Order(WoolOnboardingStep step)
        {
            switch(step){case WoolOnboardingStep.Gameplay:return 0;case WoolOnboardingStep.Coins:return 1;case WoolOnboardingStep.Map:return 2;case WoolOnboardingStep.HomeTasks:return 3;case WoolOnboardingStep.Login:return 4;case WoolOnboardingStep.TasksMeter:return 5;case WoolOnboardingStep.Spin:return 6;case WoolOnboardingStep.Profile:return 7;case WoolOnboardingStep.Name:return 8;case WoolOnboardingStep.Avatar:return 9;case WoolOnboardingStep.Shop:return 10;case WoolOnboardingStep.Finished:return 11;default:return 12;}
        }
    }
}
