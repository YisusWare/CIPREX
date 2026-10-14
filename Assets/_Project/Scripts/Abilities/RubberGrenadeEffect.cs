using Game.Audio;
using Game.Combat;
using Game.Player;
using UnityEngine;

namespace Game.Abilities
{
    /// <summary>
    /// Granada de goma de Rosslyn (sección 8.2): sale del cañón con un arco distinto para cada
    /// dirección de apuntado y explota con daño de área. Sin límite en pantalla (la limitan la
    /// energía y el cooldown de AbilityData).
    /// </summary>
    public class RubberGrenadeEffect : TimedAbilityEffect
    {
        [SerializeField] RubberGrenade grenadePrefab;
        [SerializeField] Explosion explosionPrefab;

        [Header("Lanzamiento (u/s; la X se multiplica por la dirección en la que mira) [AJUSTAR]")]
        [SerializeField] Vector2 velocityForward = new(6f, 6f);
        [SerializeField] Vector2 velocityDiagUp = new(4f, 9f);
        [SerializeField] Vector2 velocityUp = new(0f, 11f);
        [Tooltip("Solo en el aire.")]
        [SerializeField] Vector2 velocityDown = new(3f, -6f);
        [SerializeField] Vector2 velocityCrouch = new(7f, 3f);

        [Header("Explosión [AJUSTAR]")]
        [SerializeField] float fuseTime = 2f;
        [SerializeField] float explosionRadius = 1.5f;
        [SerializeField] int explosionDamage = 4;
        [Tooltip("Hurtboxes que la hacen explotar y reciben el daño (EnemyHurtbox).")]
        [SerializeField] LayerMask targetMask;
        [Tooltip("Si la granada nace dentro de una pared, explota ahí mismo.")]
        [SerializeField] LayerMask solidMask;
        [SerializeField] SfxData explosionSfx;

        ComponentPool<RubberGrenade> grenades;
        ComponentPool<Explosion> explosions;
        System.Action<RubberGrenade> onExplode;

        public override int ActiveInstances => grenades?.CountActive ?? 0;

        protected override void OnBegin(AbilityContext ctx)
        {
            if (grenadePrefab == null) return;
            grenades ??= new ComponentPool<RubberGrenade>(grenadePrefab, "Grenades");
            if (explosionPrefab != null) explosions ??= new ComponentPool<Explosion>(explosionPrefab, "Explosions");
            if (onExplode == null) onExplode = OnGrenadeExploded;

            Vector2 v = LaunchVelocity(ctx.aim, ctx.crouching);
            v.x *= ctx.facing;

            var grenade = grenades.Get();
            grenade.Launch(ctx.origin, v, fuseTime, targetMask, onExplode);

            if (Physics2D.OverlapPoint(ctx.origin, solidMask) != null) grenade.Explode();
        }

        Vector2 LaunchVelocity(AimDirection aim, bool crouching)
        {
            if (crouching) return velocityCrouch;
            return aim switch
            {
                AimDirection.DiagUp => velocityDiagUp,
                AimDirection.Up => velocityUp,
                AimDirection.Down => velocityDown,
                _ => velocityForward,
            };
        }

        void OnGrenadeExploded(RubberGrenade grenade)
        {
            Vector2 pos = grenade.transform.position;
            SfxPlayer.Play(explosionSfx, pos);
            var source = Owner != null ? Owner.gameObject : null;
            if (explosions != null)
            {
                explosions.Get().Explode(pos, explosionRadius, explosionDamage, targetMask, source);
            }
            else
            {
                // Sin prefab de explosión: solo el daño.
                var filter = new ContactFilter2D();
                filter.SetLayerMask(targetMask);
                filter.useTriggers = true;
                var results = new System.Collections.Generic.List<Collider2D>();
                var damaged = new System.Collections.Generic.HashSet<IDamageable>();
                int count = Physics2D.OverlapCircle(pos, explosionRadius, filter, results);
                for (int i = 0; i < count; i++)
                {
                    var t = results[i].GetComponentInParent<IDamageable>();
                    if (t != null && damaged.Add(t))
                        t.TakeDamage(new DamageInfo { amount = explosionDamage, direction = Vector2.up, source = source });
                }
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
#endif
    }
}
