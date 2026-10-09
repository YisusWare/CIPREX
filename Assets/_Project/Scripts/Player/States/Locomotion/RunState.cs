namespace Game.Player
{
    public class RunState : LocomotionState
    {
        public RunState(PlayerContext ctx, LocomotionFSM fsm) : base(ctx, fsm) { }

        public override void Enter()
        {
            fsm.OnGrounded();
            UpdateFacing();
        }

        public override void Tick(float dt)
        {
            if (!Motor.IsGrounded) { fsm.GoAirborne(AirborneEntry.Fall); return; }
            UpdateFacing();
            if (TryStartJump()) return;
            if (TryStartDash()) return;
            if (Input.MoveY < 0) { fsm.ChangeState(fsm.Crouch); return; }
            if (Input.MoveX == 0) { fsm.ChangeState(fsm.Idle); return; }
        }

        public override void FixedTick(float dt) => GroundStep(Input.MoveX * Data.runSpeed, dt);
    }
}
