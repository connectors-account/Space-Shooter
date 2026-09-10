"""Offline asset integrity checks, not a Unity compiler or gameplay test.
Run: python Tools/check_assets.py [--preview]
Requires Pillow. Does not call an image-generation service.
"""
import argparse
import json
import math
from pathlib import Path
import struct
import wave
from PIL import Image, ImageDraw
from prepare_assets import cut_sprite

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'Assets/SpaceShooter/Art'
NAMES = ['Player', 'Scout', 'Gunship', 'Commander', 'Bolt', 'EnemyBolt',
         'Repair', 'RapidFire', 'Shield', 'Star']
DURATIONS = {'Laser': 0.13, 'Hit': 0.21, 'Explosion': 0.5, 'Pickup': 0.45, 'GameOver': 1.05}


def check_assets():
    for name in ('ships', 'icons'):
        with Image.open(ROOT / 'ArtSource' / f'{name}-original.png') as sheet:
            assert sheet.size == (1024, 1024), name
            sheet.load()
            if name == 'ships':
                # The measured gutter must not cross a visible ship/exhaust.
                assert max(sheet.convert('RGB').crop((0, 400, 1024, 401)).tobytes()) <= 34
    for name in NAMES:
        with Image.open(ART / f'{name}.png') as sprite:
            assert sprite.size == (256, 256) and sprite.mode == 'RGBA', name
            alpha = sprite.getchannel('A')
            bounds = alpha.getbbox()
            assert bounds and min(bounds[:2]) >= 12 and max(bounds[2:]) <= 244, (name, bounds)
            assert alpha.getextrema()[1] >= 200, name
            assert sum(a > 0 for a in alpha.tobytes()) > 100, name
            for edge in ((0, 0, 256, 1), (0, 255, 256, 256), (0, 0, 1, 256), (255, 0, 256, 256)):
                assert alpha.crop(edge).getextrema() == (0, 0), name
            if name == 'Star':
                assert alpha.getpixel((128, 128)) > 200
                assert alpha.getpixel((32, 32)) == 0
                assert len(set(alpha.tobytes())) > 100, 'Star feathering missing'
            print(f'PASS sprite {name}: RGBA, 256x256, visible, padded transparent edges; bounds={bounds}')
    for name, duration in DURATIONS.items():
        with wave.open(str(ROOT / 'Assets/SpaceShooter/Audio' / f'{name}.wav'), 'rb') as wav:
            assert (wav.getnchannels(), wav.getsampwidth(), wav.getframerate()) == (1, 2, 22050), name
            assert wav.getnframes() == int(22050 * duration), name
            samples = struct.unpack('<' + 'h' * wav.getnframes(), wav.readframes(wav.getnframes()))
            peak = max(abs(s) for s in samples)
            rms = math.sqrt(sum(s * s for s in samples) / len(samples))
            assert 1000 < peak < 32767 and rms > 100, (name, peak, rms)
            assert samples[0] == 0 and abs(samples[-1]) <= 2, name
            print(f'PASS audio {name}: {len(samples)} mono PCM16 samples, peak={peak}, RMS={rms:.1f}')
    for glow in (False, True):
        try:
            cut_sprite(Image.new('RGB', (32, 32)), 0, 0, glow=glow)
        except AssertionError as error:
            assert 'no visible sprite' in str(error)
        else:
            raise AssertionError('Blank sprite was accepted')
    try:
        cut_sprite(Image.new('RGB', (32, 32)), 2, 0)
    except AssertionError:
        pass
    else:
        raise AssertionError('Invalid grid column was accepted')
    with Image.open(ROOT / 'ArtSource/ships-original.png') as sheet:
        assert cut_sprite(sheet, 0, 0, 400).tobytes() == Image.open(ART / 'Player.png').tobytes()
    manifest = json.loads((ROOT / 'Packages/manifest.json').read_text())
    assert manifest['dependencies']['com.unity.modules.physics2d'] == '1.0.0'
    assert '6000.0.62f1' in (ROOT / 'ProjectSettings/ProjectVersion.txt').read_text()
    assert 'activeInputHandler: 0' in (ROOT / 'ProjectSettings/ProjectSettings.asset').read_text()
    print('PASS blank/invalid crop rejection, reproducible player crop, version, package JSON, legacy input setting.')
    print('Unity API compilation, generated scene/prefabs, and Windows runtime are NOT verified by this script.')


def preview():
    image = Image.new('RGB', (1280, 592), (9, 17, 30))
    draw = ImageDraw.Draw(image)
    for i, name in enumerate(NAMES):
        x, y = i % 5 * 256, i // 5 * 296
        with Image.open(ART / f'{name}.png') as sprite:
            image.paste(sprite, (x, y), sprite)
        draw.text((x + 14, y + 265), name, fill=(195, 231, 250))
    path = ROOT / 'Documentation/AssetPreview.png'
    path.parent.mkdir(exist_ok=True)
    image.save(path)
    print('Created Documentation/AssetPreview.png (asset contact sheet, not a gameplay screenshot).')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--preview', action='store_true')
    args = parser.parse_args()
    check_assets()
    if args.preview:
        preview()
