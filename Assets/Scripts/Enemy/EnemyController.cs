// ============================================================
//  EnemyController.cs  –  Movement patterns
//  Patterns: StraightDown, SineWave, ZigZag, DiveAtPlayer, BossPatrol
// ============================================================
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum Pattern { StraightDown, SineWave, ZigZag, DiveAtPlayer, BossPatrol }

    [Header("Movement")]
    public Pattern pattern      = Pattern.StraightDown;
    public float   speed        = 2.5f;
    public float   sineAmp      = 1.5f;
    public float   sineFreq     = 1.5f;
    public float   zigZagWidth  = 1.8f;
    public float   zigZagPeriod = 1.2f;

    float   _startX;
    float   _startY;
    float   _elapsed;
    int     _zigDir = 1;
    float   _zigTimer;
    Transform _player;

    void Start()
    {
        _startX = transform.position.x;
        _startY = transform.position.y;
        var go  = GameObject.FindGameObjectWithTag("Player");
        if (go) _player = go.transform;
    }

    void Update()
    {
        if (GameManager.Instance?.State != GameState.Playing) return;
        _elapsed += Time.deltaTime;

        switch (pattern)
        {
            case Pattern.StraightDown:  MoveStraight();      break;
            case Pattern.SineWave:      MoveSine();          break;
            case Pattern.ZigZag:        MoveZigZag();        break;
            case Pattern.DiveAtPlayer:  MoveDive();          break;
            case Pattern.BossPatrol:    MoveBossPatrol();    break;
        }

        // Despawn if below screen
        if (transform.position.y < -6f)
            Destroy(gameObject);
    }

    void MoveStraight()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);
    }

    void MoveSine()
    {
        float x = _startX + Mathf.Sin(_elapsed * sineFreq * Mathf.PI * 2f) * sineAmp;
        float y = transform.position.y - speed * Time.deltaTime;
        transform.position = new Vector3(x, y, 0f);
    }

    void MoveZigZag()
    {
        _zigTimer += Time.deltaTime;
        if (_zigTimer >= zigZagPeriod * 0.5f)
        {
            _zigDir   = -_zigDir;
            _zigTimer = 0f;
        }
        transform.Translate(new Vector3(_zigDir * zigZagWidth * Time.deltaTime,
                                        -speed * Time.deltaTime, 0f));
    }

    void MoveDive()
    {
        Vector3 target = _player
            ? _player.position
            : new Vector3(0f, -5f, 0f);

        transform.position = Vector3.MoveTowards(
            transform.position, target, speed * Time.deltaTime);
    }

    void MoveBossPatrol()
    {
        // Boss patrols left-right in upper third of screen
        float x = Mathf.PingPong(_elapsed * speed * 0.6f, 7f) - 3.5f;
        float y = Mathf.Lerp(transform.position.y, 3f, Time.deltaTime * 2f);
        transform.position = new Vector3(x, y, 0f);
    }
}
