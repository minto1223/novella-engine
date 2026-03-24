#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Novella.UI;

/// <summary>
/// Novella > Build CG Gallery
/// TitleSceneにCGギャラリーUIを構築する。
/// </summary>
public class CGGalleryBuilder
{
    private const string FontPath = "Assets/font_1_kokugl_1.asset";

    [MenuItem("Novella/Build CG Gallery")]
    public static void Build()
    {
        var canvas = GameObject.Find("TitleCanvas");
        if (canvas == null) { Debug.LogError("[Novella] TitleCanvas が見つかりません。TitleSceneを開いてください。"); return; }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        // --- GalleryPanel ---
        var existingPanel = canvas.transform.Find("GalleryPanel");
        if (existingPanel != null) Object.DestroyImmediate(existingPanel.gameObject);

        var panelGO = new GameObject("GalleryPanel");
        panelGO.transform.SetParent(canvas.transform, false);
        var panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // 半透明背景
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.9f);

        var panelVLG = panelGO.AddComponent<VerticalLayoutGroup>();
        panelVLG.padding = new RectOffset(20, 20, 20, 20);
        panelVLG.spacing = 10;
        panelVLG.childControlWidth = true;
        panelVLG.childControlHeight = true;
        panelVLG.childForceExpandWidth = true;
        panelVLG.childForceExpandHeight = false;

        // タイトル
        var titleGO = MakeTMP(panelGO, "GalleryTitle", "CG ギャラリー", 42, Color.white, TextAlignmentOptions.Center, font);
        var titleLE = titleGO.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 70;

        // ScrollView
        var scrollGO = new GameObject("GalleryScrollView");
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

        var content = new GameObject("GridContainer");
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = Vector2.zero;

        var gridLayout = content.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(240, 135);
        gridLayout.spacing = new Vector2(15, 15);
        gridLayout.padding = new RectOffset(20, 20, 10, 10);
        gridLayout.constraint = GridLayoutGroup.Constraint.Flexible;

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = vpRect;
        scrollRect.content = contentRect;

        // 閉じるボタン
        var closeBtnGO = new GameObject("GalleryCloseButton");
        closeBtnGO.transform.SetParent(panelGO.transform, false);
        var closeBtnLE = closeBtnGO.AddComponent<LayoutElement>();
        closeBtnLE.preferredHeight = 50;
        var closeBtnImg = closeBtnGO.AddComponent<Image>();
        closeBtnImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        var closeBtn = closeBtnGO.AddComponent<Button>();

        var closeTxt = MakeTMP(closeBtnGO, "Text", "閉じる", 28, Color.white, TextAlignmentOptions.Center, font);
        var closeTxtRect = closeTxt.GetComponent<RectTransform>();
        closeTxtRect.anchorMin = Vector2.zero;
        closeTxtRect.anchorMax = Vector2.one;
        closeTxtRect.sizeDelta = Vector2.zero;

        // --- FullViewPanel (拡大表示用) ---
        var fullViewGO = new GameObject("FullViewPanel");
        fullViewGO.transform.SetParent(panelGO.transform.parent, false);
        var fvRect = fullViewGO.AddComponent<RectTransform>();
        fvRect.anchorMin = Vector2.zero;
        fvRect.anchorMax = Vector2.one;
        fvRect.sizeDelta = Vector2.zero;

        var fvImg = fullViewGO.AddComponent<Image>();
        fvImg.color = new Color(0, 0, 0, 0.95f);

        // 画像
        var fvImageGO = new GameObject("FullViewImage");
        fvImageGO.transform.SetParent(fullViewGO.transform, false);
        var fvImageRect = fvImageGO.AddComponent<RectTransform>();
        fvImageRect.anchorMin = new Vector2(0.05f, 0.05f);
        fvImageRect.anchorMax = new Vector2(0.95f, 0.9f);
        fvImageRect.sizeDelta = Vector2.zero;
        var fvImageImg = fvImageGO.AddComponent<Image>();
        fvImageImg.preserveAspect = true;

        // 閉じるボタン
        var fvCloseBtnGO = new GameObject("FullViewCloseButton");
        fvCloseBtnGO.transform.SetParent(fullViewGO.transform, false);
        var fvCloseBtnRect = fvCloseBtnGO.AddComponent<RectTransform>();
        fvCloseBtnRect.anchorMin = new Vector2(0.3f, 0.02f);
        fvCloseBtnRect.anchorMax = new Vector2(0.7f, 0.08f);
        fvCloseBtnRect.sizeDelta = Vector2.zero;
        var fvCloseBtnImg = fvCloseBtnGO.AddComponent<Image>();
        fvCloseBtnImg.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        var fvCloseBtn = fvCloseBtnGO.AddComponent<Button>();

        var fvCloseTxt = MakeTMP(fvCloseBtnGO, "Text", "戻る", 28, Color.white, TextAlignmentOptions.Center, font);
        var fvCloseTxtRect = fvCloseTxt.GetComponent<RectTransform>();
        fvCloseTxtRect.anchorMin = Vector2.zero;
        fvCloseTxtRect.anchorMax = Vector2.one;
        fvCloseTxtRect.sizeDelta = Vector2.zero;

        fullViewGO.SetActive(false);
        panelGO.SetActive(false);

        // --- CGGalleryUIController をTitleManagerにアタッチ ---
        var titleManager = GameObject.Find("TitleManager");
        if (titleManager != null)
        {
            var galleryUI = titleManager.GetComponent<CGGalleryUIController>();
            if (galleryUI == null)
                galleryUI = titleManager.AddComponent<CGGalleryUIController>();

            var so = new SerializedObject(galleryUI);
            so.FindProperty("_panel").objectReferenceValue = panelGO;
            so.FindProperty("_gridContainer").objectReferenceValue = content.transform;
            so.FindProperty("_closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("_fullViewPanel").objectReferenceValue = fullViewGO;
            so.FindProperty("_fullViewImage").objectReferenceValue = fvImageImg;
            so.FindProperty("_fullViewCloseButton").objectReferenceValue = fvCloseBtn;
            so.ApplyModifiedProperties();

            // TitleManager の _galleryUI を配線
            var tmSO = new SerializedObject(titleManager.GetComponent<Novella.Core.TitleManager>());
            tmSO.FindProperty("_galleryUI").objectReferenceValue = galleryUI;
            tmSO.ApplyModifiedProperties();
        }

        // --- ギャラリーボタンをTitleCanvasに追加 ---
        var existingGalleryBtn = canvas.transform.Find("GalleryButton");
        if (existingGalleryBtn == null)
        {
            // QuitButtonの位置を参考に、コンティニューとQuitの間にギャラリーボタンを配置
            var quitBtn = canvas.transform.Find("QuitButton");
            var continueBtn = canvas.transform.Find("ContinueButton");

            var galleryBtnGO = new GameObject("GalleryButton");
            galleryBtnGO.transform.SetParent(canvas.transform, false);

            var gbRect = galleryBtnGO.AddComponent<RectTransform>();
            if (quitBtn != null)
            {
                var quitRect = quitBtn.GetComponent<RectTransform>();
                gbRect.anchorMin = quitRect.anchorMin;
                gbRect.anchorMax = quitRect.anchorMax;
                gbRect.sizeDelta = quitRect.sizeDelta;
                // QuitButtonの上に配置（Y+70）
                gbRect.anchoredPosition = quitRect.anchoredPosition + new Vector2(0, 70);
                // QuitButtonを下にずらす
                quitRect.anchoredPosition += new Vector2(0, -35);
                // ギャラリーボタンも少し下げる
                gbRect.anchoredPosition += new Vector2(0, -35);
            }
            else
            {
                gbRect.anchorMin = new Vector2(0.5f, 0.5f);
                gbRect.anchorMax = new Vector2(0.5f, 0.5f);
                gbRect.sizeDelta = new Vector2(300, 60);
                gbRect.anchoredPosition = new Vector2(0, -100);
            }

            var gbImg = galleryBtnGO.AddComponent<Image>();
            gbImg.color = new Color(0.3f, 0.5f, 0.7f, 1f);
            var gbBtn = galleryBtnGO.AddComponent<Button>();

            var gbTxt = MakeTMP(galleryBtnGO, "Text", "ギャラリー", 32, Color.white, TextAlignmentOptions.Center, font);
            var gbTxtRect = gbTxt.GetComponent<RectTransform>();
            gbTxtRect.anchorMin = Vector2.zero;
            gbTxtRect.anchorMax = Vector2.one;
            gbTxtRect.sizeDelta = Vector2.zero;

            // TitleManager に配線
            if (titleManager != null)
            {
                var tmSO = new SerializedObject(titleManager.GetComponent<Novella.Core.TitleManager>());
                tmSO.FindProperty("_galleryButton").objectReferenceValue = gbBtn;
                tmSO.ApplyModifiedProperties();
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Novella] CG Gallery を構築しました。");
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
