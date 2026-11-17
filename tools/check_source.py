"""Static source checks only: not a Unity compiler, shader compiler, or playtest."""
import json
import re
from pathlib import Path

from tree_sitter import Language, Parser
import tree_sitter_c_sharp

ROOT = Path(__file__).resolve().parents[1]
parser = Parser(Language(tree_sitter_c_sharp.language()))
checks = []


def check(name, condition):
    assert condition, name
    checks.append(name)
    print("PASS:", name)


for path in sorted((ROOT / "Assets").rglob("*.cs")):
    tree = parser.parse(path.read_bytes())
    check("C# syntax: " + str(path.relative_to(ROOT)), not tree.root_node.has_error)

manifest = json.loads((ROOT / "Packages/manifest.json").read_text())
check("Only built-in IMGUI and legacy input modules", manifest["dependencies"] == {
    "com.unity.modules.imgui": "1.0.0", "com.unity.modules.inputlegacy": "1.0.0"})
guids = {}
for path in (ROOT / "Assets").rglob("*.meta"):
    guid = re.search(r"^guid: ([0-9a-f]{32})$", path.read_text(), re.M).group(1)
    check("Unique asset GUID: " + path.name, guid not in guids)
    guids[guid] = path
for path in (ROOT / "Assets").rglob("*"):
    if path.suffix != ".meta":
        check("Asset metadata: " + path.name, Path(str(path) + ".meta").is_file())
scene = (ROOT / "Assets/Scenes/Main.unity").read_text()
script_guid = re.search(r"m_Script: .*guid: ([0-9a-f]+)", scene).group(1)
check("Scene references the actual controller", guids[script_guid].name == "SpaceShooterGame.cs.meta")
scene_guid = re.search(r"^guid: (\w+)$", (ROOT / "Assets/Scenes/Main.unity.meta").read_text(), re.M).group(1)
build_settings = (ROOT / "ProjectSettings/EditorBuildSettings.asset").read_text()
check("Build scene enabled and GUID matches", "enabled: 1" in build_settings
      and "Assets/Scenes/Main.unity" in build_settings and scene_guid in build_settings)
check("Legacy input selected", "activeInputHandler: 0" in
      (ROOT / "ProjectSettings/ProjectSettings.asset").read_text())
source = (ROOT / "Assets/Scripts/SpaceShooterGame.cs").read_text()
check("Both input backends are conditionally compiled", "#if ENABLE_INPUT_SYSTEM" in source
      and "#elif ENABLE_LEGACY_INPUT_MANAGER" in source)
check("Shader exists in Resources", (ROOT / "Assets/Resources/FlatColor.shader").is_file())
all_source = "\n".join(p.read_text() for p in (ROOT / "Assets").rglob("*.cs"))
check("No audio or external asset loaders", not any(x in all_source for x in (
    "AudioSource", "AudioClip", "UnityWebRequest", "http://", "https://")))
check("No placeholder implementation", not re.search(r"\bTODO\b|NotImplementedException", all_source))
build = (ROOT / "Assets/Editor/WindowsBuild.cs").read_text()
check("Explicit Windows x64 target and build-result check", "BuildTarget.StandaloneWindows64" in build
      and "BuildResult.Succeeded" in build)
print("\nAll", len(checks), "static checks passed. Unity compilation/runtime NOT verified.")
