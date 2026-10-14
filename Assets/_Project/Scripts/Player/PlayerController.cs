using Game.Combat;
using Game.Core;
using UnityEngine;

namespace Game.Player
{
    /// <summary>Estado de nivel superior (sección 6.2), para animación y depuración.</summary>
    public enum PlayerLifeState { Alive, Hurt, Dead, Respawning, Disabled }

    /// <summary>
    /// Crea el contexto y actualiza las máquinas de estados (secciones 5.2, 5.3 y 6.2).
    /// No lee input ni mueve el Rigidbody directamente.
    /// La FSM de nivel superior (Alive, Hurt, Dead, Respawning, Disabled) está en PlayerLifeStates.cs.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(PlayerInputReader), typeof(PlayerMotor))]
    [RequireComponent(typeof(PlayerBuster), typeof(PlayerHealth))]
    public partial class PlayerController : MonoBehaviour
    {
        [Tooltip("Duración de la aparición (Teleport_In) al empezar y al reaparecer. Debe coincidir con el clip FullBody_Teleport_In.")]
        [SerializeField] float spawnDuration = 0.67f;

        PlayerInputReader inputReader;
        PlayerMotor motor;
        PlayerBuster buster;
        PlayerHealth health;

        readonly StateMachine life = new();
        AliveState alive;
        HurtState hurt;
        DeadState dead;
        RespawningState respawning;
        DisabledState disabled;

        public PlayerContext Context { get; private set; }
        public LocomotionFSM Locomotion => Context.Locomotion;
        public ActionFSM Action => Context.Action;
        public PlayerMotor Motor => motor;
        public PlayerBuster Buster => buster;
        public PlayerHealth Health => health;

        /// <summary>Dónde reaparece al morir. En el Hito 5 lo actualizan los checkpoints.</summary>
        public Vector3 RespawnPoint { get; set; }

        public bool IsAlive => life.Current == alive;
        public bool IsHurt => life.Current == hurt;

        public PlayerLifeState LifeState =>
            life.Current == hurt ? PlayerLifeState.Hurt :
            life.Current == dead ? PlayerLifeState.Dead :
            life.Current == respawning ? PlayerLifeState.Respawning :
            life.Current == disabled ? PlayerLifeState.Disabled :
            PlayerLifeState.Alive;
        public string LifeStateName => life.Current == null ? "-" : life.Current.GetType().Name.Replace("State", "");

        public event System.Action<IState, IState> LifeStateChanged;

        void Awake()
        {
            inputReader = GetComponent<PlayerInputReader>();
            motor = GetComponent<PlayerMotor>();
            buster = GetComponent<PlayerBuster>();
            health = GetComponent<PlayerHealth>();

            Context = new PlayerContext
            {
                Input = inputReader.Snapshot,
                Movement = motor.Movement,
                Motor = motor,
                Buster = buster,
                Health = health,
            };
            Context.Locomotion = new LocomotionFSM(Context);
            Context.Action = new ActionFSM(Context);

            alive = new AliveState(this);
            hurt = new HurtState(this);
            dead = new DeadState(this);
            respawning = new RespawningState(this);
            disabled = new DisabledState(this);
            life.Changed += (prev, next) => LifeStateChanged?.Invoke(prev, next);

            health.Damaged += OnDamaged;
            health.Died += OnDied;
        }

        void Start()
        {
            RespawnPoint = transform.position;
            Locomotion.Start();
            Action.Start();
            // Aparece con Teleport_In, igual que al reaparecer.
            Locomotion.Suspend();
            life.ChangeState(respawning);
        }

        void OnDestroy()
        {
            if (health == null) return;
            health.Damaged -= OnDamaged;
            health.Died -= OnDied;
        }

        void Update()
        {
            // Orden de la sección 5.3: input → locomoción → acción (dentro de Alive) → animación (Hito 3).
            inputReader.Poll();
            life.Tick(Time.deltaTime);
        }

        void FixedUpdate()
        {
            motor.UpdateContacts();
            life.FixedTick(Time.fixedDeltaTime);
        }

        /// <summary>Cinemáticas, diálogos, jefe derrotado.</summary>
        public void SetDisabled(bool value)
        {
            if (value)
            {
                if (life.Current == alive || life.Current == hurt) life.ChangeState(disabled);
            }
            else if (life.Current == disabled)
            {
                life.ChangeState(alive);
            }
        }

        void OnDamaged(DamageInfo info)
        {
            if (life.Current == alive) life.ChangeState(hurt);
        }

        void OnDied() => life.ChangeState(dead);

        /// <summary>Quita el control: sale de los estados de locomoción y acción y limpia buffers.</summary>
        void SuspendControl()
        {
            Locomotion.Suspend();
            Action.Reset();
            Context.Input.ClearBuffers();
        }

        void ResumeControl()
        {
            Context.Input.ClearBuffers();
            Locomotion.Resume();
        }
    }
}
