using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    // Owned by the scene controller; clips are loaded once and sources are reused.
    public sealed class WoolGameAudio : MonoBehaviour
    {
        public enum Cue { Click, Blocked, Dock, Booster, Start, Lose, Coin, Spin, Fire, Win, UnlockSlot, RemoveVehicle, SortColors, PierceFog }
        readonly AudioClip[] clips=new AudioClip[14];
        AudioSource effects,music;
        AudioClip homeTrack,gameplayTrack;
        AudioClip requestedMusic;
        float requestedVolume=.18f,outcomeDuckUntil;
        bool adVisible,applicationPaused;
        public string CurrentMusic=>music&&music.clip?music.clip.name:string.Empty;
        float lastClick=-1,lastBlocked=-1;
        bool initialized;
        bool audioAllowed;
        public bool MusicEnabled=>PlayerPrefs.GetInt("WoolRescue.Music",1)==1;
        public bool EffectsEnabled=>PlayerPrefs.GetInt("WoolRescue.Sound",1)==1;
        public void SetMusic(bool enabled){PlayerPrefs.SetInt("WoolRescue.Music",enabled?1:0);PlayerPrefs.Save();ApplyPreferences();}
        public void SetEffects(bool enabled){PlayerPrefs.SetInt("WoolRescue.Sound",enabled?1:0);PlayerPrefs.Save();ApplyPreferences();}
        void ApplyPreferences(){bool suspended=adVisible||applicationPaused;if(music)music.mute=!audioAllowed||!MusicEnabled||suspended;if(effects)effects.mute=!audioAllowed||!EffectsEnabled||suspended;if(fireSource)fireSource.mute=!audioAllowed||!EffectsEnabled||suspended;}
        public void Initialize(WoolSceneRig rig,bool enabledAudio)
        {
            if(initialized)return;
            initialized=true;
            audioAllowed=enabledAudio;
            effects=rig.audioSource;
            if(!effects){effects=gameObject.AddComponent<AudioSource>();rig.audioSource=effects;}
            effects.playOnAwake=false;effects.loop=false;effects.spatialBlend=0;effects.pitch=1;effects.mute=!enabledAudio;
            for(int i=0;i<clips.Length;i++)clips[i]=Resources.Load<AudioClip>("WoolAudio/"+((Cue)i));
            var library=Resources.Load<WoolSoundLibrary>("WoolSoundLibrary");
            if(library)
            {
                clips[(int)Cue.Booster]=library.bonus;
                clips[(int)Cue.Lose]=library.end;
                clips[(int)Cue.Coin]=library.coin;
                clips[(int)Cue.Spin]=library.spin;
                clips[(int)Cue.Fire]=library.fire;
                if(library.uiClick)clips[(int)Cue.Click]=library.uiClick;
                if(library.defeat)clips[(int)Cue.Lose]=library.defeat;
                clips[(int)Cue.Win]=library.victory?library.victory:library.win;
                clips[(int)Cue.UnlockSlot]=library.unlockSlot?library.unlockSlot:library.bonus;
                clips[(int)Cue.RemoveVehicle]=library.removeVehicle?library.removeVehicle:library.bonus;
                clips[(int)Cue.SortColors]=library.sortColors?library.sortColors:library.bonus;
                clips[(int)Cue.PierceFog]=library.pierceFog?library.pierceFog:library.bonus;
            }
            rig.departureSound=Resources.Load<AudioClip>("WoolAudio/Move");
            rig.winSound=clips[(int)Cue.Win]?clips[(int)Cue.Win]:Resources.Load<AudioClip>("WoolAudio/Win");
            clips[(int)Cue.Win]=rig.winSound;
            if(rig.arrowPool)
            {
                rig.arrowPool.audioSource=effects;
                rig.arrowPool.shotSound=Resources.Load<AudioClip>("WoolAudio/Shot");
                rig.arrowPool.dockSound=clips[(int)Cue.Dock];
            }
            music=gameObject.AddComponent<AudioSource>();music.playOnAwake=false;music.spatialBlend=0;
            music.loop=true;music.volume=0;music.mute=!enabledAudio;music.priority=160;
            gameplayTrack=library&&library.background?library.background:Resources.Load<AudioClip>("WoolAudio/Music");
            homeTrack=library&&library.menuMusic?library.menuMusic:gameplayTrack;
            requestedMusic=homeTrack;
            music.clip=homeTrack;
            if(music.clip)music.Play();
            fireSource=gameObject.AddComponent<AudioSource>();fireSource.playOnAwake=false;
            fireSource.spatialBlend=0;fireSource.loop=true;fireSource.volume=.28f;
            fireSource.mute=!enabledAudio;fireSource.clip=clips[(int)Cue.Fire];
            ApplyPreferences();
            rig.hud.BindAudio(this);
        }
        public void SetGameplayMusic(bool gameplay)
        {
            if(!music)return;
            requestedMusic=gameplay?gameplayTrack:homeTrack;
            requestedVolume=gameplay?.25f:.18f;
            ApplyPreferences();
        }
        void Update()
        {
            if(!music||applicationPaused||adVisible)return;
            float dt=Mathf.Min(Time.unscaledDeltaTime,.1f);
            if(music.clip!=requestedMusic)
            {
                music.volume=Mathf.MoveTowards(music.volume,0,dt*.85f);
                if(music.volume<=.001f){music.Stop();music.clip=requestedMusic;if(requestedMusic)music.Play();}
            }
            else
            {
                float target=requestedVolume*(Time.unscaledTime<outcomeDuckUntil?.32f:1);
                music.volume=Mathf.MoveTowards(music.volume,target,dt*.42f);
                if(requestedMusic&&!music.isPlaying&&MusicEnabled)music.Play();
            }
        }
        public void PlayAbility(int kind){if(kind>=0&&kind<4)Play((Cue)((int)Cue.UnlockSlot+kind));}
        public void PlayOutcome(bool won)
        {
            SetFireActive(false);SetGameplayMusic(false);
            outcomeDuckUntil=Time.unscaledTime+(won?3.5f:2.5f);
            Play(won?Cue.Win:Cue.Lose);
        }
        public void Play(Cue cue)
        {
            if(!effects||effects.mute||AudioListener.volume<=0)return;
            float now=Time.unscaledTime;
            if(cue==Cue.Click){if(now-lastClick<.06f)return;lastClick=now;}
            if(cue==Cue.Blocked){if(now-lastBlocked<.18f)return;lastBlocked=now;}
            var clip=clips[(int)cue];
            float gain=cue==Cue.Click?.4f:cue==Cue.Blocked?.3f:cue==Cue.Dock?.35f:cue==Cue.Lose?.3f:cue==Cue.Win?.65f:cue==Cue.SortColors?.7f:.55f;
            if(clip)effects.PlayOneShot(clip,gain);
        }
        public void SetAdVisible(bool visible)
        {
            adVisible=visible;
            if(visible){SetFireActive(false);if(music)music.Pause();if(effects)effects.Stop();}
            else if(music&&!applicationPaused)music.UnPause();
            ApplyPreferences();
        }
        public void SetFireActive(bool active)
        {
            if(!fireSource)return;
            if(active&&!fireSource.isPlaying&&fireSource.clip)fireSource.Play();
            else if(!active)fireSource.Stop();
        }
        AudioSource fireSource;
        void OnApplicationPause(bool paused)
        {
            applicationPaused=paused;ApplyPreferences();
            if(!music)return;
            if(paused){SetFireActive(false);music.Pause();if(effects)effects.Stop();}else if(!adVisible)music.UnPause();
        }
        void OnDisable(){SetFireActive(false);if(music)music.Pause();}
        void OnEnable(){if(music)music.UnPause();}
        void OnDestroy(){if(music)Destroy(music);if(fireSource)Destroy(fireSource);}
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
            if(!gameAudio)return;
            var feedback=button.GetComponent<WoolButtonAudio>();
            if(!feedback)feedback=button.gameObject.AddComponent<WoolButtonAudio>();
            feedback.Initialize(button,gameAudio);
        }
    }
}
