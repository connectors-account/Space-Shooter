### STARWARD — standalone Unity 6.3 desktop project

The complete STARWARD project is in [`unity-space-shooter/`](unity-space-shooter/). The existing repository-root Unity 2022.3 project is preserved unchanged; these are separate Unity projects, not a combined game.

**Open `unity-space-shooter/` in Unity Hub, not the repository root, to use STARWARD.**

- Editor: **Unity 6000.3.11f1 LTS**.
- Target: Windows x86_64 desktop, Mono backend; not WebGL.
- [Setup, gameplay controls, architecture and Windows build instructions](unity-space-shooter/README.md).
- [Original art/audio provenance](unity-space-shooter/ASSETS.md).
- [Recorded offline validation results](unity-space-shooter/Docs/Validation.json).
- [Remaining Windows/Unity acceptance checklist](unity-space-shooter/Docs/WindowsTestChecklist.md).

The first-open editor utility generates the scene and seven prefabs. If automatic setup is deferred, use **Starward > Create or Rebuild Game**. Use **Starward > Build Windows x86_64** after switching the active Build Profile to Windows / Intel 64-bit.

The existing project's source and media were copied without gameplay changes. Previously recorded checks passed for C# source against handwritten Unity API stubs, 51 rule/health assertions, and media/settings/metadata integrity. These tests were **not rerun merely for this upload**. Actual Unity compilation/import, generated-scene execution, gameplay, and the Windows executable remain **unverified**; no executable is included.

Only project source, local assets, metadata, tools and relevant Markdown/JSON/PNG documentation are added. Platform identity files, secrets, Unity caches, build outputs, tool caches, and redundant PDF/Word report copies are not part of this addition. Files already tracked in the base branch are left untouched.
