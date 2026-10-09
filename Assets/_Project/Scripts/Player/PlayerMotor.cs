using System.Collections.Generic;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Ejecuta el movimiento (sección 5.2): mueve, salta, hace dash, detecta suelo y techo
    /// y cambia el collider al agacharse. No decide CUÁNDO hacer nada: eso lo hacen los estados.
    /// La velocidad se asigna directamente y la gravedad la aplica el código (sección 11.2).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerMotor : MonoBehaviour
    {
        [SerializeField] PlayerMovementData movement;

        [Tooltip("Capas sobre las que se puede estar parado.")]
        [SerializeField] LayerMask groundMask;
        [Tooltip("Capas que impiden levantarse al estar agachado (sin plataformas atravesables).")]
        [SerializeField] LayerMask ceilingMask;

        Rigidbody2D rb;
        BoxCollider2D box;
        ContactFilter2D groundFilter;
        readonly List<Collider2D> overlapResults = new();
        readonly List<RaycastHit2D> groundHits = new();
        const float GroundProbeDistance = 0.05f; // menos de 1 px (0.0625)

        public Rigidbody2D Body => rb;
        public PlayerMovementData Movement => movement;
        public bool IsGrounded { get; private set; }
        public bool IsCrouchCollider { get; private set; }

        public Vector2 Velocity
        {
            get => rb.linearVelocity;
            set => rb.linearVelocity = value;
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            box = GetComponent<BoxCollider2D>();

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Solo cuentan las superficies con normal hacia arriba (suelo, no paredes ni techo).
            groundFilter = new ContactFilter2D();
            groundFilter.SetLayerMask(groundMask);
            groundFilter.SetNormalAngle(45f, 135f);

            SetCrouching(false);
        }

        /// <summary>Lo llama PlayerController al inicio de cada FixedUpdate.</summary>
        public void UpdateContacts()
        {
            // Se proyecta el collider unos píxeles hacia abajo en vez de leer los contactos:
            // al cambiar el tamaño del collider (agacharse) Unity borra sus contactos hasta el
            // siguiente paso de física, y el jugador "perdía el suelo" por un frame.
            // Rigidbody2D.Cast ignora los colliders que ya se traslapan al inicio, así que
            // al subir a través de una plataforma atravesable no la cuenta como suelo.
            if (rb.linearVelocity.y > 0.01f)
            {
                IsGrounded = false;
                return;
            }
            IsGrounded = rb.Cast(Vector2.down, groundFilter, groundHits, GroundProbeDistance) > 0;
        }

        // ---------- Movimiento ----------

        public void SetVelocityX(float x) => rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
        public void SetVelocityY(float y) => rb.linearVelocity = new Vector2(rb.linearVelocity.x, y);

        /// <summary>Acerca la velocidad X al objetivo con una aceleración en u/s².</summary>
        public void MoveTowardsX(float target, float acceleration, float dt)
        {
            Vector2 v = rb.linearVelocity;
            v.x = Mathf.MoveTowards(v.x, target, acceleration * dt);
            rb.linearVelocity = v;
        }

        /// <summary>Aplica la gravedad calculada desde la altura de salto.</summary>
        public void ApplyGravity(float dt, float multiplier = 1f)
        {
            Vector2 v = rb.linearVelocity;
            v.y -= movement.Gravity * multiplier * dt;
            if (v.y < -movement.maxFallSpeed) v.y = -movement.maxFallSpeed;
            rb.linearVelocity = v;
        }

        // ---------- Collider ----------

        public void SetCrouching(bool crouching)
        {
            IsCrouchCollider = crouching;
            float h = crouching ? movement.crouchingHeight : movement.standingHeight;
            box.size = new Vector2(movement.colliderWidth, h);
            box.offset = new Vector2(0f, h * 0.5f); // pivote en los pies
        }

        /// <summary>True si hay espacio para pasar del collider agachado al de pie.</summary>
        public bool CanStandUp()
        {
            float bottom = movement.crouchingHeight;
            float top = movement.standingHeight;
            Vector2 center = (Vector2)transform.position + new Vector2(0f, (bottom + top) * 0.5f);
            // Un poco más angosto para no detectar paredes que solo tocan los lados.
            Vector2 size = new Vector2(movement.colliderWidth - 0.05f, top - bottom - 0.02f);

            var filter = new ContactFilter2D();
            filter.SetLayerMask(ceilingMask);
            filter.useTriggers = false;
            return Physics2D.OverlapBox(center, size, 0f, filter, overlapResults) == 0;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (movement == null) return;
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Vector3 p = transform.position;
            Gizmos.DrawLine(p + Vector3.left * 0.4f, p + Vector3.right * 0.4f);
        }
#endif
    }
}
