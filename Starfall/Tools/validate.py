"""Source/structure checks only; not a substitute for a Unity import/build/playtest.
Dependencies: python -m pip install tree-sitter tree-sitter-c-sharp pyyaml
Run from any directory: python Tools/validate.py
"""
import json
import re
from pathlib import Path
import tree_sitter_c_sharp
import yaml
from tree_sitter import Language, Parser

ROOT = Path(__file__).resolve().parents[1]
parser = Parser(Language(tree_sitter_c_sharp.language()))
files = sorted(ROOT.rglob('*.cs'))
files = [p for p in files if not any(x in p.parts for x in ('Library', 'Temp', 'Builds'))]
errors = []
for path in files:
    source = path.read_bytes()
    tree = parser.parse(source)
    if tree.root_node.has_error:
        errors.append(f'C# parse error: {path.relative_to(ROOT)}')
    if re.search(rb'\b(TODO|FIXME|NotImplementedException)\b', source):
        errors.append(f'Unfinished code: {path.relative_to(ROOT)}')
    if 'Assets' in path.parts:
        assert Path(str(path) + '.meta').exists(), f'Missing script metadata: {path}'
assert not errors, '\n'.join(errors)
assert len(files) < 20, 'C# file budget exceeded'
lines = sum(len(p.read_text().splitlines()) for p in files)
assert lines < 5000, 'C# line budget exceeded'

guids = {}
for meta in (ROOT / 'Assets').rglob('*.meta'):
    match = re.search(r'^guid: ([a-f0-9]{32})$', meta.read_text(), re.M)
    assert match, f'Invalid GUID in {meta}'
    assert match[1] not in guids, f'Duplicate GUID: {meta}'
    guids[match[1]] = meta
scene = ROOT / 'Assets/Starfall/Scenes/Starfall.unity'
scene_text = scene.read_text()
for guid in re.findall(r'guid: ([a-f0-9]{32})', scene_text):
    assert guid in guids, f'Scene GUID unresolved: {guid}'
    assert guids[guid].name == 'SceneEntry.cs.meta'
for path in [scene, *(ROOT / 'ProjectSettings').glob('*.asset')]:
    text = re.sub(r'^%.*\n', '', path.read_text(), flags=re.M)
    text = re.sub(r'^--- !u!\d+ &(\d+)', r'---', text, flags=re.M)
    assert all(isinstance(d, dict) for d in yaml.safe_load_all(text)), f'Invalid YAML: {path}'
scene_guid = re.search(r'^guid: (\w+)', Path(str(scene) + '.meta').read_text(), re.M)[1]
build_settings = (ROOT / 'ProjectSettings/EditorBuildSettings.asset').read_text()
assert scene_guid in build_settings and 'enabled: 1' in build_settings
assert 'Assets/Starfall/Scenes/Starfall.unity' in build_settings
manifest = json.loads((ROOT / 'Packages/manifest.json').read_text())
assert set(manifest['dependencies']) == {
    'com.unity.modules.audio', 'com.unity.modules.imageconversion',
    'com.unity.modules.imgui', 'com.unity.modules.physics2d'
}
assert 'activeInputHandler: 0' in (ROOT / 'ProjectSettings/ProjectSettings.asset').read_text()
assert 'm_EditorVersion: 2022.3.70f1' in (ROOT / 'ProjectSettings/ProjectVersion.txt').read_text()
print(f'PASS: C# syntax parsed for {len(files)} files / {lines} lines; no unfinished-code markers.')
print(f'PASS: {len(guids)} unique metadata GUIDs, bootstrap scene references, YAML, build-scene registration, built-in packages and legacy input setting.')
print('NOT CHECKED: Unity API compilation, asset generation/import, actual scene loading, graphics, audio, physics, input, or Windows runtime.')
