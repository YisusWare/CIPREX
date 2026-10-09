using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Level;
using Game.Player;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Game.EditorTools
{
    /// <summary>
    /// Arma lo necesario para el Hito 1 (sección 16): sprites placeholder, tiles,
    /// datos, prefab del jugador y la escena de prueba de movimiento.
    /// Menú: Game/Setup. Se puede volver a ejecutar; los PNG que ya existen no se sobrescriben.
    /// </summary>
    public static class Hito1Setup
    {
        const string Root = "Assets/_Project";
        const string PlaceholderDir = Root + "/Art/Characters/Player/Placeholder";
        const string TilesArtDir = Root + "/Art/Tiles";
        const string TileAssetsDir = Root + "/Art/Tiles/TileAssets";
        const string PlayerDataDir = Root + "/Data/Player";
        const string PhysicsDir = Root + "/Data/Physics";
        const string PlayerPrefabDir = Root + "/Prefabs/Player";
        const string SceneDir = Root + "/Scenes";

        const string InputPath = Root + "/Input/PlayerControls.inputactions";
        const string MovementDataPath = PlayerDataDir + "/PlayerMovementData.asset";
        const string NoFrictionPath = PhysicsDir + "/NoFriction.physicsMaterial2D";
        const string PlayerPrefabPath = PlayerPrefabDir + "/Player.prefab";
        const string ScenePath = SceneDir + "/Test_Movement.unity";
        const string TemplateScenePath = "Assets/Settings/Scenes/URP2DSceneTemplate.unity";

        const string PlayerStandPath = PlaceholderDir + "/Player_Placeholder.png";
        const string PlayerCrouchPath = PlaceholderDir + "/Player_Placeholder_Crouch.png";
        const string TileGroundPath = TilesArtDir + "/Tile_Ground.png";
        const string TilePlatformPath = TilesArtDir + "/Tile_Platform.png";
        const string TileGridPath = TilesArtDir + "/Tile_Grid.png";

        // Pivote del jugador: pies en (24, 12) de un lienzo de 48×64 (sección 4.1).
        static readonly Vector2 PlayerPivot = new(24f / 48f, 12f / 64f);

        static readonly string[] RequiredLayers = { "Ground", "Breakable", "Meltable", "Platform", "Player" };

        // ------------------------------------------------------------------ Menú

        [MenuItem("Game/Setup/Hito 1 - Crear todo", priority = 0)]
        public static void CreateAll()
        {
            if (!CheckLayers()) return;
            CreateArtAndData();
            CreatePlayerPrefab();
            CreateTestScene();
        }

        [MenuItem("Game/Setup/1. Placeholders, tiles y datos", priority = 20)]
        public static void CreateArtAndData()
        {
            EnsureFolders();
            CreatePlaceholderSprites();
            CreateTileAssets();
            EnsureMovementData();
            EnsureNoFrictionMaterial();
            AssetDatabase.SaveAssets();
            Debug.Log("[Hito 1] Placeholders, tiles y datos listos.");
        }

        [MenuItem("Game/Setup/2. Prefab del jugador", priority = 21)]
        public static void CreatePlayerPrefab()
        {
            if (!CheckLayers()) return;
            EnsureFolders();

            var input = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            if (input == null)
            {
                EditorUtility.DisplayDialog("Hito 1", $"No se encontró {InputPath}.", "OK");
                return;
            }

            var movement = EnsureMovementData();
            var noFriction = EnsureNoFrictionMaterial();
            Sprite stand = LoadSprite(PlayerStandPath);
            Sprite crouch = LoadSprite(PlayerCrouchPath);
            if (stand == null)
            {
                CreatePlaceholderSprites();
                stand = LoadSprite(PlayerStandPath);
                crouch = LoadSprite(PlayerCrouchPath);
            }

            var root = new GameObject("Player") { layer = LayerMask.NameToLayer("Player") };
            try
            {
                var rb = root.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 0f;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;

                var box = root.AddComponent<BoxCollider2D>();
                box.size = new Vector2(movement.colliderWidth, movement.standingHeight);
                box.offset = new Vector2(0f, movement.standingHeight * 0.5f);
                box.sharedMaterial = noFriction;

                var motor = root.AddComponent<PlayerMotor>();
                SetRef(motor, "movement", movement);
                SetMask(motor, "groundMask", LayerMask.GetMask("Ground", "Platform", "Breakable", "Meltable"));
                SetMask(motor, "ceilingMask", LayerMask.GetMask("Ground", "Breakable", "Meltable"));

                var reader = root.AddComponent<PlayerInputReader>();
                SetRef(reader, "actions", input);
                SetRef(reader, "movement", movement);

                var controller = root.AddComponent<PlayerController>();

                // Visual (se voltea) → Placeholder. En el Hito 3 se reemplaza por Legs, Torso y FullBody.
                var visual = new GameObject("Visual");
                visual.transform.SetParent(root.transform, false);
                visual.layer = root.layer;

                var placeholder = new GameObject("Placeholder");
                placeholder.transform.SetParent(visual.transform, false);
                placeholder.layer = root.layer;
                var sr = placeholder.AddComponent<SpriteRenderer>();
                sr.sprite = stand;
                sr.sortingOrder = 10;

                var camTarget = new GameObject("CameraTarget");
                camTarget.transform.SetParent(root.transform, false);
                camTarget.transform.localPosition = new Vector3(1.5f, 1f, 0f);
                var look = camTarget.AddComponent<CameraLookAhead>();
                SetRef(look, "player", controller);

                var debug = root.AddComponent<PlayerDebugView>();
                SetRef(debug, "player", controller);
                SetRef(debug, "visual", visual.transform);
                SetRef(debug, "body", sr);
                SetRef(debug, "standingSprite", stand);
                SetRef(debug, "crouchingSprite", crouch);

                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
                Debug.Log($"[Hito 1] Prefab creado: {PlayerPrefabPath}");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [MenuItem("Game/Setup/3. Escena de prueba", priority = 22)]
        public static void CreateTestScene()
        {
            if (!CheckLayers()) return;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                CreatePlayerPrefab();
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
                if (prefab == null) return;
            }

            var ground = AssetDatabase.LoadAssetAtPath<Tile>(TileAssetsDir + "/Tile_Ground.asset");
            var platform = AssetDatabase.LoadAssetAtPath<Tile>(TileAssetsDir + "/Tile_Platform.asset");
            var gridTile = AssetDatabase.LoadAssetAtPath<Tile>(TileAssetsDir + "/Tile_Grid.asset");
            if (ground == null || platform == null || gridTile == null)
            {
                CreateArtAndData();
                ground = AssetDatabase.LoadAssetAtPath<Tile>(TileAssetsDir + "/Tile_Ground.asset");
                platform = AssetDatabase.LoadAssetAtPath<Tile>(TileAssetsDir + "/Tile_Platform.asset");
                gridTile = AssetDatabase.LoadAssetAtPath<Tile>(TileAssetsDir + "/Tile_Grid.asset");
            }

            if (File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("Hito 1", $"{ScenePath} ya existe. ¿Reemplazarla?", "Reemplazar", "Cancelar"))
                return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureFolders();
            Scene scene;
            if (File.Exists(ScenePath)) AssetDatabase.DeleteAsset(ScenePath);
            if (File.Exists(TemplateScenePath) && AssetDatabase.CopyAsset(TemplateScenePath, ScenePath))
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            else
            {
                Debug.LogWarning("[Hito 1] No se encontró la escena plantilla URP 2D; se crea una vacía (agrega una Global Light 2D).");
                scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            }

            // Abrir la escena descarga los assets sin referencias (UnloadUnusedAssets),
            // así que las referencias cargadas antes quedan destruidas: se vuelven a cargar aquí.
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            ground = AssetDatabase.LoadAssetAtPath<Tile>(TileAssetsDir + "/Tile_Ground.asset");
            platform = AssetDatabase.LoadAssetAtPath<Tile>(TileAssetsDir + "/Tile_Platform.asset");
            gridTile = AssetDatabase.LoadAssetAtPath<Tile>(TileAssetsDir + "/Tile_Grid.asset");

            // ---------- Tilemaps ----------
            var gridGO = new GameObject("Grid");
            var grid = gridGO.AddComponent<Grid>();
            grid.cellSize = Vector3.one;

            var bgMap = CreateTilemap("Background_Grid", gridGO.transform, "Default", -10, collider: false, oneWay: false);
            var groundMap = CreateTilemap("Ground", gridGO.transform, "Ground", 0, collider: true, oneWay: false);
            var platformMap = CreateTilemap("Platforms", gridGO.transform, "Platform", 0, collider: true, oneWay: true);

            PaintTestLevel(groundMap, platformMap, bgMap, ground, platform, gridTile, out Vector3 spawn);
            if (groundMap.GetUsedTilesCount() == 0)
                Debug.LogError("[Hito 1] El Tilemap Ground quedó vacío: no se pudieron cargar los tiles.");

            // ---------- Jugador ----------
            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            player.transform.position = spawn;
            Transform camTarget = player.transform.Find("CameraTarget");

            // ---------- Cámara ----------
            SetupCamera(camTarget, spawn);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuild(ScenePath);
            Debug.Log($"[Hito 1] Escena creada: {ScenePath}. Presiona Play para probar.");
        }

        // ------------------------------------------------------------------ Nivel de prueba

        /// <summary>
        /// Nivel de la sección 16 (Hito 1). Coordenadas en celdas; la superficie del suelo está en y = 0.
        /// </summary>
        static void PaintTestLevel(Tilemap groundMap, Tilemap platformMap, Tilemap bgMap,
            TileBase ground, TileBase platform, TileBase gridTile, out Vector3 spawn)
        {
            const int minX = -4, maxX = 168;
            const int bottomY = -4, topY = 16;

            // Fondo con rejilla para medir alturas y distancias.
            Fill(bgMap, gridTile, minX, bottomY, maxX, topY);

            // Suelo base (4 filas).
            Fill(groundMap, ground, minX, bottomY, maxX, -1);

            // Pared izquierda alta (no pegarse a la pared).
            Fill(groundMap, ground, minX, 0, minX + 2, topY);

            // Pasillo largo y plano: x 0..39 (correr, dash, look ahead).

            // Escalones de 1, 2, 3 y 4 tiles de alto (4 de ancho cada uno).
            int[] stepHeights = { 1, 2, 3, 4 };
            for (int i = 0; i < stepHeights.Length; i++)
            {
                int x0 = 44 + i * 8;
                Fill(groundMap, ground, x0, 0, x0 + 3, stepHeights[i] - 1);
            }

            // Fosos de 3, 5, 7 y 9 tiles de ancho, 3 tiles de hondo (se puede salir saltando).
            int[,] pits = { { 80, 3 }, { 89, 5 }, { 100, 7 }, { 113, 9 } };
            for (int i = 0; i < pits.GetLength(0); i++)
            {
                int x0 = pits[i, 0], w = pits[i, 1];
                Clear(groundMap, x0, -3, x0 + w - 1, -1);
            }

            // Plataforma flotante (coyote time): escalón de 2 para subir y plataforma sólida en y = 3.
            Fill(groundMap, ground, 127, 0, 128, 1);
            Fill(groundMap, ground, 131, 3, 136, 3);

            // Plataformas atravesables escalonadas (superficies a 3, 6 y 9 tiles).
            Fill(platformMap, platform, 140, 2, 144, 2);
            Fill(platformMap, platform, 146, 5, 150, 5);
            Fill(platformMap, platform, 152, 8, 156, 8);

            // Pared derecha alta.
            Fill(groundMap, ground, maxX - 2, 0, maxX, topY);

            groundMap.CompressBounds();
            platformMap.CompressBounds();
            bgMap.CompressBounds();

            spawn = new Vector3(2f, 0f, 0f);
        }

        static void Fill(Tilemap map, TileBase tile, int x0, int y0, int x1, int y1)
        {
            for (int x = x0; x <= x1; x++)
                for (int y = y0; y <= y1; y++)
                    map.SetTile(new Vector3Int(x, y, 0), tile);
        }

        static void Clear(Tilemap map, int x0, int y0, int x1, int y1) => Fill(map, null, x0, y0, x1, y1);

        static Tilemap CreateTilemap(string name, Transform parent, string layer, int sortingOrder, bool collider, bool oneWay)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer(layer);

            var map = go.AddComponent<Tilemap>();
            var renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;

            if (collider)
            {
                // Rigidbody estático primero: si no, CompositeCollider2D agrega uno dinámico.
                var rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;

                var tilemapCollider = go.AddComponent<TilemapCollider2D>();
                tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;

                var composite = go.AddComponent<CompositeCollider2D>();
                composite.geometryType = CompositeCollider2D.GeometryType.Outlines;

                if (oneWay)
                {
                    var effector = go.AddComponent<PlatformEffector2D>();
                    effector.useOneWay = true;
                    effector.surfaceArc = 160f;
                    composite.usedByEffector = true;
                }
            }
            return map;
        }

        // ------------------------------------------------------------------ Cámara

        static void SetupCamera(Transform followTarget, Vector3 spawn)
        {
            Camera cam = Object.FindFirstObjectByType<Camera>();
            if (cam == null)
            {
                var camGO = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGO.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.06f, 0.11f);
            cam.transform.position = new Vector3(spawn.x, spawn.y + 2f, -10f);

            var ppc = cam.GetComponent<PixelPerfectCamera>();
            if (ppc == null) ppc = cam.gameObject.AddComponent<PixelPerfectCamera>();
            ppc.assetsPPU = 16;
            ppc.refResolutionX = 320;
            ppc.refResolutionY = 180;
            ppc.gridSnapping = PixelPerfectCamera.GridSnapping.UpscaleRenderTexture;
            ppc.cropFrame = PixelPerfectCamera.CropFrame.None;

            if (cam.GetComponent<CinemachineBrain>() == null)
                cam.gameObject.AddComponent<CinemachineBrain>();

            var vcamGO = new GameObject("CM Player Camera");
            vcamGO.transform.position = cam.transform.position;
            var vcam = vcamGO.AddComponent<CinemachineCamera>();
            var lens = vcam.Lens;
            lens.OrthographicSize = 180f / 16f / 2f; // 5.625: lo corrige CinemachinePixelPerfect
            vcam.Lens = lens;
            vcam.Follow = followTarget;

            var composer = vcamGO.AddComponent<CinemachinePositionComposer>();
            composer.CameraDistance = 10f;
            composer.Damping = new Vector3(0.25f, 0.4f, 0f);      // [AJUSTAR]
            var composition = composer.Composition;
            composition.DeadZone.Enabled = true;
            composition.DeadZone.Size = new Vector2(0.08f, 0.25f); // dead zone horizontal (sección 13) [AJUSTAR]
            composer.Composition = composition;

            vcamGO.AddComponent<CinemachinePixelPerfect>();
        }

        // ------------------------------------------------------------------ Arte placeholder

        static void CreatePlaceholderSprites()
        {
            var white = new Color32(255, 255, 255, 255);
            var mark = new Color32(60, 60, 60, 255);

            // De pie: 12×30 px centrado en x = 24, base en y = 12. Marca a la derecha a la altura de la cabeza.
            CreateSpriteIfMissing(PlayerStandPath, 48, 64, PlayerPivot, px =>
            {
                FillRect(px, 48, 18, 12, 29, 41, white);
                FillRect(px, 48, 27, 36, 29, 38, mark);
            });

            // Agachado: 12×15 px.
            CreateSpriteIfMissing(PlayerCrouchPath, 48, 64, PlayerPivot, px =>
            {
                FillRect(px, 48, 18, 12, 29, 26, white);
                FillRect(px, 48, 27, 22, 29, 24, mark);
            });

            var center = new Vector2(0.5f, 0.5f);

            CreateSpriteIfMissing(TileGroundPath, 16, 16, center, px =>
            {
                FillRect(px, 16, 0, 0, 15, 15, new Color32(52, 42, 74, 255));
                FillRect(px, 16, 1, 1, 14, 14, new Color32(92, 76, 128, 255));
            });

            CreateSpriteIfMissing(TilePlatformPath, 16, 16, center, px =>
            {
                FillRect(px, 16, 0, 12, 15, 15, new Color32(40, 120, 115, 255));
                FillRect(px, 16, 0, 15, 15, 15, new Color32(140, 230, 215, 255));
            });

            CreateSpriteIfMissing(TileGridPath, 16, 16, center, px =>
            {
                var line = new Color32(255, 255, 255, 22);
                FillRect(px, 16, 0, 0, 15, 0, line);
                FillRect(px, 16, 0, 1, 0, 15, line);
            });
        }

        static void FillRect(Color32[] px, int width, int x0, int y0, int x1, int y1, Color32 c)
        {
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    px[y * width + x] = c;
        }

        /// <summary>Crea el PNG solo si no existe, para no pisar el arte hecho en Piskel.</summary>
        static void CreateSpriteIfMissing(string path, int w, int h, Vector2 pivot, System.Action<Color32[]> paint)
        {
            if (File.Exists(path)) return;

            var px = new Color32[w * h];
            paint(px);
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels32(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMode = (int)SpriteImportMode.Single;
            settings.spritePixelsPerUnit = 16;
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.filterMode = FilterMode.Point;
            settings.mipmapEnabled = false;
            settings.alphaIsTransparency = true;
            importer.SetTextureSettings(settings);
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        static Sprite LoadSprite(string path) =>
            AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();

        // ------------------------------------------------------------------ Assets

        static void CreateTileAssets()
        {
            EnsureTile("Tile_Ground", TileGroundPath, Tile.ColliderType.Grid);
            // Sprite: el collider sigue la forma del sprite (solo las 4 filas de arriba), no la celda completa.
            EnsureTile("Tile_Platform", TilePlatformPath, Tile.ColliderType.Sprite);
            EnsureTile("Tile_Grid", TileGridPath, Tile.ColliderType.None);
        }

        static void EnsureTile(string name, string spritePath, Tile.ColliderType collider)
        {
            string path = $"{TileAssetsDir}/{name}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.sprite = LoadSprite(spritePath);
            tile.colliderType = collider;
            tile.color = Color.white;
            EditorUtility.SetDirty(tile);
        }

        static PlayerMovementData EnsureMovementData()
        {
            var data = AssetDatabase.LoadAssetAtPath<PlayerMovementData>(MovementDataPath);
            if (data != null) return data;
            data = ScriptableObject.CreateInstance<PlayerMovementData>();
            AssetDatabase.CreateAsset(data, MovementDataPath);
            return data;
        }

        static PhysicsMaterial2D EnsureNoFrictionMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(NoFrictionPath);
            if (mat != null) return mat;
            mat = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(mat, NoFrictionPath);
            return mat;
        }

        // ------------------------------------------------------------------ Utilidades

        static bool CheckLayers()
        {
            var missing = RequiredLayers.Where(l => LayerMask.NameToLayer(l) < 0).ToList();
            if (missing.Count == 0) return true;
            EditorUtility.DisplayDialog("Hito 1",
                "Faltan capas en Project Settings → Tags and Layers: " + string.Join(", ", missing), "OK");
            return false;
        }

        static void EnsureFolders()
        {
            foreach (var dir in new[] { PlaceholderDir, TilesArtDir, TileAssetsDir, PlayerDataDir, PhysicsDir, PlayerPrefabDir, SceneDir })
                EnsureFolder(dir);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static void SetRef(Object target, string property, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(property);
            if (prop == null) { Debug.LogError($"[Hito 1] {target.GetType().Name} no tiene '{property}'."); return; }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetMask(Object target, string property, int mask)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(property);
            if (prop == null) { Debug.LogError($"[Hito 1] {target.GetType().Name} no tiene '{property}'."); return; }
            prop.intValue = mask;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddSceneToBuild(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Any(s => s.path == path)) return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
