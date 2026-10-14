using Game.Combat;
using UnityEngine;

namespace Game.Abilities
{
    /// <summary>
    /// TEMPORAL (Hito 4, fase A): efecto de prueba por pulsación. Dispara un abanico de proyectiles para
    /// probar energía, cooldown, bloqueo, límite en pantalla y HUD. En la fase B se reemplaza por los
    /// efectos reales (granada, espada, taladro) en el effectPrefab de cada AbilityData.
    /// </summary>
    public class TestProjectileAbilityEffect : TimedAbilityEffect
    {
        [SerializeField] Projectile projectilePrefab;
        [SerializeField] float speed = 14f;
        [SerializeField] int damage = 2;
        [SerializeField, Min(1)] int count = 3;
        [Tooltip("Ángulo total del abanico, en grados.")]
        [SerializeField] float spreadDegrees = 30f;

        ProjectilePool pool;

        public override int ActiveInstances => pool?.CountActive ?? 0;

        protected override void OnBegin(AbilityContext ctx)
        {
            if (projectilePrefab == null) return;
            pool ??= new ProjectilePool(projectilePrefab, $"{name}_Projectiles");

            float step = count > 1 ? spreadDegrees / (count - 1) : 0f;
            float start = -spreadDegrees * 0.5f;
            for (int i = 0; i < count; i++)
            {
                float angle = count > 1 ? start + step * i : 0f;
                Vector2 dir = Quaternion.Euler(0f, 0f, angle) * ctx.direction;
                pool.Get().Launch(ctx.origin, dir, speed, damage, Owner != null ? Owner.gameObject : null);
            }
        }
    }
}
