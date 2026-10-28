using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Echo.World;

namespace Echo.EditorTools
{
    // Bloque 3: atmósfera viva. Crea StarField (cascarón de estrellas que sigue a la cámara),
    // luciérnagas nocturnas en la región A y esporas violetas permanentes en la región B.
    // Idempotente: recrea los contenedores por nombre; no toca gameplay ni portales.
    public static class SetupAmbience
    {
        private const string MenuName = "Setup Ambience";
        private const string StarName = "StarField";
        private const string AmbienceAName = "Ambience_A";
        private const string FirefliesName = "Fireflies_A";
        private const string AmbienceBName = "Ambience_B";
        private const string SporesName = "Spores_B";

        private static readonly Vector3 RegionBCenter = new Vector3(200f, 0f, 0f);

        [MenuItem("Tools/ECHO/Setup Ambience", false, 29)]
        public static void Execute()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning($"[ECHO] {MenuName}: solo disponible en modo edición.");
                return;
            }

            TimeOfDayController timeOfDay = FindTimeOfDay();
            if (timeOfDay == null)
            {
                Debug.LogError($"[ECHO] {MenuName}: no se encontró un TimeOfDayController en la escena. " +
                               "Ejecuta la herramienta con el ciclo día/noche ya montado.");
                return;
            }

            Material material = SetupVfx.EnsureParticleMaterial();
            if (material == null)
            {
                return;
            }

            CreateStars(timeOfDay, material);
            int fireflies = CreateFireflies(timeOfDay, material);
            int spores = CreateSpores(material);

            Debug.Log($"[ECHO] {MenuName}: StarField + {fireflies} luciérnagas (A, nocturnas) + " +
                      $"{spores} esporas (B, permanentes). Presupuesto: 3 sistemas, sin luces dinámicas.");
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        // ---------------------------------------------------------------- StarField

        private static void CreateStars(TimeOfDayController timeOfDay, Material material)
        {
            GameObject previous = GameObject.Find(StarName);
            if (previous != null)
            {
                Undo.DestroyObjectImmediate(previous);
            }

            GameObject stars = new GameObject(StarName);
            stars.transform.position = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(stars, MenuName);

            ParticleSystem particles = stars.AddComponent<ParticleSystem>();
            ConfigureStars(particles, material);

            StarField starField = stars.AddComponent<StarField>();
            AssignReference(starField, "timeOfDay", timeOfDay);
        }

        private static void ConfigureStars(ParticleSystem particles, Material material)
        {
            ParticleSystem.MainModule main = particles.main;
            main.duration = 320f;
            main.loop = true;
            main.prewarm = false;
            main.startDelay = 0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(320f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 1.1f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 1f, 1f, 1f), new Color(0.72f, 0.85f, 1f, 0.65f));
            main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = true;
            main.maxParticles = 500;
            main.stopAction = ParticleSystemStopAction.None;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)500) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 70f; // radio corto para vencer la niebla nocturna sin aparecer gigante
            shape.radiusThickness = 1f;
            shape.rotation = Vector3.zero;
            shape.scale = Vector3.one;

            ParticleSystemRenderer renderer = EnsureRenderer(particles);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.maxParticleSize = 0.5f;
            renderer.castShadows = false;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = material;
        }

        // ---------------------------------------------------------------- Fireflies A

        private static int CreateFireflies(TimeOfDayController timeOfDay, Material material)
        {
            GameObject container = RecreateContainer(AmbienceAName, Vector3.zero);

            GameObject fireflies = new GameObject(FirefliesName);
            fireflies.transform.SetParent(container.transform, false);
            fireflies.transform.localPosition = new Vector3(0f, 3f, 0f);
            Undo.RegisterCreatedObjectUndo(fireflies, MenuName);

            ParticleSystem particles = fireflies.AddComponent<ParticleSystem>();
            ConfigureFireflies(particles, material);

            AmbientParticles ambient = fireflies.AddComponent<AmbientParticles>();
            AssignReference(ambient, "timeOfDay", timeOfDay);

            return (int)particles.main.maxParticles;
        }

        private static void ConfigureFireflies(ParticleSystem particles, Material material)
        {
            ParticleSystem.MainModule main = particles.main;
            main.duration = 20f;
            main.loop = true;
            main.prewarm = true;
            main.startDelay = 0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.25f, 0.4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.07f, 0.12f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.95f, 0.55f, 1f));
            main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = true;
            main.maxParticles = 80; // presupuesto de luciérnagas: máx. 80
            main.stopAction = ParticleSystemStopAction.None;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(9f);
            emission.SetBursts(new ParticleSystem.Burst[0]);

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(75f, 6f, 75f); // caja de la región A, centrada en y=3

            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = true;
            noise.strength = new ParticleSystem.MinMaxCurve(0.35f);
            noise.frequency = 0.3f;
            noise.damping = true;
            noise.octaveCount = 2;
            noise.quality = ParticleSystemNoiseQuality.Medium;

            ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(CreateFireflyGradient());

            ParticleSystemRenderer renderer = EnsureRenderer(particles);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.maxParticleSize = 0.5f;
            renderer.castShadows = false;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = material;
        }

        // ---------------------------------------------------------------- Spores B

        private static int CreateSpores(Material material)
        {
            GameObject container = RecreateContainer(AmbienceBName, RegionBCenter);

            GameObject spores = new GameObject(SporesName);
            spores.transform.SetParent(container.transform, false);
            spores.transform.localPosition = new Vector3(0f, 7f, 0f);
            Undo.RegisterCreatedObjectUndo(spores, MenuName);

            ParticleSystem particles = spores.AddComponent<ParticleSystem>();
            ConfigureSpores(particles, material);

            return (int)particles.main.maxParticles;
        }

        private static void ConfigureSpores(ParticleSystem particles, Material material)
        {
            ParticleSystem.MainModule main = particles.main;
            main.duration = 30f;
            main.loop = true;
            main.prewarm = true;
            main.startDelay = 0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(10f, 14f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.75f, 0.45f, 1f, 0.8f));
            main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = true;
            main.maxParticles = 120; // presupuesto de esporas: máx. 120
            main.stopAction = ParticleSystemStopAction.None;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(9.5f);
            emission.SetBursts(new ParticleSystem.Burst[0]);

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(46f, 14f, 46f); // caja de la región B, centrada en y=7

            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = true;
            noise.strength = new ParticleSystem.MinMaxCurve(0.5f);
            noise.frequency = 0.2f;
            noise.damping = true;
            noise.octaveCount = 2;
            noise.quality = ParticleSystemNoiseQuality.Medium;

            ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(CreateSporeGradient());

            ParticleSystemRenderer renderer = EnsureRenderer(particles);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.maxParticleSize = 0.5f;
            renderer.castShadows = false;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = material;
        }

        // ---------------------------------------------------------------- Ayudantes

        private static GameObject RecreateContainer(string name, Vector3 position)
        {
            GameObject previous = GameObject.Find(name);
            if (previous != null)
            {
                Undo.DestroyObjectImmediate(previous);
            }

            GameObject container = new GameObject(name);
            container.transform.position = position;
            Undo.RegisterCreatedObjectUndo(container, MenuName);
            return container;
        }

        private static ParticleSystemRenderer EnsureRenderer(ParticleSystem particles)
        {
            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            if (renderer == null)
            {
                renderer = particles.gameObject.AddComponent<ParticleSystemRenderer>();
            }

            return renderer;
        }

        private static void AssignReference(Component component, string propertyName, Object value)
        {
            SerializedObject serialized = new SerializedObject(component);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"[ECHO] {MenuName}: el componente {component.GetType().Name} no tiene " +
                               $"la propiedad serializada '{propertyName}'.");
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TimeOfDayController FindTimeOfDay()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                TimeOfDayController[] candidates = root.GetComponentsInChildren<TimeOfDayController>(true);
                if (candidates.Length > 0)
                {
                    return candidates[0];
                }
            }

            return null;
        }

        private static Gradient CreateFireflyGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.95f, 0.55f), 0f),
                    new GradientColorKey(new Color(0.85f, 1f, 0.55f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.95f, 0.2f),
                    new GradientAlphaKey(0.35f, 0.45f),
                    new GradientAlphaKey(0.95f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private static Gradient CreateSporeGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.75f, 0.45f, 1f), 0f),
                    new GradientColorKey(new Color(0.6f, 0.35f, 0.95f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.8f, 0.25f),
                    new GradientAlphaKey(0.8f, 0.75f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }
    }
}
