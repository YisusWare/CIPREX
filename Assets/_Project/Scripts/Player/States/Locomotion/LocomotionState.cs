using Game.Core;

namespace Game.Player
{
    /// <summary>Base de los estados de locomoción, con ayudas comunes.</summary>
    public abstract class LocomotionState : IState
    {
        protected readonly PlayerContext ctx;
        protected readonly LocomotionFSM fsm;

        protected LocomotionState(PlayerContext ctx, LocomotionFSM fsm)
        {
            this.ctx = ctx;
            this.fsm = fsm;
        }

        protected InputSnapshot Input => ctx.Input;
        protected PlayerMotor Motor => ctx.Motor;
        protected PlayerMovementData Data => ctx.Movement;
        /// <summary>No se puede hacer dash durante una habilidad por pulsación (8.5).</summary>
        protected bool DashBlocked => ctx.Action != null && ctx.Action.BlocksDash;

        /// <summary>moveSpeedMultiplier de la habilidad activa (sección 6.4); 1 si no hay ninguna.</summary>
        protected float SpeedMultiplier => ctx.Action != null ? ctx.Action.MoveSpeedMultiplier : 1f;

        public virtual void Enter() { }
        public virtual void Tick(float dt) { }
        public virtual void FixedTick(float dt) { }
        public virtual void Exit() { }

        protected void UpdateFacing()
        {
            if (Input.MoveX != 0) fsm.FacingDir = Input.MoveX;
        }

        /// <summary>Salto desde el suelo, si hay uno en el buffer.</summary>
        protected bool TryStartJump(bool carryDashMomentum = false)
        {
            if (!Input.Jump.Consume()) return false;
            fsm.GoAirborne(AirborneEntry.Jump, carryDashMomentum);
            return true;
        }

        /// <summary>Dash en el suelo, si hay uno en el buffer.</summary>
        protected bool TryStartDash()
        {
            if (DashBlocked) return false; // se queda en el buffer por si la habilidad termina a tiempo
            if (!Input.Dash.Consume()) return false;
            fsm.ChangeState(fsm.Dash);
            return true;
        }

        /// <summary>Movimiento horizontal en el suelo + gravedad (mantiene el contacto con el suelo).</summary>
        protected void GroundStep(float targetX, float dt)
        {
            Motor.MoveTowardsX(targetX, Data.groundAcceleration, dt);
            Motor.ApplyGravity(dt);
        }
    }
}
