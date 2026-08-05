#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Novella.Core;
using Novella.UI;
using Novella.Editor;

/// <summary>
/// Novella > Patch Title: Add Reset Button
/// TitleCanvasのButtonRowにデータリセットボタンを追加し、TitleManagerに配線する。
/// 全データ消去は取り消せないため、確認ダイアログも同時に生成して配線する。
/// </summary>
public class TitleResetButtonPatcher
{

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

        var font = NovellaEditorFont.Load();

        int undoGroup = NovellaEditorUndo.Begin();

        // 既存のResetButtonがあれば削除
        var existing = buttonRow.transform.Find("ResetButton");
        if (existing != null) NovellaEditorUndo.Destroy(existing.gameObject);

        // QuitButtonを参照にサイズ取得
        var quitBtn = buttonRow.transform.Find("QuitButton");
        Vector2 btnSize = new Vector2(200f, 60f);
        if (quitBtn != null)
        {
            var rt = quitBtn.GetComponent<RectTransform>();
            if (rt != null) btnSize = rt.sizeDelta;
        }

        // ResetButton作成（子のTextはこのルートごと消えるためUndo登録はルートのみ）
        var resetGO = new GameObject("ResetButton");
        resetGO.transform.SetParent(buttonRow.transform, false);
        NovellaEditorUndo.Created(resetGO);

        var resetRT = resetGO.AddComponent<RectTransform>();
        resetRT.sizeDelta = btnSize;

        var img = resetGO.AddComponent<Image>();
        img.color = new Color(0.6f, 0.15f, 0.15f, 1f); // スタイル未適用時のフォールバック色

        var btn = resetGO.AddComponent<Button>();

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

        // 他のタイトルボタンと同じ見た目にする（Dangerスタイル）
        ApplyDangerStyle(resetGO, quitBtn != null ? quitBtn.gameObject : null);

        // QuitButtonの右隣に配置
        if (quitBtn != null)
            resetGO.transform.SetSiblingIndex(quitBtn.GetSiblingIndex() + 1);

        // 確認ダイアログを生成（TitleSceneにはNovellaManagerが無いためTitleManagerに載せる）
        var canvas = buttonRow.GetComponentInParent<Canvas>();
        ConfirmDialogController confirmDialog = null;
        if (canvas != null)
            confirmDialog = ConfirmDialogBuilder.EnsureExists(canvas.transform, titleManager.gameObject);

        if (confirmDialog == null)
            Debug.LogWarning("[Novella] 確認ダイアログを生成できませんでした。ResetButtonは無効のままになります。");

        // TitleManagerに配線
        var so = new SerializedObject(titleManager);
        var resetProp = so.FindProperty("_resetButton");
        if (resetProp != null) resetProp.objectReferenceValue = btn;
        var dialogProp = so.FindProperty("_resetConfirmDialog");
        if (dialogProp != null) dialogProp.objectReferenceValue = confirmDialog;
        so.ApplyModifiedProperties();

        NovellaEditorUndo.End(undoGroup, "Novella: Patch Title Reset Button");

        EditorUtility.SetDirty(titleManager.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            titleManager.gameObject.scene);

        Debug.Log("[Novella] ResetButton と確認ダイアログを追加しました。Ctrl+S でシーンを保存してください。");
    }

    /// <summary>
    /// QuitButtonと同じNovellaButtonスタイルを適用する。
    /// QuitButtonにスタイルが無ければ DangerButtonStyle アセットを名前で探す。
    /// </summary>
    private static void ApplyDangerStyle(GameObject target, GameObject reference)
    {
        NovellaButtonStyle style = null;

        if (reference != null)
        {
            var refButton = reference.GetComponent<NovellaButton>();
            if (refButton != null) style = refButton.Style;
        }

        if (style == null)
        {
            var guids = AssetDatabase.FindAssets("DangerButtonStyle t:NovellaButtonStyle");
            if (guids.Length > 0)
                style = AssetDatabase.LoadAssetAtPath<NovellaButtonStyle>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        if (style == null)
        {
            Debug.LogWarning("[Novella] DangerButtonStyle が見つかりませんでした。ResetButtonは既定色で表示されます。");
            return;
        }

        var novellaButton = target.GetComponent<NovellaButton>();
        if (novellaButton == null) novellaButton = target.AddComponent<NovellaButton>();
        novellaButton.SetStyle(style);
    }
}
#endif
