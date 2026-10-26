# Reference difficulty update — 2026-10-08

## Implemented and saved
- All 50 campaign levels start with four working slots. Three additional slots use the rewarded-video callback and reset on loading a level.
- Small/medium/large capacities are 4/6/10; all level assets were rebuilt with exact matching wool quantities per color.
- 19 Easy, 21 Medium, 10 Hard; every fifth level is Hard.
- Dense orthogonal and diagonal layouts, stronger directional dependencies, and delayed color batches replace variable starting-slot difficulty. Base snake speed and winding rate are shared across generated levels.
- Level 3 has 45 vehicles, level 10 has 36, level 25 has 42, level 50 has 45. Hard boards aim for two initially available departures; medium four; easy eight.
- Uniform board rotation is applied to rendering and departure directions. Departure collision uses oriented box casts. Diagonal movement intersects the board boundary before turning toward a reserved slot.
- Partial wool amounts remain paired with their colors when Sort/Remove is used.

## Reference evidence
User supplied shared image.jfif and shared image (1) through (14).jfif from Downloads. They show mixed vehicle sizes, interlocking arrows, four active slots, extra unlocks, waiting spools, diagonal silhouettes, conveyors, numbered dark objects, and dual dragons. Still images do not establish the numbered objects' behavior. Conveyors and dual dragons are not implemented by this update.

## Evidence and limits
Campaign authoring successfully saved 50 assets and the scene. Its guards checked four slots, exact capacity/color totals, no starting overlaps, pool limits, and a geometric departure witness. Unity returned zero error entries after the initial campaign build. A level 25 portrait preview exposed undersized framing; the saved board scale was enlarged from 0.58765 to 0.6936197.

These are NOT timed difficulty results. Prior campaign playthrough reports predate the new capacities and layouts and do not validate this revision. Exact zero/one/two/three-slot assistance requirements, timed solvability, diagonal runtime departures, and fractional booster behavior still require play balancing. Rewarded callbacks are connected; production ad delivery is not verified. No full campaign tests were run in this update.

CSV fields in Reference-Difficulty-20261008.csv: level, difficulty, vehicle count, starting slots, initial legal moves, snake batch size, board rotation.

Final enlarged Level 25 portrait capture inspected: Captures/CoreAcceptance/reference-layout25.png. Preview used isolated progress and was ended after capture.

Vehicle proportions follow-up: generated levels 3–50 now use small .62 x .62, medium .52 x .80, large .52 x .94 logical footprints. All assets rebuilt with authoring guards; Unity error log empty. Captures/CoreAcceptance/vehicle-size25.png visually inspected. Tutorial and Level 2 dimensions preserved. This does not establish timed solvability or required assistance.

## Runtime observation and duplicate audit
- In isolated Play Mode, Level 25's first authored move (vehicle 6) was accepted through SelectTile. After departure it appeared as a purple spool in the reserved right slot, with remaining capacity 5. At elapsed 19.84 seconds the game was still Playing with 276 wool units remaining. Screenshot: Captures/CoreAcceptance/diagonal-departure25.png. This proves one diagonal departure/docking/partial collection, not a whole-level solution or all movement directions.
- Preview was ended and timescale restored. No campaign completion was credited.
- SHA256 content audit of all Assets .cs and .shader files found one exact-duplicate group: five 82-byte MaxSdk deprecated placeholder files. Their contents are only a vendor deprecation comment and their metadata carries SDK export labels. They were preserved as package-owned files; no redundant gameplay implementation was established by this exact-content audit. Semantic duplicates and binary art assets are outside this audit's scope.

## Route variety implementation
Generated levels now cycle between the original serpentine route, its horizontal mirror, a winding S approach, and a coiled approach. Tutorial and Level 2 retain their authored routes. Runtime WoolRoutePresentation resamples the active spline into the existing road renderer and rebuilds its sewn-edge mesh once per level; its runtime mesh is destroyed with the component. Non-tutorial kitten placement now uses the same route distance as the loss threshold (length minus .7), replacing the fixed left-side placement.

Campaign regenerated and scene saved; Unity Console returned zero errors. Play Mode captures for levels 5, 6, and 7 were visually inspected at 720x1600: mirrored-route5.png, winding-route6.png, coiled-route7.png under Captures/CoreAcceptance. Preview progress was isolated. Coiled route length observed: 32.53942 world units. Timed balancing must account for these different path lengths; this is not evidence that all campaign levels are beatable or that extra-slot counts are calibrated.
