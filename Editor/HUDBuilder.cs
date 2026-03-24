#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Novella.UI;

public class HUDBuilder
{
    [MenuItem("Novella/Build HUD")]
    public static void Build()
    {
        var canvas = GameObject.Find("NovellaCanvas");
        if (canvas == null) { Debug.LogError("NovellaCanvas not found"); return; }

        var existing = canvas.transform.Find("HUDPanel");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        // HUDPanel
        var panel = new GameObject("HUDPanel");
        panel.transform.SetParent(canvas.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(1, 0);
        panelRect.anchoredPosition = new Vector2(-10, 10);
        panelRect.sizeDelta = new Vector2(585, 48);

        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.55f);

        var hlg = panel.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(8, 8, 6, 6);
        hlg.spacing = 6;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        var buttonDefs = new (string label, string name)[]
        {
            ("QS",   "QSButton"),
            ("QL",   "QLButton"),
            ("SAVE", "SaveButton"),
            ("LOAD", "LoadButton"),
            ("AUTO", "AutoButton"),
            ("SKIP", "SkipButton"),
            ("LOG",  "BacklogButton"),
            ("HIDE", "HideButton"),
            ("MENU", "MenuButton"),
        };

        foreach (var (label, name) in buttonDefs)
        {
            var btn = new GameObject(name);
            btn.transform.SetParent(panel.transform, false);
            btn.AddComponent<RectTransform>();
            var img = btn.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            btn.AddComponent<Button>();

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(btn.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 18;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }

        // HUDController を NovellaManager に追加して配線
        var novellaManager = GameObject.Find("NovellaManager");
        if (novellaManager != null)
        {
            var existingHud = novellaManager.GetComponent<HUDController>();
            if (existingHud != null) Object.DestroyImmediate(existingHud);

            var hud = novellaManager.AddComponent<HUDController>();
            var so = new SerializedObject(hud);

            so.FindProperty("_hudPanel").objectReferenceValue = panel;
            so.FindProperty("_engine").objectReferenceValue =
                novellaManager.GetComponent<Novella.Core.NovellaEngine>();
            so.FindProperty("_menuUI").objectReferenceValue =
                novellaManager.GetComponent<MenuUIController>();
            so.FindProperty("_backlogUI").objectReferenceValue =
                novellaManager.GetComponent<BacklogUIController>();

            // MenuUIController から SavePanel / LoadPanel の SaveUIController を取得して配線
            var menuUI = novellaManager.GetComponent<MenuUIController>();
            if (menuUI != null)
            {
                var menuSO = new SerializedObject(menuUI);
                so.FindProperty("_saveUI").objectReferenceValue =
                    menuSO.FindProperty("_saveUI").objectReferenceValue;
                so.FindProperty("_loadUI").objectReferenceValue =
                    menuSO.FindProperty("_loadUI").objectReferenceValue;
            }

            so.FindProperty("_quickSaveButton").objectReferenceValue =
                panel.transform.Find("QSButton")?.GetComponent<Button>();
            so.FindProperty("_quickLoadButton").objectReferenceValue =
                panel.transform.Find("QLButton")?.GetComponent<Button>();
            so.FindProperty("_saveButton").objectReferenceValue =
                panel.transform.Find("SaveButton")?.GetComponent<Button>();
            so.FindProperty("_loadButton").objectReferenceValue =
                panel.transform.Find("LoadButton")?.GetComponent<Button>();
            so.FindProperty("_autoButton").objectReferenceValue =
                panel.transform.Find("AutoButton")?.GetComponent<Button>();
            so.FindProperty("_skipButton").objectReferenceValue =
                panel.transform.Find("SkipButton")?.GetComponent<Button>();
            so.FindProperty("_backlogButton").objectReferenceValue =
                panel.transform.Find("BacklogButton")?.GetComponent<Button>();
            so.FindProperty("_hideButton").objectReferenceValue =
                panel.transform.Find("HideButton")?.GetComponent<Button>();
            so.FindProperty("_menuButton").objectReferenceValue =
                panel.transform.Find("MenuButton")?.GetComponent<Button>();

            so.FindProperty("_autoLabel").objectReferenceValue =
                panel.transform.Find("AutoButton/Text")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("_skipLabel").objectReferenceValue =
                panel.transform.Find("SkipButton/Text")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("_quickLoadLabel").objectReferenceValue =
                panel.transform.Find("QLButton/Text")?.GetComponent<TextMeshProUGUI>();

            so.ApplyModifiedProperties();
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Novella] HUD built successfully.");
    }
}
#endif
