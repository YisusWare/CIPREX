using Game.Abilities;

namespace Game.Player
{
    /// <summary>Referencias compartidas por los estados del jugador (sección 6.1).</summary>
    public class PlayerContext
    {
        public InputSnapshot Input;
        public PlayerMovementData Movement;
        public PlayerMotor Motor;
        public PlayerBuster Buster;
        public PlayerHealth Health;
        public PlayerEnergy Energy;
        public PlayerAbilities Abilities;
        public LocomotionFSM Locomotion;
        public ActionFSM Action;
    }
}
