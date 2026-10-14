using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Combat
{
    /// <summary>
    /// Proyectil movido por código con un CircleCast por frame (sección 7.3).
    /// Lo usan el disparo básico, el taladro y los enemigos. Sale de un pool.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Tooltip("Capas que reciben daño (hurtboxes del bando contrario).")]
        [SerializeField] LayerMask targetMask;
        [Tooltip("Capas que detienen el proyectil (Ground, AbilityObstacle).")]
        [SerializeField] LayerMask solidMask;
        [Tooltip("Radio de colisión en unidades.")]
        [SerializeField] float radius = 0.125f;
        [SerializeField] float maxLifetime = 2f;
        [Tooltip("Enemigos extra que atraviesa. 0 = se destruye al primer golpe; -1 = sin límite.")]
        [SerializeField] int pierceCount = 0;
        [Tooltip("Margen fuera de la cámara (en fracción de pantalla) antes de desaparecer.")]
        [SerializeField] float offscreenMargin = 0.05f;

        static Camera cachedCamera;

        readonly List<RaycastHit2D> hits = new();
        readonly HashSet<IDamageable> alreadyHit = new();
        IObjectPool<Projectile> pool;
        ContactFilter2D filter;

        Vector2 velocity;
        int damage;
        GameObject owner;
        float life;
        int pierced;
        bool released;

        public Vector2 Velocity => velocity;

        public void SetPool(IObjectPool<Projectile> owningPool) => pool = owningPool;

        void Awake()
        {
            filter = new ContactFilter2D();
            filter.SetLayerMask(targetMask | solidMask);
            filter.useTriggers = true;
        }

        public void Launch(Vector2 position, Vector2 direction, float speed, int damageAmount, GameObject source)
        {
            transform.position = position;
            velocity = direction.normalized * speed;
            damage = damageAmount;
            owner = source;
            life = 0f;
            pierced = 0;
            released = false;
            alreadyHit.Clear();
        }

        void Update()
        {
            if (released) return;
            float dt = Time.deltaTime;

            life += dt;
            if (life >= maxLifetime) { Despawn(); return; }

            Vector2 pos = transform.position;
            Vector2 step = velocity * dt;
            float distance = step.magnitude;

            if (distance > 0f)
            {
                Physics2D.CircleCast(pos, radius, step / distance, filter, hits, distance);
                hits.Sort((a, b) => a.distance.CompareTo(b.distance));

                foreach (var hit in hits)
                {
                    int layerBit = 1 << hit.collider.gameObject.layer;

                    if ((solidMask.value & layerBit) != 0)
                    {
                        transform.position = hit.centroid;
                        OnHitSolid(hit);
                        Despawn();
                        return;
                    }

                    var target = hit.collider.GetComponentInParent<IDamageable>();
                    if (target == null || alreadyHit.Contains(target)) continue;

                    alreadyHit.Add(target);
                    target.TakeDamage(new DamageInfo
                    {
                        amount = damage,
                        direction = velocity.normalized,
                        source = owner,
                    });

                    if (pierceCount >= 0 && pierced++ >= pierceCount)
                    {
                        transform.position = hit.centroid;
                        Despawn();
                        return;
                    }
                }
            }

            transform.position = pos + step;

            if (IsOffscreen(pos + step)) Despawn();
        }

        /// <summary>Para efectos al chocar con una pared (y, en el Hito 4, romper bloques).</summary>
        protected virtual void OnHitSolid(RaycastHit2D hit) { }

        bool IsOffscreen(Vector2 worldPos)
        {
            if (cachedCamera == null) cachedCamera = Camera.main;
            if (cachedCamera == null) return false;
            Vector3 vp = cachedCamera.WorldToViewportPoint(worldPos);
            return vp.x < -offscreenMargin || vp.x > 1f + offscreenMargin ||
                   vp.y < -offscreenMargin || vp.y > 1f + offscreenMargin;
        }

        public void Despawn()
        {
            if (released) return;
            released = true;
            if (pool != null) pool.Release(this);
            else Destroy(gameObject);
        }
    }
}
