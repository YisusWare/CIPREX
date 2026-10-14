namespace Game.Abilities
{
    /// <summary>
    /// Base para habilidades sostenidas (lanzallamas, láser; sección 8.4): siguen activas hasta que se
    /// suelta el botón. Si se acaba la energía, llega daño o empieza un dash, UsingAbility llama a End.
    /// </summary>
    public abstract class HeldAbilityEffect : AbilityEffect
    {
        bool released;

        public override bool IsFinished => released;

        public sealed override void Begin(AbilityContext ctx)
        {
            released = false;
            OnBegin(ctx);
        }

        public sealed override void ButtonReleased() => released = true;

        protected abstract void OnBegin(AbilityContext ctx);
    }
}
