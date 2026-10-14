using Game.Combat;
using UnityEngine;

namespace Game.Abilities
{
    /// <summary>
    /// Lanzallamas de Rosslyn (sección 8.2). Sostenido; sigue la dirección de apuntado en tiempo real.
    /// - Daño: una zona fija (DamageHitbox por tick) girada hacia el apuntado, de largo igual al alcance
    ///   recortado por la pared. Crece al ritmo de las llamas al empezar.
    /// - Visual: un chorro de FlamePuff que viajan en el mundo y se disipan; al soltar, las que ya
    ///   salieron terminan solas.
    /// - Obstáculos: si la llama llega a una pared que es obstáculo, le pasa la interacción con dt
    ///   (calor acumulado del acero, 15.2).
    /// </summary>
    public class FlamethrowerEffect : HeldAbilityEffect
    {
        [Header("Daño [AJUSTAR]")]
        [Tooltip("Hijo con la zona de daño (modo Tick). Su tamaño lo pone este efecto.")]
        [SerializeField] DamageHitbox damageZone;
        [SerializeField] int damagePerTick = 1;
        [SerializeField] float range = 4f;
        [SerializeField] float width = 0.75f;
        [Tooltip("Lo que recorta la llama (Ground, AbilityObstacle).")]
        [SerializeField] LayerMask solidMask;

        [Header("Llamas (visual) [AJUSTAR]")]
        [SerializeField] FlamePuff puffPrefab;
        [Tooltip("Frames de la llama: primero los de fuego (de pequeña a grande) y al final los de humo.")]
        [SerializeField] Sprite[] puffFrames;
        [Tooltip("Variante opcional con la misma cantidad de frames, para que no se vea repetida.")]
        [SerializeField] Sprite[] puffFramesVariant;
        [SerializeField, Min(0)] int smokeFrames = 2;
        [SerializeField] float smokeDuration = 0.12f;
        [SerializeField] float spawnInterval = 0.03f;
        [SerializeField] float puffSpeed = 9f;
        [Tooltip("Dispersión al azar, en grados (±).")]
        [SerializeField] float spreadDegrees = 6f;
        [Tooltip("Variación al azar de la velocidad (±, fracción).")]
        [SerializeField, Range(0f, 0.5f)] float speedVariation = 0.1f;

        ComponentPool<FlamePuff> puffs;
        float spawnTimer;
        float elapsed;

        protected override void OnBegin(AbilityContext ctx)
        {
            if (puffPrefab != null) puffs ??= new ComponentPool<FlamePuff>(puffPrefab, "FlamePuffs", 24, 48);
            spawnTimer = 0f;
            elapsed = 0f;
            if (damageZone != null)
            {
                damageZone.Size = new Vector2(0f, width);
                damageZone.Activate(Owner != null ? Owner.gameObject : null, damagePerTick);
            }
            Sustain(ctx, 0f);
        }

        public override void Sustain(AbilityContext ctx, float dt)
        {
            elapsed += dt;

            // Hasta dónde llega: la pared, o el alcance.
            float wallDistance = range;
            var wall = Physics2D.Raycast(ctx.origin, ctx.direction, range, solidMask);
            if (wall.collider != null) wallDistance = wall.distance;

            // La zona de daño crece al ritmo de las llamas al empezar.
            float reach = Mathf.Min(wallDistance, puffSpeed * elapsed);
            if (damageZone != null)
            {
                float angle = Mathf.Atan2(ctx.direction.y, ctx.direction.x) * Mathf.Rad2Deg;
                damageZone.transform.SetPositionAndRotation(ctx.origin + ctx.direction * (reach * 0.5f),
                    Quaternion.Euler(0f, 0f, angle));
                damageZone.Size = new Vector2(reach, width);
            }

            // Acero: solo cuando la llama ya llegó a la pared.
            if (wall.collider != null && reach >= wallDistance - 0.01f)
                AbilityObstacles.TryInteract(wall.collider, wall.point, ctx.direction, Data, dt);

            SpawnPuffs(ctx, wallDistance, dt);
        }

        void SpawnPuffs(AbilityContext ctx, float travel, float dt)
        {
            if (puffs == null || puffFrames == null || puffFrames.Length == 0) return;
            spawnTimer -= dt;
            while (spawnTimer <= 0f)
            {
                spawnTimer += spawnInterval;
                float angle = Random.Range(-spreadDegrees, spreadDegrees);
                Vector2 dir = Quaternion.Euler(0f, 0f, angle) * ctx.direction;
                float speed = puffSpeed * (1f + Random.Range(-speedVariation, speedVariation));
                bool variant = puffFramesVariant != null && puffFramesVariant.Length == puffFrames.Length &&
                               Random.value < 0.5f;
                puffs.Get().Launch(ctx.origin, dir * speed, travel, range,
                    variant ? puffFramesVariant : puffFrames, smokeFrames, smokeDuration,
                    Random.value < 0.5f, Random.value < 0.5f);
                if (spawnInterval <= 0f) break;
            }
        }

        public override void End()
        {
            if (damageZone != null) damageZone.Deactivate();
        }
    }
}
