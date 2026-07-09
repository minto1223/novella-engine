#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Novella.UI;

/// <summary>
/// Novella > Rebuild Confirm Dialog
/// 汎用のYes/No確認ダイアログをシーンに構築する。
/// </summary>
public class ConfirmDialogBuilder
{
    private const string FontPath = "Assets/font_1_kokugl_1.asset";

    [MenuItem("Novella/Rebuild Confirm Dialog")]
    public static void Build()
    {
        var canvas = GameObject.Find("NovellaCanvas");
        if (canvas == null) { Debug.LogError("[Novella] NovellaCanvas が見つかりません。"); return; }

        EnsureExists(canvas.transform);

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

        var existingPanel = canvasTransform.Find("ConfirmDialog");
        if (existingPanel != null)
        {
            var existingController = novellaManager.GetComponent<ConfirmDialogController>();
            if (existingController != null)
                return existingController;
        }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        if (existingPanel != null)
            Object.DestroyImmediate(existingPanel.gameObject);

        // --- 全画面オーバーレイ ---
        var panelGO = new GameObject("ConfirmDialog");
        panelGO.transform.SetParent(canvasTransform, false);
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
        var controller = novellaManager.GetComponent<ConfirmDialogController>();
        if (controller == null)
            controller = novellaManager.AddComponent<ConfirmDialogController>();

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
