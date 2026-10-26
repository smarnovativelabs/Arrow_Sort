# Portrait camera and ability progression - 2026-10-23

## Applied
- Camera pitch remains at the current authored 35-degree rescue / 90-degree vehicle view. Projected visible geometry determines the split on every level and orientation change. The rescue camera fills behind the HUD instead of reserving an empty upper band. Full dragon-head clearance and vehicle travel lanes remain included.
- Static containers first appear at level 16. Moving rail containers first appear at level 21. Early manual overrides with incompatible source mechanics are bypassed.
- Unlock Slot, Remove, Sort and Amplify unlock at levels 2, 4, 6 and 8. Locked controls are dimmed and display their level.
- Save-backed gift and tutorial masks award one charge once per unlock and record completed lessons. Existing inventories remain intact.
- Guided levels present full slots, a blocker, buried matching wool and fog-covered wool. Dark overlay blocks unrelated input and an animated hand points at the required action. Remove has a second target-selection step.
- Every activation has visible feedback: slot pulse, lifting/spinning removal, a traveling body-color pulse, and an Amplify activation halo. Sort brings a loaded color forward; Amplify now permits fog targeting with extended range during its existing timer.

## Verification
- RuntimeValidation.txt: 119 PASS, 0 FAIL, including all four lessons, single charge consumption, persisted completion, duplicate gift prevention, Amplify fog targeting and slot/vehicle camera containment.
- Camera matrix: 10 levels (1,2,4,6,8,15,16,20,21,42), 4 portrait sizes (720x1280,720x1600,960x2658,2048x2732), with and without simulated top/bottom insets.
- 100 generated levels checked for mechanic gating. Boundary evidence: level15 no containers, level16 static containers, level20 no moving source, level21 moving source.
- Phone/tablet final screenshots captured and inspected. All tests used save-isolated preview. No device build performed.
- Runtime validator removed from Assets after testing; its source is retained here for review. Final small animation polish preserves removal rotation and uses a stronger traveling Sort pulse.

The camera preserves object proportions; sparse layouts and different aspect ratios naturally retain some background around the board rather than stretching geometry.

Final cleanup note: tutorial icon reuse was corrected to use the existing LayeredIcon0-3 sprite assets because footer roots use WoolAbilityTileGraphic, not Image. All four guide icons were retested successfully through Unity CLI after rebuilding. A stale temporary-file compiler reference was recovered; the validator was then removed through AssetDatabase.DeleteAsset rather than direct filesystem removal. Existing AdMob unused-field warning and Daily Tasks teardown error are outside this change.
