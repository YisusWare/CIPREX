using Game.Audio;
using Game.Combat;
using Game.Core;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Sonidos del jugador a partir de eventos (salto, aterrizaje, dash, daño, muerte).
    /// Los pasos llegan en el Hito 3 con el Animation Event OnFootstep.
    /// El disparo lo reproduce PlayerBuster con BusterData.fireSfx.
    /// </summary>
    public class PlayerAudio : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        [SerializeField] SfxData jump;
        [SerializeField] SfxData land;
        [SerializeField] SfxData dash;
        [SerializeField] SfxData hurt;
        [SerializeField] SfxData death;

        void Start()
        {
            if (player == null) player = GetComponent<PlayerController>();
            player.Locomotion.Jumped += OnJumped;
            player.Locomotion.StateChanged += OnLocomotionChanged;
            player.Health.Damaged += OnDamaged;
            player.Health.Died += OnDied;
        }

        void OnDestroy()
        {
            if (player == null || player.Context == null) return;
            player.Locomotion.Jumped -= OnJumped;
            player.Locomotion.StateChanged -= OnLocomotionChanged;
            player.Health.Damaged -= OnDamaged;
            player.Health.Died -= OnDied;
        }

        void OnJumped() => Play(jump);
        void OnDamaged(DamageInfo info) => Play(hurt);
        void OnDied() => Play(death);

        void OnLocomotionChanged(IState prev, IState next)
        {
            var loco = player.Locomotion;
            if (next == loco.Dash || next == loco.AirDash)
            {
                Play(dash);
                return;
            }

            bool wasInAir = prev == loco.Airborne || prev == loco.AirDash;
            bool nowOnGround = next == loco.Idle || next == loco.Run;
            if (wasInAir && nowOnGround) Play(land);
        }

        void Play(SfxData sfx) => SfxPlayer.Play(sfx, transform.position);
    }
}
