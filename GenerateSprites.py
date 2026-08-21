#!/usr/bin/env python3
"""
GenerateSprites.py
Generates all PNG sprites for the Space Shooter Unity project using Pillow.

Usage:
    pip install Pillow
    python GenerateSprites.py

All sprites are written to Assets/Sprites/. Re-running overwrites existing files.
"""

import os
import math
import random

try:
    from PIL import Image, ImageDraw
except ImportError:
    raise SystemExit("Pillow is required. Install with: pip install Pillow")

# Output directory relative to this script.
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
OUT_DIR = os.path.join(SCRIPT_DIR, "Assets", "Sprites")
os.makedirs(OUT_DIR, exist_ok=True)

random.seed(1337)


def save(img, name):
    path = os.path.join(OUT_DIR, name)
    img.save(path)
    print(f"  wrote {name} ({img.size[0]}x{img.size[1]})")


def new_canvas(w, h):
    return Image.new("RGBA", (w, h), (0, 0, 0, 0))


def player_ship():
    w = h = 64
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    cyan = (0, 220, 255, 255)
    dark = (0, 120, 160, 255)
    # Main arrow/triangle pointing up.
    d.polygon([(32, 4), (58, 56), (32, 44), (6, 56)], fill=cyan, outline=(200, 255, 255, 255))
    # Cockpit.
    d.ellipse([26, 20, 38, 36], fill=(230, 255, 255, 255))
    # Wings.
    d.polygon([(6, 56), (2, 44), (20, 48)], fill=dark)
    d.polygon([(58, 56), (62, 44), (44, 48)], fill=dark)
    # Engine glow.
    for i, a in enumerate([180, 120, 60]):
        r = 6 - i * 2
        d.ellipse([32 - r, 52 - r + 8, 32 + r, 52 + r + 8], fill=(255, 160, 40, a))
    save(img, "player_ship.png")


def enemy_a():
    w = h = 48
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    red = (220, 40, 40, 255)
    # Diamond.
    d.polygon([(24, 2), (46, 24), (24, 46), (2, 24)], fill=red, outline=(255, 150, 150, 255))
    d.ellipse([18, 18, 30, 30], fill=(255, 200, 80, 255))
    save(img, "enemy_a.png")


def enemy_b():
    w = h = 48
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    orange = (255, 140, 20, 255)
    # Angular fighter (pointing down toward player).
    d.polygon([(24, 46), (46, 8), (24, 20), (2, 8)], fill=orange, outline=(255, 210, 120, 255))
    d.polygon([(24, 20), (34, 30), (24, 40), (14, 30)], fill=(120, 40, 0, 255))
    save(img, "enemy_b.png")


def enemy_boss():
    w = h = 128
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    purple = (150, 40, 200, 255)
    dark = (80, 10, 120, 255)
    # Large menacing hull.
    d.polygon([(64, 118), (124, 30), (90, 44), (64, 14), (38, 44), (4, 30)], fill=purple,
              outline=(220, 150, 255, 255))
    # Side pods.
    d.ellipse([6, 30, 34, 70], fill=dark)
    d.ellipse([94, 30, 122, 70], fill=dark)
    # Core.
    d.ellipse([48, 40, 80, 72], fill=(255, 60, 60, 255))
    d.ellipse([56, 48, 72, 64], fill=(255, 200, 120, 255))
    save(img, "enemy_boss.png")


def bullet(name, color):
    w, h = 8, 16
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    d.ellipse([1, 1, 7, 15], fill=color)
    d.ellipse([2, 3, 6, 9], fill=(255, 255, 255, 220))
    save(img, name)


def explosion_sheet():
    # 4 frames horizontally, each 64x64 => 256x64.
    fw = fh = 64
    frames = 4
    img = new_canvas(fw * frames, fh)
    d = ImageDraw.Draw(img)
    palette = [
        (255, 240, 120, 255),
        (255, 170, 40, 255),
        (255, 90, 20, 220),
        (120, 40, 10, 160),
    ]
    for f in range(frames):
        cx = f * fw + fw // 2
        cy = fh // 2
        base_r = 8 + f * 8
        color = palette[f]
        d.ellipse([cx - base_r, cy - base_r, cx + base_r, cy + base_r], fill=color)
        # Debris sparks.
        for _ in range(6 + f * 2):
            ang = random.uniform(0, 2 * math.pi)
            dist = random.uniform(0, base_r + 4)
            px = cx + math.cos(ang) * dist
            py = cy + math.sin(ang) * dist
            r = random.uniform(1, 3)
            d.ellipse([px - r, py - r, px + r, py + r], fill=(255, 220, 150, 200))
    save(img, "explosion_sheet.png")


def powerup_speed():
    img = new_canvas(32, 32)
    d = ImageDraw.Draw(img)
    d.ellipse([1, 1, 31, 31], fill=(60, 60, 0, 180), outline=(255, 240, 60, 255))
    d.polygon([(18, 4), (8, 18), (15, 18), (12, 28), (24, 12), (17, 12)], fill=(255, 240, 60, 255))
    save(img, "powerup_speed.png")


def powerup_rapid():
    img = new_canvas(32, 32)
    d = ImageDraw.Draw(img)
    d.ellipse([1, 1, 31, 31], fill=(60, 30, 0, 180), outline=(255, 150, 40, 255))
    for off in (-5, 5):
        d.polygon([(16 + off, 6), (24 + off, 16), (16 + off, 16), (16 + off, 26), (8 + off, 16), (16 + off, 16)],
                  fill=(255, 150, 40, 255))
    save(img, "powerup_rapid.png")


def powerup_triple():
    img = new_canvas(32, 32)
    d = ImageDraw.Draw(img)
    d.ellipse([1, 1, 31, 31], fill=(0, 60, 20, 180), outline=(60, 230, 90, 255))
    for off in (-8, 0, 8):
        d.polygon([(16 + off, 8), (20 + off, 16), (12 + off, 16)], fill=(60, 230, 90, 255))
        d.rectangle([14 + off, 16, 18 + off, 24], fill=(60, 230, 90, 255))
    save(img, "powerup_triple.png")


def powerup_shield():
    img = new_canvas(32, 32)
    d = ImageDraw.Draw(img)
    d.ellipse([1, 1, 31, 31], fill=(0, 20, 60, 180), outline=(80, 160, 255, 255))
    d.ellipse([8, 8, 24, 24], outline=(120, 200, 255, 255), width=3)
    d.ellipse([13, 13, 19, 19], fill=(180, 220, 255, 255))
    save(img, "powerup_shield.png")


def powerup_health():
    img = new_canvas(32, 32)
    d = ImageDraw.Draw(img)
    d.ellipse([1, 1, 31, 31], fill=(60, 0, 10, 180), outline=(255, 70, 90, 255))
    d.rectangle([13, 7, 19, 25], fill=(255, 70, 90, 255))
    d.rectangle([7, 13, 25, 19], fill=(255, 70, 90, 255))
    save(img, "powerup_health.png")


def powerup_bomb():
    img = new_canvas(32, 32)
    d = ImageDraw.Draw(img)
    d.ellipse([1, 1, 31, 31], fill=(30, 0, 50, 180), outline=(180, 80, 230, 255))
    cx = cy = 16
    pts = []
    for i in range(10):
        ang = math.pi / 2 + i * math.pi / 5
        r = 12 if i % 2 == 0 else 5
        pts.append((cx + math.cos(ang) * r, cy - math.sin(ang) * r))
    d.polygon(pts, fill=(200, 120, 255, 255))
    save(img, "powerup_bomb.png")


def star_small():
    img = new_canvas(4, 4)
    d = ImageDraw.Draw(img)
    d.ellipse([0, 0, 3, 3], fill=(255, 255, 255, 255))
    save(img, "star_small.png")


def star_large():
    img = new_canvas(8, 8)
    d = ImageDraw.Draw(img)
    d.line([4, 0, 4, 7], fill=(255, 255, 255, 255))
    d.line([0, 4, 7, 4], fill=(255, 255, 255, 255))
    d.ellipse([2, 2, 5, 5], fill=(255, 255, 255, 255))
    save(img, "star_large.png")


def background_nebula():
    w = h = 256
    img = new_canvas(w, h)
    px = img.load()
    for y in range(h):
        for x in range(w):
            # Diagonal gradient purple -> blue.
            t = (x + y) / (w + h)
            r = int(40 + 60 * (1 - t))
            g = int(10 + 30 * t)
            b = int(70 + 120 * t)
            # Soft noise.
            n = random.randint(-12, 12)
            px[x, y] = (max(0, min(255, r + n)),
                        max(0, min(255, g + n)),
                        max(0, min(255, b + n)),
                        255)
    # Sprinkle bright specks.
    d = ImageDraw.Draw(img)
    for _ in range(120):
        x = random.randint(0, w - 1)
        y = random.randint(0, h - 1)
        a = random.randint(60, 200)
        d.point((x, y), fill=(255, 255, 255, a))
    save(img, "background_nebula.png")


def ui_healthbar():
    w, h = 200, 20
    img = new_canvas(w, h)
    px = img.load()
    for x in range(w):
        t = x / w
        r = int(40 + 215 * t)
        g = int(220 - 180 * t)
        b = 40
        for y in range(h):
            px[x, y] = (r, g, b, 255)
    save(img, "ui_healthbar.png")


def ui_heart():
    w = h = 24
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    red = (230, 50, 70, 255)
    d.ellipse([3, 4, 12, 13], fill=red)
    d.ellipse([12, 4, 21, 13], fill=red)
    d.polygon([(4, 10), (20, 10), (12, 21)], fill=red)
    save(img, "ui_heart.png")


def main():
    print("Generating sprites into", OUT_DIR)
    player_ship()
    enemy_a()
    enemy_b()
    enemy_boss()
    bullet("bullet_player.png", (0, 230, 255, 255))
    bullet("bullet_enemy.png", (255, 70, 40, 255))
    explosion_sheet()
    powerup_speed()
    powerup_rapid()
    powerup_triple()
    powerup_shield()
    powerup_health()
    powerup_bomb()
    star_small()
    star_large()
    background_nebula()
    ui_healthbar()
    ui_heart()
    print("Done. All sprites generated.")


if __name__ == "__main__":
    main()
