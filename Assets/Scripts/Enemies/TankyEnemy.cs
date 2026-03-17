using UnityEngine;

/// <summary>
/// Tanky enemy: slow but lots of health, shoots frequently.
/// </summary>
public class TankyEnemy : EnemyBase
{
    protected override void Start()
    {
        maxHealth = 5;
        moveSpeed = 1.5f;
        scoreValue = 500;
        fireRate = 1.5f;
        canShoot = true;
        dropChance = 0.35f;
        base.Start();
    }

    protected override void Move()
    {
        // Move down slowly, stop at 1/3 from top and strafe
        if (transform.position.y > 3f)
        {
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            // Strafe left and right
            float strafeX = Mathf.Sin(Time.time * 0.8f) * 4f * Time.deltaTime;
            transform.Translate(new Vector3(strafeX, 0, 0), Space.World);
        }
    }
}
