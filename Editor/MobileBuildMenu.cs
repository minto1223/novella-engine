#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Novella.Editor
{
    public class MobileBuildMenu
    {
        [MenuItem("Novella/Configure Android Settings")]
        public static void ConfigureAndroid()
        {
            PlayerSettings.companyName = PlayerSettings.companyName;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)33;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            Debug.Log("[Novella] Android settings configured: Landscape, IL2CPP, ARM64, API 24+");
        }

        [MenuItem("Novella/Build Android")]
        public static void BuildAndroid()
        {
            ConfigureAndroid();

            var scenes = new[]
            {
                "Assets/Scenes/TitleScene.unity",
                "Assets/Scenes/SampleScene.unity"
            };

            string outputPath = "Builds/Android/Novella.apk";
            System.IO.Directory.CreateDirectory("Builds/Android");

            var result = BuildPipeline.BuildPlayer(scenes, outputPath, BuildTarget.Android, BuildOptions.None);

            if (result.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                Debug.Log($"[Novella] Android build succeeded: {outputPath} ({result.summary.totalSize / (1024 * 1024)}MB)");
            else
                Debug.LogError($"[Novella] Android build failed: {result.summary.result}");
        }

        [MenuItem("Novella/Configure iOS Settings")]
        public static void ConfigureIOS()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;

            Debug.Log("[Novella] iOS settings configured: Landscape, IL2CPP, iPhone+iPad");
        }

        [MenuItem("Novella/Build iOS")]
        public static void BuildIOS()
        {
            ConfigureIOS();

            var scenes = new[]
            {
                "Assets/Scenes/TitleScene.unity",
                "Assets/Scenes/SampleScene.unity"
            };

            string outputPath = "Builds/iOS";
            System.IO.Directory.CreateDirectory(outputPath);

            var result = BuildPipeline.BuildPlayer(scenes, outputPath, BuildTarget.iOS, BuildOptions.None);

            if (result.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                Debug.Log($"[Novella] iOS Xcode project generated: {outputPath}");
            else
                Debug.LogError($"[Novella] iOS build failed: {result.summary.result}");
        }
    }
}
#endif
