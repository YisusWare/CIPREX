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
