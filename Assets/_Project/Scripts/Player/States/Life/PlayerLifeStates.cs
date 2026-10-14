using Game.Core;
using UnityEngine;

namespace Game.Player
{
    /// <summary>FSM de nivel superior del jugador (sección 6.2).</summary>
    public partial class PlayerController
    {
        abstract class LifeStateBase : IState
        {
            protected readonly PlayerController p;
            protected LifeStateBase(PlayerController owner) => p = owner;

            public virtual void Enter() { }
            public virtual void Tick(float dt) { }
            public virtual void FixedTick(float dt) { }
            public virtual void Exit() { }
        }

        /// <summary>Control normal: corren las FSM de locomoción y acción.</summary>
        class AliveState : LifeStateBase
        {
            public AliveState(PlayerController owner) : base(owner) { }

            public override void Tick(float dt)
            {
                p.Locomotion.Tick(dt);
                p.Action.Tick(dt);
            }

            public override void FixedTick(float dt) => p.Locomotion.FixedTick(dt);
        }

        /// <summary>Knockback hacia atrás. Los i-frames siguen corriendo en PlayerHealth al volver a Alive.</summary>
        class HurtState : LifeStateBase
        {
            float timer;
            int direction;

            public HurtState(PlayerController owner) : base(owner) { }

            public override void Enter()
            {
                p.SuspendControl();
                timer = p.health.KnockbackDuration;
                direction = -p.Locomotion.FacingDir;
                var v = p.motor.Velocity;
                p.motor.Velocity = new Vector2(direction * p.health.KnockbackSpeed, Mathf.Min(v.y, 0f));
            }

            public override void Tick(float dt)
            {
                timer -= dt;
                if (timer <= 0f) p.life.ChangeState(p.alive);
            }

            public override void FixedTick(float dt)
            {
                p.motor.SetVelocityX(direction * p.health.KnockbackSpeed);
                float multiplier = p.motor.Velocity.y < 0f ? p.motor.Movement.fallGravityMultiplier : 1f;
                p.motor.ApplyGravity(dt, multiplier);
            }

            public override void Exit() => p.ResumeControl();
        }

        /// <summary>Se congela y, tras un momento, pasa a Respawning.</summary>
        class DeadState : LifeStateBase
        {
            float timer;

            public DeadState(PlayerController owner) : base(owner) { }

            public override void Enter()
            {
                p.SuspendControl();
                p.motor.Velocity = Vector2.zero;
                p.motor.Body.simulated = false;
                timer = p.health.RespawnDelay;
            }

            public override void Tick(float dt)
            {
                timer -= dt;
                if (timer <= 0f) p.life.ChangeState(p.respawning);
            }

            public override void Exit() => p.motor.Body.simulated = true;
        }

        /// <summary>
        /// Aparece en RespawnPoint con la vida llena mientras se ve Teleport_In
        /// (al empezar el nivel y al reaparecer). En el Hito 4 también se rellena la energía.
        /// </summary>
        class RespawningState : LifeStateBase
        {
            float timer;

            public RespawningState(PlayerController owner) : base(owner) { }

            public override void Enter()
            {
                p.SuspendControl();
                p.motor.Teleport(p.RespawnPoint);
                p.health.Refill();
                timer = p.spawnDuration;
            }

            public override void Tick(float dt)
            {
                timer -= dt;
                if (timer <= 0f) p.life.ChangeState(p.alive);
            }

            public override void FixedTick(float dt) => p.motor.Velocity = Vector2.zero;

            public override void Exit() => p.ResumeControl();
        }

        /// <summary>Sin control (cinemáticas, diálogos). Solo cae con la gravedad.</summary>
        class DisabledState : LifeStateBase
        {
            public DisabledState(PlayerController owner) : base(owner) { }

            public override void Enter()
            {
                p.SuspendControl();
                p.motor.SetVelocityX(0f);
            }

            public override void FixedTick(float dt)
            {
                p.motor.SetVelocityX(0f);
                p.motor.ApplyGravity(dt);
            }

            public override void Exit() => p.ResumeControl();
        }
    }
}
