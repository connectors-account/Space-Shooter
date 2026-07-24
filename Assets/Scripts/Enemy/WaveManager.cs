// ============================================================
//  WaveManager.cs  –  Wave definitions & progression
//  Every 5th wave spawns a boss.
// ============================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveData
{
    public string waveName  = "Wave";
    public int    enemyCount = 6;
    public float  spawnInterval = 0.7f;
    public float  enemySpeed    = 2.5f;
    public EnemyController.Pattern movePattern = EnemyController.Pattern.StraightDown;
    public EnemyShooter.ShotPattern shotPattern = EnemyShooter.ShotPattern.Single;
    public bool   isBossWave = false;
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Enemy Prefabs")]
    public GameObject basicEnemyPrefab;
    public GameObject fastEnemyPrefab;
    public GameObject heavyEnemyPrefab;
    public GameObject bossEnemyPrefab;

    [Header("Spawn")]
    public float ySpawn     = 6f;
    public float xRange     = 4f;
    public float waveDelay  = 3f;   // seconds between waves

    public int  CurrentWave { get; private set; }
    public bool WaveActive  { get; private set; }

    int _enemiesAlive;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (GameManager.Instance?.State == GameState.Playing)
            StartCoroutine(WaveLoop());
    }

    public void Reset()
    {
        CurrentWave  = 0;
        WaveActive   = false;
        _enemiesAlive = 0;
    }

    public void EnemyKilled()
    {
        _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
    }

    // ── Wave generation ──────────────────────────────────────

    WaveData BuildWave(int waveIndex)
    {
        bool isBoss = (waveIndex % 5 == 0) && waveIndex > 0;

        return new WaveData
        {
            waveName      = isBoss ? $"BOSS {waveIndex / 5}" : $"Wave {waveIndex}",
            enemyCount    = isBoss ? 1 : Mathf.Min(4 + waveIndex * 2, 20),
            spawnInterval = Mathf.Max(0.25f, 0.8f - waveIndex * 0.04f),
            enemySpeed    = Mathf.Min(2f + waveIndex * 0.25f, 6f),
            movePattern   = PickMovePattern(waveIndex),
            shotPattern   = PickShotPattern(waveIndex),
            isBossWave    = isBoss
        };
    }

    EnemyController.Pattern PickMovePattern(int w)
    {
        if (w % 5 == 0 && w > 0) return EnemyController.Pattern.BossPatrol;
        return (EnemyController.Pattern)((w / 2) %
               System.Enum.GetValues(typeof(EnemyController.Pattern)).Length);
    }

    EnemyShooter.ShotPattern PickShotPattern(int w)
    {
        if (w >= 15) return EnemyShooter.ShotPattern.Circle8;
        if (w >= 10) return EnemyShooter.ShotPattern.Spread5;
        if (w >=  7) return EnemyShooter.ShotPattern.Spread3;
        if (w >=  4) return EnemyShooter.ShotPattern.Aimed;
        return EnemyShooter.ShotPattern.Single;
    }

    // ── Loop ─────────────────────────────────────────────────

    IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(1.5f);

        while (GameManager.Instance?.State == GameState.Playing)
        {
            CurrentWave++;
            WaveData wave = BuildWave(CurrentWave);

            UIManager.Instance?.ShowWaveBanner(wave.waveName);
            if (wave.isBossWave) AudioManager.Instance?.PlayBossAlarm();

            yield return new WaitForSeconds(waveDelay);

            yield return StartCoroutine(SpawnWave(wave));

            // Wait until all enemies from this wave are cleared
            yield return new WaitUntil(() => _enemiesAlive <= 0);
            yield return new WaitForSeconds(1.5f);
        }
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        WaveActive    = true;
        _enemiesAlive = wave.enemyCount;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemy(wave);
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        WaveActive = false;
    }

    void SpawnEnemy(WaveData wave)
    {
        GameObject prefab = PickPrefab(wave);
        if (prefab == null) return;

        float   x  = Random.Range(-xRange, xRange);
        Vector3 pos = new Vector3(x, ySpawn, 0f);
        var     go  = Instantiate(prefab, pos, Quaternion.identity);

        // Configure movement
        var ctrl = go.GetComponent<EnemyController>();
        if (ctrl)
        {
            ctrl.pattern = wave.movePattern;
            ctrl.speed   = wave.enemySpeed;
        }

        // Configure shooting
        var shooter = go.GetComponent<EnemyShooter>();
        if (shooter)
        {
            shooter.pattern  = wave.shotPattern;
            shooter.fireRate = Mathf.Max(0.6f, 2f - CurrentWave * 0.05f);
        }

        // Mark boss
        var eb = go.GetComponent<EnemyBase>();
        if (eb && wave.isBossWave)
        {
            eb.IsBoss = true;
            UIManager.Instance?.ShowBossHP(true, eb.maxHP);
        }
    }

    GameObject PickPrefab(WaveData wave)
    {
        if (wave.isBossWave) return bossEnemyPrefab ?? basicEnemyPrefab;
        if (CurrentWave >= 8 && heavyEnemyPrefab != null)
            return Random.value < 0.3f ? heavyEnemyPrefab : basicEnemyPrefab;
        if (CurrentWave >= 4 && fastEnemyPrefab  != null)
            return Random.value < 0.4f ? fastEnemyPrefab  : basicEnemyPrefab;
        return basicEnemyPrefab;
    }
}
