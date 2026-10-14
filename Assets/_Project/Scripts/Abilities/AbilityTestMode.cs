namespace Game.Abilities
{
    /// <summary>
    /// Modo de prueba (solo en el editor, Hito 4): todas las habilidades del personaje empiezan
    /// desbloqueadas. Se activa o desactiva en el menú Game → Modo de prueba → Todas las habilidades.
    /// En una build siempre está apagado.
    /// </summary>
    public static class AbilityTestMode
    {
        public const string PrefKey = "CIPREX.AbilityTestMode";

        public static bool Enabled
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetBool(PrefKey, true);
#else
                return false;
#endif
            }
            set
            {
#if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetBool(PrefKey, value);
#endif
            }
        }
    }
}
