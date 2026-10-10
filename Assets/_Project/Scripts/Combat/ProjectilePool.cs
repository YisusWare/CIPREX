using UnityEngine;
using UnityEngine.Pool;

namespace Game.Combat
{
    /// <summary>Pool de un prefab de proyectil (UnityEngine.Pool.ObjectPool).</summary>
    public class ProjectilePool
    {
        readonly Projectile prefab;
        readonly Transform container;
        readonly ObjectPool<Projectile> pool;

        public int CountActive => pool.CountActive;

        public ProjectilePool(Projectile prefab, string containerName, int defaultCapacity = 16, int maxSize = 128)
        {
            this.prefab = prefab;
            container = new GameObject(containerName).transform;
            pool = new ObjectPool<Projectile>(Create, OnGet, OnRelease, OnDestroyItem,
                collectionCheck: false, defaultCapacity: defaultCapacity, maxSize: maxSize);
        }

        public Projectile Get() => pool.Get();

        Projectile Create()
        {
            var p = Object.Instantiate(prefab, container);
            p.SetPool(pool);
            p.gameObject.SetActive(false);
            return p;
        }

        static void OnGet(Projectile p) => p.gameObject.SetActive(true);
        static void OnRelease(Projectile p) => p.gameObject.SetActive(false);
        static void OnDestroyItem(Projectile p)
        {
            if (p != null) Object.Destroy(p.gameObject);
        }
    }
}
