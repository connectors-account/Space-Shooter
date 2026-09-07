"""
Space Shooter — Single-file Python/Pygame desktop game.
Run:  pip install pygame
      python space_shooter.py
Controls: WASD / Arrow keys to move, SPACE to shoot, P to pause, ESC = quit.
"""

import pygame
import random
import math
import sys

# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------
WIDTH, HEIGHT = 800, 900
FPS = 60
TITLE = "Space Shooter"

# Colours
BLACK   = (0,   0,   0)
WHITE   = (255, 255, 255)
CYAN    = (0,   230, 255)
RED     = (220, 30,  30)
ORANGE  = (255, 140, 0)
YELLOW  = (255, 215, 0)
GREEN   = (50,  220, 80)
PURPLE  = (160, 60,  220)
PINK    = (255, 80,  180)
GREY    = (120, 120, 120)
DARK    = (10,  10,  30)
LBLUE   = (80,  160, 255)

# Layers (Z-order via draw order, not pygame layers)
STAR_COUNTS = [80, 50, 30]          # far, mid, near
STAR_SPEEDS = [0.4, 1.0, 2.0]
STAR_SIZES  = [1,   2,   2]

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def draw_text(surf, text, size, x, y, color=WHITE, align="center"):
    font = pygame.font.SysFont("consolas", size, bold=True)
    img = font.render(text, True, color)
    rect = img.get_rect()
    if align == "center":
        rect.centerx, rect.centery = x, y
    elif align == "left":
        rect.x, rect.centery = x, y
    else:
        rect.right, rect.centery = x, y
    surf.blit(img, rect)

def clamp(val, lo, hi):
    return max(lo, min(hi, val))

def lerp(a, b, t):
    return a + (b - a) * t

# ---------------------------------------------------------------------------
# Star-field (parallax)
# ---------------------------------------------------------------------------

class StarField:
    def __init__(self):
        self.layers = []
        for i, count in enumerate(STAR_COUNTS):
            stars = []
            for _ in range(count):
                x = random.randint(0, WIDTH)
                y = random.randint(0, HEIGHT)
                stars.append([x, y])
            self.layers.append(stars)

    def update(self):
        for i, stars in enumerate(self.layers):
            speed = STAR_SPEEDS[i]
            for s in stars:
                s[1] += speed
                if s[1] > HEIGHT:
                    s[1] = 0
                    s[0] = random.randint(0, WIDTH)

    def draw(self, surf):
        for i, stars in enumerate(self.layers):
            sz = STAR_SIZES[i]
            brightness = 100 + i * 60
            color = (brightness, brightness, brightness)
            for s in stars:
                if sz == 1:
                    surf.set_at((int(s[0]), int(s[1])), color)
                else:
                    pygame.draw.circle(surf, color, (int(s[0]), int(s[1])), sz)

# ---------------------------------------------------------------------------
# Bullet
# ---------------------------------------------------------------------------

class Bullet:
    def __init__(self, x, y, dy, damage, color, width=4, height=14):
        self.x = float(x)
        self.y = float(y)
        self.dy = dy          # negative = up (player), positive = down (enemy)
        self.damage = damage
        self.color = color
        self.w = width
        self.h = height
        self.alive = True

    def update(self):
        self.y += self.dy
        if self.y < -20 or self.y > HEIGHT + 20:
            self.alive = False

    def draw(self, surf):
        rect = pygame.Rect(self.x - self.w // 2, self.y - self.h // 2, self.w, self.h)
        pygame.draw.rect(surf, self.color, rect, border_radius=3)
        # glow
        glow = pygame.Surface((self.w + 6, self.h + 6), pygame.SRCALPHA)
        glow.fill((0, 0, 0, 0))
        gc = (*self.color, 60)
        pygame.draw.rect(glow, gc, glow.get_rect(), border_radius=4)
        surf.blit(glow, (self.x - self.w // 2 - 3, self.y - self.h // 2 - 3),
                  special_flags=pygame.BLEND_RGBA_ADD)

    def get_rect(self):
        return pygame.Rect(self.x - self.w // 2, self.y - self.h // 2, self.w, self.h)

# ---------------------------------------------------------------------------
# Enemy bullet (spread/aimed variants handled by EnemyShooter)
# ---------------------------------------------------------------------------

class EnemyBullet:
    def __init__(self, x, y, vx, vy, damage=10):
        self.x = float(x)
        self.y = float(y)
        self.vx = vx
        self.vy = vy
        self.damage = damage
        self.alive = True
        self.r = 5

    def update(self):
        self.x += self.vx
        self.y += self.vy
        if self.y > HEIGHT + 20 or self.y < -20 or self.x < -20 or self.x > WIDTH + 20:
            self.alive = False

    def draw(self, surf):
        pygame.draw.circle(surf, RED, (int(self.x), int(self.y)), self.r)
        pygame.draw.circle(surf, ORANGE, (int(self.x), int(self.y)), self.r - 2)

    def get_rect(self):
        return pygame.Rect(self.x - self.r, self.y - self.r, self.r * 2, self.r * 2)

# ---------------------------------------------------------------------------
# Particle / Explosion
# ---------------------------------------------------------------------------

class Particle:
    def __init__(self, x, y, color, vx, vy, life, radius):
        self.x = float(x)
        self.y = float(y)
        self.color = color
        self.vx = vx
        self.vy = vy
        self.life = life
        self.max_life = life
        self.radius = radius

    def update(self):
        self.x += self.vx
        self.y += self.vy
        self.vx *= 0.92
        self.vy *= 0.92
        self.life -= 1

    def draw(self, surf):
        alpha = int(255 * (self.life / self.max_life))
        r = max(1, int(self.radius * (self.life / self.max_life)))
        c = (*self.color[:3], alpha)
        s = pygame.Surface((r * 2, r * 2), pygame.SRCALPHA)
        pygame.draw.circle(s, c, (r, r), r)
        surf.blit(s, (int(self.x) - r, int(self.y) - r))

    @property
    def alive(self):
        return self.life > 0

def spawn_explosion(particles, x, y, colors, count=20, speed=4, radius=5):
    for _ in range(count):
        angle = random.uniform(0, math.tau)
        spd = random.uniform(0.5, speed)
        vx = math.cos(angle) * spd
        vy = math.sin(angle) * spd
        color = random.choice(colors)
        life = random.randint(20, 45)
        r = random.randint(2, radius)
        particles.append(Particle(x, y, color, vx, vy, life, r))

# ---------------------------------------------------------------------------
# Power-Up
# ---------------------------------------------------------------------------

POWERUP_TYPES = ["health", "shield", "rapid", "triple", "bomb"]
POWERUP_COLORS = {
    "health":  GREEN,
    "shield":  LBLUE,
    "rapid":   YELLOW,
    "triple":  PINK,
    "bomb":    ORANGE,
}
POWERUP_LABELS = {
    "health":  "HP",
    "shield":  "SH",
    "rapid":   "RF",
    "triple":  "3X",
    "bomb":    "BM",
}

class PowerUp:
    SIZE = 22

    def __init__(self, x, y, kind):
        self.x = float(x)
        self.y = float(y)
        self.kind = kind
        self.color = POWERUP_COLORS[kind]
        self.label = POWERUP_LABELS[kind]
        self.alive = True
        self.angle = 0.0
        self.bob = 0.0

    def update(self):
        self.y += 1.8
        self.angle += 2
        self.bob += 0.05
        if self.y > HEIGHT + 40:
            self.alive = False

    def draw(self, surf):
        cx = int(self.x)
        cy = int(self.y + math.sin(self.bob) * 4)
        pygame.draw.circle(surf, self.color, (cx, cy), self.SIZE)
        pygame.draw.circle(surf, WHITE, (cx, cy), self.SIZE, 2)
        draw_text(surf, self.label, 13, cx, cy, BLACK)

    def get_rect(self):
        s = self.SIZE
        return pygame.Rect(self.x - s, self.y - s, s * 2, s * 2)

# ---------------------------------------------------------------------------
# Player
# ---------------------------------------------------------------------------

class Player:
    W, H = 36, 44
    SPEED = 5.5
    MAX_HP = 100
    FIRE_COOLDOWN = 12       # frames
    RAPID_COOLDOWN = 5
    INVINCIBLE_FRAMES = 90

    def __init__(self):
        self.x = float(WIDTH // 2)
        self.y = float(HEIGHT - 100)
        self.hp = self.MAX_HP
        self.shield = 0          # hits absorbed
        self.alive = True
        self.score = 0
        self.lives = 3

        # Power-up timers (frames)
        self.rapid_timer  = 0
        self.triple_timer = 0

        self._inv_timer = 0      # invincibility after hit
        self._fire_timer = 0
        self._tilt = 0.0         # sprite lean

    # ---- drawing ----
    def draw(self, surf):
        cx, cy = int(self.x), int(self.y)

        # Engine glow
        for i in range(3):
            r = 8 - i * 2
            alpha = 80 - i * 25
            s = pygame.Surface((r * 2, r * 2), pygame.SRCALPHA)
            pygame.draw.circle(s, (100, 200, 255, alpha), (r, r), r)
            surf.blit(s, (cx - r, cy + 20))

        # Body (triangle hull)
        tilt = int(self._tilt * 8)
        pts = [
            (cx + tilt,        cy - 22),   # nose
            (cx - 18 + tilt,   cy + 20),   # left wing
            (cx - 8  + tilt,   cy + 10),
            (cx      + tilt,   cy + 16),   # exhaust notch
            (cx + 8  + tilt,   cy + 10),
            (cx + 18 + tilt,   cy + 20),   # right wing
        ]
        pygame.draw.polygon(surf, CYAN, pts)
        pygame.draw.polygon(surf, WHITE, pts, 2)

        # Cockpit
        pygame.draw.ellipse(surf, (150, 230, 255),
                            (cx - 6 + tilt, cy - 12, 12, 14))

        # Shield ring
        if self.shield > 0:
            sc = pygame.Surface((70, 70), pygame.SRCALPHA)
            pygame.draw.circle(sc, (80, 180, 255, 100), (35, 35), 34)
            pygame.draw.circle(sc, (80, 200, 255, 180), (35, 35), 34, 2)
            surf.blit(sc, (cx - 35, cy - 35))

        # Flicker when invincible
        if self._inv_timer > 0 and (self._inv_timer // 5) % 2 == 0:
            s = pygame.Surface((self.W + 10, self.H + 10), pygame.SRCALPHA)
            pygame.draw.ellipse(s, (255, 255, 255, 60), s.get_rect())
            surf.blit(s, (cx - self.W // 2 - 5, cy - self.H // 2 - 5))

    def update(self, keys, bullets):
        # Movement
        dx = dy = 0
        if keys[pygame.K_LEFT]  or keys[pygame.K_a]: dx -= 1
        if keys[pygame.K_RIGHT] or keys[pygame.K_d]: dx += 1
        if keys[pygame.K_UP]    or keys[pygame.K_w]: dy -= 1
        if keys[pygame.K_DOWN]  or keys[pygame.K_s]: dy += 1

        self._tilt = lerp(self._tilt, dx * 0.6, 0.2)

        if dx != 0 and dy != 0:
            dx *= 0.707; dy *= 0.707
        self.x = clamp(self.x + dx * self.SPEED, 20, WIDTH - 20)
        self.y = clamp(self.y + dy * self.SPEED, 20, HEIGHT - 20)

        # Shooting
        self._fire_timer = max(0, self._fire_timer - 1)
        cooldown = self.RAPID_COOLDOWN if self.rapid_timer > 0 else self.FIRE_COOLDOWN
        if keys[pygame.K_SPACE] and self._fire_timer == 0:
            self._fire(bullets)
            self._fire_timer = cooldown

        # Timers
        if self.rapid_timer  > 0: self.rapid_timer  -= 1
        if self.triple_timer > 0: self.triple_timer -= 1
        if self._inv_timer   > 0: self._inv_timer   -= 1

    def _fire(self, bullets):
        if self.triple_timer > 0:
            bullets.append(Bullet(self.x - 14, self.y - 10, -14, 20, CYAN))
            bullets.append(Bullet(self.x,      self.y - 18, -14, 20, CYAN))
            bullets.append(Bullet(self.x + 14, self.y - 10, -14, 20, CYAN))
        else:
            bullets.append(Bullet(self.x - 8, self.y - 16, -14, 20, CYAN))
            bullets.append(Bullet(self.x + 8, self.y - 16, -14, 20, CYAN))

    def take_damage(self, dmg):
        if self._inv_timer > 0:
            return
        if self.shield > 0:
            self.shield -= 1
            self._inv_timer = 30
            return
        self.hp -= dmg
        self._inv_timer = self.INVINCIBLE_FRAMES
        if self.hp <= 0:
            self.hp = 0
            self.alive = False

    def apply_powerup(self, kind, particles):
        if kind == "health":
            self.hp = min(self.MAX_HP, self.hp + 35)
            spawn_explosion(particles, self.x, self.y, [GREEN, WHITE], 15, 3, 4)
        elif kind == "shield":
            self.shield = 3
            spawn_explosion(particles, self.x, self.y, [LBLUE, WHITE], 15, 3, 4)
        elif kind == "rapid":
            self.rapid_timer = FPS * 8
            spawn_explosion(particles, self.x, self.y, [YELLOW, WHITE], 15, 3, 4)
        elif kind == "triple":
            self.triple_timer = FPS * 8
            spawn_explosion(particles, self.x, self.y, [PINK, WHITE], 15, 3, 4)
        elif kind == "bomb":
            spawn_explosion(particles, self.x, self.y, [ORANGE, YELLOW, RED], 40, 8, 8)
            return True   # caller clears all enemies
        return False

    def get_rect(self):
        return pygame.Rect(self.x - 14, self.y - 20, 28, 40)

# ---------------------------------------------------------------------------
# Enemies
# ---------------------------------------------------------------------------

class Enemy:
    """Base class. Subclass to get specific appearance & behaviour."""
    def __init__(self, x, y):
        self.x = float(x)
        self.y = float(y)
        self.alive = True
        self.hp = 30
        self.max_hp = 30
        self.score_value = 100
        self.shoot_timer = random.randint(60, 120)
        self.shoot_interval = 90
        self.speed = 2.0
        self.color = RED
        self._hit_flash = 0

    # override
    def _draw_shape(self, surf, cx, cy): pass

    def draw(self, surf):
        cx, cy = int(self.x), int(self.y)
        self._draw_shape(surf, cx, cy)
        # HP bar
        bar_w = 36
        ratio = self.hp / self.max_hp
        pygame.draw.rect(surf, GREY, (cx - 18, cy - self._half_h - 8, bar_w, 4))
        pygame.draw.rect(surf, GREEN, (cx - 18, cy - self._half_h - 8,
                                        int(bar_w * ratio), 4))
        # hit flash overlay
        if self._hit_flash > 0:
            s = pygame.Surface((self._half_w * 2, self._half_h * 2), pygame.SRCALPHA)
            s.fill((255, 255, 255, 140))
            surf.blit(s, (cx - self._half_w, cy - self._half_h))
            self._hit_flash -= 1

    def update(self, enemy_bullets, player):
        self._move()
        # Shooting
        self.shoot_timer -= 1
        if self.shoot_timer <= 0:
            self._shoot(enemy_bullets, player)
            self.shoot_timer = self.shoot_interval + random.randint(-10, 20)
        if self.y > HEIGHT + 60:
            self.alive = False

    def _move(self):
        self.y += self.speed

    def take_damage(self, dmg):
        self.hp -= dmg
        self._hit_flash = 4
        if self.hp <= 0:
            self.alive = False

    def _shoot(self, enemy_bullets, player):
        enemy_bullets.append(EnemyBullet(self.x, self.y + 20, 0, 4))

    def get_rect(self):
        return pygame.Rect(self.x - self._half_w, self.y - self._half_h,
                           self._half_w * 2, self._half_h * 2)

    _half_w = 18
    _half_h = 18


class BasicEnemy(Enemy):
    """Red triangle — goes straight down, single shot."""
    _half_w = 20
    _half_h = 20

    def __init__(self, x, y):
        super().__init__(x, y)
        self.hp = self.max_hp = 30
        self.score_value = 100
        self.shoot_interval = 80

    def _draw_shape(self, surf, cx, cy):
        pts = [
            (cx,      cy + 18),
            (cx - 18, cy - 16),
            (cx + 18, cy - 16),
        ]
        pygame.draw.polygon(surf, RED, pts)
        pygame.draw.polygon(surf, ORANGE, pts, 2)
        pygame.draw.circle(surf, YELLOW, (cx, cy - 4), 5)


class FastEnemy(Enemy):
    """Orange diamond — zigzags, shoots aimed."""
    _half_w = 16
    _half_h = 16

    def __init__(self, x, y):
        super().__init__(x, y)
        self.hp = self.max_hp = 18
        self.speed = 3.5
        self.score_value = 150
        self.shoot_interval = 70
        self._phase = random.uniform(0, math.tau)
        self._zigzag_amp = random.choice([-1, 1]) * 2.5

    def _draw_shape(self, surf, cx, cy):
        pts = [(cx, cy - 16), (cx + 16, cy), (cx, cy + 16), (cx - 16, cy)]
        pygame.draw.polygon(surf, ORANGE, pts)
        pygame.draw.polygon(surf, YELLOW, pts, 2)

    def _move(self):
        self._phase += 0.06
        self.x += math.sin(self._phase) * self._zigzag_amp
        self.y += self.speed
        self.x = clamp(self.x, 10, WIDTH - 10)

    def _shoot(self, enemy_bullets, player):
        # aimed shot
        dx = player.x - self.x
        dy = player.y - self.y
        dist = math.hypot(dx, dy) or 1
        spd = 4.5
        enemy_bullets.append(EnemyBullet(self.x, self.y + 16,
                                          dx / dist * spd, dy / dist * spd))


class TankEnemy(Enemy):
    """Purple hexagon — slow, high HP, spread shot."""
    _half_w = 26
    _half_h = 26

    def __init__(self, x, y):
        super().__init__(x, y)
        self.hp = self.max_hp = 90
        self.speed = 1.0
        self.score_value = 250
        self.shoot_interval = 110

    def _draw_shape(self, surf, cx, cy):
        pts = []
        for i in range(6):
            a = math.radians(i * 60 - 30)
            pts.append((cx + math.cos(a) * 26, cy + math.sin(a) * 26))
        pygame.draw.polygon(surf, PURPLE, pts)
        pygame.draw.polygon(surf, PINK, pts, 2)
        pygame.draw.circle(surf, (200, 100, 255), (cx, cy), 10)

    def _shoot(self, enemy_bullets, player):
        # 3-bullet spread
        for angle_off in [-20, 0, 20]:
            rad = math.radians(90 + angle_off)
            spd = 3.0
            enemy_bullets.append(EnemyBullet(self.x, self.y + 26,
                                              math.cos(rad) * spd,
                                              math.sin(rad) * spd, 15))


# ---------------------------------------------------------------------------
# Boss
# ---------------------------------------------------------------------------

class Boss:
    W, H = 90, 70

    def __init__(self, wave):
        self.x = float(WIDTH // 2)
        self.y = -80.0
        self.max_hp = 400 + wave * 100
        self.hp = self.max_hp
        self.alive = True
        self.score_value = 3000 + wave * 500
        self.entering = True        # moving into screen
        self._phase = 0.0           # sine wave position
        self._shoot_timer = 0
        self._shoot_interval = 55
        self._hit_flash = 0
        self._pattern = 0           # 0=spread, 1=circle, 2=aimed

    def draw(self, surf):
        cx, cy = int(self.x), int(self.y)

        # Body
        body_pts = [
            (cx,       cy - 35),
            (cx - 45,  cy - 10),
            (cx - 40,  cy + 30),
            (cx,       cy + 20),
            (cx + 40,  cy + 30),
            (cx + 45,  cy - 10),
        ]
        pygame.draw.polygon(surf, (180, 20, 20), body_pts)
        pygame.draw.polygon(surf, ORANGE, body_pts, 3)

        # Cockpit
        pygame.draw.ellipse(surf, (255, 80, 0), (cx - 16, cy - 18, 32, 22))
        pygame.draw.ellipse(surf, YELLOW,        (cx - 16, cy - 18, 32, 22), 2)

        # Wing accents
        pygame.draw.line(surf, YELLOW, (cx - 45, cy - 10), (cx - 40, cy + 30), 3)
        pygame.draw.line(surf, YELLOW, (cx + 45, cy - 10), (cx + 40, cy + 30), 3)

        # Cannons
        for ex in [-28, 28]:
            pygame.draw.rect(surf, GREY, (cx + ex - 4, cy + 18, 8, 16), border_radius=2)

        # Hit flash
        if self._hit_flash > 0:
            s = pygame.Surface((self.W * 2, self.H * 2), pygame.SRCALPHA)
            s.fill((255, 255, 255, 150))
            surf.blit(s, (cx - self.W, cy - self.H))
            self._hit_flash -= 1

        # HP bar (wide, at top)
        bw = 300
        ratio = self.hp / self.max_hp
        pygame.draw.rect(surf, GREY,  (WIDTH // 2 - bw // 2, 16, bw, 14), border_radius=6)
        bar_color = GREEN if ratio > 0.5 else (YELLOW if ratio > 0.25 else RED)
        pygame.draw.rect(surf, bar_color,
                         (WIDTH // 2 - bw // 2, 16, int(bw * ratio), 14), border_radius=6)
        pygame.draw.rect(surf, WHITE, (WIDTH // 2 - bw // 2, 16, bw, 14), 2, border_radius=6)
        draw_text(surf, "BOSS", 14, WIDTH // 2, 23, WHITE)

    def update(self, enemy_bullets, player):
        if self.entering:
            self.y = min(self.y + 2.5, 100)
            if self.y >= 100:
                self.entering = False
            return

        self._phase += 0.012
        self.x = WIDTH // 2 + math.sin(self._phase) * (WIDTH // 2 - 80)

        self._shoot_timer -= 1
        if self._shoot_timer <= 0:
            # Cycle attack patterns based on remaining HP
            hp_ratio = self.hp / self.max_hp
            if hp_ratio > 0.6:
                self._spread_shot(enemy_bullets)
            elif hp_ratio > 0.3:
                self._circle_shot(enemy_bullets)
            else:
                self._spread_shot(enemy_bullets)
                self._aimed_shot(enemy_bullets, player)
            self._shoot_timer = self._shoot_interval
            # Rage at low HP
            if hp_ratio < 0.3:
                self._shoot_interval = 30

    def _spread_shot(self, enemy_bullets):
        for angle_off in range(-40, 41, 20):
            rad = math.radians(90 + angle_off)
            spd = 3.5
            enemy_bullets.append(
                EnemyBullet(self.x, self.y + 30,
                            math.cos(rad) * spd, math.sin(rad) * spd, 12))

    def _circle_shot(self, enemy_bullets):
        for i in range(12):
            rad = math.radians(i * 30)
            spd = 3.0
            enemy_bullets.append(
                EnemyBullet(self.x, self.y,
                            math.cos(rad) * spd, math.sin(rad) * spd, 10))

    def _aimed_shot(self, enemy_bullets, player):
        dx = player.x - self.x
        dy = player.y - self.y
        dist = math.hypot(dx, dy) or 1
        spd = 5.5
        enemy_bullets.append(
            EnemyBullet(self.x, self.y + 30,
                        dx / dist * spd, dy / dist * spd, 15))

    def take_damage(self, dmg):
        self.hp -= dmg
        self._hit_flash = 4
        if self.hp <= 0:
            self.alive = False

    def get_rect(self):
        return pygame.Rect(self.x - 44, self.y - 34, 88, 68)

# ---------------------------------------------------------------------------
# Wave Manager
# ---------------------------------------------------------------------------

class WaveManager:
    def __init__(self):
        self.wave = 0
        self.enemies_remaining = 0
        self.spawn_queue = []
        self.spawn_timer = 0
        self.spawn_interval = 40   # frames between spawns
        self.boss = None
        self.between_waves = True
        self.between_timer = 180   # 3 s pause between waves
        self.started = False

    def start(self):
        self.started = True
        self._next_wave()

    def _next_wave(self):
        self.wave += 1
        self.between_waves = False
        self.spawn_queue = self._build_queue()
        self.enemies_remaining = len(self.spawn_queue)
        self.spawn_timer = 0

    def _build_queue(self):
        queue = []
        w = self.wave
        # Every 5th wave → boss only
        if w % 5 == 0:
            return []   # boss handled separately

        basic = 4 + w * 2
        fast  = max(0, w - 1) * 2
        tank  = max(0, (w - 2)) * 1

        xs = [random.randint(40, WIDTH - 40) for _ in range(basic + fast + tank)]
        random.shuffle(xs)
        idx = 0
        for _ in range(basic):
            queue.append(("basic", xs[idx])); idx += 1
        for _ in range(fast):
            queue.append(("fast",  xs[idx])); idx += 1
        for _ in range(tank):
            queue.append(("tank",  xs[idx])); idx += 1
        random.shuffle(queue)
        return queue

    def update(self, enemies, enemy_bullets, player, particles):
        if not self.started:
            return

        # Boss wave
        if self.wave % 5 == 0 and self.boss is None and not self.between_waves:
            self.boss = Boss(self.wave)
            return

        if self.boss:
            self.boss.update(enemy_bullets, player)
            if not self.boss.alive:
                # Boss death explosion
                spawn_explosion(particles, self.boss.x, self.boss.y,
                                [RED, ORANGE, YELLOW, WHITE], 60, 9, 10)
                player.score += self.boss.score_value
                self.boss = None
                self._begin_between()
            return

        # Normal spawn
        if self.spawn_queue:
            self.spawn_timer -= 1
            if self.spawn_timer <= 0:
                kind, x = self.spawn_queue.pop(0)
                y = -40
                if kind == "basic":
                    enemies.append(BasicEnemy(x, y))
                elif kind == "fast":
                    enemies.append(FastEnemy(x, y))
                else:
                    enemies.append(TankEnemy(x, y))
                self.spawn_timer = self.spawn_interval

        # Check wave clear
        if not self.spawn_queue and not enemies:
            self._begin_between()

    def _begin_between(self):
        self.between_waves = True
        self.between_timer = 180

    def tick_between(self):
        """Call every frame when between_waves. Returns True when ready."""
        if not self.between_waves:
            return False
        self.between_timer -= 1
        if self.between_timer <= 0:
            self._next_wave()
            return True
        return False

    def wave_clear(self):
        return self.between_waves

# ---------------------------------------------------------------------------
# HUD
# ---------------------------------------------------------------------------

def draw_hud(surf, player, wave_mgr):
    # Score
    draw_text(surf, f"{player.score:,}", 26, WIDTH - 12, 20, CYAN, align="right")
    draw_text(surf, "SCORE", 13, WIDTH - 12, 40, GREY, align="right")

    # Wave
    draw_text(surf, f"WAVE  {wave_mgr.wave}", 22, 14, 20, WHITE, align="left")

    # HP bar
    hp_ratio = player.hp / player.MAX_HP
    bar_w, bar_h = 180, 14
    bx, by = 14, 50
    pygame.draw.rect(surf, GREY,  (bx, by, bar_w, bar_h), border_radius=6)
    hpc = GREEN if hp_ratio > 0.5 else (YELLOW if hp_ratio > 0.25 else RED)
    pygame.draw.rect(surf, hpc,   (bx, by, int(bar_w * hp_ratio), bar_h), border_radius=6)
    pygame.draw.rect(surf, WHITE, (bx, by, bar_w, bar_h), 2, border_radius=6)
    draw_text(surf, f"HP  {player.hp}", 13, bx + bar_w // 2, by + 7, WHITE)

    # Lives
    for i in range(player.lives):
        sx = 14 + i * 26
        sy = 74
        pts = [(sx + 10, sy), (sx, sy + 16), (sx + 20, sy + 16)]
        pygame.draw.polygon(surf, CYAN, pts)

    # Shield indicator
    if player.shield > 0:
        draw_text(surf, f"SHIELD x{player.shield}", 16, 14, 96, LBLUE, align="left")

    # Active power-ups
    pup_x = 14
    pup_y = 116
    if player.rapid_timer > 0:
        secs = player.rapid_timer // FPS
        draw_text(surf, f"RAPID  {secs}s", 14, pup_x, pup_y, YELLOW, align="left")
        pup_y += 18
    if player.triple_timer > 0:
        secs = player.triple_timer // FPS
        draw_text(surf, f"TRIPLE {secs}s", 14, pup_x, pup_y, PINK, align="left")

    # Wave banner
    if wave_mgr.between_waves and wave_mgr.between_timer > 120:
        alpha = min(255, (wave_mgr.between_timer - 120) * 8)
        banner = pygame.Surface((400, 60), pygame.SRCALPHA)
        banner.fill((0, 0, 0, 140))
        surf.blit(banner, (WIDTH // 2 - 200, HEIGHT // 2 - 30))
        draw_text(surf, f"WAVE  {wave_mgr.wave}  CLEAR!", 34,
                  WIDTH // 2, HEIGHT // 2, YELLOW)

# ---------------------------------------------------------------------------
# Screens
# ---------------------------------------------------------------------------

def main_menu(screen, clock):
    stars = StarField()
    frame = 0
    while True:
        clock.tick(FPS)
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit(); sys.exit()
            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_RETURN or event.key == pygame.K_SPACE:
                    return "play"
                if event.key == pygame.K_ESCAPE:
                    pygame.quit(); sys.exit()

        screen.fill(DARK)
        stars.update(); stars.draw(screen)

        # Pulsing title
        pulse = int(math.sin(frame * 0.05) * 8)
        draw_text(screen, "SPACE  SHOOTER", 62 + pulse // 4,
                  WIDTH // 2, HEIGHT // 2 - 140, CYAN)
        draw_text(screen, "SPACE  SHOOTER", 62 + pulse // 4,
                  WIDTH // 2 + 2, HEIGHT // 2 - 138, (0, 80, 120))    # shadow

        draw_text(screen, "PRESS  SPACE  TO  PLAY", 26,
                  WIDTH // 2, HEIGHT // 2 + 20, WHITE)
        draw_text(screen, "ESC to quit", 18,
                  WIDTH // 2, HEIGHT // 2 + 60, GREY)

        draw_text(screen, "WASD / ARROWS  —  move", 16,
                  WIDTH // 2, HEIGHT // 2 + 120, GREY)
        draw_text(screen, "SPACE  —  shoot      P  —  pause", 16,
                  WIDTH // 2, HEIGHT // 2 + 142, GREY)

        # Animated mini-ship
        angle = math.sin(frame * 0.04) * 12
        cx = WIDTH // 2 + int(math.sin(frame * 0.03) * 60)
        cy = HEIGHT // 2 - 260
        pts = [(cx, cy - 14), (cx - 12, cy + 10), (cx + 12, cy + 10)]
        pygame.draw.polygon(screen, CYAN, pts)

        pygame.display.flip()
        frame += 1


def game_over_screen(screen, clock, score, wave, high_score):
    stars = StarField()
    frame = 0
    while True:
        clock.tick(FPS)
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit(); sys.exit()
            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_RETURN or event.key == pygame.K_SPACE:
                    return "restart"
                if event.key == pygame.K_ESCAPE or event.key == pygame.K_m:
                    return "menu"

        screen.fill(DARK)
        stars.update(); stars.draw(screen)

        draw_text(screen, "GAME  OVER", 58, WIDTH // 2, HEIGHT // 2 - 170, RED)
        draw_text(screen, f"SCORE :  {score:,}",     28, WIDTH // 2, HEIGHT // 2 - 80, WHITE)
        draw_text(screen, f"WAVE  :  {wave}",         24, WIDTH // 2, HEIGHT // 2 - 44, WHITE)
        draw_text(screen, f"BEST  :  {high_score:,}", 22, WIDTH // 2, HEIGHT // 2 - 10, YELLOW)

        if score >= high_score and score > 0:
            pulse = int(abs(math.sin(frame * 0.08)) * 255)
            draw_text(screen, "NEW  HIGH  SCORE!", 28,
                      WIDTH // 2, HEIGHT // 2 + 40, (pulse, 255, 100))

        draw_text(screen, "SPACE / ENTER — restart", 20,
                  WIDTH // 2, HEIGHT // 2 + 110, WHITE)
        draw_text(screen, "M / ESC  —  main menu",  20,
                  WIDTH // 2, HEIGHT // 2 + 138, GREY)

        pygame.display.flip()
        frame += 1


def pause_screen(screen, clock):
    overlay = pygame.Surface((WIDTH, HEIGHT), pygame.SRCALPHA)
    overlay.fill((0, 0, 30, 170))
    screen.blit(overlay, (0, 0))
    draw_text(screen, "PAUSED", 54, WIDTH // 2, HEIGHT // 2 - 40, WHITE)
    draw_text(screen, "P — resume     ESC — quit", 22, WIDTH // 2, HEIGHT // 2 + 30, GREY)
    pygame.display.flip()
    while True:
        clock.tick(FPS)
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit(); sys.exit()
            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_p:
                    return "resume"
                if event.key == pygame.K_ESCAPE:
                    return "quit"

# ---------------------------------------------------------------------------
# Main game loop
# ---------------------------------------------------------------------------

def run_game(screen, clock):
    stars = StarField()
    player = Player()
    enemies = []
    player_bullets = []
    enemy_bullets = []
    powerups = []
    particles = []
    wave_mgr = WaveManager()
    wave_mgr.start()

    high_score = int(pygame.font.get_default_font() and
                     # We load HS from title caption trick – use a simple global
                     0)

    running = True
    while running:
        clock.tick(FPS)

        # --- Events ---
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit(); sys.exit()
            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_p:
                    result = pause_screen(screen, clock)
                    if result == "quit":
                        return player.score, wave_mgr.wave, True  # to menu

        keys = pygame.key.get_pressed()

        # --- Update ---
        stars.update()
        player.update(keys, player_bullets)

        for b in player_bullets:
            b.update()
        player_bullets = [b for b in player_bullets if b.alive]

        for b in enemy_bullets:
            b.update()
        enemy_bullets = [b for b in enemy_bullets if b.alive]

        # Enemy update
        for e in enemies:
            e.update(enemy_bullets, player)
        enemies = [e for e in enemies if e.alive]

        # Boss update (wave manager handles it)
        wave_mgr.update(enemies, enemy_bullets, player, particles)
        if wave_mgr.between_waves:
            wave_mgr.tick_between()

        # Power-ups
        for p in powerups:
            p.update()
        powerups = [p for p in powerups if p.alive]

        # Particles
        for pt in particles:
            pt.update()
        particles = [pt for pt in particles if pt.alive]

        # --- Collisions: player bullets vs enemies ---
        for b in player_bullets:
            if not b.alive:
                continue
            br = b.get_rect()
            # vs boss
            if wave_mgr.boss and wave_mgr.boss.alive:
                if br.colliderect(wave_mgr.boss.get_rect()):
                    wave_mgr.boss.take_damage(b.damage)
                    b.alive = False
                    spawn_explosion(particles, b.x, b.y, [ORANGE, YELLOW], 6, 3, 3)
            for e in enemies:
                if not e.alive:
                    continue
                if br.colliderect(e.get_rect()):
                    e.take_damage(b.damage)
                    b.alive = False
                    spawn_explosion(particles, b.x, b.y, [ORANGE, YELLOW], 6, 3, 3)
                    if not e.alive:
                        spawn_explosion(particles, e.x, e.y,
                                        [RED, ORANGE, YELLOW, WHITE], 22, 5, 6)
                        player.score += e.score_value
                        # Power-up drop chance
                        if random.random() < 0.22:
                            kind = random.choice(POWERUP_TYPES)
                            powerups.append(PowerUp(e.x, e.y, kind))
                    break

        # --- Collisions: enemy bullets vs player ---
        if player.alive and player._inv_timer == 0:
            for b in enemy_bullets:
                if not b.alive:
                    continue
                if b.get_rect().colliderect(player.get_rect()):
                    player.take_damage(b.damage)
                    b.alive = False
                    spawn_explosion(particles, b.x, b.y, [RED, WHITE], 8, 2, 3)

        # --- Collisions: enemies body vs player ---
        if player.alive:
            for e in enemies:
                if e.alive and e.get_rect().colliderect(player.get_rect()):
                    player.take_damage(25)
                    e.take_damage(999)
                    spawn_explosion(particles, e.x, e.y,
                                    [RED, ORANGE, YELLOW], 20, 5, 6)

        # --- Power-up collection ---
        if player.alive:
            for pu in powerups:
                if pu.alive and pu.get_rect().colliderect(player.get_rect()):
                    bomb = player.apply_powerup(pu.kind, particles)
                    pu.alive = False
                    if bomb:
                        for e in enemies:
                            spawn_explosion(particles, e.x, e.y,
                                            [ORANGE, YELLOW, RED], 18, 5, 5)
                            player.score += e.score_value
                            e.alive = False
                        enemies.clear()

        # --- Player death ---
        if not player.alive:
            player.lives -= 1
            if player.lives > 0:
                spawn_explosion(particles, player.x, player.y,
                                [CYAN, WHITE, YELLOW], 40, 7, 7)
                player.x = WIDTH // 2
                player.y = HEIGHT - 100
                player.hp = player.MAX_HP
                player.alive = True
                player._inv_timer = 150
            else:
                spawn_explosion(particles, player.x, player.y,
                                [CYAN, WHITE, YELLOW, RED], 60, 9, 9)
                running = False

        # --- Draw ---
        screen.fill(DARK)
        stars.draw(screen)

        for p in particles:
            p.draw(screen)
        for pu in powerups:
            pu.draw(screen)
        for e in enemies:
            e.draw(screen)
        if wave_mgr.boss and wave_mgr.boss.alive:
            wave_mgr.boss.draw(screen)
        for b in player_bullets:
            b.draw(screen)
        for b in enemy_bullets:
            b.draw(screen)
        if player.alive:
            player.draw(screen)

        draw_hud(screen, player, wave_mgr)
        pygame.display.flip()

    return player.score, wave_mgr.wave, False

# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

def main():
    pygame.init()
    pygame.display.set_caption(TITLE)
    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    clock = pygame.time.Clock()
    high_score = 0

    while True:
        result = main_menu(screen, clock)
        if result == "play":
            score, wave, to_menu = run_game(screen, clock)
            high_score = max(high_score, score)
            if to_menu:
                continue
            result = game_over_screen(screen, clock, score, wave, high_score)
            if result == "restart":
                continue          # loops back to game
            # else "menu" → loops back to main_menu

if __name__ == "__main__":
    main()
