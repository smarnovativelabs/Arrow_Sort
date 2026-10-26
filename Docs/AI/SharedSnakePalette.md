# Shared snake palette and gloss

The original snake atlas and material remain unchanged. Vehicle prefab, all eight authored crossbow slots and their ten arrow variants use Shared Snake Gloss. Ten rows are sampled from the original snake tail atlas, with deeper body shading and compact gloss highlights per the requested dark finish. Neutral hardware and white direction symbols remain intact.

Assets: Assets/WoolGame/Designer/SharedSnakePalette; shader: Assets/WoolGame/Shaders/SharedSnakeGloss.shader. Palette indexes retain the original gameplay ordering. WoolArt.Palette and authored trails share the same ten identity colors. UV3 selects the row; UV4 carries painted highlight coordinates. Generated meshes preserve skinning. No runtime texture recoloring, additional lights, or light bake.

Validation: shader supported, no shader compiler messages or Console errors; 101/101 generated meshes have full UV3 and UV4; all ten palette rows present; 80 authored bow/arrow pairs intact. Isolated level-30 preview inspected. Baseline 215 batches / 23 SetPass / 225 draws; final 215 / 26 / 225. Android device build/performance not tested. Preview ended without campaign progress changes. Backup and comparison images: Docs/Backups/SharedSnakePalette.

Update: shader now uses UniversalForward with main-light diffuse response, dark body cap and localized emission gloss. Roof UVs corrected to X/Z projection. Lit preview 217 batches / 27 SetPass / 227 draws; zero compiler/Console errors. User also changed original snake/environment materials to URP Lit. White speckle artifact observed across these surfaces; not explained by active particles, direct light, specular/environment keywords, or camera MSAA in initial isolation tests. Temporary tests restored; root cause remains unconfirmed.
