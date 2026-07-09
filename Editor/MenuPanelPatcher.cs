#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Novella.UI;

/// <summary>
/// Novella > Patch Menu: Add Title Button
/// MenuPanelに「タイトルへ」ボタンを追加し、MenuUIControllerに配線する。
/// </summary>
public class MenuPanelPatcher
{
    private const string FontPath = "Assets/font_1_kokugl_1.asset";

    [MenuItem("Novella/Patch Menu: Add Title Button")]
    public static void Patch()
    {
        var menuCard = GameObject.Find("NovellaCanvas/CameraRoot/MenuPanel/MenuCard")
            ?? GameObject.Find("NovellaCanvas/MenuPanel/MenuCard"); // CameraRootが無い旧構成へのフォールバック
        if (menuCard == null)
        {
            Debug.LogError("[Novella] MenuCard が見つかりません。SampleSceneを開いてください。");
            return;
        }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        // 既存のタイトルボタンがあれば削除
        var existing = menuCard.transform.Find("MenuTitleButton");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        // 既存ボタンのサイズを参考にする
        var refBtn = menuCard.transform.Find("MenuSaveButton");
        float btnHeight = 60f;
        if (refBtn != null)
        {
            var refLE = refBtn.GetComponent<LayoutElement>();
            if (refLE != null) btnHeight = refLE.preferredHeight;
        }

        // ボタン作成
        var btnGO = new GameObject("MenuTitleButton");
        btnGO.transform.SetParent(menuCard.transform, false);

        var rect = btnGO.AddComponent<RectTransform>();
        var img = btnGO.AddComponent<Image>();
        img.color = new Color(0.6f, 0.25f, 0.25f, 1f);

        var btn = btnGO.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.75f, 0.3f, 0.3f);
        colors.pressedColor = new Color(0.5f, 0.2f, 0.2f);
        btn.colors = colors;

        var le = btnGO.AddComponent<LayoutElement>();
        le.preferredHeight = btnHeight;

        // テキスト
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "TITLE";
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        if (font != null) tmp.font = font;

        // CloseButton(最後の子)の直前に配置
        int closeIndex = menuCard.transform.childCount - 1;
        btnGO.transform.SetSiblingIndex(closeIndex);

        // MenuUIController に配線
        var novellaManager = GameObject.Find("NovellaManager");
        if (novellaManager != null)
        {
            var menuUI = novellaManager.GetComponent<MenuUIController>();
            if (menuUI != null)
            {
                var so = new SerializedObject(menuUI);
                so.FindProperty("_titleButton").objectReferenceValue = btn;
                so.ApplyModifiedProperties();
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Novella] MenuPanel に「TITLE」ボタンを追加しました。");
    }
}
#endif
