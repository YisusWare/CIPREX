using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    /// <summary>
    /// Único lugar que lee el Input System (secciones 5.2, 12 y 18.3).
    /// PlayerController llama a Poll() al inicio de cada Update.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] InputActionAsset actions;
        [SerializeField] PlayerMovementData movement;

        [Tooltip("Umbral por eje para convertir el stick en dirección discreta.")]
        [SerializeField, Range(0.1f, 0.9f)] float axisThreshold = 0.4f;
        [SerializeField] float fireBuffer = 0.05f;
        [SerializeField] float abilityBuffer = 0.05f;

        public InputSnapshot Snapshot { get; } = new();

        InputActionMap gameplay;
        InputAction move, jump, fire, ability, dash, abilityNext, abilityPrev, pause;

        void Awake()
        {
            if (actions == null)
            {
                Debug.LogError("PlayerInputReader: falta asignar el InputActionAsset.", this);
                enabled = false;
                return;
            }

            gameplay = actions.FindActionMap("Gameplay", throwIfNotFound: true);
            move = gameplay.FindAction("Move", true);
            jump = gameplay.FindAction("Jump", true);
            fire = gameplay.FindAction("Fire", true);
            ability = gameplay.FindAction("Ability", true);
            dash = gameplay.FindAction("Dash", true);
            abilityNext = gameplay.FindAction("AbilityNext", true);
            abilityPrev = gameplay.FindAction("AbilityPrev", true);
            pause = gameplay.FindAction("Pause", true);
        }

        void OnEnable() => gameplay?.Enable();
        void OnDisable() => gameplay?.Disable();

        /// <summary>Activa o desactiva la lectura (diálogos, cinemáticas). Limpia los buffers.</summary>
        public void SetGameplayEnabled(bool value)
        {
            if (gameplay == null) return;
            if (value) gameplay.Enable();
            else gameplay.Disable();
            Snapshot.ClearBuffers();
        }

        public void Poll()
        {
            if (gameplay == null) return;
            var s = Snapshot;
            float dt = Time.deltaTime;

            s.Jump.Tick(dt);
            s.Dash.Tick(dt);
            s.Fire.Tick(dt);
            s.Ability.Tick(dt);

            Vector2 v = move.ReadValue<Vector2>();
            s.MoveX = Discrete(v.x);
            s.MoveY = Discrete(v.y);

            s.JumpHeld = jump.IsPressed();
            s.DashHeld = dash.IsPressed();
            s.FireHeld = fire.IsPressed();
            s.AbilityHeld = ability.IsPressed();

            float jumpBuf = movement != null ? movement.jumpBuffer : 0.1f;
            float dashBuf = movement != null ? movement.dashBuffer : 0.1f;
            if (jump.WasPressedThisFrame()) s.Jump.Press(jumpBuf);
            if (dash.WasPressedThisFrame()) s.Dash.Press(dashBuf);
            if (fire.WasPressedThisFrame()) s.Fire.Press(fireBuffer);
            if (ability.WasPressedThisFrame()) s.Ability.Press(abilityBuffer);

            s.AbilityNextPressed = abilityNext.WasPressedThisFrame();
            s.AbilityPrevPressed = abilityPrev.WasPressedThisFrame();
            s.PausePressed = pause.WasPressedThisFrame();
        }

        int Discrete(float value)
        {
            if (value >= axisThreshold) return 1;
            if (value <= -axisThreshold) return -1;
            return 0;
        }
    }
}
