namespace Game.Player
{
    /// <summary>
    /// Shoot pose (sección 6.4): dura shootPoseDuration y se reinicia con cada disparo.
    /// Se cancela si empieza un dash.
    /// </summary>
    public class ActionShootingState : ActionState
    {
        public ActionShootingState(PlayerContext ctx, ActionFSM fsm) : base(ctx, fsm) { }

        public override void Exit() => fsm.ShootPoseTimer = 0f;

        public override void Tick(float dt)
        {
            if (BlockedByDash())
            {
                fsm.ChangeState(fsm.Ready);
                return;
            }

            if (Input.Fire.IsActive && ctx.Buster.CanFire)
            {
                Input.Fire.Consume();
                fsm.TryFire();
            }

            fsm.ShootPoseTimer -= dt;
            if (fsm.ShootPoseTimer <= 0f) fsm.ChangeState(fsm.Ready);
        }
    }
}
