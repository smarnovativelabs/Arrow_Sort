# Reconnection and Level 2 correction — 2026-10-07

The existing Unity MCP HTTP bridge was reconnected to Wool Crush@1e5fabeb70d3e4f0, Unity 6000.3.20f1. A live read confirmed C:/Users/Muneeb Nazir/Wool Crush/Assets and Assets/Scenes/Wool Crush.unity. Codex's registered Unity tool surface was unavailable; the existing local bridge responded to MCP requests through an approved PowerShell client.

## Confirmed defect and fix
A live Physics.BoxCastAll query on the first purple Level 2 vehicle returned both itself and Vehicle 3 at distance zero. Their authored footprints overlapped. Corrected the vertical and horizontal piece dimensions in Level02.asset and its authoring source. The swept-footprint rule now also rejects a starting overlap behind the moving tile. Added all-pairs initial-layout overlap checks.

The acceptance driver compares pooled object identities as sets because tutorial hint reparenting changes traversal order. Its completion cleanup now restores saved progress even if the game object was destroyed by leaving Play Mode.

## Evidence
- 487 rule assertions passed.
- Level 1 completed through six public selection actions in 6.13 seconds.
- Next Level UI moved into Level 2; all 28 vehicles completed in 116.73 seconds.
- Pooled and authored scene object identities preserved.
- 7,262 active render samples; maximum 53 batches, 241,029 triangles and 17 SetPass calls.
- Editor frame time median 16.45 ms, p95 23.58 ms. These are Editor measurements, not target-device performance proof.
- Background gameplay updates were temporarily enabled for this test because the Editor was unfocused, then restored and Play Mode stopped.

## Still incomplete
Visual proportions, smooth road presentation, profile layout, independent green/teal colors, all booster edge cases, five portrait aspect-ratio captures, device profiling and difficulty analysis remain unfinished. Passing these two playthroughs does not establish a complete clone. Additional source links and reliability notes are in Additional-sources.md.
