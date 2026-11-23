"""Static project checks only. This does NOT compile C# or run Unity."""
import json
from pathlib import Path
import re

import tree_sitter_c_sharp
from tree_sitter import Language, Parser
import yaml

ROOT = Path(__file__).resolve().parents[1]
checks = []


def check(name, condition):
    if not condition:
        raise AssertionError(name)
    checks.append(name)
    print("PASS (static):", name)


def unity_yaml(path):
    text = path.read_text()
    text = re.sub(r"^%.*\n", "", text, flags=re.M)
    text = re.sub(r"^--- !u!\d+ &\d+.*$", "---", text, flags=re.M)
    return list(yaml.safe_load_all(text))


parser = Parser(Language(tree_sitter_c_sharp.language()))
sources = list((ROOT / "Assets").rglob("*.cs"))
check("Complete game source exists", len(sources) == 1)
for source in sources:
    tree = parser.parse(source.read_bytes())
    check(f"C# syntax tree has no error nodes: {source.name}", not tree.root_node.has_error)

manifest = json.loads((ROOT / "Packages/manifest.json").read_text())
check("Only built-in IMGUI, text and legacy input dependencies", manifest["dependencies"] == {
    "com.unity.modules.imgui": "1.0.0", "com.unity.modules.inputlegacy": "1.0.0",
    "com.unity.modules.textrendering": "1.0.0"
})
settings = unity_yaml(ROOT / "ProjectSettings/ProjectSettings.asset")[0]["PlayerSettings"]
check("Input handling matches UnityEngine.Input", settings["activeInputHandler"] == 0)
check("Windows standalone uses Mono", settings["scriptingBackend"]["Standalone"] == 0)
check("Windowed 960 x 720 defaults", (settings["defaultScreenWidth"],
      settings["defaultScreenHeight"], settings["fullscreenMode"]) == (960, 720, 3))
check("Pinned editor version", (ROOT / "ProjectSettings/ProjectVersion.txt").read_text().strip()
      == "m_EditorVersion: 6000.3.0f1")

guids = {}
for meta in (ROOT / "Assets").rglob("*.meta"):
    data = yaml.safe_load(meta.read_text())
    guid = data["guid"]
    check(f"Valid unique GUID: {meta.name}", re.fullmatch(r"[0-9a-f]{32}", guid) is not None
          and guid not in guids)
    guids[guid] = meta
for asset in (ROOT / "Assets").rglob("*"):
    if asset.suffix != ".meta":
        check(f"Metadata exists: {asset.relative_to(ROOT)}", Path(str(asset) + ".meta").is_file())

scene_path = ROOT / "Assets/Scenes/Main.unity"
scene = unity_yaml(scene_path)
behaviours = [doc["MonoBehaviour"] for doc in scene if "MonoBehaviour" in doc]
check("Scene has exactly one enabled game controller", len(behaviours) == 1
      and behaviours[0]["m_Enabled"] == 1)
script_guid = behaviours[0]["m_Script"]["guid"]
check("Scene points to the game script", guids[script_guid]
      == ROOT / "Assets/Scripts/SpaceShooter.cs.meta")
scene_text = scene_path.read_text()
anchors = set(re.findall(r"^--- !u!\d+ &(\d+)", scene_text, flags=re.M))
refs = set(re.findall(r"\{fileID: (\d+)\}", scene_text)) - {"0"}
check("Scene-local object references resolve", refs <= anchors)
build = unity_yaml(ROOT / "ProjectSettings/EditorBuildSettings.asset")[0]["EditorBuildSettings"]
scenes = build["m_Scenes"]
check("Only Main scene is enabled for builds", len(scenes) == 1 and scenes[0]["enabled"] == 1
      and scenes[0]["path"] == "Assets/Scenes/Main.unity"
      and guids[scenes[0]["guid"]] == ROOT / "Assets/Scenes/Main.unity.meta")
axes = unity_yaml(ROOT / "ProjectSettings/InputManager.asset")[0]["InputManager"]["m_Axes"]
check("No unused named input axes", axes == [])
print(f"\n{len(checks)} static checks passed. Unity compile/import/play and Windows build NOT verified.")
