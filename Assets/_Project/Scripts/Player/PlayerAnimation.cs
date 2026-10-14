using System.Collections.Generic;
using Game.Abilities;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Traduce el estado del jugador a los tres Animators (sección 9): Legs, Torso y FullBody.
    /// Los estados se eligen por código con Animator.Play() y listas de prioridad; los Animators
    /// no tienen parámetros ni transiciones. También voltea el Visual y hace el parpadeo de i-frames.
    /// </summary>
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        [Tooltip("El Visual que se voltea con localScale.x = ±1.")]
        [SerializeField] Transform visual;
        [SerializeField] Animator legs;
        [SerializeField] Animator torso;
        [SerializeField] Animator fullBody;
        [Tooltip("Segundos entre encendido y apagado del parpadeo de i-frames.")]
        [SerializeField] float blinkInterval = 0.05f;

        // Nombres de estado = nombres de clip (sección 9.3).
        static readonly int LegsIdle = Hash("Legs_Idle"), LegsRun = Hash("Legs_Run"),
            LegsJumpUp = Hash("Legs_JumpUp"), LegsFall = Hash("Legs_Fall"),
            LegsCrouch = Hash("Legs_Crouch"), LegsDash = Hash("Legs_Dash");

        static readonly int TorsoIdle = Hash("Torso_Idle"), TorsoRun = Hash("Torso_Run"),
            TorsoJumpUp = Hash("Torso_JumpUp"), TorsoFall = Hash("Torso_Fall"),
            TorsoCrouch = Hash("Torso_Crouch"), TorsoDash = Hash("Torso_Dash"),
            TorsoAimUp = Hash("Torso_Aim_Up"), TorsoAimDiagUp = Hash("Torso_Aim_DiagUp"),
            ShootForward = Hash("Torso_Shoot_Forward"), ShootDiagUp = Hash("Torso_Shoot_DiagUp"),
            ShootUp = Hash("Torso_Shoot_Up"), ShootDown = Hash("Torso_Shoot_Down"),
            ShootCrouch = Hash("Torso_Shoot_Crouch");

        static readonly int FullHurt = Hash("FullBody_Hurt"), FullDeath = Hash("FullBody_Death"),
            FullTeleportIn = Hash("FullBody_Teleport_In"), FullTeleportOut = Hash("FullBody_Teleport_Out");

        static int Hash(string s) => Animator.StringToHash(s);

        struct ClipInfo { public int frames; public bool loop; }

        readonly Dictionary<int, ClipInfo> clipInfo = new();
        SpriteRenderer legsRenderer, torsoRenderer, fullBodyRenderer;
        int currentLegs, currentTorso, currentFull;
        bool shotThisFrame;

        void Awake()
        {
            legsRenderer = legs != null ? legs.GetComponent<SpriteRenderer>() : null;
            torsoRenderer = torso != null ? torso.GetComponent<SpriteRenderer>() : null;
            fullBodyRenderer = fullBody != null ? fullBody.GetComponent<SpriteRenderer>() : null;
            CacheClips(legs);
            CacheClips(torso);
            CacheClips(fullBody);
        }

        void Start()
        {
            if (player != null && player.Buster != null) player.Buster.Fired += OnFired;
            if (player != null && player.Context != null) player.Action.AbilityStarted += OnAbilityStarted;
        }

        void OnDestroy()
        {
            if (player != null && player.Buster != null) player.Buster.Fired -= OnFired;
            if (player != null && player.Context != null) player.Action.AbilityStarted -= OnAbilityStarted;
        }

        void OnFired(AimDirection aim, Vector2 origin) => shotThisFrame = true;
        // Activar una habilidad reinicia su clip (propio o de disparo) aunque el estado no cambie.
        void OnAbilityStarted(AbilityData ability) => shotThisFrame = true;

        void CacheClips(Animator animator)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                clipInfo[Hash(clip.name)] = new ClipInfo
                {
                    frames = Mathf.RoundToInt(clip.length * clip.frameRate),
                    loop = clip.isLooping,
                };
            }
        }

        void LateUpdate()
        {
            if (player == null || player.Context == null) return;
            var loco = player.Locomotion;

            if (visual != null) visual.localScale = new Vector3(loco.FacingDir, 1f, 1f);

            // Cuerpo completo en Hurt, Dead y Respawning (sección 6.2).
            int full = player.LifeState switch
            {
                PlayerLifeState.Hurt => FullHurt,
                PlayerLifeState.Dead => FullDeath,
                PlayerLifeState.Respawning => FullTeleportIn,
                _ => 0,
            };
            bool showFull = full != 0 && fullBody != null && fullBody.HasState(0, full);

            if (showFull)
            {
                if (full != currentFull) { fullBody.Play(full, 0, 0f); currentFull = full; }
                currentLegs = currentTorso = 0; // al volver se reproducen desde el inicio
            }
            else
            {
                currentFull = 0;
                Play(legs, ChooseLegs(loco), LegsIdle, ref currentLegs, false);
                int t = ChooseTorso(loco, player.Action);
                bool isShoot = t == ShootForward || t == ShootDiagUp || t == ShootUp || t == ShootDown || t == ShootCrouch;
                bool isAbilityClip = t == abilityClipHash && t != 0;
                // Cada disparo (y cada uso de habilidad) reinicia su clip, aunque el estado no cambie (sección 9.3).
                Play(torso, t, TorsoIdle, ref currentTorso, (isShoot || isAbilityClip) && shotThisFrame);
            }
            shotThisFrame = false;

            // Visibilidad + parpadeo de i-frames (sección 10.1).
            bool blinkOff = player.Health.IsInvulnerable && Mathf.FloorToInt(Time.time / blinkInterval) % 2 == 0;
            SetVisible(fullBodyRenderer, showFull && !blinkOff);
            SetVisible(legsRenderer, !showFull && !blinkOff);
            SetVisible(torsoRenderer, !showFull && !blinkOff);
        }

        static void SetVisible(SpriteRenderer r, bool visible)
        {
            if (r != null && r.enabled != visible) r.enabled = visible;
        }

        int ChooseLegs(LocomotionFSM loco)
        {
            if (loco.IsDashing) return LegsDash;
            if (loco.IsCrouching) return LegsCrouch;
            if (loco.IsAirborne) return player.Motor.Velocity.y > 0f ? LegsJumpUp : LegsFall;
            if (loco.Current == loco.Run) return LegsRun;
            return LegsIdle;
        }

        int abilityClipHash;

        /// <summary>Lista de prioridad del torso (sección 9.2).</summary>
        int ChooseTorso(LocomotionFSM loco, ActionFSM action)
        {
            abilityClipHash = 0;
            if (loco.IsDashing) return TorsoDash;

            // 2. Habilidad con clip propio en curso (espada, cuchillo).
            var ability = action.ActiveAbility;
            if (ability != null && ability.torsoPose == AbilityTorsoPose.CustomClip)
            {
                var clip = loco.IsCrouching && ability.customTorsoClipCrouch != null
                    ? ability.customTorsoClipCrouch
                    : ability.customTorsoClip;
                if (clip != null)
                {
                    abilityClipHash = Hash(clip.name);
                    return abilityClipHash;
                }
            }

            if (action.IsInShootPose)
            {
                if (loco.IsCrouching) return ShootCrouch;
                // 3. Pose forzada hacia adelante (granada).
                if (action.ForceForwardPose) return ShootForward;
                return action.AimDir switch
                {
                    AimDirection.Up => ShootUp,
                    AimDirection.DiagUp => ShootDiagUp,
                    AimDirection.Down => ShootDown,
                    _ => ShootForward,
                };
            }

            if (action.AimDir == AimDirection.Up) return TorsoAimUp;
            if (action.AimDir == AimDirection.DiagUp) return TorsoAimDiagUp;

            if (loco.IsCrouching) return TorsoCrouch;
            if (loco.IsAirborne) return player.Motor.Velocity.y > 0f ? TorsoJumpUp : TorsoFall;
            if (loco.Current == loco.Run) return TorsoRun;
            return TorsoIdle;
        }

        /// <summary>
        /// Llama a Animator.Play solo si el estado cambió (o si se pide reiniciar).
        /// Entre dos clips en loop con la misma cantidad de frames se conserva el ciclo
        /// (por ejemplo Run ↔ Aim_DiagUp corriendo), para que las piernas no salten de frame.
        /// </summary>
        void Play(Animator animator, int state, int fallback, ref int current, bool restart)
        {
            if (animator == null) return;
            if (!animator.HasState(0, state)) state = fallback; // clip aún no dibujado
            if (state == current && !restart) return;

            float t = 0f;
            if (!restart && current != 0 && KeepsCycle(current, state))
                t = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;

            animator.Play(state, 0, t);
            current = state;
        }

        bool KeepsCycle(int from, int to) =>
            clipInfo.TryGetValue(from, out var a) && clipInfo.TryGetValue(to, out var b) &&
            a.loop && b.loop && a.frames > 1 && a.frames == b.frames;
    }
}
