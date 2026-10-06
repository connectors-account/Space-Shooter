### Confirmed offline checks

`Validation.json` is produced by `python Tools/validate_source.py`. It records C# grammar parsing, matching class filenames, unique source GUIDs and metadata presence, PNG decoding/alpha, WAV PCM format and non-silent/unclipped signal, manifest/settings parsing, builder asset existence, Windows-target source configuration, and source-level lifecycle/collision setup checks.

These checks do **not** resolve Unity APIs or compile C#. They do not execute Unity import callbacks, a physics simulation, UI, or Windows code. Python tools were also checked with `py_compile`; Git whitespace checks were run. No stubbed Unity classes are used as supposed proof of compatibility.

### Unity / Windows acceptance checklist — not executed here

Use Unity 2022.3.62f3. This checklist documents the remaining real-runtime verification, not completed tests.

1. **Clean import:** open the extracted source in Hub. Confirm no Console errors, the automatic setup success message, the generated scene, all nine prefabs, GameAssets catalogue, and enabled build-scene entry. Run **Validate Generated Project**. Reopen the Editor and verify it does not recreate/overwrite the existing scene.
2. **Title and input:** press Play. Confirm title text and buttons are visible. Launch with Enter or the mouse. Test every WASD/arrow direction and diagonals. Hold opposing keys; motion should cancel. Reach all four edges without leaving the screen. Release input and verify movement stops immediately.
3. **Firing and collisions:** hold Space. Player bullets hit only enemy ships; enemy bullets hit only the player. Confirm enemy deaths award the stated score once, even during triple fire. Ship contact damages the player once per entry and respects invulnerability. Same-side bullets cannot hit each other.
4. **Health:** take a hit, verify hull decrement and flashing. Multiple overlapping bullets within 1.2 seconds must not stack damage. Escaped enemies attempt a hit. Reach zero health, verify the game-over overlay and frozen world.
5. **Patterns and waves:** observe aimed fire in wave 1, fans from wave 2, radial bursts from wave 3. Confirm new waves wait for old enemies to clear, and that an escape cannot permanently block a wave. Spawn rate and projectile counts remain bounded.
6. **Powerups:** every third kill drops a pickup, cycling repair, rapid, triple. Collect each. Repair cannot exceed five health. Fire rate changes for ten seconds; triple fire for twelve. Both can coexist, and recollecting refreshes the matching timer. Uncollected items leave the scene.
7. **Pause and focus:** pause with Escape/P or lose window focus. Enemies, bullets, pickup expiry, powerup timers, and wave delays must freeze. Resume explicitly. Test mute from both a running game and the pause menu. Clicking another window must not cause a later movement jump.
8. **Restart and menu:** from pause and game over, restart repeatedly. Health resets to five, score to zero, wave to one, old bullets/enemies disappear, and only one Active Run root and one wave coroutine remain. Return to title, then launch again. Best score and sound setting survive stopping/relaunching the player.
9. **Presentation/audio:** confirm all four ship sprites and three pickup symbols have clean alpha edges, two parallax layers scroll without gaps, effects fade, and all eight SFX can be heard without excessive overlap. No assertion about listening quality was made by the offline WAV checks.
10. **Desktop build:** run the Windows build menu. Confirm success in the Console, then run the complete output folder on Windows. Check window dimensions, controls, focus pause, menus, audio and Quit. Read `Player.log` if errors appear. Test on the intended display/DPI configuration; editor playtesting alone is not sufficient.

### Known scope / limitations

- Single-player, endless arcade play only; no networking, saves beyond preferences/high score, controller support, bosses, or web build.
- Simple sprite bursts rather than a particle package; enemies and bullets instantiate/destroy objects rather than using a pool. Spawn, lifetime and wave-size caps keep the intended small-game workload bounded; performance has not been profiled in Unity.
- Fixed landscape gameplay framing. The default 1280×720 window is the intended presentation; other aspect ratios receive a letterboxed camera.
- Sprite appearance has been inspected as image assets, not in a running Unity frame. There is no fabricated gameplay screenshot or fake runtime test report.
- Unity may normalize or augment the intentionally small serialized ProjectSettings on first import. That is expected; preserve those changes if you continue the project.
