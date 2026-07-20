using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Novella.UI;

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
                if (TryApplyButtonStyle(btn, theme.PrimaryButtonStyle)) continue;

                var img = btn.GetComponent<Image>();
                if (img != null)
                    ApplyImageOrColor(img, theme.ChoiceButtonImage, theme.ChoiceButtonColor);

                var txt = btn.GetComponentInChildren<TMP_Text>(true);
                if (txt != null) txt.color = theme.ChoiceTextColor;
            }
        }

        private static void ApplyHUD(NovellaUITheme theme, NovellaEngine engine)
        {
            // HUDはNovellaCanvas（またはCameraRoot）直下の"HUDPanel"を探す
            var hud = FindPanel(engine, "HUDPanel");
            if (hud == null) return;

            var buttons = hud.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                if (TryApplyButtonStyle(btn, theme.IconButtonStyle)) continue;

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

            // SavePanel / LoadPanel を探す
            foreach (var name in new[] { "SavePanel", "LoadPanel" })
            {
                var t = FindPanel(engine, name);
                if (t == null) continue;
                var img = t.GetComponent<Image>();
                if (img != null)
                    ApplyImageOrColor(img, theme.SavePanelImage, img.color);
            }
        }

        private static void ApplySettingsPanel(NovellaUITheme theme, NovellaEngine engine)
        {
            if (theme.SettingsPanelImage != null)
            {
                var t = FindPanel(engine, "SettingsPanel");
                var img = t != null ? t.GetComponent<Image>() : null;
                if (img != null)
                    ApplyImageOrColor(img, theme.SettingsPanelImage, img.color);
            }

            var settingsUI = engine.GetComponent<SettingsUIController>();
            settingsUI?.ApplyTheme(theme.SettingsTabActiveColor, theme.SettingsTabInactiveColor,
                theme.SettingsTabActiveTextColor, theme.SettingsTabInactiveTextColor);

            var confirmDialog = Object.FindFirstObjectByType<ConfirmDialogController>();
            confirmDialog?.ApplyTheme(theme.ConfirmDialogBackground, theme.ConfirmDialogTextColor,
                theme.ConfirmDialogYesButtonColor, theme.ConfirmDialogNoButtonColor, theme.ConfirmDialogButtonTextColor);
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

        /// <summary>
        /// NovellaButtonが付いたボタンにはスタイル経由で見た目を適用する。
        /// 適用できた場合true（呼び出し側はフラット色適用をスキップする）。
        /// スタイルがボタン側にもテーマ側にも無い場合はfalse（従来のフラット色適用に任せる）。
        /// </summary>
        private static bool TryApplyButtonStyle(Button btn, NovellaButtonStyle style)
        {
            var novellaBtn = btn.GetComponent<NovellaButton>();
            if (novellaBtn == null) return false;
            if (!novellaBtn.HasStyle)
            {
                if (style == null) return false;
                novellaBtn.SetStyle(style);
            }
            return true;
        }

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

        /// <summary>
        /// パネル検索のルートを取得する。CameraRootラッパーがあればそちらを、無ければCanvas自体を返す。
        /// </summary>
        private static Transform FindContainer(NovellaEngine engine)
        {
            var canvas = FindCanvas(engine);
            if (canvas == null) return null;
            var cameraRoot = canvas.transform.Find("CameraRoot");
            return cameraRoot != null ? cameraRoot : canvas.transform;
        }

        /// <summary>
        /// 名前でUIパネルを探す。CameraRoot配下→Canvas直下の順で探索する。
        /// （HUDPanel等のUIパネルはズーム対象外としてCameraRootの外に置かれているため、
        /// CameraRootのみの探索では見つからない）
        /// </summary>
        private static Transform FindPanel(NovellaEngine engine, string name)
        {
            var canvas = FindCanvas(engine);
            if (canvas == null) return null;
            var cameraRoot = canvas.transform.Find("CameraRoot");
            var t = cameraRoot != null ? cameraRoot.Find(name) : null;
            return t != null ? t : canvas.transform.Find(name);
        }
    }
}
