"""Create stable Unity GUIDs for source assets. Never replaces existing GUIDs."""
from pathlib import Path
import uuid

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / 'Assets'

def write_metadata():
    count = 0
    for path in sorted(ASSETS.rglob('*')):
        if path.suffix == '.meta':
            continue
        meta = Path(str(path) + '.meta')
        if meta.exists():
            continue
        guid = uuid.uuid5(uuid.NAMESPACE_URL, 'stellar-shooter/' + path.relative_to(ROOT).as_posix()).hex
        text = f'fileFormatVersion: 2\nguid: {guid}\n'
        if path.is_dir():
            text += 'folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
        elif path.suffix == '.cs':
            text += 'MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
        elif path.suffix == '.png':
            text += 'TextureImporter:\n  externalObjects: {}\n  serializedVersion: 13\n  textureType: 8\n  spriteMode: 1\n  spritePixelsToUnits: 256\n  alphaIsTransparency: 1\n  maxTextureSize: 1024\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
        elif path.suffix == '.wav':
            text += 'AudioImporter:\n  externalObjects: {}\n  serializedVersion: 7\n  defaultSettings:\n    serializedVersion: 2\n    loadType: 0\n    sampleRateSetting: 0\n    sampleRateOverride: 44100\n    compressionFormat: 0\n    quality: 1\n    conversionMode: 0\n    preloadAudioData: 1\n  platformSettingOverrides: {}\n  forceToMono: 1\n  normalize: 1\n  loadInBackground: 0\n  ambisonic: 0\n  3D: 0\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
        else:
            text += 'DefaultImporter:\n  externalObjects: {}\n'
        meta.write_text('\n'.join(line.rstrip() for line in text.splitlines()) + '\n')
        count += 1
    print(f'Created {count} stable metadata files.')

if __name__ == '__main__':
    write_metadata()
