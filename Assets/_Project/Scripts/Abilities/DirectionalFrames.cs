using System;
using UnityEngine;

namespace Game.Abilities
{
    /// <summary>
    /// Frames de un efecto que apunta en una dirección (sección 8.8). En pixel art no se rotan los
    /// sprites 45°: hay un juego horizontal (dibujado hacia la derecha), uno diagonal (arriba-derecha)
    /// y uno vertical (hacia arriba). Las demás direcciones se obtienen volteando.
    /// </summary>
    [Serializable]
    public class DirectionalFrames
    {
        public Sprite[] horizontal;
        public Sprite[] diagonal;
        public Sprite[] vertical;

        /// <summary>Los frames para una dirección, con el volteo que hay que aplicar.</summary>
        public Sprite[] For(Vector2 direction, out bool flipX, out bool flipY)
        {
            const float Threshold = 0.2f;
            bool horizontalComponent = Mathf.Abs(direction.x) > Threshold;
            bool verticalComponent = Mathf.Abs(direction.y) > Threshold;
            flipX = direction.x < -Threshold;
            flipY = direction.y < -Threshold;

            Sprite[] frames;
            if (horizontalComponent && verticalComponent) frames = diagonal;
            else if (verticalComponent) { frames = vertical; flipX = false; }
            else { frames = horizontal; flipY = false; }

            // Si falta un juego, se usa el horizontal (mejor algo que nada mientras se dibuja).
            return frames != null && frames.Length > 0 ? frames : horizontal;
        }

        /// <summary>Frame según el tiempo. Con loop = false se queda en el último.</summary>
        public static Sprite Frame(Sprite[] frames, float time, float fps, bool loop)
        {
            if (frames == null || frames.Length == 0) return null;
            int i = Mathf.FloorToInt(time * fps);
            i = loop ? i % frames.Length : Mathf.Min(i, frames.Length - 1);
            return frames[Mathf.Max(i, 0)];
        }
    }
}
