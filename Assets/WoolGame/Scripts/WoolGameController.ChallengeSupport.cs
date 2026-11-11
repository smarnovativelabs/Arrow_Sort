namespace WoolGame {
public sealed partial class WoolGameController {
    bool challengeSupportCompleted;
    bool ChallengeSupportRequired=>level!=null&&level.Number>4&&level.Number<=10&&!challengeSupportCompleted;
    bool AllowChallengeAdFallback(){
        if(!ChallengeSupportRequired||ended||phase==Phase.Menu)return false;
        // Preserve opening-level fallback; later challenge slots require their rewarded unlock.
        challengeSupportCompleted=true;unlockedSlotMask|=16;UpdateCrossbowCounts();return true;
    }
}
}
