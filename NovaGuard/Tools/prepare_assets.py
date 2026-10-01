"""Crop the generated original sheet and synthesize original mono PCM effects.
Requires Pillow; run from any directory. No downloads are performed.
"""
from pathlib import Path
import math
import random
import struct
import wave
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'Assets/Art'
AUDIO = ROOT / 'Assets/Audio'
CROPS = {
    'Player': (14, 35, 342, 334),
    'Scout': (390, 52, 632, 323),
    'Gunner': (680, 45, 1009, 324),
    'Tank': (0, 350, 382, 668),
    'Repair': (404, 401, 618, 640),
    'Rapid': (721, 406, 957, 641),
    'Shield': (67, 734, 318, 985),
    'PlayerBolt': (445, 703, 580, 990),
    'EnemyBolt': (789, 717, 895, 990),
}

def sprites():
    source = Image.open(ART / 'source-sheet.png').convert('RGBA')
    assert source.size == (1024, 1024)
    assert source.getchannel('A').getextrema()[0] == 0
    for name, box in CROPS.items():
        sprite = source.crop(box)
        # The generated tank's nose points up; enemies fire toward the screen bottom.
        if name == 'Tank':
            sprite = sprite.transpose(Image.Transpose.ROTATE_180)
        sprite.thumbnail((256, 256), Image.Resampling.LANCZOS)
        # A transparent border prevents texture filtering from bleeding at edges.
        result = Image.new('RGBA', (sprite.width + 8, sprite.height + 8))
        result.paste(sprite, (4, 4))
        result.save(ART / (name + '.png'))

def sounds():
    AUDIO.mkdir(exist_ok=True)
    rate = 44100
    settings = {
        'Fire': (0.11, 1150, 310, 0.0),
        'EnemyFire': (0.16, 540, 190, 0.03),
        'Hit': (0.20, 210, 65, 0.5),
        'Explosion': (0.43, 130, 35, 0.8),
        'Pickup': (0.34, 480, 1200, 0.0),
        'Wave': (0.55, 330, 880, 0.0),
        'GameOver': (0.85, 360, 75, 0.12),
        'Click': (0.075, 650, 920, 0.0),
    }
    randomizer = random.Random(731)
    for name, (duration, start, end, noise) in settings.items():
        count = int(rate * duration)
        frames = bytearray()
        phase = 0.0
        for i in range(count):
            u = i / count
            frequency = start + (end - start) * u
            phase += math.tau * frequency / rate
            envelope = min(1, i / (rate * .006)) * ((1 - u) ** 2)
            tone = math.sin(phase) * .8 + math.sin(phase * 2) * .2
            value = ((1 - noise) * tone + noise * randomizer.uniform(-1, 1)) * envelope * .65
            frames.extend(struct.pack('<h', round(value * 32767)))
        with wave.open(str(AUDIO / (name + '.wav')), 'wb') as output:
            output.setnchannels(1)
            output.setsampwidth(2)
            output.setframerate(rate)
            output.writeframes(frames)
        print(f'{name}: {count} samples')

if __name__ == '__main__':
    sprites()
    sounds()
