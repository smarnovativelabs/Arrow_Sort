# Campaign, startup, daily tasks and wheel update — 8 October 2026

## Saved implementation

- Scene: `Assets/Scenes/Wool Crush.unity`. Existing controller/rig retained; no parallel game system added.
- Bottom navigation: Shop, Home, Map (user clarification). Home Daily Tasks, Spin and No Ads shortcuts restored. No Ads opens the store, or shows remaining time while a wheel reward is active.
- Versioned `WoolProgress` save owns current level, completed stages, attempts, first-try wins, streaks and coins. Profile displays recorded statistics instead of formulas. Unreliable prototype counters are backed up during migration; wallet retained.
- Startup: Wool Crush branding, motivation, progress bar; new players go to the first level, returning players go Home. Map unlocks and play labels use saved progress.
- 50 saved level definitions: 20 Easy, 20 Medium, 10 Hard. Every multiple of five is Hard. Eight layout families; dense milestones 10/25/50 have 36/42/45 boxes. Shared pools expanded to 48 vehicles and 512 yarn pieces.
- Normal levels move immediately at twice the level's base speed until the first collecting area, then at normal speed. Level 1 instead emerges from a shaded portal in about 2.4 seconds and stops near the kitten. A transparent red edge warning pulses as danger rises.
- Spool bodies and windings use the selected box's color; collectors continue independently for all eligible colors.
- Daily tasks: rescue three kittens (100 coins), collect 100 wool (75), match four colors (75). Only completed objectives can be claimed, once each. Tasks reset at local midnight. Collection is recorded from real winding events and periodically saved; level completion, pause and quit also persist progress.
- Daily wheel: available from Level 1, one spin each local calendar day. Eight equal visual sectors: 25 coins / Try Again / 50 coins / Try Again / 100 coins / Try Again / 150 coins / No Ads for 24 hours. Normal spins choose uniformly among the first seven sectors. Every 30th consecutive daily spin (30, 60, …) guarantees sector eight. A missed day restarts the streak. Empty outcomes consume the daily spin; UI says Try Again Tomorrow. Streak, result and reward are saved before animation, whose final angle matches the outcome.
- The No Ads reward stores an absolute UTC expiry, suppresses forced interstitials and configured banners without changing the permanent-purchase flag. Optional rewarded videos remain available. Home shows the remaining reward duration.

## Evidence from this update

- Unity compiled and saved the scene. Final Console read returned zero error entries.
- `CoreAcceptance/rules.txt`: 15,768 assertions covering all 50 authored levels, non-overlap, legal departure witnesses, exact per-color capacities and one active gameplay owner.
- `CoreAcceptance/campaign-progress.txt`: 117 isolated assertions, including daily reward idempotency, 30 consecutive days, milestone-only No Ads, missed-day reset, expiry and serialized state round-trip. Persistent player save unchanged.
- `CoreAcceptance/campaign-first-five.txt`: Levels 1–5 completed through actual public selection / Next UI, at 3x simulation time. Peak three concurrent collectors. Player save and scene object identities unchanged.
- `CoreAcceptance/campaign-level50.txt`: Hard Level 50 completed, 45 moves, 157.80 simulated seconds, two concurrent collectors. Player save and scene object identities unchanged.
- `CoreAcceptance/tutorial-entry.txt`: portal active, dragon stationary after the entrance, distance to kitten 1.26 world units without input.
- Inspected screenshots: `Captures/CoreAcceptance/campaign-home.png`, `campaign-map.png`, `campaign-profile.png`, `daily-tasks.png`, `daily-wheel.png`, `daily-wheel-tablet.png`, `tutorial-danger.png`. Tablet wheel was captured at 768x1024; phone captures 720x1600.
- Visual inspection caught and repaired the danger sprite being imported as an empty multiple-sprite sheet, and the custom wheel pointer lacking a CanvasRenderer.

## Reference and validation limits

- User recording and supplied images are the observed visual evidence. `Docs/Reference20261008/YouTube-level-design.md` lists 20 located videos. They were not watched: browser helper failed to start and direct YouTube opens returned cache misses. No claim of extracting layouts from unseen videos.
- All 50 levels passed content rules; only 1–5 and 50 received live playthroughs in this update. Human difficulty tuning and remaining timed runs are still pending. Editor timings are not device profiling.
- Billing remains unavailable and production ad IDs need configuration. The wheel's temporary entitlement logic is implemented; live network delivery and real purchases were not tested.
- Calendar and entitlement data are local-device saves. A production server clock/account backend is not present.
- Remaining broader game work includes closer character/scene art fidelity, the fractional-wool Sort/Remove interaction audit, broader device/layout checks and remaining campaign balancing. This update does not establish that the entire reference game has been cloned.

## Wheel arrangement revision

User requested four coin slots and three Try Again slots alternating. Saved the new layout and reward mapping together: 25, Try Again, 50, Try Again, 100, Try Again, 150, No Ads 24h. Existing earned rewards, daily spin usage and the 30-day guarantee are unchanged. Earlier reward-check counts predate this arrangement revision; no new tests were run for this small edit.
