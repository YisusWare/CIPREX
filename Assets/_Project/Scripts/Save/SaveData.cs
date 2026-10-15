using System;
using System.Collections.Generic;

namespace Game.Save
{
    /// <summary>
    /// Progreso de una partida (un slot, sección 15.4). Solo datos: sin lógica ni referencias a objetos
    /// de Unity. Todo va por id (MissionData.id, AbilityData.id, CharacterData.id), nunca por índice.
    /// Se serializa con JsonUtility, que no admite Dictionary: por eso se usan listas.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version = SaveSystem.CurrentVersion;
        public string createdAt;
        public string lastSavedAt;
        public float playTimeSeconds;
        /// <summary>Del slot, no del personaje: en el lore los dos soldados van a cada misión.</summary>
        public List<string> completedMissions = new();
        /// <summary>CharacterData.id del último personaje usado.</summary>
        public string lastCharacter;
        public List<CharacterProgress> characters = new();
        public SaveStats stats = new();

        public static SaveData CreateNew() => new() { createdAt = DateTime.UtcNow.ToString("o") };

        /// <summary>El progreso del personaje; si no tiene entrada, la crea vacía.</summary>
        public CharacterProgress GetOrCreateCharacter(string id)
        {
            foreach (var c in characters)
                if (c != null && c.id == id) return c;
            var created = new CharacterProgress { id = id };
            characters.Add(created);
            return created;
        }
    }

    /// <summary>Las habilidades son de cada personaje, igual que la seleccionada.</summary>
    [Serializable]
    public class CharacterProgress
    {
        public string id;
        public List<string> unlockedAbilities = new();
        public string selectedAbility;
    }

    [Serializable]
    public class SaveStats
    {
        public int deaths;
    }

    /// <summary>Opciones del jugador (settings.json). Independientes de las partidas.</summary>
    [Serializable]
    public class SettingsData
    {
        public int version = SaveSystem.CurrentVersion;
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
        public string fullscreenMode = "FullScreenWindow";
        public int windowScale = 4;
        public bool vSync = true;
        /// <summary>InputActionAsset.SaveBindingOverridesAsJson (Hito 6).</summary>
        public string bindingOverrides = "";
        /// <summary>Para "Continuar" en el menú principal. 0 = ninguno.</summary>
        public int lastUsedSlot;
    }
}
