using System.Collections.Generic;
using Echo.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Echo.EditorTools
{
    public static class SetupCrystalGlow
    {
        private const string CrystalsBName = "Crystals_B";
        private const string CrystalsAName = "Crystals_A";
        private const string GlowBName = "CrystalGlow_B";
        private const string GlowAName = "CrystalGlow_A";

        [MenuItem("Tools/ECHO/Setup Crystal Glow", false, 22)]
        public static void Setup()
        {
            TimeOfDayController controller =
                Object.FindAnyObjectByType<TimeOfDayController>(FindObjectsInactive.Include);

            if (controller == null)
            {
                Debug.LogError("[ECHO] No se encontró TimeOfDayController en la escena activa. " +
                               "Abre Assets/Scenes/SantuarioPrincipal.unity y vuelve a ejecutar el menú.");
                return;
            }

            GameObject crystalsB = FindSceneObjectByName(CrystalsBName);
            GameObject crystalsA = FindSceneObjectByName(CrystalsAName);

            if (crystalsB == null)
            {
                Debug.LogError($"[ECHO] No se encontró '{CrystalsBName}' en la escena activa.");
            }

            if (crystalsA == null)
            {
                Debug.Log($"[ECHO] No existe '{CrystalsAName}'; se omite {GlowAName}.");
            }

            if (crystalsB == null && crystalsA == null)
            {
                return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Setup Crystal Glow");

            GameObject glowB = crystalsB != null ? EnsureGlow(GlowBName, crystalsB, controller) : null;
            GameObject glowA = crystalsA != null ? EnsureGlow(GlowAName, crystalsA, controller) : null;

            if (glowB == null && glowA == null)
            {
                Undo.CollapseUndoOperations(undoGroup);
                return;
            }

            GameObject selected = glowB != null ? glowB : glowA;
            EditorSceneManager.MarkSceneDirty(selected.scene);
            Undo.CollapseUndoOperations(undoGroup);

            Selection.activeGameObject = selected;
            EditorGUIUtility.PingObject(selected);

            string configured = glowB != null && glowA != null
                ? $"{GlowBName} y {GlowAName}"
                : glowB != null ? GlowBName : GlowAName;

            Debug.Log($"[ECHO] Crystal Glow configurado: {configured}. " +
                      "Referencias: TimeOfDayController y el padre de cristales correspondiente.", selected);
        }

        private static GameObject EnsureGlow(string glowName, GameObject crystals, TimeOfDayController controller)
        {
            Transform targetParent = crystals.transform.parent;
            List<Transform> matches = FindSceneObjectsByName(glowName);

            Transform glowTransform = null;
            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].parent == targetParent)
                {
                    glowTransform = matches[i];
                    break;
                }
            }

            if (glowTransform == null && matches.Count > 0)
            {
                glowTransform = matches[0];
            }

            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i] == glowTransform)
                {
                    continue;
                }

                Undo.DestroyObjectImmediate(matches[i].gameObject);
            }

            if (glowTransform == null)
            {
                GameObject created = new GameObject(glowName);
                Undo.RegisterCreatedObjectUndo(created, "Setup Crystal Glow");
                created.transform.SetParent(targetParent, false);
                glowTransform = created.transform;
            }
            else if (glowTransform.parent != targetParent)
            {
                Undo.SetTransformParent(glowTransform, targetParent, "Setup Crystal Glow");
            }

            glowTransform.localPosition = Vector3.zero;
            glowTransform.localRotation = Quaternion.identity;
            glowTransform.localScale = Vector3.one;

            CrystalNightGlow glow = glowTransform.GetComponent<CrystalNightGlow>();
            if (glow == null)
            {
                glow = Undo.AddComponent<CrystalNightGlow>(glowTransform.gameObject);
            }

            if (glow == null)
            {
                Debug.LogError($"[ECHO] No se pudo añadir CrystalNightGlow a '{glowName}'.");
                return null;
            }

            SerializedObject serialized = new SerializedObject(glow);
            SerializedProperty timeOfDayProperty = serialized.FindProperty("timeOfDay");
            SerializedProperty crystalsRootProperty = serialized.FindProperty("crystalsRoot");

            if (timeOfDayProperty == null || crystalsRootProperty == null)
            {
                Debug.LogError("[ECHO] CrystalNightGlow no tiene las propiedades serializadas " +
                               "'timeOfDay' y 'crystalsRoot'.");
                return null;
            }

            Undo.RecordObject(glow, "Setup Crystal Glow");
            timeOfDayProperty.objectReferenceValue = controller;
            crystalsRootProperty.objectReferenceValue = crystals.transform;
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(glow);

            return glowTransform.gameObject;
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
