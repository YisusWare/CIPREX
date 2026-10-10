namespace Game.Player
{
    public class ActionReadyState : ActionState
    {
        public ActionReadyState(PlayerContext ctx, ActionFSM fsm) : base(ctx, fsm) { }

        public override void Tick(float dt)
        {
            if (BlockedByDash()) return;

            // Si la cadencia todavía no lo permite, el buffer (0.05 s) guarda la pulsación.
            if (Input.Fire.IsActive && ctx.Buster.CanFire)
            {
                Input.Fire.Consume();
                if (fsm.TryFire()) fsm.ChangeState(fsm.Shooting);
            }
        }
    }
}
