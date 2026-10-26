# Portrait layout and opening levels

UI reference: 1080 x 1920, Expand scaling, safe-area-aware controls. Runtime layout no longer replaces this with 720 x 1600. Camera fits the visible path, slots and initial board; it excludes fog-hidden path points. Full camera viewport. Path entry is concealed by FogPortraitEntrance material; original Fog material retained.

Level 1: six vehicles, 40 matching dragon sections, six action-led crossbow lessons. Each selection waits for its crossbow to land a matching hit before the next prompt. The duck family cannot be caught during this tutorial. Level 2 has 30 vehicles; level 3 has 40; both have deterministic legal exit orders and balanced ammunition. Level 4 onward retains the existing generator and cardinal directions; mixed diagonal directions were not introduced.

Validation: Unity compilation succeeded and Console errors zero. Live isolated tutorial reached Won, 6/6 selected, zero remaining sections. Full legal exit witnesses verified for 6, 30 and 40 vehicles. Camera bounds and full viewport passed 18 combinations (3 levels x 6 portrait sizes), including simulated safe-area insets. Runtime tablet tutorial rendered at 768 x 1024. Physical-device testing was not performed.

Backups: Docs/Backups/PortraitResponsive-20261016 and Docs/Backups/TutorialPortrait-20261016-1758.
