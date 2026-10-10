using Game.Audio;
using Game.Combat;
using UnityEngine;

namespace Game.Player
{
    /// <summary>Disparo básico (sección 7.1).</summary>
    [CreateAssetMenu(menuName = "Game/Buster", fileName = "BusterData")]
    public class BusterData : ScriptableObject
    {
        public Projectile projectilePrefab;
        public int damage = 1;
        [Tooltip("Unidades por segundo.")]
        public float projectileSpeed = 18f;
        public float minTimeBetweenShots = 0.1f;
        [Tooltip("0 = sin límite.")]
        public int maxProjectilesOnScreen = 0;
        public float shootPoseDuration = 0.3f;
        public SfxData fireSfx;
    }
}
