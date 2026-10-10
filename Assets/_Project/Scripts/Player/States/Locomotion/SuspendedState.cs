namespace Game.Player
{
    /// <summary>
    /// Locomoción en pausa mientras la FSM de nivel superior está en Hurt, Dead, Respawning
    /// o Disabled. Ese estado se encarga de la física; aquí no se hace nada.
    /// </summary>
    public class SuspendedState : LocomotionState
    {
        public SuspendedState(PlayerContext ctx, LocomotionFSM fsm) : base(ctx, fsm) { }
    }
}
