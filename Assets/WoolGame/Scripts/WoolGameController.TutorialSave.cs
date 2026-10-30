using UnityEngine;
namespace WoolGame
{
    public sealed partial class WoolGameController
    {
        bool TutorialGuidance=>definition&&definition.tutorial&&WoolProgress.OnboardingStep==WoolOnboardingStep.Gameplay&&!WoolProgress.HasCompleted(1);
        bool TutorialCheckpointActive=>TutorialGuidance;
        public void RestartCurrentLevel(){if(level==null)return;int number=level.Number;if(TutorialCheckpointActive)WoolProgress.ClearTutorialCheckpoint();LoadLevel(number);}
        int TutorialLayoutHash()
        {
            unchecked
            {
                int hash=17;
                foreach(var tile in level.Tiles){hash=hash*31+tile.color;hash=hash*31+tile.ammo;hash=hash*31+(int)tile.direction;hash=hash*31+tile.x.GetHashCode();hash=hash*31+tile.y.GetHashCode();}
                foreach(int index in level.Solution)hash=hash*31+index;
                return hash;
            }
        }
        bool ValidTutorialState(WoolTutorialCheckpoint state)
        {
            if(state==null||state.level!=level.Number||!state.Valid(TutorialLayoutHash(),level.Tiles.Count,total,active.Length))return false;
            if(definition.tutorial)for(int i=0;i<state.step;i++)if(!state.selected[level.Solution[i]])return false;
            float expected=total;
            for(int i=0;i<level.Tiles.Count;i++)if(state.selected[i])expected-=level.Tiles[i].ammo;
            for(int i=0;i<active.Length;i++)if(state.bowTiles[i]>=0)
            {
                if(state.bowInitial[i]!=level.Tiles[state.bowTiles[i]].ammo)return false;
                expected+=state.bowAmmo[i];
            }
            return Mathf.Abs(expected-state.colors.Length)<.001f;
        }
        void SaveTutorialState()
        {
            if(!TutorialCheckpointActive||ended||yarnAmounts.Count!=level.Yarn.Count)return;
            var state=new WoolTutorialCheckpoint{level=level.Number,layoutHash=TutorialLayoutHash(),step=tutorialStep,total=total,
                waitingShot=tutorialShotSlot,sequence=collectionSequence,head=Mathf.Max(0,headDistance-rig.route.EntryExtension),elapsed=elapsed,
                unlockedMask=unlockedSlotMask,sortCount=sortUses,grace=grace,amplifyRemaining=Mathf.Max(0,amplifyUntil-elapsed),
                feedbackRemaining=Mathf.Max(0,tutorialFeedbackUntil-elapsed),colors=level.Yarn.ToArray(),amounts=yarnAmounts.ToArray(),
                selected=new bool[level.Tiles.Count],bowTiles=new int[active.Length],bowAmmo=new float[active.Length],
                bowInitial=new int[active.Length],bowOrder=new int[active.Length]};
            for(int i=0;i<level.Tiles.Count;i++)state.selected[i]=level.Tiles[i].selected;
            for(int i=0;i<active.Length;i++)
            {
                var bow=active[i];state.bowTiles[i]=bow==null?-1:bow.index;
                if(bow==null)continue;state.bowAmmo[i]=bow.ammo;state.bowInitial[i]=bow.initialAmmo;state.bowOrder[i]=bow.arrivalOrder;
            }
            WoolProgress.SaveTutorialCheckpoint(state);
        }
        void RestoreTutorialState()
        {
            if(!TutorialCheckpointActive)return;
            var state=WoolProgress.TutorialCheckpoint;if(state==null)return;
            if(state.level!=level.Number)return;
            if(!ValidTutorialState(state)){WoolProgress.ClearTutorialCheckpoint();return;}
            tutorialStep=state.step;tutorialShotSlot=state.waitingShot;collectionSequence=state.sequence;
            elapsed=state.elapsed;tutorialFeedbackUntil=state.feedbackRemaining>0?elapsed+state.feedbackRemaining:0;
            headDistance=Mathf.Clamp(state.head+rig.route.EntryExtension,definition.tutorial?entryTarget:0,definition.tutorial?rig.route.Length-1.5f:rig.route.Length);entering=false;phase=Phase.Playing;
            unlockedSlotMask=state.unlockedMask;sortUses=state.sortCount;grace=state.grace;amplifyUntil=elapsed+state.amplifyRemaining;
            level.Yarn.Clear();level.Yarn.AddRange(state.colors);yarnAmounts.Clear();yarnAmounts.AddRange(state.amounts);
            SyncYarnViews();
            for(int i=0;i<level.Tiles.Count;i++)if(state.selected[i]){level.Tiles[i].selected=true;tileViews[i].gameObject.SetActive(false);}
            for(int slot=0;slot<active.Length;slot++)
            {
                int index=state.bowTiles[slot];if(index<0)continue;
                active[slot]=new ActiveCrossbow{index=index,slot=slot,ammo=state.bowAmmo[slot],initialAmmo=state.bowInitial[slot],
                    arrived=true,arrivalOrder=state.bowOrder[slot]>0?state.bowOrder[slot]:++collectionSequence};
                rig.arrowPool.Dock(slot,level.Tiles[index].color,world.TransformPoint(Slot(slot)));rig.arrowPool.Reveal(slot);
            }
            hud.Status(level.Number,money,RemainingYarn,total);UpdateCrossbowCounts();PositionYarn();UpdateTutorial();
            if(tutorialShotSlot>=0)hud.Hint("Watch the colours match!");
            else if(tutorialFeedbackUntil>elapsed)hud.Hint("Great match!");
        }
    }
}
