using UnityEditor;
using UnityEngine;

namespace Novella.Editor
{
    public static class WindowsBuilder
    {
        [MenuItem("Novella/Build Windows")]
        public static void BuildWindows()
        {
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;

            string buildPath = "Builds/Windows/NovellaGame.exe";

            var scenes = new[]
            {
                "Assets/Scenes/TitleScene.unity",
                "Assets/Scenes/SampleScene.unity"
            };

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log($"[WindowsBuilder] Starting Windows build to '{buildPath}'...");
            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[WindowsBuilder] Build succeeded! Size: {report.summary.totalSize / (1024 * 1024)}MB, Time: {report.summary.totalTime.TotalSeconds:F1}s");
                Debug.Log($"[WindowsBuilder] Output: {System.IO.Path.GetFullPath(buildPath)}");
            }
            else
            {
                Debug.LogError($"[WindowsBuilder] Build failed: {report.summary.result}");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error || msg.type == LogType.Warning)
                            Debug.LogError($"  [{step.name}] {msg.content}");
                    }
                }
            }
        }
    }
}
