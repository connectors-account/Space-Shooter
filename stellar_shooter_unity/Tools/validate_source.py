"""Offline integrity/syntax checks. Does NOT compile Unity assemblies or exercise Unity."""
from pathlib import Path
import json
import re
import struct
import wave
from PIL import Image
from tree_sitter import Language, Parser
import tree_sitter_c_sharp

ROOT = Path(__file__).resolve().parents[1]
parser = Parser(Language(tree_sitter_c_sharp.language()))
checks = 0

def require(condition, message):
    global checks
    if not condition:
        raise AssertionError(message)
    checks += 1

manifest = json.loads((ROOT / 'Packages/manifest.json').read_text())
require(manifest['dependencies']['com.unity.ugui'] == '2.0.0', 'Pinned UGUI')
require('com.unity.inputsystem' not in manifest['dependencies'], 'Use one input implementation')
require('activeInputHandler: 0' in (ROOT / 'ProjectSettings/ProjectSettings.asset').read_text(), 'Legacy input enabled')
require('6000.0.62f1' in (ROOT / 'ProjectSettings/ProjectVersion.txt').read_text(), 'Pinned editor')
for axis in ['Horizontal', 'Vertical', 'Submit', 'Cancel']:
    require(f'm_Name: {axis}' in (ROOT / 'ProjectSettings/InputManager.asset').read_text(), f'UI axis {axis}')

sources = sorted(ROOT.glob('Assets/**/*.cs')) + sorted(ROOT.glob('Tools/*.cs'))
for source in sources:
    data = source.read_bytes()
    tree = parser.parse(data)
    require(not tree.root_node.has_error, f'C# syntax: {source.relative_to(ROOT)}')
    require(not re.search(rb'\b(TODO|FIXME|NotImplementedException)\b', data), f'No incomplete code: {source}')
    if source.parent.name == 'Scripts' and b': MonoBehaviour' in data:
        require(f'class {source.stem} '.encode() in data, f'MonoBehaviour matches file: {source}')

assets = ROOT / 'Assets/StellarShooter'
art = sorted((assets / 'Art').glob('*.png'))
require(len(art) == 11, 'Expected all nine sprites and two parallax textures')
for path in art:
    with Image.open(path) as im:
        im.load()
        require(im.width > 0 and im.height > 0, f'Valid PNG {path.name}')
        if path.stem not in ('space', 'stars'):
            require(im.size == (256, 256), f'Sprite dimensions {path.name}')
            require(im.mode == 'RGBA', f'Alpha channel {path.name}')
            alpha = im.getchannel('A')
            require(alpha.getextrema()[0] == 0 and alpha.getextrema()[1] > 240, f'Transparent and opaque pixels {path.name}')
            bbox = alpha.getbbox()
            require(bbox[0] > 0 and bbox[1] > 0 and bbox[2] < 256 and bbox[3] < 256, f'Safe transparent sprite margins {path.name}')

sounds = sorted((assets / 'Audio').glob('*.wav'))
require(len(sounds) == 8, 'Eight sound cues')
for path in sounds:
    with wave.open(str(path)) as audio:
        require(audio.getnchannels() == 1 and audio.getsampwidth() == 2 and audio.getframerate() == 44100, f'PCM format {path.name}')
        samples = struct.unpack('<' + 'h'*audio.getnframes(), audio.readframes(audio.getnframes()))
        require(len(samples) > 1000 and max(abs(s) for s in samples) > 1000, f'Non-silent audio {path.name}')
        require(max(abs(s) for s in samples) < 32767, f'Unclipped audio {path.name}')

guids = []
for path in (ROOT / 'Assets').rglob('*'):
    if path.suffix == '.meta':
        continue
    meta = Path(str(path) + '.meta')
    require(meta.exists(), f'Metadata for {path.relative_to(ROOT)}')
    match = re.search(r'^guid: ([a-f0-9]{32})$', meta.read_text(), re.M)
    require(match is not None, f'Valid GUID {meta}')
    guids.append(match.group(1))
require(len(guids) == len(set(guids)), 'Unique Unity GUIDs')

builder = (assets / 'Editor/ProjectBuilder.cs').read_text()
for art_name in ['player', 'scout', 'gunner', 'bomber', 'player-shot', 'enemy-shot', 'repair', 'spread', 'shield', 'space', 'stars']:
    require(f'"{art_name}"' in builder, f'Builder consumes sprite {art_name}')
for path in sounds:
    require(f'"{path.stem}"' in builder, f'Builder consumes sound {path.stem}')
for marker in ['PrefabUtility.SaveAsPrefabAsset', 'EditorSceneManager.SaveScene', 'EditorBuildSettings.scenes',
               'BuildTarget.StandaloneWindows64', '"Architecture", "x64"', 'BuildPipeline.BuildPlayer',
               'ValidateLoadedScene', 'StandaloneBuildSubtarget.Player']:
    require(marker in builder, f'Implemented setup/build operation: {marker}')
print(f'PASS: {checks} offline integrity/syntax assertions; {len(sources)} C# files parsed, {len(art)} PNGs and {len(sounds)} WAVs decoded.')
print('NOT RUN: Unity import/assembly compilation, scene generation, Play mode, Windows player build/runtime.')
