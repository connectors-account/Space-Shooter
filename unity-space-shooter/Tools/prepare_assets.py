"""Offline asset preparation. Requires Python 3 + Pillow + numpy; not needed to play."""
from pathlib import Path
from PIL import Image
import numpy as np
import wave

ROOT = Path(__file__).resolve().parents[1]
art = ROOT / 'Assets/Art'
atlas = Image.open(art / 'OriginalAtlas.png').convert('RGBA')
# Fixed atlas quadrants; discard faint generation residue, preserving solid silhouettes.
for name, box in [('Player',(0,0,512,500)), ('Enemy',(512,0,1024,500)),
                  ('Repair',(0,512,512,1024)), ('Weapon',(512,512,1024,1024))]:
    im = atlas.crop(box)
    a = np.array(im)
    a[a[:,:,3] < 100] = 0
    im = Image.fromarray(a)
    bounds = im.getbbox()
    assert bounds, name
    im = im.crop(bounds)
    im.thumbnail((240,240), Image.Resampling.LANCZOS)
    out = Image.new('RGBA',(256,256))
    out.alpha_composite(im, ((256-im.width)//2,(256-im.height)//2))
    out.save(art / (name+'.png'))
# Mathematical light kernels, not illustrated artwork: used for shots, stars and sparks.
y,x = np.mgrid[-1:1:64j,-1:1:64j]
a = np.clip(1-np.sqrt(x*x+y*y),0,1)**1.4
rgba = np.full((64,64,4),255,dtype=np.uint8)
rgba[:,:,3] = (a*255).astype(np.uint8)
Image.fromarray(rgba).save(art/'Glow.png')

rng = np.random.default_rng(417)
sr = 44100

def sound(name, seconds, make):
    t = np.arange(int(sr*seconds))/sr
    samples = make(t)
    samples *= np.minimum(t/.006,1)*np.minimum((seconds-t)/.025,1)
    samples = np.clip(samples,-1,1)
    with wave.open(str(ROOT/'Assets/Audio'/f'{name}.wav'),'wb') as f:
        f.setnchannels(1); f.setsampwidth(2); f.setframerate(sr)
        f.writeframes((samples*26000).astype('<i2').tobytes())

sound('EnemyShot', .18, lambda t: .35*np.sin(2*np.pi*(550*t-700*t*t))*np.exp(-15*t))
sound('Explosion', .48, lambda t: (.65*rng.uniform(-1,1,len(t))+.25*np.sin(2*np.pi*65*t))*np.exp(-9*t))
sound('Hit', .22, lambda t: .5*np.sin(2*np.pi*(160*t-160*t*t))*np.exp(-12*t))
sound('Pickup', .38, lambda t: .4*np.sin(2*np.pi*(600*t+1000*t*t))*np.exp(-6*t))
sound('UI', .10, lambda t: .25*np.sin(2*np.pi*880*t)*np.exp(-25*t))
sound('GameOver', 1.0, lambda t: .35*np.sin(2*np.pi*(300*t-100*t*t))*np.exp(-3*t))
sound('Wave', .65, lambda t: .30*(np.sin(2*np.pi*440*t)+.5*np.sin(2*np.pi*660*t))*np.sin(np.pi*t/.65)**2)
print('Prepared four illustrated sprites, one light kernel, and seven PCM sound effects.')
