"""Create stable Unity metadata and minimal native settings before first import."""
from pathlib import Path
import hashlib
import json
root = Path(__file__).resolve().parents[1]
settings = root/'ProjectSettings'
(settings/'ProjectSettings.asset').write_text('''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!129 &1
PlayerSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 28
  companyName: Starward Studio
  productName: Starward
  bundleVersion: 1.0.0
  defaultScreenWidth: 1280
  defaultScreenHeight: 720
  defaultIsNativeResolution: 0
  m_ActiveColorSpace: 0
  runInBackground: 0
  resizableWindow: 1
  fullscreenMode: 3
  activeInputHandler: 0
  scriptingBackend:
    Standalone: 0
''')
layers = ['Default','TransparentFX','Ignore Raycast','','Water','UI','','','Player','Enemy','PlayerBullet','EnemyBullet','Pickup'] + ['']*19
(settings/'TagManager.asset').write_text('''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!78 &1
TagManager:
  serializedVersion: 2
  tags: []
  layers:
''' + ''.join(('  - '+v).rstrip()+'\n' for v in layers) + '''  m_SortingLayers:
  - name: Default
    uniqueID: 0
    locked: 0
  m_RenderingLayers:
  - Default
''')
(settings/'TimeManager.asset').write_text('''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!5 &1
TimeManager:
  m_ObjectHideFlags: 0
  Fixed Timestep: 0.02
  Maximum Allowed Timestep: 0.1
  m_TimeScale: 1
  Maximum Particle Timestep: 0.03
''')
(settings/'InputManager.asset').write_text('''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!13 &1
InputManager:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Axes: []
''')
(settings/'EditorBuildSettings.asset').write_text('''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1045 &1
EditorBuildSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Scenes: []
  m_configObjects: {}
''')
manifest=json.loads((root/'Packages/manifest.json').read_text())
(root/'Packages/packages-lock.json').write_text(json.dumps({'dependencies':{name:{'version':ver,'depth':0,'source':'builtin','dependencies':{}} for name,ver in manifest['dependencies'].items()}},indent=2)+'\n')
for p in sorted((root/'Assets').rglob('*')):
    if p.suffix == '.meta': continue
    meta=Path(str(p)+'.meta')
    if meta.exists(): continue
    guid=hashlib.md5(('starward/'+p.relative_to(root).as_posix()).encode()).hexdigest()
    text='fileFormatVersion: 2\nguid: '+guid+'\n'
    if p.is_dir(): text += 'folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n'
    elif p.suffix=='.cs': text += 'MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n'
    else:
        # Unity fills in default importer details, then ProjectSetup configures sprites.
        text += ('TextureImporter' if p.suffix=='.png' else 'AudioImporter')+':\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n'
    meta.write_text(text)
print('Wrote project settings, built-in package lock, and stable asset GUIDs.')
