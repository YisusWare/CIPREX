using UnityEngine;

namespace Game.Combat
{
    /// <summary>Cualquier cosa que recibe daño (sección 10.1).</summary>
    public interface IDamageable
    {
        void TakeDamage(DamageInfo info);
    }

    public struct DamageInfo
    {
        public int amount;
        /// <summary>Dirección del golpe (de la fuente hacia el objetivo), normalizada.</summary>
        public Vector2 direction;
        public GameObject source;
        /// <summary>Fosos y pinchos: ignoran los i-frames.</summary>
        public bool instantKill;
        /// <summary>Fuerza de empuje (u/s); 0 = ninguno. Opcional (10.1): cada receptor decide cómo reacciona.</summary>
        public float knockback;
        /// <summary>Segundos de aturdimiento; 0 = ninguno. Opcional (10.1).</summary>
        public float stunDuration;
    }
}
