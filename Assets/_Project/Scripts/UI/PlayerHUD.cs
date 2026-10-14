using Game.Abilities;
using Game.Level;
using Game.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// HUD del jugador (sección 15.6): barra de vida, barra de energía e icono de la habilidad seleccionada.
    /// Se conecta solo cuando PlayerSpawner crea al jugador. Los máximos salen del CharacterData.
    /// </summary>
    public class PlayerHUD : MonoBehaviour
    {
        [SerializeField] SegmentedBar healthBar;
        [SerializeField] SegmentedBar energyBar;
        [SerializeField] Image abilityIcon;
        [SerializeField] Text abilityName;
        [Tooltip("Segundos que se ve el nombre al cambiar de habilidad.")]
        [SerializeField] float nameDuration = 1.5f;

        PlayerController player;
        float nameTimer;

        void OnEnable() => PlayerSpawner.PlayerSpawned += Bind;

        void OnDisable()
        {
            PlayerSpawner.PlayerSpawned -= Bind;
            Unbind();
        }

        public void Bind(PlayerController target)
        {
            Unbind();
            player = target;
            if (player == null) return;

            player.Health.Changed += OnHealthChanged;
            player.Energy.Changed += OnEnergyChanged;
            player.Abilities.SelectedChanged += OnSelectedChanged;

            OnHealthChanged(player.Health.Current, player.Health.Max);
            OnEnergyChanged(player.Energy.Current, player.Energy.Max);
            OnSelectedChanged(player.Abilities.Selected);
            // Al empezar no se muestra el nombre.
            nameTimer = 0f;
            if (abilityName != null) abilityName.enabled = false;
        }

        void Unbind()
        {
            if (player == null) return;
            player.Health.Changed -= OnHealthChanged;
            player.Energy.Changed -= OnEnergyChanged;
            player.Abilities.SelectedChanged -= OnSelectedChanged;
            player = null;
        }

        void Update()
        {
            if (nameTimer > 0f)
            {
                nameTimer -= Time.deltaTime;
                if (nameTimer <= 0f && abilityName != null) abilityName.enabled = false;
            }
        }

        void OnHealthChanged(int current, int max)
        {
            if (healthBar != null) healthBar.SetValue(current, max);
        }

        void OnEnergyChanged(float current, float max)
        {
            if (energyBar != null) energyBar.SetValue(current, max);
        }

        void OnSelectedChanged(AbilityData ability)
        {
            bool has = ability != null;
            if (energyBar != null) energyBar.gameObject.SetActive(has);
            if (abilityIcon != null)
            {
                abilityIcon.enabled = has && ability.icon != null;
                if (has) abilityIcon.sprite = ability.icon;
            }
            if (abilityName != null)
            {
                abilityName.enabled = has;
                abilityName.text = has ? ability.displayName : "";
                nameTimer = has ? nameDuration : 0f;
            }
        }
    }
}
