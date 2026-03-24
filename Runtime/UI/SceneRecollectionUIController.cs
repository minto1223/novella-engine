using Novella.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Novella.UI
{
    /// <summary>
    /// シーン回想画面。SceneDefinitionベースでクリア済みシーンを一覧表示し、
    /// 選択でラベル区間をリプレイできる。未クリアシーンはロック表示。
    /// </summary>
    public class SceneRecollectionUIController : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _listContainer;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_FontAsset _font;

        [Header("Settings")]
        [SerializeField] private string _gameSceneName = "SampleScene";

        [Header("Scene Definitions")]
        [SerializeField] private SceneDefinition[] _sceneDefinitions;

        [Header("Colors")]
        [SerializeField] private Color _unlockedColor = new Color(0.15f, 0.15f, 0.2f, 0.9f);
        [SerializeField] private Color _lockedColor = new Color(0.1f, 0.1f, 0.1f, 0.6f);
        [SerializeField] private Color _unlockedTextColor = Color.white;
        [SerializeField] private Color _lockedTextColor = new Color(0.4f, 0.4f, 0.4f);

        public bool IsOpen => _panel != null && _panel.activeSelf;

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        public void Open()
        {
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

            // SceneDefinitionが設定されている場合はそれを使う
            if (_sceneDefinitions != null && _sceneDefinitions.Length > 0)
            {
                // sortOrder順にソート
                var sorted = new SceneDefinition[_sceneDefinitions.Length];
                System.Array.Copy(_sceneDefinitions, sorted, _sceneDefinitions.Length);
                System.Array.Sort(sorted, (a, b) =>
                {
                    if (a == null && b == null) return 0;
                    if (a == null) return 1;
                    if (b == null) return -1;
                    return a.sortOrder.CompareTo(b.sortOrder);
                });

                bool anyUnlocked = false;
                foreach (var def in sorted)
                {
                    if (def == null) continue;
                    bool cleared = SceneRecollectionManager.IsCleared(def.sceneId);
                    if (cleared) anyUnlocked = true;
                    CreateSceneCard(def, cleared);
                }

                if (!anyUnlocked)
                {
                    // 全ロック時のメッセージ（カード自体は表示済み）
                }
            }
            else
            {
                // フォールバック: 従来のスクリプトパスベース
                var scenes = SceneRecollectionManager.GetClearedScenes();
                if (scenes.Count == 0)
                {
                    CreateLabel("(No cleared scenes)");
                    return;
                }
                foreach (var (id, title) in scenes)
                    CreateLegacyButton(id, title);
            }
        }

        private void CreateSceneCard(SceneDefinition def, bool cleared)
        {
            var go = new GameObject($"Scene_{def.sceneId}");
            go.transform.SetParent(_listContainer, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900, def.thumbnail != null ? 140 : 90);

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12;
            layout.padding = new RectOffset(12, 12, 8, 8);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var bgImg = go.AddComponent<Image>();
            bgImg.color = cleared ? _unlockedColor : _lockedColor;

            // サムネイル
            if (def.thumbnail != null)
            {
                var thumbGO = new GameObject("Thumb");
                thumbGO.transform.SetParent(go.transform, false);
                var thumbRect = thumbGO.AddComponent<RectTransform>();
                thumbRect.sizeDelta = new Vector2(160, 100);
                var thumbImg = thumbGO.AddComponent<Image>();
                if (cleared)
                {
                    thumbImg.sprite = def.thumbnail;
                    thumbImg.preserveAspect = true;
                }
                else
                {
                    thumbImg.color = new Color(0.15f, 0.15f, 0.15f);
                }
                var thumbLE = thumbGO.AddComponent<LayoutElement>();
                thumbLE.preferredWidth = 160;
                thumbLE.preferredHeight = 100;
            }

            // テキスト部分
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(680, 80);
            var textLE = textGO.AddComponent<LayoutElement>();
            textLE.flexibleWidth = 1;

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            if (cleared)
                tmp.text = def.title;
            else
                tmp.text = "???";
            tmp.fontSize = 34;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = cleared ? _unlockedTextColor : _lockedTextColor;
            if (_font != null) tmp.font = _font;

            // ボタン（クリア済みのみ）
            if (cleared)
            {
                var btn = go.AddComponent<Button>();
                var colors = btn.colors;
                colors.highlightedColor = new Color(0.3f, 0.3f, 0.5f);
                colors.pressedColor = new Color(0.2f, 0.2f, 0.4f);
                btn.colors = colors;

                string scriptPath = def.scriptPath;
                string startLabel = def.startLabel;
                string endLabel = def.endLabel;
                btn.onClick.AddListener(() => PlayScene(scriptPath, startLabel, endLabel));
            }
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

        private void CreateLegacyButton(string sceneId, string title)
        {
            var go = new GameObject($"Scene_{sceneId}");
            go.transform.SetParent(_listContainer, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900, 90);

            var img = go.AddComponent<Image>();
            img.color = _unlockedColor;

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.5f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.4f);
            btn.colors = colors;

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(16, 4);
            labelRect.offsetMax = new Vector2(-16, -4);

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = string.IsNullOrEmpty(title) ? sceneId : title;
            tmp.fontSize = 34;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = _unlockedTextColor;
            if (_font != null) tmp.font = _font;

            string capturedId = sceneId;
            btn.onClick.AddListener(() => PlayScene(capturedId, null, null));
        }

        private void PlayScene(string scriptPath, string startLabel, string endLabel)
        {
            PlayerPrefs.SetString("novella_start_mode", "recollection");
            PlayerPrefs.SetString("novella_recollection_script", scriptPath);
            if (!string.IsNullOrEmpty(startLabel))
                PlayerPrefs.SetString("novella_recollection_start", startLabel);
            if (!string.IsNullOrEmpty(endLabel))
                PlayerPrefs.SetString("novella_recollection_end", endLabel);
            PlayerPrefs.Save();

            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene(_gameSceneName);
            else
                SceneManager.LoadScene(_gameSceneName);
        }
    }
}
