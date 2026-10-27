using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Echo.EditorTools
{
    public static class BuildWindows
    {
        private const string ScenePath = "Assets/Scenes/SantuarioPrincipal.unity";
        private const string OutputFolderName = "Builds";
        private const string ExecutableName = "ECHO_SantuarioDelTiempo.exe";
        private const string MenuPath = "Tools/ECHO/Build Windows";

        [MenuItem(MenuPath, false, 30)]
        public static void Build()
        {
            if (!ValidateEditorState())
            {
                return;
            }

            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (scene == null)
            {
                Fail($"No se encontró la escena '{ScenePath}'.");
                return;
            }

            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
            {
                Fail("Windows x64 Build Support no está instalado. " +
                     "Añádelo desde Unity Hub > Installs > Add modules.");
                return;
            }

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrEmpty(projectRoot))
            {
                Fail("No se pudo determinar la raíz del proyecto.");
                return;
            }

            string outputDirectory = Path.Combine(projectRoot, OutputFolderName);
            string executablePath = Path.Combine(outputDirectory, ExecutableName);

            try
            {
                Directory.CreateDirectory(outputDirectory);
            }
            catch (Exception exception)
            {
                Fail($"No se pudo crear la carpeta de salida '{outputDirectory}': {exception.Message}");
                return;
            }

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = executablePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report;
            try
            {
                report = BuildPipeline.BuildPlayer(options);
            }
            catch (Exception exception)
            {
                Fail($"El build lanzó una excepción: {exception.Message}");
                return;
            }

            if (report == null)
            {
                Fail("BuildPipeline no devolvió un reporte de build.");
                return;
            }

            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                Fail($"El build falló con resultado '{summary.result}'. " +
                     $"Errores: {summary.totalErrors}. Revisa la consola o el log de Unity.");
                return;
            }

            Debug.Log(
                "[ECHO] Build Windows x64 completado.\n" +
                $"  Escena: {ScenePath}\n" +
                $"  Ejecutable: {summary.outputPath}\n" +
                $"  Tamaño: {FormatSize(summary.totalSize)}\n" +
                $"  Errores: {summary.totalErrors}\n" +
                $"  Advertencias: {summary.totalWarnings}\n" +
                $"  Duración: {summary.totalTime.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture)} s");

            ExitIfBatchMode(0);
        }

        private static bool ValidateEditorState()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Fail("No se puede compilar mientras el Editor está en Play Mode.");
                return false;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                Fail("El Editor está compilando o importando assets. Espera a que termine.");
                return false;
            }

            if (EditorSceneManager.GetActiveScene().path == ScenePath &&
                EditorSceneManager.GetActiveScene().isDirty)
            {
                Debug.LogWarning(
                    $"[ECHO] La escena '{ScenePath}' tiene cambios sin guardar. " +
                    "El build usará la última versión guardada. Guarda la escena antes de compilar.");
            }

            return true;
        }

        private static void Fail(string message)
        {
            Debug.LogError($"[ECHO] {message}");
            ExitIfBatchMode(1);
        }

        private static void ExitIfBatchMode(int exitCode)
        {
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
        }

        private static string FormatSize(ulong bytes)
        {
            const double BytesPerMegabyte = 1024d * 1024d;
            return (bytes / BytesPerMegabyte).ToString("0.0", CultureInfo.InvariantCulture) + " MB";
        }
    }
}
