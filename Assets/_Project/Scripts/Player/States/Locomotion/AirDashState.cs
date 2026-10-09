namespace Game.Player
{
    /// <summary>
    /// Air dash (sección 6.3): horizontal, sin gravedad, uno por salto.
    /// Al terminar, la velocidad Y empieza en 0.
    /// </summary>
    public class AirDashState : LocomotionState
    {
        // Evita "aterrizar" en el primer frame si se hace el air dash justo al salir del suelo.
        const float MinTimeBeforeLanding = 0.05f;

        float timer;
        float elapsed;
        int dir;

        public AirDashState(PlayerContext ctx, LocomotionFSM fsm) : base(ctx, fsm) { }

        public override void Enter()
        {
            fsm.AirDashesLeft--;
            fsm.CarriesDashMomentum = false;
            dir = fsm.FacingDir;
            fsm.DashDirection = dir;
            timer = Data.dashDuration;
            elapsed = 0f;
            Motor.Velocity = new UnityEngine.Vector2(Data.DashSpeed * dir, 0f);
        }

        public override void Tick(float dt)
        {
            timer -= dt;
            elapsed += dt;
            if (elapsed >= MinTimeBeforeLanding && Motor.IsGrounded) { fsm.GoGrounded(); return; }
            if (timer <= 0f) { fsm.GoAirborne(AirborneEntry.AfterAirDash); return; }
        }

        public override void FixedTick(float dt)
        {
            Motor.Velocity = new UnityEngine.Vector2(Data.DashSpeed * dir, 0f);
        }
    }
}
