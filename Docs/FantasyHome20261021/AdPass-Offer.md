# Ad Pass offer

Saved in `Assets/Scenes/Wool Crush.unity` using Unity CLI. Clicking Home > Ad Pass opens the new Remove Ads popup over Home. Close returns to Home. The full-screen dimmer is black at 0.32 alpha and blocks input to the home buttons beneath it.

## Editing

Select `Authored HUD > Ad pass offer`, component **Wool Ad Pass Offer**:

- **Bonus Coins**: default 5000; updates the native `Editable bonus coins` TextMeshPro label.
- **Display Price**: default `Rs2,250.00`; updates the native `Editable price` label. Currency and formatting are editable.
- **Backdrop Alpha**: default 0.32.

The close and purchase buttons are separate objects. Purchase uses the same project Coffee UIEffects UIShiny settings as Level: duration 0.65 seconds plus delay 1.35 seconds. It has no text scale animation, and uses the Home Sharp font and Level outline material. Existing store-unavailable handling is retained; this change does not implement billing or grant entitlements/coins.

The card fits uniformly inside 90% of the available safe-area width and height; its background dimmer covers the full canvas. No device-preview safe-area override is used.

## Brief verification

- Unity compilation passed with zero errors and zero warnings.
- Live button checks: Ad Pass opened the popup and Close hid it.
- Live values: `+5000`, `Rs2,250.00`, dimmer alpha 0.32, shine cycle 2 seconds.
- Native RectTransform bounds passed for available safe-area sizes 360x600, 720x1280, 768x980, 960x2518 and 2048x2600. These were Edit Mode layout checks, not physical-device tests.
- The actual 2048x2732 render target was visually captured at the same aspect ratio, 1024x1366: `Assets/Docs/FantasyHome20261021/AdPass-Offer.png`.
- Final state: saved, popup hidden, Play Mode stopped.

## Artwork

Assets: `Assets/WoolGame/Designer/UI/AdPass/OfferPanel.png`, with `OfferReference.png` preserving the supplied original. The green purchase sprite and red close sprite reuse existing project UI art.

Built-in ImageGen edit prompt: preserve the supplied Remove Ads panel, title, duck, stripes, coins, Forever ribbon and all benefit wording; remove only the baked `+5000` value, entire bottom green price button/price, and red X close button; reconstruct the cream reward area and red/gold panel behind them; preserve transparent outer alpha. Native UI supplies the editable values and separate buttons. No external image-generation API was used.

Authoring command: **Wool Crush > Author Ad Pass offer**.
