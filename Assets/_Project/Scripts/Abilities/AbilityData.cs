using System;
using Game.Audio;
using Game.Combat;
using Game.Player;
using UnityEngine;

namespace Game.Abilities
{
    /// <summary>Cómo se cobra la energía (sección 8.3).</summary>
    public enum AbilityActivation { Press, Hold }

    /// <summary>Qué hace el torso mientras se usa la habilidad (sección 8.3).</summary>
    public enum AbilityTorsoPose
    {
        /// <summary>Pose de disparo según la dirección de apuntado.</summary>
        ShootPose,
        /// <summary>Pose de disparo siempre hacia adelante. Ninguna habilidad la usa por ahora.</summary>
        ShootPoseForward,
        /// <summary>Clip propio del torso (espada, cuchillo).</summary>
        CustomClip,
    }

    /// <summary>Un clip del torso por dirección (espada y cuchillo, sección 8.3).</summary>
    [Serializable]
    public struct DirectionalClips
    {
        [Tooltip("Torso_Melee. También se usa cuando falta el clip de otra dirección.")]
        public AnimationClip forward;
        [Tooltip("Torso_Melee_DiagUp. Si falta, se usa el de arriba.")]
        public AnimationClip diagUp;
        [Tooltip("Torso_Melee_Up")]
        public AnimationClip up;
        [Tooltip("Torso_Melee_Down (solo en el aire)")]
        public AnimationClip down;
        [Tooltip("Torso_Melee_Crouch")]
        public AnimationClip crouch;

        /// <summary>
        /// La dirección que de verdad se va a ver: si falta el clip de la pedida, la de forward.
        /// Así la hitbox coincide con el dibujo (por ejemplo, sin clip diagonal, el tajo diagonal es el frontal).
        /// </summary>
        public void Resolve(ref AimDirection aim, ref bool crouching)
        {
            if (crouching)
            {
                if (crouch == null) { crouching = false; aim = AimDirection.Forward; }
                return;
            }
            // Sin clip diagonal, el tajo diagonal es el de arriba (y si tampoco hay, el frontal).
            if (aim == AimDirection.DiagUp && diagUp == null && up != null) aim = AimDirection.Up;
            bool has = aim switch
            {
                AimDirection.DiagUp => diagUp != null,
                AimDirection.Up => up != null,
                AimDirection.Down => down != null,
                _ => true,
            };
            if (!has && forward != null) aim = AimDirection.Forward;
        }

        /// <summary>
        /// El clip de la dirección. Si falta, usa forward (o crouch si está agachada); sin diagonal,
        /// usa el de arriba.
        /// </summary>
        public AnimationClip Get(AimDirection aim, bool crouching)
        {
            if (crouching) return crouch != null ? crouch : forward;
            if (aim == AimDirection.DiagUp && diagUp == null && up != null) return up;
            AnimationClip clip = aim switch
            {
                AimDirection.DiagUp => diagUp,
                AimDirection.Up => up,
                AimDirection.Down => down,
                _ => forward,
            };
            return clip != null ? clip : forward;
        }
    }

    /// <summary>
    /// Datos de una habilidad que leen OTROS sistemas: guardado, HUD, energía, FSM, animación y obstáculos
    /// (sección 8.3). Los valores propios de cada habilidad (daño, radio, velocidad, alcance…) son campos
    /// del AbilityEffect de su prefab, no de este asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Ability", fileName = "Ability")]
    public class AbilityData : ScriptableObject
    {
        [Tooltip("Clave para el guardado. No cambiarla después de tener partidas guardadas.")]
        public string id;
        public string displayName;
        [Tooltip("Icono del HUD (16×16).")]
        public Sprite icon;

        [Header("Activación")]
        public AbilityActivation activation;
        [Tooltip("Press: energía que cuesta cada uso.")]
        [Min(0f)] public float energyCost;
        [Tooltip("Hold: energía por segundo mientras está activa.")]
        [Min(0f)] public float energyPerSecond;
        [Tooltip("Segundos entre usos.")]
        [Min(0f)] public float cooldown;
        [Tooltip("Máximo de instancias del efecto en pantalla (p. ej. 1 taladro). 0 = sin límite.")]
        [Min(0)] public int maxInstancesOnScreen;

        [Header("Jugador")]
        [Tooltip("¿Respeta la dirección de apuntado? Si no, siempre sale hacia adelante.")]
        public bool usesAim = true;
        public bool usableWhileCrouching = true;
        [Tooltip("Multiplica la velocidad del jugador mientras está activa. 0 = inmoviliza (láser): en el aire solo cae.")]
        [Min(0f)] public float moveSpeedMultiplier = 1f;

        [Header("Mundo")]
        [Tooltip("Interacciones con obstáculos (15.2): Break, Melt…")]
        public ObstacleInteraction[] obstacleInteractions;

        [Header("Presentación")]
        [Tooltip("Prefab con el componente AbilityEffect.")]
        public AbilityEffect effectPrefab;
        public AbilityTorsoPose torsoPose = AbilityTorsoPose.ShootPose;
        [Tooltip("Solo con CustomClip: un clip por dirección. Cada clip debe existir como estado en el Animator del torso.")]
        public DirectionalClips customTorsoClips;
        public SfxData activateSfx;
        [Tooltip("Hold: sonido en loop mientras está activa.")]
        public SfxData loopSfx;

        public bool HasInteraction(ObstacleInteraction interaction)
        {
            if (interaction == null || obstacleInteractions == null) return false;
            foreach (var i in obstacleInteractions)
                if (i == interaction) return true;
            return false;
        }

        /// <summary>Energía mínima para poder activarla (Hold: al menos 0.1 s de uso, sección 8.5).</summary>
        public float MinimumEnergyToStart =>
            activation == AbilityActivation.Press ? energyCost : energyPerSecond * 0.1f;
    }
}
