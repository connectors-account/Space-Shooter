"""
Space Shooter — Single-file Python + Pygame Desktop Game
Controls: Arrow Keys / WASD to move | Space to shoot | P to pause | Esc to quit
Requires: pip install pygame
Run:      python space_shooter.py
"""

import pygame
import random
import math
import sys

# ─────────────────────────────────────────────
#  CONSTANTS
# ─────────────────────────────────────────────
WIDTH, HEIGHT = 800, 700
FPS           = 60
TITLE         = "Space Shooter"

# Colours
BLACK     = (0,   0,   0)
WHITE     = (255, 255, 255)
RED       = (220,  30,  30)
GREEN     = (0,   220,  80)
BLUE      = (30,  120, 255)
CYAN      = (0,   220, 220)
YELLOW    = (255, 220,   0)
ORANGE    = (255, 140,   0)
PURPLE    = (160,  0,  220)
DARK_GREY = (20,   20,  30)
GREY      = (100, 100, 120)

# ─────────────────────────────────────────────
#  HELPER — draw pixel-art shapes
# ─────────────────────────────────────────────

def make_surface(w, h, color_key=BLACK):
    surf = pygame.Surface((w, h))
    surf.fill(color_key)
    surf.set_colorkey(color_key)
    return surf


def player_surface():
    s = make_surface(40, 48)
    # Body
    pygame.draw.polygon(s, CYAN, [(20,0),(38,40),(20,32),(2,40)])
    # Cockpit
    pygame.draw.polygon(s, WHITE, [(20,10),(28,28),(20,24),(12,28)])
    # Engine glow
    pygame.draw.ellipse(s, ORANGE, (13, 38, 14, 10))
    return s


def enemy_surface(kind):
    s = make_surface(36, 28)
    if kind == "basic":
        pygame.draw.polygon(s, RED, [(18,0),(34,20),(26,28),(10,28),(2,20)])
        pygame.draw.circle(s, YELLOW, (18,14), 5)
    elif kind == "fast":
        pygame.draw.polygon(s, ORANGE, [(18,0),(36,14),(28,28),(8,28),(0,14)])
        pygame.draw.circle(s, WHITE, (18,12), 4)
    else:  # tank
        pygame.draw.rect(s, PURPLE, (4, 4, 28, 20))
        pygame.draw.rect(s, (100,0,150), (0, 8, 36, 12))
        pygame.draw.circle(s, WHITE, (18,14), 5)
    return s


def bullet_surface(color, w=6, h=14):
    s = make_surface(w, h)
    pygame.draw.ellipse(s, color, (0, 0, w, h))
    return s


def powerup_surface(kind):
    s = make_surface(24, 24)
    colors = {"weapon": GREEN, "shield": CYAN, "life": RED, "bomb": YELLOW}
    c = colors.get(kind, WHITE)
    pygame.draw.circle(s, c, (12, 12), 10)
    pygame.draw.circle(s, WHITE, (12, 12), 5)
    return s


def explosion_frames():
    frames = []
    for radius in range(4, 32, 4):
        s = make_surface(64, 64)
        alpha_val = max(0, 255 - radius * 7)
        col = (255, max(0, 200 - radius * 5), 0)
        pygame.draw.circle(s, col, (32, 32), radius)
        frames.append(s)
    return frames


# ─────────────────────────────────────────────
#  STAR BACKGROUND
# ─────────────────────────────────────────────
class StarField:
    def __init__(self, count=120):
        self.stars = []
        for _ in range(count):
            x     = random.randint(0, WIDTH)
            y     = random.randint(0, HEIGHT)
            speed = random.uniform(0.5, 3.0)
            size  = 1 if speed < 1.5 else 2
            bright = int(80 + speed * 50)
            color = (bright, bright, bright)
            self.stars.append([x, y, speed, size, color])

    def update(self):
        for star in self.stars:
            star[1] += star[2]
            if star[1] > HEIGHT:
                star[1] = 0
                star[0] = random.randint(0, WIDTH)

    def draw(self, screen):
        for x, y, speed, size, color in self.stars:
            pygame.draw.circle(screen, color, (int(x), int(y)), size)


# ─────────────────────────────────────────────
#  EXPLOSION
# ─────────────────────────────────────────────
class Explosion(pygame.sprite.Sprite):
    FRAMES = None

    def __init__(self, pos):
        super().__init__()
        if Explosion.FRAMES is None:
            Explosion.FRAMES = explosion_frames()
        self.frames  = Explosion.FRAMES
        self.index   = 0
        self.image   = self.frames[0]
        self.rect    = self.image.get_rect(center=pos)
        self.timer   = 0

    def update(self):
        self.timer += 1
        if self.timer % 3 == 0:
            self.index += 1
            if self.index >= len(self.frames):
                self.kill()
            else:
                self.image = self.frames[self.index]
                self.rect  = self.image.get_rect(center=self.rect.center)


# ─────────────────────────────────────────────
#  BULLET
# ─────────────────────────────────────────────
class Bullet(pygame.sprite.Sprite):
    def __init__(self, pos, vel, damage, color, w=6, h=14):
        super().__init__()
        self.image  = bullet_surface(color, w, h)
        self.rect   = self.image.get_rect(center=pos)
        self.vel    = vel          # (vx, vy) floats
        self.damage = damage
        self.fx     = float(self.rect.centerx)
        self.fy     = float(self.rect.centery)

    def update(self):
        self.fx += self.vel[0]
        self.fy += self.vel[1]
        self.rect.centerx = int(self.fx)
        self.rect.centery  = int(self.fy)
        if self.rect.bottom < 0 or self.rect.top > HEIGHT \
           or self.rect.right < 0 or self.rect.left > WIDTH:
            self.kill()


# ─────────────────────────────────────────────
#  POWER-UP
# ─────────────────────────────────────────────
class PowerUp(pygame.sprite.Sprite):
    KINDS = ["weapon", "shield", "life", "bomb"]

    def __init__(self, pos):
        super().__init__()
        self.kind   = random.choice(self.KINDS)
        self.image  = powerup_surface(self.kind)
        self.rect   = self.image.get_rect(center=pos)
        self.fy     = float(self.rect.centery)
        self.angle  = 0
        self.life   = FPS * 8   # despawn after 8 s

    def update(self):
        self.fy    += 1.2
        self.rect.centery = int(self.fy)
        self.angle = (self.angle + 3) % 360
        self.life -= 1
        if self.rect.top > HEIGHT or self.life <= 0:
            self.kill()

    def draw(self, screen):
        # Glow pulse
        pulse = int(abs(math.sin(pygame.time.get_ticks() * 0.005)) * 60)
        colors = {"weapon": (0, 220+pulse//3, 80),
                  "shield": (0, 220, 220),
                  "life":   (220, 30+pulse, 30),
                  "bomb":   (255, 220-pulse//2, 0)}
        pygame.draw.circle(screen, colors[self.kind],
                           self.rect.center, 14, 2)
        screen.blit(self.image, self.rect)


# ─────────────────────────────────────────────
#  PLAYER
# ─────────────────────────────────────────────
class Player(pygame.sprite.Sprite):
    SPEED      = 5
    FIRE_DELAY = 15          # frames between shots
    MAX_HP     = 5
    MAX_SHIELD = 1

    def __init__(self, groups):
        super().__init__(*groups)
        self._base  = player_surface()
        self.image  = self._base.copy()
        self.rect   = self.image.get_rect(center=(WIDTH // 2, HEIGHT - 80))
        self.fx     = float(self.rect.centerx)
        self.fy     = float(self.rect.centery)

        self.hp         = self.MAX_HP
        self.shield     = 0
        self.weapon_lvl = 1           # 1–5
        self.fire_timer = 0
        self.inv_timer  = 0           # invincibility frames after hit
        self.score_mult = 1

    # ── movement ──────────────────────────────
    def handle_input(self, keys):
        dx = dy = 0
        if keys[pygame.K_LEFT]  or keys[pygame.K_a]: dx -= 1
        if keys[pygame.K_RIGHT] or keys[pygame.K_d]: dx += 1
        if keys[pygame.K_UP]    or keys[pygame.K_w]: dy -= 1
        if keys[pygame.K_DOWN]  or keys[pygame.K_s]: dy += 1

        self.fx = max(20, min(WIDTH  - 20, self.fx + dx * self.SPEED))
        self.fy = max(20, min(HEIGHT - 20, self.fy + dy * self.SPEED))
        self.rect.center = (int(self.fx), int(self.fy))

        # Tilt sprite
        if dx != 0:
            self.image = pygame.transform.rotate(self._base, -dx * 12)
        else:
            self.image = self._base.copy()

    # ── shooting ──────────────────────────────
    def shoot(self, all_sprites, player_bullets):
        if self.fire_timer > 0:
            return
        self.fire_timer = self.FIRE_DELAY

        cx, cy = self.rect.centerx, self.rect.top + 4

        patterns = {
            1: [(0, -14)],
            2: [(-10, -14), (10, -14)],
            3: [(-12, -12), (0, -14), (12, -12)],
            4: [(-16, -10), (-6, -14), (6, -14), (16, -10)],
            5: [(-18, -8), (-8, -14), (0, -15), (8, -14), (18, -8)],
        }
        offsets = patterns.get(self.weapon_lvl, patterns[1])
        for ox, oy in offsets:
            b = Bullet((cx + ox, cy), (ox * 0.25, oy), 1, CYAN)
            all_sprites.add(b)
            player_bullets.add(b)

    # ── take damage ───────────────────────────
    def hit(self, amount=1):
        if self.inv_timer > 0:
            return
        if self.shield > 0:
            self.shield -= 1
            self.inv_timer = 40
            return
        self.hp       -= amount
        self.inv_timer = 60

    # ── update ────────────────────────────────
    def update(self):
        if self.fire_timer > 0:
            self.fire_timer -= 1
        if self.inv_timer > 0:
            self.inv_timer -= 1
            # Flash while invincible
            self.image.set_alpha(80 if (self.inv_timer // 6) % 2 == 0 else 255)
        else:
            self.image.set_alpha(255)

    # ── power-up pickup ───────────────────────
    def collect(self, pu):
        if pu.kind == "weapon":
            self.weapon_lvl = min(5, self.weapon_lvl + 1)
        elif pu.kind == "shield":
            self.shield = self.MAX_SHIELD
        elif pu.kind == "life":
            self.hp = min(self.MAX_HP, self.hp + 1)
        elif pu.kind == "bomb":
            return "bomb"
        return None


# ─────────────────────────────────────────────
#  ENEMIES
# ─────────────────────────────────────────────
class Enemy(pygame.sprite.Sprite):
    CONFIGS = {
        "basic": dict(hp=2, speed=2.0, score=100,  fire_delay=90,  reward_prob=0.15),
        "fast":  dict(hp=1, speed=3.5, score=150,  fire_delay=120, reward_prob=0.10),
        "tank":  dict(hp=5, speed=1.2, score=250,  fire_delay=60,  reward_prob=0.25),
    }

    def __init__(self, kind, pos, groups):
        super().__init__(*groups)
        cfg        = self.CONFIGS[kind]
        self.kind  = kind
        self.image = enemy_surface(kind)
        self.rect  = self.image.get_rect(center=pos)
        self.fx    = float(pos[0])
        self.fy    = float(pos[1])

        self.hp          = cfg["hp"]
        self.speed       = cfg["speed"]
        self.score_val   = cfg["score"]
        self.fire_delay  = cfg["fire_delay"]
        self.fire_timer  = random.randint(0, self.fire_delay)
        self.reward_prob = cfg["reward_prob"]

        # Sine / zigzag state
        self.t           = random.uniform(0, math.pi * 2)

    def update(self):
        self.fy += self.speed
        self.t  += 0.07

        if self.kind == "fast":
            self.fx += math.sin(self.t) * 3
        elif self.kind == "tank":
            self.fx += math.sin(self.t * 0.5) * 1.5

        self.fx = max(18, min(WIDTH - 18, self.fx))
        self.rect.center = (int(self.fx), int(self.fy))

        if self.rect.top > HEIGHT:
            self.kill()

    def try_fire(self, player_pos, all_sprites, enemy_bullets):
        self.fire_timer -= 1
        if self.fire_timer > 0:
            return
        self.fire_timer = self.fire_delay

        dx = player_pos[0] - self.rect.centerx
        dy = player_pos[1] - self.rect.centery
        dist = math.hypot(dx, dy) or 1
        spd  = 5 if self.kind == "tank" else 4
        vx   = dx / dist * spd
        vy   = dy / dist * spd

        b = Bullet(self.rect.center, (vx, vy), 1, RED, 5, 10)
        all_sprites.add(b)
        enemy_bullets.add(b)

    def hit(self, damage):
        self.hp -= damage
        return self.hp <= 0

    def drop_powerup(self, all_sprites, powerups):
        if random.random() < self.reward_prob:
            pu = PowerUp(self.rect.center)
            all_sprites.add(pu)
            powerups.add(pu)


# ─────────────────────────────────────────────
#  WAVE MANAGER
# ─────────────────────────────────────────────
class WaveManager:
    def __init__(self):
        self.wave       = 0
        self.spawn_buf  = []   # list of (kind, delay) queued spawns
        self.spawn_timer = 0
        self.active     = False

    def start_next(self):
        self.wave   += 1
        self.active  = True
        self.spawn_buf = self._build_wave(self.wave)
        self.spawn_timer = 0

    def _build_wave(self, w):
        buf = []
        # Composition changes per wave
        n_basic = 4 + w * 2
        n_fast  = max(0, w - 1) * 2
        n_tank  = max(0, w - 2)
        delay   = max(20, 50 - w * 3)

        for _ in range(n_basic):
            buf.append(("basic", delay))
        for _ in range(n_fast):
            buf.append(("fast", delay))
        for _ in range(n_tank):
            buf.append(("tank", delay))

        random.shuffle(buf)
        return buf

    def update(self, all_sprites, enemies):
        if not self.active:
            return
        if not self.spawn_buf:
            return

        self.spawn_timer -= 1
        if self.spawn_timer > 0:
            return

        kind, delay    = self.spawn_buf.pop(0)
        self.spawn_timer = delay
        x = random.randint(30, WIDTH - 30)
        e = Enemy(kind, (x, -20), [all_sprites, enemies])
        return e

    @property
    def wave_done(self):
        return self.active and not self.spawn_buf


# ─────────────────────────────────────────────
#  HUD
# ─────────────────────────────────────────────
def draw_hud(screen, fonts, player, score, hi_score, wave, paused):
    sm, lg = fonts

    # Score
    screen.blit(sm.render(f"SCORE  {score:>7}", True, WHITE), (10, 10))
    screen.blit(sm.render(f"BEST   {hi_score:>7}", True, GREY),  (10, 34))

    # Wave
    wave_txt = sm.render(f"WAVE  {wave}", True, YELLOW)
    screen.blit(wave_txt, (WIDTH - wave_txt.get_width() - 10, 10))

    # Weapon level
    lvl_txt = sm.render(f"WPN  {'★'*player.weapon_lvl}{'☆'*(5-player.weapon_lvl)}", True, CYAN)
    screen.blit(lvl_txt, (WIDTH - lvl_txt.get_width() - 10, 34))

    # HP hearts
    for i in range(player.MAX_HP):
        col = RED if i < player.hp else DARK_GREY
        pygame.draw.polygon(screen, col, _heart(10 + i * 26, HEIGHT - 30, 10))

    # Shield indicator
    if player.shield > 0:
        sh = sm.render("SHIELD ●", True, CYAN)
        screen.blit(sh, (10, HEIGHT - 54))

    # Pause overlay
    if paused:
        overlay = pygame.Surface((WIDTH, HEIGHT), pygame.SRCALPHA)
        overlay.fill((0, 0, 0, 140))
        screen.blit(overlay, (0, 0))
        ptxt = lg.render("PAUSED", True, WHITE)
        screen.blit(ptxt, ptxt.get_rect(center=(WIDTH//2, HEIGHT//2 - 20)))
        htxt = sm.render("Press P to resume", True, GREY)
        screen.blit(htxt, htxt.get_rect(center=(WIDTH//2, HEIGHT//2 + 30)))


def _heart(cx, cy, r):
    pts = []
    for i in range(360):
        ang = math.radians(i)
        x = r * (16 * math.sin(ang) ** 3) / 16
        y = -r * (13*math.cos(ang) - 5*math.cos(2*ang) - 2*math.cos(3*ang) - math.cos(4*ang)) / 16
        pts.append((cx + x, cy + y))
    return pts


# ─────────────────────────────────────────────
#  SCREENS
# ─────────────────────────────────────────────
def draw_menu(screen, fonts, hi_score, tick):
    sm, lg = fonts
    screen.fill(DARK_GREY)

    # Animated title
    scale = 1 + 0.04 * math.sin(tick * 0.05)
    title = lg.render("SPACE SHOOTER", True, CYAN)
    tw, th = title.get_size()
    big    = pygame.transform.scale(title, (int(tw * scale), int(th * scale)))
    screen.blit(big, big.get_rect(center=(WIDTH//2, 160)))

    lines = [
        ("Press SPACE to Play", WHITE, 280),
        ("Move:  Arrow Keys / WASD",     GREY,  360),
        ("Fire:  SPACE",                 GREY,  390),
        ("Pause: P",                     GREY,  420),
        (f"Best Score: {hi_score}", YELLOW, 490),
    ]
    for text, color, y in lines:
        surf = sm.render(text, True, color)
        screen.blit(surf, surf.get_rect(center=(WIDTH//2, y)))


def draw_game_over(screen, fonts, score, hi_score, new_best):
    sm, lg = fonts
    overlay = pygame.Surface((WIDTH, HEIGHT), pygame.SRCALPHA)
    overlay.fill((0, 0, 0, 180))
    screen.blit(overlay, (0, 0))

    screen.blit(lg.render("GAME OVER", True, RED),
                lg.render("GAME OVER", True, RED).get_rect(center=(WIDTH//2, 220)))

    screen.blit(sm.render(f"Score: {score}", True, WHITE),
                sm.render(f"Score: {score}", True, WHITE).get_rect(center=(WIDTH//2, 300)))

    if new_best:
        screen.blit(sm.render("★  NEW BEST!  ★", True, YELLOW),
                    sm.render("★  NEW BEST!  ★", True, YELLOW).get_rect(center=(WIDTH//2, 340)))

    screen.blit(sm.render(f"Best: {hi_score}", True, GREY),
                sm.render(f"Best: {hi_score}", True, GREY).get_rect(center=(WIDTH//2, 380)))

    screen.blit(sm.render("Press SPACE to restart  |  ESC to quit", True, WHITE),
                sm.render("Press SPACE to restart  |  ESC to quit", True, WHITE).get_rect(center=(WIDTH//2, 460)))


# ─────────────────────────────────────────────
#  MAIN GAME LOOP
# ─────────────────────────────────────────────
def play(screen, clock, fonts, hi_score):
    all_sprites    = pygame.sprite.Group()
    enemies        = pygame.sprite.Group()
    player_bullets = pygame.sprite.Group()
    enemy_bullets  = pygame.sprite.Group()
    powerups       = pygame.sprite.Group()

    stars  = StarField()
    player = Player([all_sprites])
    wave_mgr = WaveManager()
    wave_mgr.start_next()

    score      = 0
    paused     = False
    tick       = 0
    wave_delay = 0   # countdown between waves

    running = True
    while running:
        clock.tick(FPS)
        tick += 1

        # ── Events ───────────────────────────
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                return score, hi_score, "quit"
            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    return score, hi_score, "quit"
                if event.key == pygame.K_p:
                    paused = not paused

        if paused:
            draw_hud(screen, fonts, player, score, hi_score, wave_mgr.wave, True)
            pygame.display.flip()
            continue

        # ── Input ────────────────────────────
        keys = pygame.key.get_pressed()
        player.handle_input(keys)
        if keys[pygame.K_SPACE] or keys[pygame.K_z]:
            player.shoot(all_sprites, player_bullets)

        # ── Wave logic ───────────────────────
        if wave_delay > 0:
            wave_delay -= 1
        else:
            if wave_mgr.wave_done and not enemies:
                wave_delay = FPS * 3   # 3 s break
                wave_mgr.start_next()
            wave_mgr.update(all_sprites, enemies)

        # ── Update ───────────────────────────
        stars.update()
        all_sprites.update()

        # Player bullets hit enemies
        hits = pygame.sprite.groupcollide(enemies, player_bullets,
                                          False, True)
        for enemy, bullets in hits.items():
            for b in bullets:
                if enemy.hit(b.damage):
                    Explosion(enemy.rect.center).add(all_sprites)
                    enemy.drop_powerup(all_sprites, powerups)
                    score += enemy.score_val
                    enemy.kill()
                else:
                    # Flash tint
                    enemy.image.fill((255, 120, 120), special_flags=pygame.BLEND_MULT)

        # Enemy bullets hit player
        if pygame.sprite.spritecollide(player, enemy_bullets, True):
            player.hit(1)

        # Enemies collide with player
        if pygame.sprite.spritecollide(player, enemies, False):
            player.hit(1)

        # Enemies fire
        for e in enemies:
            e.try_fire(player.rect.center, all_sprites, enemy_bullets)

        # Power-up collection
        pu_hits = pygame.sprite.spritecollide(player, powerups, True)
        for pu in pu_hits:
            result = player.collect(pu)
            if result == "bomb":
                for e in list(enemies):
                    Explosion(e.rect.center).add(all_sprites)
                    score += e.score_val
                    e.kill()
                for eb in list(enemy_bullets):
                    eb.kill()

        # Update hi-score
        if score > hi_score:
            hi_score = score

        # ── Death check ──────────────────────
        if player.hp <= 0:
            return score, hi_score, "dead"

        # ── Draw ─────────────────────────────
        screen.fill(DARK_GREY)
        stars.draw(screen)

        # Draw power-ups with glow
        for pu in powerups:
            pu.draw(screen)

        # Draw remaining sprites (skip powerups, drawn above)
        for sp in all_sprites:
            if not isinstance(sp, PowerUp):
                screen.blit(sp.image, sp.rect)

        draw_hud(screen, fonts, player, score, hi_score, wave_mgr.wave, False)

        # Wave announcement
        if wave_delay > FPS * 2:
            sm, lg = fonts
            wt = lg.render(f"WAVE  {wave_mgr.wave}", True, YELLOW)
            screen.blit(wt, wt.get_rect(center=(WIDTH//2, HEIGHT//2)))

        pygame.display.flip()

    return score, hi_score, "quit"


# ─────────────────────────────────────────────
#  ENTRY POINT
# ─────────────────────────────────────────────
def main():
    pygame.init()
    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    pygame.display.set_caption(TITLE)
    clock  = pygame.time.Clock()

    sm = pygame.font.SysFont("consolas", 22, bold=True)
    lg = pygame.font.SysFont("consolas", 48, bold=True)
    fonts = (sm, lg)

    hi_score = 0
    state    = "menu"
    tick     = 0

    while True:
        clock.tick(FPS)
        tick += 1

        if state == "menu":
            stars = StarField(80)
            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    pygame.quit(); sys.exit()
                if event.type == pygame.KEYDOWN:
                    if event.key == pygame.K_ESCAPE:
                        pygame.quit(); sys.exit()
                    if event.key == pygame.K_SPACE:
                        state = "play"
            stars.update()
            screen.fill(DARK_GREY)
            stars.draw(screen)
            draw_menu(screen, fonts, hi_score, tick)
            pygame.display.flip()

        elif state == "play":
            score, hi_score, outcome = play(screen, clock, fonts, hi_score)
            if outcome == "quit":
                pygame.quit(); sys.exit()
            state      = "gameover"
            last_score = score
            new_best   = (score == hi_score and score > 0)

        elif state == "gameover":
            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    pygame.quit(); sys.exit()
                if event.type == pygame.KEYDOWN:
                    if event.key == pygame.K_ESCAPE:
                        pygame.quit(); sys.exit()
                    if event.key == pygame.K_SPACE:
                        state = "play"
            draw_game_over(screen, fonts, last_score, hi_score, new_best)
            pygame.display.flip()


if __name__ == "__main__":
    main()
