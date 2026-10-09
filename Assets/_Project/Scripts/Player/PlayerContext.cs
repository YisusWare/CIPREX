namespace Game.Player
{
    /// <summary>
    /// Referencias compartidas por los estados del jugador (sección 6.1).
    /// En hitos posteriores se agregan Buster, Abilities, Energy, Anim y Action.
    /// </summary>
    public class PlayerContext
    {
        public InputSnapshot Input;
        public PlayerMovementData Movement;
        public PlayerMotor Motor;
        public LocomotionFSM Locomotion;
    }
}
