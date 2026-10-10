using Game.Combat;
using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Muñeco de práctica (Hito 2): recibe daño sin morir, hace flash y muestra el daño acumulado.
    /// El contador se reinicia después de unos segundos sin golpes.
    /// </summary>
    public class PracticeDummy : MonoBehaviour, IDamageable
    {
        [SerializeField] DamageFlash flash;
        [SerializeField] float resetAfter = 2f;
        [Tooltip("Altura de la etiqueta sobre el pivote, en unidades.")]
        [SerializeField] float labelHeight = 2.4f;

        int totalDamage;
        int hits;
        float lastHitTime = float.NegativeInfinity;
        float firstHitTime;

        public void TakeDamage(DamageInfo info)
        {
            if (Time.time - lastHitTime > resetAfter)
            {
                totalDamage = 0;
                hits = 0;
                firstHitTime = Time.time;
            }
            totalDamage += info.amount;
            hits++;
            lastHitTime = Time.time;
            if (flash != null) flash.Flash();
        }

        void OnGUI()
        {
            var cam = Camera.main;
            if (cam == null || hits == 0) return;

            Vector3 screen = cam.WorldToScreenPoint(transform.position + Vector3.up * labelHeight);
            if (screen.z < 0f) return;

            float elapsed = Mathf.Max(lastHitTime - firstHitTime, 0.0001f);
            float dps = hits > 1 ? totalDamage / elapsed : 0f;
            string text = $"Daño: {totalDamage}  Golpes: {hits}\nDPS: {dps:0.0}";
            var rect = new Rect(screen.x - 70f, Screen.height - screen.y - 20f, 140f, 40f);
            GUI.Box(rect, GUIContent.none);
            GUI.Label(new Rect(rect.x + 6f, rect.y + 2f, rect.width - 6f, rect.height), text);
        }
    }
}
