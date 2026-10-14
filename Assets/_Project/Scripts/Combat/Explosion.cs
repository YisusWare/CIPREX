using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Explosión de un solo golpe (sección 8.7): OverlapCircle al aparecer, golpea una vez a cada
    /// objetivo y reproduce su animación. Sale de un ComponentPool. La usan la granada y los enemigos.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Explosion : MonoBehaviour, IPoolable
    {
        [Tooltip("Frames de la animación. El arte se dibuja del tamaño del radio de daño.")]
        [SerializeField] Sprite[] frames;
        [SerializeField] float fps = 15f;

        static readonly List<Collider2D> Results = new();
        readonly HashSet<IDamageable> damaged = new();

        SpriteRenderer sr;
        Action release;
        float time;
        bool playing;

        public void BindPool(Action releaseAction) => release = releaseAction;

        void Awake() => sr = GetComponent<SpriteRenderer>();

        public void Explode(Vector2 position, float radius, int damage, LayerMask targetMask, GameObject source,
            float knockback = 0f)
        {
            transform.position = position;
            time = 0f;
            playing = true;
            if (frames != null && frames.Length > 0) sr.sprite = frames[0];

            var filter = new ContactFilter2D();
            filter.SetLayerMask(targetMask);
            filter.useTriggers = true;
            damaged.Clear();
            int count = Physics2D.OverlapCircle(position, radius, filter, Results);
            for (int i = 0; i < count; i++)
            {
                var target = Results[i].GetComponentInParent<IDamageable>();
                if (target == null || !damaged.Add(target)) continue;
                Vector2 dir = (Vector2)Results[i].bounds.center - position;
                target.TakeDamage(new DamageInfo
                {
                    amount = damage,
                    direction = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.up,
                    source = source,
                    knockback = knockback,
                });
            }
        }

        void Update()
        {
            if (!playing) return;
            time += Time.deltaTime;
            int frame = Mathf.FloorToInt(time * fps);
            if (frames == null || frame >= frames.Length)
            {
                playing = false;
                if (release != null) release();
                else Destroy(gameObject);
                return;
            }
            sr.sprite = frames[frame];
        }
    }
}
