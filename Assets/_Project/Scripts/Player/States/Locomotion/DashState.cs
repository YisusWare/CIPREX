namespace Game.Player
{
    /// <summary>
    /// Dash en el suelo (sección 6.3): hacia donde mira, sin voltear, collider normal.
    /// Saltar durante el dash da un dash jump que conserva el impulso.
    /// </summary>
    public class DashState : LocomotionState
    {
        float timer;
        int dir;

        public DashState(PlayerContext ctx, LocomotionFSM fsm) : base(ctx, fsm) { }

        public override void Enter()
        {
            fsm.OnGrounded();
            dir = fsm.FacingDir;
            fsm.DashDirection = dir;
            timer = Data.dashDuration;
            Motor.SetVelocityX(Data.DashSpeed * dir);
        }

        public override void Tick(float dt)
        {
            timer -= dt;
            if (!Motor.IsGrounded) { fsm.GoAirborne(AirborneEntry.Fall); return; }
            if (TryStartJump(carryDashMomentum: true)) return;
            if (timer <= 0f) { fsm.GoGrounded(); return; }
        }

        public override void FixedTick(float dt)
        {
            Motor.SetVelocityX(Data.DashSpeed * dir);
            Motor.ApplyGravity(dt);
        }
    }
}
