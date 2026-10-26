# Gameplay speed control — 2026-10-12

Added an authored gameplay-header button that switches between x1 and x2. The game controller multiplies the simulation delta used by snake advance, vehicle travel, winding, and game-time power duration. Unity Time.timeScale is untouched, so menus, ads, and celebration animation timing keep their existing behavior.

The multiplier resets to x1 on level load. The entrance phase retains its existing authored fast approach; a selected multiplier takes effect after entry. The button is unavailable through the modal overlay and its handler ignores paused, ended, transitioning, and menu states. The tutorial's existing hidden gameplay header also hides this control.

Source: WoolHud.Speed.cs; controller event binding, reset, simulation delta, and teardown; HUD hint spacing. The button is saved in Assets/Scenes/Wool Crush.unity. It does not regenerate any level definitions or change manual editor drafts.
