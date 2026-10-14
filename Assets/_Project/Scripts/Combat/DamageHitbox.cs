using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Zona rectangular que daña (sección 8.7). No es un collider: consulta con Physics2D.OverlapBox
    /// usando su propia forma (size, offset) y el transform, así que se puede girar, voltear con la
    /// escala y activar en el mismo frame sin esperar a la física. La forma se ve con Gizmos.
    /// Dos modos: un golpe por objetivo por activación (espada) o daño por tick (lanzallamas).
    /// También puede destruir proyectiles (la espada, capa EnemyProjectile).
    /// </summary>
    public class DamageHitbox : MonoBehaviour
    {
        public enum HitMode { OncePerTarget, Tick }

        [Tooltip("Tamaño en unidades locales (antes de la escala del transform).")]
        [SerializeField] Vector2 size = Vector2.one;
        [SerializeField] Vector2 offset;
        [Tooltip("Capas que reciben daño (hurtboxes del bando contrario).")]
        [SerializeField] LayerMask targetMask;
        [SerializeField] HitMode mode = HitMode.OncePerTarget;
        [Tooltip("Solo en modo Tick: segundos entre golpes al mismo objetivo.")]
        [SerializeField] float tickInterval = 0.15f;
        [Tooltip("Capas de proyectiles que destruye (EnemyProjectile). Vacío = ninguno. El proyectil necesita un collider trigger.")]
        [SerializeField] LayerMask projectileMask;

        readonly List<Collider2D> results = new();
        readonly Dictionary<IDamageable, float> nextHit = new();
        ContactFilter2D filter;
        GameObject source;
        int damage;
        float knockback, stunDuration;

        public bool IsActive { get; private set; }
        public Vector2 Size { get => size; set => size = value; }

        /// <summary>Cada vez que daña a un objetivo.</summary>
        public event Action<IDamageable> Hit;
        /// <summary>Cada vez que destruye un proyectil.</summary>
        public event Action<Projectile> ProjectileDestroyed;

        void Awake()
        {
            filter = new ContactFilter2D();
            filter.SetLayerMask(targetMask | projectileMask);
            filter.useTriggers = true;
        }

        /// <summary>Empieza a dañar. Limpia el registro de golpes y revisa en este mismo frame.</summary>
        public void Activate(GameObject damageSource, int damageAmount, float knockbackForce = 0f, float stun = 0f)
        {
            source = damageSource;
            damage = damageAmount;
            knockback = knockbackForce;
            stunDuration = stun;
            nextHit.Clear();
            IsActive = true;
            Check();
        }

        public void Deactivate() => IsActive = false;

        void FixedUpdate()
        {
            if (IsActive) Check();
        }

        /// <summary>Revisa la zona una vez. Normalmente lo llama FixedUpdate.</summary>
        public void Check()
        {
            GetWorldBox(out Vector2 center, out Vector2 worldSize, out float angle);
            if (worldSize.x <= 0f || worldSize.y <= 0f) return;

            int count = Physics2D.OverlapBox(center, worldSize, angle, filter, results);
            for (int i = 0; i < count; i++)
            {
                var col = results[i];
                int bit = 1 << col.gameObject.layer;

                if ((projectileMask.value & bit) != 0)
                {
                    var projectile = col.GetComponentInParent<Projectile>();
                    if (projectile != null && projectile.isActiveAndEnabled && !projectile.Indestructible)
                    {
                        projectile.Despawn();
                        ProjectileDestroyed?.Invoke(projectile);
                    }
                    continue;
                }

                var target = col.GetComponentInParent<IDamageable>();
                if (target == null) continue;
                if (nextHit.TryGetValue(target, out float next) && Time.time < next) continue;

                nextHit[target] = mode == HitMode.Tick ? Time.time + tickInterval : float.PositiveInfinity;
                Vector2 dir = (Vector2)col.bounds.center - center;
                target.TakeDamage(new DamageInfo
                {
                    amount = damage,
                    direction = dir.sqrMagnitude > 0.0001f ? dir.normalized : (Vector2)transform.right,
                    source = source,
                    knockback = knockback,
                    stunDuration = stunDuration,
                });
                Hit?.Invoke(target);
            }
        }

        /// <summary>Centro, tamaño y ángulo en el mundo. Respeta rotación y escala negativa (volteo).</summary>
        public void GetWorldBox(out Vector2 center, out Vector2 worldSize, out float angle)
        {
            center = transform.TransformPoint(offset);
            Vector2 ax = transform.TransformVector(Vector3.right);
            Vector2 ay = transform.TransformVector(Vector3.up);
            worldSize = new Vector2(Mathf.Abs(size.x) * ax.magnitude, Mathf.Abs(size.y) * ay.magnitude);
            angle = Mathf.Atan2(ax.y, ax.x) * Mathf.Rad2Deg;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            GetWorldBox(out Vector2 center, out Vector2 worldSize, out float angle);
            Gizmos.color = IsActive ? new Color(1f, 0.3f, 0.2f, 0.9f) : new Color(1f, 0.8f, 0.2f, 0.35f);
            Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0f, 0f, angle), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, worldSize);
            Gizmos.matrix = Matrix4x4.identity;
        }
#endif
    }
}
