using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Daña a lo que esté dentro de su collider (enemigos por contacto, pinchos, zonas de prueba).
    /// Usa una consulta por paso de física en vez de OnTrigger, así funciona con hurtboxes trigger.
    /// Los i-frames del objetivo evitan el daño en cada frame.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ContactDamage : MonoBehaviour
    {
        [SerializeField] int damage = 4;
        [Tooltip("Fosos y pinchos: ignora los i-frames.")]
        [SerializeField] bool instantKill;
        [Tooltip("Capas que reciben el daño (normalmente PlayerHurtbox).")]
        [SerializeField] LayerMask targetMask;

        Collider2D area;
        ContactFilter2D filter;
        readonly List<Collider2D> results = new();
        readonly HashSet<IDamageable> damagedThisStep = new();

        void Awake()
        {
            area = GetComponent<Collider2D>();
            filter = new ContactFilter2D();
            filter.SetLayerMask(targetMask);
            filter.useTriggers = true;
        }

        void FixedUpdate()
        {
            if (area.Overlap(filter, results) == 0) return;

            damagedThisStep.Clear();
            foreach (var col in results)
            {
                var target = col.GetComponentInParent<IDamageable>();
                if (target == null || !damagedThisStep.Add(target)) continue;

                Vector2 dir = ((Vector2)col.bounds.center - (Vector2)area.bounds.center).normalized;
                target.TakeDamage(new DamageInfo
                {
                    amount = damage,
                    direction = dir,
                    source = gameObject,
                    instantKill = instantKill,
                });
            }
        }
    }
}
