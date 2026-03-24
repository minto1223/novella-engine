using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.Core
{
    /// <summary>
    /// NovellaUIThemeをシーン内のUIに適用するユーティリティ。
    /// NovellaEngine.Awake()後に自動適用される。
    /// </summary>
    public static class UIThemeApplicator
    {
        public static void Apply(NovellaUITheme theme, NovellaEngine engine)
        {
            if (theme == null) return;

            ApplyMessageWindow(theme, engine);
            ApplyChoiceUI(theme, engine);
            ApplyHUD(theme, engine);
            ApplyMenuPanel(theme, engine);
            ApplyBacklogPanel(theme, engine);
            ApplySavePanel(theme, engine);
            ApplySettingsPanel(theme, engine);
            ApplyFont(theme, engine);

            Debug.Log("[Novella] UI Theme applied.");
        }

        private static void ApplyMessageWindow(NovellaUITheme theme, NovellaEngine engine)
        {
            if (engine.MessageWindow == null) return;

            var panel = engine.MessageWindow.GetComponentInChildren<Image>(true);
            if (panel != null)
            {
                ApplyImageOrColor(panel, theme.MessageWindowImage, theme.MessageWindowBackground);
            }

            // テキスト色
            var texts = engine.MessageWindow.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in texts)
                t.color = theme.MessageTextColor;

            engine.IMessageWindow?.ApplyTextSpeed(theme.TextSpeed);
            engine.IMessageWindow?.ApplyFontSize(theme.FontSize);
        }

        private static void ApplyChoiceUI(NovellaUITheme theme, NovellaEngine engine)
        {
            if (engine.ChoiceUI == null) return;

            // ボタンPrefabに適用
            var buttons = engine.ChoiceUI.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                var img = btn.GetComponent<Image>();
                if (img != null)
                    ApplyImageOrColor(img, theme.ChoiceButtonImage, theme.ChoiceButtonColor);

                var txt = btn.GetComponentInChildren<TMP_Text>(true);
                if (txt != null) txt.color = theme.ChoiceTextColor;
            }
        }

        private static void ApplyHUD(NovellaUITheme theme, NovellaEngine engine)
        {
            // HUDはNovellaCanvas直下の"MiniHUD"を探す
            var canvas = FindCanvas(engine);
            if (canvas == null) return;

            var hud = canvas.transform.Find("MiniHUD");
            if (hud == null) return;

            var buttons = hud.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                var img = btn.GetComponent<Image>();
                if (img != null)
                    ApplyImageOrColor(img, theme.HUDButtonImage, theme.HUDButtonColor);

                var txt = btn.GetComponentInChildren<TMP_Text>(true);
                if (txt != null) txt.color = theme.HUDTextColor;
            }
        }

        private static void ApplyMenuPanel(NovellaUITheme theme, NovellaEngine engine)
        {
            if (engine.MenuUI == null) return;

            var menuMb = engine.MenuUI as MonoBehaviour;
            if (menuMb == null) return;

            var panelImg = FindPanelImage(menuMb.transform);
            if (panelImg != null)
                ApplyImageOrColor(panelImg, theme.MenuPanelImage, theme.MenuPanelColor);
        }

        private static void ApplyBacklogPanel(NovellaUITheme theme, NovellaEngine engine)
        {
            if (engine.BacklogUI == null) return;

            var panelImg = FindPanelImage(engine.BacklogUI.transform);
            if (panelImg != null)
                ApplyImageOrColor(panelImg, theme.BacklogPanelImage, theme.BacklogBackground);
        }

        private static void ApplySavePanel(NovellaUITheme theme, NovellaEngine engine)
        {
            if (theme.SavePanelImage == null) return;

            var canvas = FindCanvas(engine);
            if (canvas == null) return;

            // SavePanel / LoadPanel を探す
            foreach (var name in new[] { "SavePanel", "LoadPanel" })
            {
                var t = canvas.transform.Find(name);
                if (t == null) continue;
                var img = t.GetComponent<Image>();
                if (img != null)
                    ApplyImageOrColor(img, theme.SavePanelImage, img.color);
            }
        }

        private static void ApplySettingsPanel(NovellaUITheme theme, NovellaEngine engine)
        {
            if (theme.SettingsPanelImage == null) return;

            var canvas = FindCanvas(engine);
            if (canvas == null) return;

            var t = canvas.transform.Find("SettingsPanel");
            if (t == null) return;
            var img = t.GetComponent<Image>();
            if (img != null)
                ApplyImageOrColor(img, theme.SettingsPanelImage, img.color);
        }

        private static void ApplyFont(NovellaUITheme theme, NovellaEngine engine)
        {
            if (theme.Font == null) return;

            var canvas = FindCanvas(engine);
            if (canvas == null) return;

            var texts = canvas.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in texts)
                t.font = theme.Font;
        }

        // --- Helpers ---

        private static void ApplyImageOrColor(Image target, Sprite sprite, Color color)
        {
            if (sprite != null)
            {
                target.sprite = sprite;
                target.type = Image.Type.Sliced;
                target.color = Color.white;
            }
            else
            {
                target.color = color;
            }
        }

        private static Image FindPanelImage(Transform root)
        {
            // 直下の最初の非アクティブ含むImageを持つ子を探す（_panel パターン）
            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                var img = child.GetComponent<Image>();
                if (img != null) return img;
            }
            // 自身
            return root.GetComponent<Image>();
        }

        private static Canvas FindCanvas(NovellaEngine engine)
        {
            Canvas canvas = null;
            if (engine.Background != null)
                canvas = engine.Background.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                var go = GameObject.Find("NovellaCanvas");
                if (go != null) canvas = go.GetComponent<Canvas>();
            }
            return canvas;
        }
    }
}
