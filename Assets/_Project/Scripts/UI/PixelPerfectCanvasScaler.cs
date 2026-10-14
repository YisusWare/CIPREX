using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Escala el Canvas por el mismo factor entero que la Pixel Perfect Camera (×4 a 720p, ×6 a 1080p…),
    /// para que cada píxel del HUD mida lo mismo que un píxel del juego (sección 15.6).
    /// El Canvas Scaler debe estar en Constant Pixel Size; el layout se diseña a 320×180.
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    [ExecuteAlways]
    public class PixelPerfectCanvasScaler : MonoBehaviour
    {
        [SerializeField] Vector2Int referenceResolution = new(320, 180);

        CanvasScaler scaler;

        void OnEnable()
        {
            scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            Apply();
        }

        void Update() => Apply();

        void Apply()
        {
            if (scaler == null) return;
            int scale = Mathf.Max(1, Mathf.Min(Screen.width / referenceResolution.x, Screen.height / referenceResolution.y));
            if (!Mathf.Approximately(scaler.scaleFactor, scale)) scaler.scaleFactor = scale;
        }
    }
}
