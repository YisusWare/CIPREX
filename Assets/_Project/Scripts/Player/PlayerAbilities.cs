using System;
using System.Collections.Generic;
using Game.Abilities;
using Game.Audio;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Habilidades del personaje (sección 5.2 y 8.1): cuáles tiene, cuáles están desbloqueadas, la
    /// seleccionada, el cambio siguiente/anterior, los cooldowns y si se puede usar. Crea una instancia
    /// del efecto de cada habilidad como hijo del jugador. No gasta energía por su cuenta: eso lo hace
    /// el estado UsingAbility con PlayerEnergy.
    /// </summary>
    public class PlayerAbilities : MonoBehaviour
    {
        [SerializeField] SfxData switchSfx;
        [Tooltip("Suena si se intenta usar una habilidad sin energía suficiente (8.6).")]
        [SerializeField] SfxData noEnergySfx;

        readonly List<AbilityData> all = new();
        readonly HashSet<AbilityData> unlocked = new();
        readonly Dictionary<AbilityData, AbilityEffect> effects = new();
        readonly Dictionary<AbilityData, float> cooldownUntil = new();

        PlayerController owner;
        PlayerEnergy energy;

        /// <summary>Todas las habilidades del personaje, en el orden del ciclo.</summary>
        public IReadOnlyList<AbilityData> All => all;
        /// <summary>La seleccionada. Null si no hay ninguna desbloqueada.</summary>
        public AbilityData Selected { get; private set; }
        public bool HasAnyUnlocked => unlocked.Count > 0;

        /// <summary>Para el HUD: icono y nombre.</summary>
        public event Action<AbilityData> SelectedChanged;
        public event Action<AbilityData> AbilityUnlocked;

        void Awake() => owner = GetComponent<PlayerController>();

        /// <summary>
        /// Lo llama PlayerController.Initialize con las habilidades del CharacterData. extra son las
        /// habilidades del modo de prueba (null fuera de él); van al final del ciclo.
        /// </summary>
        public void SetAbilities(AbilityData[] abilities, AbilityData[] extra, PlayerEnergy playerEnergy)
        {
            energy = playerEnergy;
            all.Clear();
            foreach (var list in new[] { abilities, extra })
            {
                if (list == null) continue;
                foreach (var a in list)
                    if (a != null && !all.Contains(a)) all.Add(a);
            }

            foreach (var a in all)
            {
                if (a.effectPrefab == null)
                {
                    Debug.LogWarning($"PlayerAbilities: '{a.name}' no tiene effectPrefab; no se podrá usar.", this);
                    continue;
                }
                var effect = Instantiate(a.effectPrefab, transform);
                effect.name = $"Ability_{a.name}";
                effect.Setup(a, owner);
                effects[a] = effect;
            }

            // Modo de prueba: todo desbloqueado. Si no, el progreso guardado lo aplica GameManager.BindPlayer.
            if (AbilityTestMode.Enabled)
                foreach (var a in all) unlocked.Add(a);

            SelectFirstUnlocked(notify: true);
        }

        public bool IsUnlocked(AbilityData ability) => ability != null && unlocked.Contains(ability);

        /// <summary>La habilidad del personaje con ese id (guardado), o null.</summary>
        public AbilityData FindById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var a in all)
                if (a.id == id) return a;
            return null;
        }

        /// <summary>Desbloquea una habilidad (coleccionable, guardado). Por defecto la deja seleccionada (8.9).</summary>
        public void Unlock(AbilityData ability, bool select = true)
        {
            if (ability == null || !all.Contains(ability) || !unlocked.Add(ability)) return;
            AbilityUnlocked?.Invoke(ability);
            if (select || Selected == null) Select(ability);
        }

        /// <summary>Carga el progreso guardado (ids de AbilityData). Ignora ids que el personaje no tiene.</summary>
        public void SetUnlockedIds(IEnumerable<string> ids)
        {
            unlocked.Clear();
            var set = new HashSet<string>(ids ?? Array.Empty<string>());
            foreach (var a in all)
                if (set.Contains(a.id)) unlocked.Add(a);
            if (!IsUnlocked(Selected)) SelectFirstUnlocked(notify: true);
        }

        public void Select(AbilityData ability)
        {
            if (!IsUnlocked(ability) || ability == Selected) return;
            Selected = ability;
            SelectedChanged?.Invoke(Selected);
        }

        public void SelectNext() => Cycle(+1);
        public void SelectPrevious() => Cycle(-1);

        void Cycle(int step)
        {
            if (unlocked.Count < 2) return;
            int start = Selected != null ? all.IndexOf(Selected) : -1;
            for (int i = 1; i <= all.Count; i++)
            {
                var candidate = all[((start + step * i) % all.Count + all.Count) % all.Count];
                if (!unlocked.Contains(candidate)) continue;
                Selected = candidate;
                SfxPlayer.Play(switchSfx);
                SelectedChanged?.Invoke(Selected);
                return;
            }
        }

        void SelectFirstUnlocked(bool notify)
        {
            Selected = null;
            foreach (var a in all)
                if (unlocked.Contains(a)) { Selected = a; break; }
            if (notify) SelectedChanged?.Invoke(Selected);
        }

        public AbilityEffect EffectFor(AbilityData ability) =>
            ability != null && effects.TryGetValue(ability, out var e) ? e : null;

        public void StartCooldown(AbilityData ability)
        {
            if (ability != null) cooldownUntil[ability] = Time.time + ability.cooldown;
        }

        /// <summary>
        /// Si la habilidad seleccionada se puede activar ahora (sección 6.4). lacksEnergy indica que el
        /// único problema es la energía, para sonar el SFX de error.
        /// </summary>
        public bool CanUseSelected(bool crouching, out bool lacksEnergy)
        {
            lacksEnergy = false;
            var a = Selected;
            if (a == null || !IsUnlocked(a)) return false;
            var effect = EffectFor(a);
            if (effect == null) return false;
            if (crouching && !a.usableWhileCrouching) return false;
            if (cooldownUntil.TryGetValue(a, out float until) && Time.time < until) return false;
            if (a.maxInstancesOnScreen > 0 && effect.ActiveInstances >= a.maxInstancesOnScreen) return false;
            if (energy == null || !energy.Has(a.MinimumEnergyToStart))
            {
                lacksEnergy = true;
                return false;
            }
            return true;
        }

        public void PlayNoEnergy() => SfxPlayer.Play(noEnergySfx);
    }
}
