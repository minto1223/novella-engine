using Novella.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Novella.UI
{
    /// <summary>
    /// ゲームプレイ中に常時表示されるミニHUD。
    /// セーブ/ロード/クイックセーブ/クイックロード/オート/スキップ/バックログ/メニューボタンを持つ。
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("HUD Panel")]
        [SerializeField] private GameObject _hudPanel;

        [Header("Engine Reference")]
        [SerializeField] private NovellaEngine _engine;

        [Header("Menu Panels")]
        [SerializeField] private MenuUIController _menuUI;
        [SerializeField] private SaveUIController _saveUI;
        [SerializeField] private SaveUIController _loadUI;
        [SerializeField] private BacklogUIController _backlogUI;

        [Header("HUD Buttons")]
        [SerializeField] private Button _quickSaveButton;
        [SerializeField] private Button _quickLoadButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _autoButton;
        [SerializeField] private Button _skipButton;
        [SerializeField] private Button _backlogButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _hideButton;

        [Header("Button Labels (optional)")]
        [SerializeField] private TMPro.TMP_Text _autoLabel;
        [SerializeField] private TMPro.TMP_Text _skipLabel;
        [SerializeField] private TMPro.TMP_Text _quickLoadLabel;

        private readonly SaveManager _saveManager = new SaveManager();
        private bool _autoMode;
        private bool _skipMode;

        private void Update()
        {
            if (_hudPanel == null || _menuUI == null) return;
            bool shouldHide = _menuUI.IsBlockingInput
                || (_backlogUI != null && _backlogUI.IsOpen)
                || (_engine != null && _engine.WindowHidden);
            if (_hudPanel.activeSelf == shouldHide)
                _hudPanel.SetActive(!shouldHide);
        }

        private void Start()
        {
            if (_engine != null)
                _engine.OnSkipModeChanged += OnSkipModeChangedByEngine;
            if (_quickSaveButton != null)
                _quickSaveButton.onClick.AddListener(OnQuickSave);
            if (_quickLoadButton != null)
                _quickLoadButton.onClick.AddListener(OnQuickLoad);
            if (_saveButton != null)
                _saveButton.onClick.AddListener(OnSave);
            if (_loadButton != null)
                _loadButton.onClick.AddListener(OnLoad);
            if (_autoButton != null)
                _autoButton.onClick.AddListener(OnToggleAuto);
            if (_skipButton != null)
                _skipButton.onClick.AddListener(OnToggleSkip);
            if (_backlogButton != null)
                _backlogButton.onClick.AddListener(OnBacklog);
            if (_menuButton != null)
                _menuButton.onClick.AddListener(OnMenu);
            if (_hideButton != null)
                _hideButton.onClick.AddListener(OnHide);

            RefreshQuickLoadButton();
        }

        private void OnQuickSave()
        {
            if (_engine == null) return;
            _saveManager.QuickSave(_engine);
            RefreshQuickLoadButton();
        }

        private void OnQuickLoad()
        {
            if (_engine == null) return;
            _saveManager.QuickLoad(_engine);
        }

        private void OnSave()
        {
            _menuUI?.Close();
            _saveUI?.Open();
        }

        private void OnLoad()
        {
            _menuUI?.Close();
            _loadUI?.Open();
        }

        private void OnToggleAuto()
        {
            if (_engine == null) return;
            _engine.ToggleAutoMode();
            _autoMode = _engine.AutoMode;
            UpdateAutoLabel();
        }

        private void OnToggleSkip()
        {
            if (_engine == null) return;
            _skipMode = !_skipMode;
            _engine.SetSkipMode(_skipMode);
            UpdateSkipLabel();
        }

        private void OnBacklog()
        {
            _backlogUI?.Toggle();
        }

        private void OnMenu()
        {
            _menuUI?.Toggle();
        }

        private void OnHide()
        {
            _engine?.ToggleWindowHide();
        }

        private void RefreshQuickLoadButton()
        {
            bool hasQuickSave = _saveManager.HasQuickSave();
            if (_quickLoadButton != null)
                _quickLoadButton.interactable = hasQuickSave;
            if (_quickLoadLabel != null)
                _quickLoadLabel.color = hasQuickSave ? Color.white : new Color(1, 1, 1, 0.4f);
        }

        private void UpdateAutoLabel()
        {
            if (_autoLabel != null)
                _autoLabel.text = _autoMode ? "AUTO▶" : "AUTO";
        }

        private void UpdateSkipLabel()
        {
            if (_skipLabel != null)
                _skipLabel.text = _skipMode ? "SKIP▶" : "SKIP";
        }

        private void OnSkipModeChangedByEngine(bool enabled)
        {
            _skipMode = enabled;
            UpdateSkipLabel();
        }

    }
}
