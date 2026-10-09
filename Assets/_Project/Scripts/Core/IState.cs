namespace Game.Core
{
    /// <summary>Estado de una máquina de estados (sección 6.1).</summary>
    public interface IState
    {
        void Enter();
        void Tick(float dt);
        void FixedTick(float dt);
        void Exit();
    }
}
