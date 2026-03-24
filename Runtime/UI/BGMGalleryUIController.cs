using Novella.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.UI
{
    /// <summary>
    /// BGM回想画面。再生済みのBGMを一覧表示し、選択で試聴できる。
    /// </summary>
    public class BGMGalleryUIController : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _listContainer;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_FontAsset _font;

        [Header("Now Playing")]
        [SerializeField] private TextMeshProUGUI _nowPlayingLabel;

        public bool IsOpen => _panel != null && _panel.activeSelf;

        private AudioSource _audioSource;
        private string _currentlyPlaying;

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        public void Open()
        {
            EnsureAudioSource();
            BuildList();
            if (_panel != null)
            {
                _panel.SetActive(true);
                _panel.transform.SetAsLastSibling();
            }
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
            UpdateNowPlaying(null);
        }

        public void Close()
        {
            StopPreview();
            if (_panel != null) _panel.SetActive(false);
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Close);
        }

        private void Update()
        {
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
                Close();
        }

        private void EnsureAudioSource()
        {
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.loop = true;
                _audioSource.playOnAwake = false;
                _audioSource.volume = SettingsData.BgmVolume;
            }
        }

        private void BuildList()
        {
            if (_listContainer == null) return;

            foreach (Transform child in _listContainer)
                Destroy(child.gameObject);

            var bgms = BGMManager.GetPlayedBGMs();
            if (bgms.Count == 0)
            {
                CreateLabel("(No BGM played yet)");
                return;
            }

            foreach (var (clipName, title) in bgms)
            {
                CreateBGMButton(clipName, title);
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

        private void CreateBGMButton(string clipName, string title)
        {
            var go = new GameObject($"BGM_{clipName}");
            go.transform.SetParent(_listContainer, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900, 90);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.12f, 0.15f, 0.25f, 0.9f);

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.25f, 0.3f, 0.5f);
            colors.pressedColor = new Color(0.2f, 0.25f, 0.45f);
            btn.colors = colors;

            // 音符アイコン + タイトル
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(16, 4);
            labelRect.offsetMax = new Vector2(-60, -4);

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            string displayName = string.IsNullOrEmpty(title) || title == clipName
                ? clipName
                : title;
            tmp.text = $"# {displayName}";
            tmp.fontSize = 34;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            if (_font != null) tmp.font = _font;
            tmp.color = Color.white;

            // 再生/停止ボタンアイコン
            var iconGO = new GameObject("PlayIcon");
            iconGO.transform.SetParent(go.transform, false);
            var iconRect = iconGO.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(1, 0);
            iconRect.anchorMax = new Vector2(1, 1);
            iconRect.pivot = new Vector2(1, 0.5f);
            iconRect.sizeDelta = new Vector2(44, 0);
            iconRect.anchoredPosition = new Vector2(-8, 0);

            var iconTmp = iconGO.AddComponent<TextMeshProUGUI>();
            iconTmp.text = ">";
            iconTmp.fontSize = 28;
            iconTmp.alignment = TextAlignmentOptions.Center;
            iconTmp.color = new Color(0.5f, 0.8f, 1f);

            string captured = clipName;
            string capturedTitle = displayName;
            btn.onClick.AddListener(() => TogglePreview(captured, capturedTitle));
        }

        private void TogglePreview(string clipName, string displayTitle)
        {
            if (_currentlyPlaying == clipName)
            {
                StopPreview();
                return;
            }

            var clip = Resources.Load<AudioClip>($"Audio/BGM/{clipName}");
            if (clip == null)
            {
                Debug.LogWarning($"[Novella] BGM not found: Audio/BGM/{clipName}");
                return;
            }

            _audioSource.Stop();
            _audioSource.clip = clip;
            _audioSource.volume = SettingsData.BgmVolume;
            _audioSource.Play();
            _currentlyPlaying = clipName;
            UpdateNowPlaying(displayTitle);
        }

        private void StopPreview()
        {
            if (_audioSource != null) _audioSource.Stop();
            _currentlyPlaying = null;
            UpdateNowPlaying(null);
        }

        private void UpdateNowPlaying(string title)
        {
            if (_nowPlayingLabel == null) return;
            if (string.IsNullOrEmpty(title))
                _nowPlayingLabel.text = "Now Playing: ---";
            else
                _nowPlayingLabel.text = $"Now Playing: {title}";
        }
    }
}
