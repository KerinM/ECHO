using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Echo.EditorTools
{
    public static class BuildRegionB
    {
        private const string RegionName = "RegionB_Crepusculo";
        private const string MaterialFolder = "Assets/Materials";
        private const string PrefabFolder = "Assets/Prefabs";
        private const string LitShaderName = "Universal Render Pipeline/Lit";
        private const string StoneDarkPath = MaterialFolder + "/M_StoneDark.mat";
        private const string CrystalVioletPath = MaterialFolder + "/M_Crystal_Violet.mat";
        private const string CrystalCyanPath = MaterialFolder + "/M_Crystal_Emissive.mat";
        private const string CrystalPrefabName = "Crystal_Cluster";
        private const float FreeRadius = 3f;
        private const float DecorPathClearance = 2.5f;
        private const float CrystalPathClearance = 3f;
        private const float StairsMinX = -16f;
        private const float StairsMaxX = -4f;
        private const float StairsMinZ = 18.8f;
        private const float StairsMaxZ = 38f;
        private const uint ForestSeed = 0x5EEDB002u;
        private const int ExtraTreeCount = 36;
        private const int ExtraBushCount = 60;
        private const int ExtraRockCount = 32;

        private static readonly Vector2 TreeScaleRange = new Vector2(2.5f, 3.5f);
        private static readonly Vector2 BushScaleRange = new Vector2(1.8f, 2.2f);
        private static readonly Vector2 RockScaleRange = new Vector2(2f, 3f);
        private static readonly Vector2 RuinScaleRange = new Vector2(1.7f, 1.9f);

        private static readonly string[] ContainerNames =
        {
            "Cliffs_B",
            "Decor_B",
            "Path_B",
            "Crystals_B",
            "Stairs_B"
        };

        private static readonly Vector2[] MainPathPositions =
        {
            new Vector2(-23f, -6f),
            new Vector2(-16f, -2.5f),
            new Vector2(-9.5f, 1f),
            new Vector2(-2.5f, 5f),
            new Vector2(4.5f, 9f),
            new Vector2(11f, 12.5f),
            new Vector2(18f, 16f),
            new Vector2(25f, 20f)
        };

        private static readonly Vector2[] BranchPathPositions =
        {
            new Vector2(-6f, 10f),
            new Vector2(-8f, 15f),
            new Vector2(-10f, 19f)
        };

        private static readonly Placement[] RuinPlacements =
        {
            new Placement(-22f, 0f, 14f, 0f, 20f, 0f, 1f),
            new Placement(-21f, -0.15f, 24f, 0f, 70f, 8f, 0.95f),
            new Placement(6f, 0f, 26f, 0f, 65f, 0f, 0.9f),
            new Placement(14f, 0f, 25f, 0f, -30f, 0f, 0.85f),
            new Placement(-18f, 0f, -18f, 0f, 45f, 0f, 1f),
            new Placement(10f, -0.2f, -8f, 12f, 10f, -8f, 1f),
            new Placement(22f, 0f, -2f, 0f, 100f, -6f, 0.9f),
            new Placement(-8f, -0.1f, -22f, 0f, 30f, 10f, 0.95f),
            new Placement(30f, 0f, 30f, 0f, -60f, 0f, 0.9f),
            new Placement(30f, 0f, 8f, 0f, 50f, 0f, 1f)
        };

        private static readonly int[] RuinPrefabIndices = { 2, 3, 0, 0, 1, 3, 0, 3, 1, 2 };

        private static readonly Placement[] CliffRockPlacements =
        {
            new Placement(-28f, 0f, 37f, 0f, 10f, 0f, 3.8f),
            new Placement(-20f, 0f, 38.5f, 0f, 33f, 0f, 4.6f),
            new Placement(5f, 0f, 37f, 0f, 57f, 0f, 3.2f),
            new Placement(22f, 0f, 38f, 0f, 81f, 0f, 5f),
            new Placement(34f, 0f, 36f, 0f, 105f, 0f, 4f),
            new Placement(37.5f, 0f, -5f, 0f, 129f, 0f, 4.2f),
            new Placement(38f, 0f, 15f, 0f, 153f, 0f, 3.5f),
            new Placement(37f, 0f, 32f, 0f, 177f, 0f, 4.8f),
            new Placement(-37.5f, 0f, -20f, 0f, 201f, 0f, 3f),
            new Placement(-38f, 0f, 20f, 0f, 225f, 0f, 4.4f)
        };

        private static readonly Placement[] BushPlacements =
        {
            new Placement(-20f, 0f, -25f, 0f, 0f, 0f, 1f),
            new Placement(8f, 0f, 22f, 0f, 45f, 0f, 1.1f),
            new Placement(32f, 0f, -10f, 0f, 90f, 0f, 0.9f)
        };

        private static readonly Placement[] DryTreePlacements =
        {
            new Placement(-24f, 0f, 20f, 0f, 15f, 0f, 1f),
            new Placement(18f, 0f, -22f, 0f, 60f, 0f, 1.1f)
        };

        private static readonly CrystalPlacement[] CrystalPlacements =
        {
            new CrystalPlacement(21f, 0f, 23f, 0f, true, true),
            new CrystalPlacement(28f, 0f, 17f, 37f, false, false),
            new CrystalPlacement(25f, 0f, 25f, 74f, false, true),
            new CrystalPlacement(-18f, 0f, 12f, 111f, true, false),
            new CrystalPlacement(-24f, 0f, 24f, 148f, false, false),
            new CrystalPlacement(12f, 0f, -14f, 185f, true, false),
            new CrystalPlacement(-10f, 0f, -26f, 222f, false, true),
            new CrystalPlacement(34f, 0f, -18f, 259f, true, false),
            new CrystalPlacement(0f, 0f, 30f, 296f, true, true),
            new CrystalPlacement(34f, 0f, 18f, 333f, false, false)
        };

        private static readonly Vector3[] AmbientLightPositions =
        {
            new Vector3(-16f, 2.5f, -2.5f),
            new Vector3(4.5f, 2.5f, 9f),
            new Vector3(18f, 2.5f, 16f)
        };

        private static readonly Vector3 CrystalCubeScale = new Vector3(0.3f, 1.2f, 0.3f);
        private static readonly Vector3[] CrystalCubePositions =
        {
            new Vector3(0f, 0.6f, 0f),
            new Vector3(0.18f, 0.6f, 0.08f),
            new Vector3(-0.16f, 0.6f, -0.1f)
        };

        private static readonly Vector3[] CrystalCubeEulers =
        {
            new Vector3(10f, 0f, 15f),
            new Vector3(-15f, 40f, -10f),
            new Vector3(5f, 80f, 20f)
        };

        private static readonly Color CyanLightColor = new Color(0.2f, 0.9f, 1f, 1f);
        private static readonly Color VioletLightColor = new Color(0.7f, 0.25f, 1f, 1f);
        private static readonly Color AmbientLightColor = new Color(180f / 255f, 60f / 255f, 1f, 1f);
        private static readonly Color StoneDarkColor = new Color(60f / 255f, 55f / 255f, 75f / 255f, 1f);
        private static readonly Color CrystalVioletBaseColor = new Color(0.45f, 0.08f, 0.85f, 1f);
        private static readonly Color CrystalVioletEmissionColor = new Color(1.6f, 0.2f, 2.2f, 1f);

        private static readonly List<Vector3> ProtectedPoints = new List<Vector3>();
        private static readonly List<ForestScatter.Segment> PathSegments = new List<ForestScatter.Segment>();
        private static readonly List<ForestScatter.Footprint> Footprints = new List<ForestScatter.Footprint>();

        private static Transform regionTransform;

        private readonly struct Placement
        {
            public readonly Vector3 Position;
            public readonly Vector3 Euler;
            public readonly float Scale;

            public Placement(float x, float y, float z, float pitch, float yaw, float roll, float scale)
            {
                Position = new Vector3(x, y, z);
                Euler = new Vector3(pitch, yaw, roll);
                Scale = scale;
            }
        }

        private readonly struct CrystalPlacement
        {
            public readonly Vector3 Position;
            public readonly float Yaw;
            public readonly bool Violet;
            public readonly bool HasLight;

            public CrystalPlacement(float x, float y, float z, float yaw, bool violet, bool hasLight)
            {
                Position = new Vector3(x, y, z);
                Yaw = yaw;
                Violet = violet;
                HasLight = hasLight;
            }
        }

        [MenuItem("Tools/ECHO/Build Region B", false, 21)]
        public static void Build()
        {
            GameObject region = FindSceneObjectByName(RegionName);
            if (region == null)
            {
                Debug.LogError($"[ECHO] No se encontró '{RegionName}' en la escena activa. " +
                               "Abre Assets/Scenes/SantuarioPrincipal.unity y vuelve a ejecutar el menú.");
                return;
            }

            GameObject fragment3 = FindSceneObjectByName("Fragment_3");
            if (fragment3 == null)
            {
                Debug.LogError("[ECHO] No se encontró 'Fragment_3'. Se cancela la construcción para no dejar la región a medias.");
                return;
            }

            EnsureFolders();

            Material stoneDark = EnsureStoneDarkMaterial();
            Material crystalViolet = EnsureCrystalVioletMaterial();
            Material crystalCyan = AssetDatabase.LoadAssetAtPath<Material>(CrystalCyanPath);
            if (crystalCyan == null)
            {
                Debug.LogError($"[ECHO] No se encontró el material '{CrystalCyanPath}' para los cristales cian.");
            }

            if (stoneDark == null || crystalViolet == null || crystalCyan == null)
            {
                return;
            }

            GameObject wallHalf = LoadPrefab("ENV_Wall_Half");
            GameObject wallCornerHalf = LoadPrefab("ENV_Wall_CornerHalf");
            GameObject column = LoadPrefab("ENV_Column");
            GameObject columnDamaged = LoadPrefab("ENV_Column_Damaged");
            GameObject rockLarge = LoadPrefab("ENV_Rock_Large");
            GameObject rockSmall = LoadPrefab("ENV_Rock_Small");
            GameObject bush = LoadPrefab("ENV_Bush");
            GameObject bushDetailed = LoadPrefab("ENV_BushDetailed");
            GameObject dryTree = LoadPrefab("ENV_Tree_Default");

            if (wallHalf == null || wallCornerHalf == null || column == null || columnDamaged == null ||
                rockLarge == null || rockSmall == null || bush == null || bushDetailed == null || dryTree == null)
            {
                Debug.LogError("[ECHO] Faltan prefabs ENV_* necesarios para la Región B. No se hacen cambios.");
                return;
            }

            regionTransform = region.transform;

            GameObject crystalPrefab = EnsureCrystalClusterPrefab(crystalCyan);
            if (crystalPrefab == null)
            {
                Debug.LogError("[ECHO] No se pudo preparar el prefab Crystal_Cluster. No se hacen cambios.");
                return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Build Region B");

            DestroyContainers(regionTransform);

            Transform cliffs = CreateContainer(regionTransform, "Cliffs_B");
            Transform decor = CreateContainer(regionTransform, "Decor_B");
            Transform path = CreateContainer(regionTransform, "Path_B");
            Transform crystals = CreateContainer(regionTransform, "Crystals_B");
            Transform stairs = CreateContainer(regionTransform, "Stairs_B");

            ProtectedPoints.Clear();
            ProtectedPoints.Add(ResolvePoint("Portal_B", new Vector3(170f, 0f, -10f)));
            ProtectedPoints.Add(ResolvePoint("Marker_PortalB_Arrival", new Vector3(170f, 0.1f, -16f)));
            ProtectedPoints.Add(ResolvePoint("Fragment_2", new Vector3(225f, 1f, 20f)));
            ProtectedPoints.Add(new Vector3(190f, 2.5f, 32f));

            Footprints.Clear();
            ForestScatter.Rng rng = new ForestScatter.Rng(ForestSeed);
            Placement[] scaledRuinPlacements = ScalePlacements(RuinPlacements, RuinScaleRange, ref rng);
            Placement[] scaledBushPlacements = ScalePlacements(BushPlacements, BushScaleRange, ref rng);
            Placement[] scaledTreePlacements = ScalePlacements(DryTreePlacements, TreeScaleRange, ref rng);

            int cliffCount = BuildCliffs(cliffs, stoneDark);
            int slabCount = BuildPath(path, regionTransform, stoneDark);

            int stepCount;
            int platformCount;
            BuildStairs(stairs, stoneDark, out stepCount, out platformCount);

            MoveFragment3(fragment3);

            int omitted = 0;

            int ruinCount = PlaceRuins(
                decor,
                new[] { wallHalf, wallCornerHalf, column, columnDamaged },
                stoneDark,
                scaledRuinPlacements,
                ref omitted);

            int cliffRockCount = PlacePrefabSeries(
                new[] { rockLarge, rockSmall },
                CliffRockPlacements,
                decor,
                "Cliff_Rock",
                stoneDark,
                false,
                DecorPathClearance,
                false,
                0.7f,
                ref omitted);

            int bushCount = PlacePrefabSeries(
                new[] { bush, bushDetailed },
                scaledBushPlacements,
                decor,
                "Bush",
                stoneDark,
                false,
                DecorPathClearance,
                false,
                0.4f,
                ref omitted);

            int treeCount = PlacePrefabSeries(
                new[] { dryTree },
                scaledTreePlacements,
                decor,
                "Dry_Tree",
                stoneDark,
                true,
                DecorPathClearance,
                false,
                0.5f,
                ref omitted);

            int crystalLightCount;
            int crystalCount = PlaceCrystals(
                crystals,
                crystalPrefab,
                crystalCyan,
                crystalViolet,
                out crystalLightCount,
                ref omitted);

            int denseTreeCount = PlaceProceduralSeries(
                new[] { dryTree },
                ExtraTreeCount,
                decor,
                "Dry_Tree_Dense",
                "árbol seco denso",
                TreeScaleRange,
                0.5f,
                stoneDark,
                true,
                ref rng,
                ref omitted);

            int denseBushCount = PlaceProceduralSeries(
                new[] { bush, bushDetailed },
                ExtraBushCount,
                decor,
                "Bush_Dense",
                "arbusto denso",
                BushScaleRange,
                0.4f,
                stoneDark,
                false,
                ref rng,
                ref omitted);

            int denseRockCount = PlaceProceduralSeries(
                new[] { rockLarge, rockSmall },
                ExtraRockCount,
                decor,
                "Rock_Dense",
                "roca decorativa",
                RockScaleRange,
                0.55f,
                stoneDark,
                false,
                ref rng,
                ref omitted);

            int ambientCount = PlaceAmbientLights(decor);

            EditorSceneManager.MarkSceneDirty(region.scene);
            Undo.CollapseUndoOperations(undoGroup);

            Selection.activeGameObject = region;
            EditorGUIUtility.PingObject(region);

            Debug.Log($"[ECHO] Región B construida: {cliffCount} muros, {slabCount} losas, " +
                      $"{stepCount} escalones + {platformCount} plataforma, {ruinCount} ruinas, " +
                      $"{cliffRockCount} rocas de acantilado, " +
                      $"{bushCount} arbustos base + {denseBushCount} nuevos, " +
                      $"{treeCount} árboles secos base + {denseTreeCount} nuevos, " +
                      $"{denseRockCount} rocas decorativas nuevas, " +
                      $"{crystalCount} cristales ({crystalLightCount} con luz), " +
                      $"{ambientCount} luces de ambiente. Semilla {ForestSeed:X8}. " +
                      $"Fragment_3 en mundo {fragment3.transform.position.ToString("F2")}.", region);

            if (omitted > 0)
            {
                Debug.LogWarning(
                    $"[ECHO] Se omitieron {omitted} piezas por invadir zonas protegidas, el camino o el espacio disponible.",
                    region);
            }
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            if (!AssetDatabase.IsValidFolder(PrefabFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
        }

        private static Material EnsureStoneDarkMaterial()
        {
            Shader shader = Shader.Find(LitShaderName);
            if (shader == null)
            {
                Debug.LogError($"[ECHO] No se encontró el shader '{LitShaderName}'.");
                return null;
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(StoneDarkPath);
            if (material == null)
            {
                material = new Material(shader);
                material.SetColor("_BaseColor", StoneDarkColor);
                material.SetColor("_Color", StoneDarkColor);
                AssetDatabase.CreateAsset(material, StoneDarkPath);
            }
            else
            {
                Undo.RecordObject(material, "Build Region B");
                if (material.shader != shader)
                {
                    material.shader = shader;
                }

                material.SetColor("_BaseColor", StoneDarkColor);
                material.SetColor("_Color", StoneDarkColor);
            }

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        private static Material EnsureCrystalVioletMaterial()
        {
            Shader shader = Shader.Find(LitShaderName);
            if (shader == null)
            {
                Debug.LogError($"[ECHO] No se encontró el shader '{LitShaderName}'.");
                return null;
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(CrystalVioletPath);
            if (material == null)
            {
                material = new Material(shader);
                material.SetColor("_BaseColor", CrystalVioletBaseColor);
                material.SetColor("_Color", CrystalVioletBaseColor);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", CrystalVioletEmissionColor);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                AssetDatabase.CreateAsset(material, CrystalVioletPath);
            }
            else
            {
                Undo.RecordObject(material, "Build Region B");
                if (material.shader != shader)
                {
                    material.shader = shader;
                }

                material.SetColor("_BaseColor", CrystalVioletBaseColor);
                material.SetColor("_Color", CrystalVioletBaseColor);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", CrystalVioletEmissionColor);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        private static GameObject LoadPrefab(string prefabName)
        {
            string path = PrefabFolder + "/" + prefabName + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"[ECHO] No se encontró el prefab '{path}'.");
            }

            return prefab;
        }

        private static GameObject EnsureCrystalClusterPrefab(Material crystalCyan)
        {
            string path = PrefabFolder + "/" + CrystalPrefabName + ".prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (IsCrystalClusterValid(existing))
            {
                return existing;
            }

            if (existing != null && !AssetDatabase.DeleteAsset(path))
            {
                Debug.LogError($"[ECHO] No se pudo reemplazar el prefab '{path}'.");
                return null;
            }

            GameObject root = new GameObject(CrystalPrefabName);

            for (int i = 0; i < CrystalCubePositions.Length; i++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = $"Crystal_{i + 1:00}";

                Collider collider = cube.GetComponent<Collider>();
                if (collider != null)
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }

                cube.transform.SetParent(root.transform, false);
                cube.transform.localPosition = CrystalCubePositions[i];
                cube.transform.localRotation = Quaternion.Euler(CrystalCubeEulers[i]);
                cube.transform.localScale = CrystalCubeScale;

                Renderer renderer = cube.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = crystalCyan;
                }
            }

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();

            if (saved == null)
            {
                Debug.LogError($"[ECHO] No se pudo guardar el prefab '{path}'.");
            }

            return saved;
        }

        private static bool IsCrystalClusterValid(GameObject prefab)
        {
            if (prefab == null)
            {
                return false;
            }

            if (prefab.transform.childCount != CrystalCubePositions.Length)
            {
                return false;
            }

            if (prefab.GetComponentsInChildren<Collider>(true).Length != 0)
            {
                return false;
            }

            foreach (Transform child in prefab.transform)
            {
                if (child.GetComponent<MeshFilter>() == null)
                {
                    return false;
                }

                Vector3 scale = child.localScale;
                if (Mathf.Abs(scale.x - CrystalCubeScale.x) > 0.01f ||
                    Mathf.Abs(scale.y - CrystalCubeScale.y) > 0.01f ||
                    Mathf.Abs(scale.z - CrystalCubeScale.z) > 0.01f)
                {
                    return false;
                }
            }

            return true;
        }

        private static void DestroyContainers(Transform region)
        {
            List<Transform> matches = new List<Transform>();
            foreach (Transform candidate in region.GetComponentsInChildren<Transform>(true))
            {
                if (candidate != region && System.Array.IndexOf(ContainerNames, candidate.name) >= 0)
                {
                    matches.Add(candidate);
                }
            }

            HashSet<Transform> matchSet = new HashSet<Transform>(matches);
            foreach (Transform match in matches)
            {
                bool nested = false;
                Transform parent = match.parent;
                while (parent != null && parent != region)
                {
                    if (matchSet.Contains(parent))
                    {
                        nested = true;
                        break;
                    }

                    parent = parent.parent;
                }

                if (!nested)
                {
                    Undo.DestroyObjectImmediate(match.gameObject);
                }
            }
        }

        private static Transform CreateContainer(Transform region, string containerName)
        {
            GameObject created = new GameObject(containerName);
            Undo.RegisterCreatedObjectUndo(created, "Build Region B");
            created.transform.SetParent(region, false);
            created.transform.localPosition = Vector3.zero;
            created.transform.localRotation = Quaternion.identity;
            created.transform.localScale = Vector3.one;
            return created.transform;
        }

        private static int BuildCliffs(Transform parent, Material material)
        {
            string[] names = { "Cliff_N", "Cliff_S", "Cliff_E", "Cliff_W" };
            Vector3[] positions =
            {
                new Vector3(0f, 7f, 40f),
                new Vector3(0f, 7f, -40f),
                new Vector3(40f, 7f, 0f),
                new Vector3(-40f, 7f, 0f)
            };
            Vector3[] scales =
            {
                new Vector3(84f, 14f, 4f),
                new Vector3(84f, 14f, 4f),
                new Vector3(4f, 14f, 84f),
                new Vector3(4f, 14f, 84f)
            };

            for (int i = 0; i < names.Length; i++)
            {
                CreateCube(names[i], parent, positions[i], Quaternion.identity, scales[i], material);
            }

            return names.Length;
        }

        private static int BuildPath(Transform parent, Transform region, Material material)
        {
            int count = 0;

            PathSegments.Clear();
            ForestScatter.AddPolylineSegments(PathSegments, MainPathPositions, region);
            ForestScatter.AddPolylineSegments(PathSegments, BranchPathPositions, region);

            for (int i = 0; i < MainPathPositions.Length; i++)
            {
                Vector2 point = MainPathPositions[i];
                CreateCube(
                    $"Path_Slab_{i + 1:00}",
                    parent,
                    new Vector3(point.x, 0.05f, point.y),
                    Quaternion.identity,
                    new Vector3(1.5f, 0.1f, 1.5f),
                    material);

                count++;
            }

            for (int i = 0; i < BranchPathPositions.Length; i++)
            {
                Vector2 point = BranchPathPositions[i];
                CreateCube(
                    $"Path_Branch_{i + 1:00}",
                    parent,
                    new Vector3(point.x, 0.05f, point.y),
                    Quaternion.identity,
                    new Vector3(1.5f, 0.1f, 1.5f),
                    material);

                count++;
            }

            return count;
        }

        private static void BuildStairs(Transform parent, Material material, out int stepCount, out int platformCount)
        {
            CreateCube(
                "Platform",
                parent,
                new Vector3(-10f, 0.75f, 32f),
                Quaternion.identity,
                new Vector3(6f, 1.5f, 6f),
                material);

            platformCount = 1;

            float[] heights = { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f };
            float[] zPositions = { 22.4f, 23.6f, 24.8f, 26f, 27.2f, 28.4f };

            for (int i = 0; i < heights.Length; i++)
            {
                CreateCube(
                    $"Step_{i + 1:00}",
                    parent,
                    new Vector3(-10f, heights[i] * 0.5f, zPositions[i]),
                    Quaternion.identity,
                    new Vector3(3f, heights[i], 1.2f),
                    material);
            }

            stepCount = heights.Length;
        }

        private static int PlaceRuins(
            Transform parent,
            GameObject[] prefabs,
            Material material,
            Placement[] placements,
            ref int omitted)
        {
            int placed = 0;

            for (int i = 0; i < placements.Length; i++)
            {
                int prefabIndex = RuinPrefabIndices[i];
                if (prefabIndex < 0 || prefabIndex >= prefabs.Length)
                {
                    continue;
                }

                if (TryPlacePrefab(
                        prefabs[prefabIndex],
                        placements[i],
                        parent,
                        $"Ruin_{i + 1:00}",
                        material,
                        false,
                        DecorPathClearance,
                        false,
                        1f,
                        ref omitted))
                {
                    placed++;
                }
            }

            return placed;
        }

        private static int PlacePrefabSeries(
            GameObject[] prefabs,
            Placement[] placements,
            Transform parent,
            string prefix,
            Material material,
            bool keepFirstMaterial,
            float pathClearance,
            bool checkFootprints,
            float footprintBaseRadius,
            ref int omitted)
        {
            int placed = 0;

            for (int i = 0; i < placements.Length; i++)
            {
                GameObject prefab = prefabs[i % prefabs.Length];
                if (prefab == null)
                {
                    continue;
                }

                if (TryPlacePrefab(
                        prefab,
                        placements[i],
                        parent,
                        $"{prefix}_{i + 1:00}",
                        material,
                        keepFirstMaterial,
                        pathClearance,
                        checkFootprints,
                        footprintBaseRadius,
                        ref omitted))
                {
                    placed++;
                }
            }

            return placed;
        }

        private static bool TryPlacePrefab(
            GameObject prefab,
            Placement placement,
            Transform parent,
            string instanceName,
            Material material,
            bool keepFirstMaterial,
            float pathClearance,
            bool checkFootprints,
            float footprintBaseRadius,
            ref int omitted)
        {
            Vector3 worldPosition = parent.TransformPoint(placement.Position);
            float footprintRadius = footprintBaseRadius * placement.Scale;
            if (!IsFree(worldPosition, pathClearance, checkFootprints, footprintRadius))
            {
                Debug.LogWarning($"[ECHO] '{instanceName}' omitido: invade una zona protegida, el camino u otra pieza.", parent);
                omitted++;
                return false;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            if (instance == null)
            {
                Debug.LogError($"[ECHO] No se pudo instanciar el prefab de '{instanceName}'.", parent);
                omitted++;
                return false;
            }

            Undo.RegisterCreatedObjectUndo(instance, "Build Region B");
            instance.name = instanceName;
            instance.transform.localPosition = placement.Position;
            instance.transform.localRotation = Quaternion.Euler(placement.Euler);
            instance.transform.localScale = Vector3.one * placement.Scale;

            ApplyMaterials(instance, material, keepFirstMaterial);
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);

            Footprints.Add(new ForestScatter.Footprint(
                new Vector2(worldPosition.x, worldPosition.z),
                footprintRadius));

            return true;
        }

        private static int PlaceProceduralSeries(
            GameObject[] prefabs,
            int targetCount,
            Transform parent,
            string prefix,
            string category,
            Vector2 scaleRange,
            float footprintBaseRadius,
            Material material,
            bool keepFirstMaterial,
            ref ForestScatter.Rng rng,
            ref int omitted)
        {
            int placed = 0;
            int attempts = 0;
            int maxAttempts = Mathf.Max(targetCount * 80, 800);

            while (placed < targetCount && attempts < maxAttempts)
            {
                attempts++;
                Vector2 sampled = ForestScatter.SampleAnnulus(
                    ref rng,
                    ForestScatter.InnerRadius,
                    ForestScatter.OuterRadius);
                Vector3 localPosition = new Vector3(sampled.x, 0f, sampled.y);
                Vector3 worldPosition = parent.TransformPoint(localPosition);
                float scale = rng.Range(scaleRange.x, scaleRange.y);
                float footprintRadius = footprintBaseRadius * scale;

                if (!IsFree(worldPosition, DecorPathClearance, true, footprintRadius))
                {
                    continue;
                }

                GameObject prefab = prefabs[placed % prefabs.Length];
                if (prefab == null)
                {
                    continue;
                }

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                if (instance == null)
                {
                    Debug.LogError($"[ECHO] No se pudo instanciar '{prefix}_{placed + 1:00}'.", parent);
                    omitted++;
                    break;
                }

                Undo.RegisterCreatedObjectUndo(instance, "Build Region B");
                instance.name = $"{prefix}_{placed + 1:00}";
                instance.transform.localPosition = localPosition;
                instance.transform.localRotation = Quaternion.Euler(0f, rng.Range(0f, 360f), 0f);
                instance.transform.localScale = Vector3.one * scale;

                ApplyMaterials(instance, material, keepFirstMaterial);
                PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);

                Footprints.Add(new ForestScatter.Footprint(
                    new Vector2(worldPosition.x, worldPosition.z),
                    footprintRadius));
                placed++;
            }

            if (placed < targetCount)
            {
                omitted += targetCount - placed;
                Debug.LogWarning(
                    $"[ECHO] {category}: solo se colocaron {placed}/{targetCount} piezas tras {attempts} intentos. " +
                    "Revisa el radio libre, los caminos, las escaleras y la separación entre piezas.",
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
                    placement.Euler.x,
                    placement.Euler.y,
                    placement.Euler.z,
                    rng.Range(scaleRange.x, scaleRange.y));
            }

            return result;
        }

        private static int PlaceCrystals(
            Transform parent,
            GameObject prefab,
            Material crystalCyan,
            Material crystalViolet,
            out int lightCount,
            ref int omitted)
        {
            int placed = 0;
            lightCount = 0;

            for (int i = 0; i < CrystalPlacements.Length; i++)
            {
                CrystalPlacement placement = CrystalPlacements[i];
                Vector3 worldPosition = parent.TransformPoint(placement.Position);
                if (!IsFree(worldPosition, CrystalPathClearance, false, 1.5f))
                {
                    Debug.LogWarning($"[ECHO] 'Crystal_Cluster_{i + 1:00}' omitido: invade una zona protegida o el camino.", parent);
                    omitted++;
                    continue;
                }

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                if (instance == null)
                {
                    Debug.LogError($"[ECHO] No se pudo instanciar 'Crystal_Cluster_{i + 1:00}'.", parent);
                    omitted++;
                    continue;
                }

                Undo.RegisterCreatedObjectUndo(instance, "Build Region B");
                instance.name = $"Crystal_Cluster_{i + 1:00}";
                instance.transform.localPosition = placement.Position;
                instance.transform.localRotation = Quaternion.Euler(0f, placement.Yaw, 0f);
                instance.transform.localScale = Vector3.one;

                Material material = placement.Violet ? crystalViolet : crystalCyan;
                ApplyMaterials(instance, material, false);
                PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);

                Footprints.Add(new ForestScatter.Footprint(
                    new Vector2(worldPosition.x, worldPosition.z),
                    1.5f));

                if (placement.HasLight)
                {
                    CreatePointLight(
                        instance.transform,
                        "Crystal_Light",
                        new Vector3(0f, 1.2f, 0f),
                        placement.Violet ? VioletLightColor : CyanLightColor,
                        1.5f,
                        5f);

                    lightCount++;
                }

                placed++;
            }

            return placed;
        }

        private static int PlaceAmbientLights(Transform parent)
        {
            for (int i = 0; i < AmbientLightPositions.Length; i++)
            {
                CreatePointLight(
                    parent,
                    $"Ambient_Light_{i + 1:00}",
                    AmbientLightPositions[i],
                    AmbientLightColor,
                    1.5f,
                    8f);
            }

            return AmbientLightPositions.Length;
        }

        private static Light CreatePointLight(
            Transform parent,
            string lightName,
            Vector3 localPosition,
            Color color,
            float intensity,
            float range)
        {
            GameObject lightObject = new GameObject(lightName);
            Undo.RegisterCreatedObjectUndo(lightObject, "Build Region B");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = localPosition;
            lightObject.transform.localRotation = Quaternion.identity;
            lightObject.transform.localScale = Vector3.one;

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;

            return light;
        }

        private static void ApplyMaterials(GameObject root, Material material, bool keepFirstMaterial)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0)
                {
                    continue;
                }

                int startIndex = keepFirstMaterial && materials.Length > 1 ? 1 : 0;
                for (int i = startIndex; i < materials.Length; i++)
                {
                    materials[i] = material;
                }

                renderer.sharedMaterials = materials;

                if (PrefabUtility.IsPartOfPrefabInstance(renderer))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                }
            }
        }

        private static GameObject CreateCube(
            string cubeName,
            Transform parent,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = cubeName;
            Undo.RegisterCreatedObjectUndo(cube, "Build Region B");
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = localRotation;
            cube.transform.localScale = localScale;

            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            return cube;
        }

        private static void MoveFragment3(GameObject fragment3)
        {
            Undo.RecordObject(fragment3.transform, "Move Fragment_3");
            fragment3.transform.position = new Vector3(190f, 2.5f, 32f);

            if (PrefabUtility.IsPartOfPrefabInstance(fragment3.transform))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(fragment3.transform);
            }

            EditorUtility.SetDirty(fragment3);
        }

        private static bool IsFree(
            Vector3 worldPosition,
            float pathClearance,
            bool checkFootprints,
            float footprintRadius)
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

            if (IsInsideStairsZone(worldPosition))
            {
                return false;
            }

            if (ForestScatter.IsNearPath(point, PathSegments, pathClearance))
            {
                return false;
            }

            if (checkFootprints && ForestScatter.OverlapsAny(point, footprintRadius, Footprints))
            {
                return false;
            }

            return true;
        }

        private static bool IsInsideStairsZone(Vector3 worldPosition)
        {
            if (regionTransform == null)
            {
                return false;
            }

            Vector3 localPosition = regionTransform.InverseTransformPoint(worldPosition);
            return localPosition.x >= StairsMinX && localPosition.x <= StairsMaxX &&
                   localPosition.z >= StairsMinZ && localPosition.z <= StairsMaxZ;
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
