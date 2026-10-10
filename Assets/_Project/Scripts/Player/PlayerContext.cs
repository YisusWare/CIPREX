namespace Game.Player
{
    /// <summary>
    /// Referencias compartidas por los estados del jugador (sección 6.1).
    /// Abilities, Energy y Anim llegan en los hitos 3 y 4.
    /// </summary>
    public class PlayerContext
    {
        public InputSnapshot Input;
        public PlayerMovementData Movement;
        public PlayerMotor Motor;
        public PlayerBuster Buster;
        public PlayerHealth Health;
        public LocomotionFSM Locomotion;
        public ActionFSM Action;
    }
}
