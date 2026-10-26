# Shop configuration

The static Shop page keeps the existing shared navigation. Edit Authored HUD > Portrait composition > Main menu > Shop, WoolShopView component, Offers array. Bundle Name, Coins, Price and Abilities are editable source values. Ability order: Unlock, Remove, Sort, Boost. Starter: [1,1,0,0]; Superior: [1,1,1,0]; Large: [1,1,1,1]. Text labels are separate TMP scene elements.

Daily Gift grants exactly 50 coins once per local calendar day, matching daily tasks. Campaign.lastShopGiftDay persists in the existing campaign JSON. Paid buttons retain the existing Store unavailable flow; no real purchase fulfilment is configured.

Validated: Unity compilation and zero Console errors; phone 1440x2960 and tablet 768x1024 captures; drag scroll; gift click +50, repeat protection, JSON reload, legacy save default and clock rollback; simulated notch/home-indicator safe area and navigation clearance. Tests used BeginPreview/EndPreview and preserved saved progress.
