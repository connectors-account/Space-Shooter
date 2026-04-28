using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        Health,
        Shield,
        RapidFire
    }

    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private int healthAmount = 20;
    [SerializeField] private float shieldDuration = 6f;
    [SerializeField] private float rapidFireDuration = 5f;

    [Header("Visuals")]
    [SerializeField] private Color healthColor = Color.green;
    [SerializeField] private Color shieldColor = Color.cyan;
    [SerializeField] private Color rapidFireColor = Color.yellow;

    private PowerUpType currentType;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        transform.Translate(Vector2.down * (fallSpeed * Time.deltaTime));

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 min = cam.ViewportToWorldPoint(new Vector3(0f, 0f, cam.nearClipPlane));
            if (transform.position.y < min.y - 2f)
            {
                Destroy(gameObject);
            }
        }
    }

    public void AssignRandomType()
    {
        currentType = (PowerUpType)Random.Range(0, 3);
        ApplyColorForType();
    }

    private void ApplyColorForType()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        switch (currentType)
        {
            case PowerUpType.Health:
                spriteRenderer.color = healthColor;
                break;
            case PowerUpType.Shield:
                spriteRenderer.color = shieldColor;
                break;
            case PowerUpType.RapidFire:
                spriteRenderer.color = rapidFireColor;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }

        switch (currentType)
        {
            case PowerUpType.Health:
                player.Heal(healthAmount);
                break;
            case PowerUpType.Shield:
                player.ActivateShield(shieldDuration);
                break;
            case PowerUpType.RapidFire:
                player.ActivateRapidFire(rapidFireDuration);
                break;
        }

        AudioManager.Instance?.PlayPowerUp(); // Add power-up SFX clip in AudioManager inspector
        Destroy(gameObject);
    }
}
