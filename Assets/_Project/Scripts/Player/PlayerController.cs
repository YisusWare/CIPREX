using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Crea el contexto y actualiza las máquinas de estados (secciones 5.2 y 5.3).
    /// No lee input ni mueve el Rigidbody directamente.
    /// La FSM de nivel superior (Alive, Hurt, Dead...) y la FSM de acción llegan en el Hito 2.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(PlayerInputReader), typeof(PlayerMotor))]
    public class PlayerController : MonoBehaviour
    {
        PlayerInputReader inputReader;
        PlayerMotor motor;

        public PlayerContext Context { get; private set; }
        public LocomotionFSM Locomotion => Context.Locomotion;
        public PlayerMotor Motor => motor;

        void Awake()
        {
            inputReader = GetComponent<PlayerInputReader>();
            motor = GetComponent<PlayerMotor>();

            Context = new PlayerContext
            {
                Input = inputReader.Snapshot,
                Movement = motor.Movement,
                Motor = motor,
            };
            Context.Locomotion = new LocomotionFSM(Context);
        }

        void Start() => Locomotion.Start();

        void Update()
        {
            // Orden de la sección 5.3.
            inputReader.Poll();
            Locomotion.Tick(Time.deltaTime);
            // ActionFSM.Tick y PlayerAnimation.Sync llegan en los hitos 2 y 3.
        }

        void FixedUpdate()
        {
            motor.UpdateContacts();
            Locomotion.FixedTick(Time.fixedDeltaTime);
        }
    }
}
