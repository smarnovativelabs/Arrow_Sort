# Camera composition and game objects — 21 October 2026

Scene: Assets/Scenes/Wool Crush.unity. Settings live on the WoolSceneRig component (Authored puzzle — persistent pools).

- Main Camera: orthographic, 28 degree X pitch, 180 degree yaw. Upper panel shows the complete road, snake and duck family. Rescue Screen Share is 0.36 of the usable gameplay height.
- Vehicle Area Camera: orthographic, 75 degree X pitch, 180 degree yaw. Lower panel shows all eight slots and active vehicle bounds. The extra rail allowance is included only on levels that actually have a rail. Side padding was reduced to 0.15 world units.
- HUD: full-screen Screen Space Overlay canvas. Its previously disabled UI parent is now enabled. A third camera is unnecessary; the uiCamera compatibility field remains available but disabled if assigned.
- Rescue View (layer 8) and Vehicle View (layer 9): prevent each camera from repeating the other panel's objects. Shared background and flying arrows remain visible to both views. New pooled vehicle visuals and tunnel sources receive their correct layer.
- Vehicle arrows show the allowed exit direction. A blocked vehicle cannot leave; an available vehicle moves to a slot. Pointer rays use the lower camera and selectable layer, preventing the invisible board borders from intercepting touches.
- Eight slots receive the vehicles/crossbows. Plus icons mark locked slots; existing entitlement and unlock rules remain in charge. Slot and depot labels face the lower camera. Depot LEFT labels indicate queued vehicles.
- Snake: follows the existing spline; exposed matching sections can be collected within the existing reach rules. The rendered body, head, tail and effects retain the existing models and behavior.
- Ducks: the mother and baby use the existing two-stage path escape behavior. Their models remain separate and are framed in the upper view.
- Entry concealment: the original path meshes and original atlas material are restored on all eight paths. Each path has an Entrance mist quad centered over its first spline point in camera projection, four units toward the main camera. It has an opaque center and soft edge. It covers the entrance instead of deleting, cutting or changing the road. The camera rig realigns it when loading a level. The exit end and curved road remain intact.
- Header: level, settings, coins and rescue progress. Footer: Unlock, Remove, Sort and Amplify, with existing offer/confirmation behavior.

Validation: inspected live portrait captures; 52/52 active level-30 vehicle center rays hit their own vehicle after excluding board borders; all eight slot centers remained within the lower viewport at 720x1600 and 900x1200; all eight entrance covers aligned with their path starts; Console returned zero errors. This is Editor validation, not a physical-device build or an exhaustive playthrough of every level.

Backups of changed pre-existing scripts, scene and TagManager are in Docs/Backups/CameraComposition-20261021. No original art mesh or source atlas was edited.

## Latest reference adjustment

Vehicle pitch is now 90 degrees, yaw 180. Vehicle Camera Size is 10 as the requested minimum; responsive fitting increases it only enough to keep the complete board and slots visible above the footer. Rescue Screen Share is now 0.24 to give the lower board more room. All 52 level-30 vehicle centers and all eight slots fit at 720x1600 and 900x1200.

The small Entrance mist quads are disabled. The original Fog object and WoolGame/SoftFog shader are used instead, facing the rescue camera. The shader's existing density controls suffice: maximum density 1, color alpha 1, minimum density 0, End Fade 0, Start Fade 0.05. Its lower edge is aligned to the existing fog boundary; its upper area conceals the entrance, while the lower gradient reveals the snake. Original path materials and meshes remain intact. No Console errors were reported during live preview. These settings supersede the earlier 75-degree pitch and local mist description above.
