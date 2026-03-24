#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Novella.Core;

/// <summary>
/// エディタ上でJSONスクリプトを変更すると自動的にリロードする。
/// プレイモード中のみ動作。
/// </summary>
[InitializeOnLoad]
public class ScriptHotReloader : AssetPostprocessor
{
    static void OnPostprocessAllAssets(
        string[] importedAssets, string[] deletedAssets,
        string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (!EditorApplication.isPlaying) return;

        bool scriptChanged = false;
        foreach (var path in importedAssets)
        {
            if (path.Contains("Resources/Scripts/") && (path.EndsWith(".json") || path.EndsWith(".csv")))
            {
                scriptChanged = true;
                break;
            }
        }

        if (!scriptChanged) return;

        var engine = Object.FindFirstObjectByType<NovellaEngine>();
        if (engine == null) return;

        string currentPath = engine.CurrentScriptPath;
        if (string.IsNullOrEmpty(currentPath)) return;

        int currentIndex = engine.CurrentIndex;

        // リソースキャッシュをクリアして再ロード
        Resources.UnloadUnusedAssets();

        engine.LoadAndPlayFrom(currentPath, currentIndex);
        Debug.Log($"[Novella] Hot reloaded: {currentPath} @ index {currentIndex}");
    }
}
#endif
