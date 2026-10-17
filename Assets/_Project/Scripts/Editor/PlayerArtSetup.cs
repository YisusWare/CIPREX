using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// Convierte el spritesheet de Rosslyn en sprites, clips y Animators (Hito 3).
    /// Dónde está cada clip lo dice el asset SpritesheetLayout (Data/Characters/SpritesheetLayout.asset,
    /// sección 4.2.1): cada clip empieza en una celda y toma las celdas no vacías consecutivas hacia
    /// la derecha. Si se agregan frames o entradas y se vuelve a ejecutar, los clips se actualizan solos.
    /// Menú: Game/Setup/4. Personaje animado.
    /// </summary>
    public static class PlayerArtSetup
    {
        public const string SheetPath = "Assets/_Project/Art/Characters/Player/RosslynSpitesheet.png";
        public const string AnimRoot = "Assets/_Project/Animations/Player";
        public const string LegsControllerPath = AnimRoot + "/Legs.controller";
        public const string TorsoControllerPath = AnimRoot + "/Torso.controller";
        public const string FullBodyControllerPath = AnimRoot + "/FullBody.controller";

        public const string LayoutPath = "Assets/_Project/Data/Characters/SpritesheetLayout.asset";

        // Layout en uso durante una ejecución del menú.
        static SpritesheetLayout layout;
        static int CellW => layout.cellWidth;
        static int CellH => layout.cellHeight;
        static Vector2 Pivot => new((float)layout.pivotPixels.x / CellW, (float)layout.pivotPixels.y / CellH);

        /// <summary>Entrada del layout con fila y columna en base 0 (el asset usa base 1).</summary>
        readonly struct ClipDef
        {
            public readonly string name;
            public readonly SpritesheetPart part;
            public readonly int row, col;
            public readonly bool loop;

            public ClipDef(SpritesheetLayout.Entry e)
            {
                name = e.clipName.Trim();
                part = e.part;
                row = e.row - 1;
                col = e.startColumn - 1;
                loop = e.loop;
            }
        }

        static ClipDef[] Layout;

        /// <summary>Mientras la herramienta corta la hoja (el aviso de corte manual no aplica).</summary>
        public static bool IsSlicing { get; private set; }

        [MenuItem("Game/Setup/4. Personaje animado (spritesheet)", priority = 23)]
        public static void Run()
        {
            if (!File.Exists(SheetPath))
            {
                EditorUtility.DisplayDialog("Personaje animado", $"No se encontró {SheetPath}.", "OK");
                return;
            }

            layout = LoadOrCreateLayout();
            var errors = layout.Validate();
            if (errors.Count > 0)
            {
                EditorUtility.DisplayDialog("Personaje animado",
                    "Corrige el SpritesheetLayout antes de continuar:\n\n" + string.Join("\n", errors), "OK");
                Selection.activeObject = layout;
                return;
            }
            Layout = layout.clips.Select(e => new ClipDef(e)).ToArray();

            var frames = DetectFrames();
            Dictionary<SpritesheetPart, List<AnimationClip>> clips;
            var stripReport = new List<string>();
            IsSlicing = true;
            try
            {
                SliceSheet(frames);
                clips = WriteClips(frames);
                foreach (var strip in layout.strips)
                {
                    var clip = WriteStripClip(strip, out int count);
                    stripReport.Add($"{strip.clipName}: {count} frames ({strip.fileName})");
                    if (clip != null) clips[strip.part].Add(clip);
                }
            }
            finally { IsSlicing = false; }

            EnsureFolder(AnimRoot);
            WriteController(LegsControllerPath, clips[SpritesheetPart.Legs], layout.DefaultClipFor(SpritesheetPart.Legs));
            WriteController(TorsoControllerPath, clips[SpritesheetPart.Torso], layout.DefaultClipFor(SpritesheetPart.Torso));
            WriteController(FullBodyControllerPath, clips[SpritesheetPart.FullBody], layout.DefaultClipFor(SpritesheetPart.FullBody));
            AssetDatabase.SaveAssets();

            // El prefab del jugador usa los Animators si existen.
            GameSetup.CreatePrefabs();

            var report = string.Join("\n", Layout.Select(l => $"{l.name}: {frames[l.name].Count} frames").Concat(stripReport));
            Debug.Log("[Personaje animado] Listo.\n" + report);
        }

        // ------------------------------------------------------------------ Layout

        /// <summary>Carga el SpritesheetLayout; si no existe, lo crea con el layout actual de Rosslyn.</summary>
        static SpritesheetLayout LoadOrCreateLayout()
        {
            var asset = AssetDatabase.LoadAssetAtPath<SpritesheetLayout>(LayoutPath);
            if (asset != null)
            {
                AddDefaultStrips(asset);
                return asset;
            }

            EnsureFolder(Path.GetDirectoryName(LayoutPath).Replace('\\', '/'));
            asset = ScriptableObject.CreateInstance<SpritesheetLayout>();
            asset.clips = SpritesheetLayout.DefaultEntries();
            AddDefaultStrips(asset);
            AssetDatabase.CreateAsset(asset, LayoutPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Personaje animado] Se creó {LayoutPath} con los {asset.clips.Count} clips actuales.");
            return asset;
        }

        /// <summary>Agrega las tiras por defecto cuyo PNG existe y que el layout todavía no tiene.</summary>
        static void AddDefaultStrips(SpritesheetLayout asset)
        {
            bool changed = false;
            foreach (var strip in SpritesheetLayout.DefaultStrips())
            {
                if (asset.strips.Any(s => s.clipName == strip.clipName)) continue;
                if (!File.Exists(StripPath(strip))) continue;
                asset.strips.Add(strip);
                changed = true;
                Debug.Log($"[Personaje animado] Tira agregada al layout: {strip.clipName} ({strip.fileName}).");
            }
            if (changed) EditorUtility.SetDirty(asset);
        }

        static string StripPath(SpritesheetLayout.Strip strip) =>
            Path.GetDirectoryName(SheetPath).Replace('\\', '/') + "/" + strip.fileName;

        [MenuItem("Game/Setup/Abrir SpritesheetLayout", priority = 24)]
        static void SelectLayout()
        {
            Selection.activeObject = LoadOrCreateLayout();
            EditorGUIUtility.PingObject(Selection.activeObject);
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

            int rows = texH / CellH;
            var result = new Dictionary<string, List<RectInt>>();
            foreach (var clip in Layout)
            {
                if (clip.row >= rows || clip.col >= cols)
                {
                    Debug.LogWarning($"[Personaje animado] {clip.name}: la celda (fila {clip.row + 1}, columna {clip.col + 1}) está fuera de la hoja ({rows} filas × {cols} columnas).");
                    result[clip.name] = new List<RectInt>();
                    continue;
                }
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

        static Dictionary<SpritesheetPart, List<AnimationClip>> WriteClips(Dictionary<string, List<RectInt>> frames)
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(SheetPath).OfType<Sprite>()
                .ToDictionary(s => s.name);
            var result = new Dictionary<SpritesheetPart, List<AnimationClip>>
            {
                [SpritesheetPart.Legs] = new(), [SpritesheetPart.Torso] = new(), [SpritesheetPart.FullBody] = new(),
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
            if (isNew) clip = new AnimationClip { frameRate = layout.defaultFps };
            float fps = clip.frameRate > 0f ? clip.frameRate : layout.defaultFps; // respeta el Sample Rate ajustado a mano

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

        // ------------------------------------------------------------------ Tiras aparte

        /// <summary>
        /// Corta una tira (frames de frameSize, de izquierda a derecha y de arriba hacia abajo, saltando
        /// las celdas vacías) con el pivote de los pies, y escribe su clip. Devuelve null si falta el PNG.
        /// </summary>
        static AnimationClip WriteStripClip(SpritesheetLayout.Strip strip, out int count)
        {
            count = 0;
            string path = StripPath(strip);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[Personaje animado] {strip.clipName}: no se encontró {path}.");
                return null;
            }

            int fw = strip.frameSize.x, fh = strip.frameSize.y;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(path));
            int texW = tex.width, texH = tex.height;
            var pixels = tex.GetPixels32();
            Object.DestroyImmediate(tex);

            var cells = new List<RectInt>();
            for (int row = 0; row < texH / fh; row++)
                for (int col = 0; col < texW / fw; col++)
                {
                    int x0 = col * fw, y0 = texH - (row + 1) * fh;
                    bool any = false;
                    for (int y = y0; y < y0 + fh && !any; y++)
                        for (int x = x0; x < x0 + fw && !any; x++)
                            if (pixels[y * texW + x].a > 0) any = true;
                    if (any) cells.Add(new RectInt(x0, y0, fw, fh));
                }
            if (cells.Count == 0)
            {
                Debug.LogWarning($"[Personaje animado] {strip.clipName}: {path} no tiene frames de {fw}×{fh}.");
                return null;
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
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
            var existing = provider.GetSpriteRects().GroupBy(r => r.name).ToDictionary(g => g.Key, g => g.First().spriteID);

            var pivot = new Vector2((float)strip.pivotPixels.x / fw, (float)strip.pivotPixels.y / fh);
            var rects = new List<SpriteRect>();
            for (int i = 0; i < cells.Count; i++)
            {
                string name = $"{strip.clipName}_{i}";
                rects.Add(new SpriteRect
                {
                    name = name,
                    spriteID = existing.TryGetValue(name, out var id) ? id : GUID.Generate(),
                    rect = new Rect(cells[i].x, cells[i].y, fw, fh),
                    alignment = SpriteAlignment.Custom,
                    pivot = pivot,
                });
            }
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()
                ?.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();

            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s => s.name);
            var clipSprites = new Sprite[cells.Count];
            for (int i = 0; i < cells.Count; i++) clipSprites[i] = sprites[$"{strip.clipName}_{i}"];

            string dir = $"{AnimRoot}/{strip.part}";
            EnsureFolder(dir);
            count = cells.Count;
            return WriteClip($"{dir}/{strip.clipName}.anim", clipSprites, strip.loop);
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
