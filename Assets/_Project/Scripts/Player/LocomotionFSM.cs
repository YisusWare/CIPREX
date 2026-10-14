using System;
using Game.Core;

namespace Game.Player
{
    /// <summary>Cómo se entra a Airborne.</summary>
    public enum AirborneEntry
    {
        /// <summary>Se perdió el suelo caminando: hay coyote time.</summary>
        Fall,
        /// <summary>Salto: aplica la velocidad inicial del salto.</summary>
        Jump,
        /// <summary>Terminó un air dash: velocidad Y en 0, sin coyote time.</summary>
        AfterAirDash,
        /// <summary>Se reanuda después de Hurt, Disabled o reaparecer: conserva la velocidad.</summary>
        Resume,
    }

    /// <summary>
    /// FSM de locomoción (sección 6.3): Idle, Run, Crouch, Dash, Airborne y AirDash.
    /// Guarda los datos que comparten los estados (dirección, air dashes, impulso del dash).
    /// </summary>
    public class LocomotionFSM
    {
        readonly PlayerContext ctx;
        readonly StateMachine machine = new();

        public readonly IdleState Idle;
        public readonly RunState Run;
        public readonly CrouchState Crouch;
        public readonly DashState Dash;
        public readonly AirborneState Airborne;
        public readonly AirDashState AirDash;
        /// <summary>Sin control: Hurt, Dead, Disabled (FSM de nivel superior, sección 6.2).</summary>
        public readonly SuspendedState Suspended;

        public LocomotionFSM(PlayerContext ctx)
        {
            this.ctx = ctx;
            Idle = new IdleState(ctx, this);
            Run = new RunState(ctx, this);
            Crouch = new CrouchState(ctx, this);
            Dash = new DashState(ctx, this);
            Airborne = new AirborneState(ctx, this);
            AirDash = new AirDashState(ctx, this);
            Suspended = new SuspendedState(ctx, this);
            machine.Changed += (prev, next) => StateChanged?.Invoke(prev, next);
        }

        // ---------- Propiedades públicas (sección 6.3) ----------

        public IState Current => machine.Current;
        public bool IsGrounded => ctx.Motor.IsGrounded;
        public bool IsCrouching => machine.Current == Crouch;
        public bool IsDashing => machine.Current == Dash || machine.Current == AirDash;
        public bool IsAirborne => machine.Current == Airborne || machine.Current == AirDash;
        public bool IsSuspended => machine.Current == Suspended;

        /// <summary>1 = derecha, -1 = izquierda.</summary>
        public int FacingDir { get; set; } = 1;

        /// <summary>Dash jump: mientras sea true, la velocidad horizontal máxima es la del dash.</summary>
        public bool CarriesDashMomentum { get; set; }
        /// <summary>Dirección del último dash (para saber si se suelta o cambia la dirección).</summary>
        public int DashDirection { get; set; } = 1;
        public int AirDashesLeft { get; set; }

        /// <summary>Lo lee AirborneState.Enter.</summary>
        public AirborneEntry PendingAirborneEntry { get; private set; }

        public event Action<IState, IState> StateChanged;
        /// <summary>Cada salto, incluido el de coyote time (para el SFX).</summary>
        public event Action Jumped;

        internal void RaiseJumped() => Jumped?.Invoke();

        public string CurrentName => machine.Current == null
            ? "-"
            : machine.Current.GetType().Name.Replace("State", "");

        // ---------- Ciclo ----------

        public void Start() => GoAirborne(AirborneEntry.Fall);

        public void Tick(float dt) => machine.Tick(dt);
        public void FixedTick(float dt) => machine.FixedTick(dt);

        public void ChangeState(IState next) => machine.ChangeState(next);

        // ---------- Transiciones compartidas ----------

        public void GoAirborne(AirborneEntry entry, bool carryDashMomentum = false)
        {
            PendingAirborneEntry = entry;
            CarriesDashMomentum = carryDashMomentum;
            machine.ChangeState(Airborne);
        }

        /// <summary>
        /// Rebote del tajo hacia abajo (pogo, 8.2): sube a la altura indicada, sin corte de salto, y
        /// recarga el air dash. Solo en el aire y fuera del air dash. Devuelve true si rebotó.
        /// </summary>
        public bool Bounce(float height)
        {
            if (height <= 0f || machine.Current != Airborne) return false;
            AirDashesLeft = ctx.Movement.airDashesPerJump;
            Airborne.Bounce(UnityEngine.Mathf.Sqrt(2f * ctx.Movement.Gravity * height));
            return true;
        }

        /// <summary>Idle o Run según el input horizontal.</summary>
        public void GoGrounded() => machine.ChangeState(ctx.Input.MoveX != 0 ? Run : Idle);

        /// <summary>Sale del estado actual (su Exit restaura el collider, etc.) y deja de responder al input.</summary>
        public void Suspend() => machine.ChangeState(Suspended);

        /// <summary>Vuelve a un estado normal según si hay suelo.</summary>
        public void Resume()
        {
            if (!IsSuspended) return;
            CarriesDashMomentum = false;
            if (ctx.Motor.IsGrounded) GoGrounded();
            else GoAirborne(AirborneEntry.Resume);
        }

        /// <summary>Lo llaman los estados de suelo al entrar: recarga el air dash y quita el impulso.</summary>
        public void OnGrounded()
        {
            AirDashesLeft = ctx.Movement.airDashesPerJump;
            CarriesDashMomentum = false;
        }
    }
}
