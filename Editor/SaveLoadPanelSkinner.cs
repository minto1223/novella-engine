using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace NovellaEditor
{
    /// <summary>
    /// Save/LoadパネルとSaveSlot.prefabにそらいろクオリアのスキンを一括適用するMCP作業用ツール。
    /// </summary>
    public static class SaveLoadPanelSkinner
    {
        private const string ThemeDir = "Assets/Novella/UI/Sprites/Theme/";
        private const string SlotPrefabPath = "Assets/Novella/Prefabs/SaveSlot.prefab";
        private static readonly Color Ink = new Color(0.133f, 0.22f, 0.29f, 1f);
        private static readonly Color UiBlue = new Color(0.239f, 0.416f, 0.541f, 1f);
        private static readonly Color SubBlue = new Color(0.357f, 0.498f, 0.608f, 1f);

        [MenuItem("Tools/Novella/Apply Sorairo SaveLoad Skin")]
        public static void Apply()
        {
            var bg = Load("bg_sky_gradient.png");
            var card = Load("panel_card.png");
            var pill = Load("button_pill.png");
            var pillAccent = Load("button_pill_accent.png");

            // ---- SaveSlot.prefab ----
            var prefabRoot = PrefabUtility.LoadPrefabContents(SlotPrefabPath);
            try
            {
                var rootImg = prefabRoot.GetComponent<Image>();
                SetImage(rootImg, card, Image.Type.Sliced, new Color(1, 1, 1, 0.95f));

                foreach (var img in prefabRoot.GetComponentsInChildren<Image>(true))
                {
                    if (img.name == "ThumbnailHolder" || (img.transform.parent != null && img.transform.parent.name == "ThumbnailHolder" && img.sprite == null))
                        img.color = new Color(0.85f, 0.9f, 0.94f, 1f);
                    else if (img.name == "NoDataOverlay")
                        img.color = new Color(0.93f, 0.96f, 0.98f, 0.85f);
                }

                foreach (var txt in prefabRoot.GetComponentsInChildren<TMP_Text>(true))
                {
                    switch (txt.name)
                    {
                        case "SlotNumberText": txt.color = UiBlue; txt.fontStyle = FontStyles.Bold; break;
                        case "TitleText": txt.color = Ink; txt.fontStyle = FontStyles.Bold; break;
                        case "DateText": txt.color = SubBlue; break;
                        case "DialogueText": txt.color = SubBlue; break;
                        case "NoDataText": txt.color = SubBlue; break;
                        default: txt.color = Ink; break;
                    }
                }
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, SlotPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            // ---- シーン内の Save/Load パネル ----
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!canvas.isRootCanvas) continue;
                foreach (var panelName in new[] { "SavePanel", "LoadPanel" })
                {
                    var panel = canvas.transform.Find(panelName);
                    if (panel == null) continue;

                    Undo.RegisterFullObjectHierarchyUndo(panel.gameObject, "Sorairo SaveLoad Skin");
                    SetImage(panel.GetComponent<Image>(), bg, Image.Type.Simple, new Color(1, 1, 1, 0.97f));

                    var cardT = panel.GetChild(0);
                    SetImage(cardT.GetComponent<Image>(), card, Image.Type.Sliced, new Color(1, 1, 1, 0.92f));

                    foreach (var txt in cardT.GetComponentsInChildren<TMP_Text>(true))
                    {
                        if (txt.name.Contains("Title") && txt.transform.parent == cardT)
                        {
                            txt.color = Ink; txt.fontStyle = FontStyles.Bold; txt.characterSpacing = 16f;
                        }
                    }

                    var pageBar = cardT.Find("PageBar");
                    if (pageBar != null)
                    {
                        foreach (var btn in pageBar.GetComponentsInChildren<Button>(true))
                        {
                            SetImage(btn.GetComponent<Image>(), pill, Image.Type.Sliced, new Color(1, 1, 1, 0.9f));
                            var t = btn.GetComponentInChildren<TMP_Text>(true);
                            if (t != null) { t.color = UiBlue; t.fontStyle = FontStyles.Bold; }
                        }
                        foreach (var t in pageBar.GetComponentsInChildren<TMP_Text>(true))
                        {
                            if (t.GetComponentInParent<Button>() == null) { t.color = Ink; t.fontStyle = FontStyles.Bold; }
                        }
                    }

                    foreach (var btn in cardT.GetComponentsInChildren<Button>(true))
                    {
                        if (!btn.name.Contains("Close")) continue;
                        SetImage(btn.GetComponent<Image>(), pillAccent, Image.Type.Sliced, Color.white);
                        var t = btn.GetComponentInChildren<TMP_Text>(true);
                        if (t != null) { t.color = Color.white; t.fontStyle = FontStyles.Bold; }
                    }

                    EditorUtility.SetDirty(panel.gameObject);
                }
            }

            Debug.Log("[SaveLoadSkinner] Sorairo save/load skin applied.");
        }

        private static Sprite Load(string file) => AssetDatabase.LoadAssetAtPath<Sprite>(ThemeDir + file);

        private static void SetImage(Image img, Sprite sprite, Image.Type type, Color color)
        {
            if (img == null || sprite == null) return;
            img.sprite = sprite;
            img.type = type;
            img.color = color;
        }
    }
}
