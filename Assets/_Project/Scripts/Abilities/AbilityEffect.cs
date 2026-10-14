using Game.Player;
using UnityEngine;

namespace Game.Abilities
{
    /// <summary>Lo que el efecto necesita saber del jugador en cada frame (sección 8.4).</summary>
    public struct AbilityContext
    {
        public PlayerController owner;
        /// <summary>FirePoint según la dirección de apuntado.</summary>
        public Vector2 origin;
        /// <summary>Normalizada.</summary>
        public Vector2 direction;
        public AimDirection aim;
        public int facing;
        public bool crouching;
    }

    /// <summary>
    /// Comportamiento de una habilidad (sección 8.4). Cada habilidad es un prefab con un componente que
    /// hereda de esta clase; la FSM no sabe nada de cada habilidad.
    /// PlayerAbilities crea UNA instancia del prefab por habilidad, como hijo del jugador. Las habilidades
    /// que lanzan proyectiles (granada, taladro) los sacan de su propio pool desde esa instancia.
    /// El efecto decide cuándo termina (IsFinished), no la FSM.
    /// </summary>
    public abstract class AbilityEffect : MonoBehaviour
    {
        public AbilityData Data { get; private set; }
        public PlayerController Owner { get; private set; }

        /// <summary>Lo llama PlayerAbilities al crear la instancia.</summary>
        public virtual void Setup(AbilityData data, PlayerController owner)
        {
            Data = data;
            Owner = owner;
        }

        /// <summary>Al activar la habilidad.</summary>
        public abstract void Begin(AbilityContext ctx);

        /// <summary>Cada frame mientras UsingAbility está activo, con el contexto actualizado.</summary>
        public virtual void Sustain(AbilityContext ctx, float dt) { }

        /// <summary>Cuando el jugador suelta el botón de habilidad (una vez por uso).</summary>
        public virtual void ButtonReleased() { }

        /// <summary>UsingAbility sale cuando esto es verdadero.</summary>
        public abstract bool IsFinished { get; }

        /// <summary>Al terminar o al cancelarse (energía agotada, daño o dash). Se llama siempre una vez por uso.</summary>
        public virtual void End() { }

        /// <summary>Instancias activas en pantalla (para AbilityData.maxInstancesOnScreen). Por defecto 0.</summary>
        public virtual int ActiveInstances => 0;
    }
}
