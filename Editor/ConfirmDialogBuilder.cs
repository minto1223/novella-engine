#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Novella.UI;
using Novella.Editor;

/// <summary>
/// Novella > Rebuild Confirm Dialog
/// 汎用のYes/No確認ダイアログをシーンに構築する。
/// </summary>
public class ConfirmDialogBuilder
{

    [MenuItem("Novella/Rebuild Confirm Dialog")]
    public static void Build()
    {
        var canvas = GameObject.Find("NovellaCanvas");
        if (canvas == null) { Debug.LogError("[Novella] NovellaCanvas が見つかりません。"); return; }

        int undoGroup = NovellaEditorUndo.Begin();

        EnsureExists(canvas.transform);

        NovellaEditorUndo.End(undoGroup, "Novella: Rebuild Confirm Dialog");

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Novella] Confirm Dialog を再構築しました。");
    }

    /// <summary>
    /// ConfirmDialogがまだ無ければ生成・配線する。既にあれば既存のControllerを返す（冪等）。
    /// </summary>
    public static ConfirmDialogController EnsureExists(Transform canvasTransform)
    {
        var novellaManager = GameObject.Find("NovellaManager");
        if (novellaManager == null)
        {
            Debug.LogError("[Novella] NovellaManager が見つかりません。");
            return null;
        }
        return EnsureExists(canvasTransform, novellaManager);
    }

    /// <summary>
    /// ConfirmDialogController を載せるGameObjectを明示して生成・配線する。
    /// NovellaManagerが存在しないシーン（タイトル画面など）から使う。
    /// </summary>
    public static ConfirmDialogController EnsureExists(Transform canvasTransform, GameObject host)
    {
        if (canvasTransform == null || host == null)
        {
            Debug.LogError("[Novella] ConfirmDialog の生成先が指定されていません。");
            return null;
        }
        var novellaManager = host;

        var existingPanel = canvasTransform.Find("ConfirmDialog");
        if (existingPanel != null)
        {
            var existingController = novellaManager.GetComponent<ConfirmDialogController>();
            // 参照が外れているものを再利用すると「見た目は在るのに動かない」状態が固定化するため、
            // 配線が生きている場合だけ再利用し、壊れていれば作り直す（Undo後の半壊からの自己修復）
            if (existingController != null && IsWired(existingController))
                return existingController;
        }

        var font = NovellaEditorFont.Load();

        if (existingPanel != null)
            NovellaEditorUndo.Destroy(existingPanel.gameObject);

        // --- 全画面オーバーレイ ---
        // 子はこのルートごと消えるため、Undo登録はルート1つで足りる
        var panelGO = new GameObject("ConfirmDialog");
        panelGO.transform.SetParent(canvasTransform, false);
        NovellaEditorUndo.Created(panelGO);
        var panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        var overlayImg = panelGO.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.6f);

        // --- 中央カード ---
        var cardGO = new GameObject("DialogCard");
        cardGO.transform.SetParent(panelGO.transform, false);
        var cardRect = cardGO.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(560, 260);
        var cardImg = cardGO.AddComponent<Image>();
        cardImg.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);
        var cardVLG = cardGO.AddComponent<VerticalLayoutGroup>();
        cardVLG.padding = new RectOffset(30, 30, 30, 30);
        cardVLG.spacing = 20;
        cardVLG.childControlWidth = true;
        cardVLG.childControlHeight = true;
        cardVLG.childForceExpandWidth = true;
        cardVLG.childForceExpandHeight = false;

        // --- メッセージ ---
        var msgGO = MakeTMP(cardGO, "MessageText", "確認してください", 30, Color.white, TextAlignmentOptions.Center, font);
        var msgLE = msgGO.AddComponent<LayoutElement>();
        msgLE.preferredHeight = 100;
        msgLE.flexibleHeight = 1;

        // --- ボタン行 ---
        var btnRow = new GameObject("ButtonRow");
        btnRow.transform.SetParent(cardGO.transform, false);
        var btnRowLE = btnRow.AddComponent<LayoutElement>();
        btnRowLE.preferredHeight = 60;
        var btnRowHLG = btnRow.AddComponent<HorizontalLayoutGroup>();
        btnRowHLG.spacing = 20;
        btnRowHLG.childControlWidth = true;
        btnRowHLG.childControlHeight = true;
        btnRowHLG.childForceExpandWidth = true;
        btnRowHLG.childForceExpandHeight = true;

        var yesBtn = MakeButton(btnRow, "YesButton", "はい", new Color(0.8f, 0.2f, 0.2f, 1f), font);
        var noBtn = MakeButton(btnRow, "NoButton", "いいえ", new Color(0.3f, 0.3f, 0.35f, 1f), font);

        panelGO.SetActive(false);

        // --- コンポーネント配線 ---
        // 載せ先は既存GameObject（NovellaManager等）なのでUndo経由で追加する
        var controller = NovellaEditorUndo.EnsureComponent<ConfirmDialogController>(novellaManager);

        var so = new SerializedObject(controller);
        so.FindProperty("_panel").objectReferenceValue = panelGO;
        so.FindProperty("_messageLabel").objectReferenceValue = msgGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_yesButton").objectReferenceValue = yesBtn.GetComponent<Button>();
        so.FindProperty("_noButton").objectReferenceValue = noBtn.GetComponent<Button>();
        so.FindProperty("_yesButtonLabel").objectReferenceValue = yesBtn.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        so.FindProperty("_noButtonLabel").objectReferenceValue = noBtn.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        so.FindProperty("_panelImage").objectReferenceValue = cardImg;
        so.FindProperty("_yesButtonImage").objectReferenceValue = yesBtn.GetComponent<Image>();
        so.FindProperty("_noButtonImage").objectReferenceValue = noBtn.GetComponent<Image>();
        so.ApplyModifiedProperties();

        return controller;
    }

    /// <summary>必須の参照が生きているか。1つでも欠けていれば作り直す判断に使う。</summary>
    private static bool IsWired(ConfirmDialogController controller)
    {
        var so = new SerializedObject(controller);
        string[] required = { "_panel", "_messageLabel", "_yesButton", "_noButton" };
        foreach (var name in required)
        {
            var prop = so.FindProperty(name);
            if (prop == null || prop.objectReferenceValue == null) return false;
        }
        return true;
    }

    // =========================================================
    // ボタン
    // =========================================================
    private static GameObject MakeButton(GameObject parent, string name, string label, Color color, TMP_FontAsset font)
    {
        var btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent.transform, false);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = color;
        btnGO.AddComponent<Button>();

        var txtGO = MakeTMP(btnGO, "Text", label, 28, Color.white, TextAlignmentOptions.Center, font);
        var txtRect = txtGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;

        return btnGO;
    }

    // =========================================================
    // ユーティリティ
    // =========================================================
    private static GameObject MakeTMP(
        GameObject parent, string name, string text,
        float fontSize, Color color, TextAlignmentOptions align,
        TMP_FontAsset font = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.isOrthographic = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
        if (font != null) tmp.font = font;
        return go;
    }
}
#endif
