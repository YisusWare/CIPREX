using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// En el aire (sección 6.3): salto con altura variable, coyote time, control libre,
    /// impulso del dash jump y air dash.
    /// </summary>
    public class AirborneState : LocomotionState
    {
        float coyoteTimer;
        bool isJumping;   // subiendo por un salto (no por una caída)
        bool jumpCut;     // ya se aplicó el corte de altura variable

        public AirborneState(PlayerContext ctx, LocomotionFSM fsm) : base(ctx, fsm) { }

        public override void Enter()
        {
            coyoteTimer = 0f;
            isJumping = false;
            jumpCut = false;

            switch (fsm.PendingAirborneEntry)
            {
                case AirborneEntry.Jump:
                    DoJump();
                    break;
                case AirborneEntry.Fall:
                    coyoteTimer = Data.coyoteTime;
                    break;
                case AirborneEntry.AfterAirDash:
                    Motor.SetVelocityY(0f);
                    break;

            }
        }

        void DoJump()
        {
            Motor.SetVelocityY(Data.JumpVelocity);
            isJumping = true;
            jumpCut = false;
            coyoteTimer = 0f;
            fsm.RaiseJumped();
        }

        /// <summary>Rebote del tajo hacia abajo (8.2): altura fija, sin corte de salto ni coyote time.</summary>
        public void Bounce(float velocityY)
        {
            Motor.SetVelocityY(velocityY);
            isJumping = false;
            jumpCut = true;
            coyoteTimer = 0f;
        }

        public override void Tick(float dt)
        {
            if (coyoteTimer > 0f) coyoteTimer -= dt;

            // El impulso del dash jump se pierde al soltar o cambiar de dirección.
            if (fsm.CarriesDashMomentum && Input.MoveX != fsm.DashDirection)
                fsm.CarriesDashMomentum = false;

            UpdateFacing();

            // Coyote time: saltar poco después de salir de una orilla.
            if (coyoteTimer > 0f && Input.Jump.Consume())
                DoJump();

            // Altura variable: soltar el botón mientras sube corta el salto.
            float vy = Motor.Velocity.y;
            if (isJumping && !jumpCut && !Input.JumpHeld && vy > 0f)
            {
                Motor.SetVelocityY(vy * Data.jumpCutMultiplier);
                jumpCut = true;
            }
            if (vy <= 0f) isJumping = false;

            // Air dash: solo se consume el buffer si queda uno disponible.
            if (fsm.AirDashesLeft > 0 && !DashBlocked && Input.Dash.Consume())
            {
                fsm.ChangeState(fsm.AirDash);
                return;
            }

            // Aterrizar.
            if (Motor.IsGrounded && Motor.Velocity.y <= 0.01f)
            {
                fsm.GoGrounded();
                return;
            }
        }

        public override void FixedTick(float dt)
        {
            float maxSpeed = (fsm.CarriesDashMomentum ? Data.DashSpeed : Data.runSpeed) * SpeedMultiplier;
            Motor.MoveTowardsX(Input.MoveX * maxSpeed, Data.airAcceleration, dt);

            float multiplier = Motor.Velocity.y < 0f ? Data.fallGravityMultiplier : 1f;
            Motor.ApplyGravity(dt, multiplier);
        }
    }
}
