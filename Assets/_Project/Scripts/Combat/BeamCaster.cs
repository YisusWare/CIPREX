using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Rayo de longitud variable con daño por tick a todo lo que toca (sección 8.7). Lo detienen las
    /// paredes (solidMask); atraviesa a los objetivos. No dibuja nada: el efecto usa la longitud
    /// devuelta para su LineRenderer. Lo usa el láser.
    /// </summary>
    public class BeamCaster : MonoBehaviour
    {
        [SerializeField] LayerMask targetMask;
        [Tooltip("Lo que detiene el rayo (Ground, AbilityObstacle).")]
        [SerializeField] LayerMask solidMask;
        [SerializeField] float maxLength = 20f;
        [Tooltip("Radio de la zona de daño alrededor de la línea (la pared se detecta con una línea fina).")]
        [SerializeField] float damageRadius = 0.1875f;
        [SerializeField] int damagePerTick = 1;
        [SerializeField] float tickInterval = 0.2f;

        readonly List<RaycastHit2D> hits = new();
        readonly Dictionary<IDamageable, float> nextHit = new();
        ContactFilter2D solidFilter, targetFilter;

        public float MaxLength => maxLength;

        void Awake()
        {
            solidFilter = new ContactFilter2D();
            solidFilter.SetLayerMask(solidMask);
            solidFilter.useTriggers = false;
            targetFilter = new ContactFilter2D();
            targetFilter.SetLayerMask(targetMask);
            targetFilter.useTriggers = true;
        }

        /// <summary>Olvida los ticks (al empezar un uso nuevo).</summary>
        public void ResetTicks() => nextHit.Clear();

        /// <summary>
        /// Lanza el rayo. Devuelve su longitud; wall es la pared que lo detuvo (collider null si no hay).
        /// Con dealDamage = false solo mide (por ejemplo, durante la carga del láser).
        /// </summary>
        public float Cast(Vector2 origin, Vector2 direction, GameObject source, bool dealDamage, out RaycastHit2D wall)
        {
            wall = default;
            float reach = maxLength;

            int count = Physics2D.Raycast(origin, direction, solidFilter, hits, maxLength);
            for (int i = 0; i < count; i++)
            {
                if (hits[i].distance < reach)
                {
                    reach = hits[i].distance;
                    wall = hits[i];
                }
            }

            if (!dealDamage || reach <= 0f) return reach;

            count = Physics2D.CircleCast(origin, damageRadius, direction, targetFilter, hits, reach);
            for (int i = 0; i < count; i++)
            {
                var target = hits[i].collider.GetComponentInParent<IDamageable>();
                if (target == null) continue;
                if (nextHit.TryGetValue(target, out float next) && Time.time < next) continue;
                nextHit[target] = Time.time + tickInterval;
                target.TakeDamage(new DamageInfo { amount = damagePerTick, direction = direction, source = source });
            }
            return reach;
        }
    }
}
