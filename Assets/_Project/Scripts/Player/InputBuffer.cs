namespace Game.Player
{
    /// <summary>
    /// Recuerda una pulsación durante un tiempo corto (en segundos), para no perderla
    /// si llega unos frames antes de que el estado pueda usarla (jump buffer, etc.).
    /// </summary>
    public class InputBuffer
    {
        float remaining;

        public bool IsActive => remaining > 0f;

        public void Press(float duration) => remaining = duration > 0f ? duration : 0.0001f;

        public void Tick(float dt)
        {
            if (remaining > 0f) remaining -= dt;
        }

        /// <summary>Devuelve true y limpia el buffer si había una pulsación pendiente.</summary>
        public bool Consume()
        {
            if (remaining <= 0f) return false;
            remaining = 0f;
            return true;
        }

        public void Clear() => remaining = 0f;
    }
}
