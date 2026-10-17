using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Abilities;
using Game.Audio;
using Game.Combat;
using Game.Player;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.EditorTools
{
    /// <summary>
    /// Hito 4, fase B: las cinco habilidades (taladro, granada, espada, lanzallamas y láser).
    /// - Arte placeholder en tiras horizontales (un PNG por animación, frames del mismo tamaño). Si
    ///   reemplazas un PNG por tu arte con el mismo nombre y tamaño de frame, al volver a ejecutar el
    ///   setup se vuelve a cortar (aunque tenga más frames) y se asigna a los prefabs.
    /// - Prefabs de efecto: se crean solo si no existen, para no pisar los valores ajustados. Las
    ///   referencias (arte, prefabs, sonidos) se reasignan siempre.
    /// </summary>
    public static partial class GameSetup
    {
        const string AbilityArtDir = Root + "/Art/Abilities";
        const string VeyronAbilitiesDir = AbilitiesDir + "/Veyron";

        // Arte
        const string GrenadeSpinPath = AbilityArtDir + "/Grenade/Grenade_Spin.png";
        const string ExplosionArtPath = AbilityArtDir + "/Grenade/Explosion.png";
        const string SlashHPath = AbilityArtDir + "/FireSword/SwordSlash_Horizontal.png";
        const string SlashDPath = AbilityArtDir + "/FireSword/SwordSlash_Diagonal.png";
        const string SlashVPath = AbilityArtDir + "/FireSword/SwordSlash_Vertical.png";
        const string FlamePuffPath = AbilityArtDir + "/Flamethrower/FlamePuff.png";
        const string DrillHPath = AbilityArtDir + "/Drill/Drill_Horizontal.png";
        const string DrillDPath = AbilityArtDir + "/Drill/Drill_Diagonal.png";
        const string DrillVPath = AbilityArtDir + "/Drill/Drill_Vertical.png";
        const string LaserBeam0Path = AbilityArtDir + "/Laser/Laser_Beam_0.png";
        const string LaserBeam1Path = AbilityArtDir + "/Laser/Laser_Beam_1.png";
        const string LaserOriginPath = AbilityArtDir + "/Laser/Laser_Origin.png";
        const string LaserImpactPath = AbilityArtDir + "/Laser/Laser_Impact.png";
        const string DrillIconPath = IconsDir + "/Icon_Drill.png";
        const string LaserIconPath = IconsDir + "/Icon_Laser.png";
        const string LaserMaterialPath = MaterialsDir + "/Laser_Beam.mat";
        const string GrenadeMaterialPath = PhysicsDir + "/GrenadeBounce.physicsMaterial2D";

        // Prefabs
        const string DrillProjectilePrefabPath = AbilityPrefabDir + "/Drill_Projectile.prefab";
        const string DrillEffectPrefabPath = AbilityPrefabDir + "/Ability_Drill.prefab";
        const string GrenadePrefabPath = AbilityPrefabDir + "/Grenade.prefab";
        const string ExplosionPrefabPath = AbilityPrefabDir + "/Explosion.prefab";
        const string GrenadeEffectPrefabPath = AbilityPrefabDir + "/Ability_RubberGrenade.prefab";
        const string SwordEffectPrefabPath = AbilityPrefabDir + "/Ability_FireSword.prefab";
        const string FlamePuffPrefabPath = AbilityPrefabDir + "/FlamePuff.prefab";
        const string FlamethrowerEffectPrefabPath = AbilityPrefabDir + "/Ability_Flamethrower.prefab";
        const string LaserEffectPrefabPath = AbilityPrefabDir + "/Ability_Laser.prefab";

        // Datos
        const string DrillPath = VeyronAbilitiesDir + "/Drill.asset";
        const string LaserPath = VeyronAbilitiesDir + "/Laser.asset";

        static readonly Vector2 Center = new(0.5f, 0.5f);

        // ------------------------------------------------------------------ Entrada

        /// <summary>Arte, prefabs y AbilityData de las cinco habilidades. Lo llama CreateAbilityPrefabsAndData.</summary>
        static void CreatePhaseBAbilities()
        {
            foreach (var dir in new[] { AbilityArtDir + "/Grenade", AbilityArtDir + "/FireSword",
                         AbilityArtDir + "/Flamethrower", AbilityArtDir + "/Drill", AbilityArtDir + "/Laser",
                         VeyronAbilitiesDir, MaterialsDir, PhysicsDir })
                EnsureFolder(dir);

            CreateAbilityArt();
            foreach (var name in new[] { "Sfx_GrenadeThrow", "Sfx_GrenadeBounce", "Sfx_Explosion", "Sfx_SwordSlash",
                         "Sfx_SwordDeflect", "Sfx_DrillShoot", "Sfx_LaserCharge" })
                EnsureSfx(name);
            EnsureLoopSfx("Sfx_FlamethrowerLoop");
            EnsureLoopSfx("Sfx_LaserLoop");

            CreateDrillPrefabs();
            CreateGrenadePrefabs();
            CreateSwordPrefab();
            CreateFlamethrowerPrefabs();
            CreateLaserPrefab();
        }

        static SfxData EnsureLoopSfx(string name)
        {
            bool existed = AssetDatabase.LoadAssetAtPath<SfxData>($"{AudioDataDir}/{name}.asset") != null;
            var sfx = EnsureSfx(name);
            if (!existed)
            {
                sfx.loop = true;
                sfx.maxVoices = 1;
                EditorUtility.SetDirty(sfx);
            }
            return sfx;
        }

        static AbilityEffect LoadEffect(string path) =>
            AssetDatabase.LoadAssetAtPath<GameObject>(path)?.GetComponent<AbilityEffect>();

        // ------------------------------------------------------------------ Taladro

        static void CreateDrillPrefabs()
        {
            EnsurePrefab(DrillProjectilePrefabPath, "Drill_Projectile", go =>
            {
                go.layer = LayerMask.NameToLayer("PlayerProjectile");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 20;
                var drill = go.AddComponent<DrillProjectile>();
                SetMask(drill, "targetMask", LayerMask.GetMask("EnemyHurtbox"));
                SetMask(drill, "solidMask", LayerMask.GetMask("Ground", "AbilityObstacle"));
                SetFloat(drill, "radius", 0.3125f);
                SetFloat(drill, "maxLifetime", 3f);
                SetInt(drill, "pierceCount", -1); // atraviesa sin límite (8.2)
            }, go =>
            {
                var drill = go.GetComponent<DrillProjectile>();
                AssignDirectional(drill, "frames", DrillHPath, DrillDPath, DrillVPath);
            });

            EnsurePrefab(DrillEffectPrefabPath, "Ability_Drill", go =>
            {
                var effect = go.AddComponent<DrillEffect>();
                SetFloat(effect, "lockDuration", 0.15f);
            }, go =>
            {
                var proj = AssetDatabase.LoadAssetAtPath<GameObject>(DrillProjectilePrefabPath);
                SetRef(go.GetComponent<DrillEffect>(), "projectilePrefab", proj != null ? proj.GetComponent<DrillProjectile>() : null);
            });
        }

        // ------------------------------------------------------------------ Granada

        static void CreateGrenadePrefabs()
        {
            var bounce = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(GrenadeMaterialPath);
            if (bounce == null)
            {
                bounce = new PhysicsMaterial2D("GrenadeBounce") { bounciness = 0.6f, friction = 0.1f }; // [AJUSTAR]
                AssetDatabase.CreateAsset(bounce, GrenadeMaterialPath);
            }

            EnsurePrefab(GrenadePrefabPath, "Grenade", go =>
            {
                go.layer = LayerMask.NameToLayer("PlayerProjectile");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 20;
                go.AddComponent<Rigidbody2D>();
                var circle = go.AddComponent<CircleCollider2D>();
                circle.radius = 0.25f; // 4 px
                go.AddComponent<RubberGrenade>();
            }, go =>
            {
                go.GetComponent<CircleCollider2D>().sharedMaterial =
                    AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(GrenadeMaterialPath);
                var grenade = go.GetComponent<RubberGrenade>();
                SetRefArray(grenade, "spinFrames", LoadFrames(GrenadeSpinPath));
                SetRef(grenade, "bounceSfx", EnsureSfx("Sfx_GrenadeBounce"));
            });

            EnsurePrefab(ExplosionPrefabPath, "Explosion", go =>
            {
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 30;
                go.AddComponent<Explosion>();
            }, go => SetRefArray(go.GetComponent<Explosion>(), "frames", LoadFrames(ExplosionArtPath)));

            EnsurePrefab(GrenadeEffectPrefabPath, "Ability_RubberGrenade", go =>
            {
                var effect = go.AddComponent<RubberGrenadeEffect>();
                SetFloat(effect, "lockDuration", 0.15f);
                SetMask(effect, "targetMask", LayerMask.GetMask("EnemyHurtbox"));
                SetMask(effect, "solidMask", LayerMask.GetMask("Ground", "AbilityObstacle"));
            }, go =>
            {
                var effect = go.GetComponent<RubberGrenadeEffect>();
                SetRef(effect, "grenadePrefab", AssetDatabase.LoadAssetAtPath<GameObject>(GrenadePrefabPath)?.GetComponent<RubberGrenade>());
                SetRef(effect, "explosionPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ExplosionPrefabPath)?.GetComponent<Explosion>());
                SetRef(effect, "explosionSfx", EnsureSfx("Sfx_Explosion"));
            });
        }

        // ------------------------------------------------------------------ Espada

        static void CreateSwordPrefab()
        {
            EnsurePrefab(SwordEffectPrefabPath, "Ability_FireSword", go =>
            {
                var effect = go.AddComponent<MeleeEffect>();
                // Hitboxes mirando a la derecha, con el pivote del jugador en los pies (1.5 × 1.5 u, 8.2).
                SetRef(effect, "hitboxForward", MeleeHitbox(go, "Hitbox_Forward", new Vector2(1.0f, 1.1f), 0f, new Vector2(1.5f, 1.5f)));
                SetRef(effect, "hitboxDiagUp", MeleeHitbox(go, "Hitbox_DiagUp", new Vector2(0.9f, 2.1f), 45f, new Vector2(1.5f, 1.5f)));
                SetRef(effect, "hitboxUp", MeleeHitbox(go, "Hitbox_Up", new Vector2(0.125f, 2.6f), 0f, new Vector2(1.5f, 1.5f)));
                SetRef(effect, "hitboxDown", MeleeHitbox(go, "Hitbox_Down", new Vector2(0.125f, -0.5f), 0f, new Vector2(1.5f, 1.5f)));
                SetRef(effect, "hitboxCrouch", MeleeHitbox(go, "Hitbox_Crouch", new Vector2(1.0f, 0.5f), 0f, new Vector2(1.5f, 1f)));

                var vfx = Child(go.transform, "SlashVfx", 0);
                var sr = vfx.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 25;
                SetRef(effect, "vfx", sr);
            }, go =>
            {
                var effect = go.GetComponent<MeleeEffect>();
                // Sin VFX aparte: la llama va en el clip del torso.
                var so = new SerializedObject(effect);
                foreach (var dir in new[] { "horizontal", "diagonal", "vertical" })
                    so.FindProperty("vfxFrames." + dir).arraySize = 0;
                // Ventana de golpe: frames 1–3 del tajo dibujado (antes 1–2).
                var to = so.FindProperty("activeToFrame");
                if (so.FindProperty("activeFromFrame").intValue == 1 && to.intValue == 2) to.intValue = 3;
                so.ApplyModifiedPropertiesWithoutUndo();
                SetRef(effect, "deflectSfx", EnsureSfx("Sfx_SwordDeflect"));

                // Hitboxes medidas sobre la llama del arte (en unidades, mirando a la derecha, pies en 0).
                // Solo se cambian si siguen con los valores provisionales.
                MigrateHitbox(go, "Hitbox_Forward", new Vector2(1.0f, 1.1f), new Vector2(0.9375f, 1.28125f), new Vector2(3.5f, 1.6875f));
                MigrateHitbox(go, "Hitbox_Crouch", new Vector2(1.0f, 0.5f), new Vector2(1.25f, 1.0625f), new Vector2(3.5f, 2f));
                MigrateHitbox(go, "Hitbox_Up", new Vector2(0.125f, 2.6f), new Vector2(0f, 2.5f), new Vector2(2.125f, 2.5f));
                MigrateHitbox(go, "Hitbox_Down", new Vector2(0.125f, -0.5f), new Vector2(0.1875f, 0.0625f), new Vector2(2f, 1.375f));
            });
        }

        static void MigrateHitbox(GameObject root, string name, Vector2 oldPosition, Vector2 newPosition, Vector2 newSize)
        {
            var t = root.transform.Find(name);
            if (t == null || ((Vector2)t.localPosition - oldPosition).sqrMagnitude > 0.0001f) return;
            t.localPosition = newPosition;
            var so = new SerializedObject(t.GetComponent<DamageHitbox>());
            so.FindProperty("size").vector2Value = newSize;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static DamageHitbox MeleeHitbox(GameObject parent, string name, Vector2 position, float angle, Vector2 size)
        {
            var go = Child(parent.transform, name, 0, position);
            go.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            var hitbox = go.AddComponent<DamageHitbox>();
            var so = new SerializedObject(hitbox);
            so.FindProperty("size").vector2Value = size;
            so.FindProperty("targetMask").intValue = LayerMask.GetMask("EnemyHurtbox");
            so.FindProperty("projectileMask").intValue = LayerMask.GetMask("EnemyProjectile");
            so.FindProperty("mode").enumValueIndex = (int)DamageHitbox.HitMode.OncePerTarget;
            so.ApplyModifiedPropertiesWithoutUndo();
            return hitbox;
        }

        // ------------------------------------------------------------------ Lanzallamas

        static void CreateFlamethrowerPrefabs()
        {
            EnsurePrefab(FlamePuffPrefabPath, "FlamePuff", go =>
            {
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 25;
                go.AddComponent<FlamePuff>();
            }, null);

            EnsurePrefab(FlamethrowerEffectPrefabPath, "Ability_Flamethrower", go =>
            {
                var effect = go.AddComponent<FlamethrowerEffect>();
                var zone = Child(go.transform, "DamageZone", 0).AddComponent<DamageHitbox>();
                var so = new SerializedObject(zone);
                so.FindProperty("targetMask").intValue = LayerMask.GetMask("EnemyHurtbox");
                so.FindProperty("mode").enumValueIndex = (int)DamageHitbox.HitMode.Tick;
                so.FindProperty("tickInterval").floatValue = 0.15f;
                so.ApplyModifiedPropertiesWithoutUndo();
                SetRef(effect, "damageZone", zone);
                SetMask(effect, "solidMask", LayerMask.GetMask("Ground", "AbilityObstacle"));
            }, go =>
            {
                var effect = go.GetComponent<FlamethrowerEffect>();
                SetRef(effect, "puffPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(FlamePuffPrefabPath)?.GetComponent<FlamePuff>());
                SetRefArray(effect, "puffFrames", LoadFrames(FlamePuffPath));
            });
        }

        // ------------------------------------------------------------------ Láser

        static void CreateLaserPrefab()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(LaserMaterialPath);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
                if (shader == null)
                {
                    Debug.LogError("[Setup] No se encontró un shader de sprites para el láser.");
                    return;
                }
                material = new Material(shader) { name = "Laser_Beam" };
                AssetDatabase.CreateAsset(material, LaserMaterialPath);
            }
            material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(LaserBeam0Path);
            EditorUtility.SetDirty(material);

            EnsurePrefab(LaserEffectPrefabPath, "Ability_Laser", go =>
            {
                var caster = go.AddComponent<BeamCaster>();
                SetMask(caster, "targetMask", LayerMask.GetMask("EnemyHurtbox"));
                SetMask(caster, "solidMask", LayerMask.GetMask("Ground", "AbilityObstacle"));

                var effect = go.AddComponent<LaserEffect>();
                var line = Child(go.transform, "Beam", 0).AddComponent<LineRenderer>();
                line.positionCount = 2;
                line.useWorldSpace = true;
                line.textureMode = LineTextureMode.Tile;
                line.numCapVertices = 0;
                line.numCornerVertices = 0;
                line.sortingOrder = 26;
                line.startWidth = line.endWidth = 0.375f;
                line.textureScale = Vector2.one; // la textura de 16 px se repite cada unidad
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;
                SetRef(effect, "line", line);

                var origin = Child(go.transform, "Origin", 0).AddComponent<SpriteRenderer>();
                origin.sortingOrder = 27;
                SetRef(effect, "originSprite", origin);
                var impact = Child(go.transform, "Impact", 0).AddComponent<SpriteRenderer>();
                impact.sortingOrder = 27;
                SetRef(effect, "impactSprite", impact);
            }, go =>
            {
                var effect = go.GetComponent<LaserEffect>();
                go.GetComponentInChildren<LineRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(LaserMaterialPath);
                SetRefArray(effect, "beamTextures", new Object[]
                {
                    AssetDatabase.LoadAssetAtPath<Texture2D>(LaserBeam0Path),
                    AssetDatabase.LoadAssetAtPath<Texture2D>(LaserBeam1Path),
                });
                SetRefArray(effect, "originFrames", LoadFrames(LaserOriginPath));
                SetRefArray(effect, "impactFrames", LoadFrames(LaserImpactPath));
            });
        }

        // ------------------------------------------------------------------ AbilityData

        /// <summary>
        /// Valores de 8.2 y 8.3. Los assets que ya existen solo se actualizan en lo que cambió el diseño
        /// (granada y espada en 4 direcciones) y en los efectos de prueba de la fase A.
        /// </summary>
        static void UpdateAbilityData()
        {
            var melt = AssetDatabase.LoadAssetAtPath<ObstacleInteraction>(MeltPath);
            var breakInteraction = AssetDatabase.LoadAssetAtPath<ObstacleInteraction>(BreakPath);

            EnsureAbility(FlamethrowerPath, "flamethrower", "Lanzallamas", FlamethrowerIconPath, a =>
            {
                a.activation = AbilityActivation.Hold;
                a.energyPerSecond = 6f;
                a.usesAim = true;
                a.usableWhileCrouching = true;
                a.moveSpeedMultiplier = 0.5f;
                a.torsoPose = AbilityTorsoPose.ShootPose;
                a.obstacleInteractions = melt != null ? new[] { melt } : new ObstacleInteraction[0];
            }, LoadEffect(FlamethrowerEffectPrefabPath));

            EnsureAbility(GrenadePath, "rubber_grenade", "Granada de goma", GrenadeIconPath, a =>
            {
                a.activation = AbilityActivation.Press;
                a.energyCost = 3f;
                a.cooldown = 0.3f;
                a.usableWhileCrouching = true;
                a.moveSpeedMultiplier = 1f;
            }, LoadEffect(GrenadeEffectPrefabPath));

            EnsureAbility(SwordPath, "fire_sword", "Espada de fuego", SwordIconPath, a =>
            {
                a.activation = AbilityActivation.Press;
                a.energyCost = 2f;
                a.usableWhileCrouching = true;
                a.moveSpeedMultiplier = 1f;
                a.torsoPose = AbilityTorsoPose.CustomClip;
            }, LoadEffect(SwordEffectPrefabPath));

            EnsureAbility(DrillPath, "drill", "Taladro", DrillIconPath, a =>
            {
                a.activation = AbilityActivation.Press;
                a.energyCost = 3f;
                a.maxInstancesOnScreen = 1;
                a.usesAim = true;
                a.usableWhileCrouching = true;
                a.moveSpeedMultiplier = 1f;
                a.torsoPose = AbilityTorsoPose.ShootPose;
                a.obstacleInteractions = breakInteraction != null ? new[] { breakInteraction } : new ObstacleInteraction[0];
            }, LoadEffect(DrillEffectPrefabPath));

            EnsureAbility(LaserPath, "laser", "Láser", LaserIconPath, a =>
            {
                a.activation = AbilityActivation.Hold;
                a.energyPerSecond = 8f;
                a.usesAim = true;
                a.usableWhileCrouching = true;
                a.moveSpeedMultiplier = 0f;
                a.torsoPose = AbilityTorsoPose.ShootPose;
            }, LoadEffect(LaserEffectPrefabPath));

            // Cambio de diseño del 2026-10-14: granada y espada en 4 direcciones.
            var grenade = AssetDatabase.LoadAssetAtPath<AbilityData>(GrenadePath);
            grenade.usesAim = true;
            if (grenade.torsoPose == AbilityTorsoPose.ShootPoseForward) grenade.torsoPose = AbilityTorsoPose.ShootPose;
            EditorUtility.SetDirty(grenade);

            var sword = AssetDatabase.LoadAssetAtPath<AbilityData>(SwordPath);
            sword.usesAim = true;
            // Clips del tajo, si ya están dibujados en la hoja (4.4).
            var clips = sword.customTorsoClips;
            if (clips.forward == null) clips.forward = LoadTorsoClip("Torso_Melee");
            if (clips.diagUp == null) clips.diagUp = LoadTorsoClip("Torso_Melee_DiagUp");
            if (clips.up == null) clips.up = LoadTorsoClip("Torso_Melee_Up");
            if (clips.down == null) clips.down = LoadTorsoClip("Torso_Melee_Down");
            if (clips.crouch == null) clips.crouch = LoadTorsoClip("Torso_Melee_Crouch");
            sword.customTorsoClips = clips;
            if (sword.activateSfx == null) sword.activateSfx = EnsureSfx("Sfx_SwordSlash");
            EditorUtility.SetDirty(sword);

            SetSfxIfEmpty(GrenadePath, "Sfx_GrenadeThrow", null);
            SetSfxIfEmpty(FlamethrowerPath, null, "Sfx_FlamethrowerLoop");
            SetSfxIfEmpty(DrillPath, "Sfx_DrillShoot", null);
            SetSfxIfEmpty(LaserPath, "Sfx_LaserCharge", "Sfx_LaserLoop");

            // Los efectos de prueba de la fase A ya no se usan.
            foreach (var path in new[] { TestPressPrefabPath, TestHoldPrefabPath })
                if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
        }

        static AnimationClip LoadTorsoClip(string name) =>
            AssetDatabase.LoadAssetAtPath<AnimationClip>($"{PlayerArtSetup.AnimRoot}/Torso/{name}.anim");

        static void SetSfxIfEmpty(string abilityPath, string activate, string loop)
        {
            var a = AssetDatabase.LoadAssetAtPath<AbilityData>(abilityPath);
            if (a == null) return;
            if (activate != null && a.activateSfx == null) a.activateSfx = EnsureSfx(activate);
            if (loop != null && a.loopSfx == null) a.loopSfx = EnsureLoopSfx(loop);
            EditorUtility.SetDirty(a);
        }

        /// <summary>Crea el AbilityData con sus valores iniciales; si ya existe, solo rellena lo que esté vacío.</summary>
        static void EnsureAbility(string path, string id, string displayName, string iconPath,
            Action<AbilityData> initialValues, AbilityEffect effect)
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
            // Reemplaza los efectos de prueba de la fase A (o una referencia vacía) por el real.
            bool isTestEffect = a.effectPrefab != null && a.effectPrefab.name.StartsWith("Test_");
            if ((a.effectPrefab == null || isTestEffect) && effect != null) a.effectPrefab = effect;
            EditorUtility.SetDirty(a);
        }

        // ------------------------------------------------------------------ Prefabs: utilidades

        /// <summary>
        /// Crea el prefab con build (estructura y valores iniciales) si no existe. Después, siempre,
        /// aplica refresh (referencias a arte, prefabs y sonidos) sobre el prefab guardado.
        /// </summary>
        static void EnsurePrefab(string path, string name, Action<GameObject> build, Action<GameObject> refresh)
        {
            if (!File.Exists(path))
            {
                var go = new GameObject(name);
                try
                {
                    build(go);
                    refresh?.Invoke(go);
                    PrefabUtility.SaveAsPrefabAsset(go, path);
                }
                finally { Object.DestroyImmediate(go); }
                return;
            }
            if (refresh == null) return;

            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                refresh(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        static void AssignDirectional(Object target, string property, string horizontal, string diagonal, string vertical)
        {
            SetRefArray(target, property + ".horizontal", LoadFrames(horizontal));
            SetRefArray(target, property + ".diagonal", LoadFrames(diagonal));
            SetRefArray(target, property + ".vertical", LoadFrames(vertical));
        }

        // ------------------------------------------------------------------ Arte en tiras

        /// <summary>Los sprites de una tira, en orden (Nombre_0, Nombre_1…).</summary>
        static Object[] LoadFrames(string path)
        {
            string baseName = Path.GetFileNameWithoutExtension(path) + "_";
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
                .Where(s => s.name.StartsWith(baseName))
                .OrderBy(s => int.TryParse(s.name.Substring(baseName.Length), out int i) ? i : 0)
                .Cast<Object>().ToArray();
        }

        /// <summary>
        /// Crea la tira si no existe (frames de fw × fh, de izquierda a derecha) y siempre la vuelve a
        /// cortar según el ancho actual del PNG, saltando las celdas vacías. Conserva los IDs de los
        /// sprites para no romper referencias.
        /// </summary>
        static void EnsureStrip(string path, int fw, int fh, int frames, Action<int, Func<int, int, Color32>, Action<int, int, Color32>> paintFrame)
        {
            if (!File.Exists(path))
            {
                int w = fw * frames;
                var px = new Color32[w * fh];
                for (int f = 0; f < frames; f++)
                {
                    int ox = f * fw;
                    paintFrame(f,
                        (x, y) => px[y * w + ox + x],
                        (x, y, c) => { if (x >= 0 && x < fw && y >= 0 && y < fh) px[y * w + ox + x] = c; });
                }
                var tex = new Texture2D(w, fh, TextureFormat.RGBA32, false);
                tex.SetPixels32(px);
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            SliceStrip(path, fw, fh);
        }

        static void SliceStrip(string path, int fw, int fh)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.isReadable = false;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();

            // Celdas no vacías, de izquierda a derecha y de arriba hacia abajo.
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(path));
            int cols = Mathf.Max(1, tex.width / fw), rows = Mathf.Max(1, tex.height / fh);
            var pixels = tex.GetPixels32();
            int texW = tex.width, texH = tex.height;
            Object.DestroyImmediate(tex);

            var cells = new List<RectInt>();
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    int x0 = c * fw, y0 = texH - (r + 1) * fh;
                    bool any = false;
                    for (int y = y0; y < y0 + fh && !any; y++)
                        for (int x = x0; x < x0 + fw && !any; x++)
                            if (pixels[y * texW + x].a > 0) any = true;
                    if (any) cells.Add(new RectInt(x0, y0, fw, fh));
                }

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            var existing = provider.GetSpriteRects().GroupBy(r => r.name).ToDictionary(g => g.Key, g => g.First().spriteID);

            string baseName = Path.GetFileNameWithoutExtension(path);
            var rects = new List<SpriteRect>();
            for (int i = 0; i < cells.Count; i++)
            {
                string name = $"{baseName}_{i}";
                var cell = cells[i];
                rects.Add(new SpriteRect
                {
                    name = name,
                    spriteID = existing.TryGetValue(name, out var id) ? id : GUID.Generate(),
                    rect = new Rect(cell.x, cell.y, cell.width, cell.height),
                    alignment = SpriteAlignment.Center,
                    pivot = Center,
                });
            }
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()
                ?.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
        }

        /// <summary>Textura para el LineRenderer del láser: Repeat y Point, sin sprites.</summary>
        static void EnsureBeamTexture(string path, int w, int h, Action<Action<int, int, Color32>> paint)
        {
            if (!File.Exists(path))
            {
                var px = new Color32[w * h];
                paint((x, y, c) => { if (x >= 0 && x < w && y >= 0 && y < h) px[y * w + x] = c; });
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                tex.SetPixels32(px);
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        // ------------------------------------------------------------------ Arte placeholder

        static void CreateAbilityArt()
        {
            var black = new Color32(20, 18, 30, 255);

            // Granada: 8×8, 4 frames. Un punto claro gira alrededor.
            EnsureStrip(GrenadeSpinPath, 8, 8, 4, (f, get, set) =>
            {
                Disc(set, 3.5f, 3.5f, 3.6f, black);
                Disc(set, 3.5f, 3.5f, 2.7f, new Color32(70, 190, 90, 255));
                int[,] spot = { { 5, 4 }, { 4, 5 }, { 2, 4 }, { 4, 2 } };
                set(spot[f, 0], spot[f, 1], new Color32(210, 255, 210, 255));
            });

            // Explosión: 48×48, 5 frames (radio de daño 1.5 u = 24 px).
            EnsureStrip(ExplosionArtPath, 48, 48, 5, (f, get, set) =>
            {
                float[] radius = { 8f, 15f, 21f, 23f, 23f };
                var outer = f < 4 ? new Color32(255, 140, 40, 255) : new Color32(120, 110, 120, 160);
                var inner = f < 3 ? new Color32(255, 235, 140, 255) : new Color32(90, 80, 90, 120);
                Disc(set, 23.5f, 23.5f, radius[f], outer);
                if (f < 4) Disc(set, 23.5f, 23.5f, radius[f] * 0.6f, inner);
                else Ring(set, 23.5f, 23.5f, radius[f] - 3f, radius[f], inner);
            });

            // La espada de Rosslyn no usa VFX aparte: la llama está dibujada en los clips Torso_Melee*
            // (tiras en Art/Characters/Player, ver SpritesheetLayout.strips). El VFX queda para el cuchillo.

            // Llama del lanzallamas: 16×16, 4 de fuego (de pequeña a grande) + 2 de humo.
            EnsureStrip(FlamePuffPath, 16, 16, 6, (f, get, set) =>
            {
                if (f < 4)
                {
                    float[] r = { 2.5f, 4f, 5.5f, 7f };
                    Disc(set, 7.5f, 7.5f, r[f], new Color32(255, 110, 30, 255));
                    Disc(set, 7.5f, 7.5f, r[f] * 0.6f, new Color32(255, 220, 90, 255));
                }
                else
                {
                    Disc(set, 7.5f, 7.5f, f == 4 ? 6.5f : 5f, new Color32(110, 100, 110, f == 4 ? (byte)190 : (byte)110));
                }
            });

            // Taladro: 16×16, 2 frames de giro, en 3 direcciones.
            EnsureStrip(DrillHPath, 16, 16, 2, (f, get, set) => Drill(set, f, new Vector2(1f, 0f)));
            EnsureStrip(DrillDPath, 16, 16, 2, (f, get, set) => Drill(set, f, new Vector2(1f, 1f).normalized));
            EnsureStrip(DrillVPath, 16, 16, 2, (f, get, set) => Drill(set, f, new Vector2(0f, 1f)));

            // Láser: textura de 16×6 que se repite (2 variantes), origen e impacto.
            var cyan = new Color32(90, 220, 255, 255);
            var white = new Color32(235, 255, 255, 255);
            for (int v = 0; v < 2; v++)
            {
                int variant = v;
                EnsureBeamTexture(variant == 0 ? LaserBeam0Path : LaserBeam1Path, 16, 6, set =>
                {
                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 6; y++) set(x, y, cyan);
                        set(x, 2, white);
                        set(x, 3, white);
                        if ((x + variant * 4) % 8 < 2) { set(x, 0, white); set(x, 5, white); }
                    }
                });
            }
            EnsureStrip(LaserOriginPath, 12, 12, 2, (f, get, set) =>
            {
                Disc(set, 5.5f, 5.5f, f == 0 ? 4.5f : 5.5f, cyan);
                Disc(set, 5.5f, 5.5f, 2.5f, white);
            });
            EnsureStrip(LaserImpactPath, 12, 12, 3, (f, get, set) =>
            {
                float[] r = { 3f, 5.5f, 4.5f };
                Ring(set, 5.5f, 5.5f, r[f] - 1.5f, r[f], cyan);
                Disc(set, 5.5f, 5.5f, 1.5f, white);
            });

            // Iconos de Veyron.
            CreateIcon(DrillIconPath, new Color32(150, 150, 165, 255), new Color32(255, 210, 80, 255));
            CreateIcon(LaserIconPath, new Color32(40, 120, 200, 255), new Color32(150, 235, 255, 255));
        }

        static void Disc(Action<int, int, Color32> set, float cx, float cy, float r, Color32 c)
        {
            for (int y = Mathf.FloorToInt(cy - r); y <= Mathf.CeilToInt(cy + r); y++)
                for (int x = Mathf.FloorToInt(cx - r); x <= Mathf.CeilToInt(cx + r); x++)
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r) set(x, y, c);
        }

        static void Ring(Action<int, int, Color32> set, float cx, float cy, float r0, float r1, Color32 c)
        {
            for (int y = Mathf.FloorToInt(cy - r1); y <= Mathf.CeilToInt(cy + r1); y++)
                for (int x = Mathf.FloorToInt(cx - r1); x <= Mathf.CeilToInt(cx + r1); x++)
                {
                    float d2 = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                    if (d2 <= r1 * r1 && d2 >= r0 * r0) set(x, y, c);
                }
        }

        /// <summary>Media luna de llamas abierta hacia el ángulo dado (0 = derecha).</summary>
        static void Slash(Action<int, int, Color32> set, int frame, float angleDeg)
        {
            float a = angleDeg * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            var center = new Vector2(15.5f, 15.5f) - dir * 6f;
            float spread = frame == 0 ? 35f : 70f;
            float r0 = frame == 2 ? 12f : 10f, r1 = 14f;
            var outer = frame == 2 ? new Color32(255, 140, 60, 170) : new Color32(255, 110, 30, 255);
            var inner = new Color32(255, 230, 120, 255);
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                {
                    var p = new Vector2(x, y) - center;
                    float d = p.magnitude;
                    if (d < r0 || d > r1 || Vector2.Angle(p, dir) > spread) continue;
                    set(x, y, d > r1 - 1.5f || frame == 2 ? outer : inner);
                }
        }

        /// <summary>Cono que apunta en dir, con franjas que se desplazan para simular el giro.</summary>
        static void Drill(Action<int, int, Color32> set, int frame, Vector2 dir)
        {
            var perp = new Vector2(-dir.y, dir.x);
            var c = new Vector2(7.5f, 7.5f);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    var p = new Vector2(x, y) - c;
                    float along = Vector2.Dot(p, dir);   // -7 (base) … +7 (punta)
                    float across = Mathf.Abs(Vector2.Dot(p, perp));
                    if (along < -6f || along > 7f) continue;
                    float half = (7f - along) * 0.33f;
                    if (across > half) continue;
                    bool stripe = Mathf.FloorToInt(along + 7f + frame * 2f) % 4 < 2;
                    set(x, y, stripe ? new Color32(220, 220, 235, 255) : new Color32(140, 140, 160, 255));
                }
        }
    }
}
