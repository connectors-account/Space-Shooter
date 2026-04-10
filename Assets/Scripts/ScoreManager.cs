using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private float comboTimeout = 2f;
    [SerializeField] private int comboStep = 1;
    [SerializeField] private int maxComboMultiplier = 10;

    public int Score { get; private set; }
    public int ComboMultiplier { get; private set; } = 1;

    private float _lastKillTime = -999f;

    private void Update()
    {
        if (Time.time - _lastKillTime > comboTimeout)
        {
            ComboMultiplier = 1;
        }
    }

    public void RegisterEnemyKill(int basePoints)
    {
        if (Time.time - _lastKillTime <= comboTimeout)
        {
            ComboMultiplier = Mathf.Min(maxComboMultiplier, ComboMultiplier + comboStep);
        }
        else
        {
            ComboMultiplier = 1;
        }

        _lastKillTime = Time.time;
        AddScore(basePoints * ComboMultiplier);
    }

    public void AddScore(int amount)
    {
        Score = Mathf.Max(0, Score + amount);
    }

    public void ResetScore()
    {
        Score = 0;
        ComboMultiplier = 1;
        _lastKillTime = -999f;
    }
}
