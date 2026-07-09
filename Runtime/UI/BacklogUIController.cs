using System;
using System.Collections.Generic;
using Novella.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.UI
{
    public class BacklogUIController : MonoBehaviour, Novella.Core.IBacklogUI
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _entryContainer;
        [SerializeField] private GameObject _entryPrefab;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private TMP_InputField _searchInput;
        [SerializeField] private Color _voicePlayingColor = new Color(0.3f, 0.8f, 1f, 0.25f);

        private bool _isOpen;
        public bool IsOpen => _isOpen;

        private Action<string, int> _onJump;
        private IReadOnlyList<BacklogEntry> _lastEntries;
        private string _currentSearchQuery = "";
        private NovellaEngine _engine;

        private string _currentPlayingClip;
        private Image _currentPlayingBg;
        private Color _defaultEntryBgColor;
        private bool _defaultColorCaptured;

        private const string HighlightColor = "#FFD700"; // ゴールド

        private void Awake()
        {
            if (_panel == null) _panel = gameObject;
            if (_scrollRect == null) _scrollRect = GetComponentInChildren<ScrollRect>();
            if (_entryContainer == null && _scrollRect != null) _entryContainer = _scrollRect.content;

            if (_scrollRect != null)
            {
                if (_scrollRect.viewport == null)
                {
                    var vp = _scrollRect.transform.Find("Viewport");
                    if (vp != null) _scrollRect.viewport = vp as RectTransform ?? vp.GetComponent<RectTransform>();
                }
                if (_scrollRect.content == null && _entryContainer != null)
                {
                    _scrollRect.content = _entryContainer as RectTransform
                        ?? _entryContainer.GetComponent<RectTransform>();
                }
            }

            if (_searchInput != null)
                _searchInput.onValueChanged.AddListener(OnSearchChanged);

            if (_panel != null) _panel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B) || Input.mouseScrollDelta.y > 0.5f)
            {
                Toggle();
                return;
            }

            if (!_isOpen) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
                return;
            }

            if (Input.GetMouseButtonDown(1) && !Novella.Core.UIInputUtil.IsPointerOverInteractableUI())
                Close();
        }

        public void Toggle()
        {
            if (_isOpen) Close();
            else Open();
        }

        public void Open()
        {
            _isOpen = true;
            _panel.SetActive(true);
            // 検索をクリア
            if (_searchInput != null)
            {
                _searchInput.text = "";
                _currentSearchQuery = "";
            }
            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = 0f;
        }

        public void Close()
        {
            _isOpen = false;
            _panel.SetActive(false);
            // バックログ閉じたらボイス停止
            if (_currentPlayingClip != null)
            {
                _engine?.IAudio?.StopVoice();
                ClearVoiceHighlight();
            }
        }

        public void SetJumpCallback(Action<string, int> onJump)
        {
            _onJump = onJump;
        }

        public void SetEngine(NovellaEngine engine)
        {
            _engine = engine;
        }

        private void OnSearchChanged(string query)
        {
            _currentSearchQuery = query;
            if (_lastEntries != null)
                RebuildFiltered(_lastEntries, query);
        }

        public void Rebuild(IReadOnlyList<BacklogEntry> entries)
        {
            _lastEntries = entries;
            RebuildFiltered(entries, _currentSearchQuery);
        }

        private void RebuildFiltered(IReadOnlyList<BacklogEntry> entries, string query)
        {
            foreach (Transform child in _entryContainer)
                Destroy(child.gameObject);

            bool hasQuery = !string.IsNullOrEmpty(query);
            string queryLower = hasQuery ? query.ToLower() : "";

            foreach (var entry in entries)
            {
                // 検索フィルタ
                if (hasQuery)
                {
                    bool matchChar = !string.IsNullOrEmpty(entry.CharacterName)
                        && entry.CharacterName.ToLower().Contains(queryLower);
                    bool matchText = !string.IsNullOrEmpty(entry.Text)
                        && entry.Text.ToLower().Contains(queryLower);
                    if (!matchChar && !matchText) continue;
                }

                var go = Instantiate(_entryPrefab, _entryContainer);

                var charNameT = go.transform.Find("TextColumn/CharNameText");
                var dialogueT = go.transform.Find("TextColumn/DialogueText");

                if (charNameT != null && dialogueT != null)
                {
                    var charTMP = charNameT.GetComponent<TMP_Text>();
                    var dlgTMP = dialogueT.GetComponent<TMP_Text>();

                    string charText = string.IsNullOrEmpty(entry.CharacterName) ? "" : entry.CharacterName;
                    string dlgText = entry.Text ?? "";

                    if (hasQuery)
                    {
                        charText = HighlightMatch(charText, query);
                        dlgText = HighlightMatch(dlgText, query);
                    }

                    if (charTMP != null)
                    {
                        charTMP.text = charText;
                        if (!string.IsNullOrEmpty(entry.NameColorHex) && !hasQuery)
                            charTMP.text = $"<color={entry.NameColorHex}>{charText}</color>";
                    }
                    if (dlgTMP != null)
                    {
                        float fontSize = dlgTMP.fontSize;
                        dlgTMP.text = Novella.Core.RubyProcessor.Convert(dlgText, fontSize, dlgTMP.font);
                    }

                    if (string.IsNullOrEmpty(entry.CharacterName))
                        charNameT.gameObject.SetActive(false);
                }
                else
                {
                    var texts = go.GetComponentsInChildren<TMP_Text>();
                    if (texts.Length >= 2)
                    {
                        texts[0].text = string.IsNullOrEmpty(entry.CharacterName) ? "" : entry.CharacterName;
                        texts[1].text = Novella.Core.RubyProcessor.Convert(entry.Text, texts[1].fontSize, texts[1].font);
                    }
                }

                var jumpBtnT = go.transform.Find("JumpButton");
                var jumpBtn = jumpBtnT != null ? jumpBtnT.GetComponent<Button>() : go.GetComponentInChildren<Button>();
                if (jumpBtn != null && entry.CommandIndex >= 0 && !string.IsNullOrEmpty(entry.ScriptPath))
                {
                    var scriptPath = entry.ScriptPath;
                    var cmdIndex = entry.CommandIndex;
                    jumpBtn.onClick.AddListener(() => OnJumpClicked(scriptPath, cmdIndex));
                }
                else if (jumpBtn != null)
                {
                    jumpBtn.gameObject.SetActive(false);
                }

                // ボイス再生ボタン
                var voiceBtnT = go.transform.Find("VoiceButton");
                if (!string.IsNullOrEmpty(entry.VoiceClip) && _engine != null)
                {
                    if (voiceBtnT == null && jumpBtnT != null)
                    {
                        var voiceGO = Instantiate(jumpBtnT.gameObject, go.transform);
                        voiceGO.name = "VoiceButton";
                        voiceBtnT = voiceGO.transform;
                    }
                    if (voiceBtnT != null)
                    {
                        voiceBtnT.gameObject.SetActive(true);
                        var vTmp = voiceBtnT.GetComponentInChildren<TMP_Text>();
                        if (vTmp != null) vTmp.text = ">";
                        var vBtn = voiceBtnT.GetComponent<Button>();
                        if (vBtn != null)
                        {
                            vBtn.onClick.RemoveAllListeners();
                            string clip = entry.VoiceClip;
                            var entryBg = go.GetComponent<Image>();
                            vBtn.onClick.AddListener(() => OnVoiceClicked(clip, entryBg));
                        }
                    }
                }
                else if (voiceBtnT != null)
                {
                    voiceBtnT.gameObject.SetActive(false);
                }
            }

            Canvas.ForceUpdateCanvases();
            if (_scrollRect != null)
                _scrollRect.verticalNormalizedPosition = hasQuery ? 1f : 0f;
        }

        private static string HighlightMatch(string text, string query)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(query)) return text;

            int idx = 0;
            string textLower = text.ToLower();
            string queryLower = query.ToLower();
            var sb = new System.Text.StringBuilder();

            while (idx < text.Length)
            {
                int found = textLower.IndexOf(queryLower, idx, StringComparison.Ordinal);
                if (found < 0)
                {
                    sb.Append(text, idx, text.Length - idx);
                    break;
                }
                sb.Append(text, idx, found - idx);
                sb.Append($"<color={HighlightColor}>");
                sb.Append(text, found, query.Length);
                sb.Append("</color>");
                idx = found + query.Length;
            }
            return sb.ToString();
        }

        private void OnVoiceClicked(string clip, Image entryBg)
        {
            if (_engine?.IAudio == null) return;

            // 再クリックで停止
            if (_currentPlayingClip == clip && _engine.IAudio.IsVoicePlaying)
            {
                _engine.IAudio.StopVoice();
                ClearVoiceHighlight();
                return;
            }

            // 別のボイスを停止してから再生
            _engine.IAudio.StopVoice();
            ClearVoiceHighlight();

            _engine.IAudio.PlayVoice(clip, 1f, null);
            _currentPlayingClip = clip;

            // ハイライト
            if (entryBg != null)
            {
                if (!_defaultColorCaptured)
                {
                    _defaultEntryBgColor = entryBg.color;
                    _defaultColorCaptured = true;
                }
                _currentPlayingBg = entryBg;
                entryBg.color = _voicePlayingColor;
            }
        }

        private void ClearVoiceHighlight()
        {
            if (_currentPlayingBg != null && _defaultColorCaptured)
            {
                _currentPlayingBg.color = _defaultEntryBgColor;
                _currentPlayingBg = null;
            }
            _currentPlayingClip = null;
        }

        private void LateUpdate()
        {
            // ボイス再生終了時にハイライト自動解除
            if (_currentPlayingClip != null && _engine?.IAudio != null && !_engine.IAudio.IsVoicePlaying)
            {
                ClearVoiceHighlight();
            }
        }

        private void OnJumpClicked(string scriptPath, int commandIndex)
        {
            Close();
            _onJump?.Invoke(scriptPath, commandIndex);
        }
    }
}
