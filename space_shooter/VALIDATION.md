# Validation

### Passed on the Linux authoring machine

- Tree-sitter C# syntax parsing for both scripts, with no error nodes. This is syntax parsing, **not C# compilation or Unity API verification**.
- Package manifest JSON parsing and expected minimal built-in module declarations.
- All eight Unity asset/folder metadata GUIDs are unique; every asset/folder has matching metadata.
- Scene YAML parsing, internal object references, attached game script GUID, enabled build scene path and GUID.
- Player settings specify legacy input, windowed mode, and the Mono standalone backend.
- The runtime shader resource exists at the path loaded by the game; the editor helper explicitly targets 64-bit Windows.
- Source inspection confirms no audio components are created.
- Six isolated arithmetic examples of the bullet sweep predicate: crossing, sideways miss, still below, already above, overlap, and boundary contact. These check a Python transcription of the formula, **not execution of the C# game**.

### Not tested

Unity Editor and a C# compiler were not available on the authoring machine, which runs Linux rather than Windows. Unity project import, assembly compilation and API resolution, shader compilation, visual appearance, gameplay, Windows executable generation, and Windows execution have **not** been verified. No executable is included.

Likely integration risks to check in Unity are import/default generation of the minimal project settings, built-in module resolution, runtime primitive/shader inclusion, and the Windows build support installation. These are untested integration points, not observed failures. Follow the README's first-run acceptance check before distributing a build.
