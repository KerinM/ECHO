using System.Collections.Generic;
using Echo.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Echo.EditorTools
{
    public static class BuildRegionA
    {
        private const string RegionName = "RegionA_Amanecer";
        private const string PrefabFolder = "Assets/Prefabs";
        private const string NatureModels = "Assets/Models/Kenney_NatureKit/FBX format";
        private const string CastleModels = "Assets/Models/Kenney_CastleKit/FBX format";
        private const float FreeRadius = 3f;
        private const float PathClearance = 2.5f;
        private const uint ForestSeed = 0x5EEDA001u;
        private const int ExtraTreeCount = 36;
        private const int ExtraBushCount = 60;
        private const int ExtraRockCount = 32;

        private static readonly Vector2 TreeScaleRange = new Vector2(2.5f, 3.5f);
        private static readonly Vector2 BushScaleRange = new Vector2(1.8f, 2.2f);
        private static readonly Vector2 RockScaleRange = new Vector2(2f, 3f);
        private static readonly Vector2 StoneScaleRange = new Vector2(1.7f, 1.9f);

        private enum ModelType
        {
            Tree,
            Leafy,
            Stone
        }

        private enum ColliderKind
        {
            None,
            TreeCapsule,
            Box,
            MeshAll
        }

        private readonly struct Placement
        {
            public readonly Vector3 Position;
            public readonly float Yaw;
            public readonly float Scale;

            public Placement(float x, float y, float z, float yaw, float scale)
            {
                Position = new Vector3(x, y, z);
                Yaw = yaw;
                Scale = scale;
            }
        }

        private static readonly Vector2[] PathPositions =
        {
            new Vector2(0f, -18f),
            new Vector2(0.5f, -12f),
            new Vector2(-0.5f, -6f),
            new Vector2(0f, 0f),
            new Vector2(0.5f, 6f),
            new Vector2(0f, 12f),
            new Vector2(-0.5f, 18f),
            new Vector2(0f, 24f)
        };

        private static readonly Placement[] ColumnPlacements =
        {
            new Placement(-6f, 0f, 26f, 0f, 1f),
            new Placement(6f, 0f, 26f, 15f, 1f),
            new Placement(-6f, 0f, 34f, -8f, 0.95f),
            new Placement(6f, 0f, 34f, 22f, 0.95f)
        };

        private static readonly Placement[] WallPlacements =
        {
            new Placement(-5f, 0f, 36f, 18f, 1f),
            new Placement(4f, 0f, 36f, -12f, 1f),
            new Placement(0f, -0.15f, 37.5f, 8f, 0.9f)
        };

        private static readonly Placement[] TreePlacements =
        {
            new Placement(30.3f, 0f, 17.5f, 0f, 1f),
            new Placement(25.5f, 0f, 25.5f, 31f, 1.15f),
            new Placement(17.5f, 0f, 30.3f, 67f, 0.9f),
            new Placement(9.5f, 0f, 31f, 102f, 1.25f),
            new Placement(-9.3f, 0f, 34.8f, 138f, 0.95f),
            new Placement(-17.5f, 0f, 30.3f, 173f, 1.1f),
            new Placement(-32.6f, 0f, 15.2f, 208f, 0.85f),
            new Placement(-33.9f, 0f, 3f, 243f, 1.2f),
            new Placement(-32.9f, 0f, -12f, 278f, 1f),
            new Placement(-23.1f, 0f, -27.6f, 313f, 1.3f),
            new Placement(-6.1f, 0f, -34.5f, 348f, 0.9f),
            new Placement(18f, 0f, -31.2f, 23f, 1.05f)
        };

        private static readonly Placement[] BushPlacements =
        {
            new Placement(-10f, 0f, 24f, 0f, 1f),
            new Placement(10f, 0f, 24f, 42f, 1.1f),
            new Placement(-13f, 0f, 31f, 84f, 0.9f),
            new Placement(13f, 0f, 31f, 126f, 1.2f),
            new Placement(-20f, 0f, 36f, 168f, 0.95f),
            new Placement(8f, 0f, 37f, 210f, 1.05f),
            new Placement(22f, 0f, 25f, 252f, 0.9f),
            new Placement(-22f, 0f, 25f, 294f, 1.15f)
        };

        private static readonly Placement[] RockPlacements =
        {
            new Placement(-24f, 0f, 29f, 0f, 1.2f),
            new Placement(21f, 0f, 29f, 37f, 1f),
            new Placement(-28f, 0f, 8f, 74f, 1.35f),
            new Placement(28f, 0f, -4f, 111f, 0.9f),
            new Placement(-21f, 0f, -21f, 148f, 1.5f),
            new Placement(19f, 0f, -24f, 185f, 1.1f),
            new Placement(-10f, 0f, -8f, 222f, 0.85f),
            new Placement(11f, 0f, 3f, 259f, 1.25f)
        };

        private static readonly Placement[] CliffRockPlacements =
        {
            new Placement(-25f, 0f, 37f, 0f, 3.8f),
            new Placement(-13f, 0f, 38f, 23f, 4.6f),
            new Placement(12f, 0f, 37f, 46f, 3.2f),
            new Placement(27f, 0f, 38f, 69f, 5f),
            new Placement(37f, 0f, -10f, 92f, 4f),
            new Placement(38f, 0f, 15f, 115f, 3.5f),
            new Placement(37f, 0f, 30f, 138f, 4.8f),
            new Placement(-37f, 0f, -5f, 161f, 3f),
            new Placement(-38f, 0f, 22f, 184f, 4.4f)
        };

        private static readonly Placement[] LanternPlacements =
        {
            new Placement(2f, 0f, -14f, 0f, 1f),
            new Placement(-2f, 0f, -3f, 37f, 1f),
            new Placement(2f, 0f, 9f, 74f, 1f),
            new Placement(-2f, 0f, 21f, 111f, 1f),
            new Placement(26f, 0f, 7f, 148f, 1f)
        };

        private static readonly List<Vector3> ProtectedPoints = new List<Vector3>();
        private static readonly List<ForestScatter.Segment> PathSegments = new List<ForestScatter.Segment>();
        private static readonly List<ForestScatter.Footprint> Footprints = new List<ForestScatter.Footprint>();

        private static Material stone;
        private static Material stoneDark;
        private static Material groundA;

        [MenuItem("Tools/ECHO/Build Region A", false, 20)]
        public static void Build()
        {
            GameObject region = FindSceneObjectByName(RegionName);
            if (region == null)
            {
                Debug.LogError($"[ECHO] No se encontró '{RegionName}' en la escena activa. " +
                               "Abre Assets/Scenes/SantuarioPrincipal.unity y vuelve a ejecutar el menú.");
                return;
            }

            if (!LoadMaterials())
            {
                return;
            }

            EnsurePrefabFolder();

            GameObject treeDefault = EnsureModelPrefab(NatureModels + "/tree_default.fbx", "ENV_Tree_Default", ModelType.Tree, ColliderKind.TreeCapsule);
            GameObject treeOak = EnsureModelPrefab(NatureModels + "/tree_oak.fbx", "ENV_Tree_Oak", ModelType.Tree, ColliderKind.TreeCapsule);
            GameObject treePine = EnsureModelPrefab(NatureModels + "/tree_pineTallA.fbx", "ENV_Tree_PineTall", ModelType.Tree, ColliderKind.TreeCapsule);
            GameObject bush = EnsureModelPrefab(NatureModels + "/plant_bush.fbx", "ENV_Bush", ModelType.Leafy, ColliderKind.None);
            GameObject bushDetailed = EnsureModelPrefab(NatureModels + "/plant_bushDetailed.fbx", "ENV_BushDetailed", ModelType.Leafy, ColliderKind.None);
            GameObject rockLarge = EnsureModelPrefab(NatureModels + "/rock_largeA.fbx", "ENV_Rock_Large", ModelType.Stone, ColliderKind.Box);
            GameObject rockSmall = EnsureModelPrefab(NatureModels + "/rock_smallA.fbx", "ENV_Rock_Small", ModelType.Stone, ColliderKind.Box);
            GameObject column = EnsureModelPrefab(NatureModels + "/statue_column.fbx", "ENV_Column", ModelType.Stone, ColliderKind.Box);
            GameObject columnDamaged = EnsureModelPrefab(NatureModels + "/statue_columnDamaged.fbx", "ENV_Column_Damaged", ModelType.Stone, ColliderKind.Box);
            GameObject wallHalf = EnsureModelPrefab(CastleModels + "/wall-half.fbx", "ENV_Wall_Half", ModelType.Stone, ColliderKind.MeshAll);
            GameObject wallCornerHalf = EnsureModelPrefab(CastleModels + "/wall-corner-half.fbx", "ENV_Wall_CornerHalf", ModelType.Stone, ColliderKind.MeshAll);
            GameObject lantern = EnsureLanternPrefab();

            if (treeDefault == null || treeOak == null || treePine == null || bush == null || bushDetailed == null ||
                rockLarge == null || rockSmall == null || column == null || columnDamaged == null ||
                wallHalf == null || wallCornerHalf == null || lantern == null)
            {
                Debug.LogError("[ECHO] No se pudieron crear todos los prefabs de la Región A. Revisa las rutas de los modelos.");
                return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Build Region A");

            DestroyIfExists("TestNightLight_1");
            DestroyIfExists("TestNightLight_2");

            ProtectedPoints.Clear();
            ProtectedPoints.Add(ResolvePoint("PlayerSpawn", new Vector3(0f, 0.1f, -25f)));
            ProtectedPoints.Add(ResolvePoint("Marker_Fragment1", new Vector3(-25f, 1f, 20f)));
            ProtectedPoints.Add(ResolvePoint("Marker_PortalA", new Vector3(30f, 0f, 10f)));
            ProtectedPoints.Add(ResolvePoint("Marker_PortalA_Arrival", new Vector3(30f, 0.1f, 4f)));
            ProtectedPoints.Add(ResolvePoint("AncestralCore", new Vector3(0f, 0f, 30f)));

            Transform decor = GetOrCreateContainer(region.transform, "Decor_A");
            Transform path = GetOrCreateContainer(region.transform, "Path_A");
            Transform lanterns = GetOrCreateContainer(region.transform, "Lanterns_A");

            PathSegments.Clear();
            ForestScatter.AddPolylineSegments(PathSegments, PathPositions, region.transform);
            Footprints.Clear();

            ForestScatter.Rng rng = new ForestScatter.Rng(ForestSeed);
            Placement[] scaledColumnPlacements = ScalePlacements(ColumnPlacements, StoneScaleRange, ref rng);
            Placement[] scaledWallPlacements = ScalePlacements(WallPlacements, StoneScaleRange, ref rng);
            Placement[] scaledTreePlacements = ScalePlacements(TreePlacements, TreeScaleRange, ref rng);
            Placement[] scaledBushPlacements = ScalePlacements(BushPlacements, BushScaleRange, ref rng);
            Placement[] scaledRockPlacements = ScalePlacements(RockPlacements, RockScaleRange, ref rng);

            int slabCount = BuildPath(path);

            int columnCount = PlaceSeries(
                new[] { column, column, columnDamaged, columnDamaged },
                scaledColumnPlacements, decor, "Column", "columna", true, false, 0.7f);

            int wallCount = PlaceSeries(
                new[] { wallHalf, wallHalf, wallCornerHalf },
                scaledWallPlacements, decor, "Wall", "muro", true, false, 1f);

            int treeCount = PlaceSeries(
                new[] { treeDefault, treeOak, treePine },
                scaledTreePlacements, decor, "Tree", "árbol", true, false, 0.5f);

            int bushCount = PlaceSeries(
                new[] { bush, bushDetailed },
                scaledBushPlacements, decor, "Bush", "arbusto", true, false, 0.4f);

            int rockCount = PlaceSeries(
                new[] { rockLarge, rockSmall },
                scaledRockPlacements, decor, "Rock", "roca", true, false, 0.55f);

            int cliffRockCount = PlaceSeries(
                new[] { rockLarge },
                CliffRockPlacements, decor, "Cliff_Rock", "roca de acantilado", true, false, 0.7f);

            int lanternCount = PlaceSeries(
                new[] { lantern },
                LanternPlacements, lanterns, "Lantern", "farol", false, false, 0.3f);

            int denseTreeCount = PlaceProceduralSeries(
                new[] { treeDefault, treeOak, treePine },
                ExtraTreeCount, decor, "Tree_Dense", "árbol denso", TreeScaleRange, 0.5f, ref rng);

            int denseBushCount = PlaceProceduralSeries(
                new[] { bush, bushDetailed },
                ExtraBushCount, decor, "Bush_Dense", "arbusto denso", BushScaleRange, 0.4f, ref rng);

            int denseRockCount = PlaceProceduralSeries(
                new[] { rockLarge, rockSmall },
                ExtraRockCount, decor, "Rock_Dense", "roca decorativa", RockScaleRange, 0.55f, ref rng);

            int lampCount = WireNightLights(lanterns);

            EditorSceneManager.MarkSceneDirty(region.scene);
            Undo.CollapseUndoOperations(undoGroup);

            Selection.activeGameObject = region;
            EditorGUIUtility.PingObject(region);

            Debug.Log($"[ECHO] Región A construida: {slabCount} losas, {columnCount} columnas, {wallCount} muros, " +
                      $"{treeCount} árboles base + {denseTreeCount} nuevos, " +
                      $"{bushCount} arbustos base + {denseBushCount} nuevos, " +
                      $"{rockCount} rocas base + {denseRockCount} nuevas, " +
                      $"{cliffRockCount} rocas de acantilado, {lanternCount} faroles y {lampCount} luces enlazadas. " +
                      $"Semilla {ForestSeed:X8}.", region);
        }

        private static bool LoadMaterials()
        {
            stone = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Stone.mat");
            stoneDark = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_StoneDark.mat");
            groundA = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Ground_A.mat");

            if (stone == null || stoneDark == null || groundA == null)
            {
                Debug.LogError("[ECHO] Faltan M_Stone, M_StoneDark o M_Ground_A en Assets/Materials.");
                return false;
            }

            return true;
        }

        private static void EnsurePrefabFolder()
        {
            if (!AssetDatabase.IsValidFolder(PrefabFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
        }

        private static GameObject EnsureModelPrefab(string modelPath, string prefabName, ModelType modelType, ColliderKind colliderKind)
        {
            string prefabPath = PrefabFolder + "/" + prefabName + ".prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                string reason;
                if (PrefabHasCollider(existing, colliderKind, out reason))
                {
                    return existing;
                }

                Debug.LogWarning($"[ECHO] El prefab '{prefabName}' no tiene el collider esperado ({reason}); se reconstruye.");
                if (!AssetDatabase.DeleteAsset(prefabPath))
                {
                    Debug.LogError($"[ECHO] No se pudo reemplazar el prefab '{prefabPath}'.");
                    return null;
                }
            }

            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (modelAsset == null)
            {
                Debug.LogError($"[ECHO] No se encontró el modelo '{modelPath}'.");
                return null;
            }

            GameObject wrapper = new GameObject(prefabName);
            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            model.transform.SetParent(wrapper.transform, false);
            PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            foreach (Collider collider in wrapper.GetComponentsInChildren<Collider>(true))
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            AssignMaterials(wrapper, modelType);
            AddCollider(wrapper, colliderKind);

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(wrapper, prefabPath);
            UnityEngine.Object.DestroyImmediate(wrapper);
            AssetDatabase.SaveAssets();

            if (saved == null)
            {
                Debug.LogError($"[ECHO] No se pudo guardar el prefab '{prefabPath}'.");
            }

            return saved;
        }

        private static GameObject EnsureLanternPrefab()
        {
            string prefabPath = PrefabFolder + "/ENV_Lantern.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null && existing.GetComponentsInChildren<Light>(true).Length > 0)
            {
                return existing;
            }

            if (existing != null)
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            GameObject root = new GameObject("ENV_Lantern");

            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "Post";
            post.transform.SetParent(root.transform, false);
            post.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            post.transform.localRotation = Quaternion.identity;
            post.transform.localScale = new Vector3(0.15f, 0.8f, 0.15f);
            Renderer postRenderer = post.GetComponent<Renderer>();
            if (postRenderer != null)
            {
                postRenderer.sharedMaterial = stone;
            }

            GameObject lamp = new GameObject("Lamp");
            lamp.transform.SetParent(root.transform, false);
            lamp.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            lamp.transform.localRotation = Quaternion.identity;
            lamp.transform.localScale = Vector3.one;
            Light light = lamp.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.64705884f, 0.3137255f);
            light.intensity = 2f;
            light.range = 7f;
            light.shadows = LightShadows.None;

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();

            if (saved == null)
            {
                Debug.LogError($"[ECHO] No se pudo guardar el prefab '{prefabPath}'.");
            }

            return saved;
        }

        private static bool PrefabHasCollider(GameObject prefab, ColliderKind kind, out string reason)
        {
            reason = string.Empty;

            switch (kind)
            {
                case ColliderKind.None:
                    return true;
                case ColliderKind.TreeCapsule:
                    if (prefab.GetComponent<CapsuleCollider>() == null)
                    {
                        reason = "CapsuleCollider en el root";
                        return false;
                    }
                    return true;
                case ColliderKind.Box:
                    if (prefab.GetComponent<BoxCollider>() == null)
                    {
                        reason = "BoxCollider en el root";
                        return false;
                    }
                    return true;
                case ColliderKind.MeshAll:
                    if (prefab.GetComponentsInChildren<MeshCollider>(true).Length == 0)
                    {
                        reason = "MeshCollider en las mallas";
                        return false;
                    }
                    return true;
                default:
                    return true;
            }
        }

        private static void AssignMaterials(GameObject root, ModelType modelType)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0)
                {
                    continue;
                }

                for (int i = 0; i < materials.Length; i++)
                {
                    switch (modelType)
                    {
                        case ModelType.Tree:
                            string materialName = materials[i] != null ? materials[i].name.ToLowerInvariant() : string.Empty;
                            if (materialName.Contains("bark") || materialName.Contains("wood"))
                            {
                                materials[i] = stoneDark;
                            }
                            else if (materialName.Contains("leaf") || materialName.Contains("green"))
                            {
                                materials[i] = groundA;
                            }
                            else
                            {
                                materials[i] = materials.Length > 1 && i == 0 ? stoneDark : groundA;
                            }
                            break;
                        case ModelType.Leafy:
                            materials[i] = groundA;
                            break;
                        case ModelType.Stone:
                            materials[i] = stone;
                            break;
                    }
                }

                renderer.sharedMaterials = materials;
            }
        }

        private static void AddCollider(GameObject root, ColliderKind kind)
        {
            Bounds bounds = GetRendererBounds(root);

            switch (kind)
            {
                case ColliderKind.TreeCapsule:
                    float height = Mathf.Max(0.6f, bounds.size.y * 0.55f);
                    float radius = Mathf.Clamp(Mathf.Min(bounds.size.x, bounds.size.z) * 0.12f, 0.12f, 0.35f);
                    if (height < radius * 2f)
                    {
                        height = radius * 2f;
                    }

                    CapsuleCollider capsule = root.AddComponent<CapsuleCollider>();
                    capsule.direction = 1;
                    capsule.radius = radius;
                    capsule.height = height;
                    capsule.center = new Vector3(bounds.center.x, bounds.min.y + height * 0.5f, bounds.center.z);
                    break;
                case ColliderKind.Box:
                    BoxCollider box = root.AddComponent<BoxCollider>();
                    box.center = bounds.center;
                    box.size = bounds.size;
                    break;
                case ColliderKind.MeshAll:
                    foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
                    {
                        if (filter.sharedMesh == null || filter.GetComponent<Collider>() != null)
                        {
                            continue;
                        }

                        MeshCollider meshCollider = filter.gameObject.AddComponent<MeshCollider>();
                        meshCollider.sharedMesh = filter.sharedMesh;
                    }
                    break;
            }
        }

        private static Bounds GetRendererBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(Vector3.zero, Vector3.one);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static Transform GetOrCreateContainer(Transform region, string containerName)
        {
            List<Transform> matches = new List<Transform>();
            foreach (Transform candidate in region.GetComponentsInChildren<Transform>(true))
            {
                if (candidate != region && candidate.name == containerName)
                {
                    matches.Add(candidate);
                }
            }

            Transform container;
            if (matches.Count == 0)
            {
                GameObject created = new GameObject(containerName);
                Undo.RegisterCreatedObjectUndo(created, "Build Region A");
                created.transform.SetParent(region, false);
                container = created.transform;
            }
            else
            {
                container = matches[0];
                for (int i = 1; i < matches.Count; i++)
                {
                    Undo.DestroyObjectImmediate(matches[i].gameObject);
                }

                if (container.parent != region)
                {
                    Undo.SetTransformParent(container, region, "Build Region A");
                }
            }

            container.localPosition = Vector3.zero;
            container.localRotation = Quaternion.identity;
            container.localScale = Vector3.one;

            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(container.GetChild(i).gameObject);
            }

            return container;
        }

        private static int BuildPath(Transform container)
        {
            for (int i = 0; i < PathPositions.Length; i++)
            {
                Vector2 point = PathPositions[i];
                GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slab.name = $"Path_Slab_{i + 1:00}";
                Undo.RegisterCreatedObjectUndo(slab, "Build Region A");
                slab.transform.SetParent(container, false);
                slab.transform.localPosition = new Vector3(point.x, 0.05f, point.y);
                slab.transform.localRotation = Quaternion.identity;
                slab.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);

                Renderer renderer = slab.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = stone;
                }
            }

            return PathPositions.Length;
        }

        private static int PlaceSeries(
            GameObject[] prefabs,
            Placement[] placements,
            Transform parent,
            string prefix,
            string category,
            bool checkPath,
            bool checkFootprints,
            float footprintBaseRadius)
        {
            int placed = 0;

            for (int i = 0; i < placements.Length; i++)
            {
                GameObject prefab = prefabs[i % prefabs.Length];
                if (prefab == null)
                {
                    continue;
                }

                Placement placement = placements[i];
                Vector3 worldPosition = parent.TransformPoint(placement.Position);
                float footprintRadius = footprintBaseRadius * placement.Scale;
                if (!IsFree(worldPosition, checkPath, checkFootprints, footprintRadius))
                {
                    Debug.LogWarning($"[ECHO] {category} {i + 1:00} omitido: invade una zona protegida, el camino u otra pieza.");
                    continue;
                }

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                Undo.RegisterCreatedObjectUndo(instance, "Build Region A");
                instance.name = $"{prefix}_{i + 1:00}";
                instance.transform.localPosition = placement.Position;
                instance.transform.localRotation = Quaternion.Euler(0f, placement.Yaw, 0f);
                instance.transform.localScale = Vector3.one * placement.Scale;

                Footprints.Add(new ForestScatter.Footprint(
                    new Vector2(worldPosition.x, worldPosition.z),
                    footprintRadius));
                placed++;
            }

            return placed;
        }

        private static int PlaceProceduralSeries(
            GameObject[] prefabs,
            int targetCount,
            Transform parent,
            string prefix,
            string category,
            Vector2 scaleRange,
            float footprintBaseRadius,
            ref ForestScatter.Rng rng)
        {
            int placed = 0;
            int attempts = 0;
            int maxAttempts = Mathf.Max(targetCount * 80, 800);

            while (placed < targetCount && attempts < maxAttempts)
            {
                attempts++;
                Vector2 localPosition = ForestScatter.SampleAnnulus(
                    ref rng,
                    ForestScatter.InnerRadius,
                    ForestScatter.OuterRadius);
                Vector3 local = new Vector3(localPosition.x, 0f, localPosition.y);
                Vector3 worldPosition = parent.TransformPoint(local);
                float scale = rng.Range(scaleRange.x, scaleRange.y);
                float footprintRadius = footprintBaseRadius * scale;

                if (!IsFree(worldPosition, true, true, footprintRadius))
                {
                    continue;
                }

                GameObject prefab = prefabs[placed % prefabs.Length];
                if (prefab == null)
                {
                    continue;
                }

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                Undo.RegisterCreatedObjectUndo(instance, "Build Region A");
                instance.name = $"{prefix}_{placed + 1:00}";
                instance.transform.localPosition = local;
                instance.transform.localRotation = Quaternion.Euler(0f, rng.Range(0f, 360f), 0f);
                instance.transform.localScale = Vector3.one * scale;

                Footprints.Add(new ForestScatter.Footprint(
                    new Vector2(worldPosition.x, worldPosition.z),
                    footprintRadius));
                placed++;
            }

            if (placed < targetCount)
            {
                Debug.LogWarning(
                    $"[ECHO] {category}: solo se colocaron {placed}/{targetCount} piezas tras {attempts} intentos. " +
                    "Revisa el radio libre, los caminos y la separación entre piezas.",
                    parent);
            }

            return placed;
        }

        private static Placement[] ScalePlacements(Placement[] source, Vector2 scaleRange, ref ForestScatter.Rng rng)
        {
            Placement[] result = new Placement[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                Placement placement = source[i];
                result[i] = new Placement(
                    placement.Position.x,
                    placement.Position.y,
                    placement.Position.z,
                    placement.Yaw,
                    rng.Range(scaleRange.x, scaleRange.y));
            }

            return result;
        }

        private static bool IsFree(Vector3 worldPosition, bool checkPath, bool checkFootprints, float footprintRadius)
        {
            Vector2 point = new Vector2(worldPosition.x, worldPosition.z);

            foreach (Vector3 protectedPoint in ProtectedPoints)
            {
                float dx = worldPosition.x - protectedPoint.x;
                float dz = worldPosition.z - protectedPoint.z;
                if (dx * dx + dz * dz < FreeRadius * FreeRadius)
                {
                    return false;
                }
            }

            if (checkPath && ForestScatter.IsNearPath(point, PathSegments, PathClearance))
            {
                return false;
            }

            if (checkFootprints && ForestScatter.OverlapsAny(point, footprintRadius, Footprints))
            {
                return false;
            }

            return true;
        }

        private static int WireNightLights(Transform lanternsRoot)
        {
            TimeOfDayController controller = UnityEngine.Object.FindAnyObjectByType<TimeOfDayController>(FindObjectsInactive.Include);
            if (controller == null)
            {
                Debug.LogError("[ECHO] No se encontró TimeOfDayController para enlazar los faroles.");
                return 0;
            }

            List<Light> lamps = new List<Light>();
            foreach (Transform child in lanternsRoot)
            {
                Light[] lights = child.GetComponentsInChildren<Light>(true);
                for (int i = 0; i < lights.Length; i++)
                {
                    lamps.Add(lights[i]);
                }
            }

            if (lamps.Count == 0)
            {
                Debug.LogError("[ECHO] Lanterns_A no contiene luces para enlazar.");
                return 0;
            }

            SerializedObject serialized = new SerializedObject(controller);
            SerializedProperty nightLights = serialized.FindProperty("nightLights");
            if (nightLights == null)
            {
                Debug.LogError("[ECHO] TimeOfDayController no tiene la propiedad serializada 'nightLights'.");
                return 0;
            }

            List<Light> finalLights = new List<Light>();
            for (int i = 0; i < nightLights.arraySize; i++)
            {
                Light light = nightLights.GetArrayElementAtIndex(i).objectReferenceValue as Light;
                if (light == null || light.transform.IsChildOf(lanternsRoot))
                {
                    continue;
                }

                finalLights.Add(light);
            }

            finalLights.AddRange(lamps);

            Undo.RecordObject(controller, "Build Region A");
            nightLights.arraySize = finalLights.Count;
            for (int i = 0; i < finalLights.Count; i++)
            {
                nightLights.GetArrayElementAtIndex(i).objectReferenceValue = finalLights[i];
            }

            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);

            return lamps.Count;
        }

        private static void DestroyIfExists(string objectName)
        {
            GameObject target = FindSceneObjectByName(objectName);
            if (target != null)
            {
                Undo.DestroyObjectImmediate(target);
            }
        }

        private static Vector3 ResolvePoint(string objectName, Vector3 fallback)
        {
            GameObject target = FindSceneObjectByName(objectName);
            if (target != null)
            {
                return target.transform.position;
            }

            Debug.LogWarning($"[ECHO] No se encontró '{objectName}'; se usa la posición por defecto {fallback}.");
            return fallback;
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
