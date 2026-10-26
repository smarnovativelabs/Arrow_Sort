# Project cleanup - 26 October 2026

Removed 136 files (128.9 MiB) and 11 empty AssetDatabase folders from the active Unity project. The per-file list and original hashes are recorded in `removed-assets.json`.

The audit retained scene dependencies, every Resources asset, scripts, shaders and shader includes, native plugins, package integrations, project settings, license notices, and assets loaded by editor authoring tools through paths or constructed filenames. Retained authoring inputs may be absent from the current build but are required to maintain or regenerate the game.

Removed content includes historical QA screenshots, temporary scene saves, obsolete authored UI assets, unused effect prefabs/materials/textures, an unused slot prefab, and the replaced Home music track. A verified local recovery copy is in `LocalCleanupArchive/20261026`, excluded from Git. Historical backups, reference tooling, captures and scratch work were also moved into that local archive. A locked Unity-generated backup may remain outside it and is excluded from Git.

Validated in Unity 6000.3.20f1 using Unity CLI 1.0.0-beta.14:

- Edit Mode: no failed script compilation; zero missing scripts in the active game scene before and after cleanup.
- Play Mode: Home startup succeeds; controller is enabled and audio system exists.
- Save-isolated Level 1 preview advances from Ready to Playing. Frame count advances from 2430 to 3151.
- Console: zero errors, zero warnings after gameplay startup.
- Play Mode stopped after verification.

The Git copy contains Assets, Packages, ProjectSettings, reusable Tools, and supporting text documentation/source-license receipts. It excludes Library, Temp, Logs, builds, IDE state, generated backups, reference videos/images, local tool environments, and QA captures. No device build was performed for this cleanup.
