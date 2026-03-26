#!/usr/bin/env python3
"""
Pixel Art Sprite Generator for Space Shooter Game
Generates all game sprites as PNG files with transparency.
"""
from PIL import Image, ImageDraw, ImageFont
import os
import math

BASE = "/home/ubuntu/space_shooter_game/Assets/Sprites"

def save(img, path):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    img.save(path)
    print(f"  Created: {path}")

def make_pixel_art(width, height, pixels, palette, scale=4):
    """Create a scaled pixel art image from a 2D array of palette indices."""
    img = Image.new("RGBA", (width * scale, height * scale), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    for y, row in enumerate(pixels):
        for x, idx in enumerate(row):
            if idx > 0 and idx <= len(palette):
                color = palette[idx - 1]
                draw.rectangle(
                    [x * scale, y * scale, (x + 1) * scale - 1, (y + 1) * scale - 1],
                    fill=color
                )
    return img

# ============================================================================
# COLOR PALETTES
# ============================================================================
# Player ship: cyan/blue tones
P_BODY = (80, 200, 255, 255)
P_DARK = (40, 120, 200, 255)
P_LIGHT = (180, 230, 255, 255)
P_ENGINE = (255, 180, 50, 255)
P_ENGINE_HOT = (255, 100, 30, 255)
P_COCKPIT = (200, 255, 200, 255)
P_WHITE = (255, 255, 255, 255)

# Enemy basic: green
E_GREEN = (50, 200, 80, 255)
E_GREEN_D = (30, 140, 50, 255)
E_GREEN_L = (100, 255, 120, 255)
E_EYE = (255, 50, 50, 255)

# Enemy fast: red/orange
E_RED = (220, 60, 60, 255)
E_RED_D = (160, 30, 30, 255)
E_RED_L = (255, 120, 100, 255)

# Enemy tank: purple
E_PURPLE = (150, 60, 200, 255)
E_PURPLE_D = (100, 30, 150, 255)
E_PURPLE_L = (200, 120, 255, 255)

# Enemy shooter: yellow/brown
E_YELLOW = (200, 180, 50, 255)
E_YELLOW_D = (150, 130, 30, 255)
E_YELLOW_L = (255, 230, 100, 255)

# Boss: dark red/black
B_BODY = (180, 30, 30, 255)
B_DARK = (100, 10, 10, 255)
B_LIGHT = (255, 80, 80, 255)
B_GOLD = (255, 200, 50, 255)

# Bullets
BULLET_CYAN = (100, 255, 255, 255)
BULLET_RED = (255, 80, 80, 255)
BULLET_YELLOW = (255, 255, 100, 255)

# PowerUps
PU_GREEN = (50, 255, 100, 255)
PU_BLUE = (80, 150, 255, 255)
PU_RED = (255, 80, 80, 255)
PU_GOLD = (255, 220, 50, 255)
PU_PURPLE = (200, 100, 255, 255)
PU_WHITE = (255, 255, 255, 255)
PU_ORANGE = (255, 160, 50, 255)

print("=== Generating Pixel Art Sprites ===\n")

# ============================================================================
# PLAYER SHIP (16x16 pixels, scaled 4x = 64x64 output)
# ============================================================================
print("[Player Ship]")
# 0=transparent, 1=body, 2=dark, 3=light, 4=engine, 5=engine_hot, 6=cockpit, 7=white
player_pixels = [
    [0,0,0,0,0,0,0,7,7,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,3,1,1,3,0,0,0,0,0,0],
    [0,0,0,0,0,3,1,6,6,1,3,0,0,0,0,0],
    [0,0,0,0,0,1,1,6,6,1,1,0,0,0,0,0],
    [0,0,0,0,2,1,1,1,1,1,1,2,0,0,0,0],
    [0,0,0,2,1,1,3,1,1,3,1,1,2,0,0,0],
    [0,0,2,1,1,1,1,1,1,1,1,1,1,2,0,0],
    [0,2,1,1,3,1,1,1,1,1,1,3,1,1,2,0],
    [0,1,1,1,1,1,1,2,2,1,1,1,1,1,1,0],
    [7,1,3,1,1,1,2,2,2,2,1,1,1,3,1,7],
    [0,2,1,1,1,2,2,1,1,2,2,1,1,1,2,0],
    [0,0,2,1,2,0,2,1,1,2,0,2,1,2,0,0],
    [0,0,0,2,0,0,2,1,1,2,0,0,2,0,0,0],
    [0,0,0,0,0,0,4,5,5,4,0,0,0,0,0,0],
    [0,0,0,0,0,0,5,4,4,5,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,5,5,0,0,0,0,0,0,0],
]
player_palette = [P_BODY, P_DARK, P_LIGHT, P_ENGINE, P_ENGINE_HOT, P_COCKPIT, P_WHITE]
player_img = make_pixel_art(16, 16, player_pixels, player_palette, 4)
save(player_img, f"{BASE}/Player/player_ship.png")

# Player with shield effect
shield_img = player_img.copy()
draw = ImageDraw.Draw(shield_img)
# Draw a semi-transparent blue circle around the ship
for r in range(28, 34):
    for angle in range(360):
        x = 32 + int(r * math.cos(math.radians(angle)))
        y = 32 + int(r * math.sin(math.radians(angle)))
        if 0 <= x < 64 and 0 <= y < 64:
            shield_img.putpixel((x, y), (100, 180, 255, 120))
save(shield_img, f"{BASE}/Player/player_shield.png")

# ============================================================================
# ENEMY - BASIC (12x12, green alien)
# ============================================================================
print("[Enemy Basic]")
basic_pixels = [
    [0,0,0,0,1,1,1,1,0,0,0,0],
    [0,0,0,1,2,1,1,2,1,0,0,0],
    [0,0,1,1,1,3,3,1,1,1,0,0],
    [0,1,1,1,3,3,3,3,1,1,1,0],
    [0,1,3,1,4,1,1,4,1,3,1,0],
    [1,1,1,1,1,1,1,1,1,1,1,1],
    [1,2,1,3,1,1,1,1,3,1,2,1],
    [1,1,1,1,1,2,2,1,1,1,1,1],
    [0,1,2,1,1,1,1,1,1,2,1,0],
    [0,0,1,1,2,1,1,2,1,1,0,0],
    [0,0,0,1,0,1,1,0,1,0,0,0],
    [0,0,0,0,0,1,1,0,0,0,0,0],
]
basic_palette = [E_GREEN, E_GREEN_D, E_GREEN_L, E_EYE]
basic_img = make_pixel_art(12, 12, basic_pixels, basic_palette, 4)
save(basic_img, f"{BASE}/Enemies/enemy_basic.png")

# ============================================================================
# ENEMY - FAST (10x10, red dart shape)
# ============================================================================
print("[Enemy Fast]")
fast_pixels = [
    [0,0,0,0,1,1,0,0,0,0],
    [0,0,0,1,2,2,1,0,0,0],
    [0,0,1,1,3,3,1,1,0,0],
    [0,1,2,1,1,1,1,2,1,0],
    [1,1,1,3,1,1,3,1,1,1],
    [1,2,1,1,4,4,1,1,2,1],
    [0,1,2,1,1,1,1,2,1,0],
    [0,0,1,2,1,1,2,1,0,0],
    [0,0,0,1,2,2,1,0,0,0],
    [0,0,0,0,1,1,0,0,0,0],
]
fast_palette = [E_RED, E_RED_D, E_RED_L, E_EYE]
fast_img = make_pixel_art(10, 10, fast_pixels, fast_palette, 4)
save(fast_img, f"{BASE}/Enemies/enemy_fast.png")

# ============================================================================
# ENEMY - TANK (14x14, purple heavy)
# ============================================================================
print("[Enemy Tank]")
tank_pixels = [
    [0,0,0,0,0,1,1,1,1,0,0,0,0,0],
    [0,0,0,0,1,2,1,1,2,1,0,0,0,0],
    [0,0,0,1,1,1,3,3,1,1,1,0,0,0],
    [0,0,1,2,1,3,3,3,3,1,2,1,0,0],
    [0,1,2,1,1,1,1,1,1,1,1,2,1,0],
    [1,1,1,1,4,1,1,1,1,4,1,1,1,1],
    [1,2,3,1,1,1,3,3,1,1,1,3,2,1],
    [1,1,1,1,1,3,3,3,3,1,1,1,1,1],
    [1,2,3,1,1,1,3,3,1,1,1,3,2,1],
    [1,1,1,1,4,1,1,1,1,4,1,1,1,1],
    [0,1,2,1,1,1,1,1,1,1,1,2,1,0],
    [0,0,1,2,1,1,2,2,1,1,2,1,0,0],
    [0,0,0,1,1,2,1,1,2,1,1,0,0,0],
    [0,0,0,0,1,1,1,1,1,1,0,0,0,0],
]
tank_palette = [E_PURPLE, E_PURPLE_D, E_PURPLE_L, E_EYE]
tank_img = make_pixel_art(14, 14, tank_pixels, tank_palette, 4)
save(tank_img, f"{BASE}/Enemies/enemy_tank.png")

# ============================================================================
# ENEMY - SHOOTER (12x12, yellow/brown)
# ============================================================================
print("[Enemy Shooter]")
shooter_pixels = [
    [0,0,0,0,0,1,1,0,0,0,0,0],
    [0,0,0,0,1,2,2,1,0,0,0,0],
    [0,0,0,1,3,1,1,3,1,0,0,0],
    [0,0,1,1,1,3,3,1,1,1,0,0],
    [0,1,2,1,4,1,1,4,1,2,1,0],
    [1,1,1,1,1,1,1,1,1,1,1,1],
    [1,3,2,1,1,2,2,1,1,2,3,1],
    [0,1,1,1,1,1,1,1,1,1,1,0],
    [0,0,1,2,1,3,3,1,2,1,0,0],
    [0,0,2,0,1,1,1,1,0,2,0,0],
    [0,0,0,0,0,2,2,0,0,0,0,0],
    [0,0,0,0,0,1,1,0,0,0,0,0],
]
shooter_palette = [E_YELLOW, E_YELLOW_D, E_YELLOW_L, E_EYE]
shooter_img = make_pixel_art(12, 12, shooter_pixels, shooter_palette, 4)
save(shooter_img, f"{BASE}/Enemies/enemy_shooter.png")

# ============================================================================
# BOSS (24x24, big red menacing ship)
# ============================================================================
print("[Boss]")
boss_pixels = [
    [0,0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,1,2,1,1,2,1,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,1,1,1,3,3,1,1,1,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,1,2,1,3,4,4,3,1,2,1,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,1,1,1,1,1,3,3,1,1,1,1,1,0,0,0,0,0,0],
    [0,0,0,0,0,1,2,1,1,3,1,1,1,1,3,1,1,2,1,0,0,0,0,0],
    [0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0],
    [0,0,0,1,2,1,3,1,1,1,3,1,1,3,1,1,1,3,1,2,1,0,0,0],
    [0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0],
    [0,1,2,1,1,1,1,3,1,1,1,2,2,1,1,1,3,1,1,1,1,2,1,0],
    [1,1,1,1,3,1,1,1,1,3,1,1,1,1,3,1,1,1,1,3,1,1,1,1],
    [1,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,1],
    [1,1,3,1,1,1,3,1,4,1,1,2,2,1,1,4,1,3,1,1,1,3,1,1],
    [1,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,1],
    [1,1,1,1,3,1,1,1,1,3,1,1,1,1,3,1,1,1,1,3,1,1,1,1],
    [0,1,2,1,1,1,1,3,1,1,1,2,2,1,1,1,3,1,1,1,1,2,1,0],
    [0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0],
    [0,0,0,1,2,1,3,1,1,1,2,1,1,2,1,1,1,3,1,2,1,0,0,0],
    [0,0,0,0,1,1,1,1,1,2,1,1,1,1,2,1,1,1,1,1,0,0,0,0],
    [0,0,0,0,0,1,2,1,2,0,2,1,1,2,0,2,1,2,1,0,0,0,0,0],
    [0,0,0,0,0,0,1,1,0,0,0,2,2,0,0,0,1,1,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],
]
boss_palette = [B_BODY, B_DARK, B_LIGHT, B_GOLD]
boss_img = make_pixel_art(24, 24, boss_pixels, boss_palette, 4)
save(boss_img, f"{BASE}/Enemies/boss.png")

# ============================================================================
# BULLETS
# ============================================================================
print("[Bullets]")

# Player bullet (4x8, cyan)
pb_pixels = [
    [0,1,1,0],
    [1,2,2,1],
    [1,2,2,1],
    [1,1,1,1],
    [1,2,2,1],
    [1,2,2,1],
    [0,1,1,0],
    [0,0,0,0],
]
pb_palette = [BULLET_CYAN, (200, 255, 255, 255)]
save(make_pixel_art(4, 8, pb_pixels, pb_palette, 4), f"{BASE}/Bullets/player_bullet.png")

# Enemy bullet (4x6, red)
eb_pixels = [
    [0,1,1,0],
    [1,2,2,1],
    [1,2,2,1],
    [1,1,1,1],
    [0,1,1,0],
    [0,0,0,0],
]
eb_palette = [BULLET_RED, (255, 160, 160, 255)]
save(make_pixel_art(4, 6, eb_pixels, eb_palette, 4), f"{BASE}/Bullets/enemy_bullet.png")

# Laser beam (3x12, bright yellow)
laser_pixels = [
    [0,1,0],
    [1,2,1],
    [1,2,1],
    [1,2,1],
    [1,2,1],
    [1,2,1],
    [1,2,1],
    [1,2,1],
    [1,2,1],
    [1,2,1],
    [1,2,1],
    [0,1,0],
]
laser_palette = [BULLET_YELLOW, (255, 255, 200, 255)]
save(make_pixel_art(3, 12, laser_pixels, laser_palette, 4), f"{BASE}/Bullets/laser.png")

# ============================================================================
# POWER-UPS (10x10 each)
# ============================================================================
print("[Power-Ups]")

def make_powerup(color, letter, filename):
    """Make a powerup: colored diamond with a letter."""
    img = Image.new("RGBA", (40, 40), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # Diamond shape
    points = [(20, 2), (38, 20), (20, 38), (2, 20)]
    draw.polygon(points, fill=color)
    # Darker border
    border = tuple(max(0, c - 60) if i < 3 else c for i, c in enumerate(color))
    draw.polygon(points, outline=border)
    # Inner glow
    inner = [(20, 8), (32, 20), (20, 32), (8, 20)]
    inner_color = tuple(min(255, c + 60) if i < 3 else 180 for i, c in enumerate(color))
    draw.polygon(inner, fill=inner_color)
    # Letter
    try:
        font = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf", 16)
    except:
        font = ImageFont.load_default()
    bbox = draw.textbbox((0, 0), letter, font=font)
    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
    draw.text((20 - tw // 2, 20 - th // 2 - 2), letter, fill=(255, 255, 255, 255), font=font)
    save(img, f"{BASE}/PowerUps/{filename}")

make_powerup(PU_BLUE, "W", "powerup_weapon.png")
make_powerup(PU_GREEN, "S", "powerup_shield.png")
make_powerup(PU_RED, "+", "powerup_health.png")
make_powerup(PU_GOLD, "★", "powerup_speed.png")
make_powerup(PU_PURPLE, "R", "powerup_rapidfire.png")
make_powerup(PU_WHITE, "1", "powerup_extralife.png")
make_powerup(PU_ORANGE, "B", "powerup_bomb.png")

# ============================================================================
# EXPLOSION (spritesheet: 5 frames, 16x16 each)
# ============================================================================
print("[Explosion]")
explosion_colors = [
    (255, 255, 200, 255),
    (255, 200, 50, 255),
    (255, 140, 30, 255),
    (255, 80, 20, 255),
    (200, 50, 10, 200),
]

for frame_i in range(5):
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # Expanding circle with color shift
    radius = 8 + frame_i * 6
    alpha = 255 - frame_i * 40
    color = explosion_colors[frame_i]
    color = (color[0], color[1], color[2], max(0, alpha))
    # Outer
    draw.ellipse([32-radius, 32-radius, 32+radius, 32+radius], fill=color)
    # Inner bright
    inner_r = max(2, radius - 8)
    inner_c = explosion_colors[max(0, frame_i-1)]
    draw.ellipse([32-inner_r, 32-inner_r, 32+inner_r, 32+inner_r], fill=inner_c)
    # Core white
    if frame_i < 3:
        core_r = max(1, inner_r - 4)
        draw.ellipse([32-core_r, 32-core_r, 32+core_r, 32+core_r], fill=(255,255,255,200))
    save(img, f"{BASE}/Effects/explosion_{frame_i}.png")

# ============================================================================
# BACKGROUND STARS (tileable 256x256)
# ============================================================================
print("[Background]")
import random
random.seed(42)

bg_img = Image.new("RGBA", (256, 256), (5, 5, 20, 255))
draw = ImageDraw.Draw(bg_img)

# Small dim stars
for _ in range(120):
    x, y = random.randint(0, 255), random.randint(0, 255)
    brightness = random.randint(60, 150)
    size = 1
    draw.rectangle([x, y, x+size, y+size], fill=(brightness, brightness, brightness+20, 255))

# Medium stars
for _ in range(30):
    x, y = random.randint(0, 255), random.randint(0, 255)
    brightness = random.randint(150, 230)
    tint = random.choice([(0,0,30), (0,20,0), (20,0,0), (0,0,0)])
    color = (min(255, brightness+tint[0]), min(255, brightness+tint[1]), min(255, brightness+tint[2]), 255)
    draw.rectangle([x, y, x+1, y+1], fill=color)

# Bright stars with glow
for _ in range(8):
    x, y = random.randint(2, 253), random.randint(2, 253)
    draw.rectangle([x, y, x+1, y+1], fill=(255, 255, 255, 255))
    # Cross glow
    for d in range(1, 3):
        alpha = 150 - d * 50
        glow = (200, 200, 255, max(0, alpha))
        draw.point((x-d, y), fill=glow)
        draw.point((x+d, y), fill=glow)
        draw.point((x, y-d), fill=glow)
        draw.point((x, y+d), fill=glow)

save(bg_img, f"{BASE}/Background/starfield_bg.png")

# Nebula overlay (256x256 with colored clouds)
nebula_img = Image.new("RGBA", (256, 256), (0, 0, 0, 0))
draw = ImageDraw.Draw(nebula_img)
for _ in range(15):
    x, y = random.randint(-30, 256), random.randint(-30, 256)
    r = random.randint(30, 80)
    color_choice = random.choice([
        (40, 20, 80, 15),
        (80, 20, 40, 12),
        (20, 40, 80, 10),
        (40, 60, 80, 8),
    ])
    draw.ellipse([x-r, y-r, x+r, y+r], fill=color_choice)
save(nebula_img, f"{BASE}/Background/nebula_overlay.png")

# ============================================================================
# UI ELEMENTS
# ============================================================================
print("[UI Elements]")

# Heart icon for health
heart_pixels = [
    [0,1,1,0,0,1,1,0],
    [1,2,2,1,1,2,2,1],
    [1,2,2,2,2,2,2,1],
    [1,2,2,2,2,2,2,1],
    [0,1,2,2,2,2,1,0],
    [0,0,1,2,2,1,0,0],
    [0,0,0,1,1,0,0,0],
    [0,0,0,0,0,0,0,0],
]
heart_palette = [(200, 30, 30, 255), (255, 80, 80, 255)]
save(make_pixel_art(8, 8, heart_pixels, heart_palette, 4), f"{BASE}/UI/heart.png")

# Empty heart
heart_empty_palette = [(80, 30, 30, 255), (120, 50, 50, 255)]
save(make_pixel_art(8, 8, heart_pixels, heart_empty_palette, 4), f"{BASE}/UI/heart_empty.png")

# Shield icon
shield_ui_pixels = [
    [0,0,1,1,1,1,0,0],
    [0,1,2,2,2,2,1,0],
    [1,2,2,2,2,2,2,1],
    [1,2,2,2,2,2,2,1],
    [1,2,2,2,2,2,2,1],
    [0,1,2,2,2,2,1,0],
    [0,0,1,2,2,1,0,0],
    [0,0,0,1,1,0,0,0],
]
shield_palette = [(40, 100, 200, 255), (80, 160, 255, 255)]
save(make_pixel_art(8, 8, shield_ui_pixels, shield_palette, 4), f"{BASE}/UI/shield_icon.png")

# Button background (simple 64x20 rounded rect)
btn_img = Image.new("RGBA", (200, 50), (0, 0, 0, 0))
draw = ImageDraw.Draw(btn_img)
draw.rounded_rectangle([0, 0, 199, 49], radius=8, fill=(40, 60, 120, 220), outline=(100, 150, 255, 255))
save(btn_img, f"{BASE}/UI/button_bg.png")

# Panel background
panel_img = Image.new("RGBA", (400, 300), (0, 0, 0, 0))
draw = ImageDraw.Draw(panel_img)
draw.rounded_rectangle([0, 0, 399, 299], radius=12, fill=(10, 15, 40, 220), outline=(60, 100, 180, 200))
# Inner border
draw.rounded_rectangle([4, 4, 395, 295], radius=10, outline=(40, 70, 140, 150))
save(panel_img, f"{BASE}/UI/panel_bg.png")

print("\n=== All sprites generated successfully! ===")
