"""
Space Shooter — Python + Pygame desktop game
Run:     python space_shooter.py
Build:   pip install pyinstaller && pyinstaller --onefile --noconsole space_shooter.py
"""

import pygame
import random
import math
import sys

# ─────────────────────────────────────────────────────────────
# CONSTANTS
# ─────────────────────────────────────────────────────────────
WIDTH, HEIGHT = 800, 900
FPS           = 60
TITLE         = "Space Shooter"

# Colours
BLACK   = (0,   0,   0)
WHITE   = (255, 255, 255)
YELLOW  = (255, 230,  50)
CYAN    = ( 50, 220, 255)
RED     = (220,  50,  50)
GREEN   = ( 50, 220,  80)
ORANGE  = (255, 140,   0)
PURPLE  = (160,  40, 200)
GRAY    = (120, 120, 120)
DKBLUE  = ( 10,  10,  40)

# ─────────────────────────────────────────────────────────────
# HELPERS — procedural sprite drawing
# ─────────────────────────────────────────────────────────────
def make_player_surf():
    s = pygame.Surface((48, 56), pygame.SRCALPHA)
    # body
    pygame.draw.polygon(s, CYAN, [(24,0),(44,44),(24,36),(4,44)])
    # cockpit
    pygame.draw.polygon(s, WHITE, [(24,10),(32,30),(24,26),(16,30)])
    # engine glow
    pygame.draw.ellipse(s, ORANGE, (12, 46, 24, 10))
    return s

def make_enemy_surf(color, kind="basic"):
    s = pygame.Surface((40, 36), pygame.SRCALPHA)
    if kind == "fast":
        pygame.draw.polygon(s, color, [(20,0),(38,28),(20,20),(2,28)])
    elif kind == "tank":
        pygame.draw.rect(s, color, (4,4,32,28), border_radius=6)
        pygame.draw.rect(s, WHITE, (12,8,16,12), border_radius=4)
    elif kind == "zigzag":
        pygame.draw.polygon(s, color, [(20,0),(40,18),(32,36),(8,36),(0,18)])
    else:  # basic
        pygame.draw.polygon(s, color, [(20,0),(38,22),(28,36),(12,36),(2,22)])
        pygame.draw.circle(s, WHITE, (20,18), 7)
    return s

def make_boss_surf():
    s = pygame.Surface((100, 80), pygame.SRCALPHA)
    pygame.draw.polygon(s, PURPLE, [(50,0),(90,30),(90,70),(50,60),(10,70),(10,30)])
    pygame.draw.circle(s, RED,   (50,38), 18)
    pygame.draw.circle(s, WHITE, (50,38), 8)
    pygame.draw.rect(s, ORANGE, (10,60,20,14), border_radius=4)
    pygame.draw.rect(s, ORANGE, (70,60,20,14), border_radius=4)
    return s

def make_bullet_surf(color, w=6, h=18):
    s = pygame.Surface((w, h), pygame.SRCALPHA)
    pygame.draw.ellipse(s, color, (0,0,w,h))
    return s

def make_powerup_surf(color):
    s = pygame.Surface((28, 28), pygame.SRCALPHA)
    pygame.draw.circle(s, color, (14,14), 14)
    pygame.draw.circle(s, WHITE, (14,14), 7)
    return s

def make_star_surf(size, alpha):
    s = pygame.Surface((size, size), pygame.SRCALPHA)
    s.fill((255,255,255,alpha))
    return s

# ─────────────────────────────────────────────────────────────
# STAR FIELD
# ─────────────────────────────────────────────────────────────
class Star:
    def __init__(self):
        self.reset(random.randint(0, HEIGHT))

    def reset(self, y=None):
        self.x     = random.randint(0, WIDTH)
        self.y     = y if y is not None else -2
        size       = random.choice([1, 2, 3])
        alpha      = random.randint(80, 220)
        self.surf  = make_star_surf(size, alpha)
        self.speed = size * 0.8 + random.random()

    def update(self):
        self.y += self.speed
        if self.y > HEIGHT + 4:
            self.reset()

    def draw(self, screen):
        screen.blit(self.surf, (self.x, self.y))

# ─────────────────────────────────────────────────────────────
# PARTICLE (explosion sparks)
# ─────────────────────────────────────────────────────────────
class Particle:
    def __init__(self, x, y, color):
        self.x      = x
        self.y      = y
        self.color  = color
        angle       = random.uniform(0, math.tau)
        speed       = random.uniform(1, 5)
        self.vx     = math.cos(angle) * speed
        self.vy     = math.sin(angle) * speed
        self.life   = random.randint(18, 35)
        self.max_life = self.life
        self.size   = random.randint(2, 5)

    def update(self):
        self.x    += self.vx
        self.y    += self.vy
        self.vy   += 0.12   # gravity
        self.life -= 1

    def draw(self, screen):
        alpha = int(255 * (self.life / self.max_life))
        s = pygame.Surface((self.size*2, self.size*2), pygame.SRCALPHA)
        pygame.draw.circle(s, (*self.color, alpha), (self.size, self.size), self.size)
        screen.blit(s, (self.x - self.size, self.y - self.size))

    @property
    def dead(self):
        return self.life <= 0

# ─────────────────────────────────────────────────────────────
# BULLET
# ─────────────────────────────────────────────────────────────
class Bullet:
    def __init__(self, x, y, vy, color=CYAN, angle_deg=0):
        self.surf  = make_bullet_surf(color)
        self.rect  = self.surf.get_rect(centerx=x, centery=y)
        rad        = math.radians(angle_deg)
        speed      = abs(vy)
        self.vx    = math.sin(rad) * speed
        self.vy    = -math.cos(rad) * speed if vy < 0 else math.cos(rad) * speed
        self.damage = 10

    def update(self):
        self.rect.x += self.vx
        self.rect.y += self.vy

    def off_screen(self):
        return self.rect.bottom < -10 or self.rect.top > HEIGHT + 10 \
            or self.rect.right < -10 or self.rect.left > WIDTH + 10

    def draw(self, screen):
        screen.blit(self.surf, self.rect)

# ─────────────────────────────────────────────────────────────
# POWER-UP
# ─────────────────────────────────────────────────────────────
POWERUP_TYPES = ["triple", "speed", "health", "shield"]

class PowerUp:
    COLORS = {"triple": CYAN, "speed": YELLOW, "health": GREEN, "shield": ORANGE}
    LABELS = {"triple": "3x SHOT", "speed": "SPEED UP", "health": "+HP", "shield": "SHIELD"}

    def __init__(self, x, y):
        self.kind  = random.choice(POWERUP_TYPES)
        color      = self.COLORS[self.kind]
        self.surf  = make_powerup_surf(color)
        self.rect  = self.surf.get_rect(center=(x, y))
        self.speed = 2.5
        self.bob   = 0.0

    def update(self):
        self.rect.y += self.speed
        self.bob    += 0.1
        self.rect.x += int(math.sin(self.bob) * 0.5)

    def off_screen(self):
        return self.rect.top > HEIGHT + 10

    def draw(self, screen, font_sm):
        screen.blit(self.surf, self.rect)
        label = font_sm.render(self.LABELS[self.kind], True, WHITE)
        screen.blit(label, (self.rect.centerx - label.get_width()//2, self.rect.bottom + 2))

# ─────────────────────────────────────────────────────────────
# ENEMY
# ─────────────────────────────────────────────────────────────
class Enemy:
    KINDS = {
        "basic":  {"hp": 30,  "score": 100, "speed": 2.5, "color": RED,    "fire_rate": 90,  "pattern": "single"},
        "fast":   {"hp": 15,  "score": 150, "speed": 5.0, "color": ORANGE, "fire_rate": 120, "pattern": "single"},
        "tank":   {"hp": 80,  "score": 300, "speed": 1.5, "color": PURPLE, "fire_rate": 70,  "pattern": "spread"},
        "zigzag": {"hp": 40,  "score": 200, "speed": 3.0, "color": YELLOW, "fire_rate": 80,  "pattern": "burst"},
    }

    def __init__(self, x, y, kind="basic"):
        d          = self.KINDS[kind]
        self.kind  = kind
        self.hp    = d["hp"]
        self.max_hp= d["hp"]
        self.score = d["score"]
        self.speed = d["speed"]
        self.surf  = make_enemy_surf(d["color"], kind)
        self.rect  = self.surf.get_rect(centerx=x, centery=y)
        self.color = d["color"]
        self.pattern    = d["pattern"]
        self.fire_timer = random.randint(30, d["fire_rate"])
        self.fire_rate  = d["fire_rate"]
        self.alive      = True
        # movement
        self.t          = 0.0
        self.origin_x   = float(x)

    def update(self):
        self.t += 0.04
        if self.kind == "zigzag":
            self.rect.x = int(self.origin_x + math.sin(self.t * 2) * 80)
        elif self.kind == "fast":
            pass  # straight down
        elif self.kind == "basic":
            self.rect.x = int(self.origin_x + math.sin(self.t) * 40)
        # all move down
        self.rect.y += self.speed
        self.fire_timer -= 1

    def should_fire(self):
        if self.fire_timer <= 0:
            self.fire_timer = self.fire_rate + random.randint(-15, 15)
            return True
        return False

    def get_bullets(self):
        cx = self.rect.centerx
        cy = self.rect.bottom
        speed = 5
        if self.pattern == "single":
            return [Bullet(cx, cy, speed, RED)]
        elif self.pattern == "spread":
            return [Bullet(cx, cy, speed, RED, a) for a in (-25, -12, 0, 12, 25)]
        elif self.pattern == "burst":
            return [Bullet(cx, cy, speed, ORANGE), Bullet(cx, cy, speed*1.3, ORANGE)]
        return []

    def take_damage(self, dmg):
        self.hp -= dmg
        if self.hp <= 0:
            self.alive = False

    def off_screen(self):
        return self.rect.top > HEIGHT + 20

    def draw(self, screen):
        screen.blit(self.surf, self.rect)
        # health bar
        if self.hp < self.max_hp:
            bar_w = self.rect.width
            ratio = max(0, self.hp / self.max_hp)
            pygame.draw.rect(screen, RED,   (self.rect.left, self.rect.top-6, bar_w, 4))
            pygame.draw.rect(screen, GREEN, (self.rect.left, self.rect.top-6, int(bar_w*ratio), 4))

# ─────────────────────────────────────────────────────────────
# BOSS
# ─────────────────────────────────────────────────────────────
class Boss:
    def __init__(self):
        self.surf       = make_boss_surf()
        self.rect       = self.surf.get_rect(centerx=WIDTH//2, centery=-80)
        self.hp         = 600
        self.max_hp     = 600
        self.score      = 2000
        self.alive      = True
        self.entered    = False
        self.t          = 0.0
        self.fire_timer = 80
        self.phase      = 1   # 1=normal 2=enraged at 50% hp

    def update(self):
        if not self.entered:
            self.rect.y += 2
            if self.rect.y >= 80:
                self.entered = True
        else:
            self.t += 0.02
            self.rect.centerx = WIDTH//2 + int(math.sin(self.t) * 200)
        if self.hp <= self.max_hp // 2:
            self.phase = 2
        self.fire_timer -= 1

    def should_fire(self):
        rate = 40 if self.phase == 2 else 65
        if self.fire_timer <= 0:
            self.fire_timer = rate
            return True
        return False

    def get_bullets(self):
        cx = self.rect.centerx
        cy = self.rect.bottom
        bullets = []
        if self.phase == 1:
            for a in (-30, -15, 0, 15, 30):
                bullets.append(Bullet(cx, cy, 6, RED, a))
        else:
            # full circle burst in phase 2
            for a in range(0, 360, 30):
                bullets.append(Bullet(cx, cy, 5, ORANGE, a))
        return bullets

    def take_damage(self, dmg):
        self.hp -= dmg
        if self.hp <= 0:
            self.alive = False

    def draw(self, screen):
        screen.blit(self.surf, self.rect)
        bar_w = self.rect.width
        ratio = max(0, self.hp / self.max_hp)
        pygame.draw.rect(screen, RED,   (self.rect.left, self.rect.top-12, bar_w, 8))
        pygame.draw.rect(screen, GREEN, (self.rect.left, self.rect.top-12, int(bar_w*ratio), 8))
        # BOSS label
        pygame.draw.rect(screen, PURPLE, (self.rect.left, self.rect.top-28, bar_w, 14), border_radius=4)

# ─────────────────────────────────────────────────────────────
# PLAYER
# ─────────────────────────────────────────────────────────────
class Player:
    def __init__(self):
        self.surf         = make_player_surf()
        self.rect         = self.surf.get_rect(center=(WIDTH//2, HEIGHT - 80))
        self.speed        = 5
        self.hp           = 100
        self.max_hp       = 100
        self.lives        = 3
        self.score        = 0
        self.fire_delay   = 8          # frames between shots
        self.fire_timer   = 0
        self.triple       = 0          # frames of triple-shot remaining
        self.speed_boost  = 0          # frames of speed boost remaining
        self.shield_timer = 0          # frames of shield remaining
        self.inv_timer    = 0          # invincibility frames after hit
        self.blink        = False

    def update(self, keys):
        spd = self.speed + (2 if self.speed_boost > 0 else 0)
        if keys[pygame.K_LEFT]  or keys[pygame.K_a]: self.rect.x -= spd
        if keys[pygame.K_RIGHT] or keys[pygame.K_d]: self.rect.x += spd
        if keys[pygame.K_UP]    or keys[pygame.K_w]: self.rect.y -= spd
        if keys[pygame.K_DOWN]  or keys[pygame.K_s]: self.rect.y += spd

        # Clamp to screen
        self.rect.clamp_ip(pygame.Rect(0, 0, WIDTH, HEIGHT))

        # Timers
        if self.fire_timer   > 0: self.fire_timer   -= 1
        if self.triple       > 0: self.triple        -= 1
        if self.speed_boost  > 0: self.speed_boost   -= 1
        if self.shield_timer > 0: self.shield_timer  -= 1
        if self.inv_timer    > 0:
            self.inv_timer -= 1
            self.blink = (self.inv_timer // 4) % 2 == 0

    def can_fire(self):
        return self.fire_timer <= 0

    def shoot(self):
        self.fire_timer = self.fire_delay
        cx = self.rect.centerx
        cy = self.rect.top + 4
        if self.triple > 0:
            return [Bullet(cx, cy, -14, CYAN, -12),
                    Bullet(cx, cy, -14, CYAN,   0),
                    Bullet(cx, cy, -14, CYAN,  12)]
        return [Bullet(cx, cy, -14, CYAN)]

    def take_damage(self, dmg):
        if self.inv_timer > 0 or self.shield_timer > 0:
            return False
        self.hp -= dmg
        self.inv_timer = 80
        return True

    def apply_powerup(self, kind):
        if kind == "triple":  self.triple      = FPS * 8
        if kind == "speed":   self.speed_boost = FPS * 6
        if kind == "shield":  self.shield_timer= FPS * 5
        if kind == "health":
            self.hp = min(self.max_hp, self.hp + 35)

    def draw(self, screen):
        if self.blink:
            return
        screen.blit(self.surf, self.rect)
        # shield ring
        if self.shield_timer > 0:
            pygame.draw.circle(screen, CYAN, self.rect.center, 36, 2)
        # engine trail flicker
        ex = self.rect.centerx + random.randint(-4, 4)
        ey = self.rect.bottom
        pygame.draw.ellipse(screen, ORANGE, (ex-8, ey, 16, random.randint(8,18)))

# ─────────────────────────────────────────────────────────────
# HUD
# ─────────────────────────────────────────────────────────────
def draw_hud(screen, player, wave, font, font_sm, powerup_msg, powerup_timer, boss=None):
    # Score
    screen.blit(font.render(f"SCORE  {player.score:,}", True, WHITE), (12, 10))

    # Lives (hearts)
    heart_x = WIDTH - 20
    for i in range(player.lives):
        pygame.draw.polygon(screen, RED, [
            (heart_x-9, 14), (heart_x-14, 10), (heart_x-14, 6),
            (heart_x-9, 3),  (heart_x-4, 6),   (heart_x-4, 10)
        ])
        heart_x -= 28

    # HP bar
    bar_w = 160
    pygame.draw.rect(screen, GRAY,  (12, 38, bar_w, 14), border_radius=6)
    pygame.draw.rect(screen, GREEN, (12, 38, int(bar_w * player.hp / player.max_hp), 14), border_radius=6)
    screen.blit(font_sm.render(f"HP {player.hp}", True, WHITE), (16, 40))

    # Wave
    screen.blit(font.render(f"WAVE {wave}", True, YELLOW), (WIDTH//2 - 60, 10))

    # Boss HP bar
    if boss:
        pygame.draw.rect(screen, GRAY,   (80, HEIGHT-30, WIDTH-160, 16), border_radius=6)
        ratio = max(0, boss.hp / boss.max_hp)
        pygame.draw.rect(screen, PURPLE, (80, HEIGHT-30, int((WIDTH-160)*ratio), 16), border_radius=6)
        label = font_sm.render("BOSS", True, WHITE)
        screen.blit(label, (80 - label.get_width() - 6, HEIGHT-28))

    # Power-up message
    if powerup_timer > 0:
        alpha = min(255, powerup_timer * 8)
        s = font.render(powerup_msg, True, CYAN)
        s.set_alpha(alpha)
        screen.blit(s, (WIDTH//2 - s.get_width()//2, HEIGHT//2 - 60))

# ─────────────────────────────────────────────────────────────
# SCREENS
# ─────────────────────────────────────────────────────────────
def draw_menu(screen, font_big, font, font_sm, high_score, stars):
    screen.fill(DKBLUE)
    for st in stars: st.draw(screen)

    title = font_big.render("SPACE  SHOOTER", True, CYAN)
    screen.blit(title, (WIDTH//2 - title.get_width()//2, 160))

    sub = font.render("Press  ENTER  to  Play", True, WHITE)
    screen.blit(sub, (WIDTH//2 - sub.get_width()//2, 310))

    controls = [
        "WASD / Arrow Keys  —  Move",
        "SPACE  —  Fire",
        "ESC  —  Pause",
    ]
    for i, line in enumerate(controls):
        t = font_sm.render(line, True, GRAY)
        screen.blit(t, (WIDTH//2 - t.get_width()//2, 400 + i * 28))

    hs = font.render(f"HIGH SCORE  {high_score:,}", True, YELLOW)
    screen.blit(hs, (WIDTH//2 - hs.get_width()//2, 520))

    credit = font_sm.render("Q  —  Quit", True, GRAY)
    screen.blit(credit, (WIDTH//2 - credit.get_width()//2, 580))


def draw_pause(screen, font_big, font):
    overlay = pygame.Surface((WIDTH, HEIGHT), pygame.SRCALPHA)
    overlay.fill((0, 0, 0, 140))
    screen.blit(overlay, (0, 0))
    t = font_big.render("PAUSED", True, WHITE)
    screen.blit(t, (WIDTH//2 - t.get_width()//2, HEIGHT//2 - 60))
    sub = font.render("ESC  —  Resume", True, GRAY)
    screen.blit(sub, (WIDTH//2 - sub.get_width()//2, HEIGHT//2 + 20))


def draw_game_over(screen, font_big, font, font_sm, score, high_score):
    overlay = pygame.Surface((WIDTH, HEIGHT), pygame.SRCALPHA)
    overlay.fill((0, 0, 0, 170))
    screen.blit(overlay, (0, 0))

    t = font_big.render("GAME  OVER", True, RED)
    screen.blit(t, (WIDTH//2 - t.get_width()//2, 260))

    s = font.render(f"SCORE  {score:,}", True, WHITE)
    screen.blit(s, (WIDTH//2 - s.get_width()//2, 360))

    if score >= high_score and score > 0:
        hs = font.render("NEW HIGH SCORE!", True, YELLOW)
        screen.blit(hs, (WIDTH//2 - hs.get_width()//2, 420))
    else:
        hs = font.render(f"BEST  {high_score:,}", True, YELLOW)
        screen.blit(hs, (WIDTH//2 - hs.get_width()//2, 420))

    r = font_sm.render("ENTER  —  Play Again     Q  —  Menu", True, GRAY)
    screen.blit(r, (WIDTH//2 - r.get_width()//2, 510))


def draw_wave_banner(screen, font_big, wave, timer):
    if timer <= 0:
        return
    alpha = min(255, timer * 7)
    is_boss = wave % 5 == 0
    label   = f"⚠ BOSS WAVE {wave} ⚠" if is_boss else f"WAVE  {wave}"
    color   = RED if is_boss else YELLOW
    t       = font_big.render(label, True, color)
    t.set_alpha(alpha)
    screen.blit(t, (WIDTH//2 - t.get_width()//2, HEIGHT//2 - 30))

# ─────────────────────────────────────────────────────────────
# WAVE SPAWNER
# ─────────────────────────────────────────────────────────────
def build_wave(wave_num):
    """Return a list of (delay_frames, kind, x) tuples."""
    spawns = []
    cycle  = (wave_num - 1) // 4   # difficulty cycle

    patterns = [
        # Wave 1: basic stream
        [("basic", 5 + cycle)],
        # Wave 2: basic + fast
        [("basic", 4 + cycle), ("fast", 3 + cycle)],
        # Wave 3: zigzag
        [("basic", 3 + cycle), ("zigzag", 4 + cycle)],
        # Wave 4: tank + fast
        [("tank", 2 + cycle), ("fast", 5 + cycle)],
    ]
    idx = (wave_num - 1) % 4

    delay = 0
    for (kind, count) in patterns[idx]:
        for _ in range(count):
            x = random.randint(60, WIDTH - 60)
            spawns.append((delay, kind, x))
            delay += max(15, 40 - wave_num * 3)
        delay += 60

    return spawns

# ─────────────────────────────────────────────────────────────
# MAIN GAME
# ─────────────────────────────────────────────────────────────
def game_loop(screen, fonts, high_score):
    font_big, font, font_sm = fonts
    clock  = pygame.time.Clock()

    # State
    state        = "menu"   # menu | playing | paused | gameover
    player       = Player()
    enemies      = []
    p_bullets    = []   # player bullets
    e_bullets    = []   # enemy bullets
    powerups     = []
    particles    = []
    boss         = None
    stars        = [Star() for _ in range(120)]
    wave         = 0
    wave_spawns  = []
    spawn_timer  = 0
    wave_clear_delay = 0
    wave_banner_timer = 0
    powerup_msg  = ""
    powerup_timer= 0

    def next_wave():
        nonlocal wave, wave_spawns, spawn_timer, wave_banner_timer, boss
        wave += 1
        wave_banner_timer = 40
        if wave % 5 == 0:
            boss = Boss()
            wave_spawns = []
        else:
            wave_spawns = build_wave(wave)
            spawn_timer = 0

    def explode(x, y, color, count=18):
        for _ in range(count):
            particles.append(Particle(x, y, color))

    next_wave()

    while True:
        clock.tick(FPS)
        keys = pygame.key.get_pressed()

        # ── Events ────────────────────────────────────────────
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                return high_score, False   # quit entirely

            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_q:
                    if state in ("menu", "gameover"):
                        return high_score, False
                    state = "menu"

                if event.key == pygame.K_RETURN:
                    if state == "menu":
                        state    = "playing"
                        player   = Player()
                        enemies  = []; p_bullets = []; e_bullets = []
                        powerups = []; particles = []
                        boss     = None
                        wave     = 0
                        wave_spawns = []
                        next_wave()
                    elif state == "gameover":
                        return high_score, True   # restart

                if event.key == pygame.K_ESCAPE:
                    if state == "playing": state = "paused"
                    elif state == "paused": state = "playing"

        # ── Draw background ───────────────────────────────────
        screen.fill(DKBLUE)
        for st in stars:
            if state != "paused": st.update()
            st.draw(screen)

        # ────────────────── MENU ──────────────────────────────
        if state == "menu":
            draw_menu(screen, font_big, font, font_sm, high_score, stars)
            pygame.display.flip()
            continue

        # ── Update particles ──────────────────────────────────
        if state == "playing":
            particles = [p for p in particles if not p.dead]
            for p in particles: p.update()
        for p in particles: p.draw(screen)

        # ────────────────── PLAYING ───────────────────────────
        if state == "playing":
            spawn_timer += 1

            # Spawn enemies from wave queue
            remaining = []
            for (delay, kind, x) in wave_spawns:
                if spawn_timer >= delay:
                    enemies.append(Enemy(x, -30, kind))
                else:
                    remaining.append((delay, kind, x))
            wave_spawns = remaining

            # Boss
            if boss:
                boss.update()
                if boss.should_fire():
                    e_bullets.extend(boss.get_bullets())
                if not boss.alive:
                    explode(boss.rect.centerx, boss.rect.centery, PURPLE, 60)
                    player.score += boss.score
                    boss = None

            # Enemies
            for en in enemies[:]:
                en.update()
                if en.should_fire():
                    e_bullets.extend(en.get_bullets())
                if en.off_screen():
                    enemies.remove(en)

            # Player movement + shooting
            player.update(keys)
            if keys[pygame.K_SPACE] and player.can_fire():
                p_bullets.extend(player.shoot())

            # Player bullets
            for b in p_bullets[:]:
                b.update()
                if b.off_screen():
                    p_bullets.remove(b)
                    continue
                # Hit enemies
                for en in enemies[:]:
                    if b.rect.colliderect(en.rect):
                        en.take_damage(b.damage)
                        if b in p_bullets: p_bullets.remove(b)
                        if not en.alive:
                            explode(en.rect.centerx, en.rect.centery, en.color)
                            player.score += en.score
                            if random.random() < 0.18:
                                powerups.append(PowerUp(en.rect.centerx, en.rect.centery))
                            enemies.remove(en)
                        break
                # Hit boss
                if boss and b in p_bullets and b.rect.colliderect(boss.rect):
                    boss.take_damage(b.damage)
                    p_bullets.remove(b)

            # Enemy bullets
            for b in e_bullets[:]:
                b.update()
                if b.off_screen():
                    e_bullets.remove(b)
                    continue
                if b.rect.colliderect(player.rect):
                    if player.take_damage(20):
                        explode(player.rect.centerx, player.rect.centery, CYAN, 10)
                    if b in e_bullets: e_bullets.remove(b)

            # Enemy body collision
            for en in enemies[:]:
                if en.rect.colliderect(player.rect):
                    if player.take_damage(30):
                        explode(player.rect.centerx, player.rect.centery, CYAN, 12)

            # Power-ups
            for pu in powerups[:]:
                pu.update()
                if pu.off_screen():
                    powerups.remove(pu)
                    continue
                if pu.rect.colliderect(player.rect):
                    player.apply_powerup(pu.kind)
                    powerup_msg   = PowerUp.LABELS[pu.kind]
                    powerup_timer = 30
                    powerups.remove(pu)

            if powerup_timer > 0: powerup_timer -= 1

            # Player death
            if player.hp <= 0:
                player.lives -= 1
                if player.lives <= 0:
                    high_score = max(high_score, player.score)
                    state = "gameover"
                else:
                    player.hp = player.max_hp
                    player.rect.center = (WIDTH//2, HEIGHT - 80)
                    player.inv_timer = 120

            # Wave clear check
            if not boss and not wave_spawns and not enemies:
                wave_clear_delay += 1
                if wave_clear_delay >= 90:
                    wave_clear_delay = 0
                    next_wave()

        # ── Draw everything ───────────────────────────────────
        # Bullets
        for b in e_bullets: b.draw(screen)
        for b in p_bullets: b.draw(screen)

        # Power-ups
        for pu in powerups: pu.draw(screen, font_sm)

        # Enemies
        for en in enemies: en.draw(screen)
        if boss: boss.draw(screen)

        # Player
        player.draw(screen)

        # HUD
        draw_hud(screen, player, wave, font, font_sm,
                 powerup_msg, powerup_timer, boss if boss else None)

        # Wave banner
        draw_wave_banner(screen, font_big, wave, wave_banner_timer)
        if wave_banner_timer > 0 and state == "playing":
            wave_banner_timer -= 1

        # Overlays
        if state == "paused":
            draw_pause(screen, font_big, font)

        if state == "gameover":
            draw_game_over(screen, font_big, font, font_sm, player.score, high_score)

        pygame.display.flip()

# ─────────────────────────────────────────────────────────────
# ENTRY POINT
# ─────────────────────────────────────────────────────────────
def main():
    pygame.init()
    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    pygame.display.set_caption(TITLE)

    # Fonts (system fallback — no external font needed)
    font_big = pygame.font.SysFont("consolas", 54, bold=True)
    font     = pygame.font.SysFont("consolas", 30, bold=True)
    font_sm  = pygame.font.SysFont("consolas", 20)
    fonts    = (font_big, font, font_sm)

    high_score = 0
    restart    = True

    while restart:
        high_score, restart = game_loop(screen, fonts, high_score)

    pygame.quit()
    sys.exit()

if __name__ == "__main__":
    main()
