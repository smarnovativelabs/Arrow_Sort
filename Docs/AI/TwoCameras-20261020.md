# Portrait cameras and snake instancing

Two nonoverlapping camera viewports share a seamless projection below the slots. Main Camera: snake, ducks, slots. Vehicle Area Camera: lower board. HUD remains full-screen. Input selects the camera under the pointer. Lower camera disables extra shadows/postprocessing.

WoolSnakeInstancing groups body sections by atlas mesh using reusable matrix arrays, retaining gameplay transforms and fallback rendering. Unused spline property blocks removed for standard Unlit material.

Target50FPS, vSync disabled; mobile selects compatible refresh divisor >=50 when possible (60 on60Hz). No device FPS guarantee.

Editor level30 identical45-section pose: baseline169 batches/179 draws/54 SetPass; after149/159/23 with7 snake instance groups. All52 vehicle raycasts passed. Camera alignment and full coverage tested six portrait sizes:1080x1920,1080x2400,768x1024,1200x1600,900x1600,1080x1081. Visual screenshot matched with no seam. No campaign reset. Backups in Docs/Backups/TwoCameras-20261020.
