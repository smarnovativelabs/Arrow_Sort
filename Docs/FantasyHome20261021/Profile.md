# Profile UI - final update 2026-10-21

Home Profile opens the illustrated profile. Pencil or name opens the separate Make It Yours editor, with a large selected portrait, immutable numeric player ID, rounded editable name field, six rounded avatar cards, a selection border and Save Profile button. Name/avatar edits persist and update the home portrait. Names have a 20-character limit. Each new campaign receives a random numeric anonymous ID as its default name; changing the display name or restarting campaign progress preserves that ID. These are local anonymous IDs, not authenticated server accounts.

Daily Play Count shows completed level attempts today, including completed replays, counted once per attempt. It resets at 00:00 UTC using the HTTPS-verified clock. Attempts without verified time remain pending until time is verified. The saved deadline/count runs across app restarts; no device wall-clock reset is used.

Coins Collected is cumulative lifetime income, not the current wallet. Completion awards, completion bonuses, daily rewards, wheel prizes and positive wallet credits increase it. Spending does not reduce it. Existing saves receive a one-time lower-bound migration using their balance, saved completion/bonus history and retained daily/spin rewards. Older income not represented in save history cannot be recovered exactly.

Compilation passed with zero errors/warnings. Isolated nonpersistent checks passed for numeric default ID, stable identity after renaming, completed-level/replay counting, duplicate callbacks, UTC midnight reset, lifetime earnings after spending, no duplicate coin grants, name validation and avatar limits. Actual home Profile and pencil routing and six avatar options were checked in Play Mode. The profile/editor were visually inspected at the 1440x2960 phone ratio. No physical-device testing was performed. Final state: saved in Edit Mode.

Profile artwork and six separate avatar sprites: Assets/WoolGame/Designer/UI/Profile/
Authoring/checks: WoolProfileBuilder.Apply(), WoolProfileBuilder.Validate(), WoolProgress.ValidateProfileLifetimeRules()
Visual captures: Assets/Docs/FantasyHome20261021/Profile-Main.png and Profile-Editor-Polished.png (the final save also fixes all six avatar captions).
