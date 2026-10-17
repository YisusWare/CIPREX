using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player
{
    /// <summary>Mitad del personaje a la que pertenece un clip (sección 4.4).</summary>
    public enum SpritesheetPart { Legs, Torso, FullBody }

    /// <summary>
    /// Dónde está cada clip en el spritesheet de un personaje (sección 4.2.1). Todos los personajes
    /// usan la misma cuadrícula y el mismo layout. Solo lo lee la herramienta del editor
    /// (Game → Setup → 4. Personaje animado); el juego no lo usa en runtime.
    /// Para agregar un clip: una entrada nueva aquí, dibujar las celdas y volver a ejecutar el menú.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Spritesheet Layout", fileName = "SpritesheetLayout")]
    public class SpritesheetLayout : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            [Tooltip("Nombre del clip y del estado del Animator. PlayerAnimation los busca por este nombre (p. ej. Torso_Run).")]
            public string clipName;
            public SpritesheetPart part;
            [Tooltip("Fila de la hoja, contando desde 1 y de arriba hacia abajo.")]
            [Min(1)] public int row = 1;
            [Tooltip("Columna donde empieza el clip, contando desde 1. Toma las celdas no vacías consecutivas hacia la derecha, hasta la siguiente columna inicial de la misma fila.")]
            [Min(1)] public int startColumn = 1;
            public bool loop = true;

            public Entry() { }

            public Entry(string clipName, SpritesheetPart part, int row, int startColumn, bool loop)
            {
                this.clipName = clipName;
                this.part = part;
                this.row = row;
                this.startColumn = startColumn;
                this.loop = loop;
            }
        }

        /// <summary>
        /// Un clip que va en su propio PNG porque no cabe en la celda de 48×64 (por ejemplo, el tajo de la
        /// espada con la llama). Tira horizontal de frames del mismo tamaño; las celdas vacías se saltan.
        /// </summary>
        [Serializable]
        public class Strip
        {
            [Tooltip("Nombre del clip y del estado del Animator (p. ej. Torso_Melee).")]
            public string clipName;
            public SpritesheetPart part = SpritesheetPart.Torso;
            [Tooltip("Nombre del PNG, en la misma carpeta que el spritesheet (respetar mayúsculas).")]
            public string fileName;
            [Tooltip("Tamaño de cada frame en píxeles.")]
            public Vector2Int frameSize = new(48, 64);
            [Tooltip("Dónde están los pies dentro del frame, en píxeles desde abajo a la izquierda. " +
                     "Debe coincidir con el pivote de la hoja para que el torso encaje con las piernas.")]
            public Vector2Int pivotPixels = new(24, 12);
            public bool loop;

            public Strip() { }

            public Strip(string clipName, string fileName, Vector2Int frameSize, Vector2Int pivotPixels)
            {
                this.clipName = clipName;
                this.fileName = fileName;
                this.frameSize = frameSize;
                this.pivotPixels = pivotPixels;
            }
        }

        [Header("Cuadrícula")]
        [Tooltip("Ancho de cada celda en píxeles.")]
        [Min(1)] public int cellWidth = 48;
        [Tooltip("Alto de cada celda en píxeles.")]
        [Min(1)] public int cellHeight = 64;
        [Tooltip("Pivote en píxeles dentro de la celda, contando desde abajo a la izquierda (los pies).")]
        public Vector2Int pivotPixels = new(24, 12);
        [Tooltip("Sample Rate de los clips nuevos. Los que ya existen conservan el que tengan.")]
        [Min(1f)] public float defaultFps = 12f;

        [Header("Estado inicial de cada Animator")]
        public string defaultLegsClip = "Legs_Idle";
        public string defaultTorsoClip = "Torso_Idle";
        public string defaultFullBodyClip = "FullBody_Hurt";

        [Header("Clips")]
        public List<Entry> clips = new();

        [Header("Clips en PNG aparte (más grandes que la celda)")]
        public List<Strip> strips = new();

        public string DefaultClipFor(SpritesheetPart part) => part switch
        {
            SpritesheetPart.Legs => defaultLegsClip,
            SpritesheetPart.Torso => defaultTorsoClip,
            _ => defaultFullBodyClip,
        };

        /// <summary>El layout actual de la hoja de Rosslyn (se usa al crear el asset).</summary>
        public static List<Entry> DefaultEntries() => new()
        {
            new("Torso_Idle", SpritesheetPart.Torso, 1, 1, true),
            new("Legs_Idle", SpritesheetPart.Legs, 2, 1, true),
            new("Torso_Run", SpritesheetPart.Torso, 3, 1, true),
            new("Legs_Run", SpritesheetPart.Legs, 4, 1, true),

            new("Torso_JumpUp", SpritesheetPart.Torso, 5, 1, true),
            new("Torso_Fall", SpritesheetPart.Torso, 5, 2, true),
            new("Torso_Crouch", SpritesheetPart.Torso, 5, 3, true),
            new("Torso_Dash", SpritesheetPart.Torso, 5, 4, true),
            new("Torso_Aim_Up", SpritesheetPart.Torso, 5, 6, true),
            new("Torso_Aim_DiagUp", SpritesheetPart.Torso, 5, 7, true),

            new("Legs_JumpUp", SpritesheetPart.Legs, 6, 1, true),
            new("Legs_Fall", SpritesheetPart.Legs, 6, 2, true),
            new("Legs_Crouch", SpritesheetPart.Legs, 6, 3, true),
            new("Legs_Dash", SpritesheetPart.Legs, 6, 4, true),

            new("Torso_Shoot_Forward", SpritesheetPart.Torso, 8, 1, false),
            new("Torso_Shoot_Up", SpritesheetPart.Torso, 8, 6, false),
            new("Torso_Shoot_DiagUp", SpritesheetPart.Torso, 8, 11, false),
            new("Torso_Shoot_Crouch", SpritesheetPart.Torso, 8, 16, false),

            new("Torso_Shoot_Down", SpritesheetPart.Torso, 9, 1, false),

            new("FullBody_Hurt", SpritesheetPart.FullBody, 10, 1, false),
            new("FullBody_Death", SpritesheetPart.FullBody, 11, 1, false),
            new("FullBody_Teleport_In", SpritesheetPart.FullBody, 12, 1, false),
            new("FullBody_Teleport_Out", SpritesheetPart.FullBody, 13, 1, false),
        };

        /// <summary>
        /// Tiras del tajo de la espada de Rosslyn (dibujadas con la llama en el mismo sprite).
        /// Los pivotes se midieron alineando el casco con el de Torso_Idle; se ajustan en el asset.
        /// </summary>
        public static List<Strip> DefaultStrips() => new()
        {
            new("Torso_Melee", "RosslynFireSlash_horizontal.png", new(100, 64), new(51, 14)),
            new("Torso_Melee_Crouch", "RosslynFireSlash_crouch.png", new(100, 48), new(46, 7)),
            new("Torso_Melee_Up", "RosslynFireSlash_Up.png", new(48, 100), new(27, 20)),
            new("Torso_Melee_Down", "rosslynFireSlash_down.png", new(48, 100), new(24, 30)),
        };

        /// <summary>
        /// Revisa el layout y devuelve los errores (nombres vacíos o repetidos, dos clips en la misma celda inicial).
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();
            var names = new HashSet<string>();
            var starts = new Dictionary<(int, int), string>();

            foreach (var e in clips)
            {
                if (string.IsNullOrWhiteSpace(e.clipName))
                {
                    errors.Add($"Hay una entrada sin nombre (fila {e.row}, columna {e.startColumn}).");
                    continue;
                }
                if (!names.Add(e.clipName))
                    errors.Add($"El clip '{e.clipName}' está repetido.");
                if (starts.TryGetValue((e.row, e.startColumn), out var other))
                    errors.Add($"'{e.clipName}' y '{other}' empiezan en la misma celda (fila {e.row}, columna {e.startColumn}).");
                else
                    starts[(e.row, e.startColumn)] = e.clipName;
            }
            foreach (var s in strips)
            {
                if (string.IsNullOrWhiteSpace(s.clipName) || string.IsNullOrWhiteSpace(s.fileName))
                {
                    errors.Add("Hay una tira sin nombre de clip o sin archivo.");
                    continue;
                }
                if (!names.Add(s.clipName))
                    errors.Add($"El clip '{s.clipName}' está repetido.");
                if (s.frameSize.x <= 0 || s.frameSize.y <= 0)
                    errors.Add($"La tira '{s.clipName}' tiene un tamaño de frame inválido.");
            }
            return errors;
        }
    }
}
