using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Generates prefabs programmatically when they don't exist
    /// </summary>
    public class PrefabGenerator : MonoBehaviour
    {
        public static PrefabGenerator Instance { get; private set; }
        
        // Cached prefabs
        private GameObject playerBulletPrefab;
        private GameObject enemyBulletPrefab;
        private GameObject basicEnemyPrefab;
        private GameObject zigZagEnemyPrefab;
        private GameObject diveEnemyPrefab;
        private GameObject bossEnemyPrefab;
        private GameObject explosionPrefab;
        private GameObject healthPowerUpPrefab;
        private GameObject shieldPowerUpPrefab;
        private GameObject rapidFirePowerUpPrefab;
        
        public GameObject PlayerBulletPrefab => playerBulletPrefab ?? CreatePlayerBullet();
        public GameObject EnemyBulletPrefab => enemyBulletPrefab ?? CreateEnemyBullet();
        public GameObject BasicEnemyPrefab => basicEnemyPrefab ?? CreateBasicEnemy();
        public GameObject ZigZagEnemyPrefab => zigZagEnemyPrefab ?? CreateZigZagEnemy();
        public GameObject DiveEnemyPrefab => diveEnemyPrefab ?? CreateDiveEnemy();
        public GameObject BossEnemyPrefab => bossEnemyPrefab ?? CreateBossEnemy();
        public GameObject ExplosionPrefab => explosionPrefab ?? CreateExplosion();
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                GenerateAllPrefabs();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void GenerateAllPrefabs()
        {
            CreatePlayerBullet();
            CreateEnemyBullet();
            CreateBasicEnemy();
            CreateZigZagEnemy();
            CreateDiveEnemy();
            CreateBossEnemy();
            CreateExplosion();
            CreatePowerUps();
        }
        
        private GameObject CreatePlayerBullet()
        {
            playerBulletPrefab = new GameObject("PlayerBullet");
            playerBulletPrefab.tag = "PlayerBullet";
            playerBulletPrefab.SetActive(false);
            
            SpriteRenderer sr = playerBulletPrefab.AddComponent<SpriteRenderer>();
            sr.sprite = CreateRectSprite(8, 16);
            sr.color = Color.cyan;
            sr.sortingOrder = 5;
            
            BoxCollider2D col = playerBulletPrefab.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.15f, 0.3f);
            
            Rigidbody2D rb = playerBulletPrefab.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            Bullet bullet = playerBulletPrefab.AddComponent<Bullet>();
            
            playerBulletPrefab.transform.SetParent(transform);
            return playerBulletPrefab;
        }
        
        private GameObject CreateEnemyBullet()
        {
            enemyBulletPrefab = new GameObject("EnemyBullet");
            enemyBulletPrefab.tag = "EnemyBullet";
            enemyBulletPrefab.SetActive(false);
            
            SpriteRenderer sr = enemyBulletPrefab.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(8);
            sr.color = Color.red;
            sr.sortingOrder = 5;
            
            CircleCollider2D col = enemyBulletPrefab.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.1f;
            
            Rigidbody2D rb = enemyBulletPrefab.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            
            Bullet bullet = enemyBulletPrefab.AddComponent<Bullet>();
            
            enemyBulletPrefab.transform.SetParent(transform);
            return enemyBulletPrefab;
        }
        
        private GameObject CreateBasicEnemy()
        {
            basicEnemyPrefab = CreateBaseEnemy("BasicEnemy", Color.red);
            basicEnemyPrefab.AddComponent<BasicEnemy>();
            return basicEnemyPrefab;
        }
        
        private GameObject CreateZigZagEnemy()
        {
            zigZagEnemyPrefab = CreateBaseEnemy("ZigZagEnemy", Color.yellow);
            zigZagEnemyPrefab.AddComponent<ZigZagEnemy>();
            return zigZagEnemyPrefab;
        }
        
        private GameObject CreateDiveEnemy()
        {
            diveEnemyPrefab = CreateBaseEnemy("DiveEnemy", Color.magenta);
            diveEnemyPrefab.AddComponent<DiveEnemy>();
            return diveEnemyPrefab;
        }
        
        private GameObject CreateBossEnemy()
        {
            bossEnemyPrefab = CreateBaseEnemy("BossEnemy", Color.white, 2f);
            bossEnemyPrefab.AddComponent<BossEnemy>();
            return bossEnemyPrefab;
        }
        
        private GameObject CreateBaseEnemy(string name, Color color, float scale = 1f)
        {
            GameObject enemy = new GameObject(name);
            enemy.tag = "Enemy";
            enemy.SetActive(false);
            
            SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
            sr.sprite = CreateEnemySprite();
            sr.color = color;
            sr.sortingOrder = 10;
            
            enemy.transform.localScale = Vector3.one * scale;
            
            BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.8f, 0.8f);
            
            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            enemy.AddComponent<AudioSource>();
            
            enemy.transform.SetParent(transform);
            return enemy;
        }
        
        private GameObject CreateExplosion()
        {
            explosionPrefab = new GameObject("Explosion");
            explosionPrefab.SetActive(false);
            
            SpriteRenderer sr = explosionPrefab.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(32);
            sr.color = Color.yellow;
            sr.sortingOrder = 100;
            
            explosionPrefab.AddComponent<SpaceShooter.Effects.ExplosionEffect>();
            
            explosionPrefab.transform.SetParent(transform);
            return explosionPrefab;
        }
        
        private void CreatePowerUps()
        {
            // Health
            healthPowerUpPrefab = CreatePowerUp("HealthPowerUp", Color.green, PowerUpType.Health);
            
            // Shield
            shieldPowerUpPrefab = CreatePowerUp("ShieldPowerUp", Color.blue, PowerUpType.Shield);
            
            // Rapid Fire
            rapidFirePowerUpPrefab = CreatePowerUp("RapidFirePowerUp", Color.yellow, PowerUpType.RapidFire);
        }
        
        private GameObject CreatePowerUp(string name, Color color, PowerUpType type)
        {
            GameObject powerUp = new GameObject(name);
            powerUp.tag = "PowerUp";
            powerUp.SetActive(false);
            
            SpriteRenderer sr = powerUp.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePowerUpSprite();
            sr.color = color;
            sr.sortingOrder = 15;
            
            CircleCollider2D col = powerUp.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;
            
            PowerUpBase pub = powerUp.AddComponent<PowerUpBase>();
            
            powerUp.transform.SetParent(transform);
            return powerUp;
        }
        
        // Sprite creation helpers
        private Sprite CreateRectSprite(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
            texture.SetPixels(colors);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f);
        }
        
        private Sprite CreateCircleSprite(int size)
        {
            Texture2D texture = new Texture2D(size, size);
            Color[] colors = new Color[size * size];
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 1;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    colors[y * size + x] = dist <= radius ? Color.white : Color.clear;
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }
        
        private Sprite CreateEnemySprite()
        {
            int size = 32;
            Texture2D texture = new Texture2D(size, size);
            Color[] colors = new Color[size * size];
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Create inverted triangle (enemy ship)
                    bool isInTriangle = y <= size - x && y <= x && y >= 4;
                    bool isInBody = x >= 10 && x <= 21 && y >= 4 && y <= 24;
                    colors[y * size + x] = (isInTriangle || isInBody) ? Color.white : Color.clear;
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }
        
        private Sprite CreatePowerUpSprite()
        {
            int size = 24;
            Texture2D texture = new Texture2D(size, size);
            Color[] colors = new Color[size * size];
            Vector2 center = new Vector2(size / 2f, size / 2f);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    // Create a diamond shape
                    int dx = Mathf.Abs(x - size / 2);
                    int dy = Mathf.Abs(y - size / 2);
                    bool isDiamond = dx + dy <= size / 2 - 2;
                    colors[y * size + x] = isDiamond ? Color.white : Color.clear;
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
