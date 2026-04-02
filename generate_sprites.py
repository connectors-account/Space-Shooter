#!/usr/bin/env python3
"""
Generate basic sprite assets for the Space Shooter game.
Creates simple geometric shapes as PNG files.
"""
from PIL import Image, ImageDraw
import os

SPRITES_DIR = os.path.join(os.path.dirname(__file__), "Assets", "Resources", "Sprites")
os.makedirs(SPRITES_DIR, exist_ok=True)

def save(img, name):
    path = os.path.join(SPRITES_DIR, name)
    img.save(path)
    print(f"  Created: {name}")

def create_player_ship():
    """Triangle-based player ship - cyan/blue"""
    size = 64
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # Ship body (triangle)
    points = [(32, 4), (8, 56), (56, 56)]
    draw.polygon(points, fill=(0, 200, 255, 255))
    # Cockpit
    draw.ellipse([24, 20, 40, 36], fill=(150, 230, 255, 255))
    # Wings
    draw.polygon([(8, 56), (0, 60), (16, 44)], fill=(0, 150, 200, 255))
    draw.polygon([(56, 56), (63, 60), (48, 44)], fill=(0, 150, 200, 255))
    # Engine glow
    draw.rectangle([26, 52, 38, 62], fill=(255, 150, 0, 200))
    save(img, "player_ship.png")

def create_enemy_basic():
    """Red diamond enemy"""
    size = 48
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    points = [(24, 4), (44, 24), (24, 44), (4, 24)]
    draw.polygon(points, fill=(220, 50, 50, 255))
    draw.polygon(points, outline=(255, 100, 100, 255), width=2)
    draw.ellipse([18, 18, 30, 30], fill=(255, 150, 150, 255))
    save(img, "enemy_basic.png")

def create_enemy_zigzag():
    """Yellow angular enemy"""
    size = 48
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    points = [(24, 2), (46, 18), (38, 46), (10, 46), (2, 18)]
    draw.polygon(points, fill=(220, 180, 0, 255))
    draw.polygon(points, outline=(255, 220, 50, 255), width=2)
    draw.ellipse([16, 14, 32, 30], fill=(255, 240, 100, 255))
    save(img, "enemy_zigzag.png")

def create_enemy_bomber():
    """Purple large enemy"""
    size = 56
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([4, 8, 52, 48], radius=8, fill=(140, 40, 180, 255))
    draw.rounded_rectangle([4, 8, 52, 48], radius=8, outline=(180, 80, 220, 255), width=2)
    draw.ellipse([18, 16, 38, 36], fill=(200, 120, 255, 255))
    # Wings
    draw.polygon([(4, 20), (0, 12), (0, 28)], fill=(120, 30, 160, 255))
    draw.polygon([(52, 20), (56, 12), (56, 28)], fill=(120, 30, 160, 255))
    save(img, "enemy_bomber.png")

def create_enemy_elite():
    """Green angular boss-like enemy"""
    size = 56
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    points = [(28, 2), (52, 16), (48, 44), (28, 54), (8, 44), (4, 16)]
    draw.polygon(points, fill=(30, 180, 60, 255))
    draw.polygon(points, outline=(80, 255, 100, 255), width=2)
    draw.ellipse([16, 14, 40, 38], fill=(50, 220, 80, 255))
    draw.ellipse([22, 20, 34, 32], fill=(100, 255, 130, 255))
    save(img, "enemy_elite.png")

def create_bullet_player():
    """Small green bullet"""
    img = Image.new("RGBA", (8, 16), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([1, 1, 7, 15], radius=3, fill=(50, 255, 100, 255))
    draw.rounded_rectangle([2, 3, 6, 13], radius=2, fill=(150, 255, 180, 200))
    save(img, "bullet_player.png")

def create_bullet_enemy():
    """Small red bullet"""
    img = Image.new("RGBA", (8, 16), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([1, 1, 7, 15], radius=3, fill=(255, 60, 40, 255))
    draw.rounded_rectangle([2, 3, 6, 13], radius=2, fill=(255, 150, 130, 200))
    save(img, "bullet_enemy.png")

def create_powerup(name, color):
    """Generic power-up icon"""
    size = 32
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.ellipse([2, 2, 30, 30], fill=color)
    draw.ellipse([2, 2, 30, 30], outline=(255, 255, 255, 200), width=2)
    draw.ellipse([8, 8, 24, 24], fill=(255, 255, 255, 100))
    save(img, f"powerup_{name}.png")

def create_explosion():
    """Orange explosion sprite"""
    size = 48
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.ellipse([4, 4, 44, 44], fill=(255, 150, 30, 200))
    draw.ellipse([8, 8, 40, 40], fill=(255, 200, 50, 220))
    draw.ellipse([14, 14, 34, 34], fill=(255, 240, 150, 240))
    draw.ellipse([18, 18, 30, 30], fill=(255, 255, 220, 255))
    save(img, "explosion.png")

def create_shield():
    """Blue shield circle"""
    size = 80
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.ellipse([2, 2, 78, 78], outline=(80, 180, 255, 180), width=3)
    draw.ellipse([6, 6, 74, 74], outline=(120, 200, 255, 100), width=2)
    save(img, "shield.png")

def create_background_star():
    """Tiny star dot"""
    img = Image.new("RGBA", (4, 4), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.rectangle([0, 0, 3, 3], fill=(255, 255, 255, 255))
    save(img, "star.png")

def create_background():
    """Dark space background"""
    w, h = 512, 512
    img = Image.new("RGBA", (w, h), (5, 5, 20, 255))
    draw = ImageDraw.Draw(img)
    import random
    random.seed(42)
    for _ in range(200):
        x = random.randint(0, w-1)
        y = random.randint(0, h-1)
        brightness = random.randint(80, 255)
        size = random.choice([1, 1, 1, 2])
        draw.ellipse([x, y, x+size, y+size], fill=(brightness, brightness, brightness+random.randint(0,20), brightness))
    save(img, "background.png")

def create_ui_button():
    """Basic button background"""
    img = Image.new("RGBA", (200, 50), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([0, 0, 199, 49], radius=10, fill=(40, 80, 160, 220))
    draw.rounded_rectangle([0, 0, 199, 49], radius=10, outline=(80, 140, 255, 255), width=2)
    save(img, "button_bg.png")

if __name__ == "__main__":
    print("Generating sprite assets...")
    create_player_ship()
    create_enemy_basic()
    create_enemy_zigzag()
    create_enemy_bomber()
    create_enemy_elite()
    create_bullet_player()
    create_bullet_enemy()
    create_powerup("weapon", (255, 140, 0, 255))
    create_powerup("shield", (60, 160, 255, 255))
    create_powerup("health", (0, 230, 70, 255))
    create_powerup("speed", (255, 255, 0, 255))
    create_powerup("score", (255, 50, 255, 255))
    create_explosion()
    create_shield()
    create_background_star()
    create_background()
    create_ui_button()
    print(f"\nAll sprites generated in: {SPRITES_DIR}")
