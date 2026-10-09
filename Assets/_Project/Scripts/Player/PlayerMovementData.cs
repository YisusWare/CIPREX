using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Valores de ajuste del movimiento (secciones 5.2 y 6.3). Se pueden cambiar en Play Mode.
    /// 1 unidad = 1 tile = 16 px. Casi todo es [AJUSTAR].
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Player Movement Data", fileName = "PlayerMovementData")]
    public class PlayerMovementData : ScriptableObject
    {
        [Header("Correr")]
        [Tooltip("Unidades por segundo.")]
        public float runSpeed = 6f;
        [Tooltip("u/s² en el suelo. Un valor alto da el arranque casi instantáneo de Mega Man.")]
        public float groundAcceleration = 90f;
        [Tooltip("u/s² en el aire (control libre).")]
        public float airAcceleration = 90f;

        [Header("Salto")]
        [Tooltip("Altura máxima en unidades (tiles).")]
        public float jumpHeight = 3.5f;
        [Tooltip("Segundos hasta la cima con el botón presionado.")]
        public float timeToApex = 0.4f;
        public float fallGravityMultiplier = 1.5f;
        [Tooltip("Velocidad máxima de caída (u/s).")]
        public float maxFallSpeed = 18f;
        [Tooltip("Al soltar el salto mientras sube, la velocidad Y se multiplica por este valor.")]
        [Range(0f, 1f)] public float jumpCutMultiplier = 0.4f;
        public float coyoteTime = 0.1f;
        public float jumpBuffer = 0.1f;

        [Header("Dash")]
        public float dashSpeedMultiplier = 2.5f;
        public float dashDuration = 0.25f;
        public int airDashesPerJump = 1;
        public float dashBuffer = 0.1f;

        [Header("Collider (unidades, pivote en los pies)")]
        public float colliderWidth = 0.75f;     // 12 px
        public float standingHeight = 1.875f;   // 30 px
        public float crouchingHeight = 0.9375f; // 15 px

        // Valores derivados: se ajusta pensando en tiles, no en fuerzas.
        public float Gravity => 2f * jumpHeight / (timeToApex * timeToApex);
        public float JumpVelocity => 2f * jumpHeight / timeToApex;
        public float DashSpeed => runSpeed * dashSpeedMultiplier;
    }
}
