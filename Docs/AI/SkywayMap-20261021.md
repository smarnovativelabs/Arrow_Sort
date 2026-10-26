# Animated Skyway Map — 2026-10-21

Implemented in the existing Wool Crush scene, HUD and campaign save system.

Visuals: supplied Duck and Dragon Skyway Map reference, cleaned underlying sky-island background, eleven numbered stops per page. All unlocked nodes including the current frontier are green; locked nodes remain grey and numbered, with no lock icons or difficulty/state captions. Existing Shop/Home navigation remains functional; Map is selected in its own navigation bar. Artwork preserves its aspect ratio. Tablet layouts scroll vertically; navigation retains its original proportions.

Animation: 48 frames baked offline from existing mother/baby duck models and their idle/eat, walk and happy-jump clips. CPU SkinnedMeshRenderer.BakeMesh per pose avoids same-frame GPU skin-cache reuse. Runtime uses one sprite atlas, with no extra 3D animation camera. Ducks follow a curved path to the latest completed level, celebrate, show an expanding arrival ring, and pop the newly unlocked green node. Current available level pulses; sleeping-dragon Z marks float. Page changes fade. Interrupted travel remains pending until completed.

Persistence: campaign adds mapLastCompletedLevel, mapPresentedLevel and mapPresentationKnown. Existing unlock, reward and replay rules remain authoritative. Older saves initialize the marker to existing completed progress; the first completion without a previous map visit still animates. Completion UI now offers VIEW MAP; choosing HOME preserves the pending transition for the next map visit. Replays do not grant duplicate coins. Startup remains Home.

Files: WoolSkywayMap.cs, WoolMapNodeGraphic.cs, WoolHud.Skyway.cs, WoolProgress.Map.cs, WoolSkywayAuthoring.cs; integrated into existing WoolHud.Campaign.cs, WoolHud.Reference.cs, WoolProgress.cs and WoolGameController.cs. Assets: Assets/WoolGame/Designer/UI/Skyway/. Scene: Assets/Scenes/Wool Crush.unity. Backup: Docs/Backups/SkywayMap-20261021.

Validation: Editor compilation clean; isolated progression tests passed fresh state, first completion, twelve sequential unlocks, page 11-to-12, presentation bookkeeping and replay rewards, with campaign PlayerPrefs unchanged during preview. Actual unlocked button opened level2 and locked button did not load a level. Walk atlas frames differed in2602 pixels. First-win coroutine captured Walking and Celebrating, ended at presented1/destination1, unlocked node2 green and returned Idle. Page button advanced to12. Rendered phone1440x2960 and tablet768x1024; measured artwork ratio0.562799 matched source. Final tablet capture confirmed gameplay header/footer hidden, existing saved progress64 preserved,65 unlocked and66 locked. No Android build or device frame-rate benchmark performed for this map change.

Capture guidance: do NOT use MCP manage_camera screenshot's composited synchronous path; its EditorApplication.Step loop caused recursive PlayerLoop errors in this project. Use ScreenCapture.CaptureScreenshot(path), allow native frames to render, then read the saved image. Do not pump PlayerLoop/Step from MCP callbacks.

Artwork edit used built-in image generation to remove baked node buttons, bottom ducks and navigation while preserving the reference path, rock islands, clouds and sleeping dragon. Original source retained as SkywayReference.png; clean project background saved as SkywayBackground.jpg.
