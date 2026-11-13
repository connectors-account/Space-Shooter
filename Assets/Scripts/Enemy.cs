using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An enemy that moves down the screen. Handles collisions with bullets and the player.
/// </summary>
public class Enemy : MonoBehaviour
{
    public static readonly List<Enemy> Active = new List<Enemy>();

    public float speed = 3f;
    public int scoreValue = 10;

    bool hit;

    public static Enemy Spawn(Vector3 position, float speed)
    {
        GameObject go = new GameObject("Enemy");
        go.transform.position = position;

        BoxCollider col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(1.0f, 0.9f, 0.6f);
        GameBootstrap.AddKinematicBody(go);

        Color body = new Color(0.9f, 0.2f, 0.25f);
        Color dark = new Color(0.5f, 0.05f, 0.1f);
        GameBootstrap.CreatePart(PrimitiveType.Sphere, go.transform, Vector3.zero, new Vector3(0.8f, 0.6f, 0.6f), body);
        GameBootstrap.CreatePart(PrimitiveType.Cube, go.transform, new Vector3(-0.45f, 0.1f, 0f), new Vector3(0.2f, 0.6f, 0.2f), dark);
        GameBootstrap.CreatePart(PrimitiveType.Cube, go.transform, new Vector3(0.45f, 0.1f, 0f), new Vector3(0.2f, 0.6f, 0.2f), dark);
        GameBootstrap.CreatePart(PrimitiveType.Sphere, go.transform, new Vector3(0f, -0.1f, -0.25f), new Vector3(0.25f, 0.25f, 0.25f), new Color(0.3f, 1f, 0.4f));

        Enemy enemy = go.AddComponent<Enemy>();
        enemy.speed = speed;
        return enemy;
    }

    void OnEnable() { Active.Add(this); }
    void OnDisable() { Active.Remove(this); }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.IsGameOver) return;

        transform.position += Vector3.down * speed * Time.deltaTime;
        if (transform.position.y < -gm.HalfScreenSize.y - 1f) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        GameManager gm = GameManager.Instance;
        if (hit || gm == null || gm.IsGameOver) return;

        GameObject otherObject = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;

        Bullet bullet = otherObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            hit = true;
            Destroy(bullet.gameObject);
            Destroy(gameObject);
            gm.AddScore(scoreValue);
            return;
        }

        if (otherObject.GetComponent<PlayerController>() != null)
        {
            hit = true;
            Destroy(gameObject);
            gm.GameOver();
        }
    }
}
