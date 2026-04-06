using UnityEngine;

/// <summary>
/// Visual indicator for the shield power-up. Shows a rotating ring around the player.
/// </summary>
public class ShieldVisual : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private GameObject shieldObj;
    private SpriteRenderer shieldRenderer;

    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnShieldBroken += OnShieldBroken;
        }

        CreateShieldVisual();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnShieldBroken -= OnShieldBroken;
    }

    private void CreateShieldVisual()
    {
        shieldObj = new GameObject("ShieldVisual");
        shieldObj.transform.parent = transform;
        shieldObj.transform.localPosition = Vector3.zero;

        shieldRenderer = shieldObj.AddComponent<SpriteRenderer>();
        Sprite ring = SpriteGenerator.CreateCircleSprite(48, new Color(0f, 1f, 1f, 0.3f));
        shieldRenderer.sprite = ring;
        shieldRenderer.sortingOrder = 5;
        shieldObj.transform.localScale = Vector3.one * 1.5f;
        shieldObj.SetActive(false);
    }

    private void Update()
    {
        if (playerHealth == null || shieldObj == null) return;

        shieldObj.SetActive(playerHealth.HasShield);

        if (playerHealth.HasShield)
        {
            shieldObj.transform.Rotate(0, 0, 60f * Time.deltaTime);

            // Pulsing effect
            float pulse = 1.3f + Mathf.Sin(Time.time * 3f) * 0.2f;
            shieldObj.transform.localScale = Vector3.one * pulse;
        }
    }

    private void OnShieldBroken()
    {
        if (shieldObj != null)
            shieldObj.SetActive(false);
    }
}
