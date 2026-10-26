# Layered fantasy home — 21 October 2026

Saved in `Assets/Scenes/Wool Crush.unity` using Unity CLI 1.0.0-beta.13 and the running Unity 6000.3.20f1 Editor. No Unity MCP calls were used.

## Artwork and animation

Project assets are in `Assets/WoolGame/Designer/UI/FantasyHome/`:

- `VillageCloudless.png`: final background with all baked clouds removed. `VillageBackground.png` preserves the earlier plate, which is no longer used by the home page.
- `InterfaceLayers.png`: sixteen sliced transparent sprites for controls, action icons, navigation, clouds, and fire. Each control and icon is a separate UI object.
- `CharacterLayers.png`: the dragon sprite; the old duck composite is retained as source art.
- `DuckFamilyFixed.png`: final integrated duck-family sprite with naturally attached wings in one fixed pose. No mother/wing animation remains.
- `ProtectiveWings.png`: retained earlier wing cutouts; these separate wing objects are disabled in the final scene.
- `LevelText.mat`: independent green-outline material for the live level label.
- `Assets/WoolGame/Fonts/LilitaOne-HomeSharp.asset`: 2048x2048 SDF atlas sampled at 128 points for home labels.

`WoolHomePresentation` lays out the safe-area controls and animates five clouds, dragon hover, a continuously rotating wheel with a fixed pointer, action icons, and the level-button artwork. The duck family remains static. Text size is stabilized: action cards no longer scale their captions, and the level label compensates for its parent's pulse. The Canvas uses pixel-perfect rendering. `WoolHomeFlame` animates a subdivided fire mesh with unscaled time. The background has a subtle breathing zoom. Decorations do not intercept touches.

The existing Coffee UIEffects `UIShiny` is attached to Play, with a 0.65-second sweep and 1.35-second delay: a two-second cycle. Wallet and level labels use live game data. Existing WoolHud navigation owns the callbacks.

## Validation evidence

### Final dragon/wing/text correction

- Saved through Unity CLI after generating one integrated duck-family cutout. The separate left/right wing objects remain disabled and no wing or mother animation is applied.
- Dragon lowered approximately 100 design units, with a responsive header clearance limit that also accounts for hover and rotated wing tips.
- Checks ran inside the player loop to prevent Editor GUI dimensions from distorting GraphicRaycaster results.
- Final 720x1280: zero failures for all eleven controls; dragon-to-Settings gap 32.49 pixels.
- Final 768x1024, top 24 / bottom 20 safe insets: zero failures; gap 25.55 pixels.
- Final 1080x2400, top 100 / bottom 80 safe insets: zero failures; gap 74.67 pixels.
- All three confirmed that separate wings were inactive and the 2048x2048 Home Sharp font was bound. Captures: `fixed-pose-720x1280.png`, `fixed-pose-768x1024.png`, `fixed-pose-1080x2400.png` under `Assets/Docs/FantasyHome20261021/`.
- Source inspection found continuous fractional scaling of text and text-bearing action cards. Stable glyph scaling, pixel-perfect Canvas rendering, and a higher-resolution font atlas reduce this softening. A heavily zoomed-out Unity Game view still downsamples text; native-resolution captures are the visual reference.

### Earlier home validation

- Unity compilation completed with zero errors and zero warnings.
- Final live Console snapshot: compilationFailed=false, consoleErrors=0, consoleWarnings=0.
- All eleven home controls passed safe-area containment and center-point EventSystem raycast checks at 720x1280, 360x640 (top 24/bottom 16), 1170x2532 (top 132/bottom 102), and 1536x2048 (top 48/bottom 40).
- The same responsive layout also passed at 768x1024 (top 24/bottom 20) and 1080x2400 (top 100/bottom 80) before the final cloud/wing artwork replacement. Their layout code was unchanged by that replacement.
- Profile, Settings, Tasks, Lucky Spin, Rescue Shop, Ad Pass, Shop, Map, and Home callbacks passed after clean Play Mode startup. Wallet opened its existing reward offer; no reward was requested and no purchase was made.
- Play wiring was inspected: Reference Play -> PlayPressed -> PlaySaved -> OpenCampaignLevel(CurrentLevel). A live level launch was not performed because automatic approval review rejected that validation as potentially changing persistent progress.
- Animation samples advanced from frame 9652/time 242.2478 to frame 13004/time 326.1751: cloud X -379.265 to 208.226, wing angle 2.042 to 7.763 degrees, wheel angle 306.052 to 91.797 degrees, flame clock 252.344 to 336.272, and level-button scale 1.0161 to 0.9960. Additional live captures confirmed different wing and cloud poses.

Text reports are alongside this file. Captures are under `Assets/Docs/FantasyHome20261021/`. Reports named `1043x836-safe-100-80`, `1080x1920-safe-24-16`, and `280x560-safe-0-0`, and captures named `device-*` or `phone-1080x2400-safe.png`, are diagnostic attempts made while the Editor GUI size or selected preview changed; they are excluded from the accepted device evidence. Simulated safe areas exercise layout, not physical-device rendering or performance.

Scene/source rollback copies are in `Docs/Backups/BeforeFantasyHome20261021/`. The authoring command is **Wool Crush > Author layered fantasy home**. Editor preview overrides are not serialized and are absent from player builds.

## Built-in ImageGen prompt set

The supplied reference was `C:/Users/Muneeb Nazir/Downloads/Cheerful Fantasy Village Game Menu.png`. All raster edits used built-in ImageGen, then outputs were copied into the project and sliced with Unity's Sprite Editor APIs.

1. Background plate: preserve the portrait village, lantern, mountains, waterfall, river and terrace; remove UI, text, dragon/fire, duck family/wings and reconstruct the scenery behind them.
2. Interface atlas: extract the reference's profile, wallet, settings, blank action card, task/wheel/gift/ad icons, blank green level button, blank navigation bar, shop/home/map icons, cloud, fire stream and fixed wheel pointer. Transparent alpha; no labels or numerals, which remain live UI text.
3. Character atlas: isolate the reference's mother and two babies, dragon without fire, and two outer white wings on transparent alpha; preserve character identity and lighting.
4. Final background correction: "Remove EVERY WHITE CLOUD from the blue sky ... including tiny wispy distant clouds and large clouds. Reconstruct perfectly clean cloudless blue sky with the existing blue gradient and sunlight at upper right. Absolutely no clouds at all anywhere. Keep the mountains, houses, lantern, river, waterfall, fence, terrace, lighting and camera framing unchanged."
5. Replacement wings: compact, rounded, anatomically bent white wings curved inward to shelter the babies; fluffy shoulder joints, softly rounded distal tips, small layered feathers; left C-shaped wing opening right, right wing mirrored; two isolated transparent cutouts. No broad fan shape or long pointed feathers.
6. Final fixed duck-family edit: preserve the original mother and baby faces, feather style and lighting; replace the bad composite with one coherent transparent family cutout. Wings originate below the neck and blend into torso feathers, extend slightly outward/down and curve inward around both babies. No floating shoulder caps, crescent gaps or doubled wings. Relaxed fixed pose, babies and feet visible, no scenery, fire, UI or text. Output saved to `DuckFamilyFixed.png` using built-in ImageGen.

The separated raster pieces reconstruct the reference composition; they are generated extractions and are not pixel-identical to the original flattened illustration.
