#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Novella.UI;

/// <summary>
/// Novella > Rebuild Title Settings Panel
/// TitleScene用の設定パネルを構築する（SampleScene版 SettingsPanelBuilder のTitle版）。
/// </summary>
public class TitleSettingsPanelBuilder
{
    private const string FontPath = "Assets/font_1_kokugl_1.asset";

    [MenuItem("Novella/Rebuild Title Settings Panel")]
    public static void Build()
    {
        var canvas = GameObject.Find("TitleCanvas");
        if (canvas == null) { Debug.LogError("[Novella] TitleCanvas が見つかりません。TitleSceneを開いてください。"); return; }

        var titleManager = GameObject.Find("TitleManager");
        if (titleManager == null) { Debug.LogError("[Novella] TitleManager が見つかりません。"); return; }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        // --- 既存パネルを削除して再構築 ---
        var existingPanel = canvas.transform.Find("SettingsPanel");
        if (existingPanel != null) Object.DestroyImmediate(existingPanel.gameObject);

        // --- 全画面オーバーレイ ---
        var panelGO = new GameObject("SettingsPanel");
        panelGO.transform.SetParent(canvas.transform, false);
        var panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        var overlayImg = panelGO.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.6f);

        // --- 中央カード ---
        var cardGO = new GameObject("SettingsCard");
        cardGO.transform.SetParent(panelGO.transform, false);
        var cardRect = cardGO.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(900, 700);
        var cardImg = cardGO.AddComponent<Image>();
        cardImg.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);
        var cardVLG = cardGO.AddComponent<VerticalLayoutGroup>();
        cardVLG.padding = new RectOffset(0, 0, 0, 0);
        cardVLG.spacing = 0;
        cardVLG.childControlWidth = true;
        cardVLG.childControlHeight = true;
        cardVLG.childForceExpandWidth = true;
        cardVLG.childForceExpandHeight = false;

        var cardTr = cardGO.transform;

        // --- タイトル ---
        var titleGO = MakeTMP(cardGO, "SettingsTitle", "設定", 42, Color.white, TextAlignmentOptions.Center, font);
        var titleLE = titleGO.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 80;

        // --- タブボタン行 ---
        var tabRow = new GameObject("TabRow");
        tabRow.transform.SetParent(cardTr, false);
        var tabRowLE = tabRow.AddComponent<LayoutElement>();
        tabRowLE.preferredHeight = 60;
        tabRowLE.flexibleHeight = 0;
        var tabRowHLG = tabRow.AddComponent<HorizontalLayoutGroup>();
        tabRowHLG.padding = new RectOffset(40, 40, 8, 8);
        tabRowHLG.spacing = 20;
        tabRowHLG.childControlWidth = true;
        tabRowHLG.childControlHeight = true;
        tabRowHLG.childForceExpandWidth = true;
        tabRowHLG.childForceExpandHeight = true;

        var gameTabBtn = MakeButton(tabRow, "GameTabButton", "ゲーム設定", Color.white, font);
        var soundTabBtn = MakeButton(tabRow, "SoundTabButton", "サウンド設定", Color.white, font);

        // --- タブパネル: ゲーム設定 ---
        var (gameTabPanelGO, gameTabContent) = BuildTabContent(cardTr, "GameTabPanel");

        MakeSectionHeader(gameTabContent, "テキスト", font);
        var textSpeedSlider = BuildSliderRow(gameTabContent, "TextSpeedRow", "テキスト速度", font);
        var autoDelaySlider = BuildSliderRow(gameTabContent, "AutoDelayRow", "オート待ち時間", font);
        var fontSizeSlider = BuildSliderRow(gameTabContent, "FontSizeRow", "フォントサイズ", font);
        var skipUnreadToggle = BuildToggleRow(gameTabContent, "SkipUnreadRow", "未読もスキップする", font);
        var skipAfterChoiceToggle = BuildToggleRow(gameTabContent, "SkipAfterChoiceRow", "選択肢後もスキップを続ける", font);
        var autoSaveToggle = BuildToggleRow(gameTabContent, "AutoSaveRow", "オートセーブ", font);

        MakeSectionHeader(gameTabContent, "表示", font);
        var windowOpacitySlider = BuildSliderRow(gameTabContent, "WindowOpacityRow", "ウィンドウ透明度", font);
        var fullscreenToggle = BuildToggleRow(gameTabContent, "FullscreenRow", "フルスクリーン", font);

        // --- タブパネル: サウンド設定 ---
        var (soundTabPanelGO, soundTabContent) = BuildTabContent(cardTr, "SoundTabPanel");

        MakeSectionHeader(soundTabContent, "サウンド", font);
        var bgmVolumeSlider = BuildSliderRow(soundTabContent, "BgmVolumeRow", "BGM音量", font);
        var seVolumeSlider = BuildSliderRow(soundTabContent, "SeVolumeRow", "SE音量", font);
        var voiceVolumeSlider = BuildSliderRow(soundTabContent, "VoiceVolumeRow", "ボイス音量", font);

        gameTabPanelGO.SetActive(true);
        soundTabPanelGO.SetActive(false);

        // --- ボタン行 ---
        var btnRow = new GameObject("ButtonRow");
        btnRow.transform.SetParent(cardTr, false);
        var btnRowLE = btnRow.AddComponent<LayoutElement>();
        btnRowLE.preferredHeight = 60;
        btnRowLE.flexibleHeight = 0;
        var btnRowHLG = btnRow.AddComponent<HorizontalLayoutGroup>();
        btnRowHLG.padding = new RectOffset(40, 40, 8, 8);
        btnRowHLG.spacing = 20;
        btnRowHLG.childControlWidth = true;
        btnRowHLG.childControlHeight = true;
        btnRowHLG.childForceExpandWidth = true;
        btnRowHLG.childForceExpandHeight = true;

        var resetBtn = MakeButton(btnRow, "ResetButton", "初期化", new Color(0.5f, 0.5f, 0.5f, 1f), font);
        var closeBtn = MakeButton(btnRow, "SettingsCloseButton", "閉じる", new Color(0.8f, 0.2f, 0.2f, 1f), font);

        panelGO.SetActive(false);

        // --- SettingsUIController をTitleManagerに用意して配線 ---
        var settingsUI = titleManager.GetComponent<SettingsUIController>();
        if (settingsUI == null)
            settingsUI = titleManager.AddComponent<SettingsUIController>();

        var so = new SerializedObject(settingsUI);
        so.FindProperty("_panel").objectReferenceValue = panelGO;
        so.FindProperty("_textSpeedSlider").objectReferenceValue = textSpeedSlider.GetComponent<Slider>();
        so.FindProperty("_bgmVolumeSlider").objectReferenceValue = bgmVolumeSlider.GetComponent<Slider>();
        so.FindProperty("_seVolumeSlider").objectReferenceValue = seVolumeSlider.GetComponent<Slider>();
        so.FindProperty("_voiceVolumeSlider").objectReferenceValue = voiceVolumeSlider.GetComponent<Slider>();
        so.FindProperty("_autoDelaySlider").objectReferenceValue = autoDelaySlider.GetComponent<Slider>();
        so.FindProperty("_windowOpacitySlider").objectReferenceValue = windowOpacitySlider.GetComponent<Slider>();
        so.FindProperty("_fontSizeSlider").objectReferenceValue = fontSizeSlider.GetComponent<Slider>();
        so.FindProperty("_fullscreenToggle").objectReferenceValue = fullscreenToggle.GetComponent<Toggle>();
        so.FindProperty("_skipUnreadToggle").objectReferenceValue = skipUnreadToggle.GetComponent<Toggle>();
        so.FindProperty("_skipAfterChoiceToggle").objectReferenceValue = skipAfterChoiceToggle.GetComponent<Toggle>();
        so.FindProperty("_autoSaveToggle").objectReferenceValue = autoSaveToggle.GetComponent<Toggle>();
        so.FindProperty("_textSpeedLabel").objectReferenceValue = textSpeedSlider.transform.parent.Find("Label")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_bgmVolumeLabel").objectReferenceValue = bgmVolumeSlider.transform.parent.Find("Label")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_seVolumeLabel").objectReferenceValue = seVolumeSlider.transform.parent.Find("Label")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_voiceVolumeLabel").objectReferenceValue = voiceVolumeSlider.transform.parent.Find("Label")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_autoDelayLabel").objectReferenceValue = autoDelaySlider.transform.parent.Find("Label")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_windowOpacityLabel").objectReferenceValue = windowOpacitySlider.transform.parent.Find("Label")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_fontSizeLabel").objectReferenceValue = fontSizeSlider.transform.parent.Find("Label")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_closeButton").objectReferenceValue = closeBtn.GetComponent<Button>();
        so.FindProperty("_resetButton").objectReferenceValue = resetBtn.GetComponent<Button>();
        so.FindProperty("_gameTabButton").objectReferenceValue = gameTabBtn.GetComponent<Button>();
        so.FindProperty("_soundTabButton").objectReferenceValue = soundTabBtn.GetComponent<Button>();
        so.FindProperty("_gameTabPanel").objectReferenceValue = gameTabPanelGO;
        so.FindProperty("_soundTabPanel").objectReferenceValue = soundTabPanelGO;
        so.FindProperty("_gameTabButtonImage").objectReferenceValue = gameTabBtn.GetComponent<Image>();
        so.FindProperty("_soundTabButtonImage").objectReferenceValue = soundTabBtn.GetComponent<Image>();
        so.FindProperty("_gameTabButtonLabel").objectReferenceValue = gameTabBtn.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        so.FindProperty("_soundTabButtonLabel").objectReferenceValue = soundTabBtn.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        // ConfirmDialogはTitleSceneに未構築のため未配線（未配線でも初期化コードが確認なしリセットにフォールバックする）
        so.ApplyModifiedProperties();

        // --- TitleManager._settingsUI に配線 ---
        var tm = titleManager.GetComponent<Novella.Core.TitleManager>();
        if (tm != null)
        {
            var tmSO = new SerializedObject(tm);
            tmSO.FindProperty("_settingsUI").objectReferenceValue = settingsUI;
            tmSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(tm);
        }

        EditorUtility.SetDirty(settingsUI);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Novella] Title Settings Panel を構築しました。");
    }

    // =========================================================
    // タブコンテンツ（ScrollView構造）
    // =========================================================
    private static (GameObject root, GameObject content) BuildTabContent(Transform cardTr, string name)
    {
        var scrollGO = new GameObject(name);
        scrollGO.transform.SetParent(cardTr, false);
        var scrollLE = scrollGO.AddComponent<LayoutElement>();
        scrollLE.flexibleHeight = 1;
        scrollLE.flexibleWidth = 1;

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

        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = Vector2.zero;

        var contentVLG = content.AddComponent<VerticalLayoutGroup>();
        contentVLG.padding = new RectOffset(20, 20, 10, 10);
        contentVLG.spacing = 8;
        contentVLG.childControlWidth = true;
        contentVLG.childControlHeight = true;
        contentVLG.childForceExpandWidth = true;
        contentVLG.childForceExpandHeight = false;

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = vpRect;
        scrollRect.content = contentRect;

        return (scrollGO, content);
    }

    // =========================================================
    // セクションヘッダー
    // =========================================================
    private static void MakeSectionHeader(GameObject parent, string text, TMP_FontAsset font)
    {
        var row = new GameObject($"Section_{text}");
        row.transform.SetParent(parent.transform, false);
        var rowLE = row.AddComponent<LayoutElement>();
        rowLE.preferredHeight = 50;

        var label = MakeTMP(row, "Header", text, 30, new Color(0.5f, 0.8f, 1f, 1f), TextAlignmentOptions.Left, font);
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;
        labelRect.offsetMin = new Vector2(20, 0);

        var line = new GameObject("Line");
        line.transform.SetParent(row.transform, false);
        var lineRect = line.AddComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0, 0);
        lineRect.anchorMax = new Vector2(1, 0);
        lineRect.sizeDelta = new Vector2(0, 2);
        lineRect.anchoredPosition = Vector2.zero;
        var lineImg = line.AddComponent<Image>();
        lineImg.color = new Color(0.4f, 0.6f, 0.8f, 0.5f);
    }

    // =========================================================
    // スライダー行
    // =========================================================
    private static GameObject BuildSliderRow(GameObject parent, string rowName, string labelText, TMP_FontAsset font)
    {
        var row = new GameObject(rowName);
        row.transform.SetParent(parent.transform, false);
        var rowLE = row.AddComponent<LayoutElement>();
        rowLE.preferredHeight = 70;
        var rowHLG = row.AddComponent<HorizontalLayoutGroup>();
        rowHLG.padding = new RectOffset(40, 40, 0, 0);
        rowHLG.spacing = 20;
        rowHLG.childAlignment = TextAnchor.MiddleLeft;
        rowHLG.childControlWidth = true;
        rowHLG.childControlHeight = true;
        rowHLG.childForceExpandWidth = false;
        rowHLG.childForceExpandHeight = true;

        var labelGO = MakeTMP(row, "Label", labelText, 26, Color.white, TextAlignmentOptions.Left, font);
        var labelLE = labelGO.AddComponent<LayoutElement>();
        labelLE.preferredWidth = 420;
        labelLE.flexibleWidth = 0;

        var sliderGO = new GameObject("Slider");
        sliderGO.transform.SetParent(row.transform, false);
        var sliderLE = sliderGO.AddComponent<LayoutElement>();
        sliderLE.preferredWidth = 500;
        sliderLE.flexibleWidth = 1;

        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        var bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.25f);
        bgRect.anchorMax = new Vector2(1, 0.75f);
        bgRect.sizeDelta = Vector2.zero;
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.35f, 1f);

        var fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        var fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1, 0.75f);
        fillAreaRect.offsetMin = new Vector2(5, 0);
        fillAreaRect.offsetMax = new Vector2(-5, 0);

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        var fillRect = fillGO.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.4f, 0.7f, 1f, 1f);

        var handleAreaGO = new GameObject("Handle Slide Area");
        handleAreaGO.transform.SetParent(sliderGO.transform, false);
        var handleAreaRect = handleAreaGO.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10, 0);
        handleAreaRect.offsetMax = new Vector2(-10, 0);

        var handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(handleAreaGO.transform, false);
        var handleRect = handleGO.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(30, 0);
        var handleImg = handleGO.AddComponent<Image>();
        handleImg.color = Color.white;

        var slider = sliderGO.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;

        return sliderGO;
    }

    // =========================================================
    // トグル行
    // =========================================================
    private static GameObject BuildToggleRow(GameObject parent, string rowName, string labelText, TMP_FontAsset font)
    {
        var row = new GameObject(rowName);
        row.transform.SetParent(parent.transform, false);
        var rowLE = row.AddComponent<LayoutElement>();
        rowLE.preferredHeight = 60;
        var rowHLG = row.AddComponent<HorizontalLayoutGroup>();
        rowHLG.padding = new RectOffset(40, 40, 0, 0);
        rowHLG.spacing = 20;
        rowHLG.childAlignment = TextAnchor.MiddleLeft;
        rowHLG.childControlWidth = true;
        rowHLG.childControlHeight = true;
        rowHLG.childForceExpandWidth = false;
        rowHLG.childForceExpandHeight = true;

        var labelGO = MakeTMP(row, "Label", labelText, 26, Color.white, TextAlignmentOptions.Left, font);
        var labelLE = labelGO.AddComponent<LayoutElement>();
        labelLE.preferredWidth = 420;
        labelLE.flexibleWidth = 1;

        var toggleGO = new GameObject("Toggle");
        toggleGO.transform.SetParent(row.transform, false);
        var toggleLE = toggleGO.AddComponent<LayoutElement>();
        toggleLE.preferredWidth = 60;
        toggleLE.preferredHeight = 40;
        toggleLE.flexibleWidth = 0;

        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(toggleGO.transform, false);
        var bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.sizeDelta = new Vector2(50, 50);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.35f, 1f);

        var checkGO = new GameObject("Checkmark");
        checkGO.transform.SetParent(bgGO.transform, false);
        var checkRect = checkGO.AddComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.15f, 0.15f);
        checkRect.anchorMax = new Vector2(0.85f, 0.85f);
        checkRect.sizeDelta = Vector2.zero;
        var checkImg = checkGO.AddComponent<Image>();
        checkImg.color = new Color(0.4f, 0.85f, 0.4f, 1f);

        var toggle = toggleGO.AddComponent<Toggle>();
        toggle.targetGraphic = bgImg;
        toggle.graphic = checkImg;
        toggle.isOn = false;

        return toggleGO;
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
