using System.Collections.Generic;
using Novella.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Novella.UI
{
    /// <summary>
    /// チャプター選択画面。カード型レイアウトで全チャプターを一覧表示。
    /// クリア状況・進捗率表示、シーンリプレイ対応。
    /// </summary>
    public class ChapterSelectUIController : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _listContainer;
        [SerializeField] private Button _closeButton;
        [SerializeField] private ChapterList _chapterList;
        [SerializeField] private TMP_FontAsset _font;

        [Header("Settings")]
        [SerializeField] private string _gameSceneName = "SampleScene";

        [Header("Colors")]
        [SerializeField] private Color _cardUnlocked = new Color(0.15f, 0.15f, 0.25f, 0.9f);
        [SerializeField] private Color _cardLocked = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        [SerializeField] private Color _cardCleared = new Color(0.1f, 0.2f, 0.15f, 0.9f);
        [SerializeField] private Color _badgeColor = new Color(0.3f, 0.5f, 0.8f, 1f);
        [SerializeField] private Color _badgeClearedColor = new Color(0.3f, 0.7f, 0.4f, 1f);

        public bool IsOpen => _panel != null && _panel.activeSelf;

        private int _expandedChapter = -1;

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        public void Open()
        {
            _expandedChapter = -1;
            BuildList();
            if (_panel != null)
            {
                _panel.SetActive(true);
                _panel.transform.SetAsLastSibling();
            }
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
        }

        public void Close()
        {
            if (_panel != null) _panel.SetActive(false);
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Close);
        }

        private void Update()
        {
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
                Close();
        }

        private void BuildList()
        {
            if (_listContainer == null) return;

            foreach (Transform child in _listContainer)
                Destroy(child.gameObject);

            if (_chapterList == null || _chapterList.Chapters.Length == 0)
            {
                CreateLabel("(No chapters defined)");
                return;
            }

            // 進捗ヘッダー
            CreateProgressHeader();

            for (int i = 0; i < _chapterList.Chapters.Length; i++)
            {
                var chapter = _chapterList.Chapters[i];
                string path = chapter.ResolvedPath;
                bool isCleared = SceneRecollectionManager.IsCleared(path);
                bool isUnlocked = (i == 0) || isCleared || IsChapterUnlocked(i);

                CreateChapterCard(chapter, i, isUnlocked, isCleared);

                // クリア済みで展開中なら、シーンリプレイ一覧を表示
                if (isCleared && _expandedChapter == i)
                    CreateSceneReplayList(chapter, i);
            }
        }

        private void CreateProgressHeader()
        {
            int total = _chapterList.Chapters.Length;
            int cleared = 0;
            foreach (var ch in _chapterList.Chapters)
            {
                if (SceneRecollectionManager.IsCleared(ch.ResolvedPath))
                    cleared++;
            }

            var go = new GameObject("ProgressHeader");
            go.transform.SetParent(_listContainer, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900, 50);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (_font != null) tmp.font = _font;
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.7f, 0.8f, 0.9f);

            float percent = total > 0 ? (float)cleared / total * 100f : 0f;
            tmp.text = $"Progress: {cleared} / {total}  ({percent:F0}%)";
        }

        private bool IsChapterUnlocked(int chapterIndex)
        {
            if (chapterIndex <= 0) return true;
            var prevChapter = _chapterList.Chapters[chapterIndex - 1];
            return SceneRecollectionManager.IsCleared(prevChapter.ResolvedPath);
        }

        private void CreateLabel(string text)
        {
            var go = new GameObject("EmptyLabel");
            go.transform.SetParent(_listContainer, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 32;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.6f, 0.6f, 0.6f);
            if (_font != null) tmp.font = _font;
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(800, 60);
        }

        private void CreateChapterCard(ChapterEntry chapter, int index, bool unlocked, bool cleared)
        {
            int number = index + 1;

            var go = new GameObject($"Chapter_{number}");
            go.transform.SetParent(_listContainer, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900, 110);

            // カード背景
            var img = go.AddComponent<Image>();
            img.color = cleared ? _cardCleared : (unlocked ? _cardUnlocked : _cardLocked);

            var btn = go.AddComponent<Button>();
            btn.interactable = unlocked;
            var colors = btn.colors;
            colors.highlightedColor = cleared
                ? new Color(_cardCleared.r + 0.1f, _cardCleared.g + 0.1f, _cardCleared.b + 0.1f)
                : new Color(0.3f, 0.3f, 0.5f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.4f);
            colors.disabledColor = _cardLocked;
            btn.colors = colors;

            // 番号バッジ
            var badgeGO = new GameObject("Badge");
            badgeGO.transform.SetParent(go.transform, false);
            var badgeRect = badgeGO.AddComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0, 0.5f);
            badgeRect.anchorMax = new Vector2(0, 0.5f);
            badgeRect.pivot = new Vector2(0, 0.5f);
            badgeRect.sizeDelta = new Vector2(56, 56);
            badgeRect.anchoredPosition = new Vector2(12, 0);

            var badgeImg = badgeGO.AddComponent<Image>();
            badgeImg.color = cleared ? _badgeClearedColor : (unlocked ? _badgeColor : new Color(0.25f, 0.25f, 0.25f));

            var badgeText = new GameObject("BadgeText");
            badgeText.transform.SetParent(badgeGO.transform, false);
            var btRect = badgeText.AddComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero;
            btRect.anchorMax = Vector2.one;
            btRect.offsetMin = Vector2.zero;
            btRect.offsetMax = Vector2.zero;
            var btTmp = badgeText.AddComponent<TextMeshProUGUI>();
            if (_font != null) btTmp.font = _font;
            btTmp.text = cleared ? "<size=22>*</size>" : number.ToString();
            btTmp.fontSize = 28;
            btTmp.alignment = TextAlignmentOptions.Center;
            btTmp.color = Color.white;

            // サムネイル（あれば）
            float contentLeft = 78;
            if (chapter.Thumbnail != null)
            {
                var thumbGO = new GameObject("Thumbnail");
                thumbGO.transform.SetParent(go.transform, false);
                var thumbRect = thumbGO.AddComponent<RectTransform>();
                thumbRect.anchorMin = new Vector2(0, 0);
                thumbRect.anchorMax = new Vector2(0, 1);
                thumbRect.pivot = new Vector2(0, 0.5f);
                thumbRect.sizeDelta = new Vector2(140, -12);
                thumbRect.anchoredPosition = new Vector2(contentLeft, 0);

                var thumbImg = thumbGO.AddComponent<Image>();
                thumbImg.sprite = chapter.Thumbnail;
                thumbImg.preserveAspect = true;
                if (!unlocked) thumbImg.color = new Color(0.3f, 0.3f, 0.3f);
                contentLeft += 150;
            }

            // タイトル + サブテキスト
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(contentLeft, 6);
            labelRect.offsetMax = new Vector2(-20, -6);

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            if (_font != null) tmp.font = _font;

            string displayTitle = string.IsNullOrEmpty(chapter.Title)
                ? $"Chapter {number}"
                : chapter.Title;

            if (unlocked)
            {
                string statusLine = cleared
                    ? "\n<size=24><color=#80FF80>Cleared</color>  <color=#AAAAAA>- Tap to replay</color></size>"
                    : "\n<size=24><color=#AAAAAA>New</color></size>";
                tmp.text = $"<b>{displayTitle}</b>{statusLine}";
                tmp.color = Color.white;
            }
            else
            {
                tmp.text = "<b>???</b>\n<size=24><color=#555555>Locked</color></size>";
                tmp.color = new Color(0.4f, 0.4f, 0.4f);
            }

            tmp.fontSize = 34;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            if (unlocked)
            {
                string capturedPath = chapter.ResolvedPath;
                int capturedIndex = index;
                btn.onClick.AddListener(() =>
                {
                    if (cleared)
                    {
                        // クリア済みならトグルで展開/折りたたみ
                        _expandedChapter = _expandedChapter == capturedIndex ? -1 : capturedIndex;
                        BuildList();
                    }
                    else
                    {
                        PlayChapter(capturedPath);
                    }
                });
            }
        }

        private void CreateSceneReplayList(ChapterEntry chapter, int chapterIndex)
        {
            // Play from start ボタン
            CreateReplayButton($"  >> Play from start", chapter.ResolvedPath, -1, chapterIndex);

            // スクリプトからラベル一覧を取得してシーンリプレイボタンを作成
            string path = chapter.ResolvedPath;
            var labels = GetScriptLabels(path);
            foreach (var label in labels)
            {
                CreateReplayButton($"      {label}", path, -1, chapterIndex, label);
            }
        }

        private void CreateReplayButton(string text, string scriptPath, int cmdIndex, int chapterIndex, string label = null)
        {
            var go = new GameObject("Replay");
            go.transform.SetParent(_listContainer, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(860, 48);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.12f, 0.18f, 0.25f, 0.8f);

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.2f, 0.3f, 0.45f);
            colors.pressedColor = new Color(0.15f, 0.22f, 0.35f);
            btn.colors = colors;

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var lRect = labelGO.AddComponent<RectTransform>();
            lRect.anchorMin = Vector2.zero;
            lRect.anchorMax = Vector2.one;
            lRect.offsetMin = new Vector2(16, 0);
            lRect.offsetMax = new Vector2(-16, 0);

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            if (_font != null) tmp.font = _font;
            tmp.text = text;
            tmp.fontSize = 26;
            tmp.color = new Color(0.7f, 0.85f, 1f);
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            string capturedPath = scriptPath;
            string capturedLabel = label;
            btn.onClick.AddListener(() => PlayChapter(capturedPath, capturedLabel));
        }

        private List<string> GetScriptLabels(string resourcePath)
        {
            var labels = new List<string>();
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null) return labels;

            try
            {
                var script = ScriptParser.Parse(asset.text);
                if (script?.Commands == null) return labels;
                foreach (var cmd in script.Commands)
                {
                    if (cmd.Type == "label" && !string.IsNullOrEmpty(cmd.Label))
                        labels.Add(cmd.Label);
                }
            }
            catch { }
            return labels;
        }

        private void PlayChapter(string scriptPath, string jumpToLabel = null)
        {
            PlayerPrefs.SetString("novella_start_mode", "chapter_select");
            PlayerPrefs.SetString("novella_chapter_script", scriptPath);
            if (!string.IsNullOrEmpty(jumpToLabel))
                PlayerPrefs.SetString("novella_chapter_label", jumpToLabel);
            else
                PlayerPrefs.DeleteKey("novella_chapter_label");
            PlayerPrefs.Save();

            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene(_gameSceneName);
            else
                SceneManager.LoadScene(_gameSceneName);
        }
    }
}
