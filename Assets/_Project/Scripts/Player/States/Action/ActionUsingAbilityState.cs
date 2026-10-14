using Game.Abilities;
using Game.Audio;

namespace Game.Player
{
    /// <summary>
    /// Estado UsingAbility (sección 8.5). Es el mismo para todas las habilidades:
    /// 1. Enter: cobra el costo (Press), inicia el cooldown, llama a Begin y pone el torso según torsoPose.
    /// 2. Tick: descuenta energía por segundo (Hold), llama a Sustain y avisa una vez cuando se suelta el botón.
    /// 3. Sale cuando el efecto termina (IsFinished), se acaba la energía, llega daño o (Hold) empieza un dash.
    ///    Siempre llama a End.
    /// </summary>
    public class ActionUsingAbilityState : ActionState
    {
        AbilityData ability;
        AbilityEffect effect;
        bool releaseSent;
        SfxHandle loopHandle;

        public ActionUsingAbilityState(PlayerContext ctx, ActionFSM fsm) : base(ctx, fsm) { }

        public override void Enter()
        {
            ability = ctx.Abilities.Selected;
            effect = ctx.Abilities.EffectFor(ability);
            releaseSent = false;
            fsm.ActiveAbility = ability;

            if (ability.activation == AbilityActivation.Press)
                ctx.Energy.TrySpend(ability.energyCost);
            ctx.Abilities.StartCooldown(ability);

            fsm.ForceForwardPose = ability.torsoPose == AbilityTorsoPose.ShootPoseForward;

            var abilityCtx = fsm.BuildAbilityContext(ability);
            fsm.AbilityAim = abilityCtx.aim;
            fsm.AbilityCrouching = abilityCtx.crouching;
            effect.Begin(abilityCtx);
            SfxPlayer.Play(ability.activateSfx, abilityCtx.origin);
            if (ability.activation == AbilityActivation.Hold && ability.loopSfx != null)
                loopHandle = SfxPlayer.Play(ability.loopSfx, abilityCtx.origin);

            fsm.RaiseAbilityStarted(ability);
        }

        public override void Tick(float dt)
        {
            bool hold = ability.activation == AbilityActivation.Hold;

            // El dash cancela las sostenidas (8.5). Para volver a usarla hay que presionar de nuevo.
            if (hold && ctx.Locomotion.IsDashing)
            {
                Input.Ability.Clear();
                fsm.ChangeState(fsm.Ready);
                return;
            }

            if (hold)
            {
                ctx.Energy.Drain(ability.energyPerSecond * dt);
                if (ctx.Energy.IsEmpty)
                {
                    fsm.ChangeState(fsm.Ready);
                    return;
                }
            }

            effect.Sustain(fsm.BuildAbilityContext(ability), dt);

            if (!releaseSent && !Input.AbilityHeld)
            {
                releaseSent = true;
                effect.ButtonReleased();
            }

            if (effect.IsFinished) Finish();
        }

        void Finish()
        {
            // Las habilidades con pose de disparo dejan la shoot pose unos instantes, como un disparo normal.
            if (ability.torsoPose != AbilityTorsoPose.CustomClip)
            {
                bool forceForward = fsm.ForceForwardPose;
                fsm.ShootPoseTimer = ctx.Buster.Data.shootPoseDuration;
                fsm.ChangeState(fsm.Shooting);
                fsm.ForceForwardPose = forceForward;
            }
            else
            {
                fsm.ChangeState(fsm.Ready);
            }
        }

        public override void Exit()
        {
            effect.End();
            loopHandle.Stop();
            loopHandle = default;
            fsm.ActiveAbility = null;
        }
    }
}
