# Audio sources

Downloaded from the individual Pixabay asset pages on 25 October 2026. The pages identify these assets as free for use under the [Pixabay Content License](https://pixabay.com/service/license-summary/). Files are embedded as part of the game; this project does not offer them as a standalone sound library.

| Game use | Track and creator | Source |
|---|---|---|
| All UI button presses | Soft UI Click — Universfield | [Pixabay](https://pixabay.com/sound-effects/film-special-effects-soft-ui-click-147352/) |
| Non-level background music | Cinematic Fantasy Calm Theme — Luis_Humanoide | [Pixabay](https://pixabay.com/music/main-title-cinematic-fantasy-calm-theme-294221/) |
| Level won | Brass Fanfare — Universfield | [Pixabay](https://pixabay.com/sound-effects/film-special-effects-brass-fanfare-144755/) |
| Level lost | Game Over Orchestral Stinger - Cartoon Defeat — Coghezzi | [Pixabay](https://pixabay.com/sound-effects/musical-game-over-orchestral-stinger-cartoon-defeat-546515/) |
| Unlock one slot | Unlock stinger — Sunovia | [Pixabay](https://pixabay.com/sound-effects/film-special-effects-unlock-stinger-289722/) |
| Remove a vehicle | Pop Cartoon — CreatorsHome | [Pixabay](https://pixabay.com/sound-effects/film-special-effects-pop-cartoon-328167/) |
| Rearrange snake colors | Magic Twinkle — Universfield | [Pixabay](https://pixabay.com/sound-effects/film-special-effects-magic-twinkle-244951/) |
| Pierce fog | Magical Whoosh — u_hyed0v3ux9 | [Pixabay](https://pixabay.com/sound-effects/film-special-effects-magical-whoosh-148459/) |

The original `Assets/Sounds/Background.mp3` remains the level music. Original coin, spin, flame, shot, docking, movement, and hit assets remain in use. No original sound file was overwritten.

`sources.json` records the exact file paths and SHA-256 hashes. The sound files were downloaded through the normal Pixabay download controls.

The effects under `Assets/Sounds/Pixabay/Processed/` are game-ready derivatives created from those downloads: leading/trailing silence was removed and short edge fades were applied. The original MP3 files are preserved. In particular, the button click had approximately 151 ms of leading silence and the unlock cue had approximately 504 ms; the processed clips begin within 8 ms.
