### Validation results

**Passed on Linux:**

- `GameRules.cs` and `Tests/GameRulesTests.cs` compiled together with Mono `mcs -warnaserror` and ran successfully: **17 checks passed**. Cases cover clamping, stationary overlap/separation, boundary contact, fast swept collisions, parallel/diagonal misses, moving-away cases, nearest-hit ordering, opposing motion and player bounds.
- All **4 C# files** parsed without syntax errors using the C# tree-sitter grammar. This is syntax checking, not Unity API/type checking.
- The package manifest parses and includes only Unity built-in IMGUI, legacy input and physics modules.
- The starter scene YAML parses; its GameObject/Transform references are consistent, and the build-settings scene path and GUID match the supplied scene metadata.
- Asset metadata is present for the game scripts, editor script, shader and scene, with valid GUID formats.
- The shader's Resources filename matches the runtime load request. This is a reference check, not shader compilation.

**Not verified here:**

Unity Editor is not installed. No Unity assembly compilation, editor import, shader compilation, visual capture, Play-mode interaction, end-to-end game-loop test or Windows executable build was performed. There is no claim of verified runtime playability and no executable included. Follow `README.md` on your Windows Unity machine to perform these steps.

The source includes movement, held-Space shooting, one descending enemy type, swept-box collisions, score, game-over on collision/escape, and a restart button. Its runtime objects are built from Unity cube primitives. Visuals intentionally use simple flat colours with no external art or sound, as requested.
