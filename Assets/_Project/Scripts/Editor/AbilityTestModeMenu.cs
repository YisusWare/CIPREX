using Game.Abilities;
using UnityEditor;

namespace Game.EditorTools
{
    /// <summary>Menú del modo de prueba de habilidades (Hito 4): todas desbloqueadas al entrar a Play.</summary>
    public static class AbilityTestModeMenu
    {
        const string MenuPath = "Game/Modo de prueba/Todas las habilidades";

        [MenuItem(MenuPath, priority = 100)]
        static void Toggle() => AbilityTestMode.Enabled = !AbilityTestMode.Enabled;

        [MenuItem(MenuPath, true)]
        static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, AbilityTestMode.Enabled);
            return true;
        }
    }
}
