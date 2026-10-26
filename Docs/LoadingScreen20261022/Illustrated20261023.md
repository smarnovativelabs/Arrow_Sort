# Illustrated loading screen - 2026-10-23

Saved background: Assets/WoolGame/Designer/UI/Loading/RescueCastle.png
Saved animated title: Assets/WoolGame/Designer/UI/Loading/RescueTitle.png
Generated with built-in imagegen from the supplied loading reference.

Prompt 1: Remove only the title/logo and loading bar/text, inpaint sky and stone bridge, preserve all characters and setting.
Prompt 2: Extract only the crown/wood/gold title sign, exact text Arrow Sort Puzzle: / Rescue Ducks, transparent background.

The title pulses, floats, tilts gently and has a looping shine. Startup progress drives the green fill. Title/status/bar fit inside Screen.safeArea; illustration covers the display.
Unity compile completed with 0 errors and 0 warnings. Play Mode verified illustrated screen, animated title, progress=.62, and EndLaunch hides the screen. Tall 960x2658 framing inspected. Corrected the vector fill center to keep the left edge inside its track. Device builds and additional tablet visual review not performed.

## Startup fill fix - 2026-10-23
Natural startup baseline: frame 1 progress=0, frame 2 progress=1 after a 5.964-second stall. Home opened, but intermediate fill values were skipped.
Fix: render zero first, advance presentation with bounded unscaled delta (maximum 1/30 second per frame) over 2.5 seconds, render 100%, hold for 0.35 seconds, then open Home. SetProgress now updates the fill immediately.
Natural startup verification: fixed.csv records 107 rendered samples, intermediate values from 0 to 1, full width 742 at 100%, HOME_ACTIVE=True. Loading overlay then inactive. Temporary Editor observer removed after verification.
