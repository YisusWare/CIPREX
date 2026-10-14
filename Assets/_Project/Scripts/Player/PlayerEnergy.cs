using System;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Barra de energía compartida por todas las habilidades del personaje (sección 8.6).
    /// Es un float por el consumo por segundo; el HUD la muestra en segmentos enteros.
    /// No sabe qué habilidad la usa.
    /// </summary>
    public class PlayerEnergy : MonoBehaviour
    {
        const float Epsilon = 0.0001f;

        public float Current { get; private set; }
        public float Max { get; private set; }
        public bool IsEmpty => Current <= Epsilon;

        /// <summary>(actual, máximo)</summary>
        public event Action<float, float> Changed;

        /// <summary>Lo llama PlayerController.Initialize con la energía del personaje. Deja la barra llena.</summary>
        public void SetMax(float max)
        {
            Max = Mathf.Max(0f, max);
            Set(Max);
        }

        public bool Has(float amount) => Current + Epsilon >= amount;

        /// <summary>Cobra un costo completo. Si no alcanza, no cobra nada y devuelve false.</summary>
        public bool TrySpend(float amount)
        {
            if (!Has(amount)) return false;
            Set(Current - amount);
            return true;
        }

        /// <summary>Consumo continuo (habilidades sostenidas). Nunca baja de 0.</summary>
        public void Drain(float amount) => Set(Current - amount);

        /// <summary>Pickups de energía.</summary>
        public void Refill(float amount) => Set(Current + amount);

        /// <summary>Al empezar el nivel y al reaparecer.</summary>
        public void RefillFull() => Set(Max);

        void Set(float value)
        {
            Current = Mathf.Clamp(value, 0f, Max);
            Changed?.Invoke(Current, Max);
        }
    }
}
