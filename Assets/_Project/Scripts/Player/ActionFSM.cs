using System;
using Game.Core;

namespace Game.Player
{
    /// <summary>
    /// FSM de acción (sección 6.4): Ready y Shooting. UsingAbility llega en el Hito 4.
    /// Calcula AimDir en cada frame, se dispare o no (sección 6.5).
    /// </summary>
    public class ActionFSM
    {
        readonly PlayerContext ctx;
        readonly StateMachine machine = new();

        public readonly ActionReadyState Ready;
        public readonly ActionShootingState Shooting;

        public ActionFSM(PlayerContext ctx)
        {
            this.ctx = ctx;
            Ready = new ActionReadyState(ctx, this);
            Shooting = new ActionShootingState(ctx, this);
            machine.Changed += (prev, next) => StateChanged?.Invoke(prev, next);
        }

        public IState Current => machine.Current;
        public AimDirection AimDir { get; private set; }
        public bool IsInShootPose => machine.Current == Shooting;
        /// <summary>Lo toma de la habilidad activa en el Hito 4; por ahora siempre 1.</summary>
        public float MoveSpeedMultiplier => 1f;
        /// <summary>Tiempo restante de la shoot pose.</summary>
        public float ShootPoseTimer { get; set; }

        public event Action<IState, IState> StateChanged;

        public string CurrentName => machine.Current == null
            ? "-"
            : machine.Current.GetType().Name.Replace("Action", "").Replace("State", "");

        public void Start() => machine.ChangeState(Ready);

        public void Tick(float dt)
        {
            AimDir = ComputeAim();
            machine.Tick(dt);
        }

        public void ChangeState(IState next) => machine.ChangeState(next);

        /// <summary>Cancela la shoot pose (daño, muerte, cinemáticas).</summary>
        public void Reset()
        {
            ShootPoseTimer = 0f;
            machine.ChangeState(Ready);
        }

        /// <summary>Dispara si la cadencia lo permite y reinicia la shoot pose.</summary>
        public bool TryFire()
        {
            if (!ctx.Buster.CanFire) return false;
            var loco = ctx.Locomotion;
            ctx.Buster.Fire(AimDir, loco.FacingDir, loco.IsCrouching);
            ShootPoseTimer = ctx.Buster.Data.shootPoseDuration;
            return true;
        }

        AimDirection ComputeAim()
        {
            var input = ctx.Input;
            var loco = ctx.Locomotion;

            if (loco.IsCrouching) return AimDirection.Forward;           // sale de FirePoint.Crouch
            if (loco.IsAirborne && input.MoveY < 0) return AimDirection.Down;
            if (input.MoveY > 0) return input.MoveX != 0 ? AimDirection.DiagUp : AimDirection.Up;
            return AimDirection.Forward;
        }
    }
}
