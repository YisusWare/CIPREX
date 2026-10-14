using Game.Abilities;
using Game.Combat;
using Game.Core;
using UnityEngine;

namespace Game.Player
{
    /// <summary>Estado de nivel superior (sección 6.2), para animación y depuración.</summary>
    public enum PlayerLifeState { Alive, Hurt, Dead, Respawning, Disabled }

    /// <summary>
    /// Recibe el CharacterData, crea el contexto y actualiza las máquinas de estados (secciones 5.2, 5.3, 5.4 y 6.2).
    /// No lee input ni mueve el Rigidbody directamente.
    /// Flujo: se instancia el prefab del personaje → Awake (cada componente toma sus referencias) →
    /// Initialize(characterData) reparte los datos → Start arranca las FSM.
    /// La FSM de nivel superior (Alive, Hurt, Dead, Respawning, Disabled) está en PlayerLifeStates.cs.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(PlayerInputReader), typeof(PlayerMotor))]
    [RequireComponent(typeof(PlayerBuster), typeof(PlayerHealth))]
    [RequireComponent(typeof(PlayerEnergy), typeof(PlayerAbilities))]
    public partial class PlayerController : MonoBehaviour
    {
        [Tooltip("Duración de la aparición (Teleport_In) al empezar y al reaparecer. Debe coincidir con el clip FullBody_Teleport_In.")]
        [SerializeField] float spawnDuration = 0.67f;

        PlayerInputReader inputReader;
        PlayerMotor motor;
        PlayerBuster buster;
        PlayerHealth health;
        PlayerEnergy energy;
        PlayerAbilities abilities;

        readonly StateMachine life = new();
        AliveState alive;
        HurtState hurt;
        DeadState dead;
        RespawningState respawning;
        DisabledState disabled;

        /// <summary>El personaje que se está jugando (5.4). Null hasta que se llama Initialize.</summary>
        public CharacterData Character { get; private set; }
        public bool IsInitialized => Character != null;

        public PlayerContext Context { get; private set; }
        public LocomotionFSM Locomotion => Context.Locomotion;
        public ActionFSM Action => Context.Action;
        public PlayerMotor Motor => motor;
        public PlayerBuster Buster => buster;
        public PlayerHealth Health => health;
        public PlayerEnergy Energy => energy;
        public PlayerAbilities Abilities => abilities;

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
            energy = GetComponent<PlayerEnergy>();
            abilities = GetComponent<PlayerAbilities>();

            alive = new AliveState(this);
            hurt = new HurtState(this);
            dead = new DeadState(this);
            respawning = new RespawningState(this);
            disabled = new DisabledState(this);
            life.Changed += (prev, next) => LifeStateChanged?.Invoke(prev, next);

            health.Damaged += OnDamaged;
            health.Died += OnDied;
        }

        /// <summary>
        /// Reparte los datos del personaje a los componentes (5.4). Ningún componente tiene estos
        /// valores en sus propios campos: todos salen de aquí. Se llama una vez, justo después de instanciar.
        /// </summary>
        public void Initialize(CharacterData character)
        {
            if (IsInitialized)
            {
                Debug.LogWarning("PlayerController: Initialize se llamó dos veces.", this);
                return;
            }
            if (character == null || character.movement == null || character.buster == null)
            {
                Debug.LogError("PlayerController: el CharacterData está vacío o le falta movement/buster.", this);
                enabled = false;
                return;
            }

            Character = character;
            motor.Configure(character.movement);
            inputReader.Configure(character.movement);
            buster.SetData(character.buster);
            health.SetMaxHealth(character.maxHealth);
            energy.SetMax(character.maxEnergy);
            abilities.SetAbilities(character.abilities, AbilityTestMode.Enabled ? character.testModeAbilities : null, energy);

            Context = new PlayerContext
            {
                Input = inputReader.Snapshot,
                Movement = character.movement,
                Motor = motor,
                Buster = buster,
                Health = health,
                Energy = energy,
                Abilities = abilities,
            };
            Context.Locomotion = new LocomotionFSM(Context);
            Context.Action = new ActionFSM(Context);
        }

        void Start()
        {
            if (!IsInitialized)
            {
                Debug.LogError("PlayerController: falta llamar a Initialize(CharacterData). " +
                               "El jugador no se coloca en la escena: lo instancia PlayerSpawner (5.4).", this);
                enabled = false;
                return;
            }

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
            // Orden de la sección 5.3: input → locomoción → acción (dentro de Alive) → animación.
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
            Action.Reset(); // también cancela la habilidad en curso (8.5)
            Context.Input.ClearBuffers();
        }

        void ResumeControl()
        {
            Context.Input.ClearBuffers();
            Locomotion.Resume();
        }
    }
}
