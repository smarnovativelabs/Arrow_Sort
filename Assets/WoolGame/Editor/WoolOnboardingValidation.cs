using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame.Editor
{
    // Opt-in Play Mode QA. All awards and profile edits remain in preview memory.
    public static class WoolOnboardingValidation
    {
        const string Request="WoolCrush.QA.NewPlayer";
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        public static string PrepareNewPlayer(){SessionState.SetBool(Request,true);return "Next Play uses a save-isolated new player.";}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializePreview()
        {
            if(!SessionState.GetBool(Request,false))return;
            SessionState.SetBool(Request,false);
            WoolProgress.Load(WoolEndlessGenerator.MaxLevel);WoolProgress.BeginPreview();
            WoolProgress.ResetProgress(false);WoolProgress.InitializeOnboarding();
        }
        public static string State()
        {
            var c=UnityEngine.Object.FindFirstObjectByType<WoolGameController>();
            var g=c?c.GetComponent<WoolOnboarding>():null;
            var message=g?(TMPro.TMP_Text)typeof(WoolOnboarding).GetField("message",Flags).GetValue(g):null;
            return "preview="+WoolProgress.IsPreview+" step="+WoolProgress.OnboardingStep+" level="+(c?c.CurrentLevel:0)+" phase="+(c?c.CurrentPhase:"")+" message="+(message?message.text:"");
        }
        public static string Continue()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<WoolOnboarding>();
            var button=g?(Button)typeof(WoolOnboarding).GetField("next",Flags).GetValue(g):null;
            if(!button||!button.gameObject.activeInHierarchy)throw new InvalidOperationException("No tutorial Continue is visible.");
            button.onClick.Invoke();return State();
        }
        public static string SelectNextVehicle()
        {
            var c=UnityEngine.Object.FindFirstObjectByType<WoolGameController>();
            int step=(int)typeof(WoolGameController).GetField("tutorialStep",Flags).GetValue(c);
            bool accepted=step<c.SolutionOrder.Count&&c.SelectTile(c.SolutionOrder[step]);
            return "accepted="+accepted+" tap="+step+" "+State();
        }
    }
}
