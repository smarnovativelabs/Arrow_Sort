# Warehouse introduction

The saved Warehouse information panel first opens when LoadLevel encounters non-empty containerPositions and Campaign.warehouseTutorialSeen is false. Old saves default to unseen. Continue and X persist the seen flag and close the normal gameplay modal. Hide cleans up the panel without marking it seen.

The five-second unscaled animation repeats indefinitely: hand approaches and taps, front vehicle exits and fades, next vehicle emerges behind the container, 6 LEFT changes to 5 LEFT, then the sequence resets. Demonstration sprites and graphics do not alter gameplay vehicles or queue counts. The panel uses safe parent bounds and uniform scaling, and creates no runtime camera. Labels are editable TMP scene objects.

Validated three full loops and unchanged controller elapsed time while open, Continue and X, resumed movement, first-container triggering, non-container levels, no replay after dismissal, JSON flag round trip and no Console errors. Preview save isolation preserved the real campaign.

Revision: reference-style cream panel and peach inset, narrow wheeled units, compact navy queue block, custom turquoise-cuff hand and duck-topped heading. Loop remains five seconds; panel dismisses automatically after 10 unscaled seconds or sooner via Continue/X. Verified timeout at 10.03 seconds and repeat cycle, plus early close callback.
