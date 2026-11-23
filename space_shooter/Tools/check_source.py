"""Source-only checks. This does not compile C# or import/run Unity."""
import json
import re
from pathlib import Path

import tree_sitter_c_sharp
import yaml
from tree_sitter import Language, Parser

ROOT = Path(__file__).resolve().parents[1]
checks = 0


def check(condition, message):
    global checks
    if not condition:
        raise AssertionError(message)
    checks += 1
    print("PASS:", message)


def unity_yaml(path):
    text = path.read_text()
    text = re.sub(r"^%.*\n", "", text, flags=re.MULTILINE)
    text = re.sub(r"--- !u!\d+", "---", text)
    return list(yaml.safe_load_all(text))


def main():
    parser = Parser(Language(tree_sitter_c_sharp.language()))
    scripts = sorted((ROOT / "Assets").rglob("*.cs"))
    check(len(scripts) == 2, "Exactly two complete C# source files")
    for path in scripts:
        tree = parser.parse(path.read_bytes())
        check(not tree.root_node.has_error, f"C# syntax tree has no errors: {path.relative_to(ROOT)}")
        source = path.read_text()
        check(not re.search(r"\b(TODO|FIXME|NotImplementedException)\b", source),
              f"No incomplete implementation markers: {path.name}")

    assets = sorted(p for p in (ROOT / "Assets").rglob("*") if p.suffix != ".meta")
    guids = []
    for asset in assets:
        meta = asset.with_name(asset.name + ".meta")
        check(meta.is_file(), f"Asset metadata exists: {asset.relative_to(ROOT)}")
        data = yaml.safe_load(meta.read_text())
        guid = data.get("guid", "")
        check(bool(re.fullmatch(r"[0-9a-f]{32}", guid)), f"Valid GUID: {meta.name}")
        guids.append(guid)
    check(len(set(guids)) == len(guids), "Asset GUIDs are unique")

    build_settings = unity_yaml(ROOT / "ProjectSettings/EditorBuildSettings.asset")[0]["EditorBuildSettings"]
    scene_entry = build_settings["m_Scenes"][0]
    scene_path = ROOT / scene_entry["path"]
    check(len(build_settings["m_Scenes"]) == 1 and scene_entry["enabled"] == 1 and scene_path.is_file(),
          "Only entry scene is present and enabled")
    scene_guid = yaml.safe_load(scene_path.with_suffix(".unity.meta").read_text())["guid"]
    check(scene_entry["guid"] == scene_guid, "Build scene GUID matches its metadata")
    scene_documents = unity_yaml(scene_path)
    check([next(iter(document)) for document in scene_documents] == ["GameObject", "Transform", "SceneRoots"],
          "Entry scene YAML parses with a GameObject, Transform and SceneRoots")
    scene_text = scene_path.read_text()
    anchors = set(re.findall(r"^--- !u!\d+ &(\d+)", scene_text, flags=re.MULTILINE))
    references = set(re.findall(r"fileID: (\d+)", scene_text)) - {"0"}
    check(references <= anchors, "Every nonzero scene object reference resolves")

    settings = unity_yaml(ROOT / "ProjectSettings/ProjectSettings.asset")[0]["PlayerSettings"]
    check(settings["activeInputHandler"] == 0, "Legacy keyboard input enabled")
    check(settings["scriptingBackend"]["Standalone"] == 0, "Standalone defaults to Mono")
    check(settings["defaultScreenWidth"] == 1280 and settings["defaultScreenHeight"] == 720
          and settings["fullscreenMode"] == 3, "1280x720 windowed desktop defaults")
    manifest = json.loads((ROOT / "Packages/manifest.json").read_text())
    check(manifest["dependencies"] == {"com.unity.modules.imgui": "1.0.0", "com.unity.modules.inputlegacy": "1.0.0"},
          "Only required built-in modules; no third-party Unity packages")
    check((ROOT / "ProjectSettings/ProjectVersion.txt").read_text().strip() == "m_EditorVersion: 6000.0.62f1",
          "Unity editor version pinned")

    runtime = (ROOT / "Assets/Scripts/SpaceShooterGame.cs").read_text()
    editor = (ROOT / "Assets/Editor/WindowsBuild.cs").read_text()
    check("using UnityEditor" not in runtime and "RuntimeInitializeOnLoadMethod" in runtime,
          "Runtime auto-bootstrap has no UnityEditor dependency")
    check("BuildTarget.StandaloneWindows64" in editor and scene_entry["path"] in editor
          and "ScriptingImplementation.Mono2x" in editor, "Build helper targets the entry scene, Win64 and Mono")
    check(all("KeyCode." + key in runtime for key in
              ["W", "A", "S", "D", "UpArrow", "DownArrow", "LeftArrow", "RightArrow", "Space", "R", "Return"]),
          "All documented keyboard controls are represented")
    check("AudioSource" not in runtime and "AudioListener" not in runtime, "No audio components")
    check("SweptOverlap(shots[s], enemies[e])" in runtime and "SweptOverlap(player, enemy)" in runtime,
          "Shot/enemy and player/enemy collision paths are present")
    check("shots.Clear();" in runtime and "enemies.Clear();" in runtime and "score = 0;" in runtime,
          "Restart clears entity lists and score")
    print(f"\n{checks} source-only checks passed. Unity compilation, import, gameplay and Windows build/run remain UNTESTED.")


if __name__ == "__main__":
    main()
