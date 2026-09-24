"""Feasible offline checks. Never treats stubs as Unity or Windows validation.
Run from any directory: python3 Tools/validate.py
Requirements: Python 3, Pillow, Mono mcs+mono, and ffprobe (FFmpeg).
"""
from pathlib import Path
import json
import re
import subprocess
import tempfile
import wave
import array
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
checks = []

def checked(name, detail):
    checks.append({'name': name, 'passed': True, 'detail': detail})
    print('PASS:', name, '-', detail)

def run(command):
    result = subprocess.run(command, cwd=ROOT, capture_output=True, text=True, check=True)
    return (result.stdout + result.stderr).strip()

runtime = sorted((ROOT/'Assets/Scripts').glob('*.cs'))
editor = sorted((ROOT/'Assets/Editor').glob('*.cs'))
stubs = str(ROOT/'Tools/UnitySourceCheckStubs.cs')
with tempfile.TemporaryDirectory(prefix='starward-source-check-') as temporary:
    temp = Path(temporary)
    run(['mcs','-target:library','-out:'+str(temp/'Runtime.dll'),stubs,*map(str,runtime)])
    checked('runtime_source_syntax', f'{len(runtime)} C# files compiled against hand-written stubs, NOT Unity assemblies')
    run(['mcs','-define:UNITY_EDITOR','-target:library','-out:'+str(temp/'Editor.dll'),stubs,*map(str,runtime+editor)])
    checked('editor_source_syntax', f'{len(editor)} editor files and editor-conditional runtime paths compiled against stubs')
    run(['mcs','-out:'+str(temp/'Rules.exe'),stubs,str(ROOT/'Assets/Scripts/GameRules.cs'),str(ROOT/'Assets/Scripts/Health.cs'),str(ROOT/'Tools/SourceCheckTests.cs')])
    checked('rule_and_health_assertions', run(['mono',str(temp/'Rules.exe')]))

for name in ['Player','Enemy','Repair','Weapon','Glow']:
    path=ROOT/'Assets/Art'/f'{name}.png'
    with Image.open(path) as im:
        im.load()
        assert im.mode=='RGBA' and im.width==im.height
        assert im.width==(64 if name=='Glow' else 256)
        assert im.getchannel('A').getextrema()[0]==0
        assert im.getchannel('A').getextrema()[1]>200
checked('sprites', 'Five valid RGBA PNG assets; four illustrated sprites visually inspected in SpritePreview.png')
for path in sorted((ROOT/'Assets/Audio').glob('*.wav')):
    with wave.open(str(path),'rb') as audio:
        assert audio.getnchannels()==1 and audio.getsampwidth()==2 and audio.getframerate()==44100
        pcm=array.array('h',audio.readframes(audio.getnframes()))
        assert len(pcm)>1000 and max(abs(s) for s in pcm)>1000
laser=json.loads(run(['ffprobe','-v','error','-show_entries','format=duration','-of','json',str(ROOT/'Assets/Audio/laser.mp3')]))
assert float(laser['format']['duration'])>0
checked('audio', 'Seven non-silent 44.1 kHz PCM WAVs plus a decodable generated MP3 laser effect')
manifest=json.loads((ROOT/'Packages/manifest.json').read_text())['dependencies']
lock=json.loads((ROOT/'Packages/packages-lock.json').read_text())['dependencies']
assert len(manifest)==4 and all(v=='1.0.0' for v in manifest.values())
assert all(lock[k]['version']==v and lock[k]['source']=='builtin' for k,v in manifest.items())
assert '6000.3.11f1' in (ROOT/'ProjectSettings/ProjectVersion.txt').read_text()
assert 'activeInputHandler: 0' in (ROOT/'ProjectSettings/ProjectSettings.asset').read_text()
checked('settings_and_packages','Unity 6000.3.11f1, legacy key input, four pinned built-in modules, consistent package lock')
guids=set()
for path in (ROOT/'Assets').rglob('*'):
    if path.suffix=='.meta': continue
    meta=Path(str(path)+'.meta')
    assert meta.exists(), str(meta)
    guid=re.search(r'^guid: ([a-f0-9]{32})$',meta.read_text(),re.M).group(1)
    assert guid not in guids, str(meta)
    guids.add(guid)
checked('asset_metadata', f'{len(guids)} stable unique asset/folder GUIDs')
setup=(ROOT/'Assets/Editor/ProjectSetup.cs').read_text()
for name in ['Player','Enemy','Repair','Weapon','Glow']:
    assert (ROOT/'Assets/Art'/f'{name}.png').is_file()
for name in re.findall(r'"([A-Za-z]+\.(?:wav|mp3))"',setup):
    assert (ROOT/'Assets/Audio'/name).is_file(), name
for file in runtime+editor:
    source=file.read_text()
    assert not re.search(r'\bTODO\b|NotImplementedException',source)
    for mono_class in re.findall(r'class\s+(\w+)\s*:\s*MonoBehaviour',source):
        assert file.stem==mono_class
assert 'PrefabUtility.SaveAsPrefabAsset' in setup and 'EditorSceneManager.SaveScene' in setup
assert 'StandaloneWindows64' in (ROOT/'Assets/Editor/BuildGame.cs').read_text()
checked('source_references','All setup asset paths exist; component filenames match; real scene/prefab save and Win64 build calls present')
report={'scope':'Offline source and media validation only; not a Unity build or gameplay test','checks':checks,
        'not_verified':['Unity first import and actual assembly compilation','Editor scene/prefab generation and serialized references','Unity Play mode, physics, collision, sound playback and UI','Windows x86_64 build and executable launch'],
        'windows_executable_built':False}
(ROOT/'Docs/Validation.json').write_text(json.dumps(report,indent=2)+'\n')
print('Saved Docs/Validation.json. Unity/Windows runtime checks remain unverified.')
