using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Flash blanco al recibir daño (sección 10.1). Requiere un material con el shader
    /// "Game/Sprite-Lit-Flash" y usa MaterialPropertyBlock para no duplicar materiales.
    /// </summary>
    public class DamageFlash : MonoBehaviour
    {
        static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

        [SerializeField] SpriteRenderer[] renderers;
        [SerializeField] float duration = 0.1f;

        MaterialPropertyBlock block;
        float timer;

        void Awake()
        {
            block = new MaterialPropertyBlock();
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<SpriteRenderer>();
        }

        public void Flash()
        {
            timer = duration;
            Apply(1f);
        }

        void Update()
        {
            if (timer <= 0f) return;
            timer -= Time.deltaTime;
            if (timer <= 0f) Apply(0f);
        }

        void Apply(float amount)
        {
            foreach (var r in renderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(block);
                block.SetFloat(FlashAmountId, amount);
                r.SetPropertyBlock(block);
            }
        }
    }
}
