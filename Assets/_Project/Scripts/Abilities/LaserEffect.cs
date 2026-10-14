using Game.Combat;
using UnityEngine;

namespace Game.Abilities
{
    /// <summary>
    /// Láser de Veyron (sección 8.2). Sostenido; atraviesa a los enemigos y solo lo detienen las paredes.
    /// Tiene un arranque corto (chargeTime) antes de hacer daño, en el que se dibuja fino.
    /// El rayo es un LineRenderer pixel perfect (8.8): textura en Tile que se repite cada 16 px,
    /// ancho en múltiplos de 1/16 y extremos en la rejilla de píxeles. Se anima alternando texturas.
    /// </summary>
    [RequireComponent(typeof(BeamCaster))]
    public class LaserEffect : HeldAbilityEffect
    {
        [Header("Rayo")]
        [SerializeField] LineRenderer line;
        [Tooltip("Texturas del rayo (Wrap Mode = Repeat, Filter = Point); se alternan para animarlo.")]
        [SerializeField] Texture2D[] beamTextures;
        [SerializeField] float beamFps = 20f;
        [Tooltip("Ancho en unidades (6 px = 0.375).")]
        [SerializeField] float width = 0.375f;
        [Tooltip("Ancho durante el arranque (2 px).")]
        [SerializeField] float chargeWidth = 0.125f;
        [Tooltip("Segundos antes de hacer daño. [AJUSTAR]")]
        [SerializeField] float chargeTime = 0.1f;

        [Header("Extremos")]
        [SerializeField] SpriteRenderer originSprite;
        [SerializeField] Sprite[] originFrames;
        [SerializeField] SpriteRenderer impactSprite;
        [SerializeField] Sprite[] impactFrames;
        [SerializeField] float endsFps = 15f;

        static readonly int MainTex = Shader.PropertyToID("_MainTex");

        BeamCaster caster;
        MaterialPropertyBlock block;
        float elapsed;

        void Awake()
        {
            caster = GetComponent<BeamCaster>();
            block = new MaterialPropertyBlock();
            if (line != null)
            {
                line.positionCount = 2;
                line.useWorldSpace = true;
                line.textureMode = LineTextureMode.Tile;
                line.enabled = false;
            }
            SetEnds(false, false);
        }

        protected override void OnBegin(AbilityContext ctx)
        {
            elapsed = 0f;
            caster.ResetTicks();
            if (line != null) line.enabled = true;
            Sustain(ctx, 0f);
        }

        public override void Sustain(AbilityContext ctx, float dt)
        {
            elapsed += dt;
            bool charged = elapsed >= chargeTime;

            float reach = caster.Cast(ctx.origin, ctx.direction, Owner != null ? Owner.gameObject : null,
                charged, out RaycastHit2D wall);
            if (charged && wall.collider != null)
                AbilityObstacles.TryInteract(wall.collider, wall.point, ctx.direction, Data, dt);

            Vector2 start = Snap(ctx.origin);
            Vector2 end = Snap(ctx.origin + ctx.direction * reach);

            if (line != null)
            {
                line.SetPosition(0, start);
                line.SetPosition(1, end);
                line.startWidth = line.endWidth = charged ? width : chargeWidth;
                if (beamTextures != null && beamTextures.Length > 0)
                {
                    int i = Mathf.FloorToInt(elapsed * beamFps) % beamTextures.Length;
                    line.GetPropertyBlock(block);
                    block.SetTexture(MainTex, beamTextures[i]);
                    line.SetPropertyBlock(block);
                }
            }

            bool hitWall = wall.collider != null;
            SetEnds(true, charged && hitWall);
            if (originSprite != null)
            {
                originSprite.transform.position = start;
                originSprite.sprite = DirectionalFrames.Frame(originFrames, elapsed, endsFps, true);
            }
            if (impactSprite != null && hitWall)
            {
                impactSprite.transform.position = end;
                impactSprite.sprite = DirectionalFrames.Frame(impactFrames, elapsed, endsFps, true);
            }
        }

        public override void End()
        {
            if (line != null) line.enabled = false;
            SetEnds(false, false);
        }

        void SetEnds(bool origin, bool impact)
        {
            if (originSprite != null) originSprite.enabled = origin;
            if (impactSprite != null) impactSprite.enabled = impact;
        }

        /// <summary>A la rejilla de píxeles (1/16), para que el rayo no tiemble entre frames.</summary>
        static Vector2 Snap(Vector2 v) => new(Mathf.Round(v.x * 16f) / 16f, Mathf.Round(v.y * 16f) / 16f);
    }
}
