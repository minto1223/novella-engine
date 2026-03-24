#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Novella.UI;

public class EndingListBuilder
{
    private const string FontPath = "Assets/font_1_kokugl_1.asset";

    [MenuItem("Novella/Build Ending List")]
    public static void Build()
    {
        var canvas = GameObject.Find("TitleCanvas");
        if (canvas == null) { Debug.LogError("[Novella] TitleCanvas not found. Open TitleScene."); return; }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        // --- EndingListPanel ---
        var existing = canvas.transform.Find("EndingListPanel");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var panelGO = new GameObject("EndingListPanel");
        panelGO.transform.SetParent(canvas.transform, false);
        var panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.9f);

        var panelVLG = panelGO.AddComponent<VerticalLayoutGroup>();
        panelVLG.padding = new RectOffset(40, 40, 30, 30);
        panelVLG.spacing = 12;
        panelVLG.childControlWidth = true;
        panelVLG.childControlHeight = true;
        panelVLG.childForceExpandWidth = true;
        panelVLG.childForceExpandHeight = false;

        // Title
        var titleGO = MakeTMP(panelGO, "EndingListTitle", "Endings", 42, Color.white, TextAlignmentOptions.Center, font);
        var titleLE = titleGO.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 70;

        // ScrollView
        var scrollGO = new GameObject("EndingScrollView");
        scrollGO.transform.SetParent(panelGO.transform, false);
        var scrollLE = scrollGO.AddComponent<LayoutElement>();
        scrollLE.flexibleHeight = 1;

        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 40;

        // Viewport
        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollGO.transform, false);
        var vpRect = viewport.AddComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.sizeDelta = Vector2.zero;
        viewport.AddComponent<Image>().color = Color.clear;
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        // Content
        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0, 0);

        var contentVLG = content.AddComponent<VerticalLayoutGroup>();
        contentVLG.spacing = 8;
        contentVLG.padding = new RectOffset(60, 60, 10, 10);
        contentVLG.childControlWidth = true;
        contentVLG.childControlHeight = true;
        contentVLG.childForceExpandWidth = true;
        contentVLG.childForceExpandHeight = false;

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = vpRect;
        scrollRect.content = contentRect;

        // Close Button
        var closeBtnGO = new GameObject("CloseButton");
        closeBtnGO.transform.SetParent(panelGO.transform, false);
        var closeBtnLE = closeBtnGO.AddComponent<LayoutElement>();
        closeBtnLE.preferredHeight = 60;

        var closeBtnImg = closeBtnGO.AddComponent<Image>();
        closeBtnImg.color = new Color(0.4f, 0.2f, 0.2f);
        var closeBtn = closeBtnGO.AddComponent<Button>();

        var closeTxtGO = MakeTMP(closeBtnGO, "Text", "Close", 28, Color.white, TextAlignmentOptions.Center, font);
        var closeTxtRect = closeTxtGO.GetComponent<RectTransform>();
        closeTxtRect.anchorMin = Vector2.zero;
        closeTxtRect.anchorMax = Vector2.one;
        closeTxtRect.sizeDelta = Vector2.zero;

        panelGO.SetActive(false);

        // --- EndingListUIController ---
        var titleManager = GameObject.Find("TitleManager");
        if (titleManager == null) titleManager = GameObject.Find("TitleCanvas");

        EndingListUIController controller = titleManager.GetComponent<EndingListUIController>();
        if (controller == null)
            controller = titleManager.AddComponent<EndingListUIController>();

        var so = new SerializedObject(controller);
        so.FindProperty("_panel").objectReferenceValue = panelGO;
        so.FindProperty("_listContainer").objectReferenceValue = content.transform;
        so.FindProperty("_closeButton").objectReferenceValue = closeBtn;
        if (font != null)
            so.FindProperty("_font").objectReferenceValue = font;
        so.ApplyModifiedProperties();

        // --- TitleManager に配線 ---
        var tm = titleManager.GetComponent<Novella.Core.TitleManager>();
        if (tm != null)
        {
            var tmSO = new SerializedObject(tm);
            tmSO.FindProperty("_endingListUI").objectReferenceValue = controller;

            // Endings ボタンを追加
            var btnContainer = canvas.transform.Find("ButtonContainer") ?? canvas.transform;
            var endingBtnGO = new GameObject("EndingButton");
            endingBtnGO.transform.SetParent(btnContainer, false);
            var endingBtnRect = endingBtnGO.AddComponent<RectTransform>();
            endingBtnRect.sizeDelta = new Vector2(300, 60);

            var endingBtnImg = endingBtnGO.AddComponent<Image>();
            endingBtnImg.color = new Color(0.5f, 0.35f, 0.1f);
            var endingBtn = endingBtnGO.AddComponent<Button>();

            var endingTxtGO = MakeTMP(endingBtnGO, "Text", "Endings", 30, Color.white, TextAlignmentOptions.Center, font);
            var endingTxtRect = endingTxtGO.GetComponent<RectTransform>();
            endingTxtRect.anchorMin = Vector2.zero;
            endingTxtRect.anchorMax = Vector2.one;
            endingTxtRect.sizeDelta = Vector2.zero;

            tmSO.FindProperty("_endingListButton").objectReferenceValue = endingBtn;
            tmSO.ApplyModifiedProperties();
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Novella] Ending List built successfully.");
    }

    private static GameObject MakeTMP(GameObject parent, string name, string text, int fontSize,
        Color color, TextAlignmentOptions alignment, TMP_FontAsset font)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        if (font != null) tmp.font = font;
        return go;
    }
}
#endif
