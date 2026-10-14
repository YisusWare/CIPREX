using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Barra de segmentos al estilo Mega Man (sección 15.6). La usan la vida, la energía y, después,
    /// el jefe y los vehículos. Recibe valor actual y máximo y dibuja un segmento por unidad,
    /// de abajo hacia arriba. Todo en píxeles de la resolución de referencia (320×180).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SegmentedBar : MonoBehaviour
    {
        [Tooltip("Sprite de cada segmento (blanco; se tiñe con los colores de abajo).")]
        [SerializeField] Sprite segmentSprite;
        [SerializeField] Color fullColor = new(1f, 0.85f, 0.35f);
        [SerializeField] Color emptyColor = new(0.15f, 0.12f, 0.2f);
        [Tooltip("Tamaño de cada segmento en píxeles.")]
        [SerializeField] Vector2Int segmentSize = new(6, 1);
        [Tooltip("Separación entre segmentos en píxeles.")]
        [SerializeField] int spacing = 1;
        [Tooltip("Borde alrededor de los segmentos en píxeles.")]
        [SerializeField] int padding = 1;

        readonly List<Image> segments = new();
        int max = -1;
        int shown = -1;

        public void SetColors(Color full, Color empty)
        {
            fullColor = full;
            emptyColor = empty;
            shown = -1;
        }

        /// <summary>Muestra el valor redondeado hacia abajo (como Mega Man).</summary>
        public void SetValue(float current, float maximum)
        {
            int newMax = Mathf.Max(0, Mathf.RoundToInt(maximum));
            if (newMax != max) Build(newMax);

            int value = Mathf.Clamp(Mathf.FloorToInt(current + 0.0001f), 0, max);
            if (value == shown) return;
            shown = value;
            for (int i = 0; i < segments.Count; i++)
                segments[i].color = i < value ? fullColor : emptyColor;
        }

        void Build(int count)
        {
            foreach (var s in segments)
                if (s != null) Destroy(s.gameObject);
            segments.Clear();
            max = count;
            shown = -1;

            var rt = (RectTransform)transform;
            int height = padding * 2 + count * segmentSize.y + Mathf.Max(0, count - 1) * spacing;
            rt.sizeDelta = new Vector2(segmentSize.x + padding * 2, height);

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"Segment_{i}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                var srt = (RectTransform)go.transform;
                srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0f);
                srt.pivot = new Vector2(0.5f, 0f);
                srt.sizeDelta = segmentSize;
                srt.anchoredPosition = new Vector2(0f, padding + i * (segmentSize.y + spacing));
                var img = go.GetComponent<Image>();
                img.sprite = segmentSprite;
                img.raycastTarget = false;
                segments.Add(img);
            }
        }
    }
}
