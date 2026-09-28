"""Static project/asset checks. Not a Unity compiler or runtime substitute.
Install: python -m pip install Pillow tree-sitter tree-sitter-c-sharp
Run: python Tools/check_project.py
"""
from pathlib import Path
import json, re, wave
from PIL import Image, ImageDraw
from tree_sitter import Language, Parser
import tree_sitter_c_sharp
ROOT=Path(__file__).resolve().parents[1]
checks=[]
def check(name, condition):
    checks.append({'name':name,'pass':bool(condition)})
    if not condition: raise AssertionError(name)
parser=Parser(Language(tree_sitter_c_sharp.language()))
scripts=list((ROOT/'Assets').rglob('*.cs'))
for file in scripts:
    tree=parser.parse(file.read_bytes())
    check('C# syntax: '+file.name,not tree.root_node.has_error)
    check('No unfinished markers: '+file.name,not re.search(r'\b(TODO|FIXME|NotImplementedException)\b',file.read_text()))
editor=(ROOT/'Assets/Starfall/Editor/ProjectSetup.cs').read_text()
art=ROOT/'Assets/Starfall/Art'
names=['player','scout','fighter','gunship','boss','repair','spread','shield','bolt','particle']
for name in names:
    im=Image.open(art/f'{name}.png'); im.load()
    alpha=im.getchannel('A')
    check('Valid transparent sprite: '+name, im.mode=='RGBA' and min(im.size)>=8 and alpha.getextrema()==(0,255))
    check('Sprite bootstrap reference: '+name, '"'+name+'"' in editor or name+'.png' in editor)
for file in sorted((ROOT/'Assets/Starfall/Audio').glob('*.wav')):
    with wave.open(str(file)) as audio:
        check('PCM WAV: '+file.stem,audio.getnframes()>1000 and audio.getnchannels()==1 and audio.getsampwidth()==2 and audio.getframerate()==22050)
    check('Audio bootstrap reference: '+file.stem,'Clip("'+file.stem+'")' in editor)
manifest=json.loads((ROOT/'Packages/manifest.json').read_text())
check('Only pinned built-in packages',len(manifest['dependencies'])==3 and all(v=='1.0.0' for v in manifest['dependencies'].values()))
check('Exact editor pin','6000.0.62f1' in (ROOT/'ProjectSettings/ProjectVersion.txt').read_text())
check('Legacy input enabled','activeInputHandler: 0' in (ROOT/'ProjectSettings/ProjectSettings.asset').read_text())
check('Both gamepad axes configured','AddAxis(axes,"PadX",0,false)' in editor and 'AddAxis(axes,"PadY",1,true)' in editor)
check('Build is Windows x64','target=BuildTarget.StandaloneWindows64' in editor)
check('Automatic import setup','[InitializeOnLoad]' in editor and 'delayCall+=AutoSetup' in editor)
check('Generated scene registered in build','EditorBuildSettingsScene(ScenePath,true)' in editor)
check('Prefab references serialized','objectReferenceValue=assets' in editor and 'SaveAsPrefabAsset' in editor)
# Human-reviewed contact sheet of real bundled assets, NOT a gameplay screenshot.
preview=Image.new('RGB',(1000,520),'#070e20'); draw=ImageDraw.Draw(preview)
for i,name in enumerate(names):
    im=Image.open(art/f'{name}.png').convert('RGBA'); im.thumbnail((170,195))
    x=(i%5)*200+(200-im.width)//2; y=(i//5)*260+20+(195-im.height)//2
    preview.paste(im,(x,y),im)
    draw.text(((i%5)*200+12,(i//5)*260+231),name.upper(),fill='#83ecff')
preview.save(ROOT/'Docs/AssetPreview.png')
report={'scope':'Static syntax, source wiring and asset checks only; not Unity compilation or playtesting.', 'checks':checks}
(ROOT/'Docs/StaticChecks.json').write_text(json.dumps(report,indent=2)+'\n')
print(f'{len(checks)} static checks passed across {len(scripts)} Unity C# files, 10 sprites and 10 WAV clips.')
