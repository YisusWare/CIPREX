using System;
using System.IO;
using UnityEngine;

namespace Game.Save
{
    /// <summary>Resultado de cargar un slot.</summary>
    public enum SlotStatus
    {
        /// <summary>Se cargó bien.</summary>
        Ok,
        /// <summary>No hay partida en ese slot.</summary>
        Empty,
        /// <summary>El archivo principal estaba dañado; se cargó el respaldo (.bak).</summary>
        RecoveredFromBackup,
        /// <summary>Ni el archivo ni el respaldo se pudieron leer. No se debe sobrescribir sin avisar.</summary>
        Corrupted,
        /// <summary>Es de una versión del juego más nueva. No se carga ni se sobrescribe.</summary>
        TooNew,
    }

    /// <summary>
    /// Lectura y escritura de archivos de guardado (sección 15.4): rutas, escritura segura con respaldo,
    /// carga con respaldo, versiones y borrado de slots. No guarda estado: la partida en memoria la tiene
    /// GameManager. Los slots se numeran desde 1 (slot_1.json … slot_3.json).
    /// </summary>
    public static class SaveSystem
    {
        /// <summary>Versión actual del formato. Al subirla, agregar el paso en SaveMigrator.</summary>
        public const int CurrentVersion = 1;
        public const int SlotCount = 3;

        /// <summary>%USERPROFILE%\AppData\LocalLow\YisusWare\CIPREX\ en Windows.</summary>
        public static string Folder => Application.persistentDataPath;
        public static string SlotPath(int slot) => Path.Combine(Folder, $"slot_{slot}.json");
        public static string SettingsPath => Path.Combine(Folder, "settings.json");

        [Serializable]
        class VersionProbe { public int version; }

        // ------------------------------------------------------------------ Slots

        public static bool SlotExists(int slot) =>
            File.Exists(SlotPath(slot)) || File.Exists(SlotPath(slot) + ".bak");

        /// <summary>Carga un slot. Si el archivo está dañado, intenta con el respaldo.</summary>
        public static SlotStatus LoadSlot(int slot, out SaveData data)
        {
            data = null;
            string path = SlotPath(slot);
            string backup = path + ".bak";
            if (!File.Exists(path) && !File.Exists(backup)) return SlotStatus.Empty;

            var main = TryRead(path, out data);
            if (main == ReadResult.Ok) return SlotStatus.Ok;
            if (main == ReadResult.TooNew) return SlotStatus.TooNew;

            Debug.LogWarning($"[Guardado] No se pudo leer {path}; se intenta con el respaldo.");
            var bak = TryRead(backup, out data);
            if (bak == ReadResult.Ok) return SlotStatus.RecoveredFromBackup;
            if (bak == ReadResult.TooNew) return SlotStatus.TooNew;

            Debug.LogError($"[Guardado] El slot {slot} está dañado (ni {Path.GetFileName(path)} ni el respaldo se pueden leer).");
            data = null;
            return SlotStatus.Corrupted;
        }

        /// <summary>Escribe el slot de forma segura. Actualiza lastSavedAt y version.</summary>
        public static bool SaveSlot(int slot, SaveData data)
        {
            if (data == null) return false;
            data.version = CurrentVersion;
            data.lastSavedAt = DateTime.UtcNow.ToString("o");
            if (string.IsNullOrEmpty(data.createdAt)) data.createdAt = data.lastSavedAt;
            return WriteSafe(SlotPath(slot), JsonUtility.ToJson(data, true));
        }

        /// <summary>Borra el slot, su respaldo y su temporal.</summary>
        public static void DeleteSlot(int slot)
        {
            string path = SlotPath(slot);
            foreach (var p in new[] { path, path + ".bak", path + ".tmp" })
            {
                try { if (File.Exists(p)) File.Delete(p); }
                catch (Exception e) { Debug.LogError($"[Guardado] No se pudo borrar {p}: {e.Message}"); }
            }
        }

        // ------------------------------------------------------------------ Opciones

        public static SettingsData LoadSettings()
        {
            string path = SettingsPath;
            if (!File.Exists(path)) return new SettingsData();
            try
            {
                var settings = JsonUtility.FromJson<SettingsData>(File.ReadAllText(path));
                if (settings != null) return settings;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Guardado] settings.json dañado, se usan las opciones por defecto: {e.Message}");
            }
            return new SettingsData();
        }

        public static bool SaveSettings(SettingsData settings) =>
            settings != null && WriteSafe(SettingsPath, JsonUtility.ToJson(settings, true));

        // ------------------------------------------------------------------ Escritura y lectura

        /// <summary>
        /// Escritura segura: primero a .tmp y después File.Replace, que deja la versión anterior como .bak.
        /// Si el juego se cierra a mitad, queda el archivo anterior intacto.
        /// </summary>
        static bool WriteSafe(string path, string json)
        {
            string tmp = path + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(tmp, json);
                if (File.Exists(path)) File.Replace(tmp, path, path + ".bak");
                else File.Move(tmp, path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Guardado] No se pudo escribir {path}: {e.Message}");
                try { if (File.Exists(tmp)) File.Delete(tmp); } catch { /* ya se avisó */ }
                return false;
            }
        }

        enum ReadResult { Ok, Missing, Invalid, TooNew }

        static ReadResult TryRead(string path, out SaveData data)
        {
            data = null;
            if (!File.Exists(path)) return ReadResult.Missing;
            try
            {
                string json = File.ReadAllText(path);
                var probe = JsonUtility.FromJson<VersionProbe>(json);
                if (probe == null || probe.version <= 0) return ReadResult.Invalid;
                if (probe.version > CurrentVersion)
                {
                    Debug.LogWarning($"[Guardado] {Path.GetFileName(path)} es de la versión {probe.version} del formato " +
                                     $"(este juego usa la {CurrentVersion}). No se carga ni se sobrescribe.");
                    return ReadResult.TooNew;
                }

                data = JsonUtility.FromJson<SaveData>(json);
                if (data == null) return ReadResult.Invalid;
                data.completedMissions ??= new System.Collections.Generic.List<string>();
                data.characters ??= new System.Collections.Generic.List<CharacterProgress>();
                data.stats ??= new SaveStats();
                if (!SaveMigrator.Migrate(data, json)) { data = null; return ReadResult.Invalid; }
                return ReadResult.Ok;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Guardado] {Path.GetFileName(path)} no se puede leer: {e.Message}");
                data = null;
                return ReadResult.Invalid;
            }
        }
    }
}
