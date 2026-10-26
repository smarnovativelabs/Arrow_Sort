# Initial UI build validation — 25 October 2026

The UI polish is saved in `Assets/Scenes/Wool Crush.unity`.

## Applied

- Kept the existing fantasy/duck artwork and unified text treatment, button feedback, cream/gold panel styling, popup close controls, and gameplay header.
- Added original icons for Unlock Slot, Remove Vehicle, Sort Colors, and Pierce Fog. Updated 39 ability images, including the visible gameplay tiles and ability popups; retained the existing gameplay identifiers and prices.
- Set 289 simple UI images to preserve their aspect ratio. Resizable surfaces use bordered, nine-sliced artwork; backgrounds cover the physical canvas with uniform scaling and crop excess edges.
- Separated physical-display background coverage from safe-area controls. Home, Shop, Map, loading artwork, gameplay header and popup dimmers extend into the notch/home-indicator margins.
- Fixed double safe-area subtraction in the Settings/Coins panel.
- Fixed three pre-existing C# name collisions in `WoolOnboarding.cs` that prevented compilation.
- Retained the gameplay frame, rewards, ability unlock levels, progression and purchase callbacks.

## Evidence

- Unity 6000.3.20f1 compiled with `compilationFailed=false` after the final code changes.
- Play Mode advanced through live frames. Validation used `WoolGameController.automatedInput` / `WoolProgress.BeginPreview` isolation and did not claim or spend real saved rewards.
- Thirty screen/device cases passed control bounds checks at 1080×2400, 720×1280 and 1536×2048. The tall phone used simulated top/bottom safe insets of 110/80 pixels; the tablet used 48/40 pixels.
- Reviewed live screen captures for Home, Settings, Coins, Shop, Map, Daily Tasks, Spin, Profile, offers, gameplay, all four ability popups, victory, defeat and loading. This checks existing screens; it does not certify every gameplay/onboarding transition.
- Twelve interaction checks passed, including Settings hit targeting and close, Home navigation, Daily Tasks, Profile, Shop, Map and each ability button opening the corresponding icon popup.
- Final targeted captures verified the corrected notch dimmers after adding CanvasRenderer components. Phone and tablet margin meshes rendered successfully for both Settings and the ability popup.
- Android is the active build target, its build module is installed, APK mode is enabled, and configuration dry-run returned `valid=true`. Android renderOutsideSafeArea was already enabled and was preserved.

The accompanying `layout-report.txt`, `interaction-report.txt`, `notch-report.txt` and PNG/JPEG captures contain the UI evidence. These are Editor simulations, not physical Android or iOS device tests.

## Build

An Android APK build was started at `Builds/UIInitial/WoolCrush.apk`. Its final outcome is recorded in `BuildResult.md`.

## Remaining device checks

Install the initial build on a notched Android phone and a tablet. Verify OS cutout rendering, native ad overlays, touch targets, audio, suspend/resume and new-player onboarding. Live ad availability and store purchases were not exercised; existing store-unavailable behavior remains. A successful build is not a store-release certification.
