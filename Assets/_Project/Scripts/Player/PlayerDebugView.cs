using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// SOLO PARA DEPURAR (Hito 1): voltea el Visual, pinta el placeholder según el estado
    /// de locomoción y muestra datos en pantalla. Se reemplaza por PlayerAnimation en el Hito 3.
    /// </summary>
    public class PlayerDebugView : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        [SerializeField] Transform visual;
        [SerializeField] SpriteRenderer body;
        [SerializeField] Sprite standingSprite;
        [SerializeField] Sprite crouchingSprite;
        [SerializeField] bool showOverlay = true;

        [Header("Colores por estado")]
        [SerializeField] Color idle = Color.white;
        [SerializeField] Color run = new(0.45f, 1f, 0.45f);
        [SerializeField] Color crouch = new(1f, 0.9f, 0.3f);
        [SerializeField] Color rising = new(0.4f, 0.95f, 1f);
        [SerializeField] Color falling = new(0.45f, 0.6f, 1f);
        [SerializeField] Color dashJump = new(1f, 0.6f, 0.2f);
        [SerializeField] Color dash = new(1f, 0.35f, 1f);
        [SerializeField] Color airDash = new(1f, 0.3f, 0.3f);

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
            }

            fps = Mathf.Lerp(fps, 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f), 0.1f);
        }

        Color ColorFor(LocomotionFSM loco)
        {
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
            Vector2 v = player.Motor.Velocity;
            string text =
                $"Estado: {loco.CurrentName}\n" +
                $"Vel: ({v.x:0.00}, {v.y:0.00})\n" +
                $"Suelo: {loco.IsGrounded}   Mira: {(loco.FacingDir > 0 ? "→" : "←")}\n" +
                $"Air dashes: {loco.AirDashesLeft}   Impulso dash: {loco.CarriesDashMomentum}\n" +
                $"Pos: ({player.transform.position.x:0.00}, {player.transform.position.y:0.00})\n" +
                $"FPS: {fps:0}";
            GUI.Box(new Rect(8, 8, 300, 110), GUIContent.none);
            GUI.Label(new Rect(16, 12, 290, 106), text);
        }
    }
}
