using System;
using Novella.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.Commands
{
    /// <summary>
    /// テキスト入力ダイアログを表示し、入力結果をフラグに保存する。
    /// JSON: { "type": "input_text", "target": "player_name", "text": "あなたの名前を入力してください" }
    /// - target: 保存先フラグ名（必須）
    /// - text: プロンプト文（任意、デフォルト: "入力してください"）
    /// - value: デフォルト値（任意）
    /// </summary>
    public class InputTextCommandHandler : ICommandHandler
    {
        public string CommandType => "input_text";

        private static GameObject _panelInstance;

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            string flagName = command.Target;
            if (string.IsNullOrEmpty(flagName))
            {
                Debug.LogWarning("[Novella] input_text: target (flag name) is required.");
                onComplete?.Invoke();
                return;
            }

            string prompt = !string.IsNullOrEmpty(command.Text) ? command.Text : "入力してください";
            string defaultValue = command.Value ?? "";

            // 既存のフラグ値があればデフォルト値にする
            string existingValue = engine.Flags.Get(flagName);
            if (!string.IsNullOrEmpty(existingValue) && string.IsNullOrEmpty(command.Value))
                defaultValue = existingValue;

            ShowInputDialog(engine, prompt, defaultValue, (result) =>
            {
                if (!string.IsNullOrEmpty(result))
                {
                    engine.Flags.Set(flagName, result);
                }
                onComplete?.Invoke();
            });
        }

        private void ShowInputDialog(NovellaEngine engine, string prompt, string defaultValue, Action<string> onSubmit)
        {
            // Canvasを取得
            Canvas canvas = null;
            var bg = engine.Background;
            if (bg != null) canvas = bg.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                var canvasGo = GameObject.Find("NovellaCanvas");
                if (canvasGo != null) canvas = canvasGo.GetComponent<Canvas>();
            }
            if (canvas == null)
            {
                Debug.LogError("[Novella] input_text: Canvas not found.");
                onSubmit?.Invoke(defaultValue);
                return;
            }

            // 既存パネルがあれば破棄
            if (_panelInstance != null) UnityEngine.Object.Destroy(_panelInstance);

            // パネル作成
            var panel = new GameObject("InputTextPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            panel.transform.SetAsLastSibling();
            _panelInstance = panel;

            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var panelImg = panel.GetComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.6f);
            panelImg.raycastTarget = true;

            // ダイアログボックス
            var dialog = new GameObject("Dialog", typeof(RectTransform), typeof(Image));
            dialog.transform.SetParent(panel.transform, false);
            var dialogRect = dialog.GetComponent<RectTransform>();
            dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.sizeDelta = new Vector2(500, 220);
            dialogRect.anchoredPosition = Vector2.zero;
            var dialogImg = dialog.GetComponent<Image>();
            dialogImg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // フォント取得
            TMP_FontAsset font = null;
            var existingTmp = canvas.GetComponentInChildren<TMP_Text>(true);
            if (existingTmp != null) font = existingTmp.font;

            // プロンプトテキスト
            var promptGo = new GameObject("PromptText", typeof(RectTransform), typeof(TextMeshProUGUI));
            promptGo.transform.SetParent(dialog.transform, false);
            var promptRect = promptGo.GetComponent<RectTransform>();
            promptRect.anchorMin = new Vector2(0, 0.7f);
            promptRect.anchorMax = new Vector2(1, 1f);
            promptRect.offsetMin = new Vector2(20, 0);
            promptRect.offsetMax = new Vector2(-20, -10);
            var promptTmp = promptGo.GetComponent<TextMeshProUGUI>();
            promptTmp.text = prompt;
            promptTmp.fontSize = 24;
            promptTmp.alignment = TextAlignmentOptions.Center;
            promptTmp.color = Color.white;
            if (font != null) promptTmp.font = font;

            // InputField
            var inputGo = new GameObject("InputField", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            inputGo.transform.SetParent(dialog.transform, false);
            var inputRect = inputGo.GetComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0.1f, 0.35f);
            inputRect.anchorMax = new Vector2(0.9f, 0.65f);
            inputRect.offsetMin = Vector2.zero;
            inputRect.offsetMax = Vector2.zero;
            var inputImg = inputGo.GetComponent<Image>();
            inputImg.color = new Color(0.25f, 0.25f, 0.3f, 1f);

            // InputField内のテキスト
            var textArea = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            textArea.transform.SetParent(inputGo.transform, false);
            var textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 2);
            textAreaRect.offsetMax = new Vector2(-10, -2);

            var inputTextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            inputTextGo.transform.SetParent(textArea.transform, false);
            var inputTextRect = inputTextGo.GetComponent<RectTransform>();
            inputTextRect.anchorMin = Vector2.zero;
            inputTextRect.anchorMax = Vector2.one;
            inputTextRect.offsetMin = Vector2.zero;
            inputTextRect.offsetMax = Vector2.zero;
            var inputTmp = inputTextGo.GetComponent<TextMeshProUGUI>();
            inputTmp.fontSize = 22;
            inputTmp.color = Color.white;
            if (font != null) inputTmp.font = font;

            // Placeholder
            var placeholderGo = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
            placeholderGo.transform.SetParent(textArea.transform, false);
            var phRect = placeholderGo.GetComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = Vector2.zero;
            phRect.offsetMax = Vector2.zero;
            var phTmp = placeholderGo.GetComponent<TextMeshProUGUI>();
            phTmp.text = "...";
            phTmp.fontSize = 22;
            phTmp.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            phTmp.fontStyle = FontStyles.Italic;
            if (font != null) phTmp.font = font;

            // TMP_InputField設定
            var inputField = inputGo.GetComponent<TMP_InputField>();
            inputField.textViewport = textAreaRect;
            inputField.textComponent = inputTmp;
            inputField.placeholder = phTmp;
            inputField.text = defaultValue;
            inputField.characterLimit = 20;
            inputField.contentType = TMP_InputField.ContentType.Standard;

            // OKボタン
            var okGo = new GameObject("OKButton", typeof(RectTransform), typeof(Image), typeof(Button));
            okGo.transform.SetParent(dialog.transform, false);
            var okRect = okGo.GetComponent<RectTransform>();
            okRect.anchorMin = new Vector2(0.3f, 0.05f);
            okRect.anchorMax = new Vector2(0.7f, 0.25f);
            okRect.offsetMin = Vector2.zero;
            okRect.offsetMax = Vector2.zero;
            var okImg = okGo.GetComponent<Image>();
            okImg.color = new Color(0.3f, 0.5f, 0.8f, 1f);

            var okTextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            okTextGo.transform.SetParent(okGo.transform, false);
            var okTextRect = okTextGo.GetComponent<RectTransform>();
            okTextRect.anchorMin = Vector2.zero;
            okTextRect.anchorMax = Vector2.one;
            okTextRect.offsetMin = Vector2.zero;
            okTextRect.offsetMax = Vector2.zero;
            var okTmp = okTextGo.GetComponent<TextMeshProUGUI>();
            okTmp.text = "OK";
            okTmp.fontSize = 22;
            okTmp.alignment = TextAlignmentOptions.Center;
            okTmp.color = Color.white;
            if (font != null) okTmp.font = font;

            var okBtn = okGo.GetComponent<Button>();
            okBtn.onClick.AddListener(() =>
            {
                string result = inputField.text;
                UnityEngine.Object.Destroy(panel);
                _panelInstance = null;
                onSubmit?.Invoke(result);
            });

            // Enterキーでも確定できるようにする
            inputField.onSubmit.AddListener((text) =>
            {
                UnityEngine.Object.Destroy(panel);
                _panelInstance = null;
                onSubmit?.Invoke(text);
            });

            // 入力フィールドにフォーカス
            inputField.Select();
            inputField.ActivateInputField();
        }
    }
}
