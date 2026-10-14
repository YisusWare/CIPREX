using System;
using Game.Combat;
using UnityEngine;

namespace Game.Abilities
{
    /// <summary>
    /// Una llama suelta del lanzallamas (sección 8.2). Solo es visual: viaja en el mundo hasta su
    /// distancia, eligiendo el frame según qué parte del alcance lleva (pequeña al salir, grande a
    /// media distancia), y al final se disipa con los frames de humo. Sale del pool de FlamethrowerEffect.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class FlamePuff : MonoBehaviour, IPoolable
    {
        [Tooltip("Velocidad con la que sube el humo (u/s).")]
        [SerializeField] float smokeRise = 1f;

        SpriteRenderer sr;
        Action release;
        Sprite[] frames;
        int smokeFrames;
        Vector2 velocity;
        float traveled, travel, fullRange, smokeDuration, smokeTime;
        bool smoking, live;

        public void BindPool(Action releaseAction) => release = releaseAction;

        void Awake() => sr = GetComponent<SpriteRenderer>();

        /// <param name="travelDistance">Hasta dónde llega (recortado por la pared).</param>
        /// <param name="range">Alcance completo: decide el frame según la distancia recorrida.</param>
        /// <param name="smokeFrameCount">Cuántos de los últimos frames son de humo.</param>
        public void Launch(Vector2 position, Vector2 velocity, float travelDistance, float range,
            Sprite[] animationFrames, int smokeFrameCount, float smokeSeconds, bool flipX, bool flipY)
        {
            transform.position = position;
            this.velocity = velocity;
            travel = Mathf.Max(travelDistance, 0f);
            fullRange = Mathf.Max(range, 0.01f);
            frames = animationFrames;
            smokeFrames = Mathf.Clamp(smokeFrameCount, 0, frames != null ? frames.Length - 1 : 0);
            smokeDuration = Mathf.Max(smokeSeconds, 0.01f);
            traveled = 0f;
            smokeTime = 0f;
            smoking = false;
            live = true;
            sr.flipX = flipX;
            sr.flipY = flipY;
            if (frames == null || frames.Length == 0) { Finish(); return; }
            sr.sprite = frames[0];
        }

        void Update()
        {
            if (!live) return;
            float dt = Time.deltaTime;

            if (!smoking)
            {
                float step = velocity.magnitude * dt;
                if (traveled + step >= travel)
                {
                    step = travel - traveled;
                    smoking = true;
                }
                traveled += step;
                if (velocity.sqrMagnitude > 0f) transform.position += (Vector3)(velocity.normalized * step);

                int flameFrames = frames.Length - smokeFrames;
                int i = Mathf.Min(Mathf.FloorToInt(traveled / fullRange * flameFrames), flameFrames - 1);
                sr.sprite = frames[Mathf.Max(i, 0)];
                return;
            }

            smokeTime += dt;
            transform.position += Vector3.up * (smokeRise * dt);
            if (smokeFrames == 0 || smokeTime >= smokeDuration) { Finish(); return; }
            int s = Mathf.Min(Mathf.FloorToInt(smokeTime / smokeDuration * smokeFrames), smokeFrames - 1);
            sr.sprite = frames[frames.Length - smokeFrames + s];
        }

        void Finish()
        {
            if (!live) return;
            live = false;
            if (release != null) release();
            else Destroy(gameObject);
        }
    }
}
