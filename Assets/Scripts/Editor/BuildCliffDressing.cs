using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Echo.EditorTools
{
    // Bloque 1: vestimenta de acantilados. Desactiva el MeshRenderer de Cliffs_A/Cliffs_B
    // (los colliders quedan intactos) y genera Dressing_A/Dressing_B con una fila continua
    // de modelos cliff_block* por lado, filas apiladas hasta ~16-18 m y montañas de fondo.
    public static class BuildCliffDressing
    {
        private const string MenuName = "Build Cliff Dressing";
        private const string NatureModels = "Assets/Models/Kenney_NatureKit/FBX format";
        private const string StoneDarkPath = "Assets/Materials/M_StoneDark.mat";

        // Muros de 84x14x4 centrados en los bordes de cada región (z/x = ±40).
        // La cara exterior queda en ±42, que coincide con la mitad del vano de 84 m.
        private const float WallHalfSpan = 42f;
        private const float WallTargetHeight = 15.75f;
        private const int MaxRows = 4;
        private const float ScaleMin = 6f;
        private const float ScaleMax = 10f;
        private const float LengthFactor = 0.88f; // solape del 12% a lo largo del lado
        private const float VerticalOverlap = 0.15f;
        private const float Row1Sink = 1f;
        private const float PeakMinScale = 10f;
        private const float PeakMaxScale = 14f;
        private const float PeakSink = 3f;
        private const float PeakMinDist = 48f;
        private const float PeakMaxDist = 60f;
        private const uint SeedA = 0xA1C11F01u;
        private const uint SeedB = 0xB2C22F02u;

        private static readonly string[] WallModelNames =
        {
            "cliff_block_rock", "cliff_block_stone",
            "cliff_blockSlope_rock", "cliff_blockSlope_stone",
            "cliff_blockSlopeWalls_rock", "cliff_blockSlopeWalls_stone"
        };

        private static readonly string[] CapModelNames =
        {
            "cliff_blockHalf_rock", "cliff_blockHalf_stone",
            "cliff_blockQuarter_rock", "cliff_blockQuarter_stone"
        };

        private static readonly string[] PeakModelNames =
        {
            "cliff_large_rock", "cliff_large_stone", "cliff_stone"
        };

        private static readonly Side[] Sides =
        {
            new Side("N", new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 1f)),
            new Side("S", new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, -1f)),
            new Side("E", new Vector3(0f, 0f, 1f), new Vector3(1f, 0f, 0f)),
            new Side("W", new Vector3(0f, 0f, 1f), new Vector3(-1f, 0f, 0f))
        };

        private static Material _stoneDark;

        [MenuItem("Tools/ECHO/Build Cliff Dressing", false, 28)]
        public static void Execute()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning($"[ECHO] {MenuName}: solo disponible en modo edición.");
                return;
            }

            _stoneDark = AssetDatabase.LoadAssetAtPath<Material>(StoneDarkPath);
            if (_stoneDark == null)
            {
                Debug.LogError($"[ECHO] {MenuName}: falta el material '{StoneDarkPath}'. Revisa la ruta.");
                return;
            }

            List<ModelRef> wall = LoadModels(WallModelNames, "muro");
            List<ModelRef> caps = LoadModels(CapModelNames, "coronamiento");
            List<ModelRef> peaks = LoadModels(PeakModelNames, "montaña");
            if (wall.Count == 0 || peaks.Count == 0)
            {
                Debug.LogError($"[ECHO] {MenuName}: faltan FBX cliff_block* / cliff_large* en '{NatureModels}'.");
                return;
            }

            BuildRegion("Dressing_A", "Cliffs_A", Vector3.zero, SeedA, false, wall, caps, peaks,
                out int piecesA, out int peaksA, out int trisA);
            BuildRegion("Dressing_B", "Cliffs_B", new Vector3(200f, 0f, 0f), SeedB, true, wall, caps, peaks,
                out int piecesB, out int peaksB, out int trisB);

            Debug.Log($"[ECHO] {MenuName}: Región A {piecesA} piezas ({trisA:N0} tris) + {peaksA} montañas; " +
                      $"Región B {piecesB} piezas ({trisB:N0} tris) + {peaksB} montañas.");
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void BuildRegion(
            string containerName,
            string cliffsName,
            Vector3 center,
            uint seed,
            bool dark,
            List<ModelRef> wall,
            List<ModelRef> caps,
            List<ModelRef> peaks,
            out int pieceCount,
            out int peakCount,
            out int triangleCount)
        {
            pieceCount = 0;
            peakCount = 0;
            triangleCount = 0;

            DisableCliffRenderers(cliffsName);

            GameObject previous = FindObjectByName(containerName);
            if (previous != null)
            {
                Undo.DestroyObjectImmediate(previous);
            }

            GameObject container = new GameObject(containerName);
            container.transform.position = center;
            Undo.RegisterCreatedObjectUndo(container, MenuName);

            ForestScatter.Rng rng = new ForestScatter.Rng(seed);
            int sidePieces = 0;

            for (int sideIndex = 0; sideIndex < Sides.Length; sideIndex++)
            {
                Side side = Sides[sideIndex];
                List<Column> columns = new List<Column>();

                // Fila base: recorre el lado colocando bloques con solape.
                float cursor = -WallHalfSpan;
                while (cursor <= WallHalfSpan + 0.001f)
                {
                    float along = Mathf.Clamp(cursor, -WallHalfSpan, WallHalfSpan);
                    ModelRef model = PickModel(0, wall, caps, ref rng);
                    float scale = rng.Range(ScaleMin, ScaleMax);
                    float height = model.Size.Height * scale;
                    float length = model.Size.Length * scale * LengthFactor;
                    float distance = WallHalfSpan + rng.Range(0f, 2f);
                    Vector3 local = side.Run * along + side.Normal * distance;
                    float y = -Row1Sink - model.Size.MinY * scale;

                    GameObject piece = (GameObject)PrefabUtility.InstantiatePrefab(model.Asset, container.transform);
                    Undo.RegisterCreatedObjectUndo(piece, MenuName);
                    piece.name = $"Dress_{side.Label}_{sidePieces + 1:000}";
                    sidePieces++;
                    piece.transform.localPosition = new Vector3(local.x, y, local.z);
                    piece.transform.localRotation = BuildRotation(ref rng);
                    piece.transform.localScale = Vector3.one * scale;
                    SetBatchingStatic(piece);
                    triangleCount += CountTriangles(piece);

                    columns.Add(new Column { Local = new Vector3(local.x, 0f, local.z), Top = -Row1Sink + height });
                    pieceCount++;
                    cursor += Mathf.Max(length, 2f);
                }

                // Filas superiores por columna hasta llegar a ~16-18 m.
                for (int row = 1; row < MaxRows; row++)
                {
                    bool changed = false;
                    for (int i = 0; i < columns.Count; i++)
                    {
                        Column column = columns[i];
                        if (column.Top >= WallTargetHeight)
                        {
                            continue;
                        }

                        ModelRef model = PickModel(row, wall, caps, ref rng);
                        float scale = rng.Range(ScaleMin, ScaleMax);
                        float height = model.Size.Height * scale;
                        float bottom = column.Top - height * VerticalOverlap;
                        float y = bottom - model.Size.MinY * scale;

                        GameObject piece = (GameObject)PrefabUtility.InstantiatePrefab(model.Asset, container.transform);
                        Undo.RegisterCreatedObjectUndo(piece, MenuName);
                        piece.name = $"Dress_{side.Label}_{sidePieces + 1:000}";
                        sidePieces++;
                        piece.transform.localPosition = new Vector3(column.Local.x, y, column.Local.z);
                        piece.transform.localRotation = BuildRotation(ref rng);
                        piece.transform.localScale = Vector3.one * scale;
                        SetBatchingStatic(piece);
                        triangleCount += CountTriangles(piece);

                        column.Top = bottom + height;
                        columns[i] = column;
                        changed = true;
                        pieceCount++;
                    }

                    if (!changed)
                    {
                        break;
                    }
                }
            }

            // Montañas de fondo detrás de los muros, sin colliders.
            for (int i = 0; i < 4; i++)
            {
                Side side = Sides[(int)(rng.Range(0f, Sides.Length)) % Sides.Length];
                ModelRef model = peaks[(int)(rng.Range(0f, peaks.Count))];
                float scale = rng.Range(PeakMinScale, PeakMaxScale);
                Vector3 local = side.Run * rng.Range(-30f, 30f) + side.Normal * rng.Range(PeakMinDist, PeakMaxDist);
                float y = -PeakSink - model.Size.MinY * scale;

                GameObject peak = (GameObject)PrefabUtility.InstantiatePrefab(model.Asset, container.transform);
                Undo.RegisterCreatedObjectUndo(peak, MenuName);
                peak.name = $"Peak_{i + 1:00}";
                peak.transform.localPosition = new Vector3(local.x, y, local.z);
                peak.transform.localRotation = Quaternion.Euler(
                    rng.Range(-5f, 5f), rng.Range(0f, 360f), rng.Range(-5f, 5f));
                peak.transform.localScale = Vector3.one * scale;
                SetBatchingStatic(peak);
                triangleCount += CountTriangles(peak);
                peakCount++;
            }

            if (dark)
            {
                ApplyDarkMaterial(container, _stoneDark);
            }

            Debug.Log($"[ECHO] {containerName}: {pieceCount} piezas, {peakCount} montañas, " +
                      $"{triangleCount:N0} triángulos (semilla 0x{seed:X8}).");
        }

        private static void DisableCliffRenderers(string cliffsName)
        {
            GameObject cliffs = FindObjectByName(cliffsName);
            if (cliffs == null)
            {
                Debug.LogError($"[ECHO] {MenuName}: no se encontró '{cliffsName}' en la escena.");
                return;
            }

            int disabled = 0;
            foreach (MeshRenderer renderer in cliffs.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (renderer.enabled)
                {
                    Undo.RecordObject(renderer, MenuName);
                    renderer.enabled = false;
                    disabled++;
                }
            }

            Debug.Log($"[ECHO] {cliffsName}: {disabled} MeshRenderer desactivado(s); colliders intactos.");
        }

        private static void ApplyDarkMaterial(GameObject container, Material material)
        {
            foreach (MeshRenderer renderer in container.GetComponentsInChildren<MeshRenderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != material)
                    {
                        materials[i] = material;
                        changed = true;
                    }
                }

                if (changed)
                {
                    Undo.RecordObject(renderer, MenuName);
                    renderer.sharedMaterials = materials;
                }
            }
        }

        private static ModelRef PickModel(int row, List<ModelRef> wall, List<ModelRef> caps, ref ForestScatter.Rng rng)
        {
            float roll = rng.Range(0f, 100f);
            if (row > 0 && caps.Count > 0 && roll >= 40f)
            {
                return caps[(int)(rng.Range(0f, caps.Count))];
            }

            return wall[(int)(rng.Range(0f, wall.Count))];
        }

        private static Quaternion BuildRotation(ref ForestScatter.Rng rng)
        {
            // Pasos de 15° (1..23): el yaw siempre cambia respecto a la pieza previa.
            int steps = 1 + (int)(rng.Range(0f, 23f));
            float yaw = (steps * 15f) % 360f;
            return Quaternion.Euler(rng.Range(-3f, 3f), yaw, rng.Range(-3f, 3f));
        }

        private static void SetBatchingStatic(GameObject go)
        {
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
            foreach (Transform child in go.transform.GetComponentsInChildren<Transform>(true))
            {
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, StaticEditorFlags.BatchingStatic);
            }
        }

        private static int CountTriangles(GameObject go)
        {
            int total = 0;
            foreach (MeshFilter filter in go.GetComponentsInChildren<MeshFilter>())
            {
                Mesh mesh = filter.sharedMesh;
                if (mesh != null)
                {
                    total += mesh.triangles.Length / 3;
                }
            }

            return total;
        }

        private static GameObject FindObjectByName(string name)
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
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

        private static List<ModelRef> LoadModels(string[] names, string label)
        {
            List<ModelRef> results = new List<ModelRef>();
            foreach (string name in names)
            {
                GameObject asset = FindModelAsset(name);
                if (asset == null)
                {
                    Debug.LogError($"[ECHO] {MenuName}: no se encontró el FBX '{name}' (para la " +
                                   $"{label}) en '{NatureModels}'. Comprueba que el pack se importa.");
                    continue;
                }

                results.Add(new ModelRef(name, asset, MeasureAsset(asset)));
            }

            return results;
        }

        private static GameObject FindModelAsset(string name)
        {
            string[] guids = AssetDatabase.FindAssets(name + " t:Model", new[] { NatureModels });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == name)
                {
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }
            }

            return null;
        }

        private static ModelSize MeasureAsset(GameObject asset)
        {
            GameObject temp = Object.Instantiate(asset);
            temp.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                bool any = false;
                float minY = float.MaxValue;
                float maxY = float.MinValue;
                float maxHorizontal = 0f;
                foreach (Renderer renderer in temp.GetComponentsInChildren<Renderer>(true))
                {
                    Bounds bounds = renderer.bounds;
                    minY = Mathf.Min(minY, bounds.min.y);
                    maxY = Mathf.Max(maxY, bounds.max.y);
                    maxHorizontal = Mathf.Max(maxHorizontal, bounds.size.x, bounds.size.z);
                    any = true;
                }

                if (!any)
                {
                    return new ModelSize(0f, 1f, 1f);
                }

                return new ModelSize(minY, maxY, maxHorizontal);
            }
            finally
            {
                Object.DestroyImmediate(temp);
            }
        }

        private readonly struct Side
        {
            public readonly string Label;
            public readonly Vector3 Run;
            public readonly Vector3 Normal;

            public Side(string label, Vector3 run, Vector3 normal)
            {
                Label = label;
                Run = run;
                Normal = normal;
            }
        }

        private struct Column
        {
            public Vector3 Local;
            public float Top;
        }

        private readonly struct ModelSize
        {
            public readonly float MinY;
            public readonly float MaxY;
            public readonly float Length;

            public ModelSize(float minY, float maxY, float length)
            {
                MinY = minY;
                MaxY = maxY;
                Length = length;
            }

            public float Height => MaxY - MinY;
        }

        private readonly struct ModelRef
        {
            public readonly string Name;
            public readonly GameObject Asset;
            public readonly ModelSize Size;

            public ModelRef(string name, GameObject asset, ModelSize size)
            {
                Name = name;
                Asset = asset;
                Size = size;
            }
        }
    }
}
