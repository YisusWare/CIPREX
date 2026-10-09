namespace Game.Player
{
    public class IdleState : LocomotionState
    {
        public IdleState(PlayerContext ctx, LocomotionFSM fsm) : base(ctx, fsm) { }

        public override void Enter() => fsm.OnGrounded();

        public override void Tick(float dt)
        {
            if (!Motor.IsGrounded) { fsm.GoAirborne(AirborneEntry.Fall); return; }
            if (TryStartJump()) return;
            if (TryStartDash()) return;
            if (Input.MoveY < 0) { fsm.ChangeState(fsm.Crouch); return; }
            if (Input.MoveX != 0) { fsm.ChangeState(fsm.Run); return; }
        }

        public override void FixedTick(float dt) => GroundStep(0f, dt);
    }
}
