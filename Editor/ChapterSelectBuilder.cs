#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Novella.UI;

/// <summary>
/// Novella > Build Chapter Select
/// TitleSceneにチャプター選択UIを構築する。
/// </summary>
public class ChapterSelectBuilder
{
    private const string FontPath = "Assets/font_1_kokugl_1.asset";

    [MenuItem("Novella/Build Chapter Select")]
    public static void Build()
    {
        var canvas = GameObject.Find("TitleCanvas");
        if (canvas == null) { Debug.LogError("[Novella] TitleCanvas が見つかりません。TitleSceneを開いてください。"); return; }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        // --- ChapterSelectPanel ---
        var existingPanel = canvas.transform.Find("ChapterSelectPanel");
        if (existingPanel != null) Object.DestroyImmediate(existingPanel.gameObject);

        var panelGO = new GameObject("ChapterSelectPanel");
        panelGO.transform.SetParent(canvas.transform, false);
        var panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.92f);

        var panelVLG = panelGO.AddComponent<VerticalLayoutGroup>();
        panelVLG.padding = new RectOffset(40, 40, 30, 30);
        panelVLG.spacing = 12;
        panelVLG.childControlWidth = true;
        panelVLG.childControlHeight = true;
        panelVLG.childForceExpandWidth = true;
        panelVLG.childForceExpandHeight = false;

        // Title
        var titleGO = MakeTMP(panelGO, "ChapterSelectTitle", "Chapter Select", 42, Color.white, TextAlignmentOptions.Center, font);
        var titleLE = titleGO.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 70;

        // ScrollView
        var scrollGO = new GameObject("ChapterSelectScrollView");
        scrollGO.transform.SetParent(panelGO.transform, false);
        var scrollLE = scrollGO.AddComponent<LayoutElement>();
        scrollLE.flexibleHeight = 1;

        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 40;

        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollGO.transform, false);
        var vpRect = viewport.AddComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.sizeDelta = Vector2.zero;
        vpRect.pivot = new Vector2(0, 1);
        viewport.AddComponent<RectMask2D>();

        var content = new GameObject("ListContainer");
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = Vector2.zero;

        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.padding = new RectOffset(20, 20, 10, 10);
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = vpRect;
        scrollRect.content = contentRect;

        // Close Button
        var closeBtnGO = new GameObject("ChapterSelectCloseButton");
        closeBtnGO.transform.SetParent(panelGO.transform, false);
        var closeBtnLE = closeBtnGO.AddComponent<LayoutElement>();
        closeBtnLE.preferredHeight = 50;
        var closeBtnImg = closeBtnGO.AddComponent<Image>();
        closeBtnImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        var closeBtn = closeBtnGO.AddComponent<Button>();

        var closeTxt = MakeTMP(closeBtnGO, "Text", "Close", 28, Color.white, TextAlignmentOptions.Center, font);
        var closeTxtRect = closeTxt.GetComponent<RectTransform>();
        closeTxtRect.anchorMin = Vector2.zero;
        closeTxtRect.anchorMax = Vector2.one;
        closeTxtRect.sizeDelta = Vector2.zero;

        panelGO.SetActive(false);

        // --- ChapterSelectUIController を TitleManager にアタッチ ---
        var titleManager = GameObject.Find("TitleManager");
        if (titleManager != null)
        {
            var chapterUI = titleManager.GetComponent<ChapterSelectUIController>();
            if (chapterUI == null)
                chapterUI = titleManager.AddComponent<ChapterSelectUIController>();

            var so = new SerializedObject(chapterUI);
            so.FindProperty("_panel").objectReferenceValue = panelGO;
            so.FindProperty("_listContainer").objectReferenceValue = content.transform;
            so.FindProperty("_closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("_font").objectReferenceValue = font;
            so.ApplyModifiedProperties();

            // TitleManager の _chapterSelectUI を配線
            var tmSO = new SerializedObject(titleManager.GetComponent<Novella.Core.TitleManager>());
            tmSO.FindProperty("_chapterSelectUI").objectReferenceValue = chapterUI;
            tmSO.ApplyModifiedProperties();
        }

        // --- チャプター選択ボタンを TitleCanvas に追加 ---
        var existingBtn = canvas.transform.Find("ChapterSelectButton");
        if (existingBtn != null) Object.DestroyImmediate(existingBtn.gameObject);

        var recollectionBtn = canvas.transform.Find("RecollectionButton");
        var quitBtn = canvas.transform.Find("QuitButton");

        var csBtnGO = new GameObject("ChapterSelectButton");
        csBtnGO.transform.SetParent(canvas.transform, false);

        var csBtnRect = csBtnGO.AddComponent<RectTransform>();
        if (recollectionBtn != null)
        {
            var rbRect = recollectionBtn.GetComponent<RectTransform>();
            csBtnRect.anchorMin = rbRect.anchorMin;
            csBtnRect.anchorMax = rbRect.anchorMax;
            csBtnRect.sizeDelta = rbRect.sizeDelta;
            csBtnRect.anchoredPosition = rbRect.anchoredPosition + new Vector2(0, -70);

            // QuitButtonをさらに下にずらす
            if (quitBtn != null)
            {
                var qbRect = quitBtn.GetComponent<RectTransform>();
                qbRect.anchoredPosition += new Vector2(0, -70);
            }
        }
        else if (quitBtn != null)
        {
            var qbRect = quitBtn.GetComponent<RectTransform>();
            csBtnRect.anchorMin = qbRect.anchorMin;
            csBtnRect.anchorMax = qbRect.anchorMax;
            csBtnRect.sizeDelta = qbRect.sizeDelta;
            csBtnRect.anchoredPosition = qbRect.anchoredPosition + new Vector2(0, 70);
        }
        else
        {
            csBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
            csBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
            csBtnRect.sizeDelta = new Vector2(300, 60);
            csBtnRect.anchoredPosition = new Vector2(0, -240);
        }

        var csBtnImg = csBtnGO.AddComponent<Image>();
        csBtnImg.color = new Color(0.2f, 0.4f, 0.6f, 1f);
        var csBtn = csBtnGO.AddComponent<Button>();

        var csBtnTxt = MakeTMP(csBtnGO, "Text", "Chapter", 32, Color.white, TextAlignmentOptions.Center, font);
        var csBtnTxtRect = csBtnTxt.GetComponent<RectTransform>();
        csBtnTxtRect.anchorMin = Vector2.zero;
        csBtnTxtRect.anchorMax = Vector2.one;
        csBtnTxtRect.sizeDelta = Vector2.zero;

        // TitleManager に配線
        if (titleManager != null)
        {
            var tmSO = new SerializedObject(titleManager.GetComponent<Novella.Core.TitleManager>());
            tmSO.FindProperty("_chapterSelectButton").objectReferenceValue = csBtn;
            tmSO.ApplyModifiedProperties();
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Novella] Chapter Select UI を構築しました。ChapterList ScriptableObjectを作成して ChapterSelectUIController に設定してください。");
    }

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
        tmp.overflowMode = TextOverflowModes.Overflow;
        if (font != null) tmp.font = font;
        return go;
    }
}
#endif
