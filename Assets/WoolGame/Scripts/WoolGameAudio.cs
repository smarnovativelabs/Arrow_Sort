using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    // Owned by the scene controller; clips are loaded once and sources are reused.
    public sealed class WoolGameAudio : MonoBehaviour
    {
        public enum Cue { Click, Blocked, Dock, Booster, Start, Lose, Coin, Spin, Fire, Win, UnlockSlot, RemoveVehicle, SortColors, PierceFog, DuckQuack }
        readonly AudioClip[] clips=new AudioClip[15];
        AudioSource effects,music,feedback,duckVoice;
        AudioClip homeTrack,gameplayTrack;
        AudioClip requestedMusic;
        const float HomeVolume=.32f,GameplayVolume=.28f;
        float requestedVolume=HomeVolume,outcomeDuckUntil;
        bool fireRequested;
        bool adVisible,applicationPaused;
        public string CurrentMusic=>music&&music.clip?music.clip.name:string.Empty;
        float lastClick=-1,lastBlocked=-1;
        bool initialized;
        bool audioAllowed;
        public bool MusicEnabled=>PlayerPrefs.GetInt("WoolRescue.Music",1)==1;
        public bool EffectsEnabled=>PlayerPrefs.GetInt("WoolRescue.Sound",1)==1;
        public void SetMusic(bool enabled){PlayerPrefs.SetInt("WoolRescue.Music",enabled?1:0);PlayerPrefs.Save();ApplyPreferences();}
        public void SetEffects(bool enabled){PlayerPrefs.SetInt("WoolRescue.Sound",enabled?1:0);PlayerPrefs.Save();ApplyPreferences();}
        void ApplyPreferences(){bool suspended=adVisible||applicationPaused;if(music)music.mute=!audioAllowed||!MusicEnabled||suspended;if(effects)effects.mute=!audioAllowed||!EffectsEnabled||suspended;if(feedback)feedback.mute=!audioAllowed||!EffectsEnabled||suspended;if(duckVoice)duckVoice.mute=!audioAllowed||!EffectsEnabled||suspended;if(fireSource)fireSource.mute=!audioAllowed||!EffectsEnabled||suspended;}
        public void Initialize(WoolSceneRig rig,bool enabledAudio)
        {
            if(initialized)return;
            initialized=true;
            audioAllowed=enabledAudio;
            // One upgrade resets the stale OFF preference behind inaudible feedback.
            // Subsequent user mute choices are respected on every later launch.
            if(PlayerPrefs.GetInt("WoolRescue.AudioFeedbackVersion",0)<2){PlayerPrefs.SetInt("WoolRescue.Sound",1);PlayerPrefs.SetInt("WoolRescue.AudioFeedbackVersion",2);PlayerPrefs.Save();}
            AudioListener.pause=false;
            effects=rig.audioSource;
            if(!effects){effects=gameObject.AddComponent<AudioSource>();rig.audioSource=effects;}
            effects.playOnAwake=false;effects.loop=false;effects.spatialBlend=0;effects.pitch=1;effects.volume=1;effects.priority=64;effects.dopplerLevel=0;effects.mute=!enabledAudio;
            feedback=gameObject.AddComponent<AudioSource>();feedback.playOnAwake=false;feedback.loop=false;feedback.spatialBlend=0;feedback.volume=1;feedback.priority=16;feedback.dopplerLevel=0;
            duckVoice=gameObject.AddComponent<AudioSource>();duckVoice.playOnAwake=false;duckVoice.loop=false;duckVoice.spatialBlend=0;duckVoice.volume=1;duckVoice.priority=24;duckVoice.dopplerLevel=0;
            var library=LoadClips();
            rig.departureSound=library.vehicleMove;
            if(library&&library.vehicleMove)rig.departureSound=library.vehicleMove;
            rig.winSound=clips[(int)Cue.Win];
            clips[(int)Cue.Win]=rig.winSound;
            if(rig.arrowPool)
            {
                rig.arrowPool.audioSource=effects;
                rig.arrowPool.shotSound=library.arrowShot;
                if(library&&library.arrowShot)rig.arrowPool.shotSound=library.arrowShot;
                var impact=library.arrowImpact;
                if(library&&library.arrowImpact)impact=library.arrowImpact;
                if(impact)rig.arrowPool.hitSound=impact;
                rig.arrowPool.dockSound=clips[(int)Cue.Dock];
            }
            music=gameObject.AddComponent<AudioSource>();music.playOnAwake=false;music.spatialBlend=0;
            music.loop=true;music.volume=0;music.mute=!enabledAudio;music.priority=160;
            gameplayTrack=library.background;
            homeTrack=library&&library.menuMusic?library.menuMusic:gameplayTrack;
            requestedMusic=homeTrack;
            music.clip=homeTrack;
            if(music.clip)music.Play();
            fireSource=gameObject.AddComponent<AudioSource>();fireSource.playOnAwake=false;
            fireSource.spatialBlend=0;fireSource.loop=true;fireSource.volume=0;fireSource.priority=96;
            fireSource.mute=!enabledAudio;fireSource.clip=clips[(int)Cue.Fire];
            ApplyPreferences();
            rig.hud.BindAudio(this);
        }
WoolSoundLibrary LoadClips()
        {
            var library=Resources.Load<WoolSoundLibrary>("WoolSoundLibrary");
            if(!library)throw new System.InvalidOperationException("The authored WoolSoundLibrary is missing.");
            clips[(int)Cue.Click]=library.uiClick;
            clips[(int)Cue.Blocked]=library.blocked;
            clips[(int)Cue.Dock]=library.vehicleDock;
            clips[(int)Cue.Booster]=library.bonus;
            clips[(int)Cue.Start]=library.levelStart;
            clips[(int)Cue.Lose]=library.defeat?library.defeat:library.end;
            clips[(int)Cue.Coin]=library.coin;
            clips[(int)Cue.Spin]=library.spin;
            clips[(int)Cue.Fire]=library.fire;
            clips[(int)Cue.Win]=library.victory?library.victory:library.win;
            clips[(int)Cue.UnlockSlot]=library.unlockSlot?library.unlockSlot:library.bonus;
            clips[(int)Cue.RemoveVehicle]=library.removeVehicle?library.removeVehicle:library.bonus;
            clips[(int)Cue.SortColors]=library.sortColors?library.sortColors:library.bonus;
            clips[(int)Cue.PierceFog]=library.pierceFog?library.pierceFog:library.bonus;
            clips[(int)Cue.DuckQuack]=library.duckQuack;
            return library;
        }
        public void SetGameplayMusic(bool gameplay)
        {
            if(!music)return;
            requestedMusic=gameplay?gameplayTrack:homeTrack;
            requestedVolume=gameplay?GameplayVolume:HomeVolume;
            ApplyPreferences();
        }
        void Update()
        {
            if(!music||applicationPaused||adVisible)return;
            float dt=Mathf.Min(Time.unscaledDeltaTime,.1f);
            if(fireSource){float targetFire=fireRequested&&audioAllowed&&EffectsEnabled?.28f:0;fireSource.volume=Mathf.MoveTowards(fireSource.volume,targetFire,dt*1.6f);if(targetFire==0&&fireSource.volume<=.001f&&fireSource.isPlaying)fireSource.Stop();}
            if(music.clip!=requestedMusic)
            {
                music.volume=Mathf.MoveTowards(music.volume,0,dt*1.4f);
                if(music.volume<=.001f){music.Stop();music.clip=requestedMusic;if(requestedMusic)music.Play();}
            }
            else
            {
                float target=requestedVolume*(Time.unscaledTime<outcomeDuckUntil?.22f:1);
                music.volume=Mathf.MoveTowards(music.volume,target,dt*(music.volume>target?2.5f:.8f));
                if(requestedMusic&&!music.isPlaying&&MusicEnabled)music.Play();
            }
        }
        public void PlayAbility(int kind){if(kind>=0&&kind<4)Play((Cue)((int)Cue.UnlockSlot+kind));}
        public void PlayOutcome(bool won)
        {
            SetFireActive(false);if(effects)effects.Stop();if(feedback)feedback.Stop();SetGameplayMusic(false);
            outcomeDuckUntil=Time.unscaledTime+(won?3.5f:2.5f);
            Play(won?Cue.Win:Cue.Lose);
        }
        public void Play(Cue cue)
        {
            bool foreground=cue==Cue.Click||cue==Cue.Coin||cue==Cue.Spin||cue==Cue.Win||cue==Cue.Lose;
            var source=cue==Cue.DuckQuack?duckVoice:foreground?feedback:effects;
            if(!source||source.mute||AudioListener.volume<=0)return;
            float now=Time.unscaledTime;
            if(cue==Cue.Click){if(now-lastClick<.06f)return;lastClick=now;}
            if(cue==Cue.Blocked){if(now-lastBlocked<.18f)return;lastBlocked=now;}
            var clip=clips[(int)cue];
            float gain=foreground?(cue==Cue.Click?.95f:1):cue==Cue.Blocked?.75f:cue==Cue.Dock?.8f:.85f;
            if(clip)source.PlayOneShot(clip,gain);
        }
        public void SetAdVisible(bool visible)
        {
            adVisible=visible;
            if(visible){SetFireActive(false);if(music)music.Pause();if(effects)effects.Stop();if(feedback)feedback.Stop();if(duckVoice)duckVoice.Stop();}
            else if(music&&!applicationPaused)music.UnPause();
            if(!visible&&!applicationPaused)AudioListener.pause=false;
            ApplyPreferences();
        }
        public void SetFireActive(bool active)
        {
            if(!fireSource)return;
            fireRequested=active&&audioAllowed&&EffectsEnabled&&!adVisible&&!applicationPaused;
            if(fireRequested&&!fireSource.isPlaying&&fireSource.clip)fireSource.Play();
            if(!fireRequested&&(adVisible||applicationPaused||!EffectsEnabled)){fireSource.Stop();fireSource.volume=0;}
        }
        AudioSource fireSource;
        void OnApplicationPause(bool paused)
        {
            applicationPaused=paused;ApplyPreferences();
            if(!music)return;
            if(paused){SetFireActive(false);music.Pause();if(effects)effects.Stop();if(feedback)feedback.Stop();if(duckVoice)duckVoice.Stop();}else if(!adVisible){AudioListener.pause=false;music.UnPause();}
        }
        void OnDisable(){SetFireActive(false);if(fireSource)fireSource.Stop();if(effects)effects.Stop();if(feedback)feedback.Stop();if(duckVoice)duckVoice.Stop();if(music)music.Pause();}
        void OnEnable(){if(initialized){LoadClips();if(fireSource)fireSource.clip=clips[(int)Cue.Fire];}if(music)music.UnPause();}
        void OnDestroy(){if(music)Destroy(music);if(fireSource)Destroy(fireSource);if(feedback)Destroy(feedback);if(duckVoice)Destroy(duckVoice);}
    }
    public sealed partial class WoolHud
    {
        WoolGameAudio gameAudio;
        public void BindAudio(WoolGameAudio audio)
        {
            gameAudio=audio;
            foreach(var button in GetComponentsInChildren<Button>(true))BindButtonAudio(button);
            BindMotion();
        }
        void BindButtonAudio(Button button)
        {
            WoolGlossyButton.Style(button);
            if(!gameAudio)return;
            var feedback=button.GetComponent<WoolButtonAudio>();
            if(!feedback)feedback=button.gameObject.AddComponent<WoolButtonAudio>();
            feedback.Initialize(button,gameAudio);
        }
    }
}
