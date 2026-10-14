using System;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Game.Combat
{
    /// <summary>
    /// Un objeto que sale de un ComponentPool. El pool le da la acción para devolverse a sí mismo;
    /// el objeto debe evitar llamarla dos veces en el mismo uso.
    /// </summary>
    public interface IPoolable
    {
        void BindPool(Action release);
    }

    /// <summary>
    /// Pool genérico de un prefab (granadas, explosiones, llamas). Los objetos se crean dentro de un
    /// contenedor en la raíz de la escena para que no sigan al jugador.
    /// </summary>
    public class ComponentPool<T> where T : Component
    {
        readonly T prefab;
        readonly Transform container;
        readonly ObjectPool<T> pool;

        public int CountActive => pool.CountActive;

        public ComponentPool(T prefab, string containerName, int defaultCapacity = 8, int maxSize = 64)
        {
            this.prefab = prefab;
            container = new GameObject(containerName).transform;
            pool = new ObjectPool<T>(Create, OnGet, OnRelease, OnDestroyItem,
                collectionCheck: false, defaultCapacity: defaultCapacity, maxSize: maxSize);
        }

        public T Get() => pool.Get();

        T Create()
        {
            var item = Object.Instantiate(prefab, container);
            if (item is IPoolable poolable) poolable.BindPool(() => pool.Release(item));
            item.gameObject.SetActive(false);
            return item;
        }

        static void OnGet(T item) => item.gameObject.SetActive(true);
        static void OnRelease(T item) => item.gameObject.SetActive(false);
        static void OnDestroyItem(T item)
        {
            if (item != null) Object.Destroy(item.gameObject);
        }
    }
}
