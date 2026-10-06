"""Offline integrity/syntax checks ONLY. Does not compile C# or execute Unity.
Run: python -m pip install -r Tools/requirements.txt
     python Tools/validate_source.py
"""
from pathlib import Path
import json
import re
import wave
import struct
import yaml
from PIL import Image
from tree_sitter import Language, Parser
import tree_sitter_c_sharp

ROOT = Path(__file__).resolve().parents[1]
checks = []

def check(name, condition, detail):
    checks.append({'name': name, 'pass': bool(condition), 'detail': detail})
    if not condition:
        raise AssertionError(f'{name}: {detail}')

def unity_yaml(path):
    text = path.read_text()
    text = '\n'.join(line for line in text.splitlines() if not line.startswith('%') and not line.startswith('--- !u!'))
    return yaml.safe_load(text)

parser = Parser(Language(tree_sitter_c_sharp.language()))
scripts = sorted((ROOT/'Assets').rglob('*.cs'))
for path in scripts:
    tree = parser.parse(path.read_bytes())
    check('C# syntax: '+path.name, not tree.root_node.has_error, 'Tree-sitter grammar parse, NOT Unity/C# compiler type checking.')
    check('Class/file: '+path.name, bool(re.search(r'class\s+'+path.stem+r'\b', path.read_text())), 'Main class matches source filename.')

assets = [p for p in (ROOT/'Assets').rglob('*') if p.suffix != '.meta']
guids = []
for path in assets:
    meta = Path(str(path)+'.meta')
    check('Metadata: '+path.relative_to(ROOT).as_posix(), meta.is_file(), 'Source files and folders carry .meta files.')
    data = yaml.safe_load(meta.read_text())
    guid = data.get('guid','')
    check('GUID: '+path.name, bool(re.fullmatch(r'[0-9a-f]{32}',guid)), 'Well-formed GUID.')
    guids.append(guid)
check('Unique GUIDs', len(guids)==len(set(guids)), f'{len(guids)} unique source GUIDs.')

images = sorted((ROOT/'Assets').rglob('*.png'))
for path in images:
    with Image.open(path) as im:
        im.load()
        check('PNG: '+path.name, im.mode=='RGBA' and min(im.size)>0, f'{im.width}x{im.height}, {im.mode}')
        alpha=im.getchannel('A').getextrema()
        check('PNG alpha: '+path.name, alpha[0]==0 and alpha[1]>0, 'Contains transparent and visible pixels.')

sounds=sorted((ROOT/'Assets').rglob('*.wav'))
for path in sounds:
    with wave.open(str(path),'rb') as wav:
        check('PCM format: '+path.name, wav.getnchannels()==1 and wav.getsampwidth()==2 and wav.getframerate()==44100, 'Mono 16-bit PCM, 44100 Hz.')
        values=struct.unpack('<'+'h'*wav.getnframes(),wav.readframes(wav.getnframes()))
        peak=max(abs(x) for x in values)
        check('Audio signal: '+path.name, 1000<peak<32767, f'{len(values)/44100:.2f}s; non-silent, unclipped peak {peak}. Audibility not listening-tested.')

manifest=json.loads((ROOT/'Packages/manifest.json').read_text())
check('UI version',manifest['dependencies']['com.unity.ugui']=='1.0.0','UGUI 1.0.0 pinned; no external gameplay libraries.')
check('No new Input System', 'com.unity.inputsystem' not in manifest['dependencies'],'Legacy KeyCode polling.')
check('Unity version','2022.3.62f3' in (ROOT/'ProjectSettings/ProjectVersion.txt').read_text(),'Pinned documented 2022.3 LTS patch.')
settings=unity_yaml(ROOT/'ProjectSettings/ProjectSettings.asset')['PlayerSettings']
check('Legacy input enabled',settings['activeInputHandler']==0,'Input Manager (Old).')
axes=unity_yaml(ROOT/'ProjectSettings/InputManager.asset')['InputManager']['m_Axes']
check('UI input axes', {a['m_Name'] for a in axes}=={'Horizontal','Vertical','Submit','Cancel'},'StandaloneInputModule axes present.')
layers=unity_yaml(ROOT/'ProjectSettings/TagManager.asset')['TagManager']['layers']
check('Layers',len(layers)==32 and layers[8:13]==['Player','Enemy','PlayerBullet','EnemyBullet','Powerup'],'32 layers with reserved game layer indexes.')
for path in (ROOT/'ProjectSettings').glob('*.asset'):
    check('Settings YAML: '+path.name,isinstance(unity_yaml(path),dict),'Parses as a Unity-tag-stripped YAML mapping; not proof of Unity import.')

setup=(ROOT/'Assets/Editor/ProjectSetup.cs').read_text()
for asset in re.findall(r'(?:Sprite|Sound)\("([a-z-]+)"\)',setup):
    check('Builder asset: '+asset,(ROOT/f'Assets/Art/{asset}.png').exists() or (ROOT/f'Assets/Audio/{asset}.wav').exists(),'Literal builder resource exists.')
for asset in ['player','scout','fan','spinner','repair','rapid','triple','bolt','orb']:
    check('Prefab source: '+asset,(ROOT/f'Assets/Art/{asset}.png').is_file(),'Actor artwork exists for generated prefab.')
all_cs='\n'.join(p.read_text() for p in scripts)
check('No placeholder implementation',not re.search(r'NotImplementedException|\bTODO\b|\bFIXME\b',all_cs),'No stub exception/TODO markers in C#.')
check('Desktop build target','BuildTarget.StandaloneWindows64' in setup and 'ScriptingImplementation.Mono2x' in setup,'Windows x86_64 / Mono selected in build code.')
check('Prefab physics configuration',all(x in setup for x in ['RigidbodyType2D.Dynamic','body.gravityScale = 0','collider.isTrigger = true','CollisionDetectionMode2D.Continuous']),'Source sets dynamic zero-gravity trigger bodies and continuous collision detection.')
# Inspect the explicitly coded truth table, not a fabricated Unity physics test.
game=(ROOT/'Assets/Scripts/GameController.cs').read_text()
check('Collision truth-table source','(a == 8 && (b == 9 || b == 11 || b == 12)) || (a == 9 && b == 10)' in game,'Only player/enemy, player/enemy-bullet, player/powerup, enemy/player-bullet pairs enabled.')
check('Pause and teardown source',all(x in game for x in ['Time.timeScale = paused ? 0f : 1f','World.gameObject.SetActive(false)','director.StopAllCoroutines()','OnApplicationFocus']),'Pause, focus loss, coroutine cancellation and deferred-destroy guard are present; behavior NOT runtime tested.')

report={'scope':'OFFLINE source syntax and asset integrity only. Unity compilation, import, Play mode, physics, audio output and Windows builds NOT executed.',
        'counts':{'csharp_scripts':len(scripts),'png_assets':len(images),'wav_assets':len(sounds),'source_meta_files':len(guids)},
        'checks':checks}
(ROOT/'Validation.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({'passed':len(checks),'counts':report['counts'],'scope':report['scope']},indent=2))
