"""Cut AI-generated sprite sheets, remove exterior black, and synthesize original effects.
Requires Pillow only for image processing. Does not draw or substitute the ship artwork.
"""
from collections import deque
from pathlib import Path
import math
import random
import struct
import wave
from PIL import Image, ImageOps

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'Assets/SpaceShooter/Art'
AUDIO = ROOT / 'Assets/SpaceShooter/Audio'


def cut_sprite(sheet, column, row, split_y=None, glow=False):
    w, h = sheet.size
    split_y = h // 2 if split_y is None else split_y
    assert column in (0, 1) and row in (0, 1) and 0 < split_y < h
    # The actual ship sheet has a clear horizontal gutter at y=400, not y=512:
    # both lower ships' exhausts extend into the nominal upper quadrants.
    image = sheet.crop((column * w // 2, 0 if row == 0 else split_y,
                        (column + 1) * w // 2, split_y if row == 0 else h)).convert('RGBA')
    w, h = image.size
    pixels = image.load()
    if glow:
        # Unmatte black-backed luminous icons into straight-alpha textures.
        # Simply cutting their background leaves dark opaque halos in Unity.
        for y in range(h):
            for x in range(w):
                r, g, b, _ = pixels[x, y]
                peak = max(r, g, b)
                alpha = max(0, round((peak - 3) * 255 / 252))
                pixels[x, y] = ((round(r * 255 / peak), round(g * 255 / peak),
                                  round(b * 255 / peak), alpha) if alpha else (0, 0, 0, 0))
    else:
        # Flood only the exterior: preserve the ships' dark internal panels.
        queue = deque([(x, 0) for x in range(w)] + [(x, h - 1) for x in range(w)] + [(0, y) for y in range(h)] + [(w - 1, y) for y in range(h)])
        seen = set()
        while queue:
            x, y = queue.popleft()
            if (x, y) in seen or x < 0 or y < 0 or x >= w or y >= h:
                continue
            seen.add((x, y))
            r, g, b, a = pixels[x, y]
            if max(r, g, b) > 34:
                continue
            pixels[x, y] = (0, 0, 0, 0)
            queue.extend(((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)))
    bounds = image.getbbox()
    assert bounds is not None, 'Generated sheet contained no visible sprite'
    image = image.crop(bounds)
    image.thumbnail((232, 232), Image.Resampling.LANCZOS)
    canvas = Image.new('RGBA', (256, 256))
    canvas.alpha_composite(image, ((256 - image.width) // 2, (256 - image.height) // 2))
    return canvas


def prepare_images():
    ART.mkdir(parents=True, exist_ok=True)
    groups = [('ships', ['Player', 'Scout', 'Gunship', 'Commander']), ('icons', ['Bolt', 'Repair', 'RapidFire', 'Shield'])]
    for source, names in groups:
        with Image.open(ROOT / 'ArtSource' / (source + '-original.png')) as sheet:
            for i, name in enumerate(names):
                split_y = round(sheet.height * 400 / 1024) if source == 'ships' else None
                image = cut_sprite(sheet, i % 2, i // 2, split_y, glow=source == 'icons')
                image.save(ART / (name + '.png'))
    bolt = Image.open(ART / 'Bolt.png').convert('RGBA')
    luminance = ImageOps.grayscale(bolt)
    enemy = Image.merge('RGBA', (luminance, luminance.point(lambda v: int(v * 0.3)), luminance.point(lambda v: int(v * 0.38)), bolt.getchannel('A')))
    enemy.save(ART / 'EnemyBolt.png')
    # Reuse the generated laser's luminous core; feather its extraction mask so
    # the parallax stars have transparent borders rather than opaque square cuts.
    star = bolt.crop((110, 110, 146, 146)).resize((232, 232), Image.Resampling.LANCZOS)
    alpha = star.getchannel('A')
    feathered = []
    for i, a in enumerate(alpha.tobytes()):
        radius = math.hypot((i % 232 - 115.5) / 115.5, (i // 232 - 115.5) / 115.5)
        feathered.append(int(a * max(0.0, 1.0 - radius) ** 1.5))
    alpha.putdata(feathered)
    star.putalpha(alpha)
    canvas = Image.new('RGBA', (256, 256))
    canvas.alpha_composite(star, (12, 12))
    canvas.save(ART / 'Star.png')
    print('Prepared 10 RGBA sprite files.')


def synthesize_audio():
    AUDIO.mkdir(parents=True, exist_ok=True)
    rate = 22050
    random_source = random.Random(8831)
    lengths = {'Laser': 0.13, 'Hit': 0.21, 'Explosion': 0.5, 'Pickup': 0.45, 'GameOver': 1.05}
    for name, duration in lengths.items():
        phase = 0.0
        filtered_noise = 0.0
        samples = []
        for i in range(int(rate * duration)):
            t = i / rate
            p = t / duration
            attack = min(1.0, t / 0.006)
            envelope = attack * (1 - p) ** 2
            if name == 'Laser':
                frequency = 1200 * (1 - p) + 240
                phase += 2 * math.pi * frequency / rate
                value = math.sin(phase) * 0.65 + math.sin(phase * 2) * 0.15
            elif name in ('Hit', 'Explosion'):
                filtered_noise = 0.75 * filtered_noise + 0.25 * random_source.uniform(-1, 1)
                phase += 2 * math.pi * (110 * (1 - p) + 35) / rate
                value = filtered_noise * 1.7 + math.sin(phase) * 0.28
            elif name == 'Pickup':
                frequency = [523.25, 659.25, 783.99, 1046.5][min(3, int(p * 4))]
                phase += 2 * math.pi * frequency / rate
                value = math.sin(phase) * 0.6 + math.sin(phase * 2) * 0.12
            else:
                frequency = [392.0, 329.63, 261.63, 130.81][min(3, int(p * 4))]
                phase += 2 * math.pi * frequency / rate
                value = math.sin(phase) * 0.5 + math.sin(phase / 2) * 0.22
            samples.append(int(max(-0.98, min(0.98, value * envelope * 0.8)) * 32767))
        with wave.open(str(AUDIO / (name + '.wav')), 'wb') as output:
            output.setnchannels(1)
            output.setsampwidth(2)
            output.setframerate(rate)
            output.writeframes(struct.pack('<' + 'h' * len(samples), *samples))
    print('Synthesized 5 original PCM WAV effects.')


if __name__ == '__main__':
    prepare_images()
    synthesize_audio()
