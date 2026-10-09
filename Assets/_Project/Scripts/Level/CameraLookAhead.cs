using Game.Player;
using UnityEngine;

namespace Game.Level
{
    /// <summary>
    /// Objetivo de la cámara (sección 13): se adelanta hacia donde mira el jugador,
    /// más durante el dash. La Cinemachine Camera sigue a este objeto, no al jugador.
    /// Va en un hijo del jugador.
    /// </summary>
    public class CameraLookAhead : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        [Tooltip("Unidades hacia adelante en estado normal. [AJUSTAR]")]
        [SerializeField] float lookAhead = 1.5f;
        [Tooltip("Unidades hacia adelante durante el dash o con impulso de dash. [AJUSTAR]")]
        [SerializeField] float dashLookAhead = 3f;
        [Tooltip("Altura del objetivo sobre los pies.")]
        [SerializeField] float height = 1f;
        [Tooltip("Velocidad con la que se desplaza el objetivo (u/s).")]
        [SerializeField] float shiftSpeed = 8f;

        float currentX;

        void Start()
        {
            if (player != null && player.Context != null)
                currentX = player.Locomotion.FacingDir * lookAhead;
            transform.localPosition = new Vector3(currentX, height, 0f);
        }

        void Update()
        {
            if (player == null || player.Context == null) return;
            var loco = player.Locomotion;
            bool dashing = loco.IsDashing || loco.CarriesDashMomentum;
            float target = loco.FacingDir * (dashing ? dashLookAhead : lookAhead);
            float speed = dashing ? shiftSpeed * 2f : shiftSpeed;
            currentX = Mathf.MoveTowards(currentX, target, speed * Time.deltaTime);
            transform.localPosition = new Vector3(currentX, height, 0f);
        }
    }
}
