"""
Space Shooter - Python + Pygame Desktop Game
============================================
Controls:
  Arrow Keys / WASD  - Move ship
  Space              - Shoot
  ESC                - Quit / Return to menu

Requirements:  pip install pygame
Compile to EXE: pip install pyinstaller
               pyinstaller --onefile --windowed space_shooter.py
"""

import pygame
import random
import math
import sys

# ─────────────────────────────────────────────
#  CONSTANTS
# ─────────────────────────────────────────────
WIDTH, HEIGHT   = 800, 700
FPS             = 60
TITLE           = "Space Shooter"

# Colours
BLACK   = (0,   0,   0)
WHITE   = (255, 255, 255)
RED     = (220,  30,  30)
GREEN   = ( 40, 200,  60)
CYAN    = (  0, 220, 255)
YELLOW  = (255, 220,   0)
ORANGE  = (255, 140,   0)
PURPLE  = (160,  40, 220)
GREY    = (120, 120, 120)
DKBLUE  = (  5,   5,  25)
LTBLUE  = ( 50, 120, 255)
PINK    = (255,  80, 180)

# ─────────────────────────────────────────────
#  HELPERS – draw shapes that act as sprites
# ─────────────────────────────────────────────

def make_player_surf(w=48, h=56):
    s = pygame.Surface((w, h), pygame.SRCALPHA)
    # Main body
    pygame.draw.polygon(s, CYAN, [(w//2, 0), (w-4, h-8), (w//2, h-16), (4, h-8)])
    # Cockpit
    pygame.draw.ellipse(s, WHITE, (w//2-8, 10, 16, 14))
    # Engine glow
    pygame.draw.ellipse(s, ORANGE, (w//2-6, h-18, 12, 10))
    pygame.draw.ellipse(s, YELLOW, (w//2-3, h-16, 6, 6))
    return s

def make_enemy_surf(kind=0, w=40, h=36):
    s = pygame.Surface((w, h), pygame.SRCALPHA)
    if kind == 0:   # Basic – diamond
        pygame.draw.polygon(s, RED, [(w//2, 2), (w-2, h//2), (w//2, h-2), (2, h//2)])
        pygame.draw.ellipse(s, ORANGE, (w//2-5, h//2-5, 10, 10))
    elif kind == 1: # Fast – triangle
        pygame.draw.polygon(s, PINK, [(w//2, h-2), (2, 2), (w-2, 2)])
        pygame.draw.circle(s, WHITE, (w//2, h//2), 5)
    else:           # Tank – hexagon-ish
        pts = [(w//2, 2),(w-4, 10),(w-4, h-10),(w//2, h-2),(4, h-10),(4, 10)]
        pygame.draw.polygon(s, PURPLE, pts)
        pygame.draw.circle(s, RED, (w//2, h//2), 7)
    return s

def make_bullet_surf(color, w=6, h=16):
    s = pygame.Surface((w, h), pygame.SRCALPHA)
    pygame.draw.ellipse(s, color, (0, 0, w, h))
    pygame.draw.ellipse(s, WHITE, (1, 1, w-2, 4))
    return s

def make_powerup_surf(kind):
    s = pygame.Surface((28, 28), pygame.SRCALPHA)
    if kind == "shield":
        pygame.draw.circle(s, CYAN,   (14, 14), 13)
        pygame.draw.circle(s, WHITE,  (14, 14), 13, 2)
        pygame.draw.polygon(s, WHITE, [(14,5),(20,20),(8,20)])
    elif kind == "rapid":
        pygame.draw.circle(s, YELLOW, (14, 14), 13)
        pygame.draw.circle(s, WHITE,  (14, 14), 13, 2)
        for i in range(3):
            pygame.draw.rect(s, WHITE, (8+i*4, 8, 3, 13))
    else:  # health
        pygame.draw.circle(s, GREEN, (14, 14), 13)
        pygame.draw.circle(s, WHITE, (14, 14), 13, 2)
        pygame.draw.rect(s, WHITE, (10, 6, 8, 16))
        pygame.draw.rect(s, WHITE, (6, 10, 16, 8))
    return s

# ─────────────────────────────────────────────
#  STAR FIELD  (parallax two layers)
# ─────────────────────────────────────────────

class StarField:
    def __init__(self, count=120):
        self.layers = [
            [(random.randint(0, WIDTH), random.randint(0, HEIGHT), random.randint(1,2)) for _ in range(count//2)],
            [(random.randint(0, WIDTH), random.randint(0, HEIGHT), random.randint(2,3)) for _ in range(count//2)],
        ]
        self.speeds = [0.6, 1.5]

    def update(self):
        for li, layer in enumerate(self.layers):
            spd = self.speeds[li]
            self.layers[li] = [
                (x, (y + spd) % HEIGHT, r) for x, y, r in layer
            ]

    def draw(self, surf):
        for li, layer in enumerate(self.layers):
            brightness = 160 if li == 0 else 220
            for x, y, r in layer:
                pygame.draw.circle(surf, (brightness, brightness, brightness), (int(x), int(y)), r)

# ─────────────────────────────────────────────
#  EXPLOSION  (particle burst)
# ─────────────────────────────────────────────

class Explosion:
    def __init__(self, cx, cy, color=ORANGE, count=18):
        self.particles = []
        for _ in range(count):
            angle = random.uniform(0, 2*math.pi)
            speed = random.uniform(1.5, 5.5)
            life  = random.randint(18, 35)
            r     = random.randint(2, 5)
            self.particles.append([cx, cy, math.cos(angle)*speed, math.sin(angle)*speed, life, life, r, color])
        self.done = False

    def update(self):
        alive = []
        for p in self.particles:
            p[0] += p[2]; p[1] += p[3]
            p[2] *= 0.93;  p[3] *= 0.93
            p[4] -= 1
            if p[4] > 0:
                alive.append(p)
        self.particles = alive
        if not self.particles:
            self.done = True

    def draw(self, surf):
        for p in self.particles:
            alpha = int(255 * p[4] / p[5])
            color = (*p[7][:3], alpha) if len(p[7]) == 4 else (*p[7], alpha)
            s = pygame.Surface((p[6]*2, p[6]*2), pygame.SRCALPHA)
            pygame.draw.circle(s, color, (p[6], p[6]), p[6])
            surf.blit(s, (int(p[0]-p[6]), int(p[1]-p[6])))

# ─────────────────────────────────────────────
#  BULLET
# ─────────────────────────────────────────────

class Bullet:
    def __init__(self, x, y, dy, color=YELLOW, damage=1):
        self.x      = x
        self.y      = y
        self.dy     = dy
        self.surf   = make_bullet_surf(color)
        self.rect   = self.surf.get_rect(center=(x, y))
        self.damage = damage
        self.alive  = True

    def update(self):
        self.y += self.dy
        self.rect.center = (int(self.x), int(self.y))
        if self.y < -20 or self.y > HEIGHT + 20:
            self.alive = False

    def draw(self, surf):
        surf.blit(self.surf, self.rect)

# ─────────────────────────────────────────────
#  POWER-UP
# ─────────────────────────────────────────────

class PowerUp:
    def __init__(self, x, y, kind):
        self.x     = float(x)
        self.y     = float(y)
        self.kind  = kind
        self.surf  = make_powerup_surf(kind)
        self.rect  = self.surf.get_rect(center=(x, y))
        self.alive = True
        self.bob   = 0.0

    def update(self):
        self.y += 1.8
        self.bob += 0.1
        self.rect.center = (int(self.x), int(self.y + math.sin(self.bob)*4))
        if self.y > HEIGHT + 40:
            self.alive = False

    def draw(self, surf):
        surf.blit(self.surf, self.rect)

# ─────────────────────────────────────────────
#  ENEMY
# ─────────────────────────────────────────────

class Enemy:
    KINDS = {
        0: dict(hp=1, speed=2.0, pts=10,  fire_rate=90,  bullet_spd=4, color=RED,    w=40, h=36),
        1: dict(hp=1, speed=3.5, pts=20,  fire_rate=120, bullet_spd=5, color=PINK,   w=36, h=32),
        2: dict(hp=3, speed=1.2, pts=50,  fire_rate=60,  bullet_spd=3, color=PURPLE, w=48, h=44),
    }

    def __init__(self, x, y, kind=0, wave=1):
        info         = self.KINDS[kind]
        self.x       = float(x)
        self.y       = float(y)
        self.kind    = kind
        self.hp      = info["hp"] + wave // 3
        self.max_hp  = self.hp
        self.speed   = info["speed"] + wave * 0.15
        self.pts     = info["pts"]
        self.color   = info["color"]
        self.fire_cd = random.randint(0, info["fire_rate"])
        self.fire_rate = max(30, info["fire_rate"] - wave * 3)
        self.bullet_spd = info["bullet_spd"]
        self.surf    = make_enemy_surf(kind, info["w"], info["h"])
        self.rect    = self.surf.get_rect(center=(x, y))
        self.alive   = True
        # Sine-wave horizontal drift
        self.drift_amp   = random.uniform(0, 60) if kind != 2 else 30
        self.drift_freq  = random.uniform(0.02, 0.05)
        self.drift_off   = random.uniform(0, 2*math.pi)
        self.origin_x    = float(x)
        self.t           = 0.0
        self.shot_bullets: list[Bullet] = []

    def update(self):
        self.t   += 1
        self.y   += self.speed
        self.x    = self.origin_x + self.drift_amp * math.sin(self.drift_freq * self.t + self.drift_off)
        self.rect.center = (int(self.x), int(self.y))
        if self.y > HEIGHT + 60:
            self.alive = False

        # Shoot downward
        self.fire_cd -= 1
        if self.fire_cd <= 0:
            self.fire_cd = self.fire_rate
            self._shoot()

        for b in self.shot_bullets:
            b.update()
        self.shot_bullets = [b for b in self.shot_bullets if b.alive]

    def _shoot(self):
        if self.kind == 2:                          # spread shot
            for ang in [-15, 0, 15]:
                rad = math.radians(ang)
                dx  = math.sin(rad) * self.bullet_spd
                dy  = math.cos(rad) * self.bullet_spd
                b   = Bullet(self.x, self.y + 20, dy, color=self.color, damage=1)
                b.dy = dy; b.dx = dx
                self.shot_bullets.append(b)
        else:
            self.shot_bullets.append(Bullet(self.x, self.y + 20, self.bullet_spd, color=self.color))

    def hit(self, dmg=1):
        self.hp -= dmg
        if self.hp <= 0:
            self.alive = False

    def draw(self, surf):
        surf.blit(self.surf, self.rect)
        # HP bar (only if damaged)
        if self.hp < self.max_hp:
            bw = self.rect.width
            pygame.draw.rect(surf, RED,   (self.rect.x, self.rect.y-7, bw, 4))
            pygame.draw.rect(surf, GREEN, (self.rect.x, self.rect.y-7, int(bw * self.hp/self.max_hp), 4))
        for b in self.shot_bullets:
            b.draw(surf)

# ─────────────────────────────────────────────
#  PLAYER
# ─────────────────────────────────────────────

class Player:
    MAX_HP     = 5
    SPEED      = 5
    FIRE_DELAY = 15        # frames between shots (normal)

    def __init__(self):
        self.surf         = make_player_surf()
        self.x            = float(WIDTH // 2)
        self.y            = float(HEIGHT - 80)
        self.rect         = self.surf.get_rect(center=(int(self.x), int(self.y)))
        self.hp           = self.MAX_HP
        self.score        = 0
        self.bullets      : list[Bullet] = []
        self.fire_cd      = 0
        self.shield_timer = 0    # frames remaining
        self.rapid_timer  = 0
        self.invincible   = 0    # brief invincibility after hit
        self.alive        = True

    # ── input ──────────────────────────────────
    def handle_input(self, keys):
        dx = dy = 0
        if keys[pygame.K_LEFT]  or keys[pygame.K_a]: dx -= 1
        if keys[pygame.K_RIGHT] or keys[pygame.K_d]: dx += 1
        if keys[pygame.K_UP]    or keys[pygame.K_w]: dy -= 1
        if keys[pygame.K_DOWN]  or keys[pygame.K_s]: dy += 1
        # Normalise diagonal
        if dx and dy:
            dx *= 0.707; dy *= 0.707
        self.x = max(24, min(WIDTH-24,  self.x + dx * self.SPEED))
        self.y = max(30, min(HEIGHT-30, self.y + dy * self.SPEED))

        # Shoot
        self.fire_cd -= 1
        if keys[pygame.K_SPACE] and self.fire_cd <= 0:
            delay = 6 if self.rapid_timer > 0 else self.FIRE_DELAY
            self.fire_cd = delay
            self._shoot()

    def _shoot(self):
        if self.rapid_timer > 0:
            # Triple spread
            for ang in [-12, 0, 12]:
                rad = math.radians(ang)
                b = Bullet(self.x + math.sin(rad)*14, self.y - 20, -12, color=YELLOW, damage=1)
                b.dx = math.sin(rad) * 4
                self.bullets.append(b)
        else:
            self.bullets.append(Bullet(self.x, self.y - 20, -12, color=YELLOW))

    # ── update ─────────────────────────────────
    def update(self):
        self.rect.center = (int(self.x), int(self.y))
        for b in self.bullets:
            b.update()
            if hasattr(b, "dx"):
                b.x += b.dx
                b.rect.centerx = int(b.x)
        self.bullets = [b for b in self.bullets if b.alive]
        if self.shield_timer > 0: self.shield_timer -= 1
        if self.rapid_timer  > 0: self.rapid_timer  -= 1
        if self.invincible   > 0: self.invincible   -= 1

    def take_hit(self):
        if self.invincible > 0 or self.shield_timer > 0:
            return False   # absorbed
        self.hp -= 1
        self.invincible = 90
        if self.hp <= 0:
            self.alive = False
        return True

    def apply_powerup(self, kind):
        if kind == "shield": self.shield_timer = 360   # 6 sec
        elif kind == "rapid": self.rapid_timer = 300   # 5 sec
        elif kind == "health": self.hp = min(self.MAX_HP, self.hp + 1)

    def draw(self, surf):
        # Flicker when invincible
        if self.invincible > 0 and (self.invincible // 6) % 2 == 0:
            return
        surf.blit(self.surf, self.rect)
        for b in self.bullets:
            b.draw(surf)
        # Shield ring
        if self.shield_timer > 0:
            alpha = min(180, self.shield_timer)
            sh = pygame.Surface((80, 80), pygame.SRCALPHA)
            pygame.draw.circle(sh, (*CYAN, alpha), (40, 40), 38, 3)
            surf.blit(sh, (int(self.x)-40, int(self.y)-40))

# ─────────────────────────────────────────────
#  HUD
# ─────────────────────────────────────────────

def draw_hud(surf, player, wave, font, sfont):
    # Health bar
    for i in range(player.MAX_HP):
        col = GREEN if i < player.hp else GREY
        pygame.draw.rect(surf, col,  (12 + i*28, 10, 22, 18), border_radius=4)
        pygame.draw.rect(surf, WHITE,(12 + i*28, 10, 22, 18), 1, border_radius=4)

    # Score
    sc_txt = font.render(f"Score: {player.score}", True, WHITE)
    surf.blit(sc_txt, (WIDTH - sc_txt.get_width() - 12, 8))

    # Wave
    wv_txt = sfont.render(f"WAVE  {wave}", True, YELLOW)
    surf.blit(wv_txt, (WIDTH//2 - wv_txt.get_width()//2, 10))

    # Power-up timers
    y_off = 36
    if player.shield_timer > 0:
        pct = player.shield_timer / 360
        pygame.draw.rect(surf, GREY,  (12, y_off, 100, 10), border_radius=3)
        pygame.draw.rect(surf, CYAN,  (12, y_off, int(100*pct), 10), border_radius=3)
        surf.blit(sfont.render("SHIELD", True, CYAN), (116, y_off-1))
        y_off += 18
    if player.rapid_timer > 0:
        pct = player.rapid_timer / 300
        pygame.draw.rect(surf, GREY,   (12, y_off, 100, 10), border_radius=3)
        pygame.draw.rect(surf, YELLOW, (12, y_off, int(100*pct), 10), border_radius=3)
        surf.blit(sfont.render("RAPID", True, YELLOW), (116, y_off-1))

# ─────────────────────────────────────────────
#  MENU SCREEN
# ─────────────────────────────────────────────

def draw_menu(surf, font, sfont, bfont, stars, high_score):
    stars.update()
    surf.fill(DKBLUE)
    stars.draw(surf)

    # Title glow
    for off in range(4, 0, -1):
        gt = bfont.render("SPACE SHOOTER", True, (*CYAN, 40*off))
        gt.set_alpha(40*off)
        surf.blit(gt, (WIDTH//2 - gt.get_width()//2 + off, 120 + off))
    title = bfont.render("SPACE SHOOTER", True, WHITE)
    surf.blit(title, (WIDTH//2 - title.get_width()//2, 120))

    sub   = font.render("Press  SPACE  to Start", True, YELLOW)
    surf.blit(sub, (WIDTH//2 - sub.get_width()//2, 280))

    ctrl  = [
        sfont.render("Arrow Keys / WASD  →  Move", True, GREY),
        sfont.render("Space              →  Shoot", True, GREY),
        sfont.render("Collect power-ups for SHIELD, RAPID FIRE & HEALTH", True, GREY),
    ]
    for i, c in enumerate(ctrl):
        surf.blit(c, (WIDTH//2 - c.get_width()//2, 360 + i*26))

    if high_score > 0:
        hs = font.render(f"High Score:  {high_score}", True, ORANGE)
        surf.blit(hs, (WIDTH//2 - hs.get_width()//2, 540))

# ─────────────────────────────────────────────
#  GAME OVER SCREEN
# ─────────────────────────────────────────────

def draw_gameover(surf, font, sfont, bfont, stars, score, high_score):
    stars.update()
    surf.fill(DKBLUE)
    stars.draw(surf)

    go   = bfont.render("GAME  OVER", True, RED)
    surf.blit(go,   (WIDTH//2 - go.get_width()//2,  160))
    sc   = font.render(f"Your Score:  {score}",      True, WHITE)
    surf.blit(sc,   (WIDTH//2 - sc.get_width()//2,  270))
    hs   = font.render(f"High Score:  {high_score}", True, YELLOW)
    surf.blit(hs,   (WIDTH//2 - hs.get_width()//2,  310))
    again = sfont.render("Press  SPACE  to Play Again   |   ESC  to Quit", True, GREY)
    surf.blit(again,(WIDTH//2 - again.get_width()//2, 420))

# ─────────────────────────────────────────────
#  WAVE SPAWNER
# ─────────────────────────────────────────────

class WaveSpawner:
    def __init__(self, wave):
        self.wave        = wave
        self.enemies_left = 6 + wave * 3   # total enemies this wave
        self.spawn_cd    = 80
        self.cd          = 40

    def update(self):
        self.cd -= 1
        if self.cd <= 0 and self.enemies_left > 0:
            self.cd = self.spawn_cd
            self.enemies_left -= 1
            kind = random.choices([0,1,2], weights=[6, 3+self.wave, 1+self.wave//2])[0]
            x    = random.randint(40, WIDTH-40)
            return Enemy(x, -40, kind, self.wave)
        return None

    @property
    def wave_done(self):
        return self.enemies_left <= 0

# ─────────────────────────────────────────────
#  SIMPLE SOUND SYNTHESISER  (no files needed)
# ─────────────────────────────────────────────

def _make_sound(freq, duration_ms, wave="sine", vol=0.3):
    import numpy as np
    rate    = 44100
    frames  = int(rate * duration_ms / 1000)
    t       = [i / rate for i in range(frames)]
    try:
        import numpy as np
        arr = np.array([math.sin(2*math.pi*freq*x) for x in t], dtype=np.float32)
        # simple envelope
        fade = min(frames//10, 200)
        for i in range(fade):
            arr[i]           *= i/fade
            arr[frames-1-i]  *= i/fade
        arr = (arr * vol * 32767).astype(np.int16)
        stereo = np.column_stack([arr, arr])
        snd = pygame.sndarray.make_sound(stereo)
        return snd
    except Exception:
        return None

def build_sounds():
    sounds = {}
    specs  = {
        "shoot":    (880,  80, "sine",   0.15),
        "explode":  (120, 250, "noise",  0.40),
        "hit":      (300, 120, "sine",   0.25),
        "powerup":  (660, 300, "sine",   0.30),
        "wave":     (440, 500, "sine",   0.30),
    }
    for name, (freq, dur, wtype, vol) in specs.items():
        sounds[name] = _make_sound(freq, dur, wtype, vol)
    return sounds

def play(sounds, name):
    s = sounds.get(name)
    if s:
        try:
            s.play()
        except Exception:
            pass

# ─────────────────────────────────────────────
#  WAVE BANNER
# ─────────────────────────────────────────────

class WaveBanner:
    def __init__(self, wave, font):
        self.timer = 120
        self.surf  = font.render(f"— WAVE  {wave} —", True, YELLOW)

    def update(self):
        self.timer -= 1

    def draw(self, surf):
        alpha = min(255, self.timer * 4, (120-self.timer+1)*4 if self.timer < 30 else 255)
        self.surf.set_alpha(alpha)
        surf.blit(self.surf, (WIDTH//2 - self.surf.get_width()//2, HEIGHT//2 - 30))

# ─────────────────────────────────────────────
#  MAIN GAME LOOP
# ─────────────────────────────────────────────

def run_game(screen, clock, fonts, sounds, stars, high_score):
    font, sfont, bfont = fonts

    player      = Player()
    wave_num    = 1
    spawner     = WaveSpawner(wave_num)
    enemies     : list[Enemy]     = []
    explosions  : list[Explosion] = []
    powerups    : list[PowerUp]   = []
    banner      = WaveBanner(wave_num, font)

    POWERUP_CHANCE = 0.12   # probability per enemy kill

    running = True
    while running:
        clock.tick(FPS)
        keys = pygame.key.get_pressed()

        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                return "quit", player.score
            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    return "menu", player.score

        # ── update player ───────────────────────
        player.handle_input(keys)
        player.update()

        # ── spawn wave ──────────────────────────
        new_enemy = spawner.update()
        if new_enemy:
            enemies.append(new_enemy)

        # ── next wave? ──────────────────────────
        if spawner.wave_done and not enemies:
            wave_num += 1
            spawner   = WaveSpawner(wave_num)
            banner    = WaveBanner(wave_num, font)
            play(sounds, "wave")

        # ── update enemies ──────────────────────
        for e in enemies:
            e.update()

        # ── update powerups ─────────────────────
        for p in powerups:
            p.update()

        # ── update explosions ───────────────────
        for ex in explosions:
            ex.update()

        # ── COLLISIONS ──────────────────────────
        # Player bullets → enemies
        for b in player.bullets:
            for e in enemies:
                if b.alive and e.alive and e.rect.collidepoint(b.rect.center):
                    e.hit(b.damage)
                    b.alive = False
                    play(sounds, "hit")
                    if not e.alive:
                        player.score += e.pts
                        explosions.append(Explosion(e.x, e.y, e.color))
                        play(sounds, "explode")
                        if random.random() < POWERUP_CHANCE:
                            kind = random.choice(["shield","rapid","health"])
                            powerups.append(PowerUp(int(e.x), int(e.y), kind))

        # Enemy bullets → player
        for e in enemies:
            for b in e.shot_bullets:
                if b.alive and player.rect.collidepoint(b.rect.center):
                    b.alive = False
                    if player.take_hit():
                        play(sounds, "hit")
                        explosions.append(Explosion(player.x, player.y, CYAN, 10))

        # Enemy body → player
        for e in enemies:
            if e.alive and e.rect.colliderect(player.rect):
                if player.take_hit():
                    play(sounds, "explode")
                    e.alive = False
                    explosions.append(Explosion(e.x, e.y, e.color))

        # Power-ups → player
        for p in powerups:
            if p.alive and player.rect.colliderect(p.rect):
                player.apply_powerup(p.kind)
                p.alive = False
                play(sounds, "powerup")

        # ── clean dead objects ──────────────────
        enemies    = [e for e in enemies    if e.alive]
        explosions = [ex for ex in explosions if not ex.done]
        powerups   = [p for p in powerups   if p.alive]
        player.bullets = [b for b in player.bullets if b.alive]

        # ── check game over ──────────────────────
        if not player.alive:
            return "gameover", player.score

        # ── update banner ───────────────────────
        if banner.timer > 0:
            banner.update()

        # ══ DRAW ════════════════════════════════
        screen.fill(DKBLUE)
        stars.update()
        stars.draw(screen)

        for e in enemies:
            e.draw(screen)
        for p in powerups:
            p.draw(screen)
        for ex in explosions:
            ex.draw(screen)
        player.draw(screen)

        draw_hud(screen, player, wave_num, font, sfont)

        if banner.timer > 0:
            banner.draw(screen)

        pygame.display.flip()

    return "menu", player.score

# ─────────────────────────────────────────────
#  ENTRY POINT
# ─────────────────────────────────────────────

def main():
    pygame.init()
    pygame.mixer.init(frequency=44100, size=-16, channels=2, buffer=512)

    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    pygame.display.set_caption(TITLE)
    clock  = pygame.time.Clock()

    bfont  = pygame.font.SysFont("Arial", 52, bold=True)
    font   = pygame.font.SysFont("Arial", 28, bold=True)
    sfont  = pygame.font.SysFont("Arial", 18)

    sounds     = build_sounds()
    stars      = StarField(150)
    high_score = 0
    state      = "menu"
    last_score = 0

    while True:
        # ── MENU ────────────────────────────────
        if state == "menu":
            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    pygame.quit(); sys.exit()
                if event.type == pygame.KEYDOWN:
                    if event.key == pygame.K_ESCAPE:
                        pygame.quit(); sys.exit()
                    if event.key == pygame.K_SPACE:
                        state = "game"
            if state == "menu":
                draw_menu(screen, font, sfont, bfont, stars, high_score)
                pygame.display.flip()
                clock.tick(FPS)

        # ── GAME ────────────────────────────────
        elif state == "game":
            result, last_score = run_game(screen, clock, (font, sfont, bfont), sounds, stars, high_score)
            high_score = max(high_score, last_score)
            state = result
            if state == "quit":
                pygame.quit(); sys.exit()

        # ── GAME OVER ───────────────────────────
        elif state == "gameover":
            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    pygame.quit(); sys.exit()
                if event.type == pygame.KEYDOWN:
                    if event.key == pygame.K_ESCAPE:
                        pygame.quit(); sys.exit()
                    if event.key == pygame.K_SPACE:
                        state = "game"
            if state == "gameover":
                draw_gameover(screen, font, sfont, bfont, stars, last_score, high_score)
                pygame.display.flip()
                clock.tick(FPS)

if __name__ == "__main__":
    main()
