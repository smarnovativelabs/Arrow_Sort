using System;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [UnityEngine.SerializeField] WoolUtilityPanel utilityPanel;
        public void ShowCoinOptions(){CloseEngagement();Hide();if(utilityPanel)utilityPanel.ShowCoins(()=>{ShowShop();var view=shopPage.GetComponent<WoolShopView>();if(view)view.FocusCoinPacks();},()=>FreeCoinsPressed?.Invoke(),RefreshWalletLabels);}
        public void ShowAudioSettings(Action back,Action home=null)
        {
            if(!gameAudio)return;
            if(utilityPanel){CloseEngagement();Hide();utilityPanel.ShowSettings(gameAudio,back,home);return;}
            Show("MAKE YOURSELF AT HOME","Choose the sounds for your adventure.",
                "MUSIC  :  "+(gameAudio.MusicEnabled?"ON":"OFF"),
                ()=>{gameAudio.SetMusic(!gameAudio.MusicEnabled);ShowAudioSettings(back);},
                "SOUND  :  "+(gameAudio.EffectsEnabled?"ON":"OFF"),
                ()=>{gameAudio.SetEffects(!gameAudio.EffectsEnabled);ShowAudioSettings(back);},back);
        }
    }
}
