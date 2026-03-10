using UnityEngine;
using System.Collections.Generic;

namespace SpaceShooter.Managers
{
    public class EffectsManager : MonoBehaviour
    {
        public static EffectsManager Instance { get; private set; }

        [Header("Explosion Effects")]
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private int explosionPoolSize = 20;

        [Header("Hit Effects")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private int hitEffectPoolSize = 30;

        [Header("Trail Effects")]
        [SerializeField] private GameObject trailPrefab;

        private Queue<GameObject> explosionPool;
        private Queue<GameObject> hitEffectPool;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializePools();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePools()
        {
            explosionPool = new Queue<GameObject>();
            hitEffectPool = new Queue<GameObject>();

            if (explosionPrefab != null)
            {
                for (int i = 0; i < explosionPoolSize; i++)
                {
                    GameObject explosion = Instantiate(explosionPrefab, transform);
                    explosion.SetActive(false);
                    explosionPool.Enqueue(explosion);
                }
            }

            if (hitEffectPrefab != null)
            {
                for (int i = 0; i < hitEffectPoolSize; i++)
                {
                    GameObject hitEffect = Instantiate(hitEffectPrefab, transform);
                    hitEffect.SetActive(false);
                    hitEffectPool.Enqueue(hitEffect);
                }
            }
        }

        public void SpawnExplosion(Vector3 position, float scale = 1f)
        {
            GameObject explosion = GetFromPool(explosionPool, explosionPrefab);
            if (explosion != null)
            {
                explosion.transform.position = position;
                explosion.transform.localScale = Vector3.one * scale;
                explosion.SetActive(true);

                Explosion explosionComponent = explosion.GetComponent<Explosion>();
                if (explosionComponent != null)
                {
                    explosionComponent.Play(() => ReturnToPool(explosion, explosionPool));
                }
                else
                {
                    StartCoroutine(ReturnToPoolAfterDelay(explosion, explosionPool, 1f));
                }
            }
        }

        public void SpawnHitEffect(Vector3 position, Color color)
        {
            GameObject hitEffect = GetFromPool(hitEffectPool, hitEffectPrefab);
            if (hitEffect != null)
            {
                hitEffect.transform.position = position;
                hitEffect.SetActive(true);

                ParticleSystem ps = hitEffect.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.startColor = color;
                    ps.Play();
                }

                StartCoroutine(ReturnToPoolAfterDelay(hitEffect, hitEffectPool, 0.5f));
            }
        }

        private GameObject GetFromPool(Queue<GameObject> pool, GameObject prefab)
        {
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }
            else if (prefab != null)
            {
                return Instantiate(prefab, transform);
            }
            return null;
        }

        private void ReturnToPool(GameObject obj, Queue<GameObject> pool)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        private System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject obj, Queue<GameObject> pool, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnToPool(obj, pool);
        }

        public void ClearAllEffects()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}
