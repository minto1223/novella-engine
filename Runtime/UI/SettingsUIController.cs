using Novella.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.UI
{
    public class SettingsUIController : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;

        [Header("Sliders")]
        [SerializeField] private Slider _textSpeedSlider;
        [SerializeField] private Slider _bgmVolumeSlider;
        [SerializeField] private Slider _seVolumeSlider;
        [SerializeField] private Slider _voiceVolumeSlider;
        [SerializeField] private Slider _autoDelaySlider;
        [SerializeField] private Slider _windowOpacitySlider;
        [SerializeField] private Slider _fontSizeSlider;

        [Header("Toggles")]
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private Toggle _skipUnreadToggle;
        [SerializeField] private Toggle _skipAfterChoiceToggle;
        [SerializeField] private Toggle _autoSaveToggle;

        [Header("Labels")]
        [SerializeField] private TMP_Text _textSpeedLabel;
        [SerializeField] private TMP_Text _bgmVolumeLabel;
        [SerializeField] private TMP_Text _seVolumeLabel;
        [SerializeField] private TMP_Text _voiceVolumeLabel;
        [SerializeField] private TMP_Text _autoDelayLabel;
        [SerializeField] private TMP_Text _windowOpacityLabel;
        [SerializeField] private TMP_Text _fontSizeLabel;

        [Header("Buttons")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _resetButton;

        [Header("Tabs")]
        [SerializeField] private Button _gameTabButton;
        [SerializeField] private Button _soundTabButton;
        [SerializeField] private GameObject _gameTabPanel;
        [SerializeField] private GameObject _soundTabPanel;
        [SerializeField] private Image _gameTabButtonImage;
        [SerializeField] private Image _soundTabButtonImage;
        [SerializeField] private TMP_Text _gameTabButtonLabel;
        [SerializeField] private TMP_Text _soundTabButtonLabel;
        [SerializeField] private Color _tabActiveColor = new Color(0.25f, 0.45f, 0.75f, 1f);
        [SerializeField] private Color _tabInactiveColor = new Color(0.15f, 0.15f, 0.2f, 0.8f);
        [SerializeField] private Color _tabActiveTextColor = Color.white;
        [SerializeField] private Color _tabInactiveTextColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        [Header("Confirm Dialog")]
        [SerializeField] private ConfirmDialogController _confirmDialog;

        private enum SettingsTab { Game, Sound }
        private SettingsTab _currentTab = SettingsTab.Game;

        private IMessageWindow _messageWindow;
        private IAudioPlayer _audio;
        private NovellaEngine _engine;

        private System.Action _onClose;

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        public void Init(IMessageWindow messageWindow, IAudioPlayer audio, NovellaEngine engine = null)
        {
            _messageWindow = messageWindow;
            _audio = audio;
            _engine = engine;

            // テキスト速度 5〜120
            InitSlider(_textSpeedSlider, 5f, 120f, SettingsData.TextSpeed, OnTextSpeedChanged);
            // BGM音量 0〜1
            InitSlider(_bgmVolumeSlider, 0f, 1f, SettingsData.BgmVolume, OnBgmVolumeChanged);
            // SE音量 0〜1
            InitSlider(_seVolumeSlider, 0f, 1f, SettingsData.SeVolume, OnSeVolumeChanged);
            // ボイス音量 0〜1
            InitSlider(_voiceVolumeSlider, 0f, 1f, SettingsData.VoiceVolume, OnVoiceVolumeChanged);
            // オート待ち時間 0.5〜8
            InitSlider(_autoDelaySlider, 0.5f, 8f, SettingsData.AutoDelay, OnAutoDelayChanged);
            // ウィンドウ透明度 0.1〜1
            InitSlider(_windowOpacitySlider, 0.1f, 1f, SettingsData.WindowOpacity, OnWindowOpacityChanged);
            // フォントサイズ 24〜72
            InitSlider(_fontSizeSlider, 24f, 72f, SettingsData.FontSize, OnFontSizeChanged);

            // フルスクリーン
            if (_fullscreenToggle != null)
            {
                _fullscreenToggle.isOn = SettingsData.Fullscreen;
                _fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }
            // 未読スキップ
            if (_skipUnreadToggle != null)
            {
                _skipUnreadToggle.isOn = SettingsData.SkipUnread;
                _skipUnreadToggle.onValueChanged.AddListener(OnSkipUnreadChanged);
            }
            // 選択肢後もスキップを続ける
            if (_skipAfterChoiceToggle != null)
            {
                _skipAfterChoiceToggle.isOn = SettingsData.SkipAfterChoice;
                _skipAfterChoiceToggle.onValueChanged.AddListener(OnSkipAfterChoiceChanged);
            }
            // オートセーブ
            if (_autoSaveToggle != null)
            {
                _autoSaveToggle.isOn = SettingsData.AutoSave;
                _autoSaveToggle.onValueChanged.AddListener(OnAutoSaveChanged);
            }

            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
            if (_resetButton != null)
                _resetButton.onClick.AddListener(OnReset);

            if (_gameTabButton != null)
                _gameTabButton.onClick.AddListener(() => SelectTab(SettingsTab.Game));
            if (_soundTabButton != null)
                _soundTabButton.onClick.AddListener(() => SelectTab(SettingsTab.Sound));

            ApplyAll();
            RefreshLabels();
        }

        private void InitSlider(Slider slider, float min, float max, float value,
            UnityEngine.Events.UnityAction<float> callback)
        {
            if (slider == null) return;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            slider.onValueChanged.AddListener(callback);
        }

        public void Open(System.Action onClose = null)
        {
            _onClose = onClose;
            // 最新の設定値を反映
            if (_textSpeedSlider != null) _textSpeedSlider.value = SettingsData.TextSpeed;
            if (_bgmVolumeSlider != null) _bgmVolumeSlider.value = SettingsData.BgmVolume;
            if (_seVolumeSlider != null) _seVolumeSlider.value = SettingsData.SeVolume;
            if (_voiceVolumeSlider != null) _voiceVolumeSlider.value = SettingsData.VoiceVolume;
            if (_autoDelaySlider != null) _autoDelaySlider.value = SettingsData.AutoDelay;
            if (_windowOpacitySlider != null) _windowOpacitySlider.value = SettingsData.WindowOpacity;
            if (_fontSizeSlider != null) _fontSizeSlider.value = SettingsData.FontSize;
            if (_fullscreenToggle != null) _fullscreenToggle.isOn = SettingsData.Fullscreen;
            if (_skipUnreadToggle != null) _skipUnreadToggle.isOn = SettingsData.SkipUnread;
            if (_skipAfterChoiceToggle != null) _skipAfterChoiceToggle.isOn = SettingsData.SkipAfterChoice;
            if (_autoSaveToggle != null) _autoSaveToggle.isOn = SettingsData.AutoSave;
            RefreshLabels();
            SelectTab(SettingsTab.Game);
            if (_panel != null) _panel.SetActive(true);
        }

        private void SelectTab(SettingsTab tab)
        {
            _currentTab = tab;
            if (_gameTabPanel != null) _gameTabPanel.SetActive(tab == SettingsTab.Game);
            if (_soundTabPanel != null) _soundTabPanel.SetActive(tab == SettingsTab.Sound);
            RefreshTabVisuals();
        }

        private void RefreshTabVisuals()
        {
            bool gameActive = _currentTab == SettingsTab.Game;
            if (_gameTabButtonImage != null) _gameTabButtonImage.color = gameActive ? _tabActiveColor : _tabInactiveColor;
            if (_soundTabButtonImage != null) _soundTabButtonImage.color = gameActive ? _tabInactiveColor : _tabActiveColor;
            if (_gameTabButtonLabel != null) _gameTabButtonLabel.color = gameActive ? _tabActiveTextColor : _tabInactiveTextColor;
            if (_soundTabButtonLabel != null) _soundTabButtonLabel.color = gameActive ? _tabInactiveTextColor : _tabActiveTextColor;
        }

        public void ApplyTheme(Color activeColor, Color inactiveColor, Color activeText, Color inactiveText)
        {
            _tabActiveColor = activeColor;
            _tabInactiveColor = inactiveColor;
            _tabActiveTextColor = activeText;
            _tabInactiveTextColor = inactiveText;
            RefreshTabVisuals();
        }

        public bool IsOpen => _panel != null && _panel.activeSelf;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && IsOpen)
                Close();
        }

        public void Close()
        {
            if (_panel != null) _panel.SetActive(false);
            var cb = _onClose;
            _onClose = null;
            cb?.Invoke();
        }

        // --- コールバック ---

        private void OnTextSpeedChanged(float value)
        {
            SettingsData.TextSpeed = value;
            _messageWindow?.ApplyTextSpeed(value);
            RefreshLabels();
        }

        private void OnBgmVolumeChanged(float value)
        {
            SettingsData.BgmVolume = value;
            _audio?.SetBgmVolume(value);
            RefreshLabels();
        }

        private void OnSeVolumeChanged(float value)
        {
            SettingsData.SeVolume = value;
            _audio?.SetSeVolume(value);
            RefreshLabels();
        }

        private void OnVoiceVolumeChanged(float value)
        {
            SettingsData.VoiceVolume = value;
            _audio?.SetVoiceVolume(value);
            RefreshLabels();
        }

        private void OnAutoDelayChanged(float value)
        {
            SettingsData.AutoDelay = value;
            _engine?.ApplyAutoDelay(value);
            RefreshLabels();
        }

        private void OnWindowOpacityChanged(float value)
        {
            SettingsData.WindowOpacity = value;
            _messageWindow?.ApplyWindowOpacity(value);
            RefreshLabels();
        }

        private void OnFontSizeChanged(float value)
        {
            SettingsData.FontSize = value;
            _messageWindow?.ApplyFontSize(value);
            RefreshLabels();
        }

        private void OnFullscreenChanged(bool value)
        {
            SettingsData.Fullscreen = value;
            Screen.fullScreen = value;
        }

        private void OnSkipUnreadChanged(bool value)
        {
            SettingsData.SkipUnread = value;
        }

        private void OnSkipAfterChoiceChanged(bool value)
        {
            SettingsData.SkipAfterChoice = value;
        }

        private void OnAutoSaveChanged(bool value)
        {
            SettingsData.AutoSave = value;
        }

        private void OnReset()
        {
            if (_confirmDialog != null)
                _confirmDialog.Show("設定をすべて初期状態に戻しますか？", DoReset, null, "戻す", "キャンセル");
            else
                DoReset();
        }

        private void DoReset()
        {
            SettingsData.ResetAll();
            // スライダー・トグルに反映（onValueChangedが発火してApplyされる）
            if (_textSpeedSlider != null) _textSpeedSlider.value = SettingsData.DefaultTextSpeed;
            if (_bgmVolumeSlider != null) _bgmVolumeSlider.value = SettingsData.DefaultBgmVolume;
            if (_seVolumeSlider != null) _seVolumeSlider.value = SettingsData.DefaultSeVolume;
            if (_voiceVolumeSlider != null) _voiceVolumeSlider.value = SettingsData.DefaultVoiceVolume;
            if (_autoDelaySlider != null) _autoDelaySlider.value = SettingsData.DefaultAutoDelay;
            if (_windowOpacitySlider != null) _windowOpacitySlider.value = SettingsData.DefaultWindowOpacity;
            if (_fontSizeSlider != null) _fontSizeSlider.value = SettingsData.DefaultFontSize;
            if (_fullscreenToggle != null) _fullscreenToggle.isOn = SettingsData.DefaultFullscreen == 1;
            if (_skipUnreadToggle != null) _skipUnreadToggle.isOn = SettingsData.DefaultSkipUnread == 1;
            if (_skipAfterChoiceToggle != null) _skipAfterChoiceToggle.isOn = SettingsData.DefaultSkipAfterChoice == 1;
            if (_autoSaveToggle != null) _autoSaveToggle.isOn = SettingsData.DefaultAutoSave == 1;
            RefreshLabels();
        }

        private void ApplyAll()
        {
            _messageWindow?.ApplyTextSpeed(SettingsData.TextSpeed);
            _messageWindow?.ApplyFontSize(SettingsData.FontSize);
            _messageWindow?.ApplyWindowOpacity(SettingsData.WindowOpacity);
            _audio?.SetBgmVolume(SettingsData.BgmVolume);
            _audio?.SetSeVolume(SettingsData.SeVolume);
            _audio?.SetVoiceVolume(SettingsData.VoiceVolume);
            _engine?.ApplyAutoDelay(SettingsData.AutoDelay);
            Screen.fullScreen = SettingsData.Fullscreen;
        }

        private void RefreshLabels()
        {
            if (_textSpeedLabel != null)
                _textSpeedLabel.text = $"テキスト速度: {Mathf.RoundToInt(SettingsData.TextSpeed)}";
            if (_bgmVolumeLabel != null)
                _bgmVolumeLabel.text = $"BGM音量: {Mathf.RoundToInt(SettingsData.BgmVolume * 100)}%";
            if (_seVolumeLabel != null)
                _seVolumeLabel.text = $"SE音量: {Mathf.RoundToInt(SettingsData.SeVolume * 100)}%";
            if (_voiceVolumeLabel != null)
                _voiceVolumeLabel.text = $"ボイス音量: {Mathf.RoundToInt(SettingsData.VoiceVolume * 100)}%";
            if (_autoDelayLabel != null)
                _autoDelayLabel.text = $"オート待ち時間: {SettingsData.AutoDelay:F1}秒";
            if (_windowOpacityLabel != null)
                _windowOpacityLabel.text = $"ウィンドウ透明度: {Mathf.RoundToInt(SettingsData.WindowOpacity * 100)}%";
            if (_fontSizeLabel != null)
                _fontSizeLabel.text = $"フォントサイズ: {Mathf.RoundToInt(SettingsData.FontSize)}";
        }
    }
}
