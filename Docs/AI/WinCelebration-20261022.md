# Level completion presentation

Compact gold and cream portrait panel with dancing duck, no stars, editable title/subtitle and reward text. Green Continue is primary; Home and rewarded-video 2x are secondary. Duck footprint currency matches the HUD.

Base reward automatically animates into the top wallet when first shown. The panel remains open. Continue opens the map; Home returns home. Successful rewarded video adds 50 bonus coins to the 50 base reward, animates the extra coins, and disables repeat bonus claims. Reopening an offer does not replay the base collection. Coin and confetti frame steps are capped to tolerate editor hitches.

Validation: native game-view screenshot inspected; automatic wallet reached 50 with popup still open; coin flight captured with 6 visible coins; bonus reached 100 total and duplicate claim returned false; Continue callback invoked after animation; Unity Console had zero errors. Reward backend simulated in isolated preview; live ad delivery was not tested.
