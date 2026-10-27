using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Echo.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Echo.EditorTools
{
    public static class SetupPlayerCharacter
    {
        private const string CharacterPath = "Assets/Models/Player/Character.fbx";
        private const string AnimationsFolder = "Assets/Models/Player/Animations";
        private const string IdlePath = AnimationsFolder + "/Idle.fbx";
        private const string WalkPath = AnimationsFolder + "/Walk.fbx";
        private const string WalkingPath = AnimationsFolder + "/Walking.fbx";
        private const string RunPath = AnimationsFolder + "/Run.fbx";
        private const string RunningPath = AnimationsFolder + "/Running.fbx";
        private const string MaterialFolder = "Assets/Materials/Player";
        private const string TemporaryTextureFolderName = "_TempTextures";
        private const string TemporaryTextureFolder = MaterialFolder + "/" + TemporaryTextureFolderName;
        private const string AnimationsRoot = "Assets/Animations";
        private const string ControllerPath = AnimationsRoot + "/AC_Player.controller";
        private const string PrefabFolder = "Assets/Prefabs";
        private const string PlayerModelPath = PrefabFolder + "/PlayerModel.prefab";
        private const string UrpLitShaderName = "Universal Render Pipeline/Lit";
        private const string UrpShaderPrefix = "Universal Render Pipeline/";
        private const string UndoName = "Setup Player Character";
        private const string IdleClipName = "Idle";
        private const string WalkClipName = "Walk";
        private const string RunClipName = "Run";
        private const string SpeedParameter = "Speed";
        private const string GroundedParameter = "Grounded";
        private const string InteractParameter = "Interact";
        private const string VictoryParameter = "Victory";
        private const string LocomotionStateName = "Locomotion";
        private const string PlayerName = "Player";
        private const string PlayerModelName = "PlayerModel";
        private const string VisualName = "Visual";
        private const string FacingMarkerName = "FacingMarker";

        [MenuItem("Tools/ECHO/Setup Player Character", false, 27)]
        public static void Setup()
        {
            string walkPath = ResolveFirstExisting(WalkPath, WalkingPath);
            string runPath = ResolveFirstExisting(RunPath, RunningPath);

            List<string> missing = new List<string>();
            if (!AssetExists(CharacterPath)) missing.Add(CharacterPath);
            if (!AssetExists(IdlePath)) missing.Add(IdlePath);
            if (walkPath == null) missing.Add($"{WalkPath} o {WalkingPath}");
            if (runPath == null) missing.Add($"{RunPath} o {RunningPath}");

            if (missing.Count > 0)
            {
                Debug.LogError("[ECHO] Faltan archivos obligatorios:\n- " + string.Join("\n- ", missing));
                return;
            }

            if (walkPath == WalkingPath)
            {
                Debug.LogWarning($"[ECHO] No se encontró '{WalkPath}'; se usa '{WalkingPath}'.");
            }

            if (runPath == RunningPath)
            {
                Debug.LogWarning($"[ECHO] No se encontró '{RunPath}'; se usa '{RunningPath}'.");
            }

            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets/Materials", "Player");
            EnsureFolder("Assets", "Animations");
            EnsureFolder("Assets", "Prefabs");

            Avatar avatar;
            if (!ConfigureCharacterImporter(out avatar))
            {
                return;
            }

            int texturesProcessed = ExtractTextures();
            int materialsCreated = ExtractMaterials();
            int materialsConverted = ConvertAllMaterialsToUrp();

            if (!ConfigureAnimationImporter(IdlePath, IdleClipName, avatar)) return;
            if (!ConfigureAnimationImporter(walkPath, WalkClipName, avatar)) return;
            if (!ConfigureAnimationImporter(runPath, RunClipName, avatar)) return;

            AnimationClip idleClip = LoadClip(IdlePath, IdleClipName);
            AnimationClip walkClip = LoadClip(walkPath, WalkClipName);
            AnimationClip runClip = LoadClip(runPath, RunClipName);

            if (idleClip == null || walkClip == null || runClip == null)
            {
                Debug.LogError("[ECHO] No se pudieron cargar los clips Idle, Walk o Run después de reimportar.");
                return;
            }

            AnimatorController controller = EnsureController(idleClip, walkClip, runClip);
            if (controller == null)
            {
                return;
            }

            GameObject playerModelPrefab = EnsurePlayerModelPrefab(avatar, controller);
            if (playerModelPrefab == null)
            {
                return;
            }

            bool sceneConfigured = ConfigureScene(playerModelPrefab, out string sceneDetails);

            AssetDatabase.SaveAssets();

            StringBuilder summary = new StringBuilder();
            summary.AppendLine("[ECHO] Setup Player Character completado.");
            summary.AppendLine($"- Character.fbx: Humanoid, Avatar '{avatar.name}', texturas procesadas: " +
                               $"{texturesProcessed}, materiales nuevos: {materialsCreated}, " +
                               $"materiales convertidos a URP: {materialsConverted}.");
            summary.AppendLine($"- Animaciones: Idle.fbx → {IdleClipName}, {Path.GetFileName(walkPath)} → " +
                               $"{WalkClipName}, {Path.GetFileName(runPath)} → {RunClipName}; " +
                               "Loop Time, Loop Pose y Bake Into Pose en Rotation/Y/XZ.");
            summary.AppendLine($"- AC_Player.controller: {SpeedParameter} (float), {GroundedParameter} (bool), " +
                               $"{InteractParameter} (trigger), {VictoryParameter} (trigger); " +
                               $"{LocomotionStateName} por defecto con {IdleClipName} 0, {WalkClipName} 4, {RunClipName} 7.");
            summary.AppendLine("- PlayerModel.prefab: Animator con AC_Player y Avatar, Apply Root Motion desactivado.");
            summary.AppendLine(sceneConfigured
                ? $"- Escena: {sceneDetails}"
                : "- Escena: omitida; no se encontró PlayerController.");

            Debug.Log(summary.ToString(), playerModelPrefab);
        }

        private static string ResolveFirstExisting(params string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (AssetExists(paths[i])) return paths[i];
            }

            return null;
        }

        private static bool AssetExists(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string absolutePath = Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(absolutePath);
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static bool ConfigureCharacterImporter(out Avatar avatar)
        {
            avatar = null;

            ModelImporter importer = AssetImporter.GetAtPath(CharacterPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError($"[ECHO] No se pudo obtener el ModelImporter de '{CharacterPath}'.");
                return false;
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.autoGenerateAvatarMappingIfUnspecified = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            importer.SaveAndReimport();

            avatar = AssetDatabase.LoadAllAssetsAtPath(CharacterPath).OfType<Avatar>().FirstOrDefault();

            if (avatar == null)
            {
                Debug.LogError($"[ECHO] '{CharacterPath}' no generó un Avatar. Revisa el Rig en Unity.");
                return false;
            }

            if (!avatar.isValid || !avatar.isHuman)
            {
                Debug.LogError($"[ECHO] El Avatar '{avatar.name}' no es válido o no es Humanoid. " +
                               "Configura el Rig manualmente y vuelve a ejecutar el menú.");
                return false;
            }

            return true;
        }

        private static int ExtractTextures()
        {
            ModelImporter importer = AssetImporter.GetAtPath(CharacterPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError($"[ECHO] No se pudo obtener el ModelImporter de '{CharacterPath}'.");
                return 0;
            }

            if (AssetDatabase.IsValidFolder(TemporaryTextureFolder))
            {
                AssetDatabase.DeleteAsset(TemporaryTextureFolder);
            }

            EnsureFolder(MaterialFolder, TemporaryTextureFolderName);

            bool extracted;
            try
            {
                extracted = importer.ExtractTextures(TemporaryTextureFolder);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[ECHO] Error al extraer texturas de '{CharacterPath}': {exception.Message}");
                AssetDatabase.DeleteAsset(TemporaryTextureFolder);
                return 0;
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            if (!extracted)
            {
                Debug.LogWarning($"[ECHO] '{CharacterPath}' no contiene texturas embebidas o no se pudieron extraer.");
                AssetDatabase.DeleteAsset(TemporaryTextureFolder);
                return 0;
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { TemporaryTextureFolder });
            int processed = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string temporaryPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                Texture2D temporaryTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(temporaryPath);
                if (temporaryTexture == null) continue;

                AssetImporter.SourceAssetIdentifier identifier =
                    new AssetImporter.SourceAssetIdentifier(temporaryTexture);
                string fileName = Path.GetFileName(temporaryPath);
                string destinationPath = MaterialFolder + "/" + fileName;

                if (AssetDatabase.LoadAssetAtPath<Texture2D>(destinationPath) != null)
                {
                    AssetDatabase.DeleteAsset(temporaryPath);
                }
                else
                {
                    string moveError = AssetDatabase.MoveAsset(temporaryPath, destinationPath);
                    if (!string.IsNullOrEmpty(moveError))
                    {
                        Debug.LogError($"[ECHO] No se pudo mover la textura '{temporaryPath}' a " +
                                       $"'{destinationPath}': {moveError}");
                        AssetDatabase.DeleteAsset(temporaryPath);
                        continue;
                    }
                }

                Texture2D destinationTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(destinationPath);
                if (destinationTexture == null)
                {
                    Debug.LogError($"[ECHO] No se encontró la textura extraída '{destinationPath}'.");
                    continue;
                }

                importer.AddRemap(identifier, destinationTexture);
                processed++;
            }

            AssetDatabase.DeleteAsset(TemporaryTextureFolder);
            AssetDatabase.WriteImportSettingsIfDirty(CharacterPath);
            AssetDatabase.ImportAsset(CharacterPath, ImportAssetOptions.ForceUpdate);
            ConfigureExtractedTextureImport();

            if (processed == 0)
            {
                Debug.LogWarning("[ECHO] No se encontraron texturas extraídas; vuelve a ejecutar el menú si " +
                                 "Unity retrasó la extracción.");
            }

            return processed;
        }

        private static void ConfigureExtractedTextureImport()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { MaterialFolder });

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string fileName = Path.GetFileNameWithoutExtension(path);

                if (fileName.IndexOf("Normal", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || importer.textureType == TextureImporterType.NormalMap)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
        }

        private static int ExtractMaterials()
        {
            ModelImporter importer = AssetImporter.GetAtPath(CharacterPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError($"[ECHO] No se pudo obtener el ModelImporter de '{CharacterPath}'.");
                return 0;
            }

            List<Material> embeddedMaterials = AssetDatabase.LoadAllAssetsAtPath(CharacterPath)
                .OfType<Material>()
                .ToList();

            int created = 0;

            for (int i = 0; i < embeddedMaterials.Count; i++)
            {
                Material source = embeddedMaterials[i];
                if (source == null) continue;

                string sourceName = source.name;
                if (string.IsNullOrEmpty(sourceName))
                {
                    sourceName = "PlayerMaterial";
                }

                string destinationPath = MaterialFolder + "/" + SanitizeFileName(sourceName) + ".mat";
                Material destination = AssetDatabase.LoadAssetAtPath<Material>(destinationPath);

                if (destination == null)
                {
                    string extractError = AssetDatabase.ExtractAsset(source, destinationPath);
                    if (!string.IsNullOrEmpty(extractError))
                    {
                        Debug.LogError($"[ECHO] No se pudo extraer el material '{sourceName}': {extractError}");
                        continue;
                    }

                    destination = AssetDatabase.LoadAssetAtPath<Material>(destinationPath);

                    if (destination == null)
                    {
                        Debug.LogError($"[ECHO] El material extraído '{destinationPath}' no se encontró.");
                        continue;
                    }

                    created++;
                }

                importer.AddRemap(
                    new AssetImporter.SourceAssetIdentifier(typeof(Material), sourceName),
                    destination);
            }

            AssetDatabase.WriteImportSettingsIfDirty(CharacterPath);
            AssetDatabase.ImportAsset(CharacterPath, ImportAssetOptions.ForceUpdate);

            return created;
        }

        private static string SanitizeFileName(string name)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            for (int i = 0; i < invalid.Length; i++)
            {
                name = name.Replace(invalid[i], '_');
            }

            return name;
        }

        private static int ConvertAllMaterialsToUrp()
        {
            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { MaterialFolder });
            int converted = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (ConvertMaterialToUrp(material)) converted++;
            }

            return converted;
        }

        private static bool ConvertMaterialToUrp(Material material)
        {
            if (material == null || material.shader == null) return false;
            if (material.shader.name.StartsWith(UrpShaderPrefix, StringComparison.Ordinal)) return false;

            Shader urpShader = Shader.Find(UrpLitShaderName);
            if (urpShader == null)
            {
                Debug.LogError($"[ECHO] No se encontró el shader '{UrpLitShaderName}'. " +
                               "Comprueba que URP está instalado y activo.");
                return false;
            }

            Texture baseMap = GetTexture(material, "_BaseMap");
            if (baseMap == null) baseMap = GetTexture(material, "_MainTex");
            Texture normalMap = GetTexture(material, "_BumpMap");

            baseMap = ResolveExternalTexture(baseMap);
            normalMap = ResolveExternalTexture(normalMap);

            Color baseColor = material.HasProperty("_BaseColor")
                ? material.GetColor("_BaseColor")
                : GetColor(material, "_Color", Color.white);
            float metallic = GetFloat(material, "_Metallic", 0f);
            float smoothness = material.HasProperty("_Smoothness")
                ? material.GetFloat("_Smoothness")
                : GetFloat(material, "_Glossiness", 0.5f);
            float normalScale = GetFloat(material, "_BumpScale", 1f);

            material.shader = urpShader;

            if (baseMap != null)
            {
                material.SetTexture("_BaseMap", baseMap);
            }

            if (normalMap != null)
            {
                material.SetTexture("_BumpMap", normalMap);
                material.SetFloat("_BumpScale", normalScale);
                material.EnableKeyword("_NORMALMAP");
            }
            else
            {
                material.DisableKeyword("_NORMALMAP");
            }

            material.SetColor("_BaseColor", baseColor);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);

            return true;
        }

        private static Texture GetTexture(Material material, string property)
        {
            return material != null && material.HasProperty(property) ? material.GetTexture(property) : null;
        }

        private static float GetFloat(Material material, string property, float fallback)
        {
            return material != null && material.HasProperty(property) ? material.GetFloat(property) : fallback;
        }

        private static Color GetColor(Material material, string property, Color fallback)
        {
            return material != null && material.HasProperty(property) ? material.GetColor(property) : fallback;
        }

        private static Texture ResolveExternalTexture(Texture texture)
        {
            if (texture == null) return null;

            string path = AssetDatabase.GetAssetPath(texture);
            if (path != CharacterPath) return texture;

            Texture2D extracted = FindTextureByName(texture.name);
            return extracted != null ? (Texture)extracted : texture;
        }

        private static Texture2D FindTextureByName(string name)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { MaterialFolder });

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (Path.GetFileNameWithoutExtension(path) == name)
                {
                    return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
            }

            return null;
        }

        private static bool ConfigureAnimationImporter(string path, string clipName, Avatar avatar)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError($"[ECHO] No se pudo obtener el ModelImporter de '{path}'.");
                return false;
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            importer.sourceAvatar = avatar;
            importer.importAnimation = true;

            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            if (clips == null || clips.Length == 0)
            {
                clips = importer.clipAnimations;
            }

            if (clips == null || clips.Length == 0)
            {
                Debug.LogError($"[ECHO] No se encontraron clips en '{path}'.");
                return false;
            }

            for (int i = 0; i < clips.Length; i++)
            {
                if (i == 0)
                {
                    clips[i].name = clipName;
                }

                clips[i].loopTime = true;
                clips[i].loopPose = true;
                clips[i].lockRootRotation = true;
                clips[i].lockRootHeightY = true;
                clips[i].lockRootPositionXZ = true;
                clips[i].keepOriginalOrientation = true;
                clips[i].keepOriginalPositionY = true;
                clips[i].keepOriginalPositionXZ = true;
            }

            importer.clipAnimations = clips;
            importer.SaveAndReimport();
            return true;
        }

        private static AnimationClip LoadClip(string path, string clipName)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            AnimationClip clip = assets.OfType<AnimationClip>().FirstOrDefault(c => c.name == clipName);

            if (clip == null)
            {
                clip = assets.OfType<AnimationClip>()
                    .FirstOrDefault(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal));

                if (clip != null)
                {
                    Debug.LogWarning($"[ECHO] El clip de '{path}' no se llama '{clipName}'; se usa '{clip.name}'.");
                }
            }

            return clip;
        }

        private static AnimatorController EnsureController(AnimationClip idle, AnimationClip walk, AnimationClip run)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

            if (controller == null)
            {
                if (AssetDatabase.LoadMainAssetAtPath(ControllerPath) != null)
                {
                    Debug.LogError($"[ECHO] '{ControllerPath}' existe pero no es un AnimatorController.");
                    return null;
                }

                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            }

            if (controller == null)
            {
                Debug.LogError($"[ECHO] No se pudo crear '{ControllerPath}'.");
                return null;
            }

            if (controller.layers == null || controller.layers.Length == 0)
            {
                Debug.LogError($"[ECHO] '{ControllerPath}' no tiene capas.");
                return null;
            }

            AnimatorControllerParameter[] existingParameters = controller.parameters;
            for (int i = existingParameters.Length - 1; i >= 0; i--)
            {
                controller.RemoveParameter(existingParameters[i]);
            }

            controller.AddParameter(SpeedParameter, AnimatorControllerParameterType.Float);
            controller.AddParameter(GroundedParameter, AnimatorControllerParameterType.Bool);
            controller.AddParameter(InteractParameter, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(VictoryParameter, AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState state = null;

            for (int i = 0; i < stateMachine.states.Length; i++)
            {
                if (stateMachine.states[i].state.name == LocomotionStateName)
                {
                    state = stateMachine.states[i].state;
                    break;
                }
            }

            BlendTree blendTree = state != null ? state.motion as BlendTree : null;

            if (blendTree == null)
            {
                if (state != null)
                {
                    stateMachine.RemoveState(state);
                }

                state = controller.CreateBlendTreeInController(LocomotionStateName, out blendTree, 0);
            }

            if (state == null || blendTree == null)
            {
                Debug.LogError($"[ECHO] No se pudo crear el Blend Tree '{LocomotionStateName}'.");
                return null;
            }

            while (blendTree.children.Length > 0)
            {
                blendTree.RemoveChild(0);
            }

            blendTree.name = LocomotionStateName;
            blendTree.blendType = BlendTreeType.Simple1D;
            blendTree.blendParameter = SpeedParameter;
            blendTree.useAutomaticThresholds = false;
            blendTree.AddChild(idle, 0f);
            blendTree.AddChild(walk, 4f);
            blendTree.AddChild(run, 7f);

            stateMachine.defaultState = state;
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            return controller;
        }

        private static GameObject EnsurePlayerModelPrefab(Avatar avatar, AnimatorController controller)
        {
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPath);
            if (modelAsset == null)
            {
                Debug.LogError($"[ECHO] No se pudo cargar '{CharacterPath}' como GameObject.");
                return null;
            }

            GameObject instance = UnityEngine.Object.Instantiate(modelAsset);
            if (instance == null)
            {
                Debug.LogError($"[ECHO] No se pudo instanciar '{CharacterPath}'.");
                return null;
            }

            try
            {
                instance.name = PlayerModelName;
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;

                Animator animator = instance.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = instance.GetComponentInChildren<Animator>(true);
                }

                if (animator == null)
                {
                    animator = instance.AddComponent<Animator>();
                }

                if (animator == null)
                {
                    Debug.LogError("[ECHO] No se pudo añadir un Animator al modelo.");
                    return null;
                }

                animator.runtimeAnimatorController = controller;
                animator.avatar = avatar;
                animator.applyRootMotion = false;

                PrefabUtility.SaveAsPrefabAsset(instance, PlayerModelPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }

            GameObject savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerModelPath);
            if (savedPrefab == null)
            {
                Debug.LogError($"[ECHO] No se pudo guardar '{PlayerModelPath}'.");
            }

            return savedPrefab;
        }

        private static bool ConfigureScene(GameObject playerModelPrefab, out string details)
        {
            details = string.Empty;

            PlayerController playerController =
                UnityEngine.Object.FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);

            if (playerController == null)
            {
                Debug.LogError("[ECHO] No se encontró PlayerController en la escena abierta. " +
                               "Abre Assets/Scenes/SantuarioPrincipal.unity y vuelve a ejecutar el menú.");
                return false;
            }

            GameObject player = playerController.gameObject;
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UndoName);

            bool removedVisual = DestroyDirectChildren(player.transform, VisualName);
            bool removedFacingMarker = DestroyDirectChildren(player.transform, FacingMarkerName);
            bool removedPlayerModel = DestroyDirectChildren(player.transform, PlayerModelName);

            GameObject instance = PrefabUtility.InstantiatePrefab(playerModelPrefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError("[ECHO] No se pudo instanciar PlayerModel.prefab en la escena.");
                EditorSceneManager.MarkSceneDirty(player.scene);
                Undo.CollapseUndoOperations(undoGroup);
                return false;
            }

            Undo.RegisterCreatedObjectUndo(instance, UndoName);
            Undo.RegisterFullObjectHierarchyUndo(instance, UndoName);
            Undo.SetTransformParent(instance.transform, player.transform, UndoName);
            instance.name = PlayerModelName;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            Animator animator = instance.GetComponent<Animator>();
            if (animator == null)
            {
                animator = instance.GetComponentInChildren<Animator>(true);
            }

            CharacterController characterController = player.GetComponent<CharacterController>();

            if (animator == null || characterController == null)
            {
                Debug.LogError("[ECHO] No se pudo asignar PlayerAnimator: falta el Animator del modelo " +
                               "o el CharacterController del Player.");
                Undo.DestroyObjectImmediate(instance);
                EditorSceneManager.MarkSceneDirty(player.scene);
                Undo.CollapseUndoOperations(undoGroup);
                return false;
            }

            PlayerAnimator playerAnimator = player.GetComponent<PlayerAnimator>();
            if (playerAnimator == null)
            {
                playerAnimator = Undo.AddComponent<PlayerAnimator>(player);
            }

            if (playerAnimator == null)
            {
                Debug.LogError("[ECHO] No se pudo añadir PlayerAnimator al Player.");
                Undo.DestroyObjectImmediate(instance);
                EditorSceneManager.MarkSceneDirty(player.scene);
                Undo.CollapseUndoOperations(undoGroup);
                return false;
            }

            SerializedObject serialized = new SerializedObject(playerAnimator);
            SerializedProperty animatorProperty = serialized.FindProperty("animator");
            SerializedProperty controllerProperty = serialized.FindProperty("controller");

            if (animatorProperty == null || controllerProperty == null)
            {
                Debug.LogError("[ECHO] PlayerAnimator no tiene las propiedades serializadas " +
                               "'animator' y 'controller'.");
                EditorSceneManager.MarkSceneDirty(player.scene);
                Undo.CollapseUndoOperations(undoGroup);
                return false;
            }

            animatorProperty.objectReferenceValue = animator;
            controllerProperty.objectReferenceValue = characterController;
            serialized.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(player.scene);
            Undo.CollapseUndoOperations(undoGroup);

            Selection.activeGameObject = instance;
            EditorGUIUtility.PingObject(instance);

            details = $"'{PlayerName}': {VisualName} {(removedVisual ? "eliminado" : "no existía")}, " +
                      $"{FacingMarkerName} {(removedFacingMarker ? "eliminado" : "no existía")}, " +
                      $"{PlayerModelName} {(removedPlayerModel ? "recreado" : "creado")} en 0/0/0 escala 1 " +
                      "y PlayerAnimator asignado.";
            return true;
        }

        private static bool DestroyDirectChildren(Transform parent, string childName)
        {
            List<GameObject> matches = new List<GameObject>();

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == childName)
                {
                    matches.Add(child.gameObject);
                }
            }

            for (int i = 0; i < matches.Count; i++)
            {
                Undo.DestroyObjectImmediate(matches[i]);
            }

            return matches.Count > 0;
        }
    }
}
