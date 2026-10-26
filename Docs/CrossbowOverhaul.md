# Crossbow shooting overhaul — 2026-10-15

## Delivered
- Original designer meshes from Assets/Art/Fbx/Arrow Bow Pack.fbx retained for all 80 weapons and arrows. The FBX source was not edited. The interim generated replacement meshes and materials were removed from Assets.
- Updated Assets/Prefabs/Slot 1.prefab and all eight slot instances in Assets/Scenes/Wool Crush.unity.
- Original problem: the large colored base dominated the small shallow bow; the narrow projectile was nearly invisible and no loaded arrow was shown. Parent scales and camera were not the cause.
- Slot discs enlarged from diameter 1.30 to 1.794 world units. Original bow width is approximately 1.45, about 81% of the slot width. Base reduced independently, stand raised, bow thickened, atlas retained with modest Simple Lit shading. Camera unchanged.
- Original FBX projectile uses 1.8x length, 3.4x width, 3x depth; loaded presentation uses 0.62 scale. Arrow tip is aligned with the projectile origin. All ten colors share these proportions.
- 0.08-second draw/string tension, snap, 0.12-second recoil response, pooled release flash, short trail, target-color impact burst and pop audio, original segment mesh punch, and recovery.
- Impact visuals use one pooled presentation object with the original target FBX mesh. Gameplay commits the hit immediately. Draw replaces the old 0.08-second idle delay so visual polish does not reduce level solving time.
- Lowest remaining ammo wins selection; ties use earliest slot arrival. One flight, one ammo and one segment per successful hit. Fog protection, four open slots, VIP and three ability unlock slots preserved.
- Removed 74 unused scene objects from the disabled Vehicles display copy, duplicate UI Canvas and obsolete hit spark. All had no outside serialized references.
- Marked 46 fixed environment/slot objects plus the Directional Light and Global Volume static. Animated/pool objects remain dynamic.

## Changed scripts
- WoolArrowPool.cs: serial firing sequence, pooled effects, recovery, invisible reset and impact presentation.
- WoolAuthoredLayout.cs: serialized visual bindings and resting rotations.
- WoolGameController.cs: ammo/arrival priority and presentation ticking; draw replaces the old shot delay.
- WoolSceneRig.cs: ammo and unlock label spacing.

## Validation
- Live Unity Editor compilation: no Console errors or warnings.
- Levels 1–8 completed through their solution orders with four normal slots.
- Per hit: exactly one segment removed and one ammunition spent; maximum one arrow in flight.
- All ten color variants: successful hit, hidden arrow at impact, correct parent restored.
- Fog-entry cancellation passed without a hit.
- 100 repeated pooled shots: 100 hits, identical scene object count (2030 before/after), zero managed bytes allocated in the measured shooting loop. This measurement excludes the rest of the game and audio playback.
- Verified no crossbow descendants marked static, 320 mesh references use the original designer FBX, and zero missing scripts.
- Captured and inspected 720x1280, 720x1560, 720x1600 and the actual Unity Game view.
- Representative Editor Game-view snapshot: 126 batches, 21255 triangles. This is whole-scene Editor data, not an Android benchmark.

## Limitations
Low-end Android 30 FPS, device audio balance and final on-device visual tuning still require a device build/profile. No APK build was performed for this visual overhaul.
Backups are in Docs/Backups/CrossbowOverhaul. No persistent Editor scripts or packages were added.
