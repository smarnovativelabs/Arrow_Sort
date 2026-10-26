# Unity project context

Inspected 2026-10-07. Root: `C:/Users/Muneeb Nazir/Wool Crush`. No Git metadata found.

- Confirmed Unity 6000.3.20f1; URP 17.3.0; Input System 1.19.0, activeInputHandler 1; uGUI 2.0.0. Sources: ProjectVersion.txt, manifest.json, ProjectSettings.asset.
- Active/build scene: `Assets/Scenes/Wool Crush.unity`; it contains Main Camera, Directional Light, Global Volume, and a `Wool Rescue - Game` object with runtime game/ad components. Build Settings has this one enabled scene.
- Imported SDKs: Google Mobile Ads, AppLovin MAX, Firebase, Tenjin. First-party ad wrappers are in `Assets/AdsMediation`; the prior missing-manager references and resolver setup were adjusted for this project. Current Console state was not re-read during the 2026-10-07 reference task.
- First-party gameplay is in `Assets/WoolGame/Scripts` under namespace `WoolGame`: `WoolGameController` composes runtime world/UI, `WoolLevel` owns deterministic rules, `WoolArt` builds procedural visuals, `WoolHud` builds uGUI, and `LevelCompletionAds` handles level-completion ads. MonoBehaviours, coroutines, and PlayerPrefs are used. Unity Test Framework is installed; no test assembly is present.
- Unity MCP v10.2.0 installed. Wool Crush instance selected; resources and console reading work. Scene hierarchy and refresh calls rejected: approval required but policy never. Desktop game capture timed out twice. User instructed continuing from available reference details.
- New gameplay belongs to namespace WoolGame; separate rules, presentation, HUD, and level completion ad flow. Use Input System for pointer/touch and uGUI.
- Public listing confirms the wool dragon, color matching, directional/blocking puzzle cues, cat rescue, offline single-player, events, and Windows availability through Intel technology. A third-party video confirms the opening tutorial prompt and a six-tile board; the remaining authored boards and exact booster effects are unverified. `Docs/WoolCrush-ReferenceSpec.md` records sources and confidence.
- Backups of affected existing files: Docs/Backups/BeforeWoolGame. Do not copy backup C# files into Assets.
- Ad credentials are configuration; do not reproduce SDK keys in documentation. Live device ads require valid application and unit configuration, consent, network availability, and network inventory.

2026-10-06 follow-up: Live Unity actions were available. Set Google.IOSResolver importer validateReferences to 0; verified Assembly.Load succeeds and no errors recurred after domain reload/Play. Repaired missing URP graphics/quality references with `Assets/WoolGame/Settings/WoolRenderer.asset` and `WoolPipeline.asset`. A live Play screenshot confirmed the earlier material issue was fixed.

2026-10-07 reference update: Core tiles display cardinal arrows and selection checks whether the full arrow path is clear. Level 1 now uses the six-tile, two-row tutorial layout from the video and begins with the blue down-arrow color; unselected board tiles no longer show ammo numbers, while occupied spool slots do. Later boards remain generated. The live PC game could not be opened because the desktop app inventory exposed no Play Games window; only its background service process was found. These code changes were not compiled or checked in Play Mode. The project still lacks a verified home/map flow, later authored levels, and exact booster behavior.
