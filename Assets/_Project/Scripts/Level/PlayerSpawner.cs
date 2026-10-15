using System;
using Game.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Level
{
    /// <summary>
    /// Punto de inicio del jugador (sección 5.4). El jugador no se coloca en la escena: este componente
    /// instancia el prefab del personaje elegido aquí, llama a Initialize y conecta la cámara.
    /// El HUD se conecta solo escuchando PlayerSpawned.
    /// Hasta el Hito 6 no hay selección de personaje: se usa fallbackCharacter.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class PlayerSpawner : MonoBehaviour
    {
        [Tooltip("Personaje que se usa si no hay uno seleccionado (para probar en el editor sin pasar por los menús).")]
        [SerializeField] CharacterData fallbackCharacter;
        [Tooltip("Cámara que sigue al jugador (su CameraTarget).")]
        [SerializeField] CinemachineCamera playerCamera;

        /// <summary>Se lanza después de Initialize. El HUD lo usa para conectarse.</summary>
        public static event Action<PlayerController> PlayerSpawned;
        public PlayerController Player { get; private set; }

        void Awake()
        {
            // En el Hito 6: GameManager.SelectedCharacter, con fallbackCharacter si es null.
            var character = fallbackCharacter;
            if (character == null || character.prefab == null)
            {
                Debug.LogError("PlayerSpawner: falta un CharacterData con prefab.", this);
                return;
            }

            // Instantiate llama a Awake del jugador; Initialize va antes de su Start.
            Player = Instantiate(character.prefab, transform.position, Quaternion.identity);
            Player.name = character.displayName;
            Player.Initialize(character);
            // Progreso guardado (habilidades desbloqueadas) y guardado al desbloquear (15.4).
            GameManager.Instance.BindPlayer(Player);

            if (playerCamera != null)
            {
                var target = Player.transform.Find("CameraTarget");
                playerCamera.Follow = target != null ? target : Player.transform;
            }
        }

        void Start()
        {
            if (Player != null) PlayerSpawned?.Invoke(Player);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 1f, 0.4f, 0.8f);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.9375f, new Vector3(0.75f, 1.875f, 0f));
        }
    }
}
