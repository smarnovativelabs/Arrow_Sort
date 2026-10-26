# Initial build audio validation

The scene uses one shared button click, a new calm menu loop, the existing `Assets/Sounds/Background.mp3` during levels, separate victory/defeat stingers, and four distinct ability cues. Existing coin, spin, flame, shot, movement, docking and hit feedback remains in use.

All eight new source files came from Pixabay through its normal download controls. See `Credits.md` and `sources.json` for creators, source pages and original file hashes. Original downloads are preserved. Seven effects have derived clips with silence trimmed and short edge fades; music streams from disk.

Unity Play Mode validation completed on 2026-10-25. `playback-report.txt` records passing checks for both music selections, audible output from all seven new effects, distinct ability clips, a real UI pointer event, 200/200 authored button coverage, independent sound/music preferences, ad suspension, application pause/resume, outcome music transitions and source reuse. Tests used isolated preview progress and restored audio preferences.

The click and unlock effect onsets were additionally measured after processing: approximately 6.6 ms and 8 ms respectively. Effects use reused AudioSources and balanced per-cue gains. Music fades between contexts and ducks briefly beneath outcome stingers.

The repeatable Editor playback check is saved in `Tools/AudioReleaseValidation.cs.txt`; import and scene bindings are authored by `WoolAudioAuthoring.Apply()`.

These are Editor playback and output-buffer checks. Physical Android speaker/headphone balance, OS interruptions and live ad SDK behavior still need device testing. The final APK build outcome is recorded separately in `BuildResult.md`.
