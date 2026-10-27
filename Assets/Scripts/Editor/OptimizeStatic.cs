using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Echo.EditorTools
{
    public static class OptimizeStatic
    {
        private const string SceneName = "SantuarioPrincipal";
        private const string RegionAName = "RegionA_Amanecer";
        private const string RegionBName = "RegionB_Crepusculo";
        private const string UndoName = "Optimize Static";
        private const int RecommendedLightCount = 8;

        private readonly struct TargetContainer
        {
            public readonly string RegionName;
            public readonly string ContainerName;

            public TargetContainer(string regionName, string containerName)
            {
                RegionName = regionName;
                ContainerName = containerName;
            }
        }

        private sealed class ContainerReport
        {
            public readonly string Name;
            public int Marked;
            public int AlreadyStatic;
            public int SkippedLights;

            public ContainerReport(string name)
            {
                Name = name;
            }
        }

        private readonly struct LightStats
        {
            public readonly int Total;
            public readonly int Active;
            public readonly int StaticLights;

            public LightStats(int total, int active, int staticLights)
            {
                Total = total;
                Active = active;
                StaticLights = staticLights;
            }
        }

        private static readonly TargetContainer[] Targets =
        {
            new TargetContainer(RegionAName, "Decor_A"),
            new TargetContainer(RegionAName, "Path_A"),
            new TargetContainer(RegionAName, "Cliffs_A"),
            new TargetContainer(RegionBName, "Decor_B"),
            new TargetContainer(RegionBName, "Path_B"),
            new TargetContainer(RegionBName, "Cliffs_B"),
            new TargetContainer(RegionBName, "Stairs_B")
        };

        [MenuItem("Tools/ECHO/Optimize Static", false, 24)]
        public static void Optimize()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                Debug.LogError($"[ECHO] La escena activa es '{scene.name}'. " +
                               $"Abre '{SceneName}' y vuelve a ejecutar el menú.");
                return;
            }

            GameObject regionA = FindSceneObjectByName(RegionAName);
            GameObject regionB = FindSceneObjectByName(RegionBName);

            if (regionA == null || regionB == null)
            {
                if (regionA == null)
                {
                    Debug.LogError($"[ECHO] No se encontró '{RegionAName}' en la escena activa.");
                }

                if (regionB == null)
                {
                    Debug.LogError($"[ECHO] No se encontró '{RegionBName}' en la escena activa.");
                }

                return;
            }

            List<Transform> containers = new List<Transform>(Targets.Length);
            bool valid = true;

            for (int i = 0; i < Targets.Length; i++)
            {
                TargetContainer target = Targets[i];
                GameObject region = target.RegionName == RegionAName ? regionA : regionB;

                int count;
                Transform container = FindUniqueDescendant(region.transform, target.ContainerName, out count);

                if (count == 0)
                {
                    Debug.LogError($"[ECHO] No se encontró '{target.ContainerName}' dentro de " +
                                   $"'{target.RegionName}'. No se aplicaron cambios.");
                    valid = false;
                }
                else if (count > 1)
                {
                    Debug.LogError($"[ECHO] Hay {count} objetos llamados '{target.ContainerName}' " +
                                   $"dentro de '{target.RegionName}'. No se aplicaron cambios.");
                    valid = false;
                }
                else
                {
                    containers.Add(container);
                }
            }

            if (!valid)
            {
                return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UndoName);

            List<ContainerReport> reports = new List<ContainerReport>(containers.Count);
            int totalMarked = 0;
            int totalAlreadyStatic = 0;
            int totalSkippedLights = 0;

            for (int i = 0; i < containers.Count; i++)
            {
                ContainerReport report = new ContainerReport(containers[i].name);
                MarkContainer(containers[i], report);
                reports.Add(report);

                totalMarked += report.Marked;
                totalAlreadyStatic += report.AlreadyStatic;
                totalSkippedLights += report.SkippedLights;
            }

            Undo.CollapseUndoOperations(undoGroup);

            if (totalMarked > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            LogReport(
                regionA,
                regionB,
                reports,
                totalMarked,
                totalAlreadyStatic,
                totalSkippedLights);
        }

        private static void MarkContainer(Transform root, ContainerReport report)
        {
            Transform[] objects = root.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < objects.Length; i++)
            {
                GameObject target = objects[i].gameObject;

                if (target.GetComponent<Light>() != null)
                {
                    report.SkippedLights++;
                    continue;
                }

                StaticEditorFlags current = GameObjectUtility.GetStaticEditorFlags(target);
                if (current == StaticEditorFlags.BatchingStatic)
                {
                    report.AlreadyStatic++;
                    continue;
                }

                Undo.RecordObject(target, UndoName);
                GameObjectUtility.SetStaticEditorFlags(target, StaticEditorFlags.BatchingStatic);

                if (PrefabUtility.IsPartOfPrefabInstance(target))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                }

                EditorUtility.SetDirty(target);
                report.Marked++;
            }
        }

        private static void LogReport(
            GameObject regionA,
            GameObject regionB,
            List<ContainerReport> reports,
            int totalMarked,
            int totalAlreadyStatic,
            int totalSkippedLights)
        {
            LightStats statsA = CountLights(regionA);
            LightStats statsB = CountLights(regionB);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("[ECHO] Optimize Static completado. Solo se aplicó Batching Static.");

            for (int i = 0; i < reports.Count; i++)
            {
                ContainerReport report = reports[i];
                builder.AppendLine(
                    $"  {report.Name}: {report.Marked} marcados, " +
                    $"{report.AlreadyStatic} ya correctos, " +
                    $"{report.SkippedLights} luces omitidas.");
            }

            builder.AppendLine(
                $"Total: {totalMarked} marcados, {totalAlreadyStatic} ya correctos, " +
                $"{totalSkippedLights} luces omitidas.");

            if (totalMarked == 0)
            {
                builder.AppendLine("No había cambios pendientes.");
            }

            builder.AppendLine("Luces por región (total / activas / con Batching Static):");
            builder.AppendLine($"  {regionA.name}: {statsA.Total} / {statsA.Active} / {statsA.StaticLights}");
            builder.AppendLine($"  {regionB.name}: {statsB.Total} / {statsB.Active} / {statsB.StaticLights}");

            Debug.Log(builder.ToString(), regionA);

            LogLightWarnings(regionA, statsA);
            LogLightWarnings(regionB, statsB);
        }

        private static LightStats CountLights(GameObject region)
        {
            Light[] lights = region.GetComponentsInChildren<Light>(true);
            int active = 0;
            int staticLights = 0;

            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];

                if (light.enabled && light.gameObject.activeInHierarchy)
                {
                    active++;
                }

                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(light.gameObject);
                if ((flags & StaticEditorFlags.BatchingStatic) != 0)
                {
                    staticLights++;
                }
            }

            return new LightStats(lights.Length, active, staticLights);
        }

        private static void LogLightWarnings(GameObject region, LightStats stats)
        {
            if (stats.Total > RecommendedLightCount)
            {
                Debug.LogWarning(
                    $"[ECHO] {region.name} tiene {stats.Total} luces en total. " +
                    $"Revisa que no haya más de {RecommendedLightCount} cerca del jugador. " +
                    "No se ha modificado ninguna luz.",
                    region);
            }

            if (stats.StaticLights > 0)
            {
                Debug.LogWarning(
                    $"[ECHO] {region.name} tiene {stats.StaticLights} luces con Batching Static. " +
                    "Este script no las ha modificado.",
                    region);
            }
        }

        private static Transform FindUniqueDescendant(Transform parent, string objectName, out int count)
        {
            Transform result = null;
            count = 0;

            Transform[] objects = parent.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i].name != objectName)
                {
                    continue;
                }

                count++;
                if (result == null)
                {
                    result = objects[i];
                }
            }

            return result;
        }

        private static GameObject FindSceneObjectByName(string objectName)
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Transform match = FindInChildren(root.transform, objectName);
                if (match != null)
                {
                    return match.gameObject;
                }
            }

            return null;
        }

        private static Transform FindInChildren(Transform parent, string objectName)
        {
            if (parent.name == objectName)
            {
                return parent;
            }

            foreach (Transform child in parent)
            {
                Transform match = FindInChildren(child, objectName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }
    }
}
