using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Novella.Core;
using Novella.UI;

namespace NovellaEditor
{
    /// <summary>
    /// SettingsPanelにそらいろクオリアのスキンを一括適用するMCP作業用ツール。
    /// </summary>
    public static class SettingsPanelSkinner
    {
        private const string ThemeDir = "Assets/Novella/UI/Sprites/Theme/";
        private static readonly Color Ink = new Color(0.133f, 0.22f, 0.29f, 1f);
        private static readonly Color UiBlue = new Color(0.239f, 0.416f, 0.541f, 1f);
        private static readonly Color SubBlue = new Color(0.357f, 0.498f, 0.608f, 1f);
        private static readonly Color Accent = new Color(0.302f, 0.639f, 0.851f, 1f);

        [MenuItem("Tools/Novella/Apply Sorairo Settings Skin")]
        public static void Apply()
        {
            // どのシーンでも動くよう、全ルートCanvas直下からSettingsPanelを探す（非アクティブ含む）
            Transform panel = null;
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!canvas.isRootCanvas) continue;
                panel = canvas.transform.Find("SettingsPanel");
                if (panel != null) break;
            }
            if (panel == null) { Debug.LogError("[Skinner] SettingsPanel not found"); return; }

            var bg = Load("bg_sky_gradient.png");
            var card = Load("panel_card.png");
            var track = Load("slider_track.png");
            var fill = Load("slider_fill.png");
            var knob = Load("knob.png");
            var tOff = Load("toggle_track_off.png");
            var tOn = Load("toggle_track_on.png");
            var pill = Load("button_pill.png");
            var pillAccent = Load("button_pill_accent.png");

            Undo.RegisterFullObjectHierarchyUndo(panel.gameObject, "Sorairo Settings Skin");

            SetImage(panel.GetComponent<Image>(), bg, Image.Type.Simple, new Color(1, 1, 1, 0.97f));
            var cardT = panel.Find("SettingsCard");
            SetImage(cardT.GetComponent<Image>(), card, Image.Type.Sliced, new Color(1, 1, 1, 0.92f));

            var title = cardT.Find("SettingsTitle")?.GetComponent<TMP_Text>();
            if (title != null) { title.color = Ink; title.fontStyle = FontStyles.Bold; }

            // タブ: ピル化（アクティブ色は実行時にSettingsUIControllerが上書きするためApplyThemeも呼ぶ）
            foreach (var tabName in new[] { "TabRow/GameTabButton", "TabRow/SoundTabButton" })
            {
                var tab = cardT.Find(tabName);
                if (tab == null) continue;
                SetImage(tab.GetComponent<Image>(), pill, Image.Type.Sliced, Color.white);
                var txt = tab.GetComponentInChildren<TMP_Text>(true);
                if (txt != null) { txt.fontStyle = FontStyles.Bold; txt.characterSpacing = 12f; }
            }

            // スライダー・トグル・ラベルを全タブまとめてスキン
            foreach (var slider in panel.GetComponentsInChildren<Slider>(true))
            {
                var sbg = slider.transform.Find("Background")?.GetComponent<Image>();
                SetImage(sbg, track, Image.Type.Sliced, Color.white);
                var sfill = slider.fillRect != null ? slider.fillRect.GetComponent<Image>() : null;
                SetImage(sfill, fill, Image.Type.Sliced, Color.white);
                var handle = slider.targetGraphic as Image;
                if (handle == null && slider.handleRect != null) handle = slider.handleRect.GetComponent<Image>();
                SetImage(handle, knob, Image.Type.Simple, Color.white);
                if (slider.handleRect != null) slider.handleRect.sizeDelta = new Vector2(28, 28);
            }

            foreach (var toggle in panel.GetComponentsInChildren<Toggle>(true))
            {
                var tbg = toggle.transform.Find("Background")?.GetComponent<Image>();
                if (tbg != null)
                {
                    SetImage(tbg, tOff, Image.Type.Simple, Color.white);
                    tbg.rectTransform.sizeDelta = new Vector2(58, 30);
                }
                var check = toggle.graphic as Image;
                if (check != null)
                {
                    SetImage(check, tOn, Image.Type.Simple, Color.white);
                    var rt = check.rectTransform;
                    rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                }
            }

            // ラベル・セクション見出し
            foreach (var txt in panel.GetComponentsInChildren<TMP_Text>(true))
            {
                if (txt.transform.parent != null && txt.transform.parent.name.StartsWith("Section_"))
                {
                    txt.color = SubBlue; txt.fontStyle = FontStyles.Bold; txt.characterSpacing = 16f;
                }
                else if (txt.name == "Label")
                {
                    txt.color = Ink;
                }
            }
            foreach (var img in panel.GetComponentsInChildren<Image>(true))
            {
                if (img.name == "Line" && img.transform.parent.name.StartsWith("Section_"))
                    img.color = new Color(Accent.r, Accent.g, Accent.b, 0.25f);
            }

            // フッター: リセット=控えめピル、閉じる=グラデピル
            var reset = cardT.Find("ButtonRow/ResetButton");
            if (reset != null)
            {
                SetImage(reset.GetComponent<Image>(), pill, Image.Type.Sliced, new Color(1, 1, 1, 0.7f));
                var nb = reset.GetComponent<NovellaButton>() ?? Undo.AddComponent<NovellaButton>(reset.gameObject);
                var danger = AssetDatabase.LoadAssetAtPath<NovellaButtonStyle>("Assets/Novella/Data/DangerButtonStyle.asset");
                new SerializedObject(nb).FindPropertyOrThrow("_style", danger);
                var txt = reset.GetComponentInChildren<TMP_Text>(true);
                if (txt != null) { txt.color = SubBlue; txt.fontStyle = FontStyles.Bold; }
            }
            var close = cardT.Find("ButtonRow/SettingsCloseButton");
            if (close != null)
            {
                SetImage(close.GetComponent<Image>(), pillAccent, Image.Type.Sliced, Color.white);
                var txt = close.GetComponentInChildren<TMP_Text>(true);
                if (txt != null) { txt.color = Color.white; txt.fontStyle = FontStyles.Bold; }
            }

            // タブのアクティブ/非アクティブ色をテーマ準拠に
            var settingsUI = Object.FindFirstObjectByType<SettingsUIController>(FindObjectsInactive.Include);
            settingsUI?.ApplyTheme(Accent, new Color(1, 1, 1, 0.8f), Color.white, UiBlue);

            EditorUtility.SetDirty(panel.gameObject);
            Debug.Log("[Skinner] Sorairo settings skin applied.");
        }

        private static Sprite Load(string file) => AssetDatabase.LoadAssetAtPath<Sprite>(ThemeDir + file);

        private static void SetImage(Image img, Sprite sprite, Image.Type type, Color color)
        {
            if (img == null || sprite == null) return;
            img.sprite = sprite;
            img.type = type;
            img.color = color;
        }

        private static void FindPropertyOrThrow(this SerializedObject so, string prop, Object value)
        {
            var p = so.FindProperty(prop);
            if (p == null) { Debug.LogWarning($"[Skinner] property {prop} not found"); return; }
            p.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
