using UnityEngine;
namespace WoolGame
{
    // Existing numeric IDs are saved in player campaigns; append new steps.
    public enum WoolOnboardingStep { Gameplay, Coins, Map, HomeTasks, Login, Spin, Shop, Profile, Name, Finished, Complete, TasksMeter, Avatar }

    public static partial class WoolProgress
    {
        public static WoolOnboardingStep OnboardingStep { get { EnsureLoaded();return System.Enum.IsDefined(typeof(WoolOnboardingStep),data.onboardingStep)?(WoolOnboardingStep)data.onboardingStep:WoolOnboardingStep.Complete; } }
        public static bool OnboardingSpinTaken { get { EnsureLoaded();return data.onboardingSpinTaken; } }
        public static void InitializeOnboarding()
        {
            EnsureLoaded();if(data.onboardingKnown)return;
            // Missing fields in an established save must not enroll an existing player.
            data.onboardingKnown=true;
            data.onboardingStep=(int)(data.hasStarted||data.levels.Count>0?WoolOnboardingStep.Complete:WoolOnboardingStep.Gameplay);
            if(data.onboardingStep==0){data.mapPresentationKnown=true;data.mapPresentedLevel=1;}
            Save();
        }
        public static void AdvanceOnboarding(WoolOnboardingStep step)
        {
            EnsureLoaded();if(!data.onboardingKnown||Order(step)<=Order(OnboardingStep))return;
            data.onboardingStep=(int)step;Save();
        }
        static int Order(WoolOnboardingStep step)
        {
            switch(step){case WoolOnboardingStep.Gameplay:return 0;case WoolOnboardingStep.Coins:return 1;case WoolOnboardingStep.Map:return 2;case WoolOnboardingStep.HomeTasks:return 3;case WoolOnboardingStep.Login:return 4;case WoolOnboardingStep.TasksMeter:return 5;case WoolOnboardingStep.Spin:return 6;case WoolOnboardingStep.Profile:return 7;case WoolOnboardingStep.Name:return 8;case WoolOnboardingStep.Avatar:return 9;case WoolOnboardingStep.Shop:return 10;case WoolOnboardingStep.Finished:return 11;default:return 12;}
        }
    }
}
