#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Novella.UI;

/// <summary>
/// Novella > Build Scene Recollection
/// TitleSceneにシーン回想UIを構築する。
/// </summary>
public class SceneRecollectionBuilder
{
    private const string FontPath = "Assets/font_1_kokugl_1.asset";

    [MenuItem("Novella/Build Scene Recollection")]
    public static void Build()
    {
        var canvas = GameObject.Find("TitleCanvas");
        if (canvas == null) { Debug.LogError("[Novella] TitleCanvas が見つかりません。TitleSceneを開いてください。"); return; }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        // --- RecollectionPanel ---
        var existingPanel = canvas.transform.Find("RecollectionPanel");
        if (existingPanel != null) Object.DestroyImmediate(existingPanel.gameObject);

        var panelGO = new GameObject("RecollectionPanel");
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
        var titleGO = MakeTMP(panelGO, "RecollectionTitle", "Scene Recollection", 42, Color.white, TextAlignmentOptions.Center, font);
        var titleLE = titleGO.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 70;

        // ScrollView
        var scrollGO = new GameObject("RecollectionScrollView");
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
        vlg.spacing = 8;
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
        var closeBtnGO = new GameObject("RecollectionCloseButton");
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

        // --- SceneRecollectionUIController を TitleManager にアタッチ ---
        var titleManager = GameObject.Find("TitleManager");
        if (titleManager != null)
        {
            var recollUI = titleManager.GetComponent<SceneRecollectionUIController>();
            if (recollUI == null)
                recollUI = titleManager.AddComponent<SceneRecollectionUIController>();

            var so = new SerializedObject(recollUI);
            so.FindProperty("_panel").objectReferenceValue = panelGO;
            so.FindProperty("_listContainer").objectReferenceValue = content.transform;
            so.FindProperty("_closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("_font").objectReferenceValue = font;
            so.ApplyModifiedProperties();

            // TitleManager の _recollectionUI を配線
            var tmSO = new SerializedObject(titleManager.GetComponent<Novella.Core.TitleManager>());
            tmSO.FindProperty("_recollectionUI").objectReferenceValue = recollUI;
            tmSO.ApplyModifiedProperties();
        }

        // --- 回想ボタンを TitleCanvas に追加 ---
        var existingBtn = canvas.transform.Find("RecollectionButton");
        if (existingBtn != null) Object.DestroyImmediate(existingBtn.gameObject);

        var galleryBtn = canvas.transform.Find("GalleryButton");
        var quitBtn = canvas.transform.Find("QuitButton");

        var recBtnGO = new GameObject("RecollectionButton");
        recBtnGO.transform.SetParent(canvas.transform, false);

        var rbRect = recBtnGO.AddComponent<RectTransform>();
        // GalleryButtonの下、QuitButtonの上に配置
        if (galleryBtn != null)
        {
            var gbRect = galleryBtn.GetComponent<RectTransform>();
            rbRect.anchorMin = gbRect.anchorMin;
            rbRect.anchorMax = gbRect.anchorMax;
            rbRect.sizeDelta = gbRect.sizeDelta;
            rbRect.anchoredPosition = gbRect.anchoredPosition + new Vector2(0, -70);

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
            rbRect.anchorMin = qbRect.anchorMin;
            rbRect.anchorMax = qbRect.anchorMax;
            rbRect.sizeDelta = qbRect.sizeDelta;
            rbRect.anchoredPosition = qbRect.anchoredPosition + new Vector2(0, 70);
        }
        else
        {
            rbRect.anchorMin = new Vector2(0.5f, 0.5f);
            rbRect.anchorMax = new Vector2(0.5f, 0.5f);
            rbRect.sizeDelta = new Vector2(300, 60);
            rbRect.anchoredPosition = new Vector2(0, -170);
        }

        var rbImg = recBtnGO.AddComponent<Image>();
        rbImg.color = new Color(0.5f, 0.3f, 0.6f, 1f);
        var rbBtn = recBtnGO.AddComponent<Button>();

        var rbTxt = MakeTMP(recBtnGO, "Text", "Scene", 32, Color.white, TextAlignmentOptions.Center, font);
        var rbTxtRect = rbTxt.GetComponent<RectTransform>();
        rbTxtRect.anchorMin = Vector2.zero;
        rbTxtRect.anchorMax = Vector2.one;
        rbTxtRect.sizeDelta = Vector2.zero;

        // TitleManager に配線
        if (titleManager != null)
        {
            var tmSO = new SerializedObject(titleManager.GetComponent<Novella.Core.TitleManager>());
            tmSO.FindProperty("_recollectionButton").objectReferenceValue = rbBtn;
            tmSO.ApplyModifiedProperties();
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Novella] Scene Recollection UI を構築しました。");
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
