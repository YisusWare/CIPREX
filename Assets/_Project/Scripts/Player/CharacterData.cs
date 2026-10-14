using Game.Abilities;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Lo que distingue a un personaje (sección 5.4). Todos usan los mismos componentes, la misma
    /// jerarquía y las mismas FSM; PlayerController.Initialize reparte estos datos.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Character", fileName = "Character")]
    public class CharacterData : ScriptableObject
    {
        [Tooltip("Clave para el guardado. No cambiarla después de tener partidas guardadas.")]
        public string id;
        public string displayName;
        [TextArea] public string description;
        [Tooltip("Imagen de la pantalla de selección (Hito 6).")]
        public Sprite selectPortrait;
        // SpeakerData (nombre y retrato en los diálogos) se agrega con el sistema de escenas (Hito 6).
        [Tooltip("Prefab del personaje: su arte, Animators y FirePoints.")]
        public PlayerController prefab;

        [Header("Estadísticas")]
        public PlayerMovementData movement;
        [Min(1)] public int maxHealth = 28;
        [Min(0f)] public float maxEnergy = 28f;

        [Header("Ataque")]
        [Tooltip("Disparo básico (sección 7).")]
        public BusterData buster;
        [Tooltip("Habilidades del personaje, en el orden del ciclo siguiente/anterior (8.1).")]
        public AbilityData[] abilities;
        [Tooltip("Solo en el editor, con el modo de prueba activado: habilidades extra para probarlas con este " +
                 "personaje (Hito 4: el taladro y el láser de Veyron se prueban con Rosslyn).")]
        public AbilityData[] testModeAbilities;
    }
}
