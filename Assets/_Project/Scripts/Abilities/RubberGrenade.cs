using System;
using System.Collections.Generic;
using Game.Audio;
using Game.Combat;
using UnityEngine;

namespace Game.Abilities
{
    /// <summary>
    /// Granada de goma (sección 8.2): la única habilidad con Rigidbody2D dinámico. Rebota en el suelo y
    /// en las paredes (PhysicsMaterial2D del collider) y explota al tocar una hurtbox enemiga o al
    /// acabarse la mecha. No daña al jugador. Sale del pool de RubberGrenadeEffect.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
    public class RubberGrenade : MonoBehaviour, IPoolable
    {
        [Tooltip("Giro de la granada (4 frames).")]
        [SerializeField] Sprite[] spinFrames;
        [SerializeField] float spinFps = 16f;
        [Tooltip("Multiplica la gravedad de Physics2D (-9.81). [AJUSTAR]")]
        [SerializeField] float gravityScale = 2f;
        [SerializeField] SfxData bounceSfx;
        [Tooltip("Velocidad mínima del golpe para que suene el rebote.")]
        [SerializeField] float bounceSfxMinSpeed = 1.5f;

        static readonly List<Collider2D> Overlaps = new();

        Rigidbody2D rb;
        CircleCollider2D circle;
        SpriteRenderer sr;
        Action release;
        Action<RubberGrenade> exploded;
        ContactFilter2D targetFilter;
        float fuse;
        float animTime;
        bool live;

        public void BindPool(Action releaseAction) => release = releaseAction;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            circle = GetComponent<CircleCollider2D>();
            sr = GetComponent<SpriteRenderer>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // el giro es de sprites, no de física
        }

        /// <summary>Lanza la granada. onExplode lo llama una vez, justo antes de volver al pool.</summary>
        public void Launch(Vector2 position, Vector2 velocity, float fuseTime, LayerMask targetMask,
            Action<RubberGrenade> onExplode)
        {
            transform.position = position;
            rb.position = position;
            rb.gravityScale = gravityScale;
            rb.linearVelocity = velocity;
            fuse = fuseTime;
            exploded = onExplode;
            targetFilter = new ContactFilter2D();
            targetFilter.SetLayerMask(targetMask);
            targetFilter.useTriggers = true;
            animTime = 0f;
            live = true;
            sr.flipX = velocity.x < 0f;
        }

        void FixedUpdate()
        {
            if (!live) return;

            fuse -= Time.fixedDeltaTime;
            if (fuse <= 0f) { Explode(); return; }

            // Explota al tocar una hurtbox enemiga (consulta, no OnTrigger: las hurtboxes son triggers).
            float radius = circle.radius * Mathf.Abs(transform.lossyScale.x);
            if (Physics2D.OverlapCircle(rb.position, radius, targetFilter, Overlaps) > 0) Explode();
        }

        void Update()
        {
            if (!live) return;
            animTime += Time.deltaTime;
            var frame = DirectionalFrames.Frame(spinFrames, animTime, spinFps, true);
            if (frame != null) sr.sprite = frame;
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f) sr.flipX = rb.linearVelocity.x < 0f;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (live && collision.relativeVelocity.magnitude >= bounceSfxMinSpeed)
                SfxPlayer.Play(bounceSfx, transform.position, cullOffscreen: true);
        }

        /// <summary>Explota ya (también si nace dentro de una pared).</summary>
        public void Explode()
        {
            if (!live) return;
            live = false;
            rb.linearVelocity = Vector2.zero;
            exploded?.Invoke(this);
            if (release != null) release();
            else Destroy(gameObject);
        }
    }
}
