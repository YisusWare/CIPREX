using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
    /// <summary>Un efecto de sonido (sección 15.5). Si no tiene clips, simplemente no suena.</summary>
    [CreateAssetMenu(menuName = "Game/Sfx", fileName = "Sfx_")]
    public class SfxData : ScriptableObject
    {
        [Tooltip("Si hay varios, se elige uno al azar.")]
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
        [Tooltip("Variación de pitch para que no suene repetitivo.")]
        public Vector2 pitchRange = new(0.95f, 1.05f);
        [Tooltip("Gameplay o UI, dentro del GameAudioMixer.")]
        public AudioMixerGroup group;
        [Tooltip("Cuántas copias pueden sonar a la vez. Al llegar al límite se reemplaza la más vieja.")]
        public int maxVoices = 4;
        [Tooltip("Tiempo mínimo entre dos reproducciones (segundos).")]
        public float cooldown = 0.03f;
        [Tooltip("Lanzallamas, láser, acero calentándose.")]
        public bool loop;

        public bool HasClips
        {
            get
            {
                if (clips == null) return false;
                foreach (var c in clips) if (c != null) return true;
                return false;
            }
        }
    }
}
