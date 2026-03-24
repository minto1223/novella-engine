using UnityEditor;
using UnityEngine;

namespace Novella.Editor
{
    public static class WebGLBuilder
    {
        [MenuItem("Novella/Configure WebGL Settings")]
        public static void ConfigureWebGL()
        {
            // Player Settings
            PlayerSettings.companyName = "Novella";
            PlayerSettings.productName = "Novella VN Engine";
            PlayerSettings.WebGL.template = "APPLICATION:Default";
            
            // 解像度
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.runInBackground = true;
            
            // WebGL固有設定
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.decompressionFallback = true; // サーバーが圧縮非対応でも動作
            
            // メモリ設定
            PlayerSettings.WebGL.initialMemorySize = 64;
            PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;
            PlayerSettings.WebGL.maximumMemorySize = 512;
            
            // 色空間（URPデフォルトのLinearのまま）
            // PlayerSettings.colorSpace = ColorSpace.Linear;
            
            Debug.Log("[WebGLBuilder] WebGL settings configured.");
        }
        
        [MenuItem("Novella/Build WebGL")]
        public static void BuildWebGL()
        {
            ConfigureWebGL();
            
            string buildPath = "Builds/WebGL";
            
            var scenes = new[]
            {
                "Assets/Scenes/TitleScene.unity",
                "Assets/Scenes/SampleScene.unity"
            };
            
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };
            
            Debug.Log($"[WebGLBuilder] Starting WebGL build to '{buildPath}'...");
            var report = BuildPipeline.BuildPlayer(options);
            
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[WebGLBuilder] Build succeeded! Size: {report.summary.totalSize / (1024 * 1024)}MB, Time: {report.summary.totalTime.TotalSeconds:F1}s");
                Debug.Log($"[WebGLBuilder] Output: {System.IO.Path.GetFullPath(buildPath)}");
            }
            else
            {
                Debug.LogError($"[WebGLBuilder] Build failed: {report.summary.result}");
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
