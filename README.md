# Arrow Sort Puzzle: Rescue Ducks

Unity portrait puzzle game with a dragon rescue board, color-matched vehicles and crossbows, campaign map, daily tasks, spin rewards, profiles, shop, and guided first-player onboarding.

## Open the project

Use **Unity 6000.3.20f1**. Open this repository as a Unity project, let Package Manager resolve `Packages/manifest.json`, and open `Assets/Scenes/Wool Crush.unity`. Press Play. Unity regenerates Library and IDE files locally.

The project keeps its original `WoolGame` code namespace and scene filename. All imported assets include their Unity metadata. Do not regenerate or discard `.meta` files.

## Source and configuration

- `Assets/WoolGame`: gameplay, UI, editor tools, authored content and shaders.
- `Assets/Art`, `Assets/Effects`, `Assets/Sounds`: supplied art, effects and audio.
- `Packages` and `ProjectSettings`: reproducible dependency and Unity configuration.
- `Tools` and `Docs`: authoring/validation helpers, historical notes and source/license receipts.

Home music source and license: `Docs/AudioRelease20261026/HomeMusicSource.json`. Other audio credits are in `Docs/AudioRelease20261025/`.

## Cleanup verification

See `Docs/Cleanup20261026/Validation.md` and `removed-assets.json` for scope and verification. Generated caches, builds, local backups, scratch work and QA captures are excluded from version control. Editor authoring inputs and native plugin files are deliberately retained even where absent from scene dependency reports.

Existing integration settings remain part of the project. Configure production service/ad/store settings for the intended deployment before releasing a build. This cleanup did not run an Android/iOS device build.
