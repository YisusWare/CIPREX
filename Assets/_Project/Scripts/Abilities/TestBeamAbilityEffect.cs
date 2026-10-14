using System.Collections.Generic;
using Game.Combat;
using UnityEngine;

namespace Game.Abilities
{
    /// <summary>
    /// TEMPORAL (Hito 4, fase A): efecto de prueba sostenido. Un rayo corto que sigue la dirección de
    /// apuntado y daña por tick, para probar el consumo por segundo, soltar el botón y la cancelación
    /// por dash o daño. En la fase B se reemplaza por el lanzallamas y el láser reales.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class TestBeamAbilityEffect : HeldAbilityEffect
    {
        [SerializeField] float length = 4f;
        [SerializeField] int damagePerTick = 1;
        [SerializeField] float tickInterval = 0.15f;
        [SerializeField] LayerMask targetMask;
        [SerializeField] LayerMask solidMask;
        [SerializeField] Color color = new(1f, 0.55f, 0.15f);

        LineRenderer line;
        readonly Dictionary<IDamageable, float> nextHit = new();
        readonly List<RaycastHit2D> hits = new();
        ContactFilter2D filter;

        void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.startWidth = line.endWidth = 0.375f; // 6 px
            line.startColor = line.endColor = color;
            line.sortingOrder = 30;
            if (line.sharedMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
                if (shader != null) line.material = new Material(shader);
            }
            line.enabled = false;

            filter = new ContactFilter2D();
            filter.SetLayerMask(targetMask | solidMask);
            filter.useTriggers = true;
        }

        protected override void OnBegin(AbilityContext ctx)
        {
            nextHit.Clear();
            line.enabled = true;
            Sustain(ctx, 0f);
        }

        public override void Sustain(AbilityContext ctx, float dt)
        {
            float reach = length;
            Physics2D.Raycast(ctx.origin, ctx.direction, filter, hits, length);
            hits.Sort((a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                if ((solidMask.value & (1 << hit.collider.gameObject.layer)) != 0)
                {
                    reach = hit.distance;
                    break;
                }
                var target = hit.collider.GetComponentInParent<IDamageable>();
                if (target == null) continue;
                if (nextHit.TryGetValue(target, out float t) && Time.time < t) continue;
                nextHit[target] = Time.time + tickInterval;
                target.TakeDamage(new DamageInfo
                {
                    amount = damagePerTick,
                    direction = ctx.direction,
                    source = Owner != null ? Owner.gameObject : null,
                });
            }

            line.SetPosition(0, ctx.origin);
            line.SetPosition(1, ctx.origin + ctx.direction * reach);
        }

        public override void End() => line.enabled = false;
    }
}
