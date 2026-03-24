#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Novella.Core;

/// <summary>
/// Novella > Create Chapter List
/// Resources/Scripts/ 内のチャプタースクリプトを検索し、ChapterList ScriptableObjectを自動生成する。
/// </summary>
public class ChapterListCreator
{
    [MenuItem("Novella/Create Chapter List")]
    public static void Create()
    {
        // 既存のChapterListがあれば上書き確認
        string assetPath = "Assets/Novella/Resources/ChapterList.asset";
        var existing = AssetDatabase.LoadAssetAtPath<ChapterList>(assetPath);
        if (existing != null)
            AssetDatabase.DeleteAsset(assetPath);

        // フォルダ確保
        if (!AssetDatabase.IsValidFolder("Assets/Novella/Resources"))
            AssetDatabase.CreateFolder("Assets/Novella", "Resources");

        var chapterList = ScriptableObject.CreateInstance<ChapterList>();

        // chapter*.json を検索してソート
        var guids = AssetDatabase.FindAssets("chapter t:TextAsset", new[] { "Assets/Novella/Resources/Scripts" });
        var entries = new System.Collections.Generic.List<ChapterEntry>();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

            // chapter01, chapter02 等のみ（chapter01_csv等は除外）
            if (!System.Text.RegularExpressions.Regex.IsMatch(fileName, @"^chapter\d+$"))
                continue;

            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset == null) continue;

            // JSONからタイトルを取得
            string title = fileName;
            try
            {
                var script = ScriptParser.Parse(asset.text);
                if (script != null && !string.IsNullOrEmpty(script.Title))
                    title = script.Title;
            }
            catch { }

            entries.Add(new ChapterEntry
            {
                Title = title,
                ScriptPath = "Scripts/" + fileName,
                ScriptAsset = asset,
            });
        }

        // ファイル名でソート
        entries.Sort((a, b) => string.Compare(a.ScriptPath, b.ScriptPath, System.StringComparison.Ordinal));
        chapterList.Chapters = entries.ToArray();

        AssetDatabase.CreateAsset(chapterList, assetPath);
        AssetDatabase.SaveAssets();

        // ChapterSelectUIControllerに自動配線
        var titleManager = GameObject.Find("TitleManager");
        if (titleManager != null)
        {
            var csUI = titleManager.GetComponent<Novella.UI.ChapterSelectUIController>();
            if (csUI != null)
            {
                var so = new SerializedObject(csUI);
                so.FindProperty("_chapterList").objectReferenceValue = chapterList;
                so.ApplyModifiedProperties();
            }
        }

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = chapterList;

        Debug.Log($"[Novella] ChapterList を作成しました（{entries.Count}チャプター）: {assetPath}");
    }
}
#endif
