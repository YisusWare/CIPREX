using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Audio;
using Game.Combat;
using Game.Enemies;
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
    /// Arma el contenido de prueba de los hitos 1, 2 y 4 (sección 16): sprites placeholder, tiles,
    /// datos, matriz de colisiones, prefabs, personaje, habilidades, HUD y la escena Test_Movement.
    /// Lo del Hito 4 está en GameSetup.Hito4.cs (personaje, HUD, capas) y GameSetup.Abilities.cs (habilidades).
    /// Menú: Game/Setup. Se puede volver a ejecutar; los PNG y los assets de datos que ya existen
    /// no se sobrescriben (para no perder arte ni valores ajustados).
    /// </summary>
    public static partial class GameSetup
    {
        const string Root = "Assets/_Project";
        const string PlaceholderDir = Root + "/Art/Characters/Player/Placeholder";
        const string TilesArtDir = Root + "/Art/Tiles";
        const string TileAssetsDir = Root + "/Art/Tiles/TileAssets";
        const string ProjectilesArtDir = Root + "/Art/Projectiles";
        const string EnemiesArtDir = Root + "/Art/Enemies";
        const string MaterialsDir = Root + "/Art/Materials";
        const string PlayerDataDir = Root + "/Data/Player";
        const string AudioDataDir = Root + "/Data/Audio";
        const string PhysicsDir = Root + "/Data/Physics";
        const string PlayerPrefabDir = Root + "/Prefabs/Player";
        const string ProjectilePrefabDir = Root + "/Prefabs/Projectiles";
        const string EnemyPrefabDir = Root + "/Prefabs/Enemies";
        const string LevelPrefabDir = Root + "/Prefabs/Level";
        const string SceneDir = Root + "/Scenes";

        const string InputPath = Root + "/Input/PlayerControls.inputactions";
        // Hasta el Hito 3 estaban en Data/Player; EnsureMovementData/EnsureBusterData los mueven (conservan el GUID).
        const string LegacyMovementDataPath = PlayerDataDir + "/PlayerMovementData.asset";
        const string LegacyBusterDataPath = PlayerDataDir + "/BusterData.asset";
        const string MovementDataPath = RosslynDataDir + "/Rosslyn_Movement.asset";
        const string BusterDataPath = RosslynDataDir + "/Rosslyn_Buster.asset";
        const string NoFrictionPath = PhysicsDir + "/NoFriction.physicsMaterial2D";
        const string FlashShaderPath = Root + "/Art/Shaders/Sprite-Lit-Flash.shader";
        const string FlashMaterialPath = MaterialsDir + "/Sprite-Lit-Flash.mat";

        const string PlayerPrefabPath = PlayerPrefabDir + "/Player.prefab";
        const string BulletPrefabPath = ProjectilePrefabDir + "/Bullet_Player.prefab";
        const string DummyPrefabPath = EnemyPrefabDir + "/PracticeDummy.prefab";
        const string DamageZonePrefabPath = LevelPrefabDir + "/DamageZone.prefab";

        const string ScenePath = SceneDir + "/Test_Movement.unity";
        const string TemplateScenePath = "Assets/Settings/Scenes/URP2DSceneTemplate.unity";

        const string PlayerStandPath = PlaceholderDir + "/Player_Placeholder.png";
        const string PlayerCrouchPath = PlaceholderDir + "/Player_Placeholder_Crouch.png";
        const string AimMarkerPath = PlaceholderDir + "/AimMarker_Placeholder.png";
        const string TileGroundPath = TilesArtDir + "/Tile_Ground.png";
        const string TilePlatformPath = TilesArtDir + "/Tile_Platform.png";
        const string TileGridPath = TilesArtDir + "/Tile_Grid.png";
        const string HazardPath = TilesArtDir + "/Hazard_Placeholder.png";
        const string BulletPath = ProjectilesArtDir + "/Bullet_Placeholder.png";
        const string DummyPath = EnemiesArtDir + "/Dummy_Placeholder.png";

        // Pivote del jugador: pies en (24, 12) de un lienzo de 48×64 (sección 4.1).
        static readonly Vector2 PlayerPivot = new(24f / 48f, 12f / 64f);

        static readonly string[] RequiredLayers =
        {
            "Ground", "AbilityObstacle", "Platform", "Hazard", "Player", "PlayerHurtbox",
            "PlayerProjectile", "Enemy", "EnemyHurtbox", "EnemyProjectile", "Pickup",
        };

        static readonly string[] SfxNames =
        {
            "Sfx_PlayerShoot", "Sfx_PlayerJump", "Sfx_PlayerLand",
            "Sfx_PlayerDash", "Sfx_PlayerHurt", "Sfx_PlayerDeath",
        };

        // ------------------------------------------------------------------ Menú

        [MenuItem("Game/Setup/Crear todo", priority = 0)]
        public static void CreateAll()
        {
            if (!CheckLayers()) return;
            ConfigureCollisionMatrix();
            CreateArtAndData();
            CreatePrefabs();
            CreateTestScene();
        }

        [MenuItem("Game/Setup/1. Arte placeholder y datos", priority = 20)]
        public static void CreateArtAndData()
        {
            EnsureFolders();
            CreatePlaceholderSprites();
            CreateTileAssets();
            EnsureFlashMaterial();
            EnsureMovementData();
            EnsureNoFrictionMaterial();
            foreach (var name in SfxNames) EnsureSfx(name);
            CreateHito4ArtAndData();
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] Arte placeholder, tiles y datos listos.");
        }

        [MenuItem("Game/Setup/2. Prefabs (jugador, bala, muñeco, zona de daño)", priority = 21)]
        public static void CreatePrefabs()
        {
            if (!CheckLayers()) return;
            EnsureFolders();
            if (LoadSprite(PlayerStandPath) == null) CreateArtAndData();

            CreateBulletPrefab();
            EnsureBusterData();
            CreateDummyPrefab();
            CreateDamageZonePrefab();
            CreatePlayerPrefab();
            CreateAbilityPrefabsAndData();
            EnsureCharacterData();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Game/Setup/3. Escena de prueba", priority = 22)]
        public static void CreateTestScene()
        {
            if (!CheckLayers()) return;

            if (AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath) == null ||
                AssetDatabase.LoadAssetAtPath<GameObject>(DummyPrefabPath) == null ||
                AssetDatabase.LoadAssetAtPath<CharacterData>(CharacterPath) == null)
                CreatePrefabs();
            if (AssetDatabase.LoadAssetAtPath<Tile>(TileAssetsDir + "/Tile_Ground.asset") == null)
                CreateArtAndData();

            if (File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("Setup", $"{ScenePath} ya existe. ¿Reemplazarla?", "Reemplazar", "Cancelar"))
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
                Debug.LogWarning("[Setup] No se encontró la escena plantilla URP 2D; se crea una vacía (agrega una Global Light 2D).");
                scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            }

            // Abrir la escena descarga los assets sin referencias (UnloadUnusedAssets), así que
            // todo lo que se use desde aquí se carga DESPUÉS de abrirla.
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            var dummyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DummyPrefabPath);
            var zonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DamageZonePrefabPath);
            var ground = AssetDatabase.LoadAssetAtPath<Tile>(TileAssetsDir + "/Tile_Ground.asset");
            var platform = AssetDatabase.LoadAssetAtPath<Tile>(TileAssetsDir + "/Tile_Platform.asset");
            var gridTile = AssetDatabase.LoadAssetAtPath<Tile>(TileAssetsDir + "/Tile_Grid.asset");

            // ---------- Tilemaps ----------
            var gridGO = new GameObject("Grid");
            var grid = gridGO.AddComponent<Grid>();
            grid.cellSize = Vector3.one;

            var bgMap = CreateTilemap("Background_Grid", gridGO.transform, "Default", -10, collider: false, oneWay: false);
            var groundMap = CreateTilemap("Ground", gridGO.transform, "Ground", 0, collider: true, oneWay: false);
            var platformMap = CreateTilemap("Platforms", gridGO.transform, "Platform", 0, collider: true, oneWay: true);

            PaintTestLevel(groundMap, platformMap, bgMap, ground, platform, gridTile, out Vector3 spawn);
            if (groundMap.GetUsedTilesCount() == 0)
                Debug.LogError("[Setup] El Tilemap Ground quedó vacío: no se pudieron cargar los tiles.");

            // ---------- Objetos de prueba del Hito 2 ----------
            var testRoot = new GameObject("Hito2_Pruebas").transform;
            PlaceInstance(dummyPrefab, scene, testRoot, "Dummy_Suelo", new Vector3(12f, 0f, 0f));
            PlaceInstance(dummyPrefab, scene, testRoot, "Dummy_Alto", new Vector3(7f, 4f, 0f));
            PlaceInstance(dummyPrefab, scene, testRoot, "Dummy_Plataforma", new Vector3(154f, 9f, 0f));
            PlaceInstance(zonePrefab, scene, testRoot, "ZonaDeDaño", new Vector3(25f, 0.5f, 0f));

            // ---------- Cámara ----------
            var vcam = SetupCamera(spawn);

            // ---------- Jugador (sección 5.4) ----------
            // El jugador no se coloca en la escena: PlayerSpawner instancia el prefab del personaje.
            var character = AssetDatabase.LoadAssetAtPath<CharacterData>(CharacterPath);
            var spawnerGO = new GameObject("PlayerSpawner");
            spawnerGO.transform.position = spawn;
            var spawner = spawnerGO.AddComponent<PlayerSpawner>();
            SetRef(spawner, "fallbackCharacter", character);
            SetRef(spawner, "playerCamera", vcam);

            // ---------- HUD (sección 15.6) ----------
            CreateHud();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuild(ScenePath);
            Debug.Log($"[Setup] Escena creada: {ScenePath}. Presiona Play para probar.");
        }

        [MenuItem("Game/Setup/Matriz de colisiones 2D", priority = 40)]
        public static void ConfigureCollisionMatrix()
        {
            if (!CheckLayers()) return;

            // Pares que SÍ colisionan entre las capas del juego (sección 11.1). Todo lo demás se ignora.
            // Los proyectiles y ContactDamage usan consultas (casts/overlaps), no dependen de esta matriz;
            // la matriz evita empujones y contactos físicos indeseados.
            string[] solids = { "Ground", "AbilityObstacle", "Platform" };
            var collide = new List<(string, string)>();
            foreach (var a in solids)
                foreach (var b in new[] { "Ground", "AbilityObstacle", "Platform", "Player", "Enemy", "Pickup" })
                    collide.Add((a, b));
            foreach (var s in new[] { "Ground", "AbilityObstacle" })
            {
                collide.Add((s, "PlayerProjectile")); // granada con física (Hito 4)
                collide.Add((s, "EnemyProjectile"));
            }
            collide.Add(("Platform", "PlayerProjectile")); // la granada rebota sobre las plataformas atravesables
            collide.Add(("Player", "Pickup"));
            collide.Add(("PlayerHurtbox", "Enemy"));
            collide.Add(("PlayerHurtbox", "Hazard"));
            collide.Add(("PlayerHurtbox", "EnemyProjectile"));
            collide.Add(("PlayerProjectile", "EnemyHurtbox"));
            collide.Add(("PlayerProjectile", "EnemyProjectile")); // la espada destruye proyectiles

            var gameLayers = RequiredLayers.Select(LayerMask.NameToLayer).ToArray();
            var allowed = new HashSet<(int, int)>();
            foreach (var (a, b) in collide)
            {
                int la = LayerMask.NameToLayer(a), lb = LayerMask.NameToLayer(b);
                allowed.Add((la, lb));
                allowed.Add((lb, la));
            }

            foreach (int a in gameLayers)
                foreach (int b in gameLayers)
                    Physics2D.IgnoreLayerCollision(a, b, !allowed.Contains((a, b)));

            // Guardar el ProjectSettings/Physics2DSettings.asset.
            var settings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/Physics2DSettings.asset");
            foreach (var s in settings) EditorUtility.SetDirty(s);
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] Matriz de colisiones 2D configurada.");
        }

        // ------------------------------------------------------------------ Prefabs

        static void CreatePlayerPrefab()
        {
            var input = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            if (input == null)
            {
                EditorUtility.DisplayDialog("Setup", $"No se encontró {InputPath}.", "OK");
                return;
            }

            // Solo para el tamaño del collider: los datos los reparte PlayerController.Initialize (5.4).
            var movement = EnsureMovementData();
            var noFriction = EnsureNoFrictionMaterial();
            Sprite stand = LoadSprite(PlayerStandPath);
            Sprite crouch = LoadSprite(PlayerCrouchPath);
            Sprite marker = LoadSprite(AimMarkerPath);

            int playerLayer = LayerMask.NameToLayer("Player");
            var root = new GameObject("Player") { layer = playerLayer };
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

                // Hurtbox (trigger, capa PlayerHurtbox), 1 px más chica que el collider.
                var hurtboxGO = Child(root.transform, "Hurtbox", LayerMask.NameToLayer("PlayerHurtbox"));
                var hurtbox = hurtboxGO.AddComponent<BoxCollider2D>();
                hurtbox.isTrigger = true;
                hurtbox.size = new Vector2(movement.colliderWidth - 0.125f, movement.standingHeight - 0.125f);
                hurtbox.offset = new Vector2(0f, movement.standingHeight * 0.5f);

                var motor = root.AddComponent<PlayerMotor>();
                SetRef(motor, "hurtbox", hurtbox);
                SetMask(motor, "groundMask", LayerMask.GetMask("Ground", "Platform", "AbilityObstacle"));
                SetMask(motor, "ceilingMask", LayerMask.GetMask("Ground", "AbilityObstacle"));

                var reader = root.AddComponent<PlayerInputReader>();
                SetRef(reader, "actions", input);

                // Visual (se voltea). Con los Animators del Hito 3: Legs + Torso + FullBody (sección 5.1).
                // Sin ellos: un placeholder de color.
                var legsController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerArtSetup.LegsControllerPath);
                var torsoController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerArtSetup.TorsoControllerPath);
                var fullController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerArtSetup.FullBodyControllerPath);
                bool animated = legsController != null && torsoController != null && fullController != null;

                var visual = Child(root.transform, "Visual", playerLayer);
                SpriteRenderer sr = null;
                Animator legsAnim = null, torsoAnim = null, fullAnim = null;
                if (animated)
                {
                    legsAnim = AnimatedPart(visual.transform, "Legs", playerLayer, legsController, 10, true);
                    torsoAnim = AnimatedPart(visual.transform, "Torso", playerLayer, torsoController, 11, true);
                    fullAnim = AnimatedPart(visual.transform, "FullBody", playerLayer, fullController, 10, false);
                }
                else
                {
                    var placeholder = Child(visual.transform, "Placeholder", playerLayer);
                    sr = placeholder.AddComponent<SpriteRenderer>();
                    sr.sprite = stand;
                    sr.sortingOrder = 10;
                }

                // FirePoints en la boca del cañón de cada pose del spritesheet de Rosslyn,
                // en múltiplos de 0.0625, mirando a la derecha (pivote en los pies).
                var firePoints = Child(visual.transform, "FirePoints", playerLayer);
                var fpForward = Child(firePoints.transform, "Forward", playerLayer, new Vector2(1.3125f, 1f));
                var fpDiagUp = Child(firePoints.transform, "DiagUp", playerLayer, new Vector2(0.75f, 2.3125f));
                var fpUp = Child(firePoints.transform, "Up", playerLayer, new Vector2(0f, 2.5f));
                var fpDown = Child(firePoints.transform, "Down", playerLayer, new Vector2(0f, -0.0625f));
                var fpCrouch = Child(firePoints.transform, "Crouch", playerLayer, new Vector2(1.5f, 0.125f));

                var playerBuster = root.AddComponent<PlayerBuster>();
                SetRef(playerBuster, "visualRoot", visual.transform);
                SetRef(playerBuster, "firePointForward", fpForward.transform);
                SetRef(playerBuster, "firePointDiagUp", fpDiagUp.transform);
                SetRef(playerBuster, "firePointUp", fpUp.transform);
                SetRef(playerBuster, "firePointDown", fpDown.transform);
                SetRef(playerBuster, "firePointCrouch", fpCrouch.transform);

                root.AddComponent<PlayerHealth>();
                root.AddComponent<PlayerEnergy>();
                var abilities = root.AddComponent<PlayerAbilities>();
                SetRef(abilities, "switchSfx", EnsureSfx("Sfx_AbilitySwitch"));
                SetRef(abilities, "noEnergySfx", EnsureSfx("Sfx_AbilityNoEnergy"));
                var controller = root.AddComponent<PlayerController>();

                var audio = root.AddComponent<PlayerAudio>();
                SetRef(audio, "player", controller);
                SetRef(audio, "jump", EnsureSfx("Sfx_PlayerJump"));
                SetRef(audio, "land", EnsureSfx("Sfx_PlayerLand"));
                SetRef(audio, "dash", EnsureSfx("Sfx_PlayerDash"));
                SetRef(audio, "hurt", EnsureSfx("Sfx_PlayerHurt"));
                SetRef(audio, "death", EnsureSfx("Sfx_PlayerDeath"));

                var camTarget = Child(root.transform, "CameraTarget", 0, new Vector2(1.5f, 1f));
                var look = camTarget.AddComponent<CameraLookAhead>();
                SetRef(look, "player", controller);

                // Indicador del cañón (solo depuración).
                var aimMarker = Child(root.transform, "AimMarker_Debug", playerLayer);
                var markerSr = aimMarker.AddComponent<SpriteRenderer>();
                markerSr.sprite = marker;
                markerSr.sortingOrder = 11;

                if (animated)
                {
                    var anim = root.AddComponent<PlayerAnimation>();
                    SetRef(anim, "player", controller);
                    SetRef(anim, "visual", visual.transform);
                    SetRef(anim, "legs", legsAnim);
                    SetRef(anim, "torso", torsoAnim);
                    SetRef(anim, "fullBody", fullAnim);
                }

                // Overlay de depuración. Con los Animators, el color por estado y el volteo
                // los deja de hacer (los hace PlayerAnimation); solo queda el texto y el indicador del cañón.
                var debug = root.AddComponent<PlayerDebugView>();
                SetRef(debug, "player", controller);
                SetRef(debug, "aimMarker", aimMarker.transform);
                if (!animated)
                {
                    SetRef(debug, "visual", visual.transform);
                    SetRef(debug, "body", sr);
                    SetRef(debug, "standingSprite", stand);
                    SetRef(debug, "crouchingSprite", crouch);
                }

                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
                Debug.Log($"[Setup] Prefab creado: {PlayerPrefabPath}");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static Animator AnimatedPart(Transform parent, string name, int layer, RuntimeAnimatorController controller,
            int sortingOrder, bool visible)
        {
            var go = Child(parent, name, layer);
            var r = go.AddComponent<SpriteRenderer>();
            r.sortingOrder = sortingOrder;
            r.enabled = visible;
            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;
            anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            return anim;
        }

        static void CreateBulletPrefab()
        {
            var root = new GameObject("Bullet_Player") { layer = LayerMask.NameToLayer("PlayerProjectile") };
            try
            {
                var sr = root.AddComponent<SpriteRenderer>();
                sr.sprite = LoadSprite(BulletPath);
                sr.sortingOrder = 20;

                var projectile = root.AddComponent<Projectile>();
                SetMask(projectile, "targetMask", LayerMask.GetMask("EnemyHurtbox"));
                SetMask(projectile, "solidMask", LayerMask.GetMask("Ground", "AbilityObstacle"));
                SetFloat(projectile, "radius", 0.1875f);

                PrefabUtility.SaveAsPrefabAsset(root, BulletPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static void CreateDummyPrefab()
        {
            var root = new GameObject("PracticeDummy") { layer = LayerMask.NameToLayer("Enemy") };
            try
            {
                var visual = Child(root.transform, "Visual", root.layer);
                var sr = visual.AddComponent<SpriteRenderer>();
                sr.sprite = LoadSprite(DummyPath);
                sr.sharedMaterial = EnsureFlashMaterial();
                sr.sortingOrder = 5;

                var hurtboxGO = Child(root.transform, "Hurtbox", LayerMask.NameToLayer("EnemyHurtbox"));
                var hurtbox = hurtboxGO.AddComponent<BoxCollider2D>();
                hurtbox.isTrigger = true;
                hurtbox.size = new Vector2(1f, 2f);
                hurtbox.offset = new Vector2(0f, 1f);

                var flash = root.AddComponent<DamageFlash>();
                SetRefArray(flash, "renderers", new Object[] { sr });

                var dummy = root.AddComponent<PracticeDummy>();
                SetRef(dummy, "flash", flash);

                PrefabUtility.SaveAsPrefabAsset(root, DummyPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static void CreateDamageZonePrefab()
        {
            var root = new GameObject("DamageZone") { layer = LayerMask.NameToLayer("Hazard") };
            try
            {
                // 2 tiles de ancho; los picos ocupan la mitad inferior de cada tile.
                var sr = root.AddComponent<SpriteRenderer>();
                sr.sprite = LoadSprite(HazardPath);
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(2f, 1f);
                sr.sortingOrder = 1;

                var col = root.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1.75f, 0.4375f);
                col.offset = new Vector2(0f, -0.28125f);

                var damage = root.AddComponent<ContactDamage>();
                SetInt(damage, "damage", 4);
                SetMask(damage, "targetMask", LayerMask.GetMask("PlayerHurtbox"));

                PrefabUtility.SaveAsPrefabAsset(root, DamageZonePrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static void PlaceInstance(GameObject prefab, Scene scene, Transform parent, string name, Vector3 position)
        {
            if (prefab == null) return;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
        }

        static GameObject Child(Transform parent, string name, int layer, Vector2 localPos = default)
        {
            var go = new GameObject(name) { layer = layer };
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            return go;
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

            // Pasillo largo y plano: x 0..39 (correr, dash, look ahead, muñecos y zona de daño).

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

        /// <summary>Cámara pixel perfect + Cinemachine. El objetivo lo asigna PlayerSpawner al crear al jugador.</summary>
        static CinemachineCamera SetupCamera(Vector3 spawn)
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

            var composer = vcamGO.AddComponent<CinemachinePositionComposer>();
            composer.CameraDistance = 10f;
            composer.Damping = new Vector3(0.25f, 0.4f, 0f);      // [AJUSTAR]
            var composition = composer.Composition;
            composition.DeadZone.Enabled = true;
            composition.DeadZone.Size = new Vector2(0.08f, 0.25f); // dead zone horizontal (sección 13) [AJUSTAR]
            composer.Composition = composition;

            vcamGO.AddComponent<CinemachinePixelPerfect>();
            return vcam;
        }

        // ------------------------------------------------------------------ Arte placeholder

        static void CreatePlaceholderSprites()
        {
            var white = new Color32(255, 255, 255, 255);
            var mark = new Color32(60, 60, 60, 255);
            var center = new Vector2(0.5f, 0.5f);

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

            // Indicador del cañón: 4×4 px.
            CreateSpriteIfMissing(AimMarkerPath, 4, 4, center, px =>
            {
                FillRect(px, 4, 0, 0, 3, 3, new Color32(255, 220, 60, 255));
            });

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

            // Picos: dos triángulos de 8 px de ancho en la mitad inferior del tile.
            CreateSpriteIfMissing(HazardPath, 16, 16, center, px =>
            {
                var red = new Color32(220, 60, 60, 255);
                var dark = new Color32(120, 25, 35, 255);
                FillRect(px, 16, 0, 0, 15, 0, dark);
                for (int t = 0; t < 2; t++)
                {
                    int x0 = t * 8;
                    for (int row = 1; row <= 7; row++)
                    {
                        int inset = (row - 1) / 2;
                        FillRect(px, 16, x0 + inset, row, x0 + 7 - inset, row, red);
                    }
                }
            });

            // Bala: círculo de 6 px en un lienzo de 8×8.
            CreateSpriteIfMissing(BulletPath, 8, 8, center, px =>
            {
                var core = new Color32(255, 250, 200, 255);
                var rim = new Color32(255, 200, 60, 255);
                FillRect(px, 8, 2, 1, 5, 6, rim);
                FillRect(px, 8, 1, 2, 6, 5, rim);
                FillRect(px, 8, 3, 2, 4, 5, core);
                FillRect(px, 8, 2, 3, 5, 4, core);
            });

            // Muñeco: 16×32 px en un lienzo de 24×40, pivote en los pies.
            CreateSpriteIfMissing(DummyPath, 24, 40, new Vector2(12f / 24f, 0f), px =>
            {
                var body = new Color32(196, 140, 84, 255);
                var dark = new Color32(120, 80, 45, 255);
                FillRect(px, 24, 4, 0, 19, 31, dark);
                FillRect(px, 24, 5, 1, 18, 30, body);
                FillRect(px, 24, 5, 20, 18, 21, dark); // franja
                FillRect(px, 24, 5, 10, 18, 11, dark); // franja
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

        static Material EnsureFlashMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(FlashMaterialPath);
            if (mat != null) return mat;

            var shader = AssetDatabase.LoadAssetAtPath<Shader>(FlashShaderPath);
            if (shader == null) shader = Shader.Find("Game/Sprite-Lit-Flash");
            if (shader == null)
            {
                Debug.LogError($"[Setup] No se encontró el shader {FlashShaderPath}.");
                return null;
            }
            mat = new Material(shader) { name = "Sprite-Lit-Flash" };
            AssetDatabase.CreateAsset(mat, FlashMaterialPath);
            return mat;
        }

        static PlayerMovementData EnsureMovementData()
        {
            MoveLegacyAsset(LegacyMovementDataPath, MovementDataPath);
            var data = AssetDatabase.LoadAssetAtPath<PlayerMovementData>(MovementDataPath);
            if (data != null) return data;
            data = ScriptableObject.CreateInstance<PlayerMovementData>();
            AssetDatabase.CreateAsset(data, MovementDataPath);
            return data;
        }

        static BusterData EnsureBusterData()
        {
            MoveLegacyAsset(LegacyBusterDataPath, BusterDataPath);
            var data = AssetDatabase.LoadAssetAtPath<BusterData>(BusterDataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<BusterData>();
                AssetDatabase.CreateAsset(data, BusterDataPath);
            }
            // Las referencias se reasignan siempre (los valores numéricos se respetan).
            if (data.projectilePrefab == null)
            {
                var bullet = AssetDatabase.LoadAssetAtPath<GameObject>(BulletPrefabPath);
                if (bullet != null) data.projectilePrefab = bullet.GetComponent<Projectile>();
            }
            if (data.fireSfx == null) data.fireSfx = EnsureSfx("Sfx_PlayerShoot");
            EditorUtility.SetDirty(data);
            return data;
        }

        static SfxData EnsureSfx(string name)
        {
            string path = $"{AudioDataDir}/{name}.asset";
            var sfx = AssetDatabase.LoadAssetAtPath<SfxData>(path);
            if (sfx != null) return sfx;
            EnsureFolder(AudioDataDir);
            sfx = ScriptableObject.CreateInstance<SfxData>();
            AssetDatabase.CreateAsset(sfx, path);
            return sfx;
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
            MigrateLayers();
            var missing = RequiredLayers.Where(l => LayerMask.NameToLayer(l) < 0).ToList();
            if (missing.Count == 0) return true;
            EditorUtility.DisplayDialog("Setup",
                "Faltan capas en Project Settings → Tags and Layers: " + string.Join(", ", missing), "OK");
            return false;
        }

        static void EnsureFolders()
        {
            foreach (var dir in new[]
                     {
                         PlaceholderDir, TilesArtDir, TileAssetsDir, ProjectilesArtDir, EnemiesArtDir, MaterialsDir,
                         PlayerDataDir, AudioDataDir, PhysicsDir, PlayerPrefabDir, ProjectilePrefabDir,
                         EnemyPrefabDir, LevelPrefabDir, SceneDir,
                     })
                EnsureFolder(dir);
            EnsureHito4Folders();
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static SerializedProperty Prop(Object target, string property, out SerializedObject so)
        {
            so = new SerializedObject(target);
            var prop = so.FindProperty(property);
            if (prop == null) Debug.LogError($"[Setup] {target.GetType().Name} no tiene '{property}'.");
            return prop;
        }

        static void SetRef(Object target, string property, Object value)
        {
            var prop = Prop(target, property, out var so);
            if (prop == null) return;
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetRefArray(Object target, string property, Object[] values)
        {
            var prop = Prop(target, property, out var so);
            if (prop == null) return;
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetMask(Object target, string property, int mask) => SetInt(target, property, mask);

        static void SetInt(Object target, string property, int value)
        {
            var prop = Prop(target, property, out var so);
            if (prop == null) return;
            prop.intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetFloat(Object target, string property, float value)
        {
            var prop = Prop(target, property, out var so);
            if (prop == null) return;
            prop.floatValue = value;
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
