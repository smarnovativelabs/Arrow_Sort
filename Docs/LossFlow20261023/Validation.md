# Loss flow validation - 2026-10-23

Implemented in Assets/Scenes/Wool Crush.unity.

- Generated ClearSlotsPopup.png, RetryChrome.png and LossCharactersSheet.png with built-in imagegen.
- Character sheet: 4 columns by 2 rows, 8 saved Sprite frames, 8 fps, shared pivot (0.5, 0.12). No wool or coins behind the characters.
- Canonical payment coin uses Assets/WoolGame/Designer/UI/Unlock/DuckFootprintCoin.png. Popup coin and broom emblem were corrected to the same webbed footprint.
- Pages use a centered 760x1320 design fitted uniformly inside Screen.safeArea. Phone 720x1600 and tablet 1536x2048 rendered and inspected.
- Coin revive in isolated WoolProgress preview: 4 occupied slots, coins 2400 -> 1200, slots -> 0, revived=true, phase=Playing after removal/retreat/emergence sequence.
- Cancel switches offer to retry. Sprite frames advance during Play Mode.
- Unity compilation: 0 errors, 0 warnings. Final current Console: 0 errors, 0 warnings.
- Rewarded callback remains connected to the existing ad service; real device video playback was not validated.

## Image prompt set (built-in tool)

1. Recreate only the supplied Clear Slots popup on transparent background, preserving duck, gold rim, cream panel, broom, bubbles, title, description, 1200 and Free buttons, and red X. Remove the game board.
2. Create an exactly 4x2 equal-cell animation sheet of the supplied crying yellow duck and laughing red dragon, both full body in every cell, fixed scale/camera/baseline, gentle sob/blink/tear and laugh/wing/breath motion. Remove wool, yarn, coins, background, UI, text and floor; transparent background.
3. Recreate only the supplied portrait retry UI on transparent background: home icon, Keep Going! ribbon, Try again and rescue the duck family! text and green Retry button. Preserve layout, remove characters and props to allow separate animation.
4. Correct only the popup coin and broom medallion to match the supplied canonical three-toed webbed duck footprint coin; preserve remaining art and text.
