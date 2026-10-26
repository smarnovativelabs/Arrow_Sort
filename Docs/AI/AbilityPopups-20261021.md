# Layered ability popups

Supersedes flattened artwork. One shared 760x720 authored card for all four abilities, raised gold title, rounded peach illustration inset, transparent standalone icons, description, green coin and blue rewarded-video buttons. Titles and descriptions are editable in WoolUnlockPresentation; prices reflect WoolBoosters.Prices. Current balance reads WoolProgress.Coins on open. Black backdrop alpha250/255. Pulse uses unscaled time. Rotating WoolAbilityGlow lies behind the transparent ability Image; CanvasRenderer required. No shine stripes cross the illustration or buttons. Button masks removed to avoid TMP clipping artifacts.

DuckFootprintCoin.png is the shared coin: HUD skin[5], gameplay coin images, Home wallet and spin rewards use it. Popup wallet and payment icon use same sprite. Layered icon and coin assets generated using built-in imagegen; original files retained. Runtime assets stored under Assets/WoolGame/Designer/UI/Unlock. Native popup video icon is WoolRewardVideoGraphic.

Validated all four variants in Play Mode: common card geometry, prices800/600/700/500, current balance345, glow behind icon; final Sort screenshot inspected. Earlier coin insufficient-funds route and dismiss/resume checked in isolated preview. Real rewarded ads and Android build not tested. Backups: Docs/Backups/AbilityLayers-20261021.

## White shine and reference layout follow-up

White shineColor (1,1,1,1) with opacity .46; glow is clipped within the illustration inset and rendered behind each icon. Card uses shared compact reference layout with rounded inset and thick gold frame. Wallet now stays at screen top-right inside safe-area insets. Balance refreshes when its value changes. Existing editable titles/descriptions and controller price/callbacks retained.

Validation: all four variants have 760x720 card, current balance345, white rays behind icon. Native Sort screenshot inspected. Rotation changed between frames; alpha250 retained. Shared coin Sprite references audited: HUD skin[5], gameplay/menu coin icons, popup and wheel icons point to DuckFootprintCoin. No build performed.
