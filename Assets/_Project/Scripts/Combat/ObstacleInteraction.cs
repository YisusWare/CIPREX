using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Etiqueta de interacción con obstáculos (sección 15.2): Break, Melt, etc. No tiene datos;
    /// solo sirve para que una habilidad diga qué sabe hacer y un obstáculo diga qué pide.
    /// Agregar un tipo nuevo de obstáculo o de interacción es crear un asset, sin código.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Obstacle Interaction", fileName = "Interaction")]
    public class ObstacleInteraction : ScriptableObject
    {
    }
}
