# Designer integration — 15 October 2026

The current scene uses DesignerContent.asset for ten vehicle colors, crossbows, arrows and eight baked designer paths. Medium and large vehicle mesh arrays may be populated later; empty entries fall back to the current car model. Four top slots are open, bottom slot 0 is VIP, and the remaining bottom slots unlock through the extra-slot ability. Rewarded slot ads are intentionally unavailable until a provider is connected.

Gameplay arrows and crossbows are authored pools. A vehicle travels to a reserved slot, plays smoke, reveals the crossbow, then participates in the single-projectile scheduler. Hit completion spends one ammunition and removes one matching segment. The arrow is deactivated and reset for reuse. Eligible crossbows prioritize the fewest remaining ammunition, then arrival order.

Validated in the live Editor:
- Level 3: 40 vehicles, 280 segments.
- Four selected vehicles produced 30 hits and reduced segments from 280 to 250.
- Level 4: 39 vehicles, 270 segments.
- All ten arrow instance IDs survived the level change unchanged.
- Slot states progressed 11110000 -> 11110100 -> 11110110 -> 11110111; VIP stayed locked.
- Console error query returned zero entries after these checks.
- Repaired the missing dragon-head mesh reference in scene and prefab while preserving its existing rig and Animator.

Inspector level preview: select the WoolGameController component, set Inspector Level, enable Open Inspector Level On Play, then press Play. During Play the component context menu also contains Open Inspector Level.

Limits: these checks do not prove every generated level solvable, Android build success, all warnings removed, complete mobile/tablet visual parity, or the long-term environment/level-count roadmap. The full HUD was not included in the offscreen world captures. Existing missing kitten art and the old fog material still need a replacement/restoration pass; the broken fog renderer is disabled. The single designer background is active.
