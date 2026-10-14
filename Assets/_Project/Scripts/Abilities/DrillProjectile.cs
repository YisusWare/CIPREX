using Game.Combat;
using UnityEngine;

namespace Game.Abilities
{
    /// <summary>
    /// Proyectil del taladro (sección 8.2): atraviesa a todos los enemigos (pierceCount = -1 en el
    /// prefab), se detiene en la pared y, si la pared es un obstáculo con Break, lo rompe (15.2).
    /// Elige sus sprites por dirección en lugar de rotarlos.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class DrillProjectile : Projectile
    {
        [SerializeField] DirectionalFrames frames = new();
        [Tooltip("Velocidad del giro (frames por segundo).")]
        [SerializeField] float fps = 16f;

        SpriteRenderer sr;
        Sprite[] current;
        float animTime;

        /// <summary>La asigna DrillEffect: la necesita el obstáculo para saber si acepta Break.</summary>
        public AbilityData Ability { get; set; }

        protected override void Awake()
        {
            base.Awake();
            sr = GetComponent<SpriteRenderer>();
        }

        protected override void OnLaunched()
        {
            current = frames.For(Velocity, out bool flipX, out bool flipY);
            sr.flipX = flipX;
            sr.flipY = flipY;
            animTime = 0f;
            var first = DirectionalFrames.Frame(current, 0f, fps, true);
            if (first != null) sr.sprite = first;
        }

        void LateUpdate()
        {
            animTime += Time.deltaTime;
            var frame = DirectionalFrames.Frame(current, animTime, fps, true);
            if (frame != null) sr.sprite = frame;
        }

        protected override void OnHitSolid(RaycastHit2D hit)
        {
            AbilityObstacles.TryInteract(hit.collider, hit.point, Velocity.normalized, Ability, 0f);
        }
    }
}
