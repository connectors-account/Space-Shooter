using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A player bullet that travels upwards and is removed when it leaves the screen.
/// </summary>
public class Bullet : MonoBehaviour
{
    public static readonly List<Bullet> Active = new List<Bullet>();

    public float speed = 14f;

    public static Bullet Spawn(Vector3 position)
    {
        GameObject go = new GameObject("Bullet");
        go.transform.position = position;

        SphereCollider col = go.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.2f;
        GameBootstrap.AddKinematicBody(go);

        GameBootstrap.CreatePart(PrimitiveType.Capsule, go.transform, Vector3.zero, new Vector3(0.15f, 0.2f, 0.15f), new Color(1f, 0.95f, 0.3f));
        return go.AddComponent<Bullet>();
    }

    void OnEnable() { Active.Add(this); }
    void OnDisable() { Active.Remove(this); }

    void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;

        GameManager gm = GameManager.Instance;
        float top = gm != null ? gm.HalfScreenSize.y : 6f;
        if (transform.position.y > top + 1f) Destroy(gameObject);
    }
}
