using System;

namespace Game.Core
{
    /// <summary>
    /// Máquina de estados genérica (sección 6.1). Las transiciones viven dentro de cada estado;
    /// quien la posee solo llama a Tick y FixedTick.
    /// </summary>
    public class StateMachine
    {
        public IState Current { get; private set; }
        public IState Previous { get; private set; }

        /// <summary>Se lanza después de cada cambio: (anterior, nuevo).</summary>
        public event Action<IState, IState> Changed;

        public void ChangeState(IState next)
        {
            if (next == null || next == Current) return;
            Current?.Exit();
            Previous = Current;
            Current = next;
            Current.Enter();
            Changed?.Invoke(Previous, Current);
        }

        public void Tick(float dt) => Current?.Tick(dt);
        public void FixedTick(float dt) => Current?.FixedTick(dt);
    }
}
