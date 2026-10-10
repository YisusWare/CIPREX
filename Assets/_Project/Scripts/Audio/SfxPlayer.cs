using System.Collections.Generic;
using UnityEngine;

namespace Game.Audio
{
    /// <summary>Referencia a un sonido en reproducción, para detener los loops.</summary>
    public readonly struct SfxHandle
    {
        readonly SfxPlayer player;
        readonly int index;
        readonly int id;

        internal SfxHandle(SfxPlayer player, int index, int id)
        {
            this.player = player;
            this.index = index;
            this.id = id;
        }

        public bool IsValid => player != null && player.IsPlaying(index, id);
        public void Stop() { if (player != null) player.Stop(index, id); }
    }

    /// <summary>
    /// Reproduce SfxData con un pool de AudioSource (sección 15.5): variación de clip y pitch,
    /// límite de voces, cooldown por sonido y paneo estéreo según la posición en pantalla.
    /// Es persistente. Si no existe en la escena, se crea al primer uso
    /// (en el Hito 6 vivirá en la escena Boot).
    /// </summary>
    public class SfxPlayer : MonoBehaviour
    {
        [SerializeField] int poolSize = 24;
        [Tooltip("Paneo máximo hacia cada lado.")]
        [SerializeField, Range(0f, 1f)] float panRange = 0.5f;
        [Tooltip("Margen fuera de cámara (fracción de pantalla, ~2 tiles) para los sonidos que se descartan fuera de pantalla.")]
        [SerializeField] float offscreenMargin = 0.1f;

        static SfxPlayer instance;

        AudioSource[] sources;
        SfxData[] owners;
        float[] startTimes;
        int[] playIds;
        int nextId = 1;
        readonly Dictionary<SfxData, float> lastPlayTime = new();
        Camera cam;

        public static SfxPlayer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<SfxPlayer>();
                    if (instance == null)
                        instance = new GameObject("SfxPlayer").AddComponent<SfxPlayer>();
                }
                return instance;
            }
        }

        /// <summary>
        /// Reproduce un sonido. worldPosition se usa para el paneo; cullOffscreen descarta el sonido
        /// si la posición está fuera de cámara (enemigos).
        /// </summary>
        public static SfxHandle Play(SfxData data, Vector3? worldPosition = null, bool cullOffscreen = false)
        {
            if (data == null || !data.HasClips) return default;
            return Instance.PlayInternal(data, worldPosition, cullOffscreen);
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);

            sources = new AudioSource[poolSize];
            owners = new SfxData[poolSize];
            startTimes = new float[poolSize];
            playIds = new int[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f; // 2D
                sources[i] = src;
            }
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        SfxHandle PlayInternal(SfxData data, Vector3? worldPosition, bool cullOffscreen)
        {
            float now = Time.unscaledTime;
            if (lastPlayTime.TryGetValue(data, out float last) && now - last < data.cooldown)
                return default;

            float pan = 0f;
            if (worldPosition.HasValue)
            {
                if (cam == null) cam = Camera.main;
                if (cam != null)
                {
                    Vector3 vp = cam.WorldToViewportPoint(worldPosition.Value);
                    if (cullOffscreen && (vp.x < -offscreenMargin || vp.x > 1f + offscreenMargin ||
                                          vp.y < -offscreenMargin || vp.y > 1f + offscreenMargin))
                        return default;
                    pan = Mathf.Lerp(-panRange, panRange, Mathf.Clamp01(vp.x));
                }
            }

            AudioClip clip = PickClip(data);
            if (clip == null) return default;

            int index = FindSource(data);
            var src = sources[index];
            src.Stop();
            src.clip = clip;
            src.volume = data.volume;
            src.pitch = Random.Range(data.pitchRange.x, data.pitchRange.y);
            src.outputAudioMixerGroup = data.group;
            src.loop = data.loop;
            src.panStereo = pan;
            src.Play();

            owners[index] = data;
            startTimes[index] = now;
            playIds[index] = nextId++;
            lastPlayTime[data] = now;
            return new SfxHandle(this, index, playIds[index]);
        }

        static AudioClip PickClip(SfxData data)
        {
            int count = 0;
            foreach (var c in data.clips) if (c != null) count++;
            if (count == 0) return null;
            int pick = Random.Range(0, count);
            foreach (var c in data.clips)
            {
                if (c == null) continue;
                if (pick-- == 0) return c;
            }
            return null;
        }

        /// <summary>Respeta maxVoices; si no hay fuentes libres, reemplaza la más vieja.</summary>
        int FindSource(SfxData data)
        {
            int voices = 0, oldestSame = -1, free = -1, oldestAny = 0;
            for (int i = 0; i < sources.Length; i++)
            {
                bool playing = sources[i].isPlaying;
                if (!playing)
                {
                    if (free < 0) free = i;
                    continue;
                }
                if (owners[i] == data)
                {
                    voices++;
                    if (oldestSame < 0 || startTimes[i] < startTimes[oldestSame]) oldestSame = i;
                }
                if (startTimes[i] < startTimes[oldestAny]) oldestAny = i;
            }

            if (voices >= Mathf.Max(1, data.maxVoices) && oldestSame >= 0) return oldestSame;
            if (free >= 0) return free;
            return oldestAny;
        }

        internal bool IsPlaying(int index, int id) =>
            sources != null && index >= 0 && index < sources.Length && playIds[index] == id && sources[index].isPlaying;

        internal void Stop(int index, int id)
        {
            if (sources == null || index < 0 || index >= sources.Length || playIds[index] != id) return;
            sources[index].Stop();
        }
    }
}
