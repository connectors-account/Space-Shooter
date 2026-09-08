"""
Space Shooter — Python + Pygame Desktop Game
=============================================
Controls:
  Arrow Keys / WASD  → Move
  SPACE              → Shoot
  P / ESC            → Pause
  Enter              → Confirm menus

Compile to .exe:
  pip install pygame pyinstaller
  pyinstaller --onefile --noconsole game.py
"""

import pygame
import random
import math
import sys
import os

# ─────────────────────────────────────────────
#  CONSTANTS
# ─────────────────────────────────────────────
SCREEN_W, SCREEN_H = 800, 900
FPS = 60
TITLE = "Space Shooter"

# Colours
BLACK      = (0,   0,   0)
WHITE      = (255, 255, 255)
RED        = (220,  50,  50)
DARK_RED   = (160,  20,  20)
GREEN      = ( 50, 220,  50)
DARK_GREEN = ( 20, 140,  20)
BLUE       = ( 60, 120, 255)
CYAN       = (  0, 220, 220)
YELLOW     = (255, 220,   0)
ORANGE     = (255, 140,   0)
PURPLE     = (180,  50, 255)
PINK       = (255, 100, 200)
GREY       = (120, 120, 120)
DARK_GREY  = ( 40,  40,  55)

# Layers (draw order)
Z_BG       = 0
Z_POWERUP  = 1
Z_ENEMY    = 2
Z_PLAYER   = 3
Z_BULLET   = 4
Z_FX       = 5
Z_HUD      = 6

# ─────────────────────────────────────────────
#  SOUND SYNTHESIS  (no external audio files)
# ─────────────────────────────────────────────
import numpy as np

def _make_sound(freq: float, duration: float, volume: float = 0.3,
                wave: str = "square", decay: float = 1.0) -> pygame.mixer.Sound:
    sample_rate = 22050
    n = int(sample_rate * duration)
    t = np.linspace(0, duration, n, endpoint=False)
    if wave == "square":
        data = np.sign(np.sin(2 * np.pi * freq * t))
    elif wave == "saw":
        data = 2 * (t * freq - np.floor(t * freq + 0.5))
    else:  # sine
        data = np.sin(2 * np.pi * freq * t)
    envelope = np.exp(-decay * t / duration * n / sample_rate)
    data = (data * envelope * volume * 32767).astype(np.int16)
    stereo = np.column_stack([data, data])
    return pygame.sndarray.make_sound(stereo)


class SoundBank:
    def __init__(self):
        pygame.mixer.pre_init(22050, -16, 2, 512)
        pygame.mixer.init()
        self.sfx = {
            "shoot":     _make_sound(880, 0.08, 0.25, "square", 8),
            "enemy_die": _make_sound(220, 0.25, 0.30, "saw",    4),
            "player_hit":_make_sound(150, 0.30, 0.35, "square", 3),
            "powerup":   _make_sound(660, 0.30, 0.35, "sine",   2),
            "boss_hit":  _make_sound(110, 0.15, 0.28, "saw",    6),
            "wave_start":_make_sound(440, 0.40, 0.30, "sine",   1),
            "game_over": _make_sound( 80, 0.80, 0.40, "saw",    1),
            "victory":   _make_sound(523, 0.60, 0.40, "sine",   1),
        }

    def play(self, name: str):
        s = self.sfx.get(name)
        if s:
            s.play()


# ─────────────────────────────────────────────
#  SPRITE FACTORIES  (no image files needed)
# ─────────────────────────────────────────────

def _surf(w: int, h: int, colour, alpha: int = 255) -> pygame.Surface:
    s = pygame.Surface((w, h), pygame.SRCALPHA)
    s.fill((*colour[:3], alpha))
    return s


def make_player_sprite() -> pygame.Surface:
    s = pygame.Surface((40, 48), pygame.SRCALPHA)
    # Body
    pygame.draw.polygon(s, (80, 200, 255), [(20, 0), (38, 44), (2, 44)])
    pygame.draw.polygon(s, (40, 120, 200), [(20, 6), (34, 44), (6, 44)])
    # Cockpit
    pygame.draw.ellipse(s, CYAN, (12, 12, 16, 14))
    # Engine glow
    pygame.draw.ellipse(s, ORANGE, (14, 40, 12, 8))
    return s


def make_enemy_sprite(kind: str) -> pygame.Surface:
    s = pygame.Surface((36, 36), pygame.SRCALPHA)
    if kind == "A":
        pygame.draw.polygon(s, RED, [(18, 36), (0, 4), (36, 4)])
        pygame.draw.polygon(s, DARK_RED, [(18, 30), (4, 6), (32, 6)])
        pygame.draw.circle(s, ORANGE, (18, 16), 5)
    elif kind == "B":
        pygame.draw.polygon(s, PURPLE, [(18, 36), (36, 18), (28, 0), (8, 0), (0, 18)])
        pygame.draw.circle(s, PINK, (18, 16), 6)
    else:  # C
        pygame.draw.circle(s, GREEN, (18, 18), 17)
        pygame.draw.circle(s, DARK_GREEN, (18, 18), 12)
        pygame.draw.circle(s, YELLOW, (18, 18), 6)
    return s


def make_boss_sprite() -> pygame.Surface:
    s = pygame.Surface((90, 70), pygame.SRCALPHA)
    pygame.draw.polygon(s, (200, 0, 50),
                        [(45, 70), (0, 20), (10, 0), (80, 0), (90, 20)])
    pygame.draw.polygon(s, (140, 0, 30),
                        [(45, 60), (8, 22), (15, 5), (75, 5), (82, 22)])
    pygame.draw.ellipse(s, ORANGE, (30, 20, 30, 22))
    pygame.draw.circle(s, RED, (20, 35), 8)
    pygame.draw.circle(s, RED, (70, 35), 8)
    return s


def make_bullet_sprite(who: str) -> pygame.Surface:
    if who == "player":
        s = pygame.Surface((6, 18), pygame.SRCALPHA)
        pygame.draw.ellipse(s, CYAN, (0, 0, 6, 18))
        pygame.draw.ellipse(s, WHITE, (1, 1, 4, 8))
    else:
        s = pygame.Surface((8, 8), pygame.SRCALPHA)
        pygame.draw.circle(s, ORANGE, (4, 4), 4)
        pygame.draw.circle(s, YELLOW, (4, 4), 2)
    return s


def make_powerup_sprite(kind: str) -> pygame.Surface:
    s = pygame.Surface((28, 28), pygame.SRCALPHA)
    colours = {"shield": CYAN, "triple": YELLOW, "speed": GREEN, "health": RED}
    letters = {"shield": "S", "triple": "T", "speed": "V", "health": "+"}
    pygame.draw.circle(s, colours.get(kind, WHITE), (14, 14), 13)
    pygame.draw.circle(s, BLACK, (14, 14), 13, 2)
    font = pygame.font.SysFont(None, 22, bold=True)
    txt = font.render(letters.get(kind, "?"), True, BLACK)
    s.blit(txt, txt.get_rect(center=(14, 14)))
    return s


# ─────────────────────────────────────────────
#  PARTICLE / EXPLOSION
# ─────────────────────────────────────────────

class Particle:
    def __init__(self, x: float, y: float, colour, speed: float,
                 angle: float, lifetime: int):
        self.x, self.y = x, y
        self.colour = colour
        rad = math.radians(angle)
        self.vx = math.cos(rad) * speed
        self.vy = math.sin(rad) * speed
        self.lifetime = self.max_life = lifetime
        self.radius = random.randint(2, 5)

    def update(self):
        self.x += self.vx
        self.y += self.vy
        self.vy += 0.05        # slight gravity
        self.lifetime -= 1

    def draw(self, surface: pygame.Surface):
        alpha = int(255 * self.lifetime / self.max_life)
        r = max(1, int(self.radius * self.lifetime / self.max_life))
        col = (*self.colour[:3], alpha)
        tmp = pygame.Surface((r*2, r*2), pygame.SRCALPHA)
        pygame.draw.circle(tmp, col, (r, r), r)
        surface.blit(tmp, (int(self.x) - r, int(self.y) - r))

    @property
    def alive(self) -> bool:
        return self.lifetime > 0


def spawn_explosion(particles: list, x: float, y: float,
                    colour=(255, 160, 40), count: int = 18):
    for _ in range(count):
        angle = random.uniform(0, 360)
        speed = random.uniform(1.5, 5.0)
        life  = random.randint(20, 45)
        c = (
            min(255, colour[0] + random.randint(-30, 30)),
            min(255, colour[1] + random.randint(-30, 30)),
            max(0,   colour[2] + random.randint(-30, 30)),
        )
        particles.append(Particle(x, y, c, speed, angle, life))


# ─────────────────────────────────────────────
#  PARALLAX BACKGROUND
# ─────────────────────────────────────────────

class StarLayer:
    def __init__(self, count: int, speed: float, size_range, colour):
        self.speed = speed
        self.stars = [
            [random.randint(0, SCREEN_W),
             random.randint(0, SCREEN_H),
             random.randint(*size_range),
             random.uniform(0.4, 1.0)]
            for _ in range(count)
        ]
        self.colour = colour

    def update(self):
        for s in self.stars:
            s[1] += self.speed
            if s[1] > SCREEN_H:
                s[1] = 0
                s[0] = random.randint(0, SCREEN_W)

    def draw(self, surface: pygame.Surface):
        for x, y, r, bright in self.stars:
            c = tuple(int(ch * bright) for ch in self.colour)
            pygame.draw.circle(surface, c, (int(x), int(y)), r)


# ─────────────────────────────────────────────
#  BULLETS
# ─────────────────────────────────────────────

class Bullet(pygame.sprite.Sprite):
    def __init__(self, x: float, y: float, vx: float, vy: float,
                 damage: int, owner: str, image: pygame.Surface):
        super().__init__()
        self.image = image
        self.rect  = self.image.get_rect(center=(x, y))
        self.vx, self.vy = vx, vy
        self.damage = damage
        self.owner  = owner   # "player" or "enemy"
        self._fx = float(x)
        self._fy = float(y)

    def update(self):
        self._fx += self.vx
        self._fy += self.vy
        self.rect.centerx = int(self._fx)
        self.rect.centery = int(self._fy)
        if (self._fy < -20 or self._fy > SCREEN_H + 20 or
                self._fx < -20 or self._fx > SCREEN_W + 20):
            self.kill()


# ─────────────────────────────────────────────
#  POWER-UPS
# ─────────────────────────────────────────────

POWERUP_TYPES = ["shield", "triple", "speed", "health"]

class PowerUp(pygame.sprite.Sprite):
    def __init__(self, x: float, y: float, kind: str):
        super().__init__()
        self.kind  = kind
        self.image = make_powerup_sprite(kind)
        self.rect  = self.image.get_rect(center=(x, y))
        self._fy   = float(y)
        self._angle = 0.0

    def update(self):
        self._fy += 1.5
        self.rect.centery = int(self._fy)
        self._angle = (self._angle + 2) % 360
        if self._fy > SCREEN_H + 30:
            self.kill()


# ─────────────────────────────────────────────
#  PLAYER
# ─────────────────────────────────────────────

class Player(pygame.sprite.Sprite):
    BASE_SPEED     = 5.5
    MAX_HP         = 5
    SHOOT_COOLDOWN = 12          # frames
    INVINCIBLE_T   = 120         # frames

    def __init__(self, sounds: SoundBank, bullets: pygame.sprite.Group):
        super().__init__()
        self._base_img = make_player_sprite()
        self.image     = self._base_img.copy()
        self.rect      = self.image.get_rect(center=(SCREEN_W // 2, SCREEN_H - 80))
        self._fx       = float(self.rect.centerx)
        self._fy       = float(self.rect.centery)

        self.sounds   = sounds
        self.bullets  = bullets
        self.b_img    = make_bullet_sprite("player")

        self.hp         = self.MAX_HP
        self.score      = 0
        self.speed      = self.BASE_SPEED
        self.fire_mode  = "single"    # "single" | "triple"
        self.has_shield = False

        self._shoot_cd   = 0
        self._inv_timer  = 0
        self._speed_timer = 0
        self._triple_timer = 0
        self._tilt       = 0.0       # sprite tilt angle

    # ── input ─────────────────────────────────
    def handle_input(self, keys):
        dx = dy = 0
        if keys[pygame.K_LEFT]  or keys[pygame.K_a]: dx -= 1
        if keys[pygame.K_RIGHT] or keys[pygame.K_d]: dx += 1
        if keys[pygame.K_UP]    or keys[pygame.K_w]: dy -= 1
        if keys[pygame.K_DOWN]  or keys[pygame.K_s]: dy += 1

        # Normalise diagonal
        if dx and dy:
            dx *= 0.707
            dy *= 0.707

        self._fx += dx * self.speed
        self._fy += dy * self.speed
        self._fx = max(20, min(SCREEN_W - 20, self._fx))
        self._fy = max(20, min(SCREEN_H - 20, self._fy))

        # Tilt on horizontal movement
        target_tilt = -dx * 15
        self._tilt += (target_tilt - self._tilt) * 0.18

        if keys[pygame.K_SPACE]:
            self._try_shoot()

    def _try_shoot(self):
        if self._shoot_cd > 0:
            return
        spd = -14
        bx, by = int(self._fx), int(self._fy) - 24
        if self.fire_mode == "triple":
            for angle_off in (-15, 0, 15):
                rad = math.radians(angle_off)
                vx = math.sin(rad) * 10
                self.bullets.add(
                    Bullet(bx, by, vx, spd * math.cos(rad),
                           1, "player", self.b_img.copy()))
            self._shoot_cd = 16
        else:
            self.bullets.add(Bullet(bx, by, 0, spd, 1, "player", self.b_img.copy()))
            self._shoot_cd = self.SHOOT_COOLDOWN
        self.sounds.play("shoot")

    # ── damage / buffs ─────────────────────────
    def take_damage(self, amount: int):
        if self._inv_timer > 0:
            return
        if self.has_shield:
            self.has_shield = False
            self._inv_timer = 60
            return
        self.hp -= amount
        self._inv_timer = self.INVINCIBLE_T
        self.sounds.play("player_hit")

    def apply_powerup(self, kind: str):
        self.sounds.play("powerup")
        if kind == "shield":
            self.has_shield = True
        elif kind == "triple":
            self.fire_mode = "triple"
            self._triple_timer = 600           # 10 s at 60 fps
        elif kind == "speed":
            self.speed = self.BASE_SPEED * 1.6
            self._speed_timer = 480            # 8 s
        elif kind == "health":
            self.hp = min(self.MAX_HP, self.hp + 1)

    def add_score(self, pts: int):
        self.score += pts

    # ── update ────────────────────────────────
    def update(self):
        self.rect.centerx = int(self._fx)
        self.rect.centery = int(self._fy)
        if self._shoot_cd   > 0: self._shoot_cd   -= 1
        if self._inv_timer  > 0: self._inv_timer  -= 1

        # Buff timers
        if self._triple_timer > 0:
            self._triple_timer -= 1
            if self._triple_timer == 0:
                self.fire_mode = "single"
        if self._speed_timer > 0:
            self._speed_timer -= 1
            if self._speed_timer == 0:
                self.speed = self.BASE_SPEED

        # Rebuild rotated image
        rotated = pygame.transform.rotate(self._base_img, self._tilt)
        self.image = rotated
        self.rect  = rotated.get_rect(center=(int(self._fx), int(self._fy)))

    def draw_extras(self, surface: pygame.Surface):
        # Invincibility flicker
        if self._inv_timer > 0 and (self._inv_timer // 5) % 2 == 0:
            return   # skip draw → flicker effect

        # Shield ring
        if self.has_shield:
            pygame.draw.circle(surface, CYAN,
                                (int(self._fx), int(self._fy)), 30, 3)

    @property
    def alive_check(self) -> bool:
        return self.hp > 0

    @property
    def triple_pct(self) -> float:
        return self._triple_timer / 600 if self._triple_timer > 0 else 0

    @property
    def speed_pct(self) -> float:
        return self._speed_timer / 480 if self._speed_timer > 0 else 0


# ─────────────────────────────────────────────
#  ENEMIES
# ─────────────────────────────────────────────

class Enemy(pygame.sprite.Sprite):
    def __init__(self, kind: str, x: float, y: float,
                 sounds: SoundBank, bullets: pygame.sprite.Group,
                 powerups: pygame.sprite.Group, particles: list):
        super().__init__()
        self.kind     = kind
        self.sounds   = sounds
        self.bullets  = bullets
        self.powerups = powerups
        self.particles = particles

        cfg = {
            "A": dict(hp=1, score=100, speed=2.0, shoot_cd=180, colour=(220, 80, 80)),
            "B": dict(hp=2, score=200, speed=2.8, shoot_cd=140, colour=(180, 80, 255)),
            "C": dict(hp=3, score=350, speed=1.8, shoot_cd=110, colour=(80, 220, 80)),
        }[kind]

        self.hp        = cfg["hp"]
        self.score_val = cfg["score"]
        self.speed     = cfg["speed"]
        self._shoot_cd = random.randint(0, cfg["shoot_cd"])
        self._max_cd   = cfg["shoot_cd"]
        self.colour    = cfg["colour"]

        self.image  = make_enemy_sprite(kind)
        self.rect   = self.image.get_rect(center=(x, y))
        self._fx    = float(x)
        self._fy    = float(y)
        self._t     = 0.0          # time accumulator for patterns
        self._ox    = float(x)     # origin x for sine/circle
        self._oy    = float(y)

        self.b_img  = make_bullet_sprite("enemy")

    def update(self):
        self._t += 0.04

        if self.kind == "A":
            self._fy += self.speed
        elif self.kind == "B":
            self._fy += self.speed * 0.7
            self._fx = self._ox + math.sin(self._t * 2) * 80
        elif self.kind == "C":
            self._fy += self.speed * 0.4
            self._fx = self._ox + math.cos(self._t) * 60
            self._fy_circ = self._oy + math.sin(self._t) * 30
            self.rect.centery = int(self._fy + math.sin(self._t * 1.5) * 20)

        self.rect.centerx = int(self._fx)
        self.rect.centery = int(self._fy)

        if self._fy > SCREEN_H + 50:
            self.kill()

        # Shooting
        self._shoot_cd -= 1
        if self._shoot_cd <= 0:
            self._shoot()
            self._shoot_cd = self._max_cd

    def _shoot(self):
        bx, by = self.rect.centerx, self.rect.bottom
        spd = 5
        if self.kind == "A":
            self.bullets.add(Bullet(bx, by, 0, spd, 1, "enemy", self.b_img.copy()))
        elif self.kind == "B":
            # Aimed at player (approximated — we just shoot down-left & down-right spread)
            for vx in (-2, 0, 2):
                self.bullets.add(Bullet(bx, by, vx, spd, 1, "enemy", self.b_img.copy()))
        elif self.kind == "C":
            for angle in (270, 300, 330, 0, 30, 60, 90):
                rad = math.radians(angle)
                self.bullets.add(
                    Bullet(bx, by, math.cos(rad)*spd, math.sin(rad)*spd,
                           1, "enemy", self.b_img.copy()))
        self.sounds.play("enemy_die") if self.hp == 0 else None

    def take_damage(self, amount: int) -> bool:
        self.hp -= amount
        spawn_explosion(self.particles, self._fx, self._fy, self.colour, 8)
        if self.hp <= 0:
            self._on_death()
            return True
        return False

    def _on_death(self):
        self.sounds.play("enemy_die")
        spawn_explosion(self.particles, self._fx, self._fy, self.colour, 22)
        if random.random() < 0.20:
            kind = random.choice(POWERUP_TYPES)
            self.powerups.add(PowerUp(self._fx, self._fy, kind))
        self.kill()


# ─────────────────────────────────────────────
#  BOSS
# ─────────────────────────────────────────────

class Boss(pygame.sprite.Sprite):
    MAX_HP   = 80
    SCORE    = 5000

    def __init__(self, sounds: SoundBank, bullets: pygame.sprite.Group,
                 particles: list):
        super().__init__()
        self.sounds    = sounds
        self.bullets   = bullets
        self.particles = particles

        self.hp      = self.MAX_HP
        self._phase  = 1
        self._dir    = 1
        self._speed  = 2.0
        self._t      = 0.0
        self._shoot_cd   = 0
        self._aimed_cd   = 0

        self.image = make_boss_sprite()
        self.rect  = self.image.get_rect(center=(SCREEN_W // 2, 100))
        self._fx   = float(self.rect.centerx)
        self._fy   = float(self.rect.centery)
        self.b_img = make_bullet_sprite("enemy")

        self._entering = True
        self._entry_y  = -60.0

    # target_x/y injected by game loop
    def update(self, player_x: float = SCREEN_W // 2, player_y: float = SCREEN_H // 2):
        # Entry slide-in
        if self._entering:
            self._entry_y += 1.2
            self.rect.centery = int(self._entry_y)
            if self._entry_y >= 100:
                self._entering = False
            return

        self._t += 0.02

        # Phase check
        if self.hp <= self.MAX_HP // 2 and self._phase == 1:
            self._phase = 2
            self._speed = 3.2
            spawn_explosion(self.particles, self._fx, self._fy, (255, 50, 50), 40)

        # Movement
        self._fx += self._dir * self._speed
        if self._fx > SCREEN_W - 60 or self._fx < 60:
            self._dir *= -1

        self.rect.centerx = int(self._fx)
        self.rect.centery = int(self._fy)

        # Shooting
        self._shoot_cd -= 1
        cd_limit = 70 if self._phase == 2 else 100
        if self._shoot_cd <= 0:
            self._fire_spread()
            self._shoot_cd = cd_limit

        if self._phase == 2:
            self._aimed_cd -= 1
            if self._aimed_cd <= 0:
                self._fire_aimed(player_x, player_y)
                self._aimed_cd = 55

    def _fire_spread(self):
        count = 5 if self._phase == 2 else 3
        bx, by = self.rect.centerx, self.rect.bottom
        spread = 40
        step   = spread / max(count - 1, 1)
        for i in range(count):
            angle_off = -spread / 2 + i * step
            rad = math.radians(90 + angle_off)     # downward base
            spd = 6
            self.bullets.add(
                Bullet(bx, by, math.cos(rad)*spd, math.sin(rad)*spd,
                       1, "enemy", self.b_img.copy()))
        self.sounds.play("boss_hit")

    def _fire_aimed(self, px: float, py: float):
        bx, by = self.rect.centerx, self.rect.bottom
        dx, dy = px - bx, py - by
        dist = math.hypot(dx, dy) or 1
        spd  = 7
        self.bullets.add(
            Bullet(bx, by, dx/dist*spd, dy/dist*spd, 1, "enemy", self.b_img.copy()))

    def take_damage(self, amount: int) -> bool:
        self.hp -= amount
        spawn_explosion(self.particles, self._fx, self._fy, (255, 100, 50), 10)
        self.sounds.play("boss_hit")
        if self.hp <= 0:
            spawn_explosion(self.particles, self._fx, self._fy, (255, 200, 50), 60)
            self.sounds.play("enemy_die")
            self.kill()
            return True
        return False

    @property
    def hp_pct(self) -> float:
        return self.hp / self.MAX_HP


# ─────────────────────────────────────────────
#  WAVE DEFINITIONS
# ─────────────────────────────────────────────

WAVE_DATA = [
    # (list of (kind, count), is_boss)
    ([("A", 5)],                      False),   # 1
    ([("A", 8)],                      False),   # 2
    ([("A", 5), ("B", 3)],            False),   # 3
    ([("B", 6)],                      False),   # 4
    ([],                               True),    # 5  BOSS
    ([("A", 6), ("B", 4), ("C", 2)],  False),   # 6
    ([("B", 8), ("C", 4)],            False),   # 7
    ([("C", 10)],                     False),   # 8
    ([("A", 5), ("B", 5), ("C", 5)],  False),   # 9
    ([],                               True),    # 10 BOSS harder
]


# ─────────────────────────────────────────────
#  SCREEN-SHAKE
# ─────────────────────────────────────────────

class CameraShake:
    def __init__(self):
        self._duration  = 0
        self._magnitude = 0.0
        self.offset = (0, 0)

    def start(self, duration: int, magnitude: float):
        self._duration  = duration
        self._magnitude = magnitude

    def update(self):
        if self._duration > 0:
            self._duration -= 1
            ox = random.uniform(-self._magnitude, self._magnitude)
            oy = random.uniform(-self._magnitude, self._magnitude)
            self.offset = (int(ox), int(oy))
        else:
            self.offset = (0, 0)


# ─────────────────────────────────────────────
#  UI HELPERS
# ─────────────────────────────────────────────

def draw_bar(surface, x, y, w, h, pct, fg, bg=DARK_GREY, border=WHITE):
    pygame.draw.rect(surface, bg,     (x, y, w, h))
    pygame.draw.rect(surface, fg,     (x, y, int(w * pct), h))
    pygame.draw.rect(surface, border, (x, y, w, h), 2)


def draw_text(surface, text, size, colour, cx, cy, bold=False):
    font = pygame.font.SysFont(None, size, bold=bold)
    img  = font.render(str(text), True, colour)
    r    = img.get_rect(center=(cx, cy))
    surface.blit(img, r)
    return r


def draw_text_left(surface, text, size, colour, x, y):
    font = pygame.font.SysFont(None, size)
    img  = font.render(str(text), True, colour)
    surface.blit(img, (x, y))


# ─────────────────────────────────────────────
#  MAIN GAME CLASS
# ─────────────────────────────────────────────

class SpaceShooter:
    # States
    MAIN_MENU  = "menu"
    PLAYING    = "playing"
    PAUSED     = "paused"
    WAVE_INTRO = "wave_intro"
    GAME_OVER  = "gameover"
    VICTORY    = "victory"

    def __init__(self):
        pygame.init()
        self.screen  = pygame.display.set_mode((SCREEN_W, SCREEN_H))
        pygame.display.set_caption(TITLE)
        self.clock   = pygame.time.Clock()
        self.sounds  = SoundBank()
        self._hi     = int(pygame.display.get_wm_info().get("window", 0))  # unused
        self._high_score = 0
        self._load_high_score()
        self.state   = self.MAIN_MENU
        self._menu_anim = 0.0
        self._go_anim   = 0
        self._score_display = 0

        # Background layers
        self.bg_layers = [
            StarLayer(300, 0.4, (1, 1), WHITE),
            StarLayer(150, 1.0, (1, 2), (200, 220, 255)),
            StarLayer(60,  2.0, (2, 3), (255, 240, 180)),
        ]

        self.shake = CameraShake()
        self._setup_game()

    # ── persistence ───────────────────────────
    def _load_high_score(self):
        try:
            path = os.path.join(os.path.dirname(__file__), "highscore.txt")
            with open(path) as f:
                self._high_score = int(f.read().strip())
        except Exception:
            self._high_score = 0

    def _save_high_score(self, score: int):
        if score > self._high_score:
            self._high_score = score
            try:
                path = os.path.join(os.path.dirname(__file__), "highscore.txt")
                with open(path, "w") as f:
                    f.write(str(score))
            except Exception:
                pass

    # ── setup / reset ─────────────────────────
    def _setup_game(self):
        self.all_sprites = pygame.sprite.LayeredUpdates()
        self.enemies     = pygame.sprite.Group()
        self.boss_group  = pygame.sprite.Group()
        self.bullets     = pygame.sprite.Group()
        self.powerups    = pygame.sprite.Group()
        self.particles   = []

        self.player = Player(self.sounds, self.bullets)
        self.all_sprites.add(self.player, layer=Z_PLAYER)

        self.wave_idx    = 0
        self._spawn_queue = []
        self._spawn_timer = 0
        self._wave_msg    = ""
        self._wave_msg_t  = 0
        self._boss        = None
        self._is_boss_wave = False

        self._start_wave()

    def _start_wave(self):
        if self.wave_idx >= len(WAVE_DATA):
            self.state = self.VICTORY
            self._save_high_score(self.player.score)
            self.sounds.play("victory")
            return

        spawns, is_boss = WAVE_DATA[self.wave_idx]
        self._is_boss_wave = is_boss
        self._wave_msg   = f"WAVE {self.wave_idx + 1}" + (" — BOSS!" if is_boss else "")
        self._wave_msg_t = 160         # frames to show message
        self.state       = self.WAVE_INTRO
        self.sounds.play("wave_start")

        # Build spawn queue
        self._spawn_queue = []
        if not is_boss:
            for kind, count in spawns:
                for _ in range(count):
                    x = random.randint(50, SCREEN_W - 50)
                    y = random.randint(-400, -40)
                    self._spawn_queue.append((kind, x, y))
            random.shuffle(self._spawn_queue)
        self._spawn_timer = 0

    def _spawn_next(self):
        if not self._spawn_queue:
            return
        kind, x, y = self._spawn_queue.pop(0)
        e = Enemy(kind, x, y, self.sounds, self.bullets,
                  self.powerups, self.particles)
        self.enemies.add(e)
        self.all_sprites.add(e, layer=Z_ENEMY)

    def _spawn_boss(self):
        self._boss = Boss(self.sounds, self.bullets, self.particles)
        if self.wave_idx == 9:           # wave 10 — tougher boss
            self._boss.hp = int(Boss.MAX_HP * 1.6)
        self.boss_group.add(self._boss)
        self.all_sprites.add(self._boss, layer=Z_ENEMY)
        self.shake.start(30, 6)

    def _next_wave(self):
        self.wave_idx += 1
        self._start_wave()

    # ── main loop ─────────────────────────────
    def run(self):
        running = True
        while running:
            dt = self.clock.tick(FPS)
            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    running = False
                self._handle_event(event)

            self._update()
            self._draw()
            pygame.display.flip()

        pygame.quit()
        sys.exit()

    def _handle_event(self, event):
        if event.type != pygame.KEYDOWN:
            return
        k = event.key

        if self.state == self.MAIN_MENU:
            if k == pygame.K_RETURN or k == pygame.K_SPACE:
                self._setup_game()
                self.state = self.WAVE_INTRO
            elif k == pygame.K_ESCAPE:
                pygame.quit(); sys.exit()

        elif self.state == self.PLAYING:
            if k in (pygame.K_p, pygame.K_ESCAPE):
                self.state = self.PAUSED

        elif self.state == self.PAUSED:
            if k in (pygame.K_p, pygame.K_ESCAPE):
                self.state = self.PLAYING
            elif k == pygame.K_r:
                self._setup_game()
            elif k == pygame.K_m:
                self.state = self.MAIN_MENU

        elif self.state in (self.GAME_OVER, self.VICTORY):
            if k == pygame.K_RETURN:
                self._setup_game()
                self.state = self.WAVE_INTRO
            elif k == pygame.K_m:
                self.state = self.MAIN_MENU

    def _update(self):
        # Always update background
        for layer in self.bg_layers:
            layer.update()
        self.shake.update()

        if self.state == self.WAVE_INTRO:
            self._wave_msg_t -= 1
            if self._wave_msg_t <= 0:
                self.state = self.PLAYING
                if self._is_boss_wave:
                    self._spawn_boss()

        elif self.state == self.PLAYING:
            keys = pygame.key.get_pressed()
            self.player.handle_input(keys)
            self.player.update()

            # Enemy spawning
            if self._spawn_queue:
                self._spawn_timer -= 1
                if self._spawn_timer <= 0:
                    self._spawn_next()
                    self._spawn_timer = 35

            # Update enemies
            for e in list(self.enemies):
                e.update()

            # Update boss
            if self._boss and self._boss.alive():
                self._boss.update(self.player.rect.centerx, self.player.rect.centery)
            elif self._boss and not self._boss.alive():
                self.player.add_score(Boss.SCORE)
                self._boss = None
                self.shake.start(20, 4)

            # Update bullets + powerups
            self.bullets.update()
            self.powerups.update()

            # Particles
            for p in self.particles: p.update()
            self.particles = [p for p in self.particles if p.alive]

            # ── Collisions ─────────────────────────
            # Player bullets → enemies
            hits = pygame.sprite.groupcollide(
                self.enemies, self.bullets, False, True,
                pygame.sprite.collide_rect)
            for enemy, blist in hits.items():
                for b in blist:
                    killed = enemy.take_damage(b.damage)
                    if killed:
                        self.player.add_score(enemy.score_val)

            # Player bullets → boss
            boss_hits = pygame.sprite.groupcollide(
                self.boss_group, self.bullets, False, True,
                pygame.sprite.collide_circle)
            for boss, blist in boss_hits.items():
                for b in blist:
                    boss.take_damage(b.damage)

            # Enemy bullets → player
            for b in list(self.bullets):
                if b.owner == "enemy" and b.rect.colliderect(self.player.rect):
                    self.player.take_damage(b.damage)
                    b.kill()
                    self.shake.start(8, 3)

            # Enemy → player collision
            for e in list(self.enemies):
                if e.rect.colliderect(self.player.rect):
                    self.player.take_damage(1)
                    e.take_damage(e.hp)
                    self.shake.start(8, 3)

            # Powerup → player
            for pu in list(self.powerups):
                if pu.rect.colliderect(self.player.rect):
                    self.player.apply_powerup(pu.kind)
                    pu.kill()

            # Wave complete?
            wave_done = (
                not self._spawn_queue and
                len(self.enemies) == 0 and
                (not self._is_boss_wave or
                 (self._boss is None and len(self.boss_group) == 0))
            )
            if wave_done:
                self._next_wave()

            # Player dead?
            if not self.player.alive_check:
                self._save_high_score(self.player.score)
                self._score_display = 0
                self.state = self.GAME_OVER
                self.sounds.play("game_over")
                self.shake.start(40, 8)

        elif self.state == self.GAME_OVER:
            # Animated score count-up
            target = self.player.score
            if self._score_display < target:
                self._score_display += max(1, (target - self._score_display) // 20)
            for p in self.particles: p.update()
            self.particles = [p for p in self.particles if p.alive]
            self._go_anim += 1

        elif self.state == self.VICTORY:
            for p in self.particles: p.update()
            self.particles = [p for p in self.particles if p.alive]

        self._menu_anim += 0.03

    # ── DRAW ──────────────────────────────────
    def _draw(self):
        ox, oy = self.shake.offset
        surf = self.screen

        # Background
        surf.fill((5, 5, 20))
        for layer in self.bg_layers:
            layer.draw(surf)

        if self.state == self.MAIN_MENU:
            self._draw_menu(surf)
        elif self.state == self.WAVE_INTRO:
            self._draw_wave_intro(surf)
        elif self.state == self.PLAYING:
            self._draw_game(surf, ox, oy)
        elif self.state == self.PAUSED:
            self._draw_game(surf, 0, 0)
            self._draw_pause(surf)
        elif self.state == self.GAME_OVER:
            self._draw_game_over(surf)
        elif self.state == self.VICTORY:
            self._draw_victory(surf)

    def _draw_game(self, surf, ox, oy):
        # Particles
        for p in self.particles:
            p.draw(surf)

        # Sprites
        for e in self.enemies:
            surf.blit(e.image, (e.rect.x + ox, e.rect.y + oy))
        for b in self.boss_group:
            surf.blit(b.image, (b.rect.x + ox, b.rect.y + oy))
        for b in self.bullets:
            surf.blit(b.image, (b.rect.x + ox, b.rect.y + oy))
        for pu in self.powerups:
            surf.blit(pu.image, (pu.rect.x + ox, pu.rect.y + oy))

        # Player (with optional skip for invincibility flicker)
        inv = self.player._inv_timer
        if inv == 0 or (inv // 5) % 2 == 1:
            surf.blit(self.player.image,
                      (self.player.rect.x + ox, self.player.rect.y + oy))
        if self.player.has_shield:
            pygame.draw.circle(surf, CYAN,
                                (self.player.rect.centerx + ox,
                                 self.player.rect.centery + oy), 30, 3)

        # HUD
        self._draw_hud(surf)

    def _draw_hud(self, surf):
        # Score
        draw_text(surf, f"SCORE: {self.player.score}", 30, WHITE,
                  SCREEN_W - 120, 22)
        draw_text(surf, f"BEST: {self._high_score}", 24, GREY,
                  SCREEN_W - 120, 44)

        # Wave
        draw_text_left(surf, f"WAVE {self.wave_idx + 1}", 28, CYAN, 10, 10)

        # HP bar
        draw_text_left(surf, "HP", 22, WHITE, 10, 40)
        draw_bar(surf, 40, 42, 120, 14,
                 self.player.hp / self.player.MAX_HP, RED)

        # Shield icon
        if self.player.has_shield:
            draw_text_left(surf, "  SHIELD", 22, CYAN, 10, 60)

        # Buff timers
        y = SCREEN_H - 30
        if self.player.triple_pct > 0:
            draw_text_left(surf, "TRIPLE", 20, YELLOW, 10, y)
            draw_bar(surf, 70, y + 3, 80, 10, self.player.triple_pct, YELLOW)
            y -= 22
        if self.player.speed_pct > 0:
            draw_text_left(surf, "SPEED ", 20, GREEN, 10, y)
            draw_bar(surf, 70, y + 3, 80, 10, self.player.speed_pct, GREEN)

        # Boss HP bar
        if self._boss and self._boss.alive():
            draw_text(surf, "BOSS", 26, RED, SCREEN_W // 2, SCREEN_H - 44)
            draw_bar(surf, SCREEN_W // 4, SCREEN_H - 30,
                     SCREEN_W // 2, 18,
                     self._boss.hp_pct, RED, bg=(80, 0, 0))

    def _draw_menu(self, surf):
        # Floating title
        ty = SCREEN_H // 3 + int(math.sin(self._menu_anim) * 8)
        draw_text(surf, TITLE,        64, CYAN,  SCREEN_W // 2, ty,        bold=True)
        draw_text(surf, "PRESS ENTER TO PLAY", 36, WHITE,
                  SCREEN_W // 2, ty + 80)
        draw_text(surf, "ESC to Quit",         24, GREY,
                  SCREEN_W // 2, ty + 120)
        draw_text(surf, f"HIGH SCORE: {self._high_score}", 30, YELLOW,
                  SCREEN_W // 2, ty + 170)

        draw_text(surf, "CONTROLS", 26, WHITE, SCREEN_W // 2, ty + 230)
        lines = [
            "Arrow Keys / WASD  — Move",
            "SPACE              — Shoot",
            "P / ESC            — Pause",
        ]
        for i, line in enumerate(lines):
            draw_text(surf, line, 22, GREY, SCREEN_W // 2, ty + 258 + i * 24)

        draw_text(surf, "v1.0", 18, DARK_GREY, SCREEN_W - 26, SCREEN_H - 14)

    def _draw_wave_intro(self, surf):
        alpha = min(255, self._wave_msg_t * 4)
        overlay = pygame.Surface((SCREEN_W, SCREEN_H), pygame.SRCALPHA)
        overlay.fill((0, 0, 0, 120))
        surf.blit(overlay, (0, 0))
        col = RED if "BOSS" in self._wave_msg else CYAN
        draw_text(surf, self._wave_msg, 60, col, SCREEN_W // 2, SCREEN_H // 2, bold=True)
        draw_text(surf, "GET READY!", 32, WHITE, SCREEN_W // 2, SCREEN_H // 2 + 56)

    def _draw_pause(self, surf):
        overlay = pygame.Surface((SCREEN_W, SCREEN_H), pygame.SRCALPHA)
        overlay.fill((0, 0, 0, 160))
        surf.blit(overlay, (0, 0))
        draw_text(surf, "PAUSED",         54, WHITE, SCREEN_W // 2, SCREEN_H // 2 - 60, bold=True)
        draw_text(surf, f"Score: {self.player.score}", 32, YELLOW,
                  SCREEN_W // 2, SCREEN_H // 2)
        draw_text(surf, "P — Resume    R — Restart    M — Menu",
                  26, GREY, SCREEN_W // 2, SCREEN_H // 2 + 50)

    def _draw_game_over(self, surf):
        # Particles still draw
        for p in self.particles:
            p.draw(surf)
        overlay = pygame.Surface((SCREEN_W, SCREEN_H), pygame.SRCALPHA)
        overlay.fill((0, 0, 0, 180))
        surf.blit(overlay, (0, 0))

        flash = abs(math.sin(self._go_anim * 0.05))
        col   = (int(200 + 55 * flash), 0, 0)
        draw_text(surf, "GAME OVER", 72, col, SCREEN_W // 2, SCREEN_H // 2 - 80, bold=True)
        draw_text(surf, f"Score: {self._score_display}", 40, WHITE,
                  SCREEN_W // 2, SCREEN_H // 2)

        new_best = self.player.score >= self._high_score and self.player.score > 0
        if new_best:
            bc = YELLOW if (self._go_anim // 10) % 2 == 0 else ORANGE
            draw_text(surf, "NEW HIGH SCORE!", 30, bc,
                      SCREEN_W // 2, SCREEN_H // 2 + 44)
        else:
            draw_text(surf, f"Best: {self._high_score}", 28, GREY,
                      SCREEN_W // 2, SCREEN_H // 2 + 44)

        draw_text(surf, "ENTER — Restart    M — Menu",
                  26, WHITE, SCREEN_W // 2, SCREEN_H // 2 + 100)

    def _draw_victory(self, surf):
        for p in self.particles:
            p.draw(surf)
        overlay = pygame.Surface((SCREEN_W, SCREEN_H), pygame.SRCALPHA)
        overlay.fill((0, 0, 0, 150))
        surf.blit(overlay, (0, 0))
        draw_text(surf, "VICTORY!", 72, YELLOW, SCREEN_W // 2,
                  SCREEN_H // 2 - 80, bold=True)
        draw_text(surf, "All waves cleared!", 34, WHITE,
                  SCREEN_W // 2, SCREEN_H // 2 - 20)
        draw_text(surf, f"Final Score: {self.player.score}", 40, CYAN,
                  SCREEN_W // 2, SCREEN_H // 2 + 30)
        if self.player.score >= self._high_score:
            draw_text(surf, "NEW HIGH SCORE!", 28, ORANGE,
                      SCREEN_W // 2, SCREEN_H // 2 + 72)
        draw_text(surf, "ENTER — Play Again    M — Menu",
                  26, WHITE, SCREEN_W // 2, SCREEN_H // 2 + 116)


# ─────────────────────────────────────────────
#  ENTRY POINT
# ─────────────────────────────────────────────
if __name__ == "__main__":
    game = SpaceShooter()
    game.run()
