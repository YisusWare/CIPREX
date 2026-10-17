using Game.Audio;
using Game.Combat;
using Game.Player;
using UnityEngine;

namespace Game.Abilities
{
    /// <summary>
    /// Tajo cuerpo a cuerpo (sección 8.2): espada de fuego de Rosslyn y cuchillo de Veyron, con
    /// distintos valores y arte. 4 direcciones + agachada; la dirección se fija al empezar.
    /// - Un DamageHitbox hijo por dirección (se colocan a ojo en el prefab). Solo se activa el de la
    ///   dirección del tajo, y solo en su ventana de frames.
    /// - La duración sale del clip del torso de esa dirección (AbilityData.customTorsoClips). Si el
    ///   clip aún no está dibujado, usa fallbackFrameCount a fallbackFps.
    /// - El tajo hacia abajo rebota al golpear (pogo) si pogoJumpHeight > 0.
    /// - La hitbox destruye proyectiles enemigos (projectileMask de cada DamageHitbox).
    /// El objeto se voltea con la dirección del jugador (escala X), así que las hitboxes se colocan
    /// mirando a la derecha.
    /// </summary>
    public class MeleeEffect : TimedAbilityEffect
    {
        [Header("Hitboxes (hijos, mirando a la derecha)")]
        [SerializeField] DamageHitbox hitboxForward;
        [SerializeField] DamageHitbox hitboxDiagUp;
        [SerializeField] DamageHitbox hitboxUp;
        [SerializeField] DamageHitbox hitboxDown;
        [SerializeField] DamageHitbox hitboxCrouch;

        [Header("Golpe [AJUSTAR]")]
        [SerializeField] int damage = 4;
        [SerializeField] float knockback;
        [Tooltip("Primer frame del clip con la hitbox activa (desde 0).")]
        [SerializeField, Min(0)] int activeFromFrame = 1;
        [Tooltip("Último frame del clip con la hitbox activa (incluido).")]
        [SerializeField, Min(0)] int activeToFrame = 2;
        [Tooltip("Frames del tajo cuando falta el clip del torso.")]
        [SerializeField, Min(1)] int fallbackFrameCount = 4;
        [SerializeField] float fallbackFps = 12f;

        [Header("Rebote del tajo hacia abajo (pogo) [AJUSTAR]")]
        [Tooltip("Altura del rebote en unidades. 0 = sin rebote.")]
        [SerializeField] float pogoJumpHeight = 2.5f;

        [Header("Arte y sonido")]
        [Tooltip("VFX del tajo (llamas de la espada). Se coloca en el centro de la hitbox activa.")]
        [SerializeField] SpriteRenderer vfx;
        [SerializeField] DirectionalFrames vfxFrames = new();
        [SerializeField] SfxData deflectSfx;

        DamageHitbox active;
        Sprite[] vfxCurrent;
        float elapsed, onTime, offTime, vfxFps;
        bool down, pogoDone;

        public override void Setup(AbilityData data, PlayerController owner)
        {
            base.Setup(data, owner);
            foreach (var h in Hitboxes())
            {
                if (h == null) continue;
                h.Hit += OnHit;
                h.ProjectileDestroyed += OnProjectileDestroyed;
            }
            if (vfx != null) vfx.enabled = false;
        }

        DamageHitbox[] Hitboxes() => new[] { hitboxForward, hitboxDiagUp, hitboxUp, hitboxDown, hitboxCrouch };

        DamageHitbox HitboxFor(AimDirection aim, bool crouching)
        {
            if (crouching) return hitboxCrouch != null ? hitboxCrouch : hitboxForward;
            var h = aim switch
            {
                AimDirection.DiagUp => hitboxDiagUp,
                AimDirection.Up => hitboxUp,
                AimDirection.Down => hitboxDown,
                _ => hitboxForward,
            };
            return h != null ? h : hitboxForward;
        }

        /// <summary>Duración del clip de esta dirección, o la de respaldo.</summary>
        protected override float LockDurationFor(AbilityContext ctx)
        {
            ResolveDirection(ref ctx);
            var clip = Data != null ? Data.customTorsoClips.Get(ctx.aim, ctx.crouching) : null;
            float fps = clip != null && clip.frameRate > 0f ? clip.frameRate : fallbackFps;
            float length = clip != null ? clip.length : fallbackFrameCount / fps;

            onTime = activeFromFrame / fps;
            offTime = Mathf.Min((activeToFrame + 1) / fps, length);
            vfxFps = fps;
            return length;
        }

        /// <summary>Si falta el clip de una dirección se usa el frontal, y la hitbox lo sigue.</summary>
        void ResolveDirection(ref AbilityContext ctx)
        {
            if (Data == null) return;
            Data.customTorsoClips.Resolve(ref ctx.aim, ref ctx.crouching);
        }

        protected override void OnBegin(AbilityContext ctx)
        {
            ResolveDirection(ref ctx);
            elapsed = 0f;
            pogoDone = false;
            down = ctx.aim == AimDirection.Down && !ctx.crouching;
            Face(ctx.facing);

            active = HitboxFor(ctx.aim, ctx.crouching);

            if (vfx != null && active != null)
            {
                // El objeto ya está volteado, así que los frames se eligen mirando a la derecha.
                Vector2 localDir = PlayerBuster.DirectionFor(ctx.aim, 1);
                vfxCurrent = vfxFrames.For(localDir, out bool flipX, out bool flipY);
                vfx.flipX = flipX;
                vfx.flipY = flipY;
                vfx.transform.position = active.transform.position;
                vfx.enabled = false;
            }
            OnSustain(ctx, 0f);
        }

        protected override void OnSustain(AbilityContext ctx, float dt)
        {
            elapsed += dt;
            Face(ctx.facing); // si el jugador se voltea a mitad del tajo, la hitbox lo sigue

            if (active == null) return;
            bool inWindow = elapsed >= onTime && elapsed < offTime;
            if (inWindow && !active.IsActive)
                active.Activate(Owner != null ? Owner.gameObject : null, damage, knockback);
            else if (!inWindow && active.IsActive)
                active.Deactivate();

            if (vfx != null)
            {
                vfx.enabled = inWindow && vfxCurrent != null && vfxCurrent.Length > 0;
                if (vfx.enabled)
                {
                    vfx.transform.position = active.transform.position;
                    vfx.sprite = DirectionalFrames.Frame(vfxCurrent, elapsed - onTime, vfxFps, false);
                }
            }
        }

        public override void End()
        {
            foreach (var h in Hitboxes())
                if (h != null) h.Deactivate();
            if (vfx != null) vfx.enabled = false;
            active = null;
        }

        void Face(int facing)
        {
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (facing < 0 ? -1f : 1f);
            transform.localScale = s;
        }

        void OnHit(IDamageable target) => TryPogo();

        void OnProjectileDestroyed(Projectile projectile)
        {
            SfxPlayer.Play(deflectSfx, projectile.transform.position);
            TryPogo();
        }

        void TryPogo()
        {
            if (!down || pogoDone || pogoJumpHeight <= 0f || Owner == null) return;
            pogoDone = Owner.Locomotion.Bounce(pogoJumpHeight);
        }
    }
}
