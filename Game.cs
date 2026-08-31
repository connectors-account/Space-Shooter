// ============================================================
//  STAR VOID  —  Space Shooter
//  Single-file C# WinForms desktop game (no engine needed)
//  Build:  dotnet build -c Release
//  Run:    bin\Release\net8.0-windows\StarVoid.exe
// ============================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace StarVoid
{
    // ── Entry point ────────────────────────────────────────────
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new GameForm());
        }
    }

    // ── Enums ──────────────────────────────────────────────────
    enum Scene      { Menu, Playing, Paused, GameOver }
    enum EnemyKind  { Scout, Fighter, Tank }
    enum FireMode   { Single, Double, Triple }
    enum PickupKind { DoubleShot, TripleShot, Shield, Heal, Bomb }

    // ── Main window / game loop ────────────────────────────────
    class GameForm : Form
    {
        // ── Constants ─────────────────────────────────────────
        const int W = 800, H = 900, FPS = 60;

        // ── Game state ────────────────────────────────────────
        Scene scene = Scene.Menu;
        int   score, highScore, wave;
        int   waveTotal, waveKilled;
        float spawnCooldown, spawnInterval;
        bool  showClear; float clearTimer;

        // ── Entity lists ──────────────────────────────────────
        Player         player = null!;
        List<Enemy>    enemies  = new();
        List<Bullet>   bullets  = new();
        List<Pickup>   pickups  = new();
        List<Particle> particles= new();
        List<Star>     stars    = new();

        // ── Input ─────────────────────────────────────────────
        readonly bool[] keys = new bool[256];

        // ── Timing ────────────────────────────────────────────
        DateTime lastTick = DateTime.Now;
        readonly Random rng = new();

        // ── Constructor ───────────────────────────────────────
        public GameForm()
        {
            Text            = "STAR VOID";
            ClientSize      = new Size(W, H);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            BackColor       = Color.Black;
            DoubleBuffered  = true;
            StartPosition   = FormStartPosition.CenterScreen;

            KeyDown += (_, e) => { SafeKey(e.KeyCode, true);  OnKey(e.KeyCode); };
            KeyUp   += (_, e) =>   SafeKey(e.KeyCode, false);
            Paint   += (_, e) =>   Draw(e.Graphics);

            MakeStars();

            var t = new System.Windows.Forms.Timer { Interval = 1000 / FPS };
            t.Tick += Loop;
            t.Start();
        }

        void SafeKey(Keys k, bool v) { if ((int)k < 256) keys[(int)k] = v; }

        // ── One-shot key events (menus / pause) ───────────────
        void OnKey(Keys k)
        {
            if (scene == Scene.Menu     && k == Keys.Return)  Begin();
            if (scene == Scene.GameOver && k == Keys.Return)  Begin();
            if (scene == Scene.GameOver && k == Keys.Escape)  scene = Scene.Menu;
            if (scene == Scene.Playing  && k == Keys.Escape)  scene = Scene.Paused;
            if (scene == Scene.Paused   && k == Keys.Escape)  scene = Scene.Playing;
            if (scene == Scene.Paused   && k == Keys.R)       Begin();
            if (scene == Scene.Paused   && k == Keys.M)       scene = Scene.Menu;
        }

        // ── Start / restart ───────────────────────────────────
        void Begin()
        {
            player   = new Player(W / 2f, H - 110f);
            enemies.Clear(); bullets.Clear(); pickups.Clear(); particles.Clear();
            score = wave = 0;
            StartWave(1);
            scene = Scene.Playing;
        }

        void StartWave(int w)
        {
            wave          = w;
            waveKilled    = 0;
            waveTotal     = 5 + (w - 1) * 3;
            spawnInterval = MathF.Max(0.45f, 2.5f - (w - 1) * 0.2f);
            spawnCooldown = 1.2f;
        }

        // ── Game loop ─────────────────────────────────────────
        void Loop(object? s, EventArgs e)
        {
            var now = DateTime.Now;
            float dt = MathF.Min((float)(now - lastTick).TotalSeconds, 0.05f);
            lastTick = now;

            if (scene == Scene.Playing) Update(dt);
            foreach (var st in stars) st.Scroll(dt, H, W, rng);
            Invalidate();
        }

        // ── Update ────────────────────────────────────────────
        void Update(float dt)
        {
            // ── Player movement ───────────────────────────────
            float dx = (Key(Keys.Right) || Key(Keys.D) ? 1 : 0)
                     - (Key(Keys.Left)  || Key(Keys.A) ? 1 : 0);
            float dy = (Key(Keys.Down)  || Key(Keys.S) ? 1 : 0)
                     - (Key(Keys.Up)    || Key(Keys.W) ? 1 : 0);
            player.Move(dx, dy, dt, W, H);

            // ── Player fire ───────────────────────────────────
            player.FireTimer -= dt;
            if ((Key(Keys.Space) || Key(Keys.Z)) && player.FireTimer <= 0)
            {
                player.FireTimer = player.FireRate;
                Fire(player);
            }

            // ── Bullet update ─────────────────────────────────
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bullets[i].Update(dt);
                if (bullets[i].OffScreen(W, H)) bullets.RemoveAt(i);
            }

            // ── Enemy update + collisions ─────────────────────
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var en = enemies[i];
                en.Update(dt, W);

                // fell off bottom
                if (en.Y > H + 60) { enemies.RemoveAt(i); continue; }

                // enemy touches player
                if (!player.Inv && Hits(en.Rect, player.Rect))
                {
                    Explode(en.X, en.Y, Color.OrangeRed, 22);
                    enemies.RemoveAt(i);
                    DamagePlayer(25);
                    continue;
                }

                // enemy shoot
                if (en.CanShoot)
                {
                    en.FireTimer -= dt;
                    if (en.FireTimer <= 0)
                    {
                        en.FireTimer = en.FireRate;
                        ShootAt(en.X, en.Y + 16, player.X, player.Y, false, 8);
                    }
                }

                // player bullet hits enemy
                bool dead = false;
                for (int j = bullets.Count - 1; j >= 0; j--)
                {
                    if (!bullets[j].FromPlayer) continue;
                    if (!Hits(bullets[j].Rect, en.Rect)) continue;
                    en.HP -= bullets[j].Dmg;
                    bullets.RemoveAt(j);
                    if (en.HP <= 0)
                    {
                        score += en.Value;
                        Explode(en.X, en.Y, Color.OrangeRed, 26);
                        TryDrop(en.X, en.Y);
                        enemies.RemoveAt(i);
                        waveKilled++;
                        dead = true;
                    }
                    break;
                }
                if (dead) continue;
            }

            // ── Enemy bullet hits player ───────────────────────
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                if (bullets[i].FromPlayer || player.Inv) continue;
                if (!Hits(bullets[i].Rect, player.Rect)) continue;
                bullets.RemoveAt(i);
                DamagePlayer(10);
            }

            // ── Pickup update ─────────────────────────────────
            for (int i = pickups.Count - 1; i >= 0; i--)
            {
                pickups[i].Update(dt);
                if (pickups[i].Y > H + 30) { pickups.RemoveAt(i); continue; }
                if (Hits(pickups[i].Rect, player.Rect))
                {
                    Collect(pickups[i].Kind);
                    pickups.RemoveAt(i);
                }
            }

            // ── Particles ─────────────────────────────────────
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                particles[i].Update(dt);
                if (particles[i].Life <= 0) particles.RemoveAt(i);
            }

            // ── Invincibility countdown ───────────────────────
            if (player.Inv)
            {
                player.InvTimer -= dt;
                if (player.InvTimer <= 0) player.Inv = false;
            }

            // ── Power-up timer ────────────────────────────────
            if (player.PuActive)
            {
                player.PuTimer -= dt;
                if (player.PuTimer <= 0) player.ResetWeapon();
            }

            // ── Wave clear message ────────────────────────────
            if (showClear)
            {
                clearTimer -= dt;
                if (clearTimer <= 0) { showClear = false; StartWave(wave + 1); }
                return;
            }

            // ── Spawn enemies ─────────────────────────────────
            int stillNeeded = waveTotal - waveKilled - enemies.Count;
            if (stillNeeded > 0)
            {
                spawnCooldown -= dt;
                if (spawnCooldown <= 0)
                {
                    spawnCooldown = spawnInterval;
                    SpawnEnemy();
                }
            }
            else if (enemies.Count == 0 && !showClear)
            {
                showClear  = true;
                clearTimer = 2.5f;
                // Small heal between waves
                player.HP = Math.Min(player.MaxHP, player.HP + 15);
            }
        }

        bool Key(Keys k) => (int)k < 256 && keys[(int)k];

        // ── Player firing ─────────────────────────────────────
        void Fire(Player p)
        {
            switch (p.Mode)
            {
                case FireMode.Single:
                    AddBullet(p.X, p.Y - 22, 0, -620, true, p.Dmg);
                    break;
                case FireMode.Double:
                    AddBullet(p.X - 13, p.Y - 18, 0, -620, true, p.Dmg);
                    AddBullet(p.X + 13, p.Y - 18, 0, -620, true, p.Dmg);
                    break;
                case FireMode.Triple:
                    AddBullet(p.X,       p.Y - 22,  0,   -640, true, p.Dmg);
                    AddBullet(p.X - 11,  p.Y - 16, -160, -610, true, p.Dmg);
                    AddBullet(p.X + 11,  p.Y - 16,  160, -610, true, p.Dmg);
                    break;
            }
        }

        void ShootAt(float x, float y, float tx, float ty, bool fromPlayer, int dmg)
        {
            float dx = tx - x, dy = ty - y;
            float len = MathF.Sqrt(dx * dx + dy * dy);
            float spd = 270;
            AddBullet(x, y, dx / len * spd, dy / len * spd, fromPlayer, dmg);
        }

        void AddBullet(float x, float y, float vx, float vy, bool fp, int dmg)
            => bullets.Add(new Bullet(x, y, vx, vy, fp, dmg));

        // ── Damage & death ────────────────────────────────────
        void DamagePlayer(int dmg)
        {
            if (player.Shield) { player.Shield = false; Explode(player.X, player.Y, Color.Cyan, 18); return; }
            player.HP -= dmg;
            player.Inv      = true;
            player.InvTimer = 1.5f;
            Explode(player.X, player.Y, Color.Yellow, 18);
            if (player.HP > 0) return;
            player.HP = 0;
            Explode(player.X, player.Y, Color.White, 45);
            if (score > highScore) highScore = score;
            scene = Scene.GameOver;
        }

        // ── Power-up collection ───────────────────────────────
        void Collect(PickupKind kind)
        {
            switch (kind)
            {
                case PickupKind.DoubleShot:
                    player.Mode = FireMode.Double; player.PuActive = true; player.PuTimer = 8f;
                    break;
                case PickupKind.TripleShot:
                    player.Mode = FireMode.Triple; player.FireRate = 0.17f;
                    player.PuActive = true; player.PuTimer = 6f;
                    break;
                case PickupKind.Shield:
                    player.Shield = true;
                    break;
                case PickupKind.Heal:
                    player.HP = Math.Min(player.MaxHP, player.HP + 35);
                    break;
                case PickupKind.Bomb:
                    foreach (var en in enemies) { score += en.Value; Explode(en.X, en.Y, Color.OrangeRed, 22); }
                    waveKilled += enemies.Count;
                    enemies.Clear();
                    break;
            }
        }

        // ── Spawn helpers ─────────────────────────────────────
        void SpawnEnemy()
        {
            float x = rng.Next(40, W - 40);
            EnemyKind kind = EnemyKind.Scout;
            if (wave >= 3 && rng.NextDouble() < 0.30) kind = EnemyKind.Fighter;
            if (wave >= 5 && rng.NextDouble() < 0.18) kind = EnemyKind.Tank;
            enemies.Add(new Enemy(x, -36f, kind, wave, rng));
        }

        void TryDrop(float x, float y)
        {
            if (rng.NextDouble() > 0.22) return;
            var all = (PickupKind[])Enum.GetValues(typeof(PickupKind));
            pickups.Add(new Pickup(x, y, all[rng.Next(all.Length)]));
        }

        void Explode(float x, float y, Color c, int n)
        {
            for (int i = 0; i < n; i++)
            {
                float a   = (float)(rng.NextDouble() * Math.PI * 2);
                float spd = rng.Next(35, 190);
                particles.Add(new Particle(x, y,
                    MathF.Cos(a) * spd, MathF.Sin(a) * spd,
                    rng.Next(28, 75) / 100f, c, rng.Next(2, 7)));
            }
        }

        void MakeStars()
        {
            for (int i = 0; i < 160; i++)
                stars.Add(new Star(rng.Next(0, W), rng.Next(0, H),
                    rng.Next(40, 190) / 100f, rng.Next(1, 4), rng.Next(90, 255)));
        }

        static bool Hits(RectangleF a, RectangleF b) => a.IntersectsWith(b);

        // ══════════════════════════════════════════════════════
        //  DRAWING
        // ══════════════════════════════════════════════════════
        void Draw(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.FromArgb(4, 4, 18));

            DrawStars(g);

            switch (scene)
            {
                case Scene.Menu:    DrawMenu(g);                   break;
                case Scene.Paused:  DrawPlay(g); DrawPause(g);     break;
                case Scene.GameOver:DrawPlay(g); DrawGameOver(g);  break;
                default:            DrawPlay(g);                   break;
            }
        }

        // ── Stars ─────────────────────────────────────────────
        void DrawStars(Graphics g)
        {
            foreach (var s in stars)
            {
                int a = (int)(s.Br * (0.65f + 0.35f * MathF.Sin(s.Y * 0.08f)));
                using var b = new SolidBrush(Color.FromArgb(Math.Clamp(a,0,255), 190, 195, 255));
                g.FillEllipse(b, s.X - s.Sz / 2f, s.Y - s.Sz / 2f, s.Sz, s.Sz);
            }
        }

        // ── Gameplay layer ────────────────────────────────────
        void DrawPlay(Graphics g)
        {
            foreach (var p in particles) DrawParticle(g, p);
            foreach (var pu in pickups)  DrawPickup(g, pu);
            foreach (var en in enemies)  DrawEnemy(g, en);
            foreach (var b  in bullets)  DrawBullet(g, b);

            // Player (blink when invincible)
            bool blink = player.Inv && (int)(player.InvTimer * 12) % 2 == 0;
            if (!blink) DrawPlayer(g);

            DrawHUD(g);

            if (showClear) DrawCentered(g, "✦ WAVE CLEAR ✦",
                new Font("Arial", 38, FontStyle.Bold),
                Color.FromArgb((int)(Math.Min(1f, clearTimer / 0.4f) * 255), Color.Lime),
                H / 2 - 30);
        }

        // ── Player ship ───────────────────────────────────────
        void DrawPlayer(Graphics g)
        {
            float x = player.X, y = player.Y;

            // Engine glow
            using var eng = new SolidBrush(Color.FromArgb(160, 80, 210, 255));
            g.FillEllipse(eng, x - 7, y + 16, 14, 12);

            // Hull
            PointF[] hull =
            {
                new(x,      y - 26),
                new(x + 18, y + 18),
                new(x + 7,  y + 10),
                new(x,      y + 16),
                new(x - 7,  y + 10),
                new(x - 18, y + 18),
            };
            using var hullBr  = new SolidBrush(Color.FromArgb(75, 135, 255));
            using var hullPen = new Pen(Color.FromArgb(160, 200, 255), 1.8f);
            g.FillPolygon(hullBr, hull);
            g.DrawPolygon(hullPen, hull);

            // Cockpit
            using var cock = new SolidBrush(Color.FromArgb(120, 180, 255, 220));
            g.FillEllipse(cock, x - 5, y - 14, 10, 10);

            // Shield ring
            if (player.Shield)
            {
                using var sp = new Pen(Color.FromArgb(140, 0, 230, 255), 3f);
                g.DrawEllipse(sp, x - 30, y - 30, 60, 60);
            }
        }

        // ── Enemies ───────────────────────────────────────────
        void DrawEnemy(Graphics g, Enemy en)
        {
            float x = en.X, y = en.Y;
            (Color body, int sz) = en.Kind switch
            {
                EnemyKind.Fighter => (Color.FromArgb(215, 70, 70),  23),
                EnemyKind.Tank    => (Color.FromArgb(170, 55, 215), 30),
                _                 => (Color.FromArgb(255, 110, 45), 19),
            };

            PointF[] pts =
            {
                new(x,        y + sz),
                new(x - sz,   y - sz / 2f),
                new(x - sz/3f,y - sz / 3f),
                new(x,        y - sz / 2f),
                new(x + sz/3f,y - sz / 3f),
                new(x + sz,   y - sz / 2f),
            };

            using var br  = new SolidBrush(body);
            using var pen = new Pen(Color.FromArgb(230, body.R, body.G, body.B), 1.5f);
            g.FillPolygon(br, pts);
            g.DrawPolygon(pen, pts);

            // HP bar (only when damaged)
            if (en.HP < en.MaxHP)
            {
                float pct = (float)en.HP / en.MaxHP;
                g.FillRectangle(Brushes.DarkRed, x - 18, y - sz - 9, 36, 5);
                using var hb = new SolidBrush(pct > 0.5f ? Color.Lime : pct > 0.25f ? Color.Yellow : Color.Red);
                g.FillRectangle(hb, x - 18, y - sz - 9, 36 * pct, 5);
            }
        }

        // ── Bullets ───────────────────────────────────────────
        void DrawBullet(Graphics g, Bullet b)
        {
            if (b.FromPlayer)
            {
                using var glow = new SolidBrush(Color.FromArgb(55, 0, 220, 255));
                g.FillEllipse(glow, b.X - 7, b.Y - 14, 14, 28);
                using var core = new SolidBrush(Color.FromArgb(210, 0, 255, 255));
                g.FillEllipse(core, b.X - 3, b.Y - 10, 6, 20);
            }
            else
            {
                using var br = new SolidBrush(Color.FromArgb(210, 255, 55, 55));
                g.FillEllipse(br, b.X - 5, b.Y - 5, 10, 10);
            }
        }

        // ── Pickups ───────────────────────────────────────────
        void DrawPickup(Graphics g, Pickup pu)
        {
            Color c = pu.Kind switch
            {
                PickupKind.Shield     => Color.Cyan,
                PickupKind.Heal       => Color.Lime,
                PickupKind.DoubleShot => Color.Yellow,
                PickupKind.TripleShot => Color.Orange,
                PickupKind.Bomb       => Color.OrangeRed,
                _                    => Color.White,
            };
            string lbl = pu.Kind switch
            {
                PickupKind.Shield     => "SH",
                PickupKind.Heal       => "HP",
                PickupKind.DoubleShot => "2X",
                PickupKind.TripleShot => "3X",
                PickupKind.Bomb       => "☆",
                _                    => "?",
            };

            float bob = MathF.Sin(pu.BobT * 3f) * 4f;
            float py  = pu.Y + bob;

            PointF[] diamond =
            {
                new(pu.X,      py - 13),
                new(pu.X + 11, py),
                new(pu.X,      py + 13),
                new(pu.X - 11, py),
            };
            using var br  = new SolidBrush(Color.FromArgb(175, c));
            using var pen = new Pen(c, 2f);
            g.FillPolygon(br, diamond);
            g.DrawPolygon(pen, diamond);

            using var f = new Font("Consolas", 8, FontStyle.Bold);
            var sz = g.MeasureString(lbl, f);
            g.DrawString(lbl, f, Brushes.White, pu.X - sz.Width / 2, py - sz.Height / 2);
        }

        // ── Particles ─────────────────────────────────────────
        void DrawParticle(Graphics g, Particle p)
        {
            float a = p.Life / p.MaxLife;
            using var br = new SolidBrush(Color.FromArgb((int)(a * 255), p.Col));
            g.FillEllipse(br, p.X - p.Sz / 2f, p.Y - p.Sz / 2f, p.Sz, p.Sz);
        }

        // ── HUD ───────────────────────────────────────────────
        void DrawHUD(Graphics g)
        {
            // Score (top-left)
            using var sf = new Font("Consolas", 18, FontStyle.Bold);
            g.DrawString($"SCORE  {score:D6}", sf, Brushes.White, 10, 10);

            // Wave (top-center)
            using var wf = new Font("Consolas", 14, FontStyle.Bold);
            Centered(g, $"— WAVE {wave} —", wf, Brushes.Yellow, 10);

            // High score (top-right)
            using var hif = new Font("Consolas", 12);
            string hi = $"BEST {highScore:D6}";
            var hisz = g.MeasureString(hi, hif);
            g.DrawString(hi, hif, Brushes.DimGray, W - hisz.Width - 10, 14);

            // Health bar (bottom-left)
            const int BW = 200, BH = 18, BX = 12, BY = H - 38;
            g.FillRectangle(Brushes.DarkRed, BX, BY, BW, BH);
            float hpPct = (float)player.HP / player.MaxHP;
            Color hpCol = hpPct > 0.5f ? Color.Lime : hpPct > 0.25f ? Color.Yellow : Color.Red;
            using var hpBr = new SolidBrush(hpCol);
            g.FillRectangle(hpBr, BX, BY, BW * hpPct, BH);
            using var barPen = new Pen(Color.Gray, 1f);
            g.DrawRectangle(barPen, BX, BY, BW, BH);
            using var hpTxt = new Font("Consolas", 10, FontStyle.Bold);
            g.DrawString($"HP  {player.HP} / {player.MaxHP}", hpTxt, Brushes.White, BX + 4, BY + 1);

            // Shield badge
            if (player.Shield)
            {
                using var shf = new Font("Consolas", 12, FontStyle.Bold);
                g.DrawString("[ SHIELD ]", shf, Brushes.Cyan, BX + BW + 14, BY);
            }

            // Active power-up timer bar (bottom-center)
            if (player.PuActive)
            {
                string pu = player.Mode == FireMode.Double ? ">> DOUBLE SHOT <<" : ">>> TRIPLE SHOT <<<";
                using var puf = new Font("Consolas", 12, FontStyle.Bold);
                Centered(g, pu, puf, Brushes.Orange, H - 38);
                float frac = player.PuTimer / 8f;
                const int TW = 200, TX = (W - TW) / 2, TY = H - 16;
                using var tBg = new SolidBrush(Color.FromArgb(70, Color.Orange));
                g.FillRectangle(tBg, TX, TY, TW, 8);
                g.FillRectangle(Brushes.Orange, TX, TY, TW * frac, 8);
            }

            // Enemies remaining (bottom-right)
            int rem = Math.Max(0, waveTotal - waveKilled);
            using var ef = new Font("Consolas", 11);
            string estr = $"ENEMIES  {rem}";
            var esz = g.MeasureString(estr, ef);
            g.DrawString(estr, ef, Brushes.LightSlateGray, W - esz.Width - 12, H - 38);
        }

        // ── Menu ──────────────────────────────────────────────
        void DrawMenu(Graphics g)
        {
            // Animated title
            double t = DateTime.Now.TimeOfDay.TotalSeconds;
            float pulse = 0.96f + 0.04f * MathF.Sin((float)t * 2.2f);

            using var tf = new Font("Arial", 54, FontStyle.Bold);
            const string title = "STAR VOID";
            var tsz = g.MeasureString(title, tf);
            float tx = (W - tsz.Width * pulse) / 2;
            float ty = H / 2f - 170;

            using var grad = new LinearGradientBrush(
                new PointF(tx, ty), new PointF(tx, ty + tsz.Height),
                Color.FromArgb(90, 170, 255), Color.FromArgb(0, 90, 255));
            g.DrawString(title, tf, grad, tx, ty);

            // Subtitle
            using var subf = new Font("Arial", 15);
            DrawCentered(g, "Space Shooter — defend the galaxy", subf, Color.DimGray, H / 2 - 80);

            // Blink prompt
            float blink = 0.5f + 0.5f * MathF.Sin((float)t * 3.2f);
            DrawCentered(g, "PRESS  ENTER  TO  START", new Font("Consolas", 22, FontStyle.Bold),
                Color.FromArgb((int)(blink * 255), Color.Lime), H / 2 + 10);

            // Controls
            using var cf = new Font("Consolas", 13);
            DrawCentered(g, "Move — WASD / Arrow Keys",    cf, Color.LightGray, H / 2 + 80);
            DrawCentered(g, "Fire  — SPACE or Z",          cf, Color.LightGray, H / 2 + 106);
            DrawCentered(g, "Pause — ESC",                 cf, Color.LightGray, H / 2 + 132);

            // High score
            if (highScore > 0)
            {
                using var hif = new Font("Consolas", 16, FontStyle.Bold);
                DrawCentered(g, $"HIGH SCORE  {highScore:D6}", hif, Color.Gold, H / 2 + 195);
            }

            // Power-up legend
            DrawCentered(g, "SH = Shield   HP = Heal   2X = Double   3X = Triple   ☆ = Bomb",
                new Font("Consolas", 10), Color.DimGray, H - 30);
        }

        // ── Pause overlay ─────────────────────────────────────
        void DrawPause(Graphics g)
        {
            using var ov = new SolidBrush(Color.FromArgb(165, Color.Black));
            g.FillRectangle(ov, 0, 0, W, H);

            using var pf = new Font("Arial", 50, FontStyle.Bold);
            DrawCentered(g, "PAUSED", pf, Color.White, H / 2 - 80);

            using var sf = new Font("Consolas", 18);
            DrawCentered(g, "ESC — Resume",    sf, Color.LightGray, H / 2 + 20);
            DrawCentered(g, "R   — Restart",   sf, Color.LightGray, H / 2 + 52);
            DrawCentered(g, "M   — Main Menu", sf, Color.LightGray, H / 2 + 84);
        }

        // ── Game over overlay ─────────────────────────────────
        void DrawGameOver(Graphics g)
        {
            using var ov = new SolidBrush(Color.FromArgb(175, Color.Black));
            g.FillRectangle(ov, 0, 0, W, H);

            using var gof = new Font("Arial", 54, FontStyle.Bold);
            DrawCentered(g, "GAME  OVER", gof, Color.Crimson, H / 2 - 140);

            using var sf = new Font("Consolas", 24, FontStyle.Bold);
            DrawCentered(g, $"SCORE   {score:D6}", sf, Color.White,  H / 2 - 30);
            DrawCentered(g, $"WAVE    {wave}",     sf, Color.Yellow, H / 2 + 20);

            if (score > 0 && score >= highScore)
            {
                double t = DateTime.Now.TimeOfDay.TotalSeconds;
                float blink = 0.5f + 0.5f * MathF.Sin((float)t * 4f);
                DrawCentered(g, "★  NEW HIGH SCORE  ★",
                    new Font("Arial", 21, FontStyle.Bold),
                    Color.FromArgb((int)(blink * 255), Color.Gold), H / 2 + 80);
            }

            using var cf = new Font("Consolas", 16);
            DrawCentered(g, "ENTER — Play Again     ESC — Menu", cf, Color.LightGray, H / 2 + 145);
        }

        // ── Drawing helpers ───────────────────────────────────
        void DrawCentered(Graphics g, string text, Font f, Color c, float y)
        {
            var sz = g.MeasureString(text, f);
            using var br = new SolidBrush(c);
            g.DrawString(text, f, br, (W - sz.Width) / 2, y);
        }

        void DrawCentered(Graphics g, string text, Font f, Brush br, float y)
        {
            var sz = g.MeasureString(text, f);
            g.DrawString(text, f, br, (W - sz.Width) / 2, y);
        }

        void Centered(Graphics g, string text, Font f, Brush br, float y)
            => DrawCentered(g, text, f, br, y);
    }

    // ══════════════════════════════════════════════════════════
    //  ENTITIES
    // ══════════════════════════════════════════════════════════

    class Player
    {
        public float X, Y;
        public int   HP = 100, MaxHP = 100;
        public float Speed = 315f;
        public float FireRate = 0.25f, FireTimer;
        public int   Dmg = 10;
        public FireMode Mode = FireMode.Single;
        public bool  Shield, Inv, PuActive;
        public float InvTimer, PuTimer;

        public Player(float x, float y) { X = x; Y = y; }

        public RectangleF Rect => new(X - 15, Y - 22, 30, 40);

        public void Move(float dx, float dy, float dt, int w, int h)
        {
            float len = MathF.Sqrt(dx * dx + dy * dy);
            if (len > 0) { dx /= len; dy /= len; }
            X = Math.Clamp(X + dx * Speed * dt, 20, w - 20);
            Y = Math.Clamp(Y + dy * Speed * dt, 50, h - 42);
        }

        public void ResetWeapon()
        {
            PuActive = false;
            Mode     = FireMode.Single;
            FireRate = 0.25f;
            Dmg      = 10;
        }
    }

    class Enemy
    {
        public float     X, Y;
        public EnemyKind Kind;
        public int       HP, MaxHP, Value;
        public float     Speed;
        public bool      CanShoot;
        public float     FireRate, FireTimer;
        readonly float   sineOff, amplitude;
        float            t;

        public Enemy(float x, float y, EnemyKind kind, int wave, Random rng)
        {
            X = x; Y = y; Kind = kind;
            sineOff   = (float)(rng.NextDouble() * Math.PI * 2);
            FireTimer = (float)(rng.NextDouble() * 2.2f);

            switch (kind)
            {
                case EnemyKind.Scout:
                    HP = MaxHP = 20 + wave * 5; Speed = 75 + wave * 5;
                    Value = 100; FireRate = 2.8f; CanShoot = wave >= 2;
                    amplitude = 28;
                    break;
                case EnemyKind.Fighter:
                    HP = MaxHP = 45 + wave * 8; Speed = 58 + wave * 3;
                    Value = 250; FireRate = 1.9f; CanShoot = true;
                    amplitude = 55;
                    break;
                case EnemyKind.Tank:
                    HP = MaxHP = 110 + wave * 15; Speed = 38 + wave * 2;
                    Value = 500; FireRate = 1.3f; CanShoot = true;
                    amplitude = 15;
                    break;
            }
        }

        public RectangleF Rect
        {
            get
            {
                int sz = Kind switch { EnemyKind.Tank => 30, EnemyKind.Fighter => 23, _ => 19 };
                return new(X - sz, Y - sz, sz * 2, sz * 2);
            }
        }

        public void Update(float dt, int w)
        {
            t += dt;
            Y += Speed * dt;
            X += MathF.Sin(t + sineOff) * amplitude * dt;
            X = Math.Clamp(X, 22, w - 22);
        }
    }

    class Bullet
    {
        public float X, Y;
        readonly float vx, vy;
        public bool FromPlayer;
        public int  Dmg;

        public Bullet(float x, float y, float vx, float vy, bool fp, int dmg)
        { X = x; Y = y; this.vx = vx; this.vy = vy; FromPlayer = fp; Dmg = dmg; }

        public RectangleF Rect => new(X - 5, Y - 8, 10, 16);

        public void Update(float dt) { X += vx * dt; Y += vy * dt; }

        public bool OffScreen(int w, int h)
            => Y < -25 || Y > h + 25 || X < -25 || X > w + 25;
    }

    class Pickup
    {
        public float      X, Y;
        public PickupKind Kind;
        public float      BobT;
        const  float      Speed = 78f;

        public Pickup(float x, float y, PickupKind kind) { X = x; Y = y; Kind = kind; }

        public RectangleF Rect => new(X - 13, Y - 13, 26, 26);

        public void Update(float dt) { Y += Speed * dt; BobT += dt; }
    }

    class Particle
    {
        public float X, Y, VX, VY, Life, MaxLife, Sz;
        public Color Col;

        public Particle(float x, float y, float vx, float vy, float life, Color c, float sz)
        { X = x; Y = y; VX = vx; VY = vy; Life = MaxLife = life; Col = c; Sz = sz; }

        public void Update(float dt)
        {
            X += VX * dt; Y += VY * dt;
            VX *= 0.95f;  VY *= 0.95f;
            Life -= dt;
        }
    }

    class Star
    {
        public float X, Y, Speed, Sz, Br;

        public Star(float x, float y, float spd, float sz, float br)
        { X = x; Y = y; Speed = spd; Sz = sz; Br = br; }

        public void Scroll(float dt, int h, int w, Random rng)
        {
            Y += Speed * 55 * dt;
            if (Y <= h) return;
            Y = 0; X = rng.Next(0, w);
        }
    }
}
