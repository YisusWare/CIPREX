namespace Game.Player
{
    /// <summary>
    /// Agachado: collider a la mitad, sin moverse (se puede voltear).
    /// Sin salto ni dash desde aquí (secciones 6.3 y 17.1).
    /// </summary>
    public class CrouchState : LocomotionState
    {
        public CrouchState(PlayerContext ctx, LocomotionFSM fsm) : base(ctx, fsm) { }

        public override void Enter()
        {
            fsm.OnGrounded();
            Motor.SetCrouching(true);
        }

        public override void Exit() => Motor.SetCrouching(false);

        public override void Tick(float dt)
        {
            UpdateFacing();
            if (!Motor.IsGrounded) { fsm.GoAirborne(AirborneEntry.Fall); return; }
            if (Input.MoveY >= 0 && Motor.CanStandUp()) { fsm.GoGrounded(); return; }
        }

        public override void FixedTick(float dt) => GroundStep(0f, dt);
    }
}
