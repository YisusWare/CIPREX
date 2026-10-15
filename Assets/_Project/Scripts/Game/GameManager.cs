using System;
using Game.Abilities;
using Game.Player;
using Game.Save;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Tiene la partida actual en memoria (SaveData) y pide a SaveSystem que guarde en los momentos
    /// definidos (sección 15.4). Los sistemas del juego leen y modifican el progreso a través de aquí,
    /// nunca leen archivos.
    ///
    /// Hito 4: solo el progreso de habilidades y lo básico de la partida. Se crea solo la primera vez que
    /// se usa y vive entre escenas. Hasta que existan la pantalla de slots y la escena Boot (Hito 6),
    /// al jugar se carga el último slot usado (o el 1) y, si está vacío, empieza una partida nueva.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        static GameManager instance;

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("GameManager");
                    DontDestroyOnLoad(go);
                    instance = go.AddComponent<GameManager>();
                }
                return instance;
            }
        }

        /// <summary>Lo usan el HUD (icono de guardado) y las herramientas. Static para no crear el GameManager al suscribirse.</summary>
        public static event Action Saved;

        /// <summary>Slot cargado (desde 1). 0 = ninguno.</summary>
        public int CurrentSlot { get; private set; }
        public SaveData Data { get; private set; }
        public SlotStatus Status { get; private set; }
        public SettingsData Settings { get; private set; }

        /// <summary>
        /// Slot dañado o de una versión más nueva: se juega con una partida en memoria, pero no se
        /// escribe nada para no destruir el archivo sin avisar (15.4).
        /// </summary>
        public bool IsReadOnly { get; private set; }

        // Reinicia el estado estático al entrar a Play sin recargar el dominio.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            instance = null;
            Saved = null;
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            Settings = SaveSystem.LoadSettings();
        }

        void Update()
        {
            // El tiempo de juego cuenta también en pausa (Time.unscaledDeltaTime).
            if (Data != null) Data.playTimeSeconds += Time.unscaledDeltaTime;
        }

        void OnApplicationQuit()
        {
            // Solo se guarda el tiempo de juego; el estado de la misión no se guarda (15.4).
            if (Data != null) Save();
        }

        // ------------------------------------------------------------------ Slots

        /// <summary>Carga un slot. Si está vacío, deja Data en null (usar NewGame).</summary>
        public SlotStatus LoadSlot(int slot)
        {
            CurrentSlot = slot;
            Status = SaveSystem.LoadSlot(slot, out var data);
            Data = data;
            IsReadOnly = Status is SlotStatus.Corrupted or SlotStatus.TooNew;

            if (IsReadOnly)
            {
                Debug.LogError($"[Guardado] El slot {slot} no se puede usar ({Status}). Se juega sin guardar. " +
                               "Revisa o borra el archivo con Game → Save.");
                Data = SaveData.CreateNew();
            }
            else if (Data != null)
            {
                RememberSlot(slot);
                Debug.Log($"[Guardado] Slot {slot} cargado ({Status}).");
            }
            return Status;
        }

        /// <summary>Partida nueva en el slot (borra la anterior).</summary>
        public void NewGame(int slot)
        {
            CurrentSlot = slot;
            Status = SlotStatus.Ok;
            IsReadOnly = false;
            Data = SaveData.CreateNew();
            RememberSlot(slot);
            Save();
            Debug.Log($"[Guardado] Partida nueva en el slot {slot}.");
        }

        /// <summary>Hasta el Hito 6: carga el último slot usado (o el 1) y, si está vacío, crea una partida.</summary>
        public void EnsureGameLoaded()
        {
            if (Data != null) return;
            int slot = Settings.lastUsedSlot is >= 1 and <= SaveSystem.SlotCount ? Settings.lastUsedSlot : 1;
            if (LoadSlot(slot) == SlotStatus.Empty) NewGame(slot);
        }

        /// <summary>Guarda la partida actual. No hace nada si el slot es de solo lectura.</summary>
        public bool Save()
        {
            if (Data == null || CurrentSlot <= 0) return false;
            if (IsReadOnly)
            {
                Debug.LogWarning($"[Guardado] No se guarda: el slot {CurrentSlot} es de solo lectura ({Status}).");
                return false;
            }
            bool ok = SaveSystem.SaveSlot(CurrentSlot, Data);
            if (ok) Saved?.Invoke();
            return ok;
        }

        void RememberSlot(int slot)
        {
            if (Settings.lastUsedSlot == slot) return;
            Settings.lastUsedSlot = slot;
            SaveSystem.SaveSettings(Settings);
        }

        // ------------------------------------------------------------------ Progreso

        public CharacterProgress GetCharacter(string characterId)
        {
            EnsureGameLoaded();
            return Data.GetOrCreateCharacter(characterId);
        }

        /// <summary>Desbloquea una habilidad del personaje y guarda en el acto (coleccionable, 15.4).</summary>
        public void UnlockAbility(CharacterData character, AbilityData ability)
        {
            if (character == null || ability == null || string.IsNullOrEmpty(ability.id)) return;
            var progress = GetCharacter(character.id);
            if (!progress.unlockedAbilities.Contains(ability.id)) progress.unlockedAbilities.Add(ability.id);
            progress.selectedAbility = ability.id;
            Save();
        }

        /// <summary>Misión completada (Hito 6): se guarda junto con el último personaje usado.</summary>
        public void CompleteMission(string missionId, CharacterData character)
        {
            EnsureGameLoaded();
            if (!string.IsNullOrEmpty(missionId) && !Data.completedMissions.Contains(missionId))
                Data.completedMissions.Add(missionId);
            if (character != null) Data.lastCharacter = character.id;
            Save();
        }

        public bool IsMissionCompleted(string missionId) =>
            Data != null && Data.completedMissions.Contains(missionId);

        // ------------------------------------------------------------------ Jugador

        /// <summary>
        /// Aplica el progreso guardado al jugador recién creado y escucha sus cambios: habilidades
        /// desbloqueadas (se guarda en el acto), habilidad seleccionada y muertes (en memoria, se escriben
        /// en el siguiente guardado). Con el modo de prueba de habilidades, no se lee ni se escribe el
        /// progreso de habilidades, para no mezclar las de prueba con las de la partida.
        /// </summary>
        public void BindPlayer(PlayerController player)
        {
            if (player == null || player.Character == null) return;
            EnsureGameLoaded();
            var character = player.Character;
            var progress = Data.GetOrCreateCharacter(character.id);
            Data.lastCharacter = character.id;
            bool testMode = AbilityTestMode.Enabled;

            if (!testMode)
            {
                foreach (var id in progress.unlockedAbilities)
                    if (player.Abilities.FindById(id) == null)
                        Debug.LogWarning($"[Guardado] {character.id} tiene desbloqueada '{id}', que no está en su CharacterData. Se ignora.");
                player.Abilities.SetUnlockedIds(progress.unlockedAbilities);

                var saved = player.Abilities.FindById(progress.selectedAbility);
                if (saved != null && player.Abilities.IsUnlocked(saved)) player.Abilities.Select(saved);
            }

            player.Abilities.AbilityUnlocked += ability =>
            {
                if (!AbilityTestMode.Enabled) UnlockAbility(character, ability);
            };
            player.Abilities.SelectedChanged += ability =>
            {
                if (!AbilityTestMode.Enabled && ability != null) progress.selectedAbility = ability.id;
            };
            player.Health.Died += () => Data.stats.deaths++;
        }
    }
}
