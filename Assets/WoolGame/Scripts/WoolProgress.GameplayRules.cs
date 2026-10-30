namespace WoolGame { public static partial class WoolProgress {
 public static bool GameplayRulesTutorialSeen {get {EnsureLoaded();return data.gameplayRulesTutorialSeen;}}
 public static void MarkGameplayRulesTutorialSeen(){EnsureLoaded();data.gameplayRulesTutorialSeen=true;Save();}
}}
