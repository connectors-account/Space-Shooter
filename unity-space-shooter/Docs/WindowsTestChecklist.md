### Unity and Windows acceptance checks — not yet executed

These are the remaining manual runtime checks, to run in Unity **6000.3.11f1 on your Windows computer**. None is claimed as passed by the Linux source check. Keep the Unity Console visible and require zero new errors.

### Editor import/setup

- [ ] Extract a fresh ZIP and import the project through Unity Hub.
- [ ] Wait for compilation; no compiler errors. Check Input Manager (Old).
- [ ] Automatic setup logs completion and opens `Starward.unity`, or the single **Starward > Create or Rebuild Game** command does so.
- [ ] Verify all seven `.prefab` files and the scene exist under `Assets/Generated`.
- [ ] **Starward > Validate Generated Project** logs success.
- [ ] Rebuild twice. There is still one scene, seven prefabs, and one manager in the generated scene; no duplicate hierarchy roots.
- [ ] Close and reopen the project. The existing generated scene is not overwritten automatically.

### Menu, movement and shooting

- [ ] Play mode shows STARWARD title, instructions, best score, Start and Quit buttons.
- [ ] Start with Enter and separately with the button. HUD begins with score 0, hull 5, weapon 1.
- [ ] WASD and arrow keys each work; opposing keys cancel, diagonals are not faster, release stops movement. Player remains inside the arena.
- [ ] Hold Space: shots travel upward; releasing stops new shots. Shots retire at arena bounds or after their lifetime.
- [ ] Confirm all three star layers scroll at different speeds behind the ships.
- [ ] Resize the game/window to tall, wide, and 16:9 dimensions. Arena/UI retain alignment and legibility.

### Combat and progression

- [ ] Straight and three-shot fan enemies both appear. Pink enemies use fans; red enemies fire straight downward.
- [ ] Player bullets hit enemies but not the player. Enemy bullets hit the player but not enemies. Bullet is consumed on a valid hit, including invulnerable-player contact.
- [ ] Score increases once per kill. No double score from simultaneous hits or overlapping trigger/cast checks.
- [ ] Wave 1 has 7 enemies. Complete it; the next wave follows the intermission and HUD counts match spawned/live enemies.
- [ ] Later waves get harder; health increases from wave 4, counts/fire rates/speeds cap according to GameRules.
- [ ] Let an enemy escape: hull decreases, subject to current invulnerability. Ramming an enemy removes it and damages hull once.
- [ ] Player flashes during spawn protection and after damage. Repeated hits inside 1.25 seconds do not reduce hull again.
- [ ] After kill 4 a gold pickup drops. Collect: weapon increases to 2 and fires two shots. Further gold upgrades cap at 3.
- [ ] After kill 8 a green pickup drops. Collect when damaged: restores 2 health, capped at 5. At full hull it is still consumed once.
- [ ] Damage reduces weapon by one, minimum 1. Uncollected pickups leave the screen and are destroyed.
- [ ] Inspect Runtime Entities: offscreen shots, removed enemies, old pickups and expired spark objects do not accumulate indefinitely.

### Pause, death and restart

- [ ] Esc/P pauses movement, bullets, spawns, protection timers and background. Esc/P and Resume each resume normally.
- [ ] Alt-Tab away: run pauses. Returning does not automatically resume it.
- [ ] Lose all hull: SIGNAL LOST shows final score, wave and best; simulation freezes and no further scoring occurs.
- [ ] Restart using Enter, R, and the button. Repeat several times: health/score/weapon reset, old projectiles/enemies disappear, fire/spawn speed does not multiply.
- [ ] Pause > Main Menu removes the old run. Start again from a clean state.
- [ ] Quit/reopen: best and mute setting survive. Quit in Editor exits Play; Quit in Windows closes the application.

### Sound and standalone

- [ ] Hear player/enemy shots, destruction, damage, pickup, UI, wave and game-over effects. No audio import or playback errors.
- [ ] Mute via M and the HUD button; both agree, all voices stop being audible, unmute restores later sounds.
- [ ] Switch the active Build Profile to **Windows / Intel 64-bit**, Mono backend; build with the Starward menu.
- [ ] Build succeeds, then launch `Builds/Windows/Starward.exe`. Repeat start, movement, combat, pause, restart and quit outside the editor.
- [ ] Copy the whole Windows build folder to a different location on Windows and confirm it runs with its data/runtime files.

### Scope of automated evidence

`Docs/Validation.json` covers only C# syntax/type relationships against hand-written stubs, 51 rule/health assertions, local media validity, package/settings consistency, metadata uniqueness, and source asset paths. Stubs cannot prove Unity API compatibility, real lifecycle ordering, rendering, input, collision callbacks or executable behavior. The editor's own validation additionally checks generated serialized references when run, but it is not a gameplay test.
