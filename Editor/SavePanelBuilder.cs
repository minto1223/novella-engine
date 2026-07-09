#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Novella.UI;

/// <summary>
/// Novella > Rebuild Save Panels
/// SaveSlot プレファブ（2列グリッドデザイン）を生成し、
/// SaveCard / LoadCard を GridLayoutGroup 構造に再構築する。
/// </summary>
public class SavePanelBuilder
{
    private const string PrefabPath   = "Assets/Novella/Prefabs/SaveSlot.prefab";
    private const string FontPath     = "Assets/font_1_kokugl_1.asset";

    // SaveCard の VLG padding に合わせたグリッドセルサイズ
    // 1920 - 40(L) - 40(R) = 1840px、2列 spacing16 → (1840-16)/2 = 912
    private const float CellWidth  = 912f;
    private const float CellHeight = 140f;

    [MenuItem("Novella/Rebuild Save Panels")]
    public static void Build()
    {
        var prefab = BuildSaveSlotPrefab();
        if (prefab == null) { Debug.LogError("[Novella] SaveSlot prefab の作成に失敗しました。"); return; }

        var canvas = GameObject.Find("NovellaCanvas");
        if (canvas == null) { Debug.LogError("[Novella] NovellaCanvas が見つかりません。"); return; }

        RebuildCard(canvas, "SavePanel", "SaveCard", "セーブ",  prefab, SavePanelMode.Save);
        RebuildCard(canvas, "LoadPanel", "LoadCard", "ロード",  prefab, SavePanelMode.Load);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Novella] Save Panels を再構築しました。");
    }

    // =========================================================
    // プレファブ生成
    // =========================================================
    private static GameObject BuildSaveSlotPrefab()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        var slot = new GameObject("SaveSlot");
        slot.AddComponent<RectTransform>();

        // 背景
        var slotImg = slot.AddComponent<Image>();
        slotImg.color = new Color(0.1f, 0.1f, 0.15f, 0.92f);

        // スロット全体をボタン化
        var btn = slot.AddComponent<Button>();
        var colors = ColorBlock.defaultColorBlock;
        colors.normalColor    = new Color(1f, 1f, 1f, 1f);
        colors.highlightedColor = new Color(0.85f, 0.92f, 1f, 1f);
        colors.pressedColor   = new Color(0.65f, 0.78f, 0.95f, 1f);
        btn.colors = colors;

        // HLG
        var hlg = slot.AddComponent<HorizontalLayoutGroup>();
        hlg.padding           = new RectOffset(0, 0, 0, 0);
        hlg.spacing           = 0;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;

        // --- サムネイルエリア ---
        var thumbGO  = new GameObject("ThumbnailHolder");
        thumbGO.transform.SetParent(slot.transform, false);
        var thumbLE  = thumbGO.AddComponent<LayoutElement>();
        thumbLE.minWidth       = 180;
        thumbLE.preferredWidth = 180;
        thumbLE.flexibleWidth  = 0;
        var thumbImg = thumbGO.AddComponent<Image>();
        thumbImg.color = new Color(0.07f, 0.07f, 0.12f, 1f);

        // No Data オーバーレイ（サムネイル内）
        var noDataGO = new GameObject("NoDataOverlay");
        noDataGO.transform.SetParent(thumbGO.transform, false);
        var noDataRect = noDataGO.AddComponent<RectTransform>();
        noDataRect.anchorMin  = Vector2.zero;
        noDataRect.anchorMax  = Vector2.one;
        noDataRect.sizeDelta  = Vector2.zero;
        var noDataVLG = noDataGO.AddComponent<VerticalLayoutGroup>();
        noDataVLG.childAlignment    = TextAnchor.MiddleCenter;
        noDataVLG.childControlWidth = true;
        noDataVLG.childControlHeight = true;

        var noDataTxtGO = new GameObject("NoDataText");
        noDataTxtGO.transform.SetParent(noDataGO.transform, false);
        var noDataTMP = noDataTxtGO.AddComponent<TextMeshProUGUI>();
        noDataTMP.text               = "No Data";
        noDataTMP.fontSize           = 22;
        noDataTMP.color              = new Color(0.55f, 0.55f, 0.6f, 1f);
        noDataTMP.alignment          = TextAlignmentOptions.Center;
        noDataTMP.verticalAlignment  = VerticalAlignmentOptions.Middle;
        noDataTMP.isOrthographic     = true;
        if (font != null) noDataTMP.font = font;

        // --- 情報エリア ---
        var infoGO  = new GameObject("InfoPanel");
        infoGO.transform.SetParent(slot.transform, false);
        var infoLE  = infoGO.AddComponent<LayoutElement>();
        infoLE.flexibleWidth = 1;
        var infoVLG = infoGO.AddComponent<VerticalLayoutGroup>();
        infoVLG.padding              = new RectOffset(12, 12, 8, 8);
        infoVLG.spacing              = 4;
        infoVLG.childControlWidth    = true;
        infoVLG.childControlHeight   = true;
        infoVLG.childForceExpandWidth  = true;
        infoVLG.childForceExpandHeight = false;

        // ヘッダー行（スロット番号 + 日時）
        var headerGO  = new GameObject("HeaderRow");
        headerGO.transform.SetParent(infoGO.transform, false);
        var headerLE  = headerGO.AddComponent<LayoutElement>();
        headerLE.preferredHeight = 32;
        var headerHLG = headerGO.AddComponent<HorizontalLayoutGroup>();
        headerHLG.childControlWidth    = true;
        headerHLG.childControlHeight   = true;
        headerHLG.childForceExpandWidth  = false;
        headerHLG.childForceExpandHeight = true;
        headerHLG.spacing = 8;

        var slotNumGO  = MakeTMP(headerGO, "SlotNumberText", "SLOT 00", 22, Color.white, TextAlignmentOptions.Left, font);
        slotNumGO.AddComponent<LayoutElement>().preferredWidth = 110;

        var dateSpacer = MakeTMP(headerGO, "DateText", "", 20, new Color(0.75f, 0.75f, 0.75f), TextAlignmentOptions.Right, font);
        dateSpacer.AddComponent<LayoutElement>().flexibleWidth = 1;

        // 章タイトル
        var titleGO = MakeTMP(infoGO, "TitleText", "", 28, Color.white, TextAlignmentOptions.Left, font);
        var titleLE  = titleGO.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 38;
        var titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
        titleTMP.overflowMode = TextOverflowModes.Ellipsis;

        // 最後のセリフ
        var dialogueGO  = MakeTMP(infoGO, "DialogueText", "", 20, new Color(0.7f, 0.7f, 0.7f), TextAlignmentOptions.Left, font);
        var dialogueLE  = dialogueGO.AddComponent<LayoutElement>();
        dialogueLE.flexibleHeight = 1;
        var dialogueTMP = dialogueGO.GetComponent<TextMeshProUGUI>();
        dialogueTMP.overflowMode      = TextOverflowModes.Ellipsis;
        dialogueTMP.verticalAlignment = VerticalAlignmentOptions.Top;

        // SaveSlotView の配線
        var view = slot.AddComponent<SaveSlotView>();
        view.SlotNumberText = slotNumGO.GetComponent<TextMeshProUGUI>();
        view.DateText       = dateSpacer.GetComponent<TextMeshProUGUI>();
        view.TitleText      = titleTMP;
        view.DialogueText   = dialogueTMP;
        view.NoDataOverlay  = noDataGO;
        view.ThumbnailImage = thumbImg;

        var saved = PrefabUtility.SaveAsPrefabAsset(slot, PrefabPath);
        Object.DestroyImmediate(slot);
        return saved;
    }

    // =========================================================
    // カード再構築
    // =========================================================
    private static void RebuildCard(
        GameObject canvas, string panelName, string cardName,
        string titleText, GameObject slotPrefab, SavePanelMode mode)
    {
        var cameraRoot = canvas.transform.Find("CameraRoot");
        var searchRoot = cameraRoot != null ? cameraRoot : canvas.transform;
        var panelTr = searchRoot.Find(panelName);
        if (panelTr == null) { Debug.LogError($"[Novella] {panelName} が見つかりません。"); return; }

        var cardTr = panelTr.Find(cardName);
        if (cardTr == null) { Debug.LogError($"[Novella] {cardName} が見つかりません。"); return; }

        // childControlHeight=true に設定（flexibleHeight が機能するために必要）
        var cardVLG = cardTr.GetComponent<VerticalLayoutGroup>();
        if (cardVLG != null)
        {
            var cvso = new SerializedObject(cardVLG);
            cvso.FindProperty("m_ChildControlHeight").boolValue = true;
            cvso.ApplyModifiedProperties();
        }

        // タイトルと閉じるボタンを保持し、他を削除
        var closeBtnTr = cardTr.Find("SCloseBtn") ?? cardTr.Find("LCloseBtn");
        var titleTr    = cardTr.Find("SaveTitle") ?? cardTr.Find("LoadTitle");

        var toDelete = new System.Collections.Generic.List<GameObject>();
        foreach (Transform child in cardTr)
        {
            if (child == closeBtnTr || child == titleTr) continue;
            toDelete.Add(child.gameObject);
        }
        toDelete.ForEach(Object.DestroyImmediate);

        // タイトルテキスト更新
        if (titleTr != null)
        {
            var tmp = titleTr.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = titleText;
            titleTr.SetAsFirstSibling();
        }

        // --- SlotScrollView（ScrollRect + Viewport + SlotContainer） ---
        var scrollGO = new GameObject("SlotScrollView");
        scrollGO.transform.SetParent(cardTr, false);
        scrollGO.transform.SetSiblingIndex(1); // タイトルの次

        var scrollLE = scrollGO.AddComponent<LayoutElement>();
        scrollLE.flexibleHeight = 1;
        scrollLE.flexibleWidth  = 1;

        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal        = false;
        scrollRect.vertical          = true;
        scrollRect.scrollSensitivity = 40;

        // Viewport
        var viewportGO   = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollGO.transform, false);
        var viewportRect = viewportGO.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.pivot     = new Vector2(0, 1);
        viewportGO.AddComponent<RectMask2D>();

        // SlotContainer（GridLayoutGroup）
        var containerGO   = new GameObject("SlotContainer");
        containerGO.transform.SetParent(viewportGO.transform, false);
        var containerRect = containerGO.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.pivot     = new Vector2(0.5f, 1);
        containerRect.sizeDelta = Vector2.zero;

        var glg = containerGO.AddComponent<GridLayoutGroup>();
        glg.cellSize        = new Vector2(CellWidth, CellHeight);
        glg.spacing         = new Vector2(16, 12);
        glg.startCorner     = GridLayoutGroup.Corner.UpperLeft;
        glg.startAxis       = GridLayoutGroup.Axis.Horizontal;
        glg.childAlignment  = TextAnchor.UpperLeft;
        glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 2;

        var csf = containerGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content  = containerRect;

        // 閉じるボタンを末尾へ
        closeBtnTr?.SetAsLastSibling();

        // SaveUIController に参照を設定
        var saveUI = panelTr.GetComponent<SaveUIController>();
        if (saveUI != null)
        {
            var so = new SerializedObject(saveUI);
            so.FindProperty("_slotContainer").objectReferenceValue = containerRect;
            so.FindProperty("_slotPrefab").objectReferenceValue    = slotPrefab;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(saveUI);
        }
    }

    // =========================================================
    // ユーティリティ
    // =========================================================
    private static GameObject MakeTMP(
        GameObject parent, string name, string text,
        float fontSize, Color color, TextAlignmentOptions align,
        TMP_FontAsset font = null)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text              = text;
        tmp.fontSize          = fontSize;
        tmp.color             = color;
        tmp.alignment         = align;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.isOrthographic    = true;
        tmp.overflowMode      = TextOverflowModes.Overflow;
        if (font != null) tmp.font = font;
        return go;
    }
}
#endif
