using UnityEngine;

public enum PowerUpType
{
    RapidFire,
    Shield,
    HealthRestore
}

[RequireComponent(typeof(Collider2D))]
public class PowerUp : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float maxLifetime = 8f;
    [SerializeField] private PowerUpType powerUpType = PowerUpType.RapidFire;
    [SerializeField] private float effectDuration = 6f;
    [SerializeField] private int healAmount = 25;

    private ObjectPool _originPool;
    private float _timer;

    private void OnEnable()
    {
        _timer = maxLifetime;
        float roll = Random.value;
        if (roll < 0.33f) powerUpType = PowerUpType.RapidFire;
        else if (roll < 0.66f) powerUpType = PowerUpType.Shield;
        else powerUpType = PowerUpType.HealthRestore;
    }

    public void Initialize(ObjectPool pool)
    {
        _originPool = pool;
        _timer = maxLifetime;
    }

    private void Update()
    {
        transform.position += Vector3.down * (fallSpeed * Time.deltaTime);
        _timer -= Time.deltaTime;

        if (_timer <= 0f || transform.position.y < -6.5f)
        {
            Release();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerController player))
        {
            return;
        }

        Apply(player);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(AudioCue.PowerUp);
        }

        Release();
    }

    private void Apply(PlayerController player)
    {
        switch (powerUpType)
        {
            case PowerUpType.RapidFire:
                player.EnableRapidFire(effectDuration);
                break;
            case PowerUpType.Shield:
                player.EnableShield(effectDuration);
                break;
            case PowerUpType.HealthRestore:
                player.Heal(healAmount);
                break;
        }
    }

    private void Release()
    {
        if (_originPool != null)
        {
            _originPool.Return(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
