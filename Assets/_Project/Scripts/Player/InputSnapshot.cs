namespace Game.Player
{
    /// <summary>
    /// Estado del input en el frame actual (sección 12). Lo llena PlayerInputReader;
    /// los estados solo leen de aquí, nunca del Input System.
    /// </summary>
    public class InputSnapshot
    {
        /// <summary>Dirección discreta: -1, 0 o 1.</summary>
        public int MoveX;
        /// <summary>Dirección discreta: -1 (abajo), 0 o 1 (arriba).</summary>
        public int MoveY;

        public bool JumpHeld;
        public bool DashHeld;
        public bool FireHeld;
        public bool AbilityHeld;

        public readonly InputBuffer Jump = new();
        public readonly InputBuffer Dash = new();
        public readonly InputBuffer Fire = new();
        public readonly InputBuffer Ability = new();

        /// <summary>Solo verdaderos en el frame en que se presionan.</summary>
        public bool AbilityNextPressed;
        public bool AbilityPrevPressed;
        public bool PausePressed;

        public void ClearBuffers()
        {
            Jump.Clear();
            Dash.Clear();
            Fire.Clear();
            Ability.Clear();
        }
    }
}
