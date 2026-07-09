using Novella.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Novella.UI
{
    // Backlog等のサブパネルより先にUpdate()を実行し、
    // 同一フレーム内で「サブパネルがEscapeで閉じた直後にMenuが誤って開く」事故を防ぐ
    [DefaultExecutionOrder(-10)]
    public class MenuUIController : MonoBehaviour, Novella.Core.IMenuUI
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _titleButton;
        [SerializeField] private Button _closeButton;

        [SerializeField] private SaveUIController _saveUI;
        [SerializeField] private SaveUIController _loadUI;
        [SerializeField] private SettingsUIController _settingsUI;
        [SerializeField] private BacklogUIController _backlogUI;

        [SerializeField] private string _titleSceneName = "TitleScene";

        private NovellaEngine _engine;

        public void Init(NovellaEngine engine)
        {
            _engine = engine;
            if (_panel != null) _panel.SetActive(false);

            _saveUI?.Init(engine);
            _loadUI?.Init(engine);
            _settingsUI?.Init(engine.IMessageWindow, engine.IAudio, engine);

            if (_saveButton != null)
                _saveButton.onClick.AddListener(() => { Close(); _saveUI?.Open(Open); });
            if (_loadButton != null)
                _loadButton.onClick.AddListener(() => { Close(); _loadUI?.Open(Open); });
            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(() => { Close(); _settingsUI?.Open(Open); });
            if (_titleButton != null)
                _titleButton.onClick.AddListener(OnReturnToTitle);
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
        }

        private void OnReturnToTitle()
        {
            if (_engine != null)
                _engine.Stop();

            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene(_titleSceneName);
            else
                SceneManager.LoadScene(_titleSceneName);
        }

        private void Update()
        {
            // サブパネル・バックログが開いている間はそちら側のUpdateに任せる
            if ((_saveUI != null && _saveUI.IsOpen) ||
                (_loadUI != null && _loadUI.IsOpen) ||
                (_settingsUI != null && _settingsUI.IsOpen) ||
                (_backlogUI != null && _backlogUI.IsOpen))
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Toggle();
                return;
            }

            if (_panel != null && _panel.activeSelf
                && Input.GetMouseButtonDown(1) && !Novella.Core.UIInputUtil.IsPointerOverInteractableUI())
                Close();
        }

        public bool IsBlockingInput =>
            (_panel != null && _panel.activeSelf) ||
            (_saveUI    != null && _saveUI.IsOpen) ||
            (_loadUI    != null && _loadUI.IsOpen) ||
            (_settingsUI != null && _settingsUI.IsOpen);

        public void Toggle()
        {
            if (_panel != null && _panel.activeSelf) Close();
            else Open();
        }

        public void Open()
        {
            if (_panel != null) _panel.SetActive(true);
        }

        public void Close()
        {
            if (_panel != null) _panel.SetActive(false);
        }
    }
}
