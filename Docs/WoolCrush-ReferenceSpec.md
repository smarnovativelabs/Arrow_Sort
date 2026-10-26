# Wool Crush reference and clone notes

Research date: 2026-10-07. Scope: public Google Play listing and its screenshots, a third-party gameplay recording, and secondary gameplay analysis. This is a working implementation brief, not a claim that every current game mode or rule has been observed.

**Latest reference:** [Opening, home and abilities research](WoolCrush-Opening-Abilities-Research.md) analyzes the user-supplied 42-second recording. Its purple/blue/red tutorial, different piece sizes, seven visible rack recesses and direct launch-to-tutorial flow supersede the earlier YouTube-based tutorial assumptions below. It also records the four reported boosters, three cat abilities and official Home Collection feature. Exact home/map layout remains unverified.

## What the public listing confirms

- Wool Crush is a single-player, offline, stylized puzzle game. The listing advertises ads, in-app purchases, events, and hundreds of levels, and marks it available on Windows through Intel technology.
- Its text describes matching a cannon to the dragon's wool color, sequencing around blocked pieces, and rescuing cats. It calls the puzzles deterministic and says there is no timer or pressure.
- The store screenshots show a portrait playfield split into a wool-dragon route at the top, a small color/spool rack in the middle, and a dense grid of colored wool tiles with directional arrows at the bottom. The route varies between loops, spirals, and more angular shapes.
- Public screenshots show several colors, per-slot counts, and booster labels. Some screens include seasonal scenery and rescue targets beyond cats.

## Working rules model

**High confidence:** Tap a legal color tile/cannon, match its color to visible wool on the dragon, and clear the board in an order that respects blockers. The cat or another small animal is the rescue objective.

**Medium confidence from screenshots and third-party analysis:** Each board tile has a cardinal exit arrow. A tile is blocked while another tile intersects its arrow's path. Releasing a tile sends its color to a limited spool/cannon rack; matching segments are removed from the dragon, and a depleted spool frees its rack slot. Rack capacity and spool length create planning pressure.

**Still unverified:** Exact hit points, dragon damage timing, initial tile/spool capacities, ability durations and unlock thresholds, level-map structure, event/store screen layouts, and the complete level catalog. The local video confirms dragon fire and a game-over warning, and the newer research note records the tutorial steps and secondary booster effects/prices. Keep numerical tuning configurable.

## Gameplay footage observations

The third-party [Levels 1–10 walkthrough](https://www.youtube.com/watch?v=bAZuZ1xWFZM) is an older recording (the page showed about four months old when checked). These are direct visual observations, not publisher documentation:

- At 0:05, the tutorial says “Match colors to attack the Dragon!” and shows a small two-row arrow-block board with a hand pointing at a blue down-arrow tile. This establishes the first tutorial prompt and that the player taps an arrow tile.
- By 0:15, the board is larger and irregular, with colored directional tiles, four visible color counters/spool stations across the top, and four rounded booster buttons along the bottom. Green “Free” labels appear on the booster buttons. Some top stations show “Unlock +” or “VIP +”.
- Around 0:30–0:35, a selected tile leaves the board and the corresponding top color count changes. The exact animation, firing order, and whether each displayed count is tile count or yarn ammo are not legible enough to assert.
- Around 34:15, dark gray numbered obstacles appear among the arrow tiles. Their rules are not demonstrated clearly enough to implement as confirmed behavior.
- This recording does not show a reliable home/map flow, full win/loss dialog, or legible booster names/effects. Its auto-generated captions are not useful for mechanics verification.

The separate [Jungle Adventure levels 1–5 recording](https://www.youtube.com/watch?v=eop700NzKHg) shows a fuller screen at 0:02: a looping route, wool dragon, cat with heart markers, a percentage progress indicator, settings control, colored top counters with empty/locked slots, the arrow board, and four bottom boosters. At 0:46, translucent blue overlays and numbered blockers are visible across parts of the board. These are confirmed visual elements; their rules, progression, and booster effects remain unknown. Both videos are third-party recordings from roughly four months ago, so seasonal and version-specific presentation may differ from the current listing.

The listing and these recordings establish the broad screen structure. The supplied local video adds direct evidence for the opening sequence, size-dependent capacity, concurrent extraction and danger feedback. The game itself was not opened on this desktop: the available computer-app inventory had no visible Play Games window, although a background `GooglePlayGamesServices` process was present.

## Reference screen and content plan

1. Portrait gameplay screen: level and currency at the top; knitted dragon route and rescue target above; spool slots in the center; arrow-block puzzle below; concise boosters along the bottom.
2. Core interactions: tap a free arrow tile, show why a blocked tile cannot leave, move a cleared tile toward a spool slot, consume matching dragon wool, update counts, and handle a full rack.
3. Level flow: deterministic authored tutorial boards, then reproducible generated boards with additional colors, rows, path shapes, and blockers. Save level and currency locally so the basic game works offline.
4. Supporting flow to build after the core loop: home/level selection, pause/settings, win/loss, booster/extra-slot offers, and event surfaces. Store events are confirmed, but their full screens and live-service behavior are not available from the listing.
5. Presentation: original knitted materials, rounded bright tiles, high-contrast white arrows, a colorful wool dragon, expressive rescue characters, soft movement, and readable counts. Use newly made project assets rather than copying store artwork.

## Project starting point

`Assets/Scenes/Wool Crush.unity` is the only enabled build scene. A `WoolGameController` builds the scene presentation and HUD at runtime. The project has a moving wool route, kitten models, matching colors, five spool slots, deterministic level generation, local level/coin saves, pause/retry/hint/undo/freeze controls, and a completion-ad flow. Level 1 now uses the six-tile, two-row tutorial layout observed in the video, starts with the blue down-arrow color, and displays remaining ammo at occupied spool slots instead of on unselected board tiles. Later levels remain generated rather than extracted from the reference. All three duplicate vehicle mesh files under `ForestArt/VehicleHD/Meshes` were consolidated into the canonical `Art/VehicleHD/Meshes` copies, and the 45 ForestArt prefab references now point to those shared meshes.

## Duplicate cleanup

- Removed three byte-identical `Vehicle_{Large,Medium,Small}.asset` copies from `Assets/ForestArt/VehicleHD/Meshes`; retained the canonical copies under `Assets/Art/VehicleHD/Meshes` and rewired 45 ForestArt prefab references.
- Removed 18 redundant Android Gradle backup/disabled copies from `Assets/Plugins 1/Android`; retained one historical backup and the distinct active template.
- Scanned 3,273 Unity `.meta` files and found no duplicate GUIDs.
- The only remaining identical file contents are five 82-byte, comment-only AppLovin deprecation stubs with distinct platform-specific names under `Assets/MaxSdk/Scripts`; they remain in their vendor paths because they are separate SDK placeholders, not repeated gameplay code.

## Sources

- [Google Play: Wool Crush](https://play.google.com/store/apps/details?id=wool.match.color.sort.jam.puzzle&hl=en) — official description, features, categories, update date, events, and screenshots.
- [YouTube: Wool Crush Levels 1–10](https://www.youtube.com/watch?v=bAZuZ1xWFZM) — third-party visual gameplay recording; useful for the tutorial cue, board layout, counters, and booster placement, but older and without a reliable description.
- [YouTube: Wool Crush Jungle Adventure levels 1–5](https://www.youtube.com/watch?v=eop700NzKHg) — third-party visual gameplay recording; shows the dragon route, rescue hearts, progress indicator, locked counter slots, and numbered/overlaid board obstacles.
- [App Store: Wool Crush — Escape Traffic Jam](https://apps.apple.com/us/app/wool-crush-escape-traffic-jam/id6743385713) — official companion listing and short how-to description.
- [Cyzone gameplay analysis](https://www.cyzone.cn/article/829446.html) — secondary observation of the screen zones, spool capacity, dragon/cat pressure, and later fog mechanic; treat those details as version-specific until confirmed.
- [LevelSolve level guide](https://levelsolve.com/wool-crush/) — unofficial guide reference for directional blocking and slot pressure; individual level details were not used because the guide contains inconsistent color labels.
