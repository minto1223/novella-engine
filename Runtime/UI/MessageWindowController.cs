using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.UI
{
    public class MessageWindowController : MonoBehaviour, Novella.Core.IMessageWindow
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _characterNameText;
        [SerializeField] private TMP_Text _dialogueText;
        [SerializeField] private float _charsPerSecond = 40f;

        [Header("既読テキスト色")]
        [SerializeField] private Color _readTextColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        [SerializeField] private Color _unreadTextColor = Color.white;

        [Header("NVL Mode")]
        [SerializeField] private Color _nvlBgColor = new Color(0, 0, 0, 0.85f);

        private Coroutine _typingCoroutine;
        private Action _onTypingComplete;
        private Image _panelImage;

        // ADV/NVLモード
        private Novella.Core.DisplayMode _displayMode = Novella.Core.DisplayMode.ADV;
        private readonly StringBuilder _nvlBuffer = new StringBuilder();
        private RectTransform _panelRect;
        private Vector2 _advAnchorMin;
        private Vector2 _advAnchorMax;
        private Vector2 _advSizeDelta;
        private Vector2 _advAnchoredPos;
        private Color _advBgColor;
        private bool _advLayoutSaved;
        private TextAlignmentOptions _advTextAlign;
        private TextOverflowModes _advOverflowMode;

        // インラインコマンドのパターン: {w:0.5} {s:20} {sr}
        private static readonly Regex InlineCommandRegex =
            new Regex(@"\{(w|s|sr)(?::([^}]*))?\}", RegexOptions.Compiled);

        // ルビタグ: {rb:漢字,かんじ}
        private static readonly Regex RubyRegex =
            new Regex(@"\{rb:([^,}]+),([^}]+)\}", RegexOptions.Compiled);

        // ルビブロック情報（タイプライター用）
        private readonly System.Collections.Generic.List<RubyBlock> _rubyBlocks =
            new System.Collections.Generic.List<RubyBlock>();

        private struct RubyBlock
        {
            public int startVisibleIndex;
            public int endVisibleIndex; // exclusive
        }

        public bool IsTyping { get; private set; }

        private void Awake()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
                _panelImage = _panel.GetComponent<Image>();
                _panelRect = _panel.GetComponent<RectTransform>();
            }
        }

        public void Show(string characterName, string text, Action onTypingComplete, bool isRead = false)
        {
            if (_panel != null) _panel.SetActive(true);

            if (_displayMode == Novella.Core.DisplayMode.NVL)
                ShowNvl(characterName, text, onTypingComplete, isRead);
            else
                ShowAdv(characterName, text, onTypingComplete, isRead);
        }

        private void ShowAdv(string characterName, string text, Action onTypingComplete, bool isRead)
        {
            if (_characterNameText != null) _characterNameText.text = characterName ?? "";
            if (_dialogueText != null) _dialogueText.color = isRead ? _readTextColor : _unreadTextColor;

            _onTypingComplete = onTypingComplete;

            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            _typingCoroutine = StartCoroutine(TypeText(text ?? ""));
        }

        private void ShowNvl(string characterName, string text, Action onTypingComplete, bool isRead)
        {
            if (_characterNameText != null) _characterNameText.text = "";

            // NVLバッファにテキストを蓄積
            if (_nvlBuffer.Length > 0) _nvlBuffer.Append("\n");
            if (!string.IsNullOrEmpty(characterName))
                _nvlBuffer.Append($"{characterName}: ");
            _nvlBuffer.Append(text ?? "");

            if (_dialogueText != null)
                _dialogueText.color = isRead ? _readTextColor : _unreadTextColor;

            _onTypingComplete = onTypingComplete;

            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            _typingCoroutine = StartCoroutine(TypeTextNvl(text ?? "", _nvlBuffer.ToString()));
        }

        private IEnumerator TypeTextNvl(string newText, string fullText)
        {
            IsTyping = true;

            string processedFull = ProcessRubyTags(fullText);
            string processedNew = ProcessRubyTags(newText);

            var parsed = ParseInlineCommands(processedFull);
            _dialogueText.text = parsed.displayText;

            // 新しいテキスト部分だけタイプライター表示
            var parsedNew = ParseInlineCommands(processedNew);
            int newVisibleLen = CountVisibleChars(parsedNew.displayText);
            int totalVisible = CountVisibleChars(parsed.displayText);
            int alreadyVisible = totalVisible - newVisibleLen;

            _dialogueText.maxVisibleCharacters = alreadyVisible;

            float currentSpeed = _charsPerSecond;
            int visibleIndex = alreadyVisible;

            for (int i = 0; i < parsedNew.segments.Length; i++)
            {
                var seg = parsedNew.segments[i];
                if (seg.isCommand)
                {
                    switch (seg.commandType)
                    {
                        case "w":
                            float waitTime = 0.5f;
                            if (float.TryParse(seg.commandValue, System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out float w))
                                waitTime = w;
                            yield return new WaitForSeconds(waitTime);
                            break;
                        case "s":
                            if (float.TryParse(seg.commandValue, System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out float s))
                                currentSpeed = Mathf.Max(1f, s);
                            break;
                        case "sr":
                            currentSpeed = _charsPerSecond;
                            break;
                    }
                }
                else
                {
                    float interval = 1f / currentSpeed;
                    for (int c = 0; c < seg.text.Length; c++)
                    {
                        visibleIndex++;
                        _dialogueText.maxVisibleCharacters = visibleIndex;
                        yield return new WaitForSeconds(interval);
                    }
                }
            }

            _dialogueText.maxVisibleCharacters = int.MaxValue;
            IsTyping = false;
            _typingCoroutine = null;
            _onTypingComplete?.Invoke();
            _onTypingComplete = null;
        }

        public void SkipTyping()
        {
            if (!IsTyping) return;
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
            IsTyping = false;
            if (_dialogueText != null) _dialogueText.maxVisibleCharacters = int.MaxValue;
            _onTypingComplete?.Invoke();
            _onTypingComplete = null;
        }

        private IEnumerator TypeText(string rawText)
        {
            IsTyping = true;

            // ルビタグをTMProリッチテキストに変換
            rawText = ProcessRubyTags(rawText);

            // インラインコマンドを解析して、表示用テキストとコマンドリストに分離
            var parsed = ParseInlineCommands(rawText);
            _dialogueText.text = parsed.displayText;
            _dialogueText.maxVisibleCharacters = 0;

            float currentSpeed = _charsPerSecond;
            int visibleIndex = 0;

            for (int i = 0; i < parsed.segments.Length; i++)
            {
                var seg = parsed.segments[i];

                if (seg.isCommand)
                {
                    // インラインコマンドを実行
                    switch (seg.commandType)
                    {
                        case "w":
                            float waitTime = 0.5f;
                            if (float.TryParse(seg.commandValue, System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out float w))
                                waitTime = w;
                            yield return new WaitForSeconds(waitTime);
                            break;
                        case "s":
                            if (float.TryParse(seg.commandValue, System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out float s))
                                currentSpeed = Mathf.Max(1f, s);
                            break;
                        case "sr":
                            currentSpeed = _charsPerSecond;
                            break;
                    }
                }
                else
                {
                    // 通常テキストを1文字ずつ表示（ルビブロックは一括表示）
                    float interval = 1f / currentSpeed;
                    for (int c = 0; c < seg.text.Length; c++)
                    {
                        visibleIndex++;

                        // ルビブロック内の場合、ブロック末尾まで一括で進める
                        int skipTo = GetRubyBlockEnd(visibleIndex);
                        if (skipTo > visibleIndex)
                        {
                            int charsInBlock = skipTo - visibleIndex + 1;
                            // seg.text内の対応分もスキップ
                            c += charsInBlock - 1;
                            visibleIndex = skipTo;
                        }

                        _dialogueText.maxVisibleCharacters = visibleIndex;
                        yield return new WaitForSeconds(interval);
                    }
                }
            }

            _dialogueText.maxVisibleCharacters = int.MaxValue;
            IsTyping = false;
            _typingCoroutine = null;
            _onTypingComplete?.Invoke();
            _onTypingComplete = null;
        }

        private ParsedText ParseInlineCommands(string rawText)
        {
            var segments = new System.Collections.Generic.List<TextSegment>();
            var displayBuilder = new StringBuilder();
            int lastIndex = 0;

            foreach (Match match in InlineCommandRegex.Matches(rawText))
            {
                // コマンドの前のテキスト部分
                if (match.Index > lastIndex)
                {
                    string textPart = rawText.Substring(lastIndex, match.Index - lastIndex);
                    segments.Add(new TextSegment { text = textPart });
                    displayBuilder.Append(textPart);
                }

                // コマンド部分（表示テキストには含めない）
                segments.Add(new TextSegment
                {
                    isCommand = true,
                    commandType = match.Groups[1].Value,
                    commandValue = match.Groups[2].Success ? match.Groups[2].Value : ""
                });

                lastIndex = match.Index + match.Length;
            }

            // 残りのテキスト
            if (lastIndex < rawText.Length)
            {
                string remaining = rawText.Substring(lastIndex);
                segments.Add(new TextSegment { text = remaining });
                displayBuilder.Append(remaining);
            }

            return new ParsedText
            {
                displayText = displayBuilder.ToString(),
                segments = segments.ToArray()
            };
        }

        public void ApplyTextSpeed(float charsPerSecond)
        {
            _charsPerSecond = Mathf.Max(1f, charsPerSecond);
        }

        public void ApplyFontSize(float size)
        {
            if (_dialogueText != null) _dialogueText.fontSize = size;
        }

        public void ApplyWindowOpacity(float opacity)
        {
            if (_panelImage != null)
            {
                var c = _panelImage.color;
                c.a = Mathf.Clamp01(opacity);
                _panelImage.color = c;
            }
        }

        public void Hide()
        {
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            IsTyping = false;
            if (_panel != null) _panel.SetActive(false);
        }

        /// <summary>
        /// パネルの表示/非表示を切り替える（ToggleWindowHide用）。
        /// Hide()と違いタイピング状態をリセットしない。
        /// </summary>
        public void SetPanelVisible(bool visible)
        {
            if (_panel != null) _panel.SetActive(visible);
        }

        public void SetDisplayMode(Novella.Core.DisplayMode mode)
        {
            if (_displayMode == mode) return;

            SaveAdvLayout();

            _displayMode = mode;
            _nvlBuffer.Clear();

            if (mode == Novella.Core.DisplayMode.NVL)
                ApplyNvlLayout();
            else
                RestoreAdvLayout();

            Debug.Log($"[Novella] Display mode: {mode}");
        }

        public void ClearNvlText()
        {
            _nvlBuffer.Clear();
            if (_dialogueText != null)
            {
                _dialogueText.text = "";
                _dialogueText.maxVisibleCharacters = int.MaxValue;
            }
        }

        private void SaveAdvLayout()
        {
            if (_advLayoutSaved || _panelRect == null) return;
            _advAnchorMin = _panelRect.anchorMin;
            _advAnchorMax = _panelRect.anchorMax;
            _advSizeDelta = _panelRect.sizeDelta;
            _advAnchoredPos = _panelRect.anchoredPosition;
            if (_panelImage != null) _advBgColor = _panelImage.color;
            if (_dialogueText != null)
            {
                _advTextAlign = _dialogueText.alignment;
                _advOverflowMode = _dialogueText.overflowMode;
            }
            _advLayoutSaved = true;
        }

        private void ApplyNvlLayout()
        {
            if (_panelRect != null)
            {
                _panelRect.anchorMin = Vector2.zero;
                _panelRect.anchorMax = Vector2.one;
                _panelRect.sizeDelta = Vector2.zero;
                _panelRect.anchoredPosition = Vector2.zero;
            }
            if (_panelImage != null) _panelImage.color = _nvlBgColor;
            if (_dialogueText != null)
            {
                _dialogueText.alignment = TextAlignmentOptions.TopLeft;
                _dialogueText.overflowMode = TextOverflowModes.ScrollRect;
            }
            if (_characterNameText != null) _characterNameText.text = "";
        }

        private void RestoreAdvLayout()
        {
            if (!_advLayoutSaved) return;
            if (_panelRect != null)
            {
                _panelRect.anchorMin = _advAnchorMin;
                _panelRect.anchorMax = _advAnchorMax;
                _panelRect.sizeDelta = _advSizeDelta;
                _panelRect.anchoredPosition = _advAnchoredPos;
            }
            if (_panelImage != null) _panelImage.color = _advBgColor;
            if (_dialogueText != null)
            {
                _dialogueText.alignment = _advTextAlign;
                _dialogueText.overflowMode = _advOverflowMode;
            }
        }

        /// <summary>ルビタグ {rb:漢字,かんじ} をTMProリッチテキストに変換し、ブロック情報を記録</summary>
        private string ProcessRubyTags(string text)
        {
            _rubyBlocks.Clear();
            if (!Novella.Core.RubyProcessor.HasRuby(text)) return text;

            float fontSize = _dialogueText != null ? _dialogueText.fontSize : 46f;

            // ルビブロックの可視文字範囲を記録（タイプライター一括表示用）
            int visibleCount = 0;
            int lastIdx = 0;
            foreach (Match m in RubyRegex.Matches(text))
            {
                if (m.Index > lastIdx)
                {
                    string before = text.Substring(lastIdx, m.Index - lastIdx);
                    visibleCount += CountVisibleChars(before);
                }
                string baseText = m.Groups[1].Value;
                string rubyText = m.Groups[2].Value;
                int blockStart = visibleCount + 1;
                int blockEnd = visibleCount + rubyText.Length + baseText.Length;
                _rubyBlocks.Add(new RubyBlock { startVisibleIndex = blockStart, endVisibleIndex = blockEnd });
                visibleCount += rubyText.Length + baseText.Length;
                lastIdx = m.Index + m.Length;
            }

            return Novella.Core.RubyProcessor.Convert(text, fontSize);
        }

        /// <summary>TMProリッチテキストタグを除いた可視文字数を概算</summary>
        private static int CountVisibleChars(string text)
        {
            bool inTag = false;
            int count = 0;
            foreach (char c in text)
            {
                if (c == '<') { inTag = true; continue; }
                if (c == '>') { inTag = false; continue; }
                if (!inTag) count++;
            }
            return count;
        }

        /// <summary>指定の可視インデックスがルビブロック内ならブロック末尾を返す</summary>
        private int GetRubyBlockEnd(int visibleIndex)
        {
            foreach (var block in _rubyBlocks)
            {
                if (visibleIndex >= block.startVisibleIndex && visibleIndex <= block.endVisibleIndex)
                    return block.endVisibleIndex;
            }
            return visibleIndex;
        }

        private struct TextSegment
        {
            public string text;
            public bool isCommand;
            public string commandType;
            public string commandValue;
        }

        private struct ParsedText
        {
            public string displayText;
            public TextSegment[] segments;
        }
    }
}
