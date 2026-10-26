# Clean Home avatar

Removed the baked legacy avatar sprite from Header controls/Profile Image; kept the transparent Image as the profile Button hit target. Saved duck avatar mask is centered at120x120. Portrait fills the circular mask with no extra artwork beneath it. Existing homeAvatar reference and runtime saved-avatar selection remain intact. AuthorProfileView now removes legacy backdrop as well.

Validated native Home capture: only the currently selected avatar appears, no lower baked icon. Scene saved. Backups: Docs/Backups/HomeAvatar-20261022.
