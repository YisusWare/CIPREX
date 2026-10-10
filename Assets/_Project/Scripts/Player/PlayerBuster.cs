using System;
using Game.Audio;
using Game.Combat;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Disparo básico (secciones 5.2 y 7): cadencia, límite de balas y spawn desde el FirePoint
    /// de la dirección de apuntado. No anima ni decide cuándo disparar (eso es la FSM de acción).
    /// </summary>
    public class PlayerBuster : MonoBehaviour
    {
        [SerializeField] BusterData data;
        [Tooltip("El Visual que se voltea. Los FirePoints son hijos suyos.")]
        [SerializeField] Transform visualRoot;
        [SerializeField] Transform firePointForward;
        [SerializeField] Transform firePointDiagUp;
        [SerializeField] Transform firePointUp;
        [SerializeField] Transform firePointDown;
        [SerializeField] Transform firePointCrouch;

        ProjectilePool pool;
        float lastShotTime = float.NegativeInfinity;

        public BusterData Data => data;

        /// <summary>Se lanza al disparar (para el fogonazo en el Hito 3).</summary>
        public event Action<AimDirection, Vector2> Fired;

        void Awake()
        {
            if (data == null || data.projectilePrefab == null)
            {
                Debug.LogError("PlayerBuster: falta BusterData o su prefab de proyectil.", this);
                return;
            }
            pool = new ProjectilePool(data.projectilePrefab, "PlayerProjectiles");
        }

        public bool CanFire =>
            pool != null &&
            Time.time - lastShotTime >= data.minTimeBetweenShots &&
            (data.maxProjectilesOnScreen <= 0 || pool.CountActive < data.maxProjectilesOnScreen);

        public void Fire(AimDirection aim, int facing, bool crouching)
        {
            if (pool == null) return;
            Vector2 origin = FirePosition(aim, facing, crouching);
            Vector2 dir = DirectionFor(aim, facing);

            pool.Get().Launch(origin, dir, data.projectileSpeed, data.damage, gameObject);
            lastShotTime = Time.time;
            SfxPlayer.Play(data.fireSfx, origin);
            Fired?.Invoke(aim, origin);
        }

        public static Vector2 DirectionFor(AimDirection aim, int facing) => aim switch
        {
            AimDirection.DiagUp => new Vector2(facing, 1f).normalized,
            AimDirection.Up => Vector2.up,
            AimDirection.Down => Vector2.down,
            _ => new Vector2(facing, 0f),
        };

        public Transform FirePointFor(AimDirection aim, bool crouching)
        {
            if (crouching) return firePointCrouch;
            return aim switch
            {
                AimDirection.DiagUp => firePointDiagUp,
                AimDirection.Up => firePointUp,
                AimDirection.Down => firePointDown,
                _ => firePointForward,
            };
        }

        /// <summary>
        /// Posición del FirePoint calculada con el facing actual. No depende de que el Visual
        /// ya se haya volteado en este frame.
        /// </summary>
        public Vector2 FirePosition(AimDirection aim, int facing, bool crouching)
        {
            Transform fp = FirePointFor(aim, crouching);
            if (fp == null || visualRoot == null) return transform.position;
            Vector3 local = visualRoot.InverseTransformPoint(fp.position); // sin el volteo
            return (Vector2)transform.position + new Vector2(local.x * facing, local.y);
        }
    }
}
