using Game.Core;

namespace Game.Player
{
    public abstract class ActionState : IState
    {
        protected readonly PlayerContext ctx;
        protected readonly ActionFSM fsm;

        protected ActionState(PlayerContext ctx, ActionFSM fsm)
        {
            this.ctx = ctx;
            this.fsm = fsm;
        }

        protected InputSnapshot Input => ctx.Input;

        public virtual void Enter() { }
        public virtual void Tick(float dt) { }
        public virtual void FixedTick(float dt) { }
        public virtual void Exit() { }

        /// <summary>
        /// Si se presionó el botón de habilidad: entra a UsingAbility si se puede (6.4), o suena el
        /// error si solo falta energía (8.6). Devuelve true si cambió de estado.
        /// </summary>
        protected bool TryStartAbility()
        {
            if (!Input.Ability.IsActive || ctx.Abilities == null) return false;

            if (ctx.Abilities.CanUseSelected(ctx.Locomotion.IsCrouching, out bool lacksEnergy))
            {
                Input.Ability.Consume();
                fsm.ChangeState(fsm.UsingAbility);
                return true;
            }
            if (lacksEnergy)
            {
                Input.Ability.Consume();
                ctx.Abilities.PlayNoEnergy();
            }
            return false;
        }

        /// <summary>Durante el dash se ignoran disparo y habilidad, y su buffer no se guarda.</summary>
        protected bool BlockedByDash()
        {
            if (!ctx.Locomotion.IsDashing) return false;
            Input.Fire.Clear();
            Input.Ability.Clear();
            return true;
        }
    }
}
