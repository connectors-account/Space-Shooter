"""
Star Defender — Python / Pygame Space Shooter
=============================================
Single-file desktop game. No external assets required.
Run:  python space_shooter.py
Pack: pyinstaller --onefile --windowed space_shooter.py
"""

import pygame
import random
import math
import sys

# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------
SCREEN_W, SCREEN_H = 480, 720
FPS = 60
TITLE = "Star Defender"

# Colours
BLACK   = (  0,   0,   0)
WHITE   = (255, 255, 255)
GREY    = (120, 120, 120)
DGREY   = ( 40,  40,  40)
RED     = (220,  40,  40)
DRED    = (140,  20,  20)
GREEN   = ( 50, 205,  50)
DGREEN  = ( 20, 120,  20)
BLUE    = ( 60, 120, 255)
LBLUE   = (130, 200, 255)
CYAN    = (  0, 220, 220)
YELLOW  = (255, 220,  30)
ORANGE  = (255, 140,   0)
PURPLE  = (160,  40, 220)
LPURPLE = (200, 120, 255)
PINK    = (255,  80, 180)

# Layers (z-order via draw order)
# Background → Stars → PowerUps → Enemies → Player → Bullets → Particles → HUD


# ---------------------------------------------------------------------------
# Utility helpers
# ---------------------------------------------------------------------------
def clamp(value, lo, hi):
    return max(lo, min(hi, value))


def dist(a, b):
    return math.hypot(a[0] - b[0], a[1] - b[1])


def lerp_color(c1, c2, t):
    return tuple(int(c1[i] + (c2[i] - c1[i]) * t) for i in range(3))


# ---------------------------------------------------------------------------
# Drawing helpers (no sprites — pure pygame.draw)
# ---------------------------------------------------------------------------
def draw_player(surf, x, y, tilt=0.0, shield=False):
    """Draw a sleek blue arrowhead ship."""
    pts = [
        (x,      y - 22),   # nose
        (x - 16, y + 14),   # left wing tip
        (x - 6,  y + 6),    # left inner
        (x,      y + 12),   # tail centre
        (x + 6,  y + 6),    # right inner
        (x + 16, y + 14),   # right wing tip
    ]
    # Apply tilt
    if tilt:
        rad = math.radians(tilt)
        cos_t, sin_t = math.cos(rad), math.sin(rad)
        pts = [(x + (px - x) * cos_t - (py - y) * sin_t,
                y + (px - x) * sin_t + (py - y) * cos_t)
               for px, py in pts]

    pygame.draw.polygon(surf, BLUE, pts)
    pygame.draw.polygon(surf, LBLUE, pts, 2)
    # Engine glow
    pygame.draw.ellipse(surf, ORANGE, (x - 5, y + 10, 10, 8))
    pygame.draw.ellipse(surf, YELLOW, (x - 3, y + 11, 6, 5))
    if shield:
        pygame.draw.circle(surf, CYAN, (int(x), int(y)), 26, 3)
        s = pygame.Surface((56, 56), pygame.SRCALPHA)
        pygame.draw.circle(s, (0, 220, 220, 45), (28, 28), 26)
        surf.blit(s, (x - 28, y - 28))


def draw_enemy_a(surf, x, y, hp_ratio=1.0):
    """Red diamond — basic enemy."""
    c = lerp_color(RED, YELLOW, 1 - hp_ratio)
    pts = [(x, y - 18), (x + 14, y), (x, y + 18), (x - 14, y)]
    pygame.draw.polygon(surf, c, pts)
    pygame.draw.polygon(surf, WHITE, pts, 1)
    pygame.draw.circle(surf, YELLOW, (int(x), int(y)), 4)


def draw_enemy_b(surf, x, y, hp_ratio=1.0):
    """Orange zigzag fighter."""
    c = lerp_color(ORANGE, RED, 1 - hp_ratio)
    pts = [(x, y - 16), (x + 18, y + 4), (x + 6, y + 8),
           (x, y + 16), (x - 6, y + 8), (x - 18, y + 4)]
    pygame.draw.polygon(surf, c, pts)
    pygame.draw.polygon(surf, YELLOW, pts, 1)
    pygame.draw.ellipse(surf, RED, (x - 5, y - 5, 10, 10))


def draw_boss(surf, x, y, hp_ratio=1.0):
    """Large purple boss ship."""
    c = lerp_color(PURPLE, PINK, 1 - hp_ratio)
    # Main body
    pygame.draw.ellipse(surf, c, (x - 36, y - 22, 72, 44))
    # Wings
    left_pts  = [(x - 36, y - 10), (x - 64, y + 20), (x - 36, y + 22)]
    right_pts = [(x + 36, y - 10), (x + 64, y + 20), (x + 36, y + 22)]
    pygame.draw.polygon(surf, c, left_pts)
    pygame.draw.polygon(surf, c, right_pts)
    # Detail lines
    pygame.draw.ellipse(surf, LPURPLE, (x - 20, y - 12, 40, 24), 2)
    pygame.draw.circle(surf, PINK, (int(x), int(y)), 10)
    pygame.draw.circle(surf, WHITE, (int(x), int(y)), 5)
    # Engine dots
    for ex in (x - 20, x, x + 20):
        pygame.draw.circle(surf, YELLOW, (int(ex), int(y + 18)), 4)


def draw_bullet_player(surf, x, y):
    pygame.draw.ellipse(surf, CYAN, (x - 3, y - 8, 6, 16))
    pygame.draw.ellipse(surf, WHITE, (x - 1, y - 7, 2, 8))


def draw_bullet_enemy(surf, x, y):
    pygame.draw.ellipse(surf, RED, (x - 3, y - 7, 6, 14))
    pygame.draw.ellipse(surf, ORANGE, (x - 1, y - 5, 2, 8))


def draw_powerup(surf, x, y, kind):
    colours = {
        "health":  (GREEN,  DGREEN),
        "shield":  (CYAN,   BLUE),
        "rapid":   (YELLOW, ORANGE),
        "triple":  (PINK,   PURPLE),
        "bomb":    (RED,    DRED),
        "speed":   (ORANGE, YELLOW),
    }
    outer, inner = colours.get(kind, (WHITE, GREY))
    pygame.draw.circle(surf, outer, (int(x), int(y)), 13)
    pygame.draw.circle(surf, inner, (int(x), int(y)), 8)
    labels = {"health": "+", "shield": "S", "rapid": "R",
              "triple": "3", "bomb": "B", "speed": ">"}
    font = pygame.font.SysFont("arial", 12, bold=True)
    txt = font.render(labels.get(kind, "?"), True, WHITE)
    surf.blit(txt, txt.get_rect(center=(int(x), int(y))))


def draw_explosion(surf, x, y, frame, max_frames, colour=ORANGE):
    t = frame / max_frames
    radius = int(4 + 28 * t)
    alpha = int(255 * (1 - t))
    s = pygame.Surface((radius * 2 + 4, radius * 2 + 4), pygame.SRCALPHA)
    c2 = lerp_color(colour, YELLOW, t)
    pygame.draw.circle(s, (*c2, alpha), (radius + 2, radius + 2), radius)
    inner_r = max(1, radius - 6)
    pygame.draw.circle(s, (*WHITE, alpha // 2), (radius + 2, radius + 2), inner_r)
    surf.blit(s, (x - radius - 2, y - radius - 2))


# ---------------------------------------------------------------------------
# Particle
# ---------------------------------------------------------------------------
class Particle:
    def __init__(self, x, y, colour=ORANGE):
        self.x = x
        self.y = y
        angle = random.uniform(0, 2 * math.pi)
        speed = random.uniform(1, 5)
        self.vx = math.cos(angle) * speed
        self.vy = math.sin(angle) * speed
        self.life = random.randint(18, 36)
        self.max_life = self.life
        self.colour = colour
        self.size = random.randint(2, 5)

    def update(self):
        self.x += self.vx
        self.y += self.vy
        self.vy += 0.08
        self.vx *= 0.97
        self.life -= 1

    def draw(self, surf):
        t = self.life / self.max_life
        r = max(1, int(self.size * t))
        alpha = int(255 * t)
        c = (*self.colour[:3], alpha)
        s = pygame.Surface((r * 2, r * 2), pygame.SRCALPHA)
        pygame.draw.circle(s, c, (r, r), r)
        surf.blit(s, (int(self.x - r), int(self.y - r)))

    @property
    def alive(self):
        return self.life > 0


# ---------------------------------------------------------------------------
# Floating score text
# ---------------------------------------------------------------------------
class FloatingText:
    FONT = None

    def __init__(self, x, y, text, colour=YELLOW):
        if FloatingText.FONT is None:
            FloatingText.FONT = pygame.font.SysFont("arial", 16, bold=True)
        self.x = float(x)
        self.y = float(y)
        self.text = text
        self.colour = colour
        self.life = 55
        self.max_life = 55

    def update(self):
        self.y -= 1.1
        self.life -= 1

    def draw(self, surf):
        t = self.life / self.max_life
        alpha = int(255 * t)
        img = FloatingText.FONT.render(self.text, True, self.colour)
        img.set_alpha(alpha)
        surf.blit(img, img.get_rect(center=(int(self.x), int(self.y))))

    @property
    def alive(self):
        return self.life > 0


# ---------------------------------------------------------------------------
# Star (background parallax)
# ---------------------------------------------------------------------------
class Star:
    def __init__(self, speed, size, y=None):
        self.x = random.uniform(0, SCREEN_W)
        self.y = random.uniform(0, SCREEN_H) if y is None else y
        self.speed = speed
        self.size = size
        self.brightness = random.randint(80, 220)

    def update(self):
        self.y += self.speed
        if self.y > SCREEN_H:
            self.y = 0
            self.x = random.uniform(0, SCREEN_W)

    def draw(self, surf):
        b = self.brightness
        pygame.draw.circle(surf, (b, b, b), (int(self.x), int(self.y)), self.size)


# ---------------------------------------------------------------------------
# Bullet
# ---------------------------------------------------------------------------
class Bullet:
    def __init__(self, x, y, vx, vy, damage, is_player):
        self.x = float(x)
        self.y = float(y)
        self.vx = vx
        self.vy = vy
        self.damage = damage
        self.is_player = is_player
        self.alive = True
        self.radius = 6

    def update(self):
        self.x += self.vx
        self.y += self.vy
        if self.y < -20 or self.y > SCREEN_H + 20 or self.x < -20 or self.x > SCREEN_W + 20:
            self.alive = False

    def draw(self, surf):
        if self.is_player:
            draw_bullet_player(surf, int(self.x), int(self.y))
        else:
            draw_bullet_enemy(surf, int(self.x), int(self.y))

    @property
    def rect(self):
        return pygame.Rect(self.x - self.radius, self.y - self.radius,
                           self.radius * 2, self.radius * 2)


# ---------------------------------------------------------------------------
# PowerUp
# ---------------------------------------------------------------------------
POWERUP_TYPES = ["health", "shield", "rapid", "triple", "bomb", "speed"]
POWERUP_WEIGHTS = [30, 15, 20, 15, 10, 10]   # relative spawn weights


class PowerUp:
    def __init__(self, x, y, kind=None):
        self.x = float(x)
        self.y = float(y)
        self.kind = kind or random.choices(POWERUP_TYPES, POWERUP_WEIGHTS)[0]
        self.vy = 1.6
        self.alive = True
        self.angle = 0.0
        self.radius = 14

    def update(self):
        self.y += self.vy
        self.angle += 2
        if self.y > SCREEN_H + 30:
            self.alive = False

    def draw(self, surf):
        draw_powerup(surf, self.x, self.y, self.kind)

    @property
    def rect(self):
        return pygame.Rect(self.x - self.radius, self.y - self.radius,
                           self.radius * 2, self.radius * 2)


# ---------------------------------------------------------------------------
# Enemy base
# ---------------------------------------------------------------------------
class Enemy:
    def __init__(self, x, y, hp, score, speed, kind="A"):
        self.x = float(x)
        self.y = float(y)
        self.hp = hp
        self.max_hp = hp
        self.score = score
        self.speed = speed
        self.kind = kind
        self.alive = True
        self.shoot_cooldown = 0
        self.shoot_interval = random.randint(90, 180)
        self.time = random.uniform(0, 2 * math.pi)   # phase offset for sine
        self.move_dir = 1
        self.entry_done = False
        self.radius = 18

    @property
    def hp_ratio(self):
        return self.hp / self.max_hp

    def take_damage(self, amount):
        self.hp -= amount
        if self.hp <= 0:
            self.hp = 0
            self.alive = False

    def shoot(self, bullets, player_x, player_y):
        self.shoot_cooldown -= 1
        if self.shoot_cooldown > 0:
            return
        self.shoot_cooldown = self.shoot_interval
        self._fire(bullets, player_x, player_y)

    def _fire(self, bullets, px, py):
        bullets.append(Bullet(self.x, self.y + 18, 0, 3.5, 1, False))

    def update(self, player_x, player_y, bullets):
        self.time += 0.05
        self._move()
        self.shoot(bullets, player_x, player_y)

    def _move(self):
        self.y += self.speed

    def draw(self, surf):
        draw_enemy_a(surf, self.x, self.y, self.hp_ratio)

    def drop_powerup(self):
        if random.random() < 0.18:
            return PowerUp(self.x, self.y)
        return None

    @property
    def rect(self):
        return pygame.Rect(self.x - self.radius, self.y - self.radius,
                           self.radius * 2, self.radius * 2)


class EnemyB(Enemy):
    def __init__(self, x, y):
        super().__init__(x, y, hp=3, score=20, speed=1.4, kind="B")
        self.radius = 20
        self.shoot_interval = random.randint(70, 120)
        self.origin_x = x

    def _move(self):
        self.y += self.speed * 0.7
        self.x = self.origin_x + math.sin(self.time * 1.2) * 60

    def _fire(self, bullets, px, py):
        # Aimed shot toward player
        dx = px - self.x
        dy = py - self.y
        d = math.hypot(dx, dy) or 1
        speed = 4.0
        bullets.append(Bullet(self.x, self.y + 18,
                               dx / d * speed, dy / d * speed, 1, False))

    def draw(self, surf):
        draw_enemy_b(surf, self.x, self.y, self.hp_ratio)

    def drop_powerup(self):
        if random.random() < 0.25:
            return PowerUp(self.x, self.y)
        return None


class Boss(Enemy):
    PHASES = 3

    def __init__(self):
        super().__init__(SCREEN_W // 2, -60, hp=80, score=500, speed=1.2, kind="C")
        self.radius = 48
        self.target_y = 100
        self.move_dir = 1
        self.burst_timer = 0
        self.burst_interval = 180
        self.shoot_interval = 45
        self.entry_done = False

    @property
    def phase(self):
        r = self.hp_ratio
        if r > 0.6:
            return 1
        elif r > 0.3:
            return 2
        else:
            return 3

    def _move(self):
        if not self.entry_done:
            self.y += 2.5
            if self.y >= self.target_y:
                self.y = self.target_y
                self.entry_done = True
            return
        # Horizontal patrol
        self.x += self.speed * self.move_dir
        if self.x > SCREEN_W - 80:
            self.move_dir = -1
        elif self.x < 80:
            self.move_dir = 1

    def _fire(self, bullets, px, py):
        if not self.entry_done:
            return
        p = self.phase
        if p == 1:
            # Spread fan (5 bullets)
            for i in range(5):
                angle = math.radians(-40 + i * 20)
                bx = math.sin(angle) * 4
                by = math.cos(angle) * 4
                bullets.append(Bullet(self.x, self.y + 30, bx, by, 1, False))
        elif p == 2:
            # Fan + circular burst every burst_interval
            for i in range(5):
                angle = math.radians(-40 + i * 20)
                bullets.append(Bullet(self.x, self.y + 30,
                                       math.sin(angle) * 4, math.cos(angle) * 4, 1, False))
            self.burst_timer -= 1
            if self.burst_timer <= 0:
                self.burst_timer = self.burst_interval
                for i in range(12):
                    angle = math.radians(i * 30)
                    bullets.append(Bullet(self.x, self.y,
                                           math.cos(angle) * 3, math.sin(angle) * 3, 1, False))
        else:
            # Rapid aimed
            self.shoot_interval = 22
            dx = px - self.x
            dy = py - self.y
            d = math.hypot(dx, dy) or 1
            bullets.append(Bullet(self.x, self.y + 30,
                                   dx / d * 5, dy / d * 5, 2, False))

    def draw(self, surf):
        draw_boss(surf, self.x, self.y, self.hp_ratio)

    def drop_powerup(self):
        drops = []
        for kind in ["health", "shield", "bomb"]:
            drops.append(PowerUp(self.x + random.randint(-60, 60), self.y, kind))
        return drops   # Boss drops multiple


# ---------------------------------------------------------------------------
# Wave definitions
# ---------------------------------------------------------------------------
WAVES = [
    # wave 1 – basic enemies only
    {"enemies": [("A", 8)], "boss": False},
    # wave 2 – mix
    {"enemies": [("A", 6), ("B", 4)], "boss": False},
    # wave 3 – harder mix
    {"enemies": [("A", 5), ("B", 6)], "boss": False},
    # wave 4 – boss!
    {"enemies": [], "boss": True},
]


def spawn_wave(wave_idx):
    enemies = []
    wdata = WAVES[min(wave_idx, len(WAVES) - 1)]
    if wdata["boss"]:
        enemies.append(Boss())
        return enemies
    for kind, count in wdata["enemies"]:
        for _ in range(count):
            x = random.randint(50, SCREEN_W - 50)
            y = random.randint(-300, -40)
            if kind == "A":
                enemies.append(Enemy(x, y, hp=2, score=10, speed=random.uniform(1.0, 1.8)))
            elif kind == "B":
                enemies.append(EnemyB(x, y))
    return enemies


# ---------------------------------------------------------------------------
# Player
# ---------------------------------------------------------------------------
class Player:
    MAX_HP = 5
    FIRE_COOLDOWN_BASE = 14
    SPEED_BASE = 4.5

    def __init__(self):
        self.x = float(SCREEN_W // 2)
        self.y = float(SCREEN_H - 100)
        self.hp = self.MAX_HP
        self.alive = True

        # Power-up state
        self.shield = False
        self.shield_timer = 0
        self.rapid_timer = 0
        self.triple_timer = 0
        self.speed_timer = 0

        self.invincible_timer = 0
        self.fire_cooldown = 0
        self.tilt = 0.0
        self.score_multiplier = 1

        self.radius = 16

    @property
    def fire_rate(self):
        return max(5, self.FIRE_COOLDOWN_BASE - (6 if self.rapid_timer > 0 else 0))

    @property
    def speed(self):
        return self.SPEED_BASE + (2.0 if self.speed_timer > 0 else 0)

    def move(self, keys):
        dx = dy = 0
        if keys[pygame.K_LEFT]  or keys[pygame.K_a]: dx -= 1
        if keys[pygame.K_RIGHT] or keys[pygame.K_d]: dx += 1
        if keys[pygame.K_UP]    or keys[pygame.K_w]: dy -= 1
        if keys[pygame.K_DOWN]  or keys[pygame.K_s]: dy += 1

        if dx and dy:
            dx *= 0.707
            dy *= 0.707

        self.x = clamp(self.x + dx * self.speed, 20, SCREEN_W - 20)
        self.y = clamp(self.y + dy * self.speed, 20, SCREEN_H - 20)

        # Tilt ship
        target_tilt = dx * 18
        self.tilt += (target_tilt - self.tilt) * 0.2

    def try_fire(self, keys, bullets):
        if not (keys[pygame.K_SPACE] or keys[pygame.K_z]):
            return
        self.fire_cooldown -= 1
        if self.fire_cooldown > 0:
            return
        self.fire_cooldown = self.fire_rate
        if self.triple_timer > 0:
            bullets.append(Bullet(self.x, self.y - 22, -0.8, -10, 1, True))
            bullets.append(Bullet(self.x, self.y - 22,  0,   -10, 1, True))
            bullets.append(Bullet(self.x, self.y - 22,  0.8, -10, 1, True))
        else:
            bullets.append(Bullet(self.x, self.y - 22, 0, -10, 1, True))

    def take_damage(self, amount=1):
        if self.invincible_timer > 0 or self.shield:
            if self.shield:
                self.shield = False
                self.shield_timer = 0
            return False
        self.hp -= amount
        self.invincible_timer = 90
        if self.hp <= 0:
            self.hp = 0
            self.alive = False
        return True

    def apply_powerup(self, kind):
        if kind == "health":
            self.hp = min(self.MAX_HP, self.hp + 1)
        elif kind == "shield":
            self.shield = True
            self.shield_timer = 600
        elif kind == "rapid":
            self.rapid_timer = 480
        elif kind == "triple":
            self.triple_timer = 480
        elif kind == "speed":
            self.speed_timer = 480
        # bomb handled by game

    def update(self, keys, bullets):
        self.move(keys)
        self.try_fire(keys, bullets)

        # Tick timers
        if self.invincible_timer > 0:  self.invincible_timer -= 1
        if self.shield_timer  > 0:
            self.shield_timer -= 1
            if self.shield_timer == 0: self.shield = False
        if self.rapid_timer  > 0: self.rapid_timer  -= 1
        if self.triple_timer > 0: self.triple_timer -= 1
        if self.speed_timer  > 0: self.speed_timer  -= 1

    def draw(self, surf):
        # Flicker when invincible
        if self.invincible_timer > 0 and (self.invincible_timer // 5) % 2 == 0:
            return
        draw_player(surf, int(self.x), int(self.y), self.tilt, self.shield)

    @property
    def rect(self):
        return pygame.Rect(self.x - self.radius, self.y - self.radius,
                           self.radius * 2, self.radius * 2)


# ---------------------------------------------------------------------------
# HUD drawing
# ---------------------------------------------------------------------------
class HUD:
    def __init__(self):
        self.font_big   = pygame.font.SysFont("consolas", 26, bold=True)
        self.font_med   = pygame.font.SysFont("consolas", 18, bold=True)
        self.font_small = pygame.font.SysFont("consolas", 14)

    def draw(self, surf, player, score, hi_score, wave, total_waves, boss=None):
        # Score
        surf.blit(self.font_big.render(f"{score:07d}", True, WHITE), (10, 8))
        surf.blit(self.font_small.render(f"HI {hi_score:07d}", True, GREY),  (10, 36))

        # Wave indicator
        wave_txt = "BOSS!" if boss else f"WAVE {wave}/{total_waves - 1}"
        w_img = self.font_med.render(wave_txt, True, YELLOW)
        surf.blit(w_img, w_img.get_rect(topright=(SCREEN_W - 10, 8)))

        # Lives (hearts)
        for i in range(Player.MAX_HP):
            filled = i < player.hp
            c = RED if filled else DGREY
            hx = SCREEN_W - 10 - (Player.MAX_HP - i) * 22
            hy = 34
            # Simple heart: circle + rotated square
            pygame.draw.circle(surf, c, (hx - 3, hy), 5)
            pygame.draw.circle(surf, c, (hx + 3, hy), 5)
            pygame.draw.polygon(surf, c, [(hx - 8, hy + 2), (hx + 8, hy + 2), (hx, hy + 12)])

        # Active power-ups
        px_off = 10
        if player.shield:
            self._draw_buff(surf, px_off, 60, "SHD", CYAN, player.shield_timer, 600)
            px_off += 68
        if player.rapid_timer > 0:
            self._draw_buff(surf, px_off, 60, "RFR", YELLOW, player.rapid_timer, 480)
            px_off += 68
        if player.triple_timer > 0:
            self._draw_buff(surf, px_off, 60, "TRP", PINK, player.triple_timer, 480)
            px_off += 68
        if player.speed_timer > 0:
            self._draw_buff(surf, px_off, 60, "SPD", ORANGE, player.speed_timer, 480)

        # Boss HP bar
        if boss:
            bar_w = SCREEN_W - 20
            bar_h = 18
            bx, by = 10, SCREEN_H - 30
            pygame.draw.rect(surf, DGREY, (bx, by, bar_w, bar_h), border_radius=4)
            fill = int(bar_w * boss.hp_ratio)
            c = lerp_color(RED, PURPLE, boss.hp_ratio)
            if fill > 0:
                pygame.draw.rect(surf, c, (bx, by, fill, bar_h), border_radius=4)
            pygame.draw.rect(surf, WHITE, (bx, by, bar_w, bar_h), 2, border_radius=4)
            label = self.font_small.render("BOSS", True, WHITE)
            surf.blit(label, (bx + 4, by + 2))

    def _draw_buff(self, surf, x, y, label, colour, timer, max_timer):
        bar_w = 60
        bar_h = 14
        pygame.draw.rect(surf, DGREY, (x, y + 16, bar_w, bar_h), border_radius=3)
        fill = int(bar_w * timer / max_timer)
        pygame.draw.rect(surf, colour, (x, y + 16, fill, bar_h), border_radius=3)
        pygame.draw.rect(surf, WHITE,  (x, y + 16, bar_w, bar_h), 1, border_radius=3)
        img = self.font_small.render(label, True, colour)
        surf.blit(img, (x, y))


# ---------------------------------------------------------------------------
# Screen helpers
# ---------------------------------------------------------------------------
def draw_title_screen(surf, font_big, font_med, font_small, hi_score, tick):
    surf.fill((5, 5, 20))
    # Animated title bob
    bob = math.sin(tick * 0.04) * 6
    title = font_big.render("STAR DEFENDER", True, CYAN)
    surf.blit(title, title.get_rect(center=(SCREEN_W // 2, 160 + bob)))

    sub = font_med.render("[ SPACE / Z ]  to start", True, WHITE)
    surf.blit(sub, sub.get_rect(center=(SCREEN_W // 2, 260)))

    ctrl = font_small.render("WASD / ARROWS  to move    ESC to pause", True, GREY)
    surf.blit(ctrl, ctrl.get_rect(center=(SCREEN_W // 2, 310)))

    hi = font_med.render(f"HI-SCORE  {hi_score:07d}", True, YELLOW)
    surf.blit(hi, hi.get_rect(center=(SCREEN_W // 2, 380)))

    credits = font_small.render("v1.0  |  pure Python + Pygame", True, DGREY)
    surf.blit(credits, credits.get_rect(center=(SCREEN_W // 2, SCREEN_H - 24)))


def draw_pause_screen(surf, font_big, font_med):
    overlay = pygame.Surface((SCREEN_W, SCREEN_H), pygame.SRCALPHA)
    overlay.fill((0, 0, 0, 160))
    surf.blit(overlay, (0, 0))
    p = font_big.render("PAUSED", True, WHITE)
    surf.blit(p, p.get_rect(center=(SCREEN_W // 2, SCREEN_H // 2 - 30)))
    r = font_med.render("[ ESC ]  to resume", True, GREY)
    surf.blit(r, r.get_rect(center=(SCREEN_W // 2, SCREEN_H // 2 + 20)))


def draw_gameover_screen(surf, font_big, font_med, font_small, score, hi_score, tick):
    overlay = pygame.Surface((SCREEN_W, SCREEN_H), pygame.SRCALPHA)
    overlay.fill((0, 0, 0, 200))
    surf.blit(overlay, (0, 0))

    go = font_big.render("GAME OVER", True, RED)
    surf.blit(go, go.get_rect(center=(SCREEN_W // 2, SCREEN_H // 2 - 90)))

    # Animated score count-up (80 ticks)
    shown = min(score, int(score * tick / 80))
    sc = font_med.render(f"SCORE  {shown:07d}", True, WHITE)
    surf.blit(sc, sc.get_rect(center=(SCREEN_W // 2, SCREEN_H // 2 - 30)))

    if score >= hi_score and score > 0:
        nb = font_med.render("NEW HI-SCORE!", True, YELLOW)
        surf.blit(nb, nb.get_rect(center=(SCREEN_W // 2, SCREEN_H // 2 + 20)))

    rs = font_med.render("[ R ]  retry     [ M ]  menu", True, GREY)
    surf.blit(rs, rs.get_rect(center=(SCREEN_W // 2, SCREEN_H // 2 + 70)))


def draw_victory_screen(surf, font_big, font_med, score, hi_score, tick):
    overlay = pygame.Surface((SCREEN_W, SCREEN_H), pygame.SRCALPHA)
    overlay.fill((0, 0, 30, 210))
    surf.blit(overlay, (0, 0))
    bob = math.sin(tick * 0.08) * 5
    v = font_big.render("VICTORY!", True, YELLOW)
    surf.blit(v, v.get_rect(center=(SCREEN_W // 2, SCREEN_H // 2 - 90 + bob)))
    shown = min(score, int(score * tick / 80))
    sc = font_med.render(f"SCORE  {shown:07d}", True, WHITE)
    surf.blit(sc, sc.get_rect(center=(SCREEN_W // 2, SCREEN_H // 2 - 20)))
    if score >= hi_score and score > 0:
        nb = font_med.render("NEW HI-SCORE!", True, CYAN)
        surf.blit(nb, nb.get_rect(center=(SCREEN_W // 2, SCREEN_H // 2 + 30)))
    rs = font_med.render("[ R ]  play again    [ M ]  menu", True, GREY)
    surf.blit(rs, rs.get_rect(center=(SCREEN_W // 2, SCREEN_H // 2 + 90)))


def draw_wave_banner(surf, font_big, wave_num, alpha):
    s = pygame.Surface((SCREEN_W, 70), pygame.SRCALPHA)
    s.fill((0, 0, 0, int(180 * alpha)))
    surf.blit(s, (0, SCREEN_H // 2 - 35))
    img = font_big.render(f"— WAVE {wave_num} —", True, (*YELLOW, int(255 * alpha)))
    surf.blit(img, img.get_rect(center=(SCREEN_W // 2, SCREEN_H // 2)))


# ---------------------------------------------------------------------------
# Main Game
# ---------------------------------------------------------------------------
class Game:
    STATE_MENU     = "menu"
    STATE_PLAYING  = "playing"
    STATE_PAUSED   = "paused"
    STATE_GAMEOVER = "gameover"
    STATE_VICTORY  = "victory"

    def __init__(self):
        pygame.init()
        self.screen = pygame.display.set_mode((SCREEN_W, SCREEN_H))
        pygame.display.set_caption(TITLE)
        self.clock = pygame.time.Clock()

        self.font_big   = pygame.font.SysFont("consolas", 36, bold=True)
        self.font_med   = pygame.font.SysFont("consolas", 22, bold=True)
        self.font_small = pygame.font.SysFont("consolas", 15)

        self.hi_score = 0
        self.state = self.STATE_MENU
        self.tick  = 0   # global tick for animations

        # Background stars — 3 parallax layers
        self.stars = (
            [Star(0.4, 1) for _ in range(60)],   # far
            [Star(0.9, 1) for _ in range(35)],   # mid
            [Star(1.8, 2) for _ in range(15)],   # near
        )

        self._init_session()

    # ------------------------------------------------------------------
    def _init_session(self):
        """Reset all gameplay state for a new game."""
        self.player    = Player()
        self.bullets   = []
        self.enemies   = []
        self.powerups  = []
        self.particles = []
        self.floats    = []
        self.hud       = HUD()
        self.score     = 0

        self.wave_idx     = 0
        self.wave_active  = False
        self.wave_banner_timer = 0   # counts down; shows banner while > 0
        self.between_wave_timer = 0  # delay before spawning next wave
        self.boss         = None

        self._start_wave()

    def _start_wave(self):
        self.wave_active = True
        self.wave_banner_timer = 120      # 2 seconds at 60fps
        self.enemies = spawn_wave(self.wave_idx)
        self.boss = next((e for e in self.enemies if isinstance(e, Boss)), None)

    def _next_wave(self):
        self.wave_idx += 1
        if self.wave_idx >= len(WAVES):
            self.state = self.STATE_VICTORY
            if self.score > self.hi_score:
                self.hi_score = self.score
            return
        self.between_wave_timer = 120
        self.wave_active = False

    # ------------------------------------------------------------------
    def run(self):
        while True:
            dt = self.clock.tick(FPS)
            self._handle_events()
            self._update()
            self._draw()
            pygame.display.flip()

    # ------------------------------------------------------------------
    def _handle_events(self):
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit(); sys.exit()
            if event.type == pygame.KEYDOWN:
                if self.state == self.STATE_MENU:
                    if event.key in (pygame.K_SPACE, pygame.K_z):
                        self.state = self.STATE_PLAYING
                        self._init_session()
                elif self.state == self.STATE_PLAYING:
                    if event.key == pygame.K_ESCAPE:
                        self.state = self.STATE_PAUSED
                elif self.state == self.STATE_PAUSED:
                    if event.key == pygame.K_ESCAPE:
                        self.state = self.STATE_PLAYING
                elif self.state in (self.STATE_GAMEOVER, self.STATE_VICTORY):
                    if event.key == pygame.K_r:
                        self.state = self.STATE_PLAYING
                        self._init_session()
                    elif event.key == pygame.K_m:
                        self.state = self.STATE_MENU

    # ------------------------------------------------------------------
    def _update(self):
        self.tick += 1

        # Always scroll stars
        for layer in self.stars:
            for s in layer:
                s.update()

        if self.state != self.STATE_PLAYING:
            return

        keys = pygame.key.get_pressed()

        # Player
        self.player.update(keys, self.bullets)
        if not self.player.alive:
            if self.score > self.hi_score:
                self.hi_score = self.score
            self.state = self.STATE_GAMEOVER
            self.tick  = 0
            self._spawn_explosion(self.player.x, self.player.y, big=True)
            return

        # Bullets
        for b in self.bullets:
            b.update()
        self.bullets = [b for b in self.bullets if b.alive]

        # Wave banner countdown
        if self.wave_banner_timer > 0:
            self.wave_banner_timer -= 1

        # Between-wave delay
        if not self.wave_active:
            self.between_wave_timer -= 1
            if self.between_wave_timer <= 0:
                self._start_wave()
            return

        # Enemies
        for e in self.enemies:
            e.update(self.player.x, self.player.y, self.bullets)

        # --- Collision: player bullets vs enemies ---
        for b in self.bullets:
            if not b.is_player or not b.alive:
                continue
            for e in self.enemies:
                if not e.alive:
                    continue
                if b.rect.colliderect(e.rect):
                    e.take_damage(b.damage)
                    b.alive = False
                    self._spawn_hit_particles(b.x, b.y)
                    if not e.alive:
                        self._on_enemy_killed(e)
                    break

        # --- Collision: enemy bullets vs player ---
        for b in self.bullets:
            if b.is_player or not b.alive:
                continue
            if b.rect.colliderect(self.player.rect):
                b.alive = False
                hit = self.player.take_damage(b.damage)
                if hit:
                    self._spawn_hit_particles(self.player.x, self.player.y, colour=RED)

        # --- Collision: player body vs enemy ---
        for e in self.enemies:
            if not e.alive:
                continue
            if self.player.rect.colliderect(e.rect):
                hit = self.player.take_damage(2)
                if hit:
                    self._spawn_hit_particles(e.x, e.y, colour=RED)
                e.take_damage(99)
                if not e.alive:
                    self._on_enemy_killed(e)

        # --- Collision: player vs powerups ---
        for p in self.powerups:
            if not p.alive:
                continue
            if self.player.rect.colliderect(p.rect):
                p.alive = False
                if p.kind == "bomb":
                    self._bomb_all_enemies()
                else:
                    self.player.apply_powerup(p.kind)

        # Power-ups
        for p in self.powerups:
            p.update()
        self.powerups = [p for p in self.powerups if p.alive]

        # Particles & floats
        for pt in self.particles: pt.update()
        self.particles = [pt for pt in self.particles if pt.alive]
        for ft in self.floats:   ft.update()
        self.floats = [ft for ft in self.floats if ft.alive]

        # Remove dead enemies
        self.enemies = [e for e in self.enemies if e.alive]

        # Check wave complete
        if self.wave_active and len(self.enemies) == 0:
            self._next_wave()

    # ------------------------------------------------------------------
    def _on_enemy_killed(self, e):
        self._spawn_explosion(e.x, e.y, big=isinstance(e, Boss))
        pts = e.score
        self.score += pts
        self.floats.append(FloatingText(e.x, e.y - 10, f"+{pts}",
                                        YELLOW if pts < 100 else CYAN))

        # Drop powerup(s)
        drop = e.drop_powerup()
        if drop:
            if isinstance(drop, list):
                self.powerups.extend(drop)
            else:
                self.powerups.append(drop)

    def _spawn_explosion(self, x, y, big=False):
        count = 28 if big else 14
        colours = [ORANGE, YELLOW, RED, WHITE]
        for _ in range(count):
            self.particles.append(Particle(x, y, random.choice(colours)))
        # Extra ring for boss
        if big:
            for _ in range(20):
                self.particles.append(Particle(x + random.randint(-40, 40),
                                                y + random.randint(-40, 40),
                                                random.choice(colours)))

    def _spawn_hit_particles(self, x, y, colour=CYAN):
        for _ in range(6):
            self.particles.append(Particle(x, y, colour))

    def _bomb_all_enemies(self):
        for e in self.enemies:
            self._on_enemy_killed(e)
            e.alive = False
        self.enemies = []
        self.floats.append(FloatingText(SCREEN_W // 2, SCREEN_H // 2,
                                        "BOMB!", RED))

    # ------------------------------------------------------------------
    def _draw(self):
        # Background
        bg_colour = (5, 5, 20) if not (self.boss and self.boss.alive) else (20, 5, 10)
        self.screen.fill(bg_colour)

        # Parallax stars
        for layer in self.stars:
            for s in layer:
                s.draw(self.screen)

        if self.state == self.STATE_MENU:
            draw_title_screen(self.screen, self.font_big, self.font_med,
                              self.font_small, self.hi_score, self.tick)
            return

        # === In-game drawing ===
        # Power-ups
        for p in self.powerups:
            p.draw(self.screen)

        # Enemies
        for e in self.enemies:
            e.draw(self.screen)

        # Player
        self.player.draw(self.screen)

        # Bullets
        for b in self.bullets:
            b.draw(self.screen)

        # Particles
        for pt in self.particles:
            pt.draw(self.screen)

        # Floating texts
        for ft in self.floats:
            ft.draw(self.screen)

        # HUD
        self.hud.draw(self.screen, self.player, self.score, self.hi_score,
                      self.wave_idx + 1, len(WAVES), self.boss if (self.boss and self.boss.alive) else None)

        # Wave banner
        if self.wave_banner_timer > 0:
            alpha = min(1.0, self.wave_banner_timer / 40)
            draw_wave_banner(self.screen, self.font_big, self.wave_idx + 1, alpha)

        # Overlays
        if self.state == self.STATE_PAUSED:
            draw_pause_screen(self.screen, self.font_big, self.font_med)

        elif self.state == self.STATE_GAMEOVER:
            draw_gameover_screen(self.screen, self.font_big, self.font_med,
                                 self.font_small, self.score, self.hi_score, self.tick)

        elif self.state == self.STATE_VICTORY:
            draw_victory_screen(self.screen, self.font_big, self.font_med,
                                self.score, self.hi_score, self.tick)


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------
if __name__ == "__main__":
    Game().run()
