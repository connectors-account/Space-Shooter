"""Rebuild bundled sprites and original synthesized audio. Requires Python 3 + Pillow."""
from pathlib import Path
import math, random, struct, wave
from PIL import Image, ImageDraw
ROOT = Path(__file__).resolve().parents[1] / 'Assets/Starfall'
source = Image.open(ROOT / 'Art/atlas.png').convert('RGBA')
# Atlas cells are cropped to their actual silhouettes rather than assumed grid spacing.
rects = {'player': (90,100,530,552), 'scout': (620,150,965,530),
         'fighter': (1060,120,1420,540), 'gunship': (1470,80,2030,540),
         'boss': (0,570,610,1100), 'repair': (700,660,910,1020),
         'spread': (1160,660,1380,1020), 'shield': (1630,660,1840,1020)}
for name, box in rects.items():
    im = source.crop(box)
    # Remove faint atlas background artifacts; preserve opaque painted silhouette.
    alpha = im.getchannel('A').point(lambda a: a if a >= 180 else 0)
    im.putalpha(alpha)
    bounds = alpha.getbbox()
    assert bounds, name
    im = im.crop(bounds)
    im.thumbnail((256,256), Image.Resampling.LANCZOS)
    out = Image.new('RGBA',(im.width+8, im.height+8))
    out.alpha_composite(im,(4,4))
    out.save(ROOT / f'Art/{name}.png')
# Neutral geometric rendering primitives, tinted by the game (not illustrated subjects).
for name, size in [('particle',16),('bolt',32)]:
    im=Image.new('RGBA',(size,size))
    d=ImageDraw.Draw(im)
    if name=='bolt': d.rounded_rectangle((11,1,21,30),radius=5,fill='white')
    else: d.ellipse((1,1,size-2,size-2),fill='white')
    im.save(ROOT / f'Art/{name}.png')
rng=random.Random(19)
RATE=22050

def sound(name, duration, frequency, end, noise=0, chord=False):
    samples=[]; phase=0
    for i in range(int(RATE*duration)):
        t=i/RATE; k=t/duration
        phase += 2*math.pi*(frequency+(end-frequency)*k)/RATE
        env=min(1,t/.008)*(1-k)**2
        tone=math.sin(phase)
        if chord: tone=(tone+0.6*math.sin(phase*1.25)+0.5*math.sin(phase*1.5))/2.1
        value=env*((1-noise)*tone+noise*rng.uniform(-1,1))*.55
        samples.append(struct.pack('<h',int(max(-1,min(1,value))*32767)))
    with wave.open(str(ROOT / f'Audio/{name}.wav'),'wb') as f:
        f.setnchannels(1); f.setsampwidth(2); f.setframerate(RATE); f.writeframes(b''.join(samples))
for args in [('fire',.10,1000,400,.08),('enemy',.15,360,190,.08),
             ('hit',.19,140,55,.6),('explosion',.42,100,25,.85),
             ('pickup',.38,520,1040,0,True),('wave',.65,220,660,0,True),
             ('win',1.4,440,880,0,True),('lose',.9,280,70,.1,True),
             ('ui',.08,640,820,0)]: sound(*args)
# A quiet periodic space-drone loop; exact integer cycles avoid a seam.
with wave.open(str(ROOT/'Audio/ambient.wav'),'wb') as f:
    f.setnchannels(1); f.setsampwidth(2); f.setframerate(RATE)
    f.writeframes(b''.join(struct.pack('<h',int(32767*.08*(math.sin(2*math.pi*55*i/RATE)+.35*math.sin(2*math.pi*82.5*i/RATE))*(.8+.2*math.cos(2*math.pi*i/RATE/4)))) for i in range(RATE*4)))
print('Prepared 10 sprites and 10 PCM WAV clips.')
