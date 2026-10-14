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
            timer = lockDuration;
            OnBegin(ctx);
        }

        public sealed override void Sustain(AbilityContext ctx, float dt)
        {
            timer -= dt;
            OnSustain(ctx, dt);
        }

        protected abstract void OnBegin(AbilityContext ctx);
        protected virtual void OnSustain(AbilityContext ctx, float dt) { }
    }
}
