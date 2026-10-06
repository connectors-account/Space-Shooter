"""Reproduce derived sprites, geometric VFX/icons, and original PCM sound effects.
Requires Python 3 + Pillow + numpy. Not needed to open or build the Unity project.
The four ship designs are cropped from the included original AI-generated atlas.
"""
from pathlib import Path
import hashlib
import math
import random
import wave
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'Assets/Art'
AUDIO = ROOT / 'Assets/Audio'

def sprites():
    atlas = Image.open(ART / 'Source/ships-atlas.png').convert('RGBA')
    for i, name in enumerate(['player', 'scout', 'fan', 'spinner']):
        x, y = (i % 2) * 512, (i // 2) * 512
        im = atlas.crop((x, y, x + 512, y + 512))
        alpha = im.getchannel('A').point(lambda a: 0 if a < 12 else min(255, round(a * 255 / 253)))
        im.putalpha(alpha)
        im = im.crop(alpha.getbbox())
        im.thumbnail((232, 232), Image.Resampling.LANCZOS)
        tile = Image.new('RGBA', (256, 256))
        tile.alpha_composite(im, ((256 - im.width) // 2, (256 - im.height) // 2))
        tile.save(ART / f'{name}.png')
    # Simple geometric effects and readable power-up symbols, not ship artwork.
    for name, color, symbol in [('repair', '#7affb1', '+'), ('rapid', '#ffc56a', 'R'), ('triple', '#d598ff', 'T')]:
        im = Image.new('RGBA', (64, 64)); d = ImageDraw.Draw(im)
        d.rounded_rectangle((5, 5, 58, 58), radius=13, fill='#12253eff', outline=color, width=4)
        if symbol == '+':
            d.rectangle((27, 16, 36, 47), fill=color); d.rectangle((16, 27, 47, 36), fill=color)
        elif symbol == 'R':
            d.polygon([(35, 13), (20, 35), (30, 35), (27, 51), (45, 27), (34, 27)], fill=color)
        else:
            for x in (19, 32, 45):
                d.line((x, 43, x, 23), fill=color, width=4)
                d.polygon([(x-5, 25), (x, 17), (x+5, 25)], fill=color)
        im.save(ART / f'{name}.png')
    for name, size, color in [('bolt', (16, 40), '#8df7ff'), ('orb', (24, 24), '#ff856f')]:
        im=Image.new('RGBA', size); d=ImageDraw.Draw(im)
        d.ellipse((2, 2, size[0]-3, size[1]-3), fill=color)
        d.ellipse((size[0]*.35, size[1]*.2, size[0]*.65, size[1]*.8), fill='white')
        im.save(ART / f'{name}.png')
    rng = random.Random(314159)
    for name, count, radius in [('stars-far', 150, 1), ('stars-near', 42, 2)]:
        im=Image.new('RGBA',(512,512)); d=ImageDraw.Draw(im)
        for _ in range(count):
            x,y=rng.randrange(4,508),rng.randrange(4,508)
            d.ellipse((x-radius,y-radius,x+radius,y+radius), fill=(rng.randrange(140,220),210,255,rng.randrange(100,240)))
        im.save(ART / f'{name}.png')
    # Radially faded spark shared by the short explosion burst.
    yy,xx=np.mgrid[-1:1:32j,-1:1:32j]
    rgba=np.full((32,32,4),255,dtype=np.uint8)
    rgba[:,:,3]=(np.maximum(0,1-np.sqrt(xx*xx+yy*yy))**2*255).astype(np.uint8)
    Image.fromarray(rgba).save(ART / 'spark.png')

def sounds():
    sr=44100
    rng=np.random.default_rng(2026)
    specs={'shoot':(.10,950,330),'enemy-shot':(.16,330,170),'hit':(.25,180,70),
           'explode':(.40,140,35),'pickup':(.30,550,1250),'wave':(.55,400,900),
           'game-over':(.85,410,75),'ui':(.10,700,1000)}
    for name,(duration,f0,f1) in specs.items():
        t=np.arange(round(sr*duration))/sr
        phase=2*math.pi*(f0*t+(f1-f0)*t*t/(2*duration))
        env=np.minimum(t/.008,1)*(1-t/duration)**2
        signal=np.sin(phase)+.22*np.sin(phase*2)
        if name in ('hit','explode'): signal=.45*signal+.55*rng.uniform(-1,1,len(t))
        pcm=(np.clip(signal*env*.45,-1,1)*32767).astype('<i2')
        with wave.open(str(AUDIO/f'{name}.wav'),'wb') as out:
            out.setnchannels(1);out.setsampwidth(2);out.setframerate(sr);out.writeframes(pcm.tobytes())

def metas():
    # Stable GUIDs for every source asset and folder. Unity creates generated assets' metas itself.
    for path in sorted((ROOT/'Assets').rglob('*')):
        if path.suffix=='.meta' or 'Generated' in path.relative_to(ROOT/'Assets').parts: continue
        guid=hashlib.md5(('orbit-breaker/'+path.relative_to(ROOT).as_posix()).encode()).hexdigest()
        if path.is_dir():
            body='folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
        elif path.suffix=='.cs':
            body='MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
        elif path.suffix=='.png':
            body='TextureImporter:\n  externalObjects: {}\n  serializedVersion: 12\n  mipmaps:\n    enableMipMap: 0\n  isReadable: 0\n  textureSettings:\n    serializedVersion: 2\n    filterMode: 1\n    aniso: 1\n    mipBias: 0\n    wrapU: 1\n    wrapV: 1\n    wrapW: 1\n  maxTextureSize: 1024\n  textureFormat: 1\n  textureCompression: 0\n  spriteMode: 1\n  spritePixelsToUnits: 128\n  spriteMeshType: 0\n  spriteExtrude: 1\n  alignment: 0\n  spritePivot: {x: 0.5, y: 0.5}\n  alphaIsTransparency: 1\n  textureType: 8\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
        elif path.suffix=='.wav':
            body='AudioImporter:\n  externalObjects: {}\n  serializedVersion: 7\n  defaultSettings:\n    serializedVersion: 2\n    loadType: 0\n    sampleRateSetting: 0\n    sampleRateOverride: 44100\n    compressionFormat: 0\n    quality: 1\n    conversionMode: 0\n  forceToMono: 1\n  normalize: 0\n  preloadAudioData: 1\n  loadInBackground: 0\n  ambisonic: 0\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
        else: body='DefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
        body = '\n'.join(line.rstrip() for line in body.splitlines()) + '\n'
        Path(str(path)+'.meta').write_text(f'fileFormatVersion: 2\nguid: {guid}\n'+body)

if __name__=='__main__':
    sprites(); sounds(); metas()
    print('Generated sprite PNGs, PCM WAVs, and stable source metadata.')
