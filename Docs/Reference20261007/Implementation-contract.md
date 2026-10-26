# Core puzzle revision — 7 October 2026

Primary evidence: `Screen_Recording_20261007_112414.mp4`, 276.696 seconds, 6,596 decoded frames. All one-second samples were visually reviewed; six-frame-per-second motion sheets cover departures, turns, collection and completion. Decoding every frame is not a claim that every frame was manually inspected.

## Observations

| Time | Evidence |
|---|---|
| 00:00–00:14 | Six-piece tutorial. Purple, blue and red; three sizes. Hand changes target after input. Vehicles leave along arrows, turn at edges, approach reserved racks, then become winding spools. Concurrent collection is allowed. |
| 00:04–00:06 | Red departs right, turns upward, docks; blue follows. Brief yellow positive feedback appears at departure. |
| 00:08–00:11 | Blue leaves left and turns upward; winding strands connect spools to matching sections. Counts decrease progressively. |
| 00:14–00:19 | Kitten saved banner, dragon tumble, orange transition, Level 2 and fog instruction. |
| 00:20–01:58 | Settings, home, shop, task rewards, locked rank/event/team features; outside the current core-puzzle scope. |
| 02:00 | Level 2 starts: 28 pieces in a diamond, four open top slots, three unlock slots plus a separate VIP offer. Upper dragon sections are behind fog. |
| 02:03–02:31 | Multiple collectors; blocked-tap symbols. Four occupied slots constrain choices. |
| 02:32–02:33 | “Need to unlock more slots” warning. Player must wait or use a power. |
| 02:34–03:47 | Dragon advances around turns while spools wait for matching visible colors. The board gradually clears. |
| 03:49–03:50 | Unlock activation animation. |
| 03:59–04:00 | Rainbow-yarn power activates. A countdown is visible afterwards; exact full duration still needs measurement. |
| 04:01–04:02 | Remove selection mode and targeted box removal. |
| 04:07–04:13 | Kitten saved, confetti, Flawless reward card; Next +50 and optional video x3 +150. |
| 04:14–04:34 | Level 3, settings, cat skill/help, restart, return to home. |
| 04:35–end | Recording ends in device controls; not game design evidence. |

## Asset and architecture findings

- Reuse 45 vehicle prefabs backed by three shared size meshes in `Assets/Art/VehicleHD`.
- Reuse `Kitten.fbx`, `DragonHead_Refined.fbx`, `SharedDragonBraid.asset`, `SharedSlot.asset`, existing wool textures and the canyon environment.
- Imported kitten and dragon FBXs contain mesh parts but **no AnimationClip assets** (queried in Unity). Animate these models; do not substitute primitive characters.
- Existing `Knot.GroundVehicleMotion`, spline and collection code is a six-piece prototype. It hardcodes arrays, 36 sections, seven slots and uses stale editor paths. Reuse suitable geometry/routing concepts; do not run it beside a second active game controller.
- The active `WoolGameController` creates its entire scene at runtime. Replace that behavior with serialized authored references and resettable pools.
- Existing Epic Toon FX supplies confetti, dust and audio. No named hand/thumb asset was found in the relevant asset-name inventory; any added feedback art must be recorded as new.

## Acceptance gates

1. Scene, cameras, UI, environment and pools visible before Play. Level reload preserves object identities.
2. Level assets configure counts, shapes, positions, arrow directions, yarn, paths, slots and tutorial sequence. Level 2 has 28 reference pieces, not a generic 15-piece rectangle.
3. Swept full footprint blocks partial overlaps. Check on selection; blocked action reserves no slot. Moving objects remain obstacles until clear.
4. Reserve slots atomically at departure, prefer the departing side, and follow authored edge routes with visible turns and docking.
5. Progressive matching collection, stable counts, no negative balances or premature wins. Pool activation/reset replaces per-shot construction/destruction.
6. Level 1 tutorial hand advances only after correct input. Level 2 has meaningful blocking and slot pressure. Invalid, repeated and rapid taps are safe.
7. Distinct departure, turn, docking, winding, dragon reaction, rescue and transition animations using existing art.
8. Replay/retry/home/next restore state correctly; maintain existing progress and booster behavior.
9. Compare captures at 9:16, 9:18, 9:19.5, 9:20 and 9:21. Keep world proportions and safe UI controls.
10. Measure active and heavy gameplay: >=30 FPS, <=33.3 ms target and <200 batches. Report actual measurements and remaining gaps; static code checks do not meet this gate.

Explicitly excluded: multiplayer, ranking, weekly/global competitions, teams, clubs, social features, seasonal/autumn events. Their appearance in footage does not authorize implementing them in this revision.

Unresolved evidence: exact Level 2 full yarn sequence, exact power duration, imported model orientation/material appearance, full tutorial timing. Mark approximations; do not present them as extracted constants.
