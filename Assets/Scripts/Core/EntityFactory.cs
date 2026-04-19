using SpaceShooter.Environment;
using SpaceShooter.Gameplay;
using UnityEngine;

namespace SpaceShooter.Core
{
    public static class EntityFactory
    {
        public static PlayerController CreatePlayer(int maxHealth)
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, -3.8f, 0f);

            SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.CreateTriangle(new Color(0.3f, 0.95f, 1f));
            renderer.sortingOrder = 10;

            CircleCollider2D collider = player.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.28f;

            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;

            PlayerController controller = player.AddComponent<PlayerController>();
            controller.Configure(maxHealth, 8.5f, 0.2f);

            GameObject muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(player.transform, false);
            muzzle.transform.localPosition = new Vector3(0f, 0.48f, 0f);
            controller.SetMuzzle(muzzle.transform);

            GameObject shieldVisual = new GameObject("ShieldVisual");
            shieldVisual.transform.SetParent(player.transform, false);
            SpriteRenderer shieldRenderer = shieldVisual.AddComponent<SpriteRenderer>();
            shieldRenderer.sprite = SpriteFactory.CreateCircle(new Color(0.3f, 0.75f, 1f, 0.4f));
            shieldRenderer.sortingOrder = 9;
            shieldVisual.transform.localScale = Vector3.one * 1.8f;
            shieldVisual.SetActive(false);
            controller.SetShieldVisual(shieldVisual);

            return controller;
        }

        public static EnemyController CreateEnemy(EnemyController.EnemyType enemyType, Vector3 position)
        {
            GameObject enemy = new GameObject($"Enemy_{enemyType}");
            enemy.transform.position = position;

            SpriteRenderer renderer = enemy.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 9;

            CircleCollider2D collider = enemy.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.3f;

            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;

            EnemyController controller = enemy.AddComponent<EnemyController>();
            controller.ConfigureByType(enemyType);

            switch (enemyType)
            {
                case EnemyController.EnemyType.Basic:
                    renderer.sprite = SpriteFactory.CreateDiamond(new Color(1f, 0.35f, 0.35f));
                    break;
                case EnemyController.EnemyType.ZigZag:
                    renderer.sprite = SpriteFactory.CreateDiamond(new Color(1f, 0.55f, 0.1f));
                    break;
                default:
                    renderer.sprite = SpriteFactory.CreateDiamond(new Color(0.8f, 0.2f, 1f));
                    enemy.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
                    collider.radius = 0.4f;
                    break;
            }

            return controller;
        }

        public static BulletController CreateBullet(Vector3 position, Vector2 direction, BulletController.BulletOwner owner, float speed, int damage)
        {
            GameObject bullet = new GameObject(owner == BulletController.BulletOwner.Player ? "PlayerBullet" : "EnemyBullet");
            bullet.transform.position = position;

            SpriteRenderer renderer = bullet.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.CreateRect(owner == BulletController.BulletOwner.Player ? Color.cyan : new Color(1f, 0.45f, 0.15f), 6, 16);
            renderer.sortingOrder = 11;
            float z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            bullet.transform.rotation = Quaternion.Euler(0f, 0f, z);

            BoxCollider2D collider = bullet.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;

            BulletController controller = bullet.AddComponent<BulletController>();
            controller.Initialize(direction, speed, damage, owner);
            return controller;
        }

        public static PowerUpController CreatePowerUp(PowerUpController.PowerUpType type, Vector3 position)
        {
            GameObject powerUp = new GameObject($"PowerUp_{type}");
            powerUp.transform.position = position;

            SpriteRenderer renderer = powerUp.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 8;
            renderer.sprite = type switch
            {
                PowerUpController.PowerUpType.Health => SpriteFactory.CreateCircle(new Color(0.2f, 1f, 0.25f)),
                PowerUpController.PowerUpType.RapidFire => SpriteFactory.CreateCircle(new Color(1f, 0.84f, 0.2f)),
                _ => SpriteFactory.CreateCircle(new Color(0.45f, 0.8f, 1f))
            };

            CircleCollider2D collider = powerUp.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.3f;

            Rigidbody2D rb = powerUp.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;

            PowerUpController controller = powerUp.AddComponent<PowerUpController>();
            controller.SetPowerUpType(type);

            return controller;
        }

        public static ParallaxScroller CreateParallaxBackground()
        {
            GameObject root = new GameObject("ParallaxBackground");
            ParallaxScroller scroller = root.AddComponent<ParallaxScroller>();
            scroller.Initialize();
            return scroller;
        }
    }
}
