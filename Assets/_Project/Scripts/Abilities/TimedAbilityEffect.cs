using UnityEngine;

namespace Game.Abilities
{
    /// <summary>
    /// Base para habilidades por pulsación con bloqueo (granada, taladro, espada, cuchillo; sección 8.4):
    /// UsingAbility dura lockDuration y después vuelve a Ready.
    /// </summary>
    public abstract class TimedAbilityEffect : AbilityEffect
    {
        [Tooltip("Segundos que el jugador queda en UsingAbility después de activarla. [AJUSTAR]")]
        [SerializeField] protected float lockDuration = 0.2f;

        float timer;

        public override bool IsFinished => timer <= 0f;

        public sealed override void Begin(AbilityContext ctx)
        {
            timer = LockDurationFor(ctx);
            OnBegin(ctx);
        }

        /// <summary>Duración del bloqueo de este uso. La espada la saca de la duración de su clip.</summary>
        protected virtual float LockDurationFor(AbilityContext ctx) => lockDuration;

        public sealed override void Sustain(AbilityContext ctx, float dt)
        {
            timer -= dt;
            OnSustain(ctx, dt);
        }

        protected abstract void OnBegin(AbilityContext ctx);
        protected virtual void OnSustain(AbilityContext ctx, float dt) { }
    }
}
