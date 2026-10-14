using System;
using Game.Combat;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Vida e i-frames (secciones 5.2 y 10.1). Lanza Damaged y Died; no cambia estados.
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        int maxHealth = 28; // lo asigna SetMaxHealth (CharacterData, sección 5.4)
        [Tooltip("Segundos de invulnerabilidad después de recibir daño. [AJUSTAR]")]
        [SerializeField] float invulnerabilityDuration = 1.5f;

        [Header("Knockback (estado Hurt)")]
        [Tooltip("Segundos sin control después del golpe. [AJUSTAR]")]
        [SerializeField] float knockbackDuration = 0.3f;
        [Tooltip("Velocidad horizontal hacia atrás durante el knockback (u/s). [AJUSTAR]")]
        [SerializeField] float knockbackSpeed = 2.5f;

        [Header("Muerte")]
        [Tooltip("Segundos entre morir y reaparecer.")]
        [SerializeField] float respawnDelay = 1.5f;

        float invulnerableTimer;

        public int Current { get; private set; }
        public int Max => maxHealth;
        public bool IsDead { get; private set; }
        public bool IsInvulnerable => invulnerableTimer > 0f;
        public float KnockbackDuration => knockbackDuration;
        public float KnockbackSpeed => knockbackSpeed;
        public float RespawnDelay => respawnDelay;

        public event Action<DamageInfo> Damaged;
        public event Action Died;
        /// <summary>(actual, máximo)</summary>
        public event Action<int, int> Changed;

        void Awake() => Current = maxHealth;

        /// <summary>Lo llama PlayerController.Initialize con la vida del personaje. Deja la vida llena.</summary>
        public void SetMaxHealth(int value)
        {
            maxHealth = Mathf.Max(1, value);
            SetHealth(maxHealth);
        }

        void Update()
        {
            if (invulnerableTimer > 0f) invulnerableTimer -= Time.deltaTime;
        }

        public void TakeDamage(DamageInfo info)
        {
            if (IsDead) return;

            // Los peligros que matan al instante ignoran los i-frames.
            if (info.instantKill)
            {
                SetHealth(0);
                Die();
                return;
            }

            if (IsInvulnerable || info.amount <= 0) return;

            SetHealth(Current - info.amount);
            if (Current <= 0)
            {
                Die();
                return;
            }

            invulnerableTimer = invulnerabilityDuration;
            Damaged?.Invoke(info);
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0) return;
            SetHealth(Current + amount);
        }

        /// <summary>Vida llena, sin i-frames. Se usa al reaparecer.</summary>
        public void Refill()
        {
            IsDead = false;
            invulnerableTimer = 0f;
            SetHealth(maxHealth);
        }

        void Die()
        {
            IsDead = true;
            invulnerableTimer = 0f;
            Died?.Invoke();
        }

        void SetHealth(int value)
        {
            Current = Mathf.Clamp(value, 0, maxHealth);
            Changed?.Invoke(Current, maxHealth);
        }
    }
}
