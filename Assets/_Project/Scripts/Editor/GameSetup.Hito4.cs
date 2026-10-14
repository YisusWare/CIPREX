using System.IO;
using Game.Abilities;
using Game.Combat;
using Game.Player;
using Game.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.EditorTools
{
    /// <summary>
    /// Hito 4, fase A: capas de obstáculos, personaje (CharacterData), habilidades de prueba y HUD.
    /// </summary>
    public static partial class GameSetup
    {
        const string CharactersDir = Root + "/Data/Characters";
        const string RosslynDataDir = CharactersDir + "/Rosslyn";
        const string CharacterPath = RosslynDataDir + "/Rosslyn.asset";
        const string AbilitiesDir = Root + "/Data/Abilities";
        const string InteractionsDir = AbilitiesDir + "/Interactions";
        const string RosslynAbilitiesDir = AbilitiesDir + "/Rosslyn";
        const string AbilityPrefabDir = Root + "/Prefabs/Abilities";
        const string UIArtDir = Root + "/Art/UI";
        const string IconsDir = UIArtDir + "/AbilityIcons";

        const string BreakPath = InteractionsDir + "/Break.asset";
        const string MeltPath = InteractionsDir + "/Melt.asset";
        const string TestPressPrefabPath = AbilityPrefabDir + "/Test_PressEffect.prefab";
        const string TestHoldPrefabPath = AbilityPrefabDir + "/Test_HoldEffect.prefab";
        const string FlamethrowerPath = RosslynAbilitiesDir + "/Flamethrower.asset";
        const string GrenadePath = RosslynAbilitiesDir + "/RubberGrenade.asset";
        const string SwordPath = RosslynAbilitiesDir + "/FireSword.asset";
        const string HudSegmentPath = UIArtDir + "/HUD_Segment.png";
        const string FlamethrowerIconPath = IconsDir + "/Icon_Flamethrower.png";
        const string GrenadeIconPath = IconsDir + "/Icon_RubberGrenade.png";
        const string SwordIconPath = IconsDir + "/Icon_FireSword.png";

        static void EnsureHito4Folders()
        {
            foreach (var dir in new[] { RosslynDataDir, InteractionsDir, RosslynAbilitiesDir, AbilityPrefabDir, IconsDir })
                EnsureFolder(dir);
        }

        // ------------------------------------------------------------------ Capas

        /// <summary>
        /// Hito 4 (sección 15.2): las capas Breakable y Meltable se reemplazan por una sola, AbilityObstacle.
        /// Breakable se renombra (así conserva su número de capa) y Meltable queda libre.
        /// </summary>
        static void MigrateLayers()
        {
            var tagManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (tagManager.Length == 0) return;
            var so = new SerializedObject(tagManager[0]);
            var layers = so.FindProperty("layers");
            if (layers == null) return;

            bool hasObstacle = false;
            for (int i = 0; i < layers.arraySize; i++)
                if (layers.GetArrayElementAtIndex(i).stringValue == "AbilityObstacle") hasObstacle = true;

            bool changed = false;
            for (int i = 0; i < layers.arraySize; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                if (layer.stringValue == "Breakable")
                {
                    layer.stringValue = hasObstacle ? "" : "AbilityObstacle";
                    hasObstacle = true;
                    changed = true;
                }
                else if (layer.stringValue == "Meltable")
                {
                    layer.stringValue = "";
                    changed = true;
                }
            }
            if (!hasObstacle)
            {
                // Proyecto sin Breakable: usar la primera capa de usuario libre.
                for (int i = 8; i < layers.arraySize; i++)
                {
                    var layer = layers.GetArrayElementAtIndex(i);
                    if (!string.IsNullOrEmpty(layer.stringValue)) continue;
                    layer.stringValue = "AbilityObstacle";
                    changed = true;
                    break;
                }
            }

            if (!changed) return;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] Capas actualizadas: Breakable → AbilityObstacle, Meltable eliminada (sección 15.2).");
        }

        static void MoveLegacyAsset(string from, string to)
        {
            if (!File.Exists(from) || File.Exists(to)) return;
            EnsureFolder(Path.GetDirectoryName(to).Replace('\\', '/'));
            string error = AssetDatabase.MoveAsset(from, to);
            if (string.IsNullOrEmpty(error)) Debug.Log($"[Setup] Movido {from} → {to}");
            else Debug.LogError($"[Setup] No se pudo mover {from}: {error}");
        }

        // ------------------------------------------------------------------ Arte y datos

        static void CreateHito4ArtAndData()
        {
            EnsureHito4Folders();

            // Segmento blanco para las barras del HUD (se tiñe por código).
            CreateSpriteIfMissing(HudSegmentPath, 4, 4, new Vector2(0.5f, 0.5f), px =>
                FillRect(px, 4, 0, 0, 3, 3, new Color32(255, 255, 255, 255)));

            // Iconos placeholder de 16×16 (sección 8.8).
            CreateIcon(FlamethrowerIconPath, new Color32(255, 140, 40, 255), new Color32(255, 230, 120, 255));
            CreateIcon(GrenadeIconPath, new Color32(70, 190, 90, 255), new Color32(200, 255, 200, 255));
            CreateIcon(SwordIconPath, new Color32(220, 60, 50, 255), new Color32(255, 200, 120, 255));

            EnsureAsset<ObstacleInteraction>(BreakPath);
            EnsureAsset<ObstacleInteraction>(MeltPath);
            EnsureSfx("Sfx_AbilitySwitch");
            EnsureSfx("Sfx_AbilityNoEnergy");
        }

        static void CreateIcon(string path, Color32 main, Color32 light)
        {
            CreateSpriteIfMissing(path, 16, 16, new Vector2(0.5f, 0.5f), px =>
            {
                var border = new Color32(20, 18, 30, 255);
                FillRect(px, 16, 0, 0, 15, 15, border);
                FillRect(px, 16, 1, 1, 14, 14, main);
                FillRect(px, 16, 4, 4, 11, 11, light);
                FillRect(px, 16, 6, 6, 9, 9, main);
            });
        }

        static T EnsureAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        // ------------------------------------------------------------------ Habilidades

        /// <summary>
        /// Prefabs de efecto de prueba (fase A) y los AbilityData de Rosslyn. Los AbilityData que ya
        /// existen no se pisan: solo se rellenan referencias vacías. En la fase B cada habilidad recibe su
        /// efecto real en effectPrefab.
        /// </summary>
        static void CreateAbilityPrefabsAndData()
        {
            CreateHito4ArtAndData();
            var bullet = AssetDatabase.LoadAssetAtPath<GameObject>(BulletPrefabPath);

            // --- Efecto de prueba por pulsación: abanico de balas.
            if (!File.Exists(TestPressPrefabPath))
            {
                var go = new GameObject("Test_PressEffect");
                try
                {
                    var effect = go.AddComponent<TestProjectileAbilityEffect>();
                    if (bullet != null) SetRef(effect, "projectilePrefab", bullet.GetComponent<Projectile>());
                    PrefabUtility.SaveAsPrefabAsset(go, TestPressPrefabPath);
                }
                finally { Object.DestroyImmediate(go); }
            }

            // --- Efecto de prueba sostenido: rayo corto.
            if (!File.Exists(TestHoldPrefabPath))
            {
                var go = new GameObject("Test_HoldEffect");
                try
                {
                    var line = go.AddComponent<LineRenderer>();
                    line.positionCount = 2;
                    var effect = go.AddComponent<TestBeamAbilityEffect>();
                    SetMask(effect, "targetMask", LayerMask.GetMask("EnemyHurtbox"));
                    SetMask(effect, "solidMask", LayerMask.GetMask("Ground", "AbilityObstacle"));
                    PrefabUtility.SaveAsPrefabAsset(go, TestHoldPrefabPath);
                }
                finally { Object.DestroyImmediate(go); }
            }

            var pressEffect = AssetDatabase.LoadAssetAtPath<GameObject>(TestPressPrefabPath).GetComponent<AbilityEffect>();
            var holdEffect = AssetDatabase.LoadAssetAtPath<GameObject>(TestHoldPrefabPath).GetComponent<AbilityEffect>();
            var melt = AssetDatabase.LoadAssetAtPath<ObstacleInteraction>(MeltPath);

            // Valores de la sección 8.2 y la tabla de 8.3.
            EnsureAbility(FlamethrowerPath, "flamethrower", "Lanzallamas", FlamethrowerIconPath, a =>
            {
                a.activation = AbilityActivation.Hold;
                a.energyPerSecond = 6f;
                a.usesAim = true;
                a.usableWhileCrouching = true;
                a.moveSpeedMultiplier = 0.5f;
                a.torsoPose = AbilityTorsoPose.ShootPose;
                a.obstacleInteractions = melt != null ? new[] { melt } : new ObstacleInteraction[0];
            }, holdEffect);

            EnsureAbility(GrenadePath, "rubber_grenade", "Granada de goma", GrenadeIconPath, a =>
            {
                a.activation = AbilityActivation.Press;
                a.energyCost = 3f;
                a.cooldown = 0.3f;
                a.usesAim = false;
                a.usableWhileCrouching = true;
                a.moveSpeedMultiplier = 1f;
                a.torsoPose = AbilityTorsoPose.ShootPoseForward;
            }, pressEffect);

            EnsureAbility(SwordPath, "fire_sword", "Espada de fuego", SwordIconPath, a =>
            {
                a.activation = AbilityActivation.Press;
                a.energyCost = 2f;
                a.usesAim = false;
                a.usableWhileCrouching = true;
                a.moveSpeedMultiplier = 1f;
                a.torsoPose = AbilityTorsoPose.CustomClip;
            }, pressEffect);

            // Clips del tajo, si ya están dibujados en la hoja (Torso_Melee, Torso_Melee_Crouch).
            var sword = AssetDatabase.LoadAssetAtPath<AbilityData>(SwordPath);
            if (sword.customTorsoClip == null)
                sword.customTorsoClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(PlayerArtSetup.AnimRoot + "/Torso/Torso_Melee.anim");
            if (sword.customTorsoClipCrouch == null)
                sword.customTorsoClipCrouch = AssetDatabase.LoadAssetAtPath<AnimationClip>(PlayerArtSetup.AnimRoot + "/Torso/Torso_Melee_Crouch.anim");
            EditorUtility.SetDirty(sword);
        }

        /// <summary>Crea el AbilityData con sus valores iniciales; si ya existe, solo rellena lo que esté vacío.</summary>
        static void EnsureAbility(string path, string id, string displayName, string iconPath,
            System.Action<AbilityData> initialValues, AbilityEffect defaultEffect)
        {
            var a = AssetDatabase.LoadAssetAtPath<AbilityData>(path);
            if (a == null)
            {
                a = ScriptableObject.CreateInstance<AbilityData>();
                a.id = id;
                a.displayName = displayName;
                initialValues(a);
                AssetDatabase.CreateAsset(a, path);
            }
            if (string.IsNullOrEmpty(a.id)) a.id = id;
            if (string.IsNullOrEmpty(a.displayName)) a.displayName = displayName;
            if (a.icon == null) a.icon = LoadSprite(iconPath);
            if (a.effectPrefab == null) a.effectPrefab = defaultEffect;
            EditorUtility.SetDirty(a);
        }

        // ------------------------------------------------------------------ Personaje

        /// <summary>CharacterData de Rosslyn (sección 5.4). Solo rellena lo que esté vacío; el prefab se reasigna siempre.</summary>
        static void EnsureCharacterData()
        {
            var c = AssetDatabase.LoadAssetAtPath<CharacterData>(CharacterPath);
            if (c == null)
            {
                c = ScriptableObject.CreateInstance<CharacterData>();
                c.id = "rosslyn";
                c.displayName = "Rosslyn";
                c.description = "Soldado espacial. Bláster de largo alcance.";
                c.maxHealth = 28;
                c.maxEnergy = 28f;
                AssetDatabase.CreateAsset(c, CharacterPath);
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab != null) c.prefab = prefab.GetComponent<PlayerController>();
            if (c.movement == null) c.movement = EnsureMovementData();
            if (c.buster == null) c.buster = EnsureBusterData();
            if (c.abilities == null || c.abilities.Length == 0)
            {
                c.abilities = new[]
                {
                    AssetDatabase.LoadAssetAtPath<AbilityData>(FlamethrowerPath),
                    AssetDatabase.LoadAssetAtPath<AbilityData>(GrenadePath),
                    AssetDatabase.LoadAssetAtPath<AbilityData>(SwordPath),
                };
            }
            EditorUtility.SetDirty(c);
        }

        // ------------------------------------------------------------------ HUD

        /// <summary>Canvas pixel perfect con las barras de vida y energía y el icono de la habilidad (15.6).</summary>
        static void CreateHud()
        {
            var segment = LoadSprite(HudSegmentPath);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var canvasGO = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.sortingOrder = 10;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            canvasGO.AddComponent<PixelPerfectCanvasScaler>();

            var health = CreateBar(canvasGO.transform, "HealthBar", new Vector2(8, -8), segment,
                new Color(1f, 0.85f, 0.35f), new Color(0.2f, 0.16f, 0.25f));
            var energy = CreateBar(canvasGO.transform, "EnergyBar", new Vector2(18, -8), segment,
                new Color(0.4f, 0.85f, 1f), new Color(0.12f, 0.18f, 0.28f));

            var iconGO = UiChild(canvasGO.transform, "AbilityIcon", new Vector2(8, -70), new Vector2(16, 16));
            var icon = iconGO.AddComponent<Image>();
            icon.raycastTarget = false;
            icon.preserveAspect = true;

            var nameGO = UiChild(canvasGO.transform, "AbilityName", new Vector2(28, -70), new Vector2(140, 16));
            var text = nameGO.AddComponent<Text>();
            text.font = font;
            text.fontSize = 8;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.raycastTarget = false;

            var hud = canvasGO.AddComponent<PlayerHUD>();
            SetRef(hud, "healthBar", health);
            SetRef(hud, "energyBar", energy);
            SetRef(hud, "abilityIcon", icon);
            SetRef(hud, "abilityName", text);
        }

        static SegmentedBar CreateBar(Transform parent, string name, Vector2 position, Sprite segment, Color full, Color empty)
        {
            var go = UiChild(parent, name, position, new Vector2(8, 58));
            var frame = go.AddComponent<Image>();
            frame.sprite = segment;
            frame.color = new Color(0.04f, 0.03f, 0.07f);
            frame.raycastTarget = false;

            var bar = go.AddComponent<SegmentedBar>();
            SetRef(bar, "segmentSprite", segment);
            var so = new SerializedObject(bar);
            so.FindProperty("fullColor").colorValue = full;
            so.FindProperty("emptyColor").colorValue = empty;
            so.ApplyModifiedPropertiesWithoutUndo();
            return bar;
        }

        /// <summary>Hijo de UI anclado arriba a la izquierda, en píxeles de 320×180.</summary>
        static GameObject UiChild(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return go;
        }
    }
}
