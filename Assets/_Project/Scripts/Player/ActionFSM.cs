using System;
using Game.Abilities;
using Game.Core;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// FSM de acción (sección 6.4): Ready, Shooting y UsingAbility.
    /// Calcula AimDir en cada frame, se dispare o no (sección 6.5), y procesa el cambio de habilidad.
    /// </summary>
    public class ActionFSM
    {
        readonly PlayerContext ctx;
        readonly StateMachine machine = new();

        public readonly ActionReadyState Ready;
        public readonly ActionShootingState Shooting;
        public readonly ActionUsingAbilityState UsingAbility;

        public ActionFSM(PlayerContext ctx)
        {
            this.ctx = ctx;
            Ready = new ActionReadyState(ctx, this);
            Shooting = new ActionShootingState(ctx, this);
            UsingAbility = new ActionUsingAbilityState(ctx, this);
            machine.Changed += (prev, next) => StateChanged?.Invoke(prev, next);
        }

        public IState Current => machine.Current;
        public AimDirection AimDir { get; private set; }

        /// <summary>Tiempo restante de la shoot pose.</summary>
        public float ShootPoseTimer { get; set; }

        /// <summary>La habilidad en uso (solo dentro de UsingAbility).</summary>
        public AbilityData ActiveAbility { get; internal set; }

        /// <summary>Dirección fijada al activar la habilidad (el clip del tajo no cambia a mitad de camino).</summary>
        public AimDirection AbilityAim { get; internal set; }
        /// <summary>Si estaba agachada al activar la habilidad.</summary>
        public bool AbilityCrouching { get; internal set; }

        /// <summary>
        /// La shoot pose debe mirar hacia adelante aunque se mantenga arriba (granada, torsoPose = ShootPoseForward).
        /// Sigue activa durante la shoot pose que deja la habilidad al terminar.
        /// </summary>
        public bool ForceForwardPose { get; internal set; }

        /// <summary>Shooting, o una habilidad que usa la pose de disparo (todas menos las de clip propio).</summary>
        public bool IsInShootPose =>
            machine.Current == Shooting ||
            (machine.Current == UsingAbility && ActiveAbility != null && ActiveAbility.torsoPose != AbilityTorsoPose.CustomClip);

        /// <summary>moveSpeedMultiplier de la habilidad activa; 1 si no hay ninguna (sección 6.4).</summary>
        public float MoveSpeedMultiplier =>
            machine.Current == UsingAbility && ActiveAbility != null ? ActiveAbility.moveSpeedMultiplier : 1f;

        /// <summary>No se puede hacer dash durante una habilidad por pulsación (8.5). Las sostenidas se cancelan con el dash.</summary>
        public bool BlocksDash =>
            machine.Current == UsingAbility && ActiveAbility != null && ActiveAbility.activation == AbilityActivation.Press;

        public event Action<IState, IState> StateChanged;
        /// <summary>Al activar una habilidad (para reiniciar el clip del torso).</summary>
        public event Action<AbilityData> AbilityStarted;

        public string CurrentName => machine.Current == null
            ? "-"
            : machine.Current.GetType().Name.Replace("Action", "").Replace("State", "");

        public void Start() => machine.ChangeState(Ready);

        public void Tick(float dt)
        {
            AimDir = ComputeAim();

            // Cambio de habilidad en cualquier estado menos UsingAbility, también durante el dash (6.4).
            if (machine.Current != UsingAbility && ctx.Abilities != null)
            {
                if (ctx.Input.AbilityNextPressed) ctx.Abilities.SelectNext();
                else if (ctx.Input.AbilityPrevPressed) ctx.Abilities.SelectPrevious();
            }

            machine.Tick(dt);
        }

        public void ChangeState(IState next) => machine.ChangeState(next);

        /// <summary>Cancela la shoot pose y la habilidad en curso (daño, muerte, cinemáticas).</summary>
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
            ForceForwardPose = false;
            return true;
        }

        /// <summary>Contexto del efecto: origen y dirección según el apuntado de la habilidad (8.4).</summary>
        public AbilityContext BuildAbilityContext(AbilityData ability)
        {
            var loco = ctx.Locomotion;
            AimDirection aim = AimDir;
            if (ability == null || !ability.usesAim || ability.torsoPose == AbilityTorsoPose.ShootPoseForward)
                aim = AimDirection.Forward;

            return new AbilityContext
            {
                owner = ctx.Motor.GetComponent<PlayerController>(),
                origin = ctx.Buster.FirePosition(aim, loco.FacingDir, loco.IsCrouching),
                direction = PlayerBuster.DirectionFor(aim, loco.FacingDir),
                aim = aim,
                facing = loco.FacingDir,
                crouching = loco.IsCrouching,
            };
        }

        internal void RaiseAbilityStarted(AbilityData ability) => AbilityStarted?.Invoke(ability);

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
