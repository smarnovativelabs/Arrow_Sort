# Gameplay ability strip

Four reference atlas tiles replace the old captions/icons. WoolAbilityTileGraphic draws rounded native geometry because Sprite.OverrideGeometry did not survive asset serialization. Existing button names/callbacks are preserved. Green top-right badges show owned count or + at zero. WoolAbilityStrip refreshes on enable and HUD status changes.

Inventory API: AbilityCount, AwardAbilities, UseAbility in WoolProgress.Abilities. Unlock retains legacy spin.unlockPowers. Remove, Sort and Boost use additive optional campaign integers defaulting to zero in existing version1 saves. No real inventory was granted during validation. Owned charges open Use/Cancel in the existing layered modal. Remove is consumed only after a valid target; cancelling retains it. Other powers consume on successful activation. Coin/rewarded-video flow remains for zero stock. Store purchase integration is still unavailable.

Validated isolated preview: positive badges, zero +, old JSON defaults, JSON stock round-trip, all four charges consumed correctly, Remove cancellation retains charge, coins unchanged by owned use, rounded graphics remain button targets and click opens offer. Backups: Docs/Backups/AbilityStrip-20261021. No Android build.
