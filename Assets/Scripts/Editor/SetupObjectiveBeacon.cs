using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Echo.Mission;
using Echo.Portals;
using Echo.Player;

namespace Echo.EditorTools
{
    // Bloque 7.1: baliza de objetivo. Crea (idempotente) el root ObjectiveBeacon con dos
    // cilindros emisivos verticales SIN colliders, los materiales M_Beacon/M_Beacon_Halo y
    // asigna todas las referencias del script por búsqueda en el editor. Sin estáticos: se mueve.
    public static class SetupObjectiveBeacon
    {
        private const string MenuName = "Setup Objective Beacon";
        private const string BeaconName = "ObjectiveBeacon";
        private const string CoreChildName = "Beam_Core";
        private const string HaloChildName = "Beam_Halo";
        private const string MaterialFolder = "Assets/Materials";
        private const string CoreMaterialPath = MaterialFolder + "/M_Beacon.mat";
        private const string HaloMaterialPath = MaterialFolder + "/M_Beacon_Halo.mat";
        private const string LitShaderName = "Universal Render Pipeline/Lit";

        private const float BeamHeight = 12f;
        private const float CoreRadius = 0.4f;
        private const float HaloRadius = 0.9f;

        [MenuItem("Tools/ECHO/Setup Objective Beacon", false, 30)]
        public static void Execute()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning($"[ECHO] {MenuName}: solo disponible en modo edición.");
                return;
            }

            MissionManager mission = UnityEngine.Object.FindAnyObjectByType<MissionManager>(FindObjectsInactive.Include);
            PlayerController playerController =
                UnityEngine.Object.FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
            if (mission == null || playerController == null)
            {
                Debug.LogError($"[ECHO] {MenuName}: no se encontró MissionManager o PlayerController " +
                               "en la escena. Abre Assets/Scenes/SantuarioPrincipal.unity.");
                return;
            }

            EnergyFragment fragment1 = PickFragment("Fragment_1");
            EnergyFragment fragment2 = PickFragment("Fragment_2");
            EnergyFragment fragment3 = PickFragment("Fragment_3");
            Transform portalA = PickPortal("Portal_A");
            Transform portalB = PickPortal("Portal_B");
            AncestralCore core = UnityEngine.Object.FindAnyObjectByType<AncestralCore>(FindObjectsInactive.Include);

            if (fragment1 == null || portalA == null || portalB == null || core == null)
            {
                Debug.LogError($"[ECHO] {MenuName}: faltan objetivos obligatorios " +
                               "(Fragment_1, Portal_A, Portal_B o AncestralCore) en la escena.");
                return;
            }

            if (fragment2 == null || fragment3 == null)
            {
                Debug.LogWarning($"[ECHO] {MenuName}: Fragment_2 o Fragment_3 no encontrados; " +
                                 "el objetivo 'fragmento pendiente más cercano' quedará reducido.");
            }

            Material coreMaterial = EnsureBeaconMaterial(CoreMaterialPath, 0.55f, 2.2f);
            Material haloMaterial = EnsureBeaconMaterial(HaloMaterialPath, 0.16f, 1.1f);
            if (coreMaterial == null || haloMaterial == null)
            {
                return;
            }

            GameObject previous = FindObjectByName(BeaconName);
            if (previous != null)
            {
                Undo.DestroyObjectImmediate(previous);
            }

            GameObject beacon = new GameObject(BeaconName);
            beacon.transform.position = fragment1.transform.position;
            Undo.RegisterCreatedObjectUndo(beacon, MenuName);

            Renderer coreRenderer = CreateBeamCylinder(beacon.transform, CoreChildName,
                CoreRadius, BeamHeight, coreMaterial);
            Renderer haloRenderer = CreateBeamCylinder(beacon.transform, HaloChildName,
                HaloRadius, BeamHeight, haloMaterial);

            ObjectiveBeacon beaconComponent = Undo.AddComponent<ObjectiveBeacon>(beacon);
            AssignReference(beaconComponent, "missionManager", mission);
            AssignReference(beaconComponent, "player", playerController.transform);
            AssignReference(beaconComponent, "fragment1", fragment1);
            AssignReference(beaconComponent, "fragment2", fragment2);
            AssignReference(beaconComponent, "fragment3", fragment3);
            AssignReference(beaconComponent, "portalA", portalA);
            AssignReference(beaconComponent, "portalB", portalB);
            AssignReference(beaconComponent, "ancestralCore",
                core != null ? core.transform : null);
            AssignRenderers(beaconComponent, coreRenderer, haloRenderer);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log($"[ECHO] {MenuName}: '{BeaconName}' creado sin colliders y sin estáticos " +
                      "(h=12 m), materiales '{CoreMaterialPath}' y '{HaloMaterialPath}', referencias asignadas.",
                beacon);
        }

        private static Renderer CreateBeamCylinder(Transform parent, string name,
            float radius, float height, Material material)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Undo.RegisterCreatedObjectUndo(cylinder, MenuName);
            cylinder.name = name;
            cylinder.transform.SetParent(parent, false);
            cylinder.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            // El cilindro primitivo mide 2 de alto y 1 de diámetro.
            cylinder.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);

            Collider collider = cylinder.GetComponent<Collider>();
            if (collider != null)
            {
                Undo.DestroyObjectImmediate(collider);
            }

            MeshRenderer renderer = cylinder.GetComponent<MeshRenderer>();
            renderer.castShadows = false;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = material;
            return renderer;
        }

        private static EnergyFragment PickFragment(string objectName)
        {
            GameObject found = FindObjectByName(objectName);
            return found != null ? found.GetComponent<EnergyFragment>() : null;
        }

        private static Transform PickPortal(string objectName)
        {
            PortalController[] portals = UnityEngine.Object.FindObjectsByType<PortalController>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (PortalController portal in portals)
            {
                if (portal != null && portal.gameObject.name == objectName)
                {
                    return portal.transform;
                }
            }

            return null;
        }

        private static Material EnsureBeaconMaterial(string path, float alpha, float emissionIntensity)
        {
            Shader shader = Shader.Find(LitShaderName);
            if (shader == null)
            {
                Debug.LogError($"[ECHO] {MenuName}: no se encontró el shader '{LitShaderName}'.");
                return null;
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null && AssetDatabase.LoadMainAssetAtPath(path) != null)
            {
                Debug.LogError($"[ECHO] '{path}' existe pero no es un Material.");
                return null;
            }

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            Color beamColor = new Color(1f, 0.9f, 0.4f, alpha);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetColor("_BaseColor", beamColor);
            material.SetColor("_Color", beamColor);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            material.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.35f, 1f) * emissionIntensity);

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
            return material;
        }

        private static void AssignReference(Component component, string propertyName, Object value)
        {
            SerializedObject serialized = new SerializedObject(component);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"[ECHO] {MenuName}: {component.GetType().Name} no tiene la propiedad " +
                               $"serializada '{propertyName}'.");
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignRenderers(ObjectiveBeacon beacon, Renderer core, Renderer halo)
        {
            SerializedObject serialized = new SerializedObject(beacon);
            SerializedProperty property = serialized.FindProperty("pulseRenderers");
            if (property == null || !property.isArray)
            {
                Debug.LogError($"[ECHO] {MenuName}: ObjectiveBeacon no tiene la propiedad serializada " +
                               "'pulseRenderers'.");
                return;
            }

            property.arraySize = 2;
            property.GetArrayElementAtIndex(0).objectReferenceValue = core;
            property.GetArrayElementAtIndex(1).objectReferenceValue = halo;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject FindObjectByName(string name)
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                if (root.name == name)
                {
                    return root;
                }
            }

            foreach (GameObject root in roots)
            {
                Transform[] children = root.transform.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in children)
                {
                    if (child.name == name)
                    {
                        return child.gameObject;
                    }
                }
            }

            return null;
        }
    }
}
