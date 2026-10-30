namespace WoolGame {
public sealed partial class WoolGameController {
    bool challengeSupportCompleted;
    bool ChallengeSupportRequired=>level!=null&&level.Number>4&&!challengeSupportCompleted;
    bool AllowChallengeAdFallback(){
        if(!ChallengeSupportRequired||ended||phase==Phase.Menu)return false;
        // Preserve the existing unavailable-ad fallback for this attempt only.
        challengeSupportCompleted=true;unlockedSlotMask|=16;UpdateCrossbowCounts();return true;
    }
}
}
