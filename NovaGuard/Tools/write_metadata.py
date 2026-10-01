"""Assign reproducible GUIDs before first Unity import. Unity fills importer defaults."""
from pathlib import Path
import uuid

ROOT = Path(__file__).resolve().parents[1]
NAMESPACE = uuid.UUID('d12594aa-d8ed-49c8-950c-b08d16e084b7')

def main():
    for path in sorted((ROOT / 'Assets').rglob('*')):
        if path.suffix == '.meta':
            continue
        meta = Path(str(path) + '.meta')
        if meta.exists():
            continue
        relative = path.relative_to(ROOT).as_posix()
        guid = uuid.uuid5(NAMESPACE, relative).hex
        content = 'fileFormatVersion: 2\nguid: ' + guid + '\n'
        if path.is_dir():
            content += 'folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n'
        elif path.suffix == '.cs':
            content += 'MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n'
        # Image/audio files deliberately use Unity's default importer settings,
        # then ProjectSetup applies explicit Sprite settings before generating prefabs.
        meta.write_text(content)
    print('Stable Unity metadata written.')

if __name__ == '__main__':
    main()
