#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Novella.UI;

public static class SavePanelPager
{
    [MenuItem("Novella/Add Save Panel Paging")]
    public static void AddPaging()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/font_1_kokugl_1.asset");

        AddPagingToPanel("SavePanel", "SaveCard", "SCloseBtn", font);
        AddPagingToPanel("LoadPanel", "LoadCard", "LCloseBtn", font);

        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("[Novella] Save/Load panel paging UI added.");
    }

    private static void AddPagingToPanel(string panelName, string cardName, string closeBtnName, TMP_FontAsset font)
    {
        var panel = GameObject.Find(panelName);
        if (panel == null) { Debug.LogError($"[Novella] {panelName} not found."); return; }

        var card = panel.transform.Find(cardName);
        if (card == null) { Debug.LogError($"[Novella] {cardName} not found in {panelName}."); return; }

        // Remove existing paging bar if present
        var existing = card.Find("PageBar");
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        // Create PageBar
        var bar = new GameObject("PageBar", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        bar.transform.SetParent(card, false);

        var barLayout = bar.GetComponent<LayoutElement>();
        barLayout.preferredHeight = 60;
        barLayout.flexibleWidth = 1;

        var hlg = bar.GetComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 20;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset(20, 20, 5, 5);

        // Place before CloseBtn (CloseBtn is last sibling)
        var closeBtn = card.Find(closeBtnName);
        if (closeBtn != null)
            bar.transform.SetSiblingIndex(closeBtn.GetSiblingIndex());

        // Prev button
        var prevBtn = CreateButton("PrevBtn", "<", 80, 50, font);
        prevBtn.transform.SetParent(bar.transform, false);

        // Page label
        var labelGo = new GameObject("PageLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
        labelGo.transform.SetParent(bar.transform, false);
        var labelTmp = labelGo.GetComponent<TextMeshProUGUI>();
        labelTmp.text = "1 / 5";
        labelTmp.fontSize = 28;
        labelTmp.alignment = TextAlignmentOptions.Center;
        labelTmp.color = Color.white;
        if (font != null) labelTmp.font = font;
        var labelLE = labelGo.GetComponent<LayoutElement>();
        labelLE.preferredWidth = 120;
        labelLE.preferredHeight = 50;

        // Next button
        var nextBtn = CreateButton("NextBtn", ">", 80, 50, font);
        nextBtn.transform.SetParent(bar.transform, false);

        // Wire to SaveUIController
        var controller = panel.GetComponent<SaveUIController>();
        if (controller != null)
        {
            var so = new SerializedObject(controller);
            so.FindProperty("_prevPageButton").objectReferenceValue = prevBtn.GetComponent<Button>();
            so.FindProperty("_nextPageButton").objectReferenceValue = nextBtn.GetComponent<Button>();
            so.FindProperty("_pageLabel").objectReferenceValue = labelTmp;
            so.ApplyModifiedProperties();
        }
    }

    private static GameObject CreateButton(string name, string label, float w, float h, TMP_FontAsset font)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
        var img = go.GetComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.3f, 1f);

        var le = go.GetComponent<LayoutElement>();
        le.preferredWidth = w;
        le.preferredHeight = h;

        var txtGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(go.transform, false);
        var rt = txtGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var tmp = txtGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        if (font != null) tmp.font = font;

        return go;
    }
}
#endif
