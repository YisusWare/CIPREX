using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// SOLO PARA DEPURAR (hitos 1 y 2): voltea el Visual, pinta el placeholder según el estado,
    /// hace parpadear los i-frames, dibuja la dirección de apuntado y muestra datos en pantalla.
    /// Se reemplaza por PlayerAnimation en el Hito 3.
    /// </summary>
    public class PlayerDebugView : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        [SerializeField] Transform visual;
        [SerializeField] SpriteRenderer body;
        [SerializeField] Sprite standingSprite;
        [SerializeField] Sprite crouchingSprite;
        [Tooltip("Indicador del cañón: se coloca en el FirePoint de la dirección de apuntado.")]
        [SerializeField] Transform aimMarker;
        [SerializeField] bool showOverlay = true;
        [Tooltip("Segundos entre encendido y apagado del parpadeo de i-frames.")]
        [SerializeField] float blinkInterval = 0.05f;

        [Header("Colores por estado")]
        [SerializeField] Color idle = Color.white;
        [SerializeField] Color run = new(0.45f, 1f, 0.45f);
        [SerializeField] Color crouch = new(1f, 0.9f, 0.3f);
        [SerializeField] Color rising = new(0.4f, 0.95f, 1f);
        [SerializeField] Color falling = new(0.45f, 0.6f, 1f);
        [SerializeField] Color dashJump = new(1f, 0.6f, 0.2f);
        [SerializeField] Color dash = new(1f, 0.35f, 1f);
        [SerializeField] Color airDash = new(1f, 0.3f, 0.3f);
        [SerializeField] Color hurt = new(0.6f, 0.1f, 0.1f);
        [SerializeField] Color dead = new(0.15f, 0.15f, 0.15f);

        float fps;

        void LateUpdate()
        {
            if (player == null || player.Context == null) return;
            var loco = player.Locomotion;

            if (visual != null)
                visual.localScale = new Vector3(loco.FacingDir, 1f, 1f);

            if (body != null)
            {
                body.color = ColorFor(loco);
                Sprite s = loco.IsCrouching && crouchingSprite != null ? crouchingSprite : standingSprite;
                if (s != null) body.sprite = s;

                // Parpadeo de i-frames (sección 10.1): se apaga y enciende el SpriteRenderer.
                bool blinkOff = player.Health.IsInvulnerable &&
                                Mathf.FloorToInt(Time.time / blinkInterval) % 2 == 0;
                body.enabled = !blinkOff;
            }

            if (aimMarker != null)
            {
                var action = player.Action;
                bool show = player.IsAlive && !loco.IsDashing &&
                            (action.IsInShootPose || action.AimDir == AimDirection.Up || action.AimDir == AimDirection.DiagUp);
                aimMarker.gameObject.SetActive(show);
                if (show)
                    aimMarker.position = player.Buster.FirePosition(action.AimDir, loco.FacingDir, loco.IsCrouching);
            }

            fps = Mathf.Lerp(fps, 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f), 0.1f);
        }

        Color ColorFor(LocomotionFSM loco)
        {
            if (player.Health.IsDead) return dead;
            if (player.IsHurt) return hurt;

            var c = loco.Current;
            if (c == loco.Idle) return idle;
            if (c == loco.Run) return run;
            if (c == loco.Crouch) return crouch;
            if (c == loco.Dash) return dash;
            if (c == loco.AirDash) return airDash;
            if (c == loco.Airborne)
            {
                if (loco.CarriesDashMomentum) return dashJump;
                return player.Motor.Velocity.y > 0f ? rising : falling;
            }
            return Color.gray;
        }

        void OnGUI()
        {
            if (!showOverlay || player == null || player.Context == null) return;
            var loco = player.Locomotion;
            var action = player.Action;
            var health = player.Health;
            Vector2 v = player.Motor.Velocity;
            string text =
                $"Vida: {health.Current}/{health.Max}{(health.IsInvulnerable ? "  (i-frames)" : "")}   [{player.LifeStateName}]\n" +
                $"Locomoción: {loco.CurrentName}   Acción: {action.CurrentName}\n" +
                $"Apunta: {action.AimDir}   Mira: {(loco.FacingDir > 0 ? "→" : "←")}\n" +
                $"Vel: ({v.x:0.00}, {v.y:0.00})   Suelo: {loco.IsGrounded}\n" +
                $"Air dashes: {loco.AirDashesLeft}   Impulso dash: {loco.CarriesDashMomentum}\n" +
                $"Pos: ({player.transform.position.x:0.00}, {player.transform.position.y:0.00})   FPS: {fps:0}\n" +
                $"Energía: {player.Energy.Current:0.0}/{player.Energy.Max:0}   Habilidad: {(player.Abilities.Selected != null ? player.Abilities.Selected.displayName : "-")}";
            // Arriba a la derecha, para no tapar el HUD.
            float x = Screen.width - 348f;
            GUI.Box(new Rect(x, 8, 340, 128), GUIContent.none);
            GUI.Label(new Rect(x + 8, 12, 330, 124), text);
        }
    }
}
