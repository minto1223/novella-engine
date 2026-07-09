#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Novella.Core;

/// <summary>
/// Novella > Patch Title: Add Reset Button
/// TitleCanvasのButtonRowにデータリセットボタンを追加し、TitleManagerに配線する。
/// </summary>
public class TitleResetButtonPatcher
{
    private const string FontPath = "Assets/font_1_kokugl_1.asset";

    [MenuItem("Novella/Patch Title: Add Reset Button")]
    public static void Patch()
    {
        var buttonRow = GameObject.Find("TitleCanvas/ButtonRow");
        if (buttonRow == null)
        {
            Debug.LogError("[Novella] TitleCanvas/ButtonRow が見つかりません。TitleSceneを開いてください。");
            return;
        }

        var titleManager = Object.FindFirstObjectByType<TitleManager>();
        if (titleManager == null)
        {
            Debug.LogError("[Novella] TitleManager が見つかりません。");
            return;
        }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        // 既存のResetButtonがあれば削除
        var existing = buttonRow.transform.Find("ResetButton");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        // QuitButtonを参照にサイズ取得
        var quitBtn = buttonRow.transform.Find("QuitButton");
        Vector2 btnSize = new Vector2(200f, 60f);
        if (quitBtn != null)
        {
            var rt = quitBtn.GetComponent<RectTransform>();
            if (rt != null) btnSize = rt.sizeDelta;
        }

        // ResetButton作成
        var resetGO = new GameObject("ResetButton");
        resetGO.transform.SetParent(buttonRow.transform, false);

        var resetRT = resetGO.AddComponent<RectTransform>();
        resetRT.sizeDelta = btnSize;

        var img = resetGO.AddComponent<Image>();
        img.color = new Color(0.6f, 0.15f, 0.15f, 1f); // 赤系

        var btn = resetGO.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        btn.colors = colors;

        // テキスト
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(resetGO.transform, false);
        var textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "RESET";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 36;
        tmp.color = Color.white;
        if (font != null) tmp.font = font;

        // QuitButtonの右隣に配置（最後の子にする前にQuitを最後にする）
        if (quitBtn != null)
            resetGO.transform.SetSiblingIndex(quitBtn.GetSiblingIndex() + 1);

        // TitleManagerの_resetButtonに配線
        var so = new SerializedObject(titleManager);
        var resetProp = so.FindProperty("_resetButton");
        if (resetProp != null)
        {
            resetProp.objectReferenceValue = btn;
            so.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(titleManager.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            titleManager.gameObject.scene);

        Debug.Log("[Novella] ResetButton を追加しました。Ctrl+S でシーンを保存してください。");
    }
}
#endif
