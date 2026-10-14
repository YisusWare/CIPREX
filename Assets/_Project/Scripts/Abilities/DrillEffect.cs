using Game.Combat;
using UnityEngine;

namespace Game.Abilities
{
    /// <summary>
    /// Taladro de Veyron (sección 8.2): un proyectil perforante en las 4 direcciones. El límite de
    /// taladros en pantalla es AbilityData.maxInstancesOnScreen (1).
    /// </summary>
    public class DrillEffect : TimedAbilityEffect
    {
        [SerializeField] DrillProjectile projectilePrefab;
        [Tooltip("u/s. [AJUSTAR]")]
        [SerializeField] float speed = 10f;
        [Tooltip("Daño a cada enemigo que atraviesa. [AJUSTAR]")]
        [SerializeField] int damage = 3;

        ProjectilePool pool;

        public override int ActiveInstances => pool?.CountActive ?? 0;

        protected override void OnBegin(AbilityContext ctx)
        {
            if (projectilePrefab == null) return;
            pool ??= new ProjectilePool(projectilePrefab, "DrillProjectiles", 2, 8);

            var drill = (DrillProjectile)pool.Get();
            drill.Ability = Data;
            drill.Launch(ctx.origin, ctx.direction, speed, damage, Owner != null ? Owner.gameObject : null);
        }
    }
}
