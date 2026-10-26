# Wool Crush: level and interaction references

Research date: 8 October 2026. This is an evidence ledger for the current project, not a claim that the referenced game has been fully reproduced.

## What was actually inspected

- The user's local `Screen_Recording_20261007_112414.mp4` remains the primary visual source. Its existing analysis decoded 6,596 frames across 276.696 seconds, reviewed one-second contact sheets and selected six-frame-per-second motion sheets. This research pass re-inspected the opening, the Level 2 concurrent-collection sheet and the Level 3 portion, and extracted three full-resolution Level 3 frames.
- Located 20 YouTube videos spanning Levels 1–50 and verified their returned video IDs, level titles and durations from public YouTube page metadata. **None of these 20 remote videos was played or visually inspected in this pass.** The browser tool could not start because its Windows sandbox helper failed; web opening returned cache misses. Metadata is not visual evidence. No remote video was downloaded.
- Fan-guide pages were used as a link index only. Several contain contradictory color lists, directions or mechanics. Their layout descriptions and difficulty ratings are not accepted as verified design facts.
- The user's requested cadence is authoritative for our campaign: every level divisible by five is Hard. Other levels mix Easy and Medium. This is our design rule, not a verified rule of the reference game.

## Confirmed evidence from the supplied recording

| Source time | Direct observation | Implementation consequence |
|---|---|---|
| 00:00–00:14 | Six-piece tutorial with purple, blue and red, mixed sizes and a moving hand. Different colors can collect simultaneously; at about 00:06, three strands are visible. | Teach directional exit, size/capacity and matching separately. Do not serialize collection to one global color. |
| 00:04–00:07 | Arrow-aligned departure, edge turn, rack approach, docking and progressive winding. The next move can start while previous pieces are in motion. | Reserve slots atomically; keep input available for other legal pieces; turn visibly before docking. |
| 00:14–00:19 | Rescue feedback, transition artwork, then the fog instruction in Level 2. | Give completion a clear payoff and introduce one new constraint at a time. |
| 02:00–02:02 | Level 2's 28-piece diamond and four open slots, three unlock slots, separate VIP offer. | Diamond silhouette, outside-to-inside unlocking, limited tray pressure. |
| 02:03.50–02:04.83 | Pink starts winding. At 02:04.33–02:04.83, pink and blue strands wind concurrently into different docked spools while purple spools wait. | Each docked spool independently searches matching visible/reachable wool. A different head color must not block other in-range matching colors. |
| 02:32–02:33 | Slot-capacity warning after the tray fills. | Difficulty can come from choice and color timing, not merely a larger piece count. |
| 02:34–03:47 | The dragon moves while existing spools wait for matching exposed colors. | Fog and distance can defer collection, but all simultaneously eligible collectors should run. |
| 03:49–03:50 | Extra-slot power activation. | Unlock must visibly change usable capacity and update the action state. |
| 03:59–04:02 | Rainbow power countdown, followed by target-selection mode for Remove. | Keep booster states explicit; exact full rainbow duration is not established by these sampled frames. |
| 04:07–04:13 | Rescue, confetti, reward card and Next +50; optional video reward x3 +150. | Completion reward and rewarded ad are separate explicit choices. |
| 04:15, 04:17, 04:30 | Level 3's irregular rectangular board, mixed-sized pieces, internal gaps and blockers. Forty-five visible pieces by manual count of the 04:17 frame: 8+7+6+6+8+3+7 in seven approximate column groups. Four normal open slots remain. | Add varied rectangle layouts after the diamond. A literal reconstruction needs at least 45 pooled vehicle views; do not silently truncate at 32. |
| 04:17 | Pastel knitted green backdrop, smooth wide tan path, small gear/Level 3 label, high-contrast arrows, cream board, clear gaps, four large booster controls. | Preserve breathing room and hierarchy. Keep gold framing restrained and readable; avoid squeezing type into decorative strips. |
| 02:02–02:05 and 04:17 | A bottom banner ad sits below the booster row. | If banner ads are enabled, reserve their own safe rectangle; do not cover the gameplay controls. Actual network delivery still requires configured ad credentials. |

Local visual evidence:

- `../Reference20261007/motion-122.jpg` — precise concurrent pink/blue collection sequence.
- `../Reference20261007/timeline-00.jpg` — tutorial, transitions, shop and home.
- `local-level3-255.jpg` — Level 3 opening at 04:15.
- `local-level3-257.jpg` — unobstructed Level 3 board at 04:17.
- `local-level3-270.jpg` — restarted Level 3 at 04:30.

The recording shows home/menu states after gameplay but does not show a clean application cold launch. The requested logo, motivation line, loading bar, new-player tutorial routing and returning-player home routing should therefore be implemented as the user's explicit specification, not described as a measured reference startup sequence.

## YouTube reference catalog: metadata verified, visuals pending

Duration is the public video length, not a solve-time benchmark. Level labels are verified video titles. No gameplay timestamp is asserted for an unviewed video.

| Level | Direct video | Duration | Visual/timestamp status |
|---|---|---:|---|
| 1 | [Aoi Toshiro — Level 1](https://www.youtube.com/watch?v=mvX8TZseifQ) | 00:16 | Not viewed; use local 00:00–00:14 for observed tutorial. |
| 2 | [Crazy Games Walkthrough — Level 2](https://www.youtube.com/watch?v=TmeIF-i_JnQ) | 01:36 | Not viewed; use local 02:00–04:13 for observed Level 2. |
| 3 | [Level 3 complete walkthrough](https://www.youtube.com/watch?v=Uw8KESElx1w) | 03:06 | Not viewed; local 04:15–04:30 shows its opening board only. |
| 4 | [Level 4 complete walkthrough](https://www.youtube.com/watch?v=cMqSO4lBzAs) | 04:13 | Not viewed; layout and timestamp observations pending. |
| 5 | [Level 5 complete walkthrough](https://www.youtube.com/watch?v=k8oRxZ46KK0) | 03:25 | Not viewed; first requested Hard milestone reference. |
| 6 | [Level 6 complete walkthrough](https://www.youtube.com/watch?v=3yp03I36H9A) | 05:10 | Not viewed. |
| 7 | [Level 7 complete walkthrough](https://www.youtube.com/watch?v=rMySqGFmDPU) | 05:44 | Not viewed. |
| 8 | [Level 8 complete walkthrough](https://www.youtube.com/watch?v=YN3-0YGxbs0) | 03:13 | Not viewed. |
| 9 | [Level 9 complete walkthrough](https://www.youtube.com/watch?v=aw16ZOA1eRU) | 03:54 | Not viewed. |
| 10 | [Level 10 complete walkthrough](https://www.youtube.com/watch?v=20KIduGEgDg) | 04:42 | Not viewed; second requested Hard milestone reference. |
| 12 | [Level 12 complete walkthrough](https://www.youtube.com/watch?v=QdSOTAF-0i8) | 03:22 | Not viewed. |
| 15 | [Level 15 puzzle solved](https://www.youtube.com/watch?v=VZ8ncNld1aM) | 04:42 | Not viewed. |
| 18 | [Level 18 puzzle solved](https://www.youtube.com/watch?v=QdOmCLaRjno) | 05:50 | Not viewed. |
| 20 | [Level 20 puzzle solved](https://www.youtube.com/watch?v=8zxDrrAxnf8) | 05:08 | Not viewed. |
| 25 | [Level 25 puzzle solved](https://www.youtube.com/watch?v=oFsQsrcV3-s) | 05:54 | Not viewed. |
| 30 | [Level 30 every puzzle solved](https://www.youtube.com/watch?v=ZYe41REWnns) | 05:41 | Not viewed. |
| 35 | [Level 35 every puzzle solved](https://www.youtube.com/watch?v=fQMkMgQpuqI) | 03:05 | Not viewed. |
| 40 | [Level 40 gameplay short](https://www.youtube.com/watch?v=mecpnb7eEkk) | 02:59 | Not viewed. |
| 45 | [Level 45 every puzzle solved](https://www.youtube.com/watch?v=5tuMCmFVfC4) | 04:37 | Not viewed. |
| 50 | [Level 50 complete walkthrough](https://www.youtube.com/watch?v=iH-Ckm0_B_E) | 03:12 | Not viewed. |

Two alternative Level 5 uploads were also found by web search: [Floras Gaming](https://www.youtube.com/watch?v=NLRkDroT3Ho) and [Drawing Tutorial With Nei](https://www.youtube.com/watch?v=1Tk9UsRldDc). These were not watched and are not included in the 20 metadata-verified records.

## Source reliability

1. Highest confidence: the user's own recording, images and exact requested behavior. User preferences may deliberately differ from the reference (three bottom tabs: Shop, Home and Map; every fifth level Hard).
2. The YouTube watch pages above verify identity/title/duration. Until their frames are viewed, they do not verify a layout, difficulty curve, move sequence or feature.
3. [LevelSolve's link index](https://levelsolve.com/wool-crush/) was useful for locating videos. Its [Level 10 page](https://levelsolve.com/wool-crush/level/10/) names different colors in its opening sequence and color summary; its [Level 50 page](https://levelsolve.com/wool-crush/level/50/) disagrees between its pink-arrow recovery descriptions. Treat these as reasons to inspect the videos, not as implementation specifications.
4. [Tap Guides' 11–20 article](https://tap-guides.com/2025/09/27/wool-crush-level-11-20-guide/) describes tubes, fireballs, bombs and a boss battle that are not established by the supplied footage. No such mechanics should be added from that article.

## Reusable campaign design recommendations

These are our design choices derived from the reliable observations, not extracted copies of unviewed levels.

- Rotate clear silhouettes: compact tutorial, diamond, irregular rectangle, split wings, staircase and hollow ring. The first three have direct local visual support; the remaining masks are original extensions using the same exit rules.
- Alternate Easy and Medium between Hard milestones. Suggested repeating rhythm: Easy, Medium, Easy, Medium, Hard. Preserve the special Level 1 tutorial, then treat multiples of five as Hard without exceptions.
- Easy: fewer colors, short dependency chains, several legal outer exits, generous slots and slow approach.
- Medium: mixed lengths, two or three regions that open each other, moderate color-order planning and less tray slack.
- Hard: deeper blocker dependencies, dense mixed directions, more colors competing for four slots and an intentionally constrained opening. Do not create difficulty solely by increasing speed or multiplying piece count.
- Construct each board with a legal departure witness. Preserve total yarn per color equal to total collector capacity and ensure tile footprints never initially overlap.
- Balance the queue as well as the board. A geometrically solvable board can still be unwinnable if the correct next color cannot reach a free slot. Test the actual movement/collection sequence when validation is authorized; do not substitute the witness for a played result.
- Let every eligible docked spool collect concurrently. Continue to exclude fog-covered/out-of-range wool. Both collectors of the same color must claim remaining yarn safely without double decrement or negative capacity.
- Show Easy/Medium/Hard in the map and level intro. Use a distinct Hard color and icon at 5, 10, 15, 20 and later multiples; do not fake unlocked progress or profile achievements.
- Give a relief level after a Hard milestone. Use a new silhouette or color arrangement while reducing dependency depth, so progress feels varied instead of continuously harsher.
- Retain an authored per-level definition with the design rationale and source note. The extra campaign levels should be described as reference-inspired original layouts until directly reconstructed and compared against specific observed footage.
