using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private Slider slider;

    private void Start()
    {
        if (player != null && slider != null)
        {
            slider.minValue = 0;
            slider.maxValue = player.MaxHealth;
            slider.value = player.CurrentHealth;
        }
    }

    private void Update()
    {
        if (player != null && slider != null)
        {
            slider.value = player.CurrentHealth;
        }
    }
}
