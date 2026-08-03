using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace SpaceShooter
{
    /// <summary>
    /// Main game window.  Hosts the fixed-timestep game loop (via a WinForms Timer),
    /// double-buffered rendering to a back-buffer Bitmap, and all input handling.
    /// </summary>
    public class GameForm : Form
    {
        // ── dimensions ──────────────────────────────────────────────────────────
        private const int W = 520;
        private const int H = 720;

        // ── game objects ────────────────────────────────────────────────────────
        private Player!        _player;
        private List<Enemy>    _enemies  = new();
        private List<Bullet>   _bullets  = new();
        private GameManager    _gm       = new();
        private EnemySpawner   _spawner  = new();

        // ── parallax starfield ──────────────────────────────────────────────────
        private struct Star
        {
            public float X, Y, Speed, Size;
            public Color Color;
        }
        private readonly List<Star> _stars = new();

        // ── input state ─────────────────────────────────────────────────────────
        private bool _kLeft, _kRight, _kUp, _kDown, _kShoot;

        // ── timing ──────────────────────────────────────────────────────────────
        private System.Windows.Forms.Timer _timer = null!;
        private DateTime _lastTick;

        // ── rendering ───────────────────────────────────────────────────────────
        private Bitmap   _backBuffer = null!;
        private Graphics _canvas     = null!;

        // ── fonts ────────────────────────────────────────────────────────────────
        private Font _fntTitle  = null!;
        private Font _fntMed    = null!;
        private Font _fntSmall  = null!;
        private Font _fntTiny   = null!;

        // ── wave transition ──────────────────────────────────────────────────────
        private float  _waveTextTimer;   // how long to show "Wave N" banner
        private string _waveBanner = "";

        // ── explosion particles ──────────────────────────────────────────────────
        private struct Particle
        {
            public float X, Y, VX, VY, Life, MaxLife, Size;
            public Color Color;
        }
        private readonly List<Particle> _particles = new();
        private readonly Random _rng = new();

        // ────────────────────────────────────────────────────────────────────────
        public GameForm()
        {
            Text            = "Space Shooter";
            ClientSize      = new Size(W, H);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            BackColor       = Color.Black;
            DoubleBuffered  = true;
            StartPosition   = FormStartPosition.CenterScreen;

            InitFonts();
            InitBuffers();
            InitStars();

            _timer          = new System.Windows.Forms.Timer { Interval = 16 }; // ~62 fps
            _timer.Tick    += OnTick;
            _timer.Start();
            _lastTick       = DateTime.Now;

            KeyDown += OnKeyDown;
            KeyUp   += OnKeyUp;
            Paint   += OnPaint;
        }

        // ── initialisation helpers ───────────────────────────────────────────────

        private void InitFonts()
        {
            _fntTitle = new Font("Consolas", 34, FontStyle.Bold,   GraphicsUnit.Pixel);
            _fntMed   = new Font("Consolas", 18, FontStyle.Bold,   GraphicsUnit.Pixel);
            _fntSmall = new Font("Consolas", 13, FontStyle.Regular, GraphicsUnit.Pixel);
            _fntTiny  = new Font("Consolas", 11, FontStyle.Regular, GraphicsUnit.Pixel);
        }

        private void InitBuffers()
        {
            _backBuffer = new Bitmap(W, H);
            _canvas     = Graphics.FromImage(_backBuffer);
            _canvas.SmoothingMode      = SmoothingMode.AntiAlias;
            _canvas.TextRenderingHint  = TextRenderingHint.ClearTypeGridFit;
        }

        private void InitStars()
        {
            for (int i = 0; i < 130; i++)
                _stars.Add(MakeStar(_rng.Next(H)));   // pre-scatter vertically
        }

        private Star MakeStar(int startY)
        {
            float speed = _rng.Next(25, 160);
            int   alpha = 60 + (int)(speed / 160f * 195);
            return new Star
            {
                X     = _rng.Next(W),
                Y     = startY,
                Speed = speed,
                Size  = speed < 60 ? 1f : speed < 110 ? 2f : 3f,
                Color = Color.FromArgb(alpha,
                            _rng.Next(150, 256),
                            _rng.Next(150, 256),
                            _rng.Next(200, 256))
            };
        }

        // ── game loop ────────────────────────────────────────────────────────────

        private void OnTick(object? sender, EventArgs e)
        {
            var   now = DateTime.Now;
            float dt  = (float)(now - _lastTick).TotalSeconds;
            _lastTick = now;
            dt        = MathF.Min(dt, 0.05f);   // clamp to avoid spiral of death

            UpdateStars(dt);
            UpdateParticles(dt);

            if (_gm.State == GameState.Playing)
                UpdateGame(dt);

            if (_waveTextTimer > 0)
                _waveTextTimer -= dt;

            Render();
            Invalidate();
        }

        // ── updates ──────────────────────────────────────────────────────────────

        private void UpdateStars(float dt)
        {
            for (int i = 0; i < _stars.Count; i++)
            {
                var s = _stars[i];
                s.Y += s.Speed * dt;
                if (s.Y > H) { s = MakeStar(-4); }
                _stars[i] = s;
            }
        }

        private void UpdateParticles(float dt)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.X    += p.VX * dt;
                p.Y    += p.VY * dt;
                p.Life -= dt;
                if (p.Life <= 0) { _particles.RemoveAt(i); continue; }
                _particles[i] = p;
            }
        }

        private void UpdateGame(float dt)
        {
            // Player
            _player.Update(_kLeft, _kRight, _kUp, _kDown, _kShoot, dt, _bullets, W, H);

            // Enemies
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                var en = _enemies[i];
                if (!en.Active || en.Bounds.Y > H + 70) { _enemies.RemoveAt(i); continue; }
                en.Update(dt, _bullets, W);
            }

            // Bullets – move then prune out-of-screen and inactive
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                var b = _bullets[i];
                b.Update(dt);
                if (!b.Active || b.Bounds.Y < -30 || b.Bounds.Y > H + 30)
                    _bullets.RemoveAt(i);
            }

            // Wave spawner tick
            _spawner.Update(dt, _enemies, W);

            CheckCollisions();
            CheckPlayerDeath();
            CheckWaveProgress();
        }

        private void CheckCollisions()
        {
            // Player bullets → enemies
            foreach (var b in _bullets)
            {
                if (!b.Active || !b.IsPlayerBullet) continue;
                foreach (var en in _enemies)
                {
                    if (!en.Active) continue;
                    if (!b.Bounds.IntersectsWith(en.Bounds)) continue;

                    en.TakeDamage(20);
                    b.Active = false;

                    if (!en.Active)
                    {
                        _gm.AddScore(en.ScoreValue);
                        SpawnExplosion(en.Bounds);
                    }
                    break; // one bullet hits one enemy
                }
            }

            // Enemy bullets → player
            foreach (var b in _bullets)
            {
                if (!b.Active || b.IsPlayerBullet) continue;
                if (!b.Bounds.IntersectsWith(_player.Bounds)) continue;
                _player.TakeDamage(20);
                b.Active = false;
            }

            // Enemy body → player (ram)
            foreach (var en in _enemies)
            {
                if (!en.Active) continue;
                if (!en.Bounds.IntersectsWith(_player.Bounds)) continue;
                _player.TakeDamage(35);
                en.TakeDamage(9999);
                if (!en.Active) SpawnExplosion(en.Bounds);
            }
        }

        private void CheckPlayerDeath()
        {
            if (_player.Health > 0) return;

            _gm.LoseLife();
            if (_gm.IsGameOver)
            {
                _gm.State = GameState.GameOver;
            }
            else
            {
                _player.Health = _player.MaxHealth;
                SpawnExplosion(_player.Bounds);
            }
        }

        private void CheckWaveProgress()
        {
            if (_spawner.AllSpawned && _enemies.Count == 0)
            {
                _gm.NextWave();
                _spawner.StartWave(_gm.Wave);
                _waveBanner    = $"WAVE  {_gm.Wave}";
                _waveTextTimer = 2.5f;
            }
        }

        // ── explosion particles ──────────────────────────────────────────────────

        private void SpawnExplosion(RectangleF bounds)
        {
            float cx = bounds.X + bounds.Width  / 2f;
            float cy = bounds.Y + bounds.Height / 2f;
            int   n  = 18 + _rng.Next(12);

            for (int i = 0; i < n; i++)
            {
                double ang = _rng.NextDouble() * Math.PI * 2;
                float  spd = 60f + _rng.Next(180);
                Color  col = _rng.Next(3) switch
                {
                    0 => Color.FromArgb(255, 200, 60),
                    1 => Color.OrangeRed,
                    _ => Color.White
                };
                _particles.Add(new Particle
                {
                    X       = cx,
                    Y       = cy,
                    VX      = (float)Math.Cos(ang) * spd,
                    VY      = (float)Math.Sin(ang) * spd,
                    Life    = 0.35f + (float)_rng.NextDouble() * 0.4f,
                    MaxLife = 0.75f,
                    Size    = 2f + _rng.Next(4),
                    Color   = col
                });
            }
        }

        // ── start / restart ──────────────────────────────────────────────────────

        private void StartGame()
        {
            _gm.Reset();
            _gm.State  = GameState.Playing;
            _player    = new Player(W / 2f, H - 85);
            _enemies.Clear();
            _bullets.Clear();
            _particles.Clear();
            _spawner.StartWave(1);
            _waveBanner    = "WAVE  1";
            _waveTextTimer = 2.5f;
        }

        // ── rendering ────────────────────────────────────────────────────────────

        private void Render()
        {
            // ── background ──
            _canvas.Clear(Color.FromArgb(4, 4, 18));

            // stars
            foreach (var s in _stars)
            {
                using var b = new SolidBrush(s.Color);
                _canvas.FillEllipse(b, s.X, s.Y, s.Size, s.Size);
            }

            // ── state-specific content ──
            switch (_gm.State)
            {
                case GameState.Menu:    DrawMenu();    break;
                case GameState.Playing: DrawPlaying(); break;
                case GameState.GameOver: DrawGameOver(); break;
            }
        }

        // menu screen
        private void DrawMenu()
        {
            DrawGlow(_canvas, W / 2f, H / 3.5f, 160, Color.FromArgb(40, 0, 150, 255));
            CenterText("SPACE", _fntTitle, Color.FromArgb(0, 210, 255), (int)(H / 3.5f) - 38);
            CenterText("SHOOTER", _fntTitle, Color.White, (int)(H / 3.5f) + 8);

            // separator
            _canvas.DrawLine(new Pen(Color.FromArgb(80, 0, 200, 255), 1), 80, H / 2 - 10, W - 80, H / 2 - 10);

            CenterText("Press  ENTER  to  Start", _fntMed, Color.FromArgb(220, 220, 220), H / 2 + 10);
            CenterText("WASD / Arrow Keys  →  Move", _fntSmall, Color.Gray, H / 2 + 50);
            CenterText("SPACE  →  Shoot",            _fntSmall, Color.Gray, H / 2 + 74);
            CenterText("Survive all waves!",          _fntTiny,  Color.FromArgb(100,100,100), H / 2 + 108);

            if (_gm.HighScore > 0)
                CenterText($"HI-SCORE:  {_gm.HighScore}", _fntSmall,
                    Color.FromArgb(255, 200, 0), H - 60);
        }

        // gameplay screen
        private void DrawPlaying()
        {
            // particles (behind ships)
            DrawParticles();

            foreach (var en in _enemies) if (en.Active)  en.Draw(_canvas);
            foreach (var b  in _bullets) if (b.Active)   b.Draw(_canvas);
            _player.Draw(_canvas);

            DrawHUD();

            // wave banner
            if (_waveTextTimer > 0)
            {
                float alpha = MathF.Min(1f, _waveTextTimer) * 255;
                CenterText(_waveBanner, _fntMed,
                    Color.FromArgb((int)alpha, 255, 220, 0), H / 2 - 20);
            }
        }

        // game-over screen
        private void DrawGameOver()
        {
            DrawParticles();  // let last explosion finish

            DrawGlow(_canvas, W / 2f, H / 3f, 140, Color.FromArgb(35, 200, 40, 0));
            CenterText("GAME  OVER",  _fntTitle, Color.OrangeRed, (int)(H / 3f) - 28);
            CenterText($"SCORE:  {_gm.Score}",      _fntMed, Color.White,  (int)(H / 3f) + 30);
            CenterText($"WAVE REACHED:  {_gm.Wave}", _fntSmall, Color.Yellow, (int)(H / 3f) + 60);
            if (_gm.Score >= _gm.HighScore)
                CenterText("NEW  HI-SCORE!", _fntSmall, Color.FromArgb(255, 215, 0), (int)(H / 3f) + 90);

            _canvas.DrawLine(new Pen(Color.FromArgb(80, 200, 80, 0), 1),
                80, H / 2 + 10, W - 80, H / 2 + 10);

            CenterText("Press  ENTER  to  Play  Again", _fntMed,
                Color.FromArgb(200, 200, 200), H / 2 + 30);
        }

        private void DrawHUD()
        {
            // Score
            _canvas.DrawString($"SCORE  {_gm.Score}", _fntSmall,
                Brushes.Cyan, 10, 8);

            // High score
            _canvas.DrawString($"BEST  {_gm.HighScore}", _fntSmall,
                new SolidBrush(Color.FromArgb(180, 255, 215, 0)), 10, 26);

            // Wave (right-aligned)
            string waveStr = $"WAVE  {_gm.Wave}";
            var    wSz     = _canvas.MeasureString(waveStr, _fntSmall);
            _canvas.DrawString(waveStr, _fntSmall, Brushes.Yellow, W - wSz.Width - 8, 8);

            // Lives (right-aligned)
            string livesStr = $"LIVES  {_gm.Lives}";
            var    lSz      = _canvas.MeasureString(livesStr, _fntSmall);
            _canvas.DrawString(livesStr, _fntSmall, Brushes.White, W - lSz.Width - 8, 26);

            // Health bar background
            _canvas.FillRectangle(new SolidBrush(Color.FromArgb(80, 80, 80, 80)),
                10, 48, 160, 10);
            // Health bar fill
            float hpFrac = Math.Max(0f, _player.Health / (float)_player.MaxHealth);
            Color hpCol  = hpFrac > 0.5f ? Color.LimeGreen : hpFrac > 0.25f ? Color.Orange : Color.Red;
            _canvas.FillRectangle(new SolidBrush(hpCol), 10, 48, 160 * hpFrac, 10);
            // Health bar border
            _canvas.DrawRectangle(new Pen(Color.FromArgb(120, 255, 255, 255)), 10, 48, 160, 10);
            _canvas.DrawString("HP", _fntTiny, Brushes.White, 174, 46);
        }

        private void DrawParticles()
        {
            foreach (var p in _particles)
            {
                float fade  = p.Life / p.MaxLife;
                int   alpha = (int)(fade * 255);
                using var br = new SolidBrush(Color.FromArgb(alpha, p.Color));
                float half = p.Size * fade / 2f;
                _canvas.FillEllipse(br, p.X - half, p.Y - half, p.Size * fade, p.Size * fade);
            }
        }

        // ── helpers ──────────────────────────────────────────────────────────────

        private void CenterText(string text, Font font, Color color, int y)
        {
            var sz = _canvas.MeasureString(text, font);
            _canvas.DrawString(text, font, new SolidBrush(color), (W - sz.Width) / 2f, y);
        }

        private static void DrawGlow(Graphics g, float cx, float cy, float radius, Color color)
        {
            using var path = new GraphicsPath();
            path.AddEllipse(cx - radius, cy - radius, radius * 2, radius * 2);
            using var brush = new PathGradientBrush(path)
            {
                CenterColor    = color,
                SurroundColors = new[] { Color.Transparent }
            };
            g.FillPath(brush, path);
        }

        // ── paint ────────────────────────────────────────────────────────────────

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(_backBuffer, 0, 0);
        }

        // ── input ────────────────────────────────────────────────────────────────

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:  case Keys.A: _kLeft  = true;  break;
                case Keys.Right: case Keys.D: _kRight = true;  break;
                case Keys.Up:    case Keys.W: _kUp    = true;  break;
                case Keys.Down:  case Keys.S: _kDown  = true;  break;
                case Keys.Space: _kShoot = true; break;
                case Keys.Enter:
                    if (_gm.State != GameState.Playing) StartGame();
                    break;
                case Keys.Escape:
                    Close();
                    break;
            }
        }

        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:  case Keys.A: _kLeft  = false; break;
                case Keys.Right: case Keys.D: _kRight = false; break;
                case Keys.Up:    case Keys.W: _kUp    = false; break;
                case Keys.Down:  case Keys.S: _kDown  = false; break;
                case Keys.Space: _kShoot = false; break;
            }
        }

        // ── cleanup ──────────────────────────────────────────────────────────────

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer.Stop();
            _canvas.Dispose();
            _backBuffer.Dispose();
            _fntTitle.Dispose();
            _fntMed.Dispose();
            _fntSmall.Dispose();
            _fntTiny.Dispose();
            base.OnFormClosed(e);
        }
    }
}
