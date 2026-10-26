# Editing levels without Play Mode

Open the `Wool Crush` scene, then choose **Wool Crush > Level Editor** from Unity's menu. You can also double-click a level asset under `Assets/WoolGame/Authored/Levels`, or select one and click **Open Visual Level Editor** in its Inspector.

## Choose and preview
- Use the level dropdown and previous/next arrows. All configured campaign levels are available regardless of player progress.
- The portrait preview uses the game's meshes and materials in a separate preview scene; it does not start gameplay, consume powers, record attempts, or change the player's current level.
- Use **Board close-up** or **Path close-up** for precise dragging on dense levels.
- Scrub **Snake preview position** to inspect how wool appears along the route. This slider is only for visualization.

## Change vehicles
1. Open **Vehicles**, then click a box in the preview or choose its number in the sidebar.
2. Drag it to move it. Adjust **Drag snap** for finer placement.
3. Set its color, size, arrow, or logical position. Small/medium/large capacities remain 4/6/10.
4. Use Add, Duplicate, or Delete as needed. Size changes apply distinct default footprints; Width/Length allow custom shapes.
5. Keep the balance toggle enabled to synchronize wool quantities with vehicle capacity while retaining as much of the existing wool sequence as possible.

The editor accounts for board rotation and scale when dragging. All levels retain four working slots and three rewarded unlocks.

## Change the snake
- **Snake path:** drag the numbered route points. Point 0 is entry; the kitten sits near the last point. Insert or delete points in the sidebar. The tutorial background is a painted reference, so changing its route may require matching art.
- **Wool sequence:** edit color runs from head to tail and reorder them with arrows. Use **Balance quantities, keep order** after quantity edits. **Rebuild sequence from exit order** replaces the sequence using the selected batch size; it preserves vehicle arrows.

## Save
- Changes stay in a draft until **Save Level**. Revert discards the draft. Unity Undo/Redo is supported.
- The layout checks flag overlaps, blocked arrow cycles, invalid data, pool overflow, and mismatched wool totals. Resolve those before saving.
- Saving updates only that level asset and marks it **Manually Authored**, protecting it from **Author fifty-level campaign**. Legacy ten-level regeneration is blocked while manual levels are present.
- Unsaved drafts are recovered from `Library/WoolLevelEditorDraft.json` after editor reload. Save regularly; deleting the Unity Library folder removes this recovery file.
- Checks establish geometry and quantity consistency. They do not prove timed difficulty or the number of extra slots a player will need.

Avoid directly rearranging the gameplay scene's pooled vehicle objects: gameplay rebuilds those transforms from level assets. Use this editor to change the data the game actually loads.
