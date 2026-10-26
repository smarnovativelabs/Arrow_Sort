# Opening, home, boosters and cat abilities

Research date: 2026-10-07. The user-provided recording is the preferred reference for the opening. Online material spans different versions; prices and feature availability must remain configurable.

## Local recording: directly observed

Source: `C:/Users/Muneeb Nazir/Downloads/First Level Video.mp4`. Duration 42.37 seconds; 720 x 1624 pixels. Inspected timestamped frames through the ending. This recording contains the opening tutorial, not a home-menu tour or booster demonstration.

| Time | Observed flow |
|---|---|
| 0:00 | Red launch screen with white game logo. |
| 0:05–0:10 | Blue loading screen: logo, quote, knitted dragon holding a dressed kitten, percentage bar. |
| 0:12–0:13 | Orange transition screen with standing cat and dragon. Play Games achievement overlay is platform UI. |
| 0:14–0:15 | Forest playfield appears directly. Winding sandy route ends at a kitten; cyan portal at the distant end. A six-piece tutorial board appears below the rack. No home menu is shown before this tutorial. |
| 0:20–0:24 | Dragon approaches/breathes fire, kitten displays a heart and Help bubble. Hand points to bottom-right purple down arrow. Tutorial teaches color matching. |
| 0:25–0:26 | Purple piece has become a horizontal spool. A purple yarn strand connects it to the matching dragon body. Next prompt teaches arrow-direction movement; hand targets top-right red piece. |
| 0:28 | Purple and red spools collect simultaneously. Prompt teaches that larger boxes remove more dragon wool; hand targets upper blue right arrow. |
| 0:30–0:31 | Prompt warns that the dragon reaching the kitten ends the game. Hand targets upper-left purple up arrow. |
| 0:33–0:36 | Remaining lower-left blue then lower-middle red pieces leave. Visible spool counters decrease; multiple strands can collect concurrently. |
| 0:38–0:40 | Final red spool finishes; body disappears, head collapses/rolls and kitten celebrates. No reward amount or victory dialog is shown. |
| 0:41 | Orange character transition returns. The recording ends without showing the in-game home. |

### Tutorial board and rack

| Position | Color | Arrow | Relative size |
|---|---|---|---|
| Upper left | Purple | Up | Tall/large |
| Upper middle | Blue | Right | Medium |
| Upper right | Red | Right | Small |
| Lower left | Blue | Left | Small |
| Lower middle | Red | Left | Medium |
| Lower right | Purple | Down | Tall/large |

Observed selection order: lower-right purple, upper-right red, upper-middle blue, upper-left purple, lower-left blue, lower-middle red. These are six pieces, not six identical square tiles. Four upper rack recesses and three lower recesses are visible (seven total); this does not prove all seven are enabled for normal levels. The tutorial has no booster row, coin header or level-map controls. Values visible after collection starts are remaining capacity, not reliable initial capacities.

### Presentation implications

The dragon is a continuous thick knitted body, with separate head/tail and alternating colored sections. Wool travels from matching body sections to winding spools. The board pieces visibly differ in length. The forest route occupies about the top half of the portrait screen, the rack divides the screen, and a sparse beige tutorial area occupies the lower half. One kitten is visible. The recording proves danger feedback and a game-over warning; it does not establish damage intervals or whether the tutorial allows actual failure.

## Boosters: online gameplay report

An April 2026 [hands-on gameplay analysis](https://www.cyzone.cn/article/829446.html) reports four consumables. English names correspond to public gameplay screenshot labels; behavior and prices below are secondary, version-specific evidence.

| Booster | Reported effect | Reported coin price |
|---|---|---:|
| Unlock | Adds a spool slot. | 800 |
| Remove | Removes one spool/piece from the lower puzzle area. | 600 |
| Sort | Reorders upcoming dragon yarn colors. | 700 |
| Amplify | Temporarily removes fog restrictions so spools can collect hidden/upcoming yarn. | 500 |

The report describes ad-supported trials. Exact selection rules for Remove, Sort's algorithm, Amplify's duration, inventory grants and unlock levels remain unobserved. Generic online claims about fireballs, bombs or lasers were not accepted as evidence for this game's booster system.

## Cat abilities: progression system

The same report describes three cat attributes earned through level-completion experience:

- Health: survives more dragon fire attacks.
- Invincibility: temporary protection followed by dragon knockback.
- Freeze: slows dragon movement.

These are separate from the four consumable boosters. Their activation conditions, durations, upgrade values and unlock thresholds remain unknown. The local tutorial shows one heart but does not open an ability screen.

## Home page and map: evidence boundary

The official [Home Collection announcement](https://play.google.com/store/apps/eventdetails/4830473752881186444) confirms that completing levels earns cards featuring Woolo and Crushpyr, and an in-game collection screen lets players browse collected cards. It does not describe a numbered level-path map, room decoration layout, or exact main-menu arrangement.

The [official App Store version history](https://apps.apple.com/qa/app/wool-crush-bus-knit-away-jam/id6743385713) mentions teams and leaderboards in version 3.8, additional areas, events and later peripheral features. The [current iPad listing](https://apps.apple.com/ro/app/wool-crush-bus-knit-away-jam/id6743385713?platform=ipad) also advertises a cat-home yarn-merging event. These are evidence for supporting features, not proof of where their buttons appear on the home page.

No verified home-screen screenshot or numbered level-select map was obtained. Browser control repeatedly timed out during this pass. The upper winding forest path in the supplied video is the active gameplay route; it should not be used as evidence of a home/level-selection map. Do not invent a map and label it a faithful reproduction.

## Required corrections to the current prototype

1. Replace the earlier tutorial color assumption with this recording's purple/blue/red layout and six-step teaching order.
2. Support different piece sizes/capacities and convert released pieces into winding spools.
3. Build launch/loading/character transitions, then enter the tutorial directly on first launch.
4. Use tutorial-specific UI and rack arrangement; avoid showing normal booster controls during this observed sequence.
5. Separate Unlock/Remove/Sort/Amplify from progression-based cat abilities. Existing Undo/Hint/Freeze controls are prototype conveniences and do not reproduce the observed booster system.
6. Add continuous knitted dragon geometry, simultaneous yarn extraction, heart/danger reactions, defeat and kitten celebration.
7. Keep the main-menu/map layout pending direct evidence. Home Collection is confirmed; a numbered map is not.

These are research findings and implementation requirements. This pass does not claim these features have been implemented or runtime-validated.
