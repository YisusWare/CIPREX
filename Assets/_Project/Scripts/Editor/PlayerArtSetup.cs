using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// Convierte el spritesheet de Rosslyn en sprites, clips y Animators (Hito 3).
    /// La hoja es una cuadrícula de celdas de 48×64; cada clip empieza en una celda conocida y
    /// toma las celdas no vacías consecutivas hacia la derecha. Si se agregan frames en la misma
    /// fila y se vuelve a ejecutar, los clips se actualizan solos.
    /// Menú: Game/Setup/4. Personaje animado.
    /// </summary>
    public static class PlayerArtSetup
    {
        public const string SheetPath = "Assets/_Project/Art/Characters/Player/RosslynSpitesheet.png";
        public const string AnimRoot = "Assets/_Project/Animations/Player";
        public const string LegsControllerPath = AnimRoot + "/Legs.controller";
        public const string TorsoControllerPath = AnimRoot + "/Torso.controller";
        public const string FullBodyControllerPath = AnimRoot + "/FullBody.controller";

        const int CellW = 48, CellH = 64;
        static readonly Vector2 Pivot = new(24f / CellW, 12f / CellH); // pies en (24, 12)
        const float DefaultFps = 12f;

        enum Part { Legs, Torso, FullBody }

        // (clip, parte, fila desde arriba, columna inicial, loop)
        static readonly (string name, Part part, int row, int col, bool loop)[] Layout =
        {
            ("Torso_Idle", Part.Torso, 0, 0, true),
            ("Legs_Idle", Part.Legs, 1, 0, true),
            ("Torso_Run", Part.Torso, 2, 0, true),
            ("Legs_Run", Part.Legs, 3, 0, true),

            ("Torso_JumpUp", Part.Torso, 4, 0, true),
            ("Torso_Fall", Part.Torso, 4, 1, true),
            ("Torso_Crouch", Part.Torso, 4, 2, true),
            ("Torso_Dash", Part.Torso, 4, 3, true),
            ("Torso_Aim_Up", Part.Torso, 4, 5, true),
            ("Torso_Aim_DiagUp", Part.Torso, 4, 6, true),

            ("Legs_JumpUp", Part.Legs, 5, 0, true),
            ("Legs_Fall", Part.Legs, 5, 1, true),
            ("Legs_Crouch", Part.Legs, 5, 2, true),
            ("Legs_Dash", Part.Legs, 5, 3, true),

            ("Torso_Shoot_Forward", Part.Torso, 7, 0, false),
            ("Torso_Shoot_Up", Part.Torso, 7, 5, false),
            ("Torso_Shoot_DiagUp", Part.Torso, 7, 10, false),
            ("Torso_Shoot_Crouch", Part.Torso, 7, 15, false),

            ("Torso_Shoot_Down", Part.Torso, 8, 0, false),

            ("FullBody_Hurt", Part.FullBody, 9, 0, false),
            ("FullBody_Death", Part.FullBody, 10, 0, false),
            ("FullBody_Teleport_In", Part.FullBody, 11, 0, false),
            ("FullBody_Teleport_Out", Part.FullBody, 12, 0, false),
        };

        [MenuItem("Game/Setup/4. Personaje animado (spritesheet)", priority = 23)]
        public static void Run()
        {
            if (!File.Exists(SheetPath))
            {
                EditorUtility.DisplayDialog("Personaje animado", $"No se encontró {SheetPath}.", "OK");
                return;
            }

            var frames = DetectFrames();
            SliceSheet(frames);
            var clips = WriteClips(frames);

            EnsureFolder(AnimRoot);
            WriteController(LegsControllerPath, clips[Part.Legs], "Legs_Idle");
            WriteController(TorsoControllerPath, clips[Part.Torso], "Torso_Idle");
            WriteController(FullBodyControllerPath, clips[Part.FullBody], "FullBody_Hurt");
            AssetDatabase.SaveAssets();

            // El prefab del jugador usa los Animators si existen.
            GameSetup.CreatePrefabs();

            var report = string.Join("\n", Layout.Select(l => $"{l.name}: {frames[l.name].Count} frames"));
            Debug.Log("[Personaje animado] Listo.\n" + report);
        }

        // ------------------------------------------------------------------ Detección de frames

        /// <summary>Para cada clip, las celdas no vacías consecutivas desde su columna inicial.</summary>
        static Dictionary<string, List<RectInt>> DetectFrames()
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(SheetPath));
            int cols = tex.width / CellW;
            var pixels = tex.GetPixels32();
            int texW = tex.width, texH = tex.height;
            Object.DestroyImmediate(tex);

            bool CellHasPixels(int row, int col)
            {
                int y0 = texH - (row + 1) * CellH; // GetPixels empieza abajo
                if (y0 < 0) return false;
                for (int y = y0; y < y0 + CellH; y++)
                    for (int x = col * CellW; x < (col + 1) * CellW; x++)
                        if (pixels[y * texW + x].a > 0) return true;
                return false;
            }

            var result = new Dictionary<string, List<RectInt>>();
            foreach (var clip in Layout)
            {
                // El clip llega hasta la siguiente columna inicial de la misma fila.
                int limit = Layout.Where(o => o.row == clip.row && o.col > clip.col)
                                  .Select(o => o.col).DefaultIfEmpty(cols).Min();
                var list = new List<RectInt>();
                for (int c = clip.col; c < limit && CellHasPixels(clip.row, c); c++)
                    list.Add(new RectInt(c * CellW, texH - (clip.row + 1) * CellH, CellW, CellH));
                result[clip.name] = list;
                if (list.Count == 0) Debug.LogWarning($"[Personaje animado] {clip.name}: no hay frames en la fila {clip.row + 1}, columna {clip.col + 1}.");
            }
            return result;
        }

        // ------------------------------------------------------------------ Slicing

        static void SliceSheet(Dictionary<string, List<RectInt>> frames)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(SheetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();

            // Se conservan los IDs de los sprites que ya existían con el mismo nombre,
            // para no romper las referencias de los clips al volver a ejecutar.
            var existing = provider.GetSpriteRects()
                .GroupBy(r => r.name).ToDictionary(g => g.Key, g => g.First().spriteID);

            var rects = new List<SpriteRect>();
            foreach (var clip in Layout)
            {
                var list = frames[clip.name];
                for (int i = 0; i < list.Count; i++)
                {
                    string name = $"{clip.name}_{i}";
                    var r = list[i];
                    rects.Add(new SpriteRect
                    {
                        name = name,
                        spriteID = existing.TryGetValue(name, out var id) ? id : GUID.Generate(),
                        rect = new Rect(r.x, r.y, r.width, r.height),
                        alignment = SpriteAlignment.Custom,
                        pivot = Pivot,
                    });
                }
            }

            provider.SetSpriteRects(rects.ToArray());
            var nameIds = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            nameIds?.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
        }

        // ------------------------------------------------------------------ Clips

        static Dictionary<Part, List<AnimationClip>> WriteClips(Dictionary<string, List<RectInt>> frames)
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(SheetPath).OfType<Sprite>()
                .ToDictionary(s => s.name);
            var result = new Dictionary<Part, List<AnimationClip>>
            {
                [Part.Legs] = new(), [Part.Torso] = new(), [Part.FullBody] = new(),
            };

            foreach (var def in Layout)
            {
                int count = frames[def.name].Count;
                if (count == 0) continue;

                var clipSprites = new Sprite[count];
                for (int i = 0; i < count; i++) clipSprites[i] = sprites[$"{def.name}_{i}"];

                string dir = $"{AnimRoot}/{def.part}";
                EnsureFolder(dir);
                result[def.part].Add(WriteClip($"{dir}/{def.name}.anim", clipSprites, def.loop));
            }
            return result;
        }

        static AnimationClip WriteClip(string path, Sprite[] frames, bool loop)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            bool isNew = clip == null;
            if (isNew) clip = new AnimationClip { frameRate = DefaultFps };
            float fps = clip.frameRate > 0f ? clip.frameRate : DefaultFps; // respeta el Sample Rate ajustado a mano

            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keys = new ObjectReferenceKeyframe[frames.Length + 1];
            for (int i = 0; i < frames.Length; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
            // Clave final para que el último frame dure lo mismo que los demás.
            keys[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / fps, value = frames[^1] };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            if (isNew) AssetDatabase.CreateAsset(clip, path);
            else EditorUtility.SetDirty(clip);
            return clip;
        }

        // ------------------------------------------------------------------ Animators

        /// <summary>Un estado por clip, con el mismo nombre, sin transiciones (sección 9.3).</summary>
        static void WriteController(string path, List<AnimationClip> clips, string defaultState)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            var sm = controller.layers[0].stateMachine;
            foreach (var child in sm.states.ToArray()) sm.RemoveState(child.state);

            foreach (var clip in clips)
            {
                var state = sm.AddState(clip.name);
                state.motion = clip;
                state.writeDefaultValues = false;
                if (clip.name == defaultState) sm.defaultState = state;
            }
            EditorUtility.SetDirty(controller);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
