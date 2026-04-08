#!/usr/bin/env python3
"""
Standalone Sprite Generator for Space Shooter Game.
Generates simple colored PNG sprites without requiring Unity.
Requires: pip install Pillow

Usage: python generate_sprites.py
Output: All sprites saved to ../Assets/Sprites/
"""

import os
import math
import random

try:
    from PIL import Image, ImageDraw
except ImportError:
    print("Pillow not installed. Install with: pip install Pillow")
    exit(1)

OUTPUT_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Assets", "Sprites")


def ensure_output_dir():
    os.makedirs(OUTPUT_DIR, exist_ok=True)


def create_player_ship():
    """Blue arrow-shaped player ship."""
    size = 64
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Main body triangle (pointing up)
    points = [
        (size // 2, 4),        # Top tip
        (8, size - 8),         # Bottom left
        (size - 8, size - 8),  # Bottom right
    ]
    draw.polygon(points, fill=(50, 120, 255, 255))

    # Cockpit highlight
    cockpit = [
        (size // 2, 12),
        (size // 2 - 5, 30),
        (size // 2 + 5, 30),
    ]
    draw.polygon(cockpit, fill=(100, 200, 255, 255))

    # Wing accents
    draw.polygon([(4, size - 12), (18, size - 20), (14, size - 6)], fill=(30, 80, 200, 255))
    draw.polygon([(size - 4, size - 12), (size - 18, size - 20), (size - 14, size - 6)], fill=(30, 80, 200, 255))

    # Engine glow
    draw.ellipse([size // 2 - 4, size - 12, size // 2 + 4, size - 4], fill=(100, 200, 255, 200))

    img.save(os.path.join(OUTPUT_DIR, "PlayerShip.png"))
    print("  Created PlayerShip.png")


def create_enemy_straight():
    """Red inverted triangle enemy."""
    size = 48
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    points = [
        (6, 6),
        (size - 6, 6),
        (size // 2, size - 6),
    ]
    draw.polygon(points, fill=(255, 70, 70, 255))
    draw.polygon([(size // 2 - 4, 10), (size // 2 + 4, 10), (size // 2, 28)], fill=(255, 150, 150, 255))

    img.save(os.path.join(OUTPUT_DIR, "EnemyStraight.png"))
    print("  Created EnemyStraight.png")


def create_enemy_zigzag():
    """Orange diamond enemy."""
    size = 48
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    c = size // 2
    r = 18
    points = [(c, c - r), (c + r, c), (c, c + r), (c - r, c)]
    draw.polygon(points, fill=(255, 150, 30, 255))
    draw.polygon([(c, c - 8), (c + 8, c), (c, c + 8), (c - 8, c)], fill=(255, 200, 100, 255))

    img.save(os.path.join(OUTPUT_DIR, "EnemyZigzag.png"))
    print("  Created EnemyZigzag.png")


def create_enemy_swooper():
    """Purple crescent swooper enemy."""
    size = 48
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Outer arc
    draw.ellipse([6, 6, size - 6, size - 6], fill=(200, 50, 200, 255))
    # Inner cutout (makes crescent)
    draw.ellipse([12, 2, size - 12, size - 16], fill=(0, 0, 0, 0))
    # Center dot
    draw.ellipse([size // 2 - 6, size // 2 - 2, size // 2 + 6, size // 2 + 10], fill=(255, 130, 255, 255))

    img.save(os.path.join(OUTPUT_DIR, "EnemySwooper.png"))
    print("  Created EnemySwooper.png")


def create_enemy_tank():
    """Gray/red heavy tank enemy."""
    size = 56
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Main body rectangle
    draw.rounded_rectangle([6, 8, size - 6, size - 8], radius=4, fill=(120, 120, 120, 255))
    # Armor plating
    draw.rounded_rectangle([12, 14, size - 12, size - 14], radius=2, fill=(180, 50, 50, 255))
    # Cannon
    draw.rectangle([size // 2 - 3, size - 12, size // 2 + 3, size + 2], fill=(80, 80, 80, 255))

    img.save(os.path.join(OUTPUT_DIR, "EnemyTank.png"))
    print("  Created EnemyTank.png")


def create_bullet(name, color):
    """Small elongated bullet."""
    w, h = 8, 16
    img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    draw.rounded_rectangle([1, 1, w - 1, h - 1], radius=3, fill=color)
    # Bright center
    core_color = tuple(min(255, c + 80) for c in color[:3]) + (255,)
    draw.rounded_rectangle([2, 3, w - 2, h - 3], radius=2, fill=core_color)

    img.save(os.path.join(OUTPUT_DIR, f"{name}.png"))
    print(f"  Created {name}.png")


def create_powerup(name, main_color, symbol_func):
    """Power-up with circle background and symbol."""
    size = 32
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Circle background
    dark_color = tuple(c // 3 for c in main_color[:3]) + (220,)
    draw.ellipse([2, 2, size - 2, size - 2], fill=dark_color)

    # Bright border
    draw.ellipse([2, 2, size - 2, size - 2], outline=main_color, width=2)

    # Symbol
    symbol_func(draw, size, main_color)

    img.save(os.path.join(OUTPUT_DIR, f"{name}.png"))
    print(f"  Created {name}.png")


def health_symbol(draw, size, color):
    c = size // 2
    # Plus sign
    draw.rectangle([c - 2, c - 8, c + 2, c + 8], fill=color)
    draw.rectangle([c - 8, c - 2, c + 8, c + 2], fill=color)


def rapid_fire_symbol(draw, size, color):
    c = size // 2
    # Upward arrows
    draw.polygon([(c, c - 8), (c - 6, c - 2), (c + 6, c - 2)], fill=color)
    draw.rectangle([c - 2, c - 2, c + 2, c + 6], fill=color)


def shield_symbol(draw, size, color):
    c = size // 2
    # Shield shape
    draw.ellipse([c - 8, c - 8, c + 8, c + 8], outline=color, width=2)
    draw.ellipse([c - 4, c - 4, c + 4, c + 4], fill=color)


def create_shield_bubble():
    """Translucent shield bubble overlay."""
    size = 80
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Semi-transparent blue circle
    draw.ellipse([4, 4, size - 4, size - 4], outline=(80, 160, 255, 150), width=3)
    draw.ellipse([8, 8, size - 8, size - 8], fill=(80, 160, 255, 25))

    img.save(os.path.join(OUTPUT_DIR, "ShieldBubble.png"))
    print("  Created ShieldBubble.png")


def create_star_background():
    """Starfield background."""
    width, height = 512, 1024
    img = Image.new('RGBA', (width, height), (5, 5, 15, 255))
    draw = ImageDraw.Draw(img)

    random.seed(42)  # Reproducible

    for _ in range(300):
        x = random.randint(0, width - 1)
        y = random.randint(0, height - 1)
        brightness = random.randint(128, 255)
        star_size = random.randint(0, 2)

        roll = random.random()
        if roll < 0.7:
            color = (brightness, brightness, brightness, 255)
        elif roll < 0.85:
            color = (brightness, int(brightness * 0.8), int(brightness * 0.5), 255)
        else:
            color = (int(brightness * 0.5), int(brightness * 0.7), brightness, 255)

        if star_size == 0:
            img.putpixel((x, y), color)
        else:
            draw.ellipse([x - star_size, y - star_size, x + star_size, y + star_size], fill=color)

    img.save(os.path.join(OUTPUT_DIR, "StarBackground.png"))
    print("  Created StarBackground.png")


def main():
    print("=== Space Shooter Sprite Generator ===")
    ensure_output_dir()

    print("\nGenerating sprites...")
    create_player_ship()
    create_enemy_straight()
    create_enemy_zigzag()
    create_enemy_swooper()
    create_enemy_tank()
    create_bullet("PlayerBullet", (100, 255, 100, 255))
    create_bullet("EnemyBullet", (255, 80, 50, 255))
    create_powerup("PowerUpHealth", (50, 255, 80, 255), health_symbol)
    create_powerup("PowerUpRapidFire", (255, 255, 50, 255), rapid_fire_symbol)
    create_powerup("PowerUpShield", (80, 160, 255, 255), shield_symbol)
    create_shield_bubble()
    create_star_background()

    print(f"\nAll sprites saved to: {os.path.abspath(OUTPUT_DIR)}")
    print("Done!")


if __name__ == "__main__":
    main()
