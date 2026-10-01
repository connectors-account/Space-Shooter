### Unity / Windows acceptance checklist — NOT YET EXECUTED

Run these checks on your Windows development PC. All boxes are intentionally unchecked: no Unity or Windows runtime testing was performed on the Linux authoring machine.

#### Import and generation

- [ ] Open the extracted project with Unity 2022.3.62f3; official package resolution finishes.
- [ ] C# compilation has no errors. Capture any first error with file, line, and Editor version.
- [ ] First-import generation creates `Assets/Generated/Scenes/NovaGuard.unity` and nine prefabs.
- [ ] The scene is enabled in Build Settings. Cameras, EventSystem and Game Systems exist.
- [ ] Tools → Nova Guard → Validate Generated Assets completes without exception.
- [ ] Close and reopen the project: generated scene/prefab edits are not automatically overwritten.

#### Menu, movement and firing

- [ ] Play Mode displays the title, best score, controls, Start Flight and Quit buttons.
- [ ] Mouse clicks, arrow navigation, Tab/Shift+Tab, and Enter activate the intended menu controls.
- [ ] Start creates one player; WASD/arrows move in the indicated direction; opposing keys cancel.
- [ ] Diagonal speed is not faster; releasing movement stops immediately at the next physics step.
- [ ] Hold all four directions against the edge: ship remains inside the playfield, below the HUD.
- [ ] Hold Space and Z separately: cyan bolts originate at the nose and destroy enemies.
- [ ] Enemy bullets do not damage enemies; player bullets do not damage the player.

#### Combat and progression

- [ ] Wave 1 scouts fire downward; wave 2 gunners aim at the player; wave 3 tanks emit five-way spreads.
- [ ] Enemy destruction increments score once; bullets do not award duplicate kills.
- [ ] Being hit reduces hull, blinks the player, and plays the hit sound.
- [ ] Several bullets in the 1.25-second hit immunity window remove only one hull point.
- [ ] Enemy contact deals damage without permanent sticking or physical knockback.
- [ ] A ship escaping below the screen costs hull unless shield/invulnerability protects the player.
- [ ] A wave clears only after all enemies leave/die; the bonus, guaranteed pickup and next wave occur.
- [ ] Later waves increase difficulty; prolonged idling eventually loses the run.

#### Power-ups and presentation

- [ ] Green repair restores 2 hull, capped at 5.
- [ ] Gold pickup enables rapid three-bolt fire for 10 simulation seconds.
- [ ] Blue pickup gives 8 seconds of protection, blue tint and a countdown in the HUD.
- [ ] Recollecting a timed pickup refreshes its timer; each pickup awards 25 points only once.
- [ ] All ship/pickup/bolt sprites have correct orientation, transparency and readable scale.
- [ ] Three star layers scroll at different speeds and wrap without visible jumps.
- [ ] Hit/pickup/explosion sparks display and expire; eight sound cues are audible in their contexts.
- [ ] M and the sound button toggle audio; the choice persists after relaunch.
- [ ] Resize the game window to wide/tall sizes: 16:9 playfield stays visible, UI is readable, black bars stay clean.

#### Pause, restart and desktop build

- [ ] Escape/P and Pause freeze movement, bullets, stars, spawns and buff timers together.
- [ ] Alt-Tab or loss of focus pauses; returning to the app requires explicit resume.
- [ ] Resume does not skip wave timers, expire buffs early, or spawn an extra player.
- [ ] Zero hull displays Game Over with final score, wave and best score.
- [ ] R/Fly Again restarts with full hull, zero score, wave progression reset and no old bullets/enemies.
- [ ] Restart twice and return to Main Menu: no duplicate input, UI, cameras, wave coroutines or leaked run objects.
- [ ] Quit button exits Play Mode in Editor and terminates the built desktop application.
- [ ] Tools → Nova Guard → Build Windows x64 completes successfully.
- [ ] Launch `NovaGuard.exe` outside the Editor and repeat the core controls/combat/pause/restart checks.
- [ ] Close/relaunch the EXE: best score and mute preference persist.
- [ ] Zip the entire Windows output folder, extract elsewhere, and launch successfully with no Unity Editor installed.

Record the Editor version, Windows version, relevant Console/player log, and the exact first failing step when reporting an issue. A passed source check is not a substitute for these runtime checks.
