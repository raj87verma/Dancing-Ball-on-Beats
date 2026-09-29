using UnityEditor;
using UnityEngine;

namespace DancingBallOnBeats.EditorTools
{
    /// <summary>
    /// Command-line build entry point used by CI (GitHub Actions). Not used at runtime -
    /// this lives under an "Editor" folder so Unity excludes it from player builds.
    /// Invoked via: Unity -batchmode -quit -buildTarget Android
    ///              -executeMethod DancingBallOnBeats.EditorTools.BuildScript.PerformAndroidBuild
    /// </summary>
    public static class BuildScript
    {
        public static void PerformAndroidBuild()
        {
            string outputPath = "Builds/Android/DancingBallOnBeats.apk";

            var scenes = new System.Collections.Generic.List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled) scenes.Add(scene.path);
            }

            if (scenes.Count == 0)
            {
                Debug.LogError("[BuildScript] No enabled scenes found in Build Settings - aborting build.");
                EditorApplication.Exit(1);
                return;
            }

            // Ensure this is a plain unsigned/debug-keystore build - fine for sideloading and
            // testing on a personal device. A real release build (for Play Store) needs a
            // proper upload keystore configured separately.
            PlayerSettings.Android.useCustomKeystore = false;

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log($"[BuildScript] Building Android APK with {scenes.Count} scene(s) to '{outputPath}'...");

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.LogError($"[BuildScript] Build FAILED: {report.summary.result} - " +
                                $"{report.summary.totalErrors} error(s).");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"[BuildScript] Build SUCCEEDED: {report.summary.totalSize} bytes, " +
                       $"output at '{outputPath}'.");
        }
    }
}
