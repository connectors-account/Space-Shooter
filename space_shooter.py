"""
Space Shooter — Desktop Game
Requirements : pip install pygame
Run          : python space_shooter.py
Controls     : LEFT / RIGHT (or A / D) to move
               SPACE to shoot  |  P to pause  |  R to restart
"""

import pygame
import random
import math
import sys

# ── Constants ────────────────────────────────────────────────────────────────
W, H        = 800, 700
FPS         = 60
WHITE       = (255, 255, 255)
BLACK       = (  0,   0,   0)
DARK_BG     = (  8,   8,  22)
CYAN        = (  0, 220, 255)
RED         = (255,  50,  50)
ORANGE      = (255, 160,   0)
GREEN       = ( 50, 220,  80)
YELLOW      = (255, 240,  50)
PURPLE      = (180,  60, 255)
GRAY        = (140, 140, 140)
LIGHT_BLUE  = (100, 180, 255)
PINK        = (255, 100, 200)


# ── Helpers ──────────────────────────────────────────────────────────────────
def draw_text(surf, text, size, x, y, color=WHITE, center=True):
    font = pygame.font.SysFont("consolas", size, bold=True)
    img  = font.render(text, True, color)
    rect = img.get_rect(center=(x, y)) if center else img.get_rect(topleft=(x, y))
    surf.blit(img, rect)
    return rect


def draw_triangle(surf, color, cx, cy, w, h, pointing="up"):
    if pointing == "up":
        pts = [(cx, cy - h // 2), (cx - w // 2, cy + h // 2), (cx + w // 2, cy + h // 2)]
    else:
        pts = [(cx, cy + h // 2), (cx - w // 2, cy - h // 2), (cx + w // 2, cy - h // 2)]
    pygame.draw.polygon(surf, color, pts)


# ── Stars (parallax background) ──────────────────────────────────────────────
class Star:
    def __init__(self):
        self.reset(random.randint(0, H))

    def reset(self, y=0):
        self.x     = random.randint(0, W)
        self.y     = float(y)
        self.speed = random.uniform(0.5, 2.5)
        self.size  = 1 if self.speed < 1.2 else (2 if self.speed < 2 else 3)
        bright     = int(80 + self.speed * 60)
        self.color = (bright, bright, bright)

    def update(self):
        self.y += self.speed
        if self.y > H:
            self.reset()

    def draw(self, surf):
        pygame.draw.circle(surf, self.color, (int(self.x), int(self.y)), self.size)


# ── Player ───────────────────────────────────────────────────────────────────
class Player(pygame.sprite.Sprite):
    SPEED      = 5
    SHOOT_CD   = 15          # frames between shots
    MAX_LIVES  = 3
    INVULN_CD  = 90          # frames of invincibility after hit

    def __init__(self, groups):
        super().__init__(groups)
        self.image = pygame.Surface((40, 40), pygame.SRCALPHA)
        self._draw()
        self.rect         = self.image.get_rect(center=(W // 2, H - 70))
        self.lives        = self.MAX_LIVES
        self.score        = 0
        self._shoot_timer = 0
        self._inv_timer   = 0
        self._blink       = False

    def _draw(self):
        self.image.fill((0, 0, 0, 0))
        cx, cy = 20, 20
        # body
        draw_triangle(self.image, CYAN,        cx, cy, 36, 36, "up")
        draw_triangle(self.image, LIGHT_BLUE,  cx, cy, 22, 24, "up")
        # cockpit
        pygame.draw.circle(self.image, WHITE, (cx, cy - 4), 5)
        # engine glow
        pygame.draw.ellipse(self.image, ORANGE, (cx - 5, cy + 12, 10, 6))

    def update(self, keys):
        self._shoot_timer = max(0, self._shoot_timer - 1)
        if self._inv_timer > 0:
            self._inv_timer -= 1
            self._blink = (self._inv_timer % 8) < 4
        else:
            self._blink = False

        dx = 0
        if keys[pygame.K_LEFT]  or keys[pygame.K_a]: dx = -self.SPEED
        if keys[pygame.K_RIGHT] or keys[pygame.K_d]: dx =  self.SPEED
        self.rect.x = max(20, min(W - 20, self.rect.x + dx))

    def shoot(self, bullet_group, all_group):
        if self._shoot_timer == 0:
            Bullet(self.rect.centerx, self.rect.top, -12, YELLOW, [bullet_group, all_group])
            self._shoot_timer = self.SHOOT_CD

    def hit(self):
        if self._inv_timer == 0:
            self.lives -= 1
            self._inv_timer = self.INVULN_CD
            return True
        return False

    def draw(self, surf):
        if not self._blink:
            surf.blit(self.image, self.rect)
        # lives display (hearts)
        for i in range(self.lives):
            x = 30 + i * 28
            pygame.draw.polygon(surf, RED, [(x, H - 25), (x - 9, H - 35),
                                            (x - 14, H - 30), (x, H - 18),
                                            (x + 14, H - 30), (x + 9, H - 35)])


# ── Bullet ───────────────────────────────────────────────────────────────────
class Bullet(pygame.sprite.Sprite):
    def __init__(self, x, y, vy, color, groups):
        super().__init__(groups)
        self.image = pygame.Surface((4, 14), pygame.SRCALPHA)
        pygame.draw.rect(self.image, color, (0, 0, 4, 14), border_radius=2)
        self.rect  = self.image.get_rect(centerx=x, top=y)
        self.vy    = vy

    def update(self, *args):
        self.rect.y += self.vy
        if self.rect.bottom < 0 or self.rect.top > H:
            self.kill()


# ── Enemy ─────────────────────────────────────────────────────────────────────
class Enemy(pygame.sprite.Sprite):
    SHOOT_CD_RANGE = (90, 200)   # frames between enemy shots

    def __init__(self, x, y, wave, groups):
        super().__init__(groups)
        self.wave     = wave
        self.hp       = 1 + wave // 3
        self._shoot_t = random.randint(*self.SHOOT_CD_RANGE)
        self._angle   = 0.0
        self._start_x = float(x)

        # Speed scales with wave
        self.vy = 1.0 + wave * 0.15
        self.points = 100 + wave * 20

        # Pick a look based on wave
        kind = (wave - 1) % 3
        if kind == 0:
            self._color  = GREEN
            self._shape  = "diamond"
        elif kind == 1:
            self._color  = ORANGE
            self._shape  = "rect"
        else:
            self._color  = PINK
            self._shape  = "saucer"

        self.image = self._make_image()
        self.rect  = self.image.get_rect(center=(x, y))
        self._flash = 0

    def _make_image(self):
        img = pygame.Surface((40, 30), pygame.SRCALPHA)
        col = self._color
        if self._shape == "diamond":
            pts = [(20, 0), (40, 15), (20, 30), (0, 15)]
            pygame.draw.polygon(img, col, pts)
            pygame.draw.polygon(img, WHITE, pts, 2)
        elif self._shape == "rect":
            pygame.draw.rect(img, col, (2, 4, 36, 22), border_radius=5)
            pygame.draw.rect(img, WHITE, (2, 4, 36, 22), 2, border_radius=5)
            pygame.draw.circle(img, WHITE, (20, 15), 5)
        else:  # saucer
            pygame.draw.ellipse(img, col, (0, 8, 40, 16))
            pygame.draw.ellipse(img, WHITE, (0, 8, 40, 16), 2)
            pygame.draw.ellipse(img, col, (10, 2, 20, 14))
            pygame.draw.ellipse(img, WHITE, (10, 2, 20, 14), 2)
        return img

    def _flash_image(self):
        img = self.image.copy()
        flash = pygame.Surface(img.get_size(), pygame.SRCALPHA)
        flash.fill((255, 255, 255, 180))
        img.blit(flash, (0, 0), special_flags=pygame.BLEND_RGBA_ADD)
        return img

    def update(self, *args):
        self._angle += 0.04
        self.rect.x  = int(self._start_x + math.sin(self._angle) * 55)
        self.rect.y += self.vy
        self._flash  = max(0, self._flash - 1)
        if self.rect.top > H:
            self.kill()

    def try_shoot(self, ebullet_group, all_group):
        self._shoot_t -= 1
        if self._shoot_t <= 0:
            self._shoot_t = random.randint(*self.SHOOT_CD_RANGE)
            EnemyBullet(self.rect.centerx, self.rect.bottom, [ebullet_group, all_group])

    def hit(self):
        self.hp -= 1
        self._flash = 6
        return self.hp <= 0

    def draw(self, surf):
        if self._flash > 0:
            surf.blit(self._flash_image(), self.rect)
        else:
            surf.blit(self.image, self.rect)
        # HP pips
        for i in range(self.hp):
            pygame.draw.circle(surf, RED, (self.rect.left + 5 + i * 10, self.rect.top - 6), 3)


# ── Enemy Bullet ─────────────────────────────────────────────────────────────
class EnemyBullet(pygame.sprite.Sprite):
    def __init__(self, x, y, groups):
        super().__init__(groups)
        self.image = pygame.Surface((5, 14), pygame.SRCALPHA)
        pygame.draw.rect(self.image, RED, (0, 0, 5, 14), border_radius=2)
        self.rect  = self.image.get_rect(centerx=x, top=y)
        self.vy    = 6

    def update(self, *args):
        self.rect.y += self.vy
        if self.rect.top > H:
            self.kill()


# ── Explosion ─────────────────────────────────────────────────────────────────
class Explosion(pygame.sprite.Sprite):
    def __init__(self, x, y, groups):
        super().__init__(groups)
        self.x, self.y = x, y
        self._frame    = 0
        self._max      = 20
        self._particles = [(random.uniform(-3, 3), random.uniform(-3, 3),
                            random.choice([ORANGE, YELLOW, RED, WHITE]))
                           for _ in range(12)]
        self.image = pygame.Surface((2, 2), pygame.SRCALPHA)
        self.rect  = self.image.get_rect(center=(x, y))

    def update(self, *args):
        self._frame += 1
        if self._frame >= self._max:
            self.kill()

    def draw(self, surf):
        t   = self._frame / self._max
        rad = int(t * 30)
        for dx, dy, col in self._particles:
            px = int(self.x + dx * rad)
            py = int(self.y + dy * rad)
            alpha = max(0, int(255 * (1 - t)))
            size  = max(1, int(4 * (1 - t)))
            s = pygame.Surface((size * 2, size * 2), pygame.SRCALPHA)
            pygame.draw.circle(s, (*col, alpha), (size, size), size)
            surf.blit(s, (px - size, py - size))


# ── Power-up ─────────────────────────────────────────────────────────────────
class PowerUp(pygame.sprite.Sprite):
    TYPES = ["double", "speed", "life"]
    COLORS = {"double": YELLOW, "speed": GREEN, "life": RED}
    LABELS = {"double": "2x", "speed": ">>", "life": "♥"}

    def __init__(self, x, y, groups):
        super().__init__(groups)
        self.kind  = random.choice(self.TYPES)
        self.image = pygame.Surface((28, 28), pygame.SRCALPHA)
        col = self.COLORS[self.kind]
        pygame.draw.circle(self.image, col,    (14, 14), 13)
        pygame.draw.circle(self.image, WHITE,  (14, 14), 13, 2)
        draw_text(self.image, self.LABELS[self.kind], 11, 14, 14, BLACK)
        self.rect  = self.image.get_rect(center=(x, y))
        self._t    = 0

    def update(self, *args):
        self._t   += 0.06
        self.rect.y += 1
        # bob
        self.rect.x = int(self.rect.x)
        if self.rect.top > H:
            self.kill()

    def draw(self, surf):
        surf.blit(self.image, self.rect)


# ── Game ──────────────────────────────────────────────────────────────────────
class Game:
    ENEMIES_PER_WAVE = 8

    def __init__(self):
        pygame.init()
        self.screen  = pygame.display.set_mode((W, H))
        pygame.display.set_caption("Space Shooter")
        self.clock   = pygame.time.Clock()
        self.stars   = [Star() for _ in range(120)]
        self._reset()

    def _reset(self):
        self.all_sprites   = pygame.sprite.Group()
        self.bullets       = pygame.sprite.Group()
        self.enemy_bullets = pygame.sprite.Group()
        self.enemies       = pygame.sprite.Group()
        self.explosions    = []
        self.powerups      = pygame.sprite.Group()

        self.player        = Player([self.all_sprites])
        self.wave          = 1
        self._spawn_count  = 0
        self._spawn_delay  = 50
        self._spawn_timer  = 0
        self._wave_clear_t = 0
        self._double_shot  = 0
        self._speed_boost  = 0
        self.state         = "playing"   # playing | paused | gameover
        self._spawn_wave()

    # ── Wave spawning ────────────────────────────────────────────────────────
    def _spawn_wave(self):
        count = self.ENEMIES_PER_WAVE + (self.wave - 1) * 2
        cols  = min(count, 8)
        rows  = (count + cols - 1) // cols
        for row in range(rows):
            for col in range(cols):
                idx = row * cols + col
                if idx >= count:
                    break
                x = 80 + col * ((W - 160) // (cols - 1)) if cols > 1 else W // 2
                y = -60 - row * 70
                Enemy(x, y, self.wave, [self.enemies, self.all_sprites])
        self._spawn_count = count

    # ── Main loop ────────────────────────────────────────────────────────────
    def run(self):
        while True:
            dt   = self.clock.tick(FPS)
            keys = pygame.key.get_pressed()
            self._handle_events(keys)

            if self.state == "playing":
                self._update(keys)

            self._draw()

    def _handle_events(self, keys):
        for e in pygame.event.get():
            if e.type == pygame.QUIT:
                pygame.quit(); sys.exit()
            if e.type == pygame.KEYDOWN:
                if e.key == pygame.K_p and self.state == "playing":
                    self.state = "paused"
                elif e.key == pygame.K_p and self.state == "paused":
                    self.state = "playing"
                elif e.key == pygame.K_r:
                    self._reset()
                elif e.key == pygame.K_ESCAPE:
                    pygame.quit(); sys.exit()

    def _update(self, keys):
        # Stars
        for s in self.stars:
            s.update()

        # Player move
        speed = self.player.SPEED + (2 if self._speed_boost > 0 else 0)
        self.player.SPEED = speed if self._speed_boost > 0 else 5
        self.player.update(keys)

        # Player shoot
        if keys[pygame.K_SPACE]:
            self.player.shoot(self.bullets, self.all_sprites)
            if self._double_shot > 0:
                # second bullet offset
                Bullet(self.player.rect.centerx - 12,
                       self.player.rect.top, -12, YELLOW,
                       [self.bullets, self.all_sprites])

        # Timers
        self._double_shot  = max(0, self._double_shot  - 1)
        self._speed_boost  = max(0, self._speed_boost  - 1)

        # Enemies
        for enemy in list(self.enemies):
            enemy.update()
            enemy.try_shoot(self.enemy_bullets, self.all_sprites)

        self.bullets.update()
        self.enemy_bullets.update()
        self.powerups.update()

        # Explosions
        for ex in list(self.explosions):
            ex.update()
            if not ex.alive():
                self.explosions.remove(ex)

        # Bullet ↔ Enemy collision
        hits = pygame.sprite.groupcollide(self.enemies, self.bullets, False, True)
        for enemy, blist in hits.items():
            if enemy.hit():
                self.explosions.append(Explosion(enemy.rect.centerx,
                                                  enemy.rect.centery, []))
                self.player.score += enemy.points
                # chance to drop power-up
                if random.random() < 0.18:
                    PowerUp(enemy.rect.centerx, enemy.rect.centery,
                            [self.powerups, self.all_sprites])
                enemy.kill()

        # Enemy bullet ↔ Player collision
        if pygame.sprite.spritecollide(self.player, self.enemy_bullets, True):
            if self.player.hit():
                self.explosions.append(Explosion(self.player.rect.centerx,
                                                  self.player.rect.centery, []))
                if self.player.lives <= 0:
                    self.state = "gameover"

        # Enemy ↔ Player collision (ramming)
        for enemy in pygame.sprite.spritecollide(self.player, self.enemies, True):
            if self.player.hit():
                if self.player.lives <= 0:
                    self.state = "gameover"

        # Power-up ↔ Player
        for pu in pygame.sprite.spritecollide(self.player, self.powerups, True):
            if pu.kind == "double":
                self._double_shot = FPS * 8
            elif pu.kind == "speed":
                self._speed_boost = FPS * 6
            elif pu.kind == "life" and self.player.lives < self.player.MAX_LIVES:
                self.player.lives += 1

        # Next wave?
        if len(self.enemies) == 0:
            self._wave_clear_t += 1
            if self._wave_clear_t > 90:
                self.wave += 1
                self._wave_clear_t = 0
                self._spawn_wave()

    # ── Draw ─────────────────────────────────────────────────────────────────
    def _draw(self):
        self.screen.fill(DARK_BG)

        for s in self.stars:
            s.draw(self.screen)

        for pu in self.powerups:
            pu.draw(self.screen)

        for enemy in self.enemies:
            enemy.draw(self.screen)

        self.bullets.draw(self.screen)
        self.enemy_bullets.draw(self.screen)

        for ex in self.explosions:
            ex.draw(self.screen)

        self.player.draw(self.screen)

        # HUD
        draw_text(self.screen, f"SCORE  {self.player.score:06d}", 22, W // 2, 22)
        draw_text(self.screen, f"WAVE {self.wave}", 20, W - 70, 22, CYAN)

        # Power-up indicator
        if self._double_shot > 0:
            draw_text(self.screen, "DOUBLE SHOT", 16, W // 2, H - 22, YELLOW)
        elif self._speed_boost > 0:
            draw_text(self.screen, "SPEED BOOST", 16, W // 2, H - 22, GREEN)

        # Wave-clear message
        if self._wave_clear_t > 0:
            draw_text(self.screen, f"WAVE {self.wave - 1} CLEARED!", 36, W // 2, H // 2, YELLOW)

        if self.state == "paused":
            self._draw_overlay("PAUSED", "Press P to resume  |  R to restart  |  ESC to quit")

        if self.state == "gameover":
            self._draw_overlay(f"GAME OVER",
                               f"Score: {self.player.score}   Wave: {self.wave}   |   R to restart")

        pygame.display.flip()

    def _draw_overlay(self, title, subtitle):
        s = pygame.Surface((W, H), pygame.SRCALPHA)
        s.fill((0, 0, 0, 160))
        self.screen.blit(s, (0, 0))
        draw_text(self.screen, title,    56, W // 2, H // 2 - 40, YELLOW)
        draw_text(self.screen, subtitle, 22, W // 2, H // 2 + 30, WHITE)


# ── Entry point ──────────────────────────────────────────────────────────────
if __name__ == "__main__":
    Game().run()
