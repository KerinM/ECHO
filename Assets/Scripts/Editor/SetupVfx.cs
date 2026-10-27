using System;
using System.IO;
using Echo.Mission;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Echo.EditorTools
{
    public static class SetupVfx
    {
        private const string ParticleShaderName = "Universal Render Pipeline/Particles/Unlit";
        private const string TextureFolder = "Assets/Textures";
        private const string MaterialFolder = "Assets/Materials";
        private const string PrefabFolder = "Assets/Prefabs";
        private const string SoftCirclePath = TextureFolder + "/T_VFX_SoftCircle.png";
        private const string ParticleMaterialPath = MaterialFolder + "/M_VFX_ParticleAdditive.mat";
        private const string FragmentEffectPath = PrefabFolder + "/VFX_FragmentPickup.prefab";
        private const string PortalPrefabPath = PrefabFolder + "/Portal.prefab";
        private const string PortalSwirlName = "PortalSwirl";
        private const string PortalGlowName = "PortalGlow";
        private const string LegacyParticleName = "Particle System";
        private const float PortalCenterY = 2.2f;
        private const float PortalParticleZ = -0.06f;

        private static readonly string[] FragmentPrefabPaths =
        {
            PrefabFolder + "/Fragment_1.prefab",
            PrefabFolder + "/Fragment_2.prefab",
            PrefabFolder + "/Fragment_3.prefab"
        };

        [MenuItem("Tools/ECHO/Setup Fragment Pickup VFX", false, 25)]
        public static void SetupFragmentPickup()
        {
            EnsureFolders();

            Material material = EnsureParticleMaterial();
            if (material == null)
            {
                return;
            }

            GameObject effectPrefab = EnsureFragmentPickupPrefab(material);
            if (effectPrefab == null)
            {
                return;
            }

            int assigned = AssignPickupEffect(effectPrefab);
            AssetDatabase.SaveAssets();

            if (assigned != FragmentPrefabPaths.Length)
            {
                Debug.LogWarning($"[ECHO] El efecto de recogida se asignó a {assigned} de " +
                                 $"{FragmentPrefabPaths.Length} fragmentos.", effectPrefab);
            }
            else
            {
                Debug.Log($"[ECHO] VFX_FragmentPickup creado/actualizado y asignado a los " +
                          $"{assigned} fragmentos.", effectPrefab);
            }
        }

        [MenuItem("Tools/ECHO/Setup Portal VFX", false, 26)]
        public static void SetupPortalVfx()
        {
            EnsureFolders();

            Material material = EnsureParticleMaterial();
            if (material == null)
            {
                return;
            }

            GameObject portalAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PortalPrefabPath);
            if (portalAsset == null)
            {
                Debug.LogError($"[ECHO] No se encontró '{PortalPrefabPath}'.");
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(PortalPrefabPath);
            bool configured = false;

            try
            {
                configured = ConfigurePortal(root, material);
                if (configured)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, PortalPrefabPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();

            if (configured)
            {
                Debug.Log($"[ECHO] Portal.prefab actualizado con {PortalSwirlName} y " +
                          $"{PortalGlowName}.", portalAsset);
            }
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Textures");
            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets", "Prefabs");
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static Material EnsureParticleMaterial()
        {
            Shader shader = Shader.Find(ParticleShaderName);
            if (shader == null)
            {
                Debug.LogError($"[ECHO] No se encontró el shader '{ParticleShaderName}'. " +
                               "Comprueba que URP está instalado y activo.");
                return null;
            }

            Texture2D softCircle = EnsureSoftCircleTexture();
            if (softCircle == null)
            {
                Debug.LogWarning("[ECHO] No se pudo preparar la textura suave; se usará la textura blanca por defecto.");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(ParticleMaterialPath);
            if (material == null && AssetDatabase.LoadMainAssetAtPath(ParticleMaterialPath) != null)
            {
                Debug.LogError($"[ECHO] '{ParticleMaterialPath}' existe pero no es un Material.");
                return null;
            }

            if (material == null)
            {
                material = new Material(shader);
                material.name = "M_VFX_ParticleAdditive";
                AssetDatabase.CreateAsset(material, ParticleMaterialPath);
            }

            Undo.RecordObject(material, "Setup VFX");

            if (material.shader != shader)
            {
                material.shader = shader;
            }

            material.SetTexture("_BaseMap", softCircle);
            material.SetColor("_BaseColor", Color.white);
            material.SetColor("_Color", Color.white);
            material.SetColor("_EmissionColor", Color.black);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 2f);
            material.SetFloat("_BlendOp", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.One);
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetFloat("_AlphaClip", 0f);
            material.SetFloat("_SoftParticlesEnabled", 0f);
            material.SetFloat("_CameraFadingEnabled", 0f);
            material.SetFloat("_DistortionEnabled", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.DisableKeyword("_ALPHAMODULATE_ON");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
            return material;
        }

        private static Texture2D EnsureSoftCircleTexture()
        {
            Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(SoftCirclePath);
            if (existing == null && AssetDatabase.LoadMainAssetAtPath(SoftCirclePath) != null)
            {
                Debug.LogError($"[ECHO] '{SoftCirclePath}' existe pero no es una Texture2D.");
                return null;
            }

            if (existing == null)
            {
                try
                {
                    CreateSoftCircleTextureFile();
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[ECHO] No se pudo crear '{SoftCirclePath}': {exception.Message}");
                    return null;
                }
            }

            ConfigureTextureImporter();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(SoftCirclePath);
        }

        private static void CreateSoftCircleTextureFile()
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false, false);
            Color32[] pixels = new Color32[size * size];
            float center = (size - 1) * 0.5f;
            float radius = size * 0.5f;

            try
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = x - center;
                        float dy = y - center;
                        float normalized = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy) / radius);
                        float alpha = normalized * normalized * (3f - 2f * normalized);
                        pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                    }
                }

                texture.SetPixels32(pixels);
                texture.Apply();

                byte[] png = texture.EncodeToPNG();
                if (png == null)
                {
                    throw new InvalidOperationException("EncodeToPNG devolvió null.");
                }

                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string absolutePath = Path.Combine(projectRoot, SoftCirclePath);
                string directory = Path.GetDirectoryName(absolutePath);

                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllBytes(absolutePath, png);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(
                SoftCirclePath,
                ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }

        private static void ConfigureTextureImporter()
        {
            TextureImporter importer = AssetImporter.GetAtPath(SoftCirclePath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[ECHO] No se pudo obtener el TextureImporter de '{SoftCirclePath}'.");
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 128;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.SaveAndReimport();
        }

        private static GameObject EnsureFragmentPickupPrefab(Material material)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(FragmentEffectPath);
            if (existing != null)
            {
                GameObject root = PrefabUtility.LoadPrefabContents(FragmentEffectPath);

                try
                {
                    if (!ConfigureFragmentEffect(root, material))
                    {
                        return null;
                    }

                    PrefabUtility.SaveAsPrefabAsset(root, FragmentEffectPath);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }

                AssetDatabase.SaveAssets();
                return AssetDatabase.LoadAssetAtPath<GameObject>(FragmentEffectPath);
            }

            if (AssetDatabase.LoadMainAssetAtPath(FragmentEffectPath) != null)
            {
                Debug.LogError($"[ECHO] '{FragmentEffectPath}' existe pero no es un GameObject.");
                return null;
            }

            GameObject created = new GameObject("VFX_FragmentPickup");

            try
            {
                if (!ConfigureFragmentEffect(created, material))
                {
                    return null;
                }

                PrefabUtility.SaveAsPrefabAsset(created, FragmentEffectPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(created);
            }

            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<GameObject>(FragmentEffectPath);
        }

        private static bool ConfigureFragmentEffect(GameObject root, Material material)
        {
            if (root == null)
            {
                Debug.LogError("[ECHO] No se pudo preparar el prefab VFX_FragmentPickup.");
                return false;
            }

            root.name = "VFX_FragmentPickup";
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            ParticleSystem particles = root.GetComponent<ParticleSystem>();
            if (particles == null)
            {
                particles = root.AddComponent<ParticleSystem>();
            }

            ParticleSystemRenderer renderer = root.GetComponent<ParticleSystemRenderer>();
            if (renderer == null)
            {
                renderer = root.AddComponent<ParticleSystemRenderer>();
            }

            if (particles == null || renderer == null)
            {
                Debug.LogError("[ECHO] No se pudo añadir ParticleSystem o ParticleSystemRenderer al prefab.");
                return false;
            }

            ParticleSystem.MainModule main = particles.main;
            main.duration = 0.4f;
            main.loop = false;
            main.prewarm = false;
            main.startDelay = 0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.75f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.55f, 0.95f, 1f, 1f));
            main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.maxParticles = 64;
            main.stopAction = ParticleSystemStopAction.Destroy;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[]
            {
                new ParticleSystem.Burst(0f, (short)26)
            });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.12f;
            shape.radiusThickness = 1f;

            ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(CreateBurstGradient());

            ParticleSystem.SizeOverLifetimeModule size = particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, CreateBurstSizeCurve());

            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortMode = ParticleSystemSortMode.Distance;
            renderer.maxParticleSize = 0.5f;
            renderer.castShadows = false;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = material;

            return true;
        }

        private static int AssignPickupEffect(GameObject effectPrefab)
        {
            int assigned = 0;

            for (int i = 0; i < FragmentPrefabPaths.Length; i++)
            {
                string path = FragmentPrefabPaths[i];
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                {
                    Debug.LogError($"[ECHO] No se encontró '{path}'.");
                    continue;
                }

                GameObject root = PrefabUtility.LoadPrefabContents(path);

                try
                {
                    EnergyFragment fragment = root.GetComponent<EnergyFragment>();
                    if (fragment == null)
                    {
                        fragment = root.GetComponentInChildren<EnergyFragment>(true);
                    }

                    if (fragment == null)
                    {
                        Debug.LogError($"[ECHO] '{path}' no contiene EnergyFragment.");
                        continue;
                    }

                    SerializedObject serialized = new SerializedObject(fragment);
                    SerializedProperty property = serialized.FindProperty("pickupEffect");
                    if (property == null)
                    {
                        Debug.LogError("[ECHO] EnergyFragment no tiene la propiedad serializada 'pickupEffect'.");
                        continue;
                    }

                    property.objectReferenceValue = effectPrefab;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    assigned++;
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            return assigned;
        }

        private static bool ConfigurePortal(GameObject root, Material material)
        {
            if (root == null)
            {
                Debug.LogError("[ECHO] No se pudo cargar Portal.prefab.");
                return false;
            }

            ParticleSystem swirl = FindParticleSystem(root.transform, PortalSwirlName);
            if (swirl == null)
            {
                ParticleSystem legacy = FindParticleSystem(root.transform, LegacyParticleName);
                if (legacy != null)
                {
                    legacy.gameObject.name = PortalSwirlName;
                    swirl = legacy;
                }
            }

            if (swirl == null)
            {
                swirl = CreatePortalParticleSystem(root.transform, PortalSwirlName);
            }

            RemoveDuplicateParticleSystems(root.transform, PortalSwirlName, swirl);
            RemoveLegacyParticleSystem(root.transform, swirl);

            ParticleSystem glow = FindParticleSystem(root.transform, PortalGlowName);
            if (glow == null)
            {
                glow = CreatePortalParticleSystem(root.transform, PortalGlowName);
            }

            RemoveDuplicateParticleSystems(root.transform, PortalGlowName, glow);

            if (swirl == null || glow == null)
            {
                Debug.LogError("[ECHO] No se pudieron preparar los sistemas de partículas del portal.");
                return false;
            }

            swirl.transform.SetParent(root.transform, false);
            swirl.gameObject.layer = root.layer;
            glow.transform.SetParent(root.transform, false);
            glow.gameObject.layer = root.layer;

            ConfigurePortalSwirl(swirl, material);
            ConfigurePortalGlow(glow, material);
            return true;
        }

        private static ParticleSystem CreatePortalParticleSystem(Transform parent, string name)
        {
            GameObject created = new GameObject(name);
            created.layer = parent.gameObject.layer;
            created.transform.SetParent(parent, false);
            ParticleSystem particles = created.AddComponent<ParticleSystem>();
            return particles;
        }

        private static ParticleSystem FindParticleSystem(Transform root, string name)
        {
            ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                if (systems[i].name == name)
                {
                    return systems[i];
                }
            }

            return null;
        }

        private static void RemoveDuplicateParticleSystems(Transform root, string name, ParticleSystem keep)
        {
            ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem candidate = systems[i];
                if (candidate != keep && candidate.name == name)
                {
                    UnityEngine.Object.DestroyImmediate(candidate.gameObject);
                }
            }
        }

        private static void RemoveLegacyParticleSystem(Transform root, ParticleSystem swirl)
        {
            ParticleSystem legacy = FindParticleSystem(root, LegacyParticleName);
            if (legacy != null && legacy != swirl)
            {
                UnityEngine.Object.DestroyImmediate(legacy.gameObject);
            }
        }

        private static void ConfigurePortalSwirl(ParticleSystem particles, Material material)
        {
            SetPortalTransform(particles, PortalParticleZ);

            ParticleSystem.MainModule main = particles.main;
            main.duration = 4f;
            main.loop = true;
            main.prewarm = true;
            main.startDelay = 0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.8f, 2.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.18f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.45f, 0.95f, 1f, 1f));
            main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = true;
            main.maxParticles = 120;
            main.stopAction = ParticleSystemStopAction.None;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(22f);
            emission.SetBursts(new ParticleSystem.Burst[0]);

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1.15f;
            shape.radiusThickness = 0.8f;
            shape.arc = 360f;
            shape.rotation = Vector3.zero;
            shape.randomDirectionAmount = 0.1f;

            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.orbitalZ = new ParticleSystem.MinMaxCurve(0.85f);
            velocity.radial = new ParticleSystem.MinMaxCurve(0.05f);

            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = true;
            noise.strength = new ParticleSystem.MinMaxCurve(0.06f);
            noise.frequency = 0.25f;
            noise.scrollSpeed = new ParticleSystem.MinMaxCurve(0.1f);
            noise.damping = true;
            noise.octaveCount = 2;
            noise.quality = ParticleSystemNoiseQuality.Medium;

            ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(CreatePortalGradient());

            ParticleSystem.SizeOverLifetimeModule size = particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, CreateSwirlSizeCurve());

            ConfigurePortalRenderer(particles, material, 0.5f);
        }

        private static void ConfigurePortalGlow(ParticleSystem particles, Material material)
        {
            SetPortalTransform(particles, PortalParticleZ - 0.02f);

            ParticleSystem.MainModule main = particles.main;
            main.duration = 4f;
            main.loop = true;
            main.prewarm = true;
            main.startDelay = 0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.2f, 2.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.01f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.2f, 1.5f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.35f, 0.9f, 1f, 1f));
            main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = true;
            main.maxParticles = 24;
            main.stopAction = ParticleSystemStopAction.None;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(3.5f);
            emission.SetBursts(new ParticleSystem.Burst[0]);

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.65f;
            shape.radiusThickness = 1f;
            shape.arc = 360f;
            shape.rotation = Vector3.zero;

            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = false;

            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = false;

            ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(CreateGlowGradient());

            ParticleSystem.SizeOverLifetimeModule size = particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, CreateGlowSizeCurve());

            ConfigurePortalRenderer(particles, material, 2f);
        }

        private static void SetPortalTransform(ParticleSystem particles, float z)
        {
            Transform particleTransform = particles.transform;
            particleTransform.localPosition = new Vector3(0f, PortalCenterY, z);
            particleTransform.localRotation = Quaternion.identity;
            particleTransform.localScale = Vector3.one;
            particles.gameObject.SetActive(true);
        }

        private static void ConfigurePortalRenderer(ParticleSystem particles, Material material, float maxParticleSize)
        {
            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            if (renderer == null)
            {
                renderer = particles.gameObject.AddComponent<ParticleSystemRenderer>();
            }

            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortMode = ParticleSystemSortMode.Distance;
            renderer.maxParticleSize = maxParticleSize;
            renderer.castShadows = false;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = material;
        }

        private static Gradient CreateBurstGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(0.3f, 0.9f, 1f), 0.35f),
                    new GradientColorKey(new Color(0.1f, 0.4f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.08f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private static Gradient CreatePortalGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(0.25f, 0.85f, 1f), 0.4f),
                    new GradientColorKey(new Color(0.1f, 0.4f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.9f, 0.15f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private static Gradient CreateGlowGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.7f, 1f, 1f), 0f),
                    new GradientColorKey(new Color(0.25f, 0.8f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.22f, 0.25f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private static AnimationCurve CreateBurstSizeCurve()
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 1f);
            curve.AddKey(1f, 0.2f);
            return curve;
        }

        private static AnimationCurve CreateSwirlSizeCurve()
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0.4f);
            curve.AddKey(0.3f, 1f);
            curve.AddKey(1f, 0.2f);
            return curve;
        }

        private static AnimationCurve CreateGlowSizeCurve()
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0.8f);
            curve.AddKey(0.35f, 1.2f);
            curve.AddKey(1f, 0.9f);
            return curve;
        }
    }
}
