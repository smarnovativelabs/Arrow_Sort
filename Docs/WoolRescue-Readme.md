# Wool Rescue implementation

Open `Assets/Scenes/Wool Crush.unity` in Unity 6000.3.20f1 and press Play. The scene's **Wool Rescue - Game** object constructs the game when Play starts. Use a portrait Game view (720 x 1440 suggested). Assets -> Refresh and reopening the scene may be needed after these external edits.

## Included

- Original procedural knitted wool materials, a moving dragon on a curved path, three animated kittens, garden decorations and colored arrow tiles.
- Directional tile paths are blocked by any unselected tile in their way. Five spool slots accept cleared tiles; matching colors remove wool from the dragon and counts update.
- Level 1 uses the six-tile tutorial layout seen in the public gameplay recording; later levels are deterministic generated layouts with increasing colors and grid depth. These later layouts are not extracted reference levels.
- Win/loss/retry, pause, sound toggle, free hints and unused-spool undo, coin-funded freeze, saved level and coins.
- Portrait HUD with safe-area handling, progress bar, tool buttons, and result dialogs.
- One interstitial attempt per completed level. In Editor, a clearly marked TEST AD panel exercises the close-to-next-level sequence. On Android/iOS, AdMob sample ads are enabled. No-fill/consent failure permits progression.

## Advertising configuration

For production, replace the sample application IDs in **Assets -> Google Mobile Ads -> Settings** with your app IDs. On **Wool Rescue - Game / Level Completion Ads**, disable **Use Test Ads** and enter the Android/iOS interstitial ad unit IDs. Leave test mode enabled for development. Real ad delivery depends on consent, valid configuration, connectivity and inventory; it cannot be guaranteed after every level.

The existing AdMob/MAX wrapper was adapted to remove missing dependencies on another game's managers. Original scripts are backed up under `Docs/Backups/BeforeWoolGame`. AdMob fullscreen callbacks now run on Unity's main thread, and display failures release the level transition. The optional Firebase remote config component retains only the relevant mediation value. This game does not initialize unrelated analytics/store systems.

## Earlier validation, 2026-10-06

- Unity Roslyn compiler using this project's actual references: Editor symbols PASS, Android player symbols PASS. Logs: `Temp/WoolValidation/editor-compile.log`, `player-compile.log`.
- 200 generated levels passed deterministic generation, per-color ammunition balance, blocked front-row selection, a front-to-back solution, and complete-clearance checks. Result: `Temp/WoolValidation/rules-results.txt`. This was before directional arrow blocking was added and does not validate the current rules.
- Existing missing startup scene entry removed from `ProjectSettings/EditorBuildSettings.asset`.
- Existing Google iOS resolver importer OS corrected to AnyOS so Windows Editor scripts can resolve it. Android plugin directory/template created; AdMob sample app configuration added.
- Unity live scene/refresh/screenshot tools were rejected by the host approval policy. Runtime rendering, touch input, full win/ad flow, portrait phone/tablet framing, and device APK builds remain unverified. User offered to refresh/reopen the scene and enter Play Mode.
- The directional tile and color-matching loop now follows the clearest public listing and video evidence. This remains an original implementation, not a verified full clone: exact hit rules, failure timing, booster effects, menu/map flow, events, and original later-level data are still unverified. See `Docs/WoolCrush-ReferenceSpec.md`.

## Save data

Keys: `WoolRescue.Level`, `WoolRescue.Coins`. Disable **Restore Progress** on the controller to begin at **Starting Level** during testing. The current level is saved only after a win.
