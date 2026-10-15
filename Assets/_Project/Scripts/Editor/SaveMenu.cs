using System.IO;
using System.Linq;
using Game.Abilities;
using Game.Player;
using Game.Save;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>Herramientas de guardado del editor (sección 15.4). Menú: Game/Save.</summary>
    public static class SaveMenu
    {
        [MenuItem("Game/Save/Abrir carpeta de guardado", priority = 0)]
        static void OpenFolder()
        {
            Directory.CreateDirectory(SaveSystem.Folder);
            EditorUtility.OpenWithDefaultApp(SaveSystem.Folder);
        }

        [MenuItem("Game/Save/Mostrar los slots en la consola", priority = 1)]
        static void LogSlots()
        {
            for (int slot = 1; slot <= SaveSystem.SlotCount; slot++)
            {
                var status = SaveSystem.LoadSlot(slot, out var data);
                if (data == null)
                {
                    Debug.Log($"[Guardado] Slot {slot}: {status}");
                    continue;
                }
                Debug.Log($"[Guardado] Slot {slot}: {status} · {data.completedMissions.Count} misiones · " +
                          $"{data.playTimeSeconds / 60f:0.0} min · guardado {data.lastSavedAt}\n{JsonUtility.ToJson(data, true)}");
            }
        }

        [MenuItem("Game/Save/Crear guardado de prueba (slot 1)", priority = 20)]
        static void CreateTestSave()
        {
            if (SaveSystem.SlotExists(1) &&
                !EditorUtility.DisplayDialog("Guardado", "El slot 1 ya tiene una partida. ¿Reemplazarla?", "Reemplazar", "Cancelar"))
                return;

            var data = SaveData.CreateNew();
            foreach (var guid in AssetDatabase.FindAssets("t:CharacterData"))
            {
                var character = AssetDatabase.LoadAssetAtPath<CharacterData>(AssetDatabase.GUIDToAssetPath(guid));
                if (character == null || string.IsNullOrEmpty(character.id)) continue;
                var progress = data.GetOrCreateCharacter(character.id);
                foreach (var ability in character.abilities ?? new AbilityData[0])
                    if (ability != null && !string.IsNullOrEmpty(ability.id)) progress.unlockedAbilities.Add(ability.id);
                progress.selectedAbility = progress.unlockedAbilities.FirstOrDefault();
                data.lastCharacter ??= character.id;
            }
            // Las misiones completadas se agregan cuando exista MissionCatalog (Hito 6).

            if (SaveSystem.SaveSlot(1, data))
                Debug.Log($"[Guardado] Guardado de prueba creado en {SaveSystem.SlotPath(1)}: todas las habilidades de " +
                          $"{data.characters.Count} personaje(s). Desactiva el modo de prueba de habilidades para usarlo.");
        }

        [MenuItem("Game/Save/Cargar un JSON en el slot 1…", priority = 21)]
        static void ImportJson()
        {
            string path = EditorUtility.OpenFilePanel("JSON de una partida", SaveSystem.Folder, "json");
            if (string.IsNullOrEmpty(path)) return;

            string json = File.ReadAllText(path);
            SaveData data;
            try { data = JsonUtility.FromJson<SaveData>(json); }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Guardado", $"El archivo no es una partida válida:\n{e.Message}", "OK");
                return;
            }
            if (data == null)
            {
                EditorUtility.DisplayDialog("Guardado", "El archivo está vacío.", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Guardado", "Se copiará el archivo como slot_1.json (el actual queda como .bak). ¿Continuar?", "Copiar", "Cancelar"))
                return;

            // Se copia tal cual (sin pasar por SaveSlot) para reproducir el archivo exacto, versión incluida.
            Directory.CreateDirectory(SaveSystem.Folder);
            string target = SaveSystem.SlotPath(1);
            if (File.Exists(target)) File.Copy(target, target + ".bak", true);
            File.Copy(path, target, true);
            Debug.Log($"[Guardado] {Path.GetFileName(path)} copiado a {target}.");
        }

        [MenuItem("Game/Save/Borrar todos los guardados", priority = 40)]
        static void DeleteAll()
        {
            if (!EditorUtility.DisplayDialog("Guardado",
                    $"Se borrarán los {SaveSystem.SlotCount} slots, sus respaldos y settings.json de:\n{SaveSystem.Folder}",
                    "Borrar", "Cancelar"))
                return;
            for (int slot = 1; slot <= SaveSystem.SlotCount; slot++) SaveSystem.DeleteSlot(slot);
            foreach (var p in new[] { SaveSystem.SettingsPath, SaveSystem.SettingsPath + ".bak", SaveSystem.SettingsPath + ".tmp" })
                if (File.Exists(p)) File.Delete(p);
            Debug.Log("[Guardado] Guardados borrados." +
                      (Application.isPlaying ? " La partida en memoria sigue hasta salir de Play." : ""));
        }

        // ------------------------------------------------------------------ En Play

        [MenuItem("Game/Save/Desbloquear la siguiente habilidad (Play)", priority = 60)]
        static void UnlockNext()
        {
            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player == null || player.Abilities == null) return;
            var next = player.Abilities.All.FirstOrDefault(a => !player.Abilities.IsUnlocked(a));
            if (next == null)
            {
                Debug.Log("[Guardado] El personaje ya tiene todas sus habilidades.");
                return;
            }
            player.Abilities.Unlock(next); // igual que un coleccionable: GameManager guarda en el acto
            Debug.Log($"[Guardado] Desbloqueada: {next.displayName} ({next.id}).");
        }

        [MenuItem("Game/Save/Desbloquear la siguiente habilidad (Play)", true)]
        static bool UnlockNextValidate() => Application.isPlaying && !AbilityTestMode.Enabled;
    }
}
