using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SpaceShooter
{
    public class GameForm : Form
    {
        private GameState _state = GameState.Menu;
        private Player _player;
        private readonly List<Enemy> _enemies = new List<Enemy>();
        private readonly List<Bullet> _playerBullets = new List<Bullet>();
        private readonly List<Bullet> _enemyBullets = new List<Bullet>();
        private readonly List<PowerUp> _powerUps = new List<PowerUp>();
        private readonly List<Star> _stars = new List<Star>();
        private readonly HashSet<Keys> _heldKeys = new HashSet<Keys>();

        private int _score = 0;
        private int _highScore = 0;
        private int _wave = 1;
        private float _waveTimer = 0;
        private bool _waveCleared = false;
        private const int _waveDelay = 180; // frames between waves
        private float _waveDelayTimer = 0;

        private readonly List<FloatingText> _floatingTexts = new List<FloatingText>();
        private readonly Random _rng = new Random();

        private readonly Timer _gameLoop;
        private int _frame = 0;

        private const int FieldWidth = 600;
        private const int FieldHeight = 800;

        private struct FloatingText
        {
            public string Text;
            public float X;
            public float Y;
            public float Life;
            public float MaxLife;
            public Color Color;
        }

        public GameForm()
        {
            Text = "Space Shooter";
            ClientSize = new Size(FieldWidth, FieldHeight);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.Black;
            DoubleBuffered = true;
            KeyPreview = true;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            for (int i = 0; i < 150; i++)
            {
                _stars.Add(new Star(FieldWidth, FieldHeight));
            }

            _player = new Player(FieldWidth, FieldHeight);

            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;

            _gameLoop = new Timer();
            _gameLoop.Interval = 16; // ~60 fps
            _gameLoop.Tick += (s, e) =>
            {
                UpdateGame();
                Invalidate();
            };
            _gameLoop.Start();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            _heldKeys.Add(e.KeyCode);

            if (e.KeyCode == Keys.Escape)
            {
                Close();
                return;
            }

            if (_state == GameState.Menu && e.KeyCode == Keys.Enter)
            {
                RestartGame();
                _state = GameState.Playing;
            }
            else if (_state == GameState.GameOver && e.KeyCode == Keys.R)
            {
                RestartGame();
                _state = GameState.Playing;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            _heldKeys.Remove(e.KeyCode);
        }

        private void UpdateGame()
        {
            _frame++;

            foreach (var s in _stars)
            {
                s.Update();
            }

            switch (_state)
            {
                case GameState.Menu:
                    // Stars already animated above.
                    break;
                case GameState.Playing:
                    UpdatePlaying();
                    break;
                case GameState.GameOver:
                    // Stars already animated above.
                    break;
            }
        }

        private void UpdatePlaying()
        {
            HandlePlayerInput();
            _player.Update();

            // Shooting
            bool shootHeld = _heldKeys.Contains(Keys.Space);
            if (shootHeld && _player.CanShoot())
            {
                _playerBullets.AddRange(_player.GetBullets());
                _player.ResetShootTimer();
            }

            // Player bullets
            foreach (var b in _playerBullets)
            {
                b.Update();
                if (b.IsOffScreen(FieldHeight)) b.Active = false;
            }

            // Enemy bullets
            foreach (var b in _enemyBullets)
            {
                b.Update();
                if (b.IsOffScreen(FieldHeight)) b.Active = false;
            }

            // Enemies
            foreach (var enemy in _enemies)
            {
                enemy.Update(1f);
                if (enemy.Y > FieldHeight + 40)
                {
                    enemy.Active = false;
                    continue;
                }
                if (enemy.ShouldShoot())
                {
                    SpawnEnemyBullet(enemy);
                }
            }

            // Power-ups
            foreach (var p in _powerUps)
            {
                p.Update();
                if (p.Y > FieldHeight + 30) p.Active = false;
            }

            HandleCollisions();

            // Cleanup inactive objects.
            _playerBullets.RemoveAll(b => !b.Active);
            _enemyBullets.RemoveAll(b => !b.Active);
            _enemies.RemoveAll(en => !en.Active);
            _powerUps.RemoveAll(p => !p.Active);

            // Player death check.
            if (_player.Health <= 0)
            {
                _state = GameState.GameOver;
                if (_score > _highScore) _highScore = _score;
            }

            // Wave management.
            if (_enemies.Count == 0)
            {
                if (!_waveCleared)
                {
                    _waveCleared = true;
                    int bonus = 500 * _wave;
                    _score += bonus;
                    AddFloatingText("WAVE CLEAR! +" + bonus, FieldWidth / 2f, FieldHeight / 2f, Color.Gold);
                    _waveDelayTimer = _waveDelay;
                }

                if (_waveDelayTimer > 0)
                {
                    _waveDelayTimer--;
                    if (_waveDelayTimer <= 0)
                    {
                        _wave++;
                        SpawnWave(_wave);
                        _waveCleared = false;
                    }
                }
            }

            // Floating texts.
            for (int i = 0; i < _floatingTexts.Count; i++)
            {
                var ft = _floatingTexts[i];
                ft.Life--;
                ft.Y -= 0.6f;
                _floatingTexts[i] = ft;
            }
            _floatingTexts.RemoveAll(ft => ft.Life <= 0);
        }

        private void HandlePlayerInput()
        {
            float speed = _player.CurrentSpeed;
            float x = _player.X;
            float y = _player.Y;

            if (_heldKeys.Contains(Keys.A) || _heldKeys.Contains(Keys.Left)) x -= speed;
            if (_heldKeys.Contains(Keys.D) || _heldKeys.Contains(Keys.Right)) x += speed;
            if (_heldKeys.Contains(Keys.W) || _heldKeys.Contains(Keys.Up)) y -= speed;
            if (_heldKeys.Contains(Keys.S) || _heldKeys.Contains(Keys.Down)) y += speed;

            // Clamp to form bounds (with margin for ship half-size).
            x = Math.Max(20f, Math.Min(FieldWidth - 20f, x));
            y = Math.Max(30f, Math.Min(FieldHeight - 30f, y));

            _player.X = x;
            _player.Y = y;
        }

        private void HandleCollisions()
        {
            // Player bullets vs enemies.
            foreach (var bullet in _playerBullets)
            {
                if (!bullet.Active) continue;
                foreach (var enemy in _enemies)
                {
                    if (!enemy.Active) continue;
                    if (IsColliding(bullet.BoundingBox, enemy.BoundingBox))
                    {
                        bullet.Active = false;
                        bool dead = enemy.TakeDamage(bullet.Damage);
                        if (dead)
                        {
                            _score += enemy.ScoreValue;
                            AddFloatingText("+" + enemy.ScoreValue, enemy.X, enemy.Y, Color.White);
                            if (_rng.NextDouble() < 0.15)
                            {
                                SpawnPowerUp(enemy.X, enemy.Y);
                            }
                        }
                        break;
                    }
                }
            }

            // Enemies vs player (collision).
            foreach (var enemy in _enemies)
            {
                if (!enemy.Active) continue;
                if (IsColliding(enemy.BoundingBox, _player.BoundingBox))
                {
                    _player.TakeDamage(1);
                    enemy.Active = false;
                }
            }

            // Enemy bullets vs player.
            foreach (var bullet in _enemyBullets)
            {
                if (!bullet.Active) continue;
                if (IsColliding(bullet.BoundingBox, _player.BoundingBox))
                {
                    bullet.Active = false;
                    _player.TakeDamage(bullet.Damage);
                }
            }

            // Power-ups vs player.
            foreach (var p in _powerUps)
            {
                if (!p.Active) continue;
                if (IsColliding(p.BoundingBox, _player.BoundingBox))
                {
                    p.Active = false;
                    _player.ApplyPowerUp(p.Type);
                    AddFloatingText(PowerUpName(p.Type), _player.X, _player.Y - 40f, p.GetColor());
                }
            }
        }

        private string PowerUpName(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.RapidFire: return "RAPID FIRE!";
                case PowerUpType.TripleShot: return "TRIPLE SHOT!";
                case PowerUpType.Shield: return "SHIELD!";
                case PowerUpType.SpeedBoost: return "SPEED BOOST!";
                default: return "";
            }
        }

        private void SpawnPowerUp(float x, float y)
        {
            var values = (PowerUpType[])Enum.GetValues(typeof(PowerUpType));
            var type = values[_rng.Next(values.Length)];
            _powerUps.Add(new PowerUp(x, y, type));
        }

        private void SpawnWave(int waveNumber)
        {
            int basicCount, fastCount, tankCount;

            switch (waveNumber)
            {
                case 1:
                    basicCount = 5; fastCount = 0; tankCount = 0;
                    break;
                case 2:
                    basicCount = 8; fastCount = 0; tankCount = 0;
                    break;
                case 3:
                    basicCount = 5; fastCount = 3; tankCount = 0;
                    break;
                case 4:
                    basicCount = 4; fastCount = 2; tankCount = 1;
                    break;
                default:
                    basicCount = Math.Min(waveNumber * 2, 14);
                    fastCount = Math.Min(waveNumber, 8);
                    tankCount = Math.Min(waveNumber / 2, 5);
                    break;
            }

            // Lay out enemies in a formation grid.
            int perRow = 5;
            float marginX = 70f;
            float spacingX = (FieldWidth - marginX * 2) / (perRow - 1);
            float startY = 70f;
            float spacingY = 60f;

            int index = 0;
            void Place(EnemyType type)
            {
                int row = index / perRow;
                int col = index % perRow;
                float x = marginX + col * spacingX;
                float y = startY + row * spacingY;
                // Tanks are wider; nudge into visible bounds.
                x = Math.Max(35f, Math.Min(FieldWidth - 35f, x));
                _enemies.Add(new Enemy(type, x, y - 60f)); // start slightly above screen
                index++;
            }

            for (int i = 0; i < basicCount; i++) Place(EnemyType.Basic);
            for (int i = 0; i < fastCount; i++) Place(EnemyType.Fast);
            for (int i = 0; i < tankCount; i++) Place(EnemyType.Tank);
        }

        private void SpawnEnemyBullet(Enemy e)
        {
            // Aim toward the player's current position with a small random spread.
            float dx = _player.X - e.X;
            float dy = _player.Y - e.Y;
            double baseAngle = Math.Atan2(dy, dx);
            double spread = (_rng.NextDouble() * 2 - 1) * (15.0 * Math.PI / 180.0); // +/- 15 degrees
            double angle = baseAngle + spread;

            float speed = 4.5f;
            float vx = (float)Math.Cos(angle) * speed;
            float vy = (float)Math.Sin(angle) * speed;

            var color = Color.FromArgb(255, 255, 120, 60);
            _enemyBullets.Add(new Bullet(e.X, e.Y + 10f, vx, vy, 1, false, color));
        }

        private bool IsColliding(Rectangle a, Rectangle b)
        {
            return a.IntersectsWith(b);
        }

        private void AddFloatingText(string text, float x, float y, Color color)
        {
            _floatingTexts.Add(new FloatingText
            {
                Text = text,
                X = x,
                Y = y,
                Life = 70f,
                MaxLife = 70f,
                Color = color
            });
        }

        private void RestartGame()
        {
            _enemies.Clear();
            _playerBullets.Clear();
            _enemyBullets.Clear();
            _powerUps.Clear();
            _floatingTexts.Clear();
            _score = 0;
            _wave = 1;
            _waveTimer = 0;
            _waveCleared = false;
            _waveDelayTimer = 0;
            _player = new Player(FieldWidth, FieldHeight);
            SpawnWave(1);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            // Stars layer.
            foreach (var s in _stars)
            {
                s.Draw(g);
            }

            switch (_state)
            {
                case GameState.Menu:
                    DrawMenu(g);
                    break;
                case GameState.Playing:
                    DrawPlaying(g);
                    break;
                case GameState.GameOver:
                    DrawGameOver(g);
                    break;
            }
        }

        private void DrawCenteredString(Graphics g, string text, Font font, Brush brush, float y)
        {
            SizeF sz = g.MeasureString(text, font);
            g.DrawString(text, font, brush, (FieldWidth - sz.Width) / 2f, y);
        }

        private void DrawMenu(Graphics g)
        {
            using (var titleFont = new Font("Consolas", 40f, FontStyle.Bold))
            using (var cyan = new SolidBrush(Color.Cyan))
            {
                DrawCenteredString(g, "SPACE SHOOTER", titleFont, cyan, 150f);
            }
            using (var subFont = new Font("Consolas", 14f, FontStyle.Regular))
            using (var white = new SolidBrush(Color.White))
            {
                DrawCenteredString(g, "A Classic Arcade Experience", subFont, white, 210f);
            }

            // Blinking "PRESS ENTER TO START".
            if ((_frame / 30) % 2 == 0)
            {
                using (var startFont = new Font("Consolas", 18f, FontStyle.Bold))
                using (var yellow = new SolidBrush(Color.Yellow))
                {
                    DrawCenteredString(g, "PRESS ENTER TO START", startFont, yellow, 300f);
                }
            }

            using (var ctrlFont = new Font("Consolas", 12f, FontStyle.Regular))
            using (var gray = new SolidBrush(Color.Gray))
            {
                DrawCenteredString(g, "WASD / Arrows: Move  |  SPACE: Shoot  |  ESC: Quit", ctrlFont, gray, 400f);
            }

            // Power-ups legend.
            using (var legendFont = new Font("Consolas", 10f, FontStyle.Bold))
            {
                string[] labels = { "RapidFire", "TripleShot", "Shield", "SpeedBoost" };
                Color[] colors = { Color.Yellow, Color.Cyan, Color.DodgerBlue, Color.LimeGreen };
                float y = 440f;
                // Center the group.
                float totalWidth = 0f;
                float[] widths = new float[labels.Length];
                for (int i = 0; i < labels.Length; i++)
                {
                    widths[i] = g.MeasureString(labels[i], legendFont).Width + 30f;
                    totalWidth += widths[i];
                }
                float x = (FieldWidth - totalWidth) / 2f;
                for (int i = 0; i < labels.Length; i++)
                {
                    using (var b = new SolidBrush(colors[i]))
                    {
                        g.FillRectangle(b, x, y + 2f, 12f, 12f);
                        g.DrawString(labels[i], legendFont, b, x + 16f, y);
                    }
                    x += widths[i];
                }
            }

            // Decorative preview ships.
            var demoEnemy1 = new Enemy(EnemyType.Basic, FieldWidth / 2f - 120f, 560f);
            var demoEnemy2 = new Enemy(EnemyType.Fast, FieldWidth / 2f, 560f);
            var demoEnemy3 = new Enemy(EnemyType.Tank, FieldWidth / 2f + 120f, 560f);
            demoEnemy1.Draw(g);
            demoEnemy2.Draw(g);
            demoEnemy3.Draw(g);

            var demoPlayer = new Player(FieldWidth, 700 + 90);
            demoPlayer.Draw(g, 0);
        }

        private void DrawPlaying(Graphics g)
        {
            foreach (var p in _powerUps) p.Draw(g);
            foreach (var enemy in _enemies) enemy.Draw(g);
            foreach (var b in _playerBullets) b.Draw(g);
            foreach (var b in _enemyBullets) b.Draw(g);
            _player.Draw(g, _frame);

            // Floating texts.
            using (var ftFont = new Font("Consolas", 12f, FontStyle.Bold))
            {
                foreach (var ft in _floatingTexts)
                {
                    int alpha = (int)Math.Max(0, Math.Min(255, 255 * (ft.Life / ft.MaxLife)));
                    using (var b = new SolidBrush(Color.FromArgb(alpha, ft.Color)))
                    {
                        SizeF sz = g.MeasureString(ft.Text, ftFont);
                        g.DrawString(ft.Text, ftFont, b, ft.X - sz.Width / 2f, ft.Y);
                    }
                }
            }

            DrawHUD(g);

            // Wave announcement.
            if (_waveDelayTimer > 0)
            {
                float ratio = _waveDelayTimer / _waveDelay;
                int alpha = (int)Math.Max(0, Math.Min(255, 255 * ratio));
                using (var waveFont = new Font("Consolas", 36f, FontStyle.Bold))
                using (var b = new SolidBrush(Color.FromArgb(alpha, Color.Cyan)))
                {
                    string txt = "WAVE " + (_wave + 1);
                    SizeF sz = g.MeasureString(txt, waveFont);
                    g.DrawString(txt, waveFont, b, (FieldWidth - sz.Width) / 2f, FieldHeight / 2f - 120f);
                }
            }
        }

        private void DrawHUD(Graphics g)
        {
            using (var font = new Font("Consolas", 14f, FontStyle.Bold))
            using (var white = new SolidBrush(Color.White))
            using (var yellow = new SolidBrush(Color.Yellow))
            {
                g.DrawString("SCORE: " + _score, font, white, 12f, 10f);

                string waveText = "WAVE: " + _wave;
                SizeF wsz = g.MeasureString(waveText, font);
                g.DrawString(waveText, font, yellow, FieldWidth - wsz.Width - 12f, 10f);
            }

            // Health bar: one rectangle per HP point.
            float hx = 12f;
            float hy = 38f;
            float boxW = 24f;
            float boxH = 14f;
            float gap = 4f;
            for (int i = 0; i < _player.MaxHealth; i++)
            {
                bool filled = i < _player.Health;
                Color c;
                if (filled)
                {
                    float t = (float)_player.Health / _player.MaxHealth;
                    c = t > 0.5f ? Color.LimeGreen : (t > 0.25f ? Color.Orange : Color.Red);
                }
                else
                {
                    c = Color.FromArgb(80, 80, 80);
                }
                using (var b = new SolidBrush(c))
                {
                    g.FillRectangle(b, hx + i * (boxW + gap), hy, boxW, boxH);
                }
                using (var pen = new Pen(Color.FromArgb(200, 255, 255, 255), 1f))
                {
                    g.DrawRectangle(pen, hx + i * (boxW + gap), hy, boxW, boxH);
                }
            }

            // Active power-up indicators with remaining time.
            using (var smallFont = new Font("Consolas", 10f, FontStyle.Bold))
            {
                float py = 62f;
                if (_player.RapidFire)
                {
                    DrawPowerUpIndicator(g, smallFont, "RAPID", Color.Yellow, _player.RapidFireTimer, ref py);
                }
                if (_player.TripleShot)
                {
                    DrawPowerUpIndicator(g, smallFont, "TRIPLE", Color.Cyan, _player.TripleShotTimer, ref py);
                }
                if (_player.HasShield)
                {
                    DrawPowerUpIndicator(g, smallFont, "SHIELD", Color.DodgerBlue, _player.ShieldTimer, ref py);
                }
                if (_player.SpeedBoost > 0)
                {
                    DrawPowerUpIndicator(g, smallFont, "SPEED", Color.LimeGreen, _player.SpeedBoostTimer, ref py);
                }
            }
        }

        private void DrawPowerUpIndicator(Graphics g, Font font, string label, Color color, float timer, ref float py)
        {
            int seconds = (int)Math.Ceiling(timer / 60f);
            using (var b = new SolidBrush(color))
            {
                g.FillRectangle(b, 12f, py + 2f, 10f, 10f);
                g.DrawString(label + " " + seconds + "s", font, b, 26f, py);
            }
            py += 16f;
        }

        private void DrawGameOver(Graphics g)
        {
            using (var overFont = new Font("Consolas", 48f, FontStyle.Bold))
            using (var red = new SolidBrush(Color.Red))
            {
                DrawCenteredString(g, "GAME OVER", overFont, red, 200f);
            }
            using (var scoreFont = new Font("Consolas", 24f, FontStyle.Bold))
            using (var white = new SolidBrush(Color.White))
            {
                DrawCenteredString(g, "Score: " + _score, scoreFont, white, 300f);
            }
            using (var hsFont = new Font("Consolas", 20f, FontStyle.Bold))
            using (var yellow = new SolidBrush(Color.Yellow))
            {
                DrawCenteredString(g, "High Score: " + _highScore, hsFont, yellow, 345f);
            }
            using (var waveFont = new Font("Consolas", 18f, FontStyle.Regular))
            using (var white = new SolidBrush(Color.White))
            {
                DrawCenteredString(g, "Wave Reached: " + _wave, waveFont, white, 385f);
            }

            if ((_frame / 30) % 2 == 0)
            {
                using (var restartFont = new Font("Consolas", 16f, FontStyle.Bold))
                using (var cyan = new SolidBrush(Color.Cyan))
                {
                    DrawCenteredString(g, "PRESS R TO RESTART", restartFont, cyan, 450f);
                }
            }
        }
    }
}
