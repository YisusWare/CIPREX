using UnityEngine;

namespace Game.Abilities
{
    /// <summary>
    /// Obstáculo que reacciona a las habilidades (sección 15.2): bloques destructibles, muros de acero.
    /// Lo implementa AbilityObstacleTilemap en el Hito 5. Los efectos ya lo llaman al tocar una pared,
    /// así que el Hito 5 no necesita cambiar las habilidades.
    /// </summary>
    public interface IAbilityObstacle
    {
        /// <summary>
        /// La habilidad toca el obstáculo en hitPoint, viajando en direction. dt es el tiempo de contacto
        /// (calor acumulado del lanzallamas; 0 para los golpes instantáneos del taladro).
        /// Devuelve true si la habilidad tiene la interacción que pide el obstáculo.
        /// </summary>
        bool TryInteract(Vector2 hitPoint, Vector2 direction, AbilityData ability, float dt);
    }

    public static class AbilityObstacles
    {
        /// <summary>Si el collider es un obstáculo y la habilidad tiene alguna interacción, se la pasa.</summary>
        public static bool TryInteract(Collider2D collider, Vector2 hitPoint, Vector2 direction, AbilityData ability, float dt)
        {
            if (collider == null || ability == null || ability.obstacleInteractions == null ||
                ability.obstacleInteractions.Length == 0)
                return false;
            var obstacle = collider.GetComponentInParent<IAbilityObstacle>();
            return obstacle != null && obstacle.TryInteract(hitPoint, direction, ability, dt);
        }
    }
}
