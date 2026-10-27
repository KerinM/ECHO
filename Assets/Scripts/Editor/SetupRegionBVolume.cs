using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Echo.EditorTools
{
    public static class SetupRegionBVolume
    {
        private const string RegionName = "RegionB_Crepusculo";
        private const string VolumeName = "Volume_RegionB";
        private const string VfxFolder = "Assets/VFX";
        private const string ProfilePath = VfxFolder + "/VP_RegionB.asset";
        private const string UndoName = "Setup Region B Volume";

        private static readonly Vector3 VolumeLocalPosition = new Vector3(0f, 15f, 0f);
        private static readonly Vector3 VolumeSize = new Vector3(84f, 30f, 84f);
        private static readonly Color FilterColor = new Color(200f / 255f, 170f / 255f, 255f / 255f, 1f);

        private const float Saturation = 15f;
        private const float PostExposure = -0.3f;
        private const float VignetteIntensity = 0.3f;
        private const float BloomThreshold = 0.9f;
        private const float BloomIntensity = 1.2f;

        [MenuItem("Tools/ECHO/Setup Region B Volume", false, 23)]
        public static void Setup()
        {
            GameObject region = FindSceneObjectByName(RegionName);
            if (region == null)
            {
                Debug.LogError($"[ECHO] No se encontró '{RegionName}' en la escena activa. " +
                               "Abre Assets/Scenes/SantuarioPrincipal.unity y vuelve a ejecutar el menú.");
                return;
            }

            CheckMainCameraPostProcessing();

            VolumeProfile profile = EnsureProfile();
            if (profile == null)
            {
                return;
            }

            ConfigureProfile(profile);

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UndoName);

            GameObject volumeObject = EnsureVolumeObject(region.transform);
            if (volumeObject == null)
            {
                Undo.CollapseUndoOperations(undoGroup);
                return;
            }

            BoxCollider box = EnsureBoxCollider(volumeObject);
            Volume volume = EnsureVolumeComponent(volumeObject);

            if (box == null || volume == null)
            {
                Undo.CollapseUndoOperations(undoGroup);
                return;
            }

            Undo.RecordObject(box, UndoName);
            box.isTrigger = true;
            box.center = Vector3.zero;
            box.size = VolumeSize;
            box.enabled = true;

            Undo.RecordObject(volume, UndoName);
            if (volume.HasInstantiatedProfile())
            {
                volume.profile = null;
            }

            volume.sharedProfile = profile;
            volume.isGlobal = false;
            volume.weight = 1f;
            volume.priority = 1f;
            volume.blendDistance = 8f;
            volume.enabled = true;

            EditorUtility.SetDirty(box);
            EditorUtility.SetDirty(volume);
            EditorSceneManager.MarkSceneDirty(volumeObject.scene);
            Undo.CollapseUndoOperations(undoGroup);

            Selection.activeGameObject = volumeObject;
            EditorGUIUtility.PingObject(volumeObject);

            Debug.Log($"[ECHO] Volume_RegionB configurado dentro de {RegionName} con el perfil " +
                      $"'{ProfilePath}'. Perfil: ColorAdjustments, Vignette y Bloom.", volumeObject);
        }

        private static void CheckMainCameraPostProcessing()
        {
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include);

            Camera mainCamera = null;
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i].CompareTag("MainCamera"))
                {
                    mainCamera = cameras[i];
                    break;
                }
            }

            if (mainCamera == null)
            {
                Debug.LogWarning("[ECHO] No se encontró Main Camera (tag MainCamera). " +
                                 "Comprueba manualmente Rendering > Post Processing.");
                return;
            }

            UniversalAdditionalCameraData cameraData =
                mainCamera.GetComponent<UniversalAdditionalCameraData>();

            if (cameraData == null)
            {
                Debug.LogWarning("[ECHO] Main Camera no tiene UniversalAdditionalCameraData. " +
                                 "El post-procesado de URP no se aplicará.", mainCamera);
                return;
            }

            if (!cameraData.renderPostProcessing)
            {
                Debug.LogWarning("[ECHO] Post Processing está desactivado en Main Camera. " +
                                 "Actívalo en Inspector > Rendering > Post Processing.", mainCamera);
            }
            else
            {
                Debug.Log("[ECHO] Main Camera tiene Post Processing activo.", mainCamera);
            }
        }

        private static VolumeProfile EnsureProfile()
        {
            if (!AssetDatabase.IsValidFolder(VfxFolder))
            {
                AssetDatabase.CreateFolder("Assets", "VFX");
            }

            if (!AssetDatabase.IsValidFolder(VfxFolder))
            {
                Debug.LogError($"[ECHO] No se pudo crear la carpeta '{VfxFolder}'.");
                return null;
            }

            Object existing = AssetDatabase.LoadMainAssetAtPath(ProfilePath);
            if (existing != null && !(existing is VolumeProfile))
            {
                Debug.LogError($"[ECHO] '{ProfilePath}' existe pero no es un VolumeProfile. " +
                               "No se sobrescribe.");
                return null;
            }

            VolumeProfile profile = existing as VolumeProfile;
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            return profile;
        }

        private static void ConfigureProfile(VolumeProfile profile)
        {
            Undo.RecordObject(profile, UndoName);

            ColorAdjustments colorAdjustments = GetOrAddComponent<ColorAdjustments>(profile);
            Vignette vignette = GetOrAddComponent<Vignette>(profile);
            Bloom bloom = GetOrAddComponent<Bloom>(profile);

            if (colorAdjustments == null || vignette == null || bloom == null)
            {
                Debug.LogError($"[ECHO] No se pudieron preparar todos los componentes de " +
                               $"'{ProfilePath}'.");
                return;
            }

            colorAdjustments.active = true;
            colorAdjustments.colorFilter.overrideState = true;
            colorAdjustments.colorFilter.value = FilterColor;
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = Saturation;
            colorAdjustments.postExposure.overrideState = true;
            colorAdjustments.postExposure.value = PostExposure;

            vignette.active = true;
            vignette.intensity.overrideState = true;
            vignette.intensity.value = VignetteIntensity;

            bloom.active = true;
            bloom.threshold.overrideState = true;
            bloom.threshold.value = BloomThreshold;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = BloomIntensity;

            EditorUtility.SetDirty(colorAdjustments);
            EditorUtility.SetDirty(vignette);
            EditorUtility.SetDirty(bloom);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);
        }

        private static T GetOrAddComponent<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet(out T existing))
            {
                return existing;
            }

            T component = profile.Add<T>(false);
            component.name = typeof(T).Name;
            component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(component, profile);
            EditorUtility.SetDirty(profile);
            return component;
        }

        private static GameObject EnsureVolumeObject(Transform region)
        {
            List<Transform> matches = FindSceneObjectsByName(VolumeName);
            Transform chosen = null;

            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].parent == region)
                {
                    chosen = matches[i];
                    break;
                }
            }

            if (chosen == null && matches.Count > 0)
            {
                chosen = matches[0];
            }

            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i] == chosen)
                {
                    continue;
                }

                Undo.DestroyObjectImmediate(matches[i].gameObject);
            }

            if (chosen == null)
            {
                GameObject created = new GameObject(VolumeName);
                Undo.RegisterCreatedObjectUndo(created, UndoName);
                created.transform.SetParent(region, false);
                chosen = created.transform;
            }
            else if (chosen.parent != region)
            {
                Undo.SetTransformParent(chosen, region, UndoName);
            }

            Undo.RecordObject(chosen, UndoName);
            chosen.localPosition = VolumeLocalPosition;
            chosen.localRotation = Quaternion.identity;
            chosen.localScale = Vector3.one;

            return chosen.gameObject;
        }

        private static BoxCollider EnsureBoxCollider(GameObject target)
        {
            BoxCollider box = target.GetComponent<BoxCollider>();
            if (box == null)
            {
                box = Undo.AddComponent<BoxCollider>(target);
            }

            return box;
        }

        private static Volume EnsureVolumeComponent(GameObject target)
        {
            Volume volume = target.GetComponent<Volume>();
            if (volume == null)
            {
                volume = Undo.AddComponent<Volume>(target);
            }

            return volume;
        }

        private static List<Transform> FindSceneObjectsByName(string objectName)
        {
            List<Transform> result = new List<Transform>();
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                CollectByName(root.transform, objectName, result);
            }

            return result;
        }

        private static void CollectByName(Transform parent, string objectName, List<Transform> result)
        {
            if (parent.name == objectName)
            {
                result.Add(parent);
            }

            foreach (Transform child in parent)
            {
                CollectByName(child, objectName, result);
            }
        }

        private static GameObject FindSceneObjectByName(string objectName)
        {
            List<Transform> matches = FindSceneObjectsByName(objectName);
            return matches.Count > 0 ? matches[0].gameObject : null;
        }
    }
}
