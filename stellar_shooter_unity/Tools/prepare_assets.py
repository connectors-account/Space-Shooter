"""Prepare generated artwork and synthesize original PCM sound effects. No runtime dependency."""
from pathlib import Path
import math
import random
import wave
import struct
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'Assets/StellarShooter/Art'
AUDIO = ROOT / 'Assets/StellarShooter/Audio'
NAMES = ['player', 'scout', 'gunner', 'bomber', 'player-shot', 'enemy-shot', 'repair', 'spread', 'shield']

def sprites():
    sheet = Image.open(ROOT / 'SourceArt/sprite-sheet.png').convert('RGBA')
    for i, name in enumerate(NAMES):
        col, row = i % 3, i // 3
        tile = sheet.crop((round(col * sheet.width / 3), round(row * sheet.height / 3),
                           round((col + 1) * sheet.width / 3), round((row + 1) * sheet.height / 3)))
        # Remove nearly invisible generator halo; retain the original illustrated pixels.
        alpha = tile.getchannel('A').point(lambda a: 0 if a < 12 else min(255, round(a * 255 / 253)))
        tile.putalpha(alpha)
        box = alpha.getbbox()
        if not box:
            raise ValueError(f'Empty generated sprite: {name}')
        tile = tile.crop(box)
        tile.thumbnail((232, 232), Image.Resampling.LANCZOS)
        result = Image.new('RGBA', (256, 256))
        result.alpha_composite(tile, ((256 - tile.width) // 2, (256 - tile.height) // 2))
        result.save(ART / f'{name}.png')
        print(f'Sprite: {name}, original bounds {box}')
    # Isolate bright stars from the generated space texture for a second parallax layer.
    space = Image.open(ART / 'space.png').convert('RGBA')
    star_alpha = space.convert('RGB').convert('L').point(lambda v: max(0, min(255, round((v - 90) * 2.2))))
    space.putalpha(star_alpha)
    space.save(ART / 'stars.png')

def tone(name, duration, start, end, noise=0, notes=None):
    rate = 44100
    rng = random.Random(name)
    samples = []
    phase = 0.0
    last_noise = 0.0
    for i in range(round(duration * rate)):
        t = i / rate
        u = t / duration
        f = start + (end - start) * u
        if notes:
            f = notes[min(len(notes) - 1, int(u * len(notes)))]
        phase += 2 * math.pi * f / rate
        last_noise = .6 * last_noise + .4 * rng.uniform(-1, 1)
        env = min(1, t / .008) * (1-u)**1.6
        signal = ((math.sin(phase) + .2 * math.sin(2 * phase)) * (1-noise) + last_noise * noise)
        samples.append(struct.pack('<h', round(max(-1, min(1, signal * env * .55)) * 32767)))
    with wave.open(str(AUDIO / f'{name}.wav'), 'wb') as out:
        out.setnchannels(1)
        out.setsampwidth(2)
        out.setframerate(rate)
        out.writeframes(b''.join(samples))
    print(f'Sound: {name}, {duration}s mono PCM')

if __name__ == '__main__':
    ART.mkdir(parents=True, exist_ok=True)
    AUDIO.mkdir(parents=True, exist_ok=True)
    sprites()
    tone('shoot', .12, 1250, 420)
    tone('enemy-fire', .17, 500, 180)
    tone('hit', .2, 170, 50, .65)
    tone('explosion', .55, 120, 25, .92)
    tone('pickup', .42, 400, 1200, notes=[523, 659, 784, 1047])
    tone('wave', .65, 300, 900, notes=[392, 523, 659])
    tone('game-over', 1.1, 400, 80, notes=[392, 330, 262, 196])
    tone('click', .08, 700, 450)
