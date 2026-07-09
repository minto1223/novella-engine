using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.UI
{
    /// <summary>
    /// 汎用のYes/No確認ダイアログ。設定画面のリセット確認等、複数箇所から再利用する想定。
    /// </summary>
    public class ConfirmDialogController : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _messageLabel;
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _noButton;
        [SerializeField] private TMP_Text _yesButtonLabel;
        [SerializeField] private TMP_Text _noButtonLabel;
        [SerializeField] private Image _panelImage;
        [SerializeField] private Image _yesButtonImage;
        [SerializeField] private Image _noButtonImage;

        private System.Action _onYes;
        private System.Action _onNo;

        public bool IsOpen => _panel != null && _panel.activeSelf;

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
            if (_yesButton != null) _yesButton.onClick.AddListener(OnYesClicked);
            if (_noButton != null) _noButton.onClick.AddListener(OnNoClicked);
        }

        private void Update()
        {
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
                OnNoClicked();
        }

        /// <summary>確認ダイアログを表示する。onNoはnull許容（キャンセル時に何もしない場合）。</summary>
        public void Show(string message, System.Action onYes, System.Action onNo = null,
            string yesLabel = "はい", string noLabel = "いいえ")
        {
            _onYes = onYes;
            _onNo = onNo;
            if (_messageLabel != null) _messageLabel.text = message;
            if (_yesButtonLabel != null) _yesButtonLabel.text = yesLabel;
            if (_noButtonLabel != null) _noButtonLabel.text = noLabel;
            if (_panel != null) _panel.SetActive(true);
        }

        /// <summary>コールバックを呼ばずに閉じる</summary>
        public void Close()
        {
            _onYes = null;
            _onNo = null;
            if (_panel != null) _panel.SetActive(false);
        }

        private void OnYesClicked()
        {
            var cb = _onYes;
            Close();
            cb?.Invoke();
        }

        private void OnNoClicked()
        {
            var cb = _onNo;
            Close();
            cb?.Invoke();
        }

        public void ApplyTheme(Color background, Color text, Color yesColor, Color noColor, Color buttonText)
        {
            if (_panelImage != null) _panelImage.color = background;
            if (_messageLabel != null) _messageLabel.color = text;
            if (_yesButtonImage != null) _yesButtonImage.color = yesColor;
            if (_noButtonImage != null) _noButtonImage.color = noColor;
            if (_yesButtonLabel != null) _yesButtonLabel.color = buttonText;
            if (_noButtonLabel != null) _noButtonLabel.color = buttonText;
        }
    }
}
