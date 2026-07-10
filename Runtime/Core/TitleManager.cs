using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Novella.Core
{
    public class TitleManager : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _galleryButton;
        [SerializeField] private Button _quitButton;

        [Header("Gallery")]
        [SerializeField] private Novella.UI.CGGalleryUIController _galleryUI;

        [Header("Scene Recollection")]
        [SerializeField] private Button _recollectionButton;
        [SerializeField] private Novella.UI.SceneRecollectionUIController _recollectionUI;

        [Header("Chapter Select")]
        [SerializeField] private Button _chapterSelectButton;
        [SerializeField] private Novella.UI.ChapterSelectUIController _chapterSelectUI;

        [Header("BGM Gallery")]
        [SerializeField] private Button _bgmGalleryButton;
        [SerializeField] private Novella.UI.BGMGalleryUIController _bgmGalleryUI;

        [Header("Ending List")]
        [SerializeField] private Button _endingListButton;
        [SerializeField] private Novella.UI.EndingListUIController _endingListUI;

        [Header("Flowchart")]
        [SerializeField] private Button _flowchartButton;
        [SerializeField] private Novella.UI.FlowchartUIController _flowchartUI;

        [Header("Settings Button")]
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Novella.UI.SettingsUIController _settingsUI;

        [Header("Theme")]
        [SerializeField] private NovellaUITheme _uiTheme;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _logoImage;
        [SerializeField] private AudioSource _bgmSource;

        [Header("Reset")]
        [SerializeField] private Button _resetButton;

        [Header("Settings")]
        [SerializeField] private string _gameSceneName = "SampleScene";

        [Tooltip("Resources/Scripts/ 以下のパス（拡張子不要）。ScriptAsset指定時はそちらが優先されます")]
        [SerializeField] private string _firstScriptPath = "Scripts/chapter01";

        [Tooltip("スクリプトファイルを直接指定（JSON/CSV自動判定）。設定するとパス指定より優先されます")]
        [SerializeField] private TextAsset _scriptAsset;

        private SaveManager _saveManager;

        private void Awake()
        {
            if (SceneTransitionManager.Instance == null)
            {
                var go = new GameObject("SceneTransitionManager");
                go.AddComponent<SceneTransitionManager>();
            }
        }

        private void Start()
        {
            _saveManager = new SaveManager();
            ApplyTheme();

            _newGameButton.onClick.AddListener(OnNewGame);
            _continueButton.onClick.AddListener(OnContinue);
            _quitButton.onClick.AddListener(OnQuit);

            if (_galleryButton != null)
            {
                _galleryButton.onClick.AddListener(OnGallery);
                // CG記録がなければギャラリーボタンを無効化
                _galleryButton.interactable = CGManager.GetViewedCGs().Count > 0;
            }

            if (_recollectionButton != null)
            {
                _recollectionButton.onClick.AddListener(OnRecollection);
                _recollectionButton.interactable = SceneRecollectionManager.GetClearedScenes().Count > 0;
            }

            if (_chapterSelectButton != null)
            {
                _chapterSelectButton.onClick.AddListener(OnChapterSelect);
                _chapterSelectButton.interactable = _chapterSelectUI != null;
            }

            if (_bgmGalleryButton != null)
            {
                _bgmGalleryButton.onClick.AddListener(OnBGMGallery);
                _bgmGalleryButton.interactable = BGMManager.GetPlayedBGMs().Count > 0;
            }

            if (_endingListButton != null)
            {
                _endingListButton.onClick.AddListener(OnEndingList);
                _endingListButton.interactable = EndingManager.GetUnlockedEndings().Count > 0;
            }

            if (_flowchartButton != null)
            {
                _flowchartButton.onClick.AddListener(OnFlowchart);
                _flowchartButton.interactable = _flowchartUI != null;
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettings);
                _settingsButton.interactable = _settingsUI != null;
            }
            // タイトル画面にはメッセージウィンドウ/オーディオ/エンジンが存在しないためnullで初期化
            // （スライダー等の値変更をSettingsDataへ保存する配線のみ有効化される）
            _settingsUI?.Init(null, null, null);

            if (_resetButton != null)
                _resetButton.onClick.AddListener(OnResetAllData);

            // クイックセーブまたはオートセーブがあればコンティニュー有効
            _continueButton.interactable = _saveManager.HasQuickSave() || _saveManager.HasAutoSave();
        }

        private void OnNewGame()
        {
            string scriptPath = _firstScriptPath;
            if (_scriptAsset != null)
                scriptPath = "Scripts/" + _scriptAsset.name;

            PlayerPrefs.SetString("novella_start_mode", "new");
            PlayerPrefs.SetString("novella_first_script", scriptPath);
            PlayerPrefs.Save();
            TransitionToGame();
        }

        private void OnContinue()
        {
            if (_saveManager.HasQuickSave())
            {
                PlayerPrefs.SetString("novella_start_mode", "quickload");
            }
            else if (_saveManager.HasAutoSave())
            {
                PlayerPrefs.SetString("novella_start_mode", "autoload");
            }
            else
            {
                return;
            }
            PlayerPrefs.Save();
            TransitionToGame();
        }

        private void OnGallery()
        {
            if (_galleryUI != null)
                _galleryUI.Open();
        }

        private void OnRecollection()
        {
            if (_recollectionUI != null)
                _recollectionUI.Open();
        }

        private void OnChapterSelect()
        {
            if (_chapterSelectUI != null)
                _chapterSelectUI.Open();
        }

        private void OnBGMGallery()
        {
            if (_bgmGalleryUI != null)
                _bgmGalleryUI.Open();
        }

        private void OnEndingList()
        {
            if (_endingListUI != null)
                _endingListUI.Open();
        }

        private void OnFlowchart()
        {
            if (_flowchartUI != null)
                _flowchartUI.Open();
        }

        private void OnSettings()
        {
            if (_settingsUI != null)
                _settingsUI.Open();
        }

        private void TransitionToGame()
        {
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene(_gameSceneName);
            else
                SceneManager.LoadScene(_gameSceneName);
        }

        private void OnResetAllData()
        {
            // セーブファイル削除
            string dir = Application.persistentDataPath;
            foreach (var file in System.IO.Directory.GetFiles(dir, "novella_*.json"))
                System.IO.File.Delete(file);
            foreach (var file in System.IO.Directory.GetFiles(dir, "novella_*.png"))
                System.IO.File.Delete(file);

            // 各マネージャーのPlayerPrefsデータをクリア
            CGManager.ClearAll();
            BGMManager.ClearAll();
            SceneRecollectionManager.ClearAll();
            EndingManager.ClearAll();
            ReadManager.ClearAll();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            // UI状態を更新
            _continueButton.interactable = false;
            if (_galleryButton != null) _galleryButton.interactable = false;
            if (_recollectionButton != null) _recollectionButton.interactable = false;
            if (_bgmGalleryButton != null) _bgmGalleryButton.interactable = false;
            if (_endingListButton != null) _endingListButton.interactable = false;

            Debug.Log("[Novella] All data reset.");
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ApplyTheme()
        {
            if (_uiTheme == null) return;

            // 背景画像
            if (_backgroundImage != null && _uiTheme.TitleBackgroundImage != null)
                _backgroundImage.sprite = _uiTheme.TitleBackgroundImage;

            // ロゴ画像
            if (_logoImage != null)
            {
                if (_uiTheme.TitleLogoImage != null)
                {
                    _logoImage.sprite = _uiTheme.TitleLogoImage;
                    _logoImage.gameObject.SetActive(true);
                }
                else
                {
                    _logoImage.gameObject.SetActive(false);
                }
            }

            // BGM
            if (_bgmSource != null && !string.IsNullOrEmpty(_uiTheme.TitleBGM))
            {
                var clip = Resources.Load<AudioClip>($"Audio/BGM/{_uiTheme.TitleBGM}");
                if (clip != null)
                {
                    _bgmSource.clip = clip;
                    _bgmSource.loop = true;
                    _bgmSource.Play();
                }
            }

            // ボタンスタイル適用
            ApplyButtonTheme(_newGameButton);
            ApplyButtonTheme(_continueButton);
            ApplyButtonTheme(_galleryButton);
            ApplyButtonTheme(_quitButton);
            ApplyButtonTheme(_recollectionButton);
            ApplyButtonTheme(_chapterSelectButton);
            ApplyButtonTheme(_bgmGalleryButton);
            ApplyButtonTheme(_endingListButton);
            ApplyButtonTheme(_flowchartButton);

            // Button Builderで手動追加されたカスタムボタンにも適用（ButtonRow配下を走査）
            var titleCanvas = GameObject.Find("TitleCanvas");
            var buttonRow = titleCanvas != null ? titleCanvas.transform.Find("ButtonRow") : null;
            if (buttonRow != null)
            {
                foreach (var btn in buttonRow.GetComponentsInChildren<Button>(true))
                    ApplyButtonTheme(btn);
            }

            // フォント適用
            if (_uiTheme.Font != null)
            {
                foreach (var tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
                    tmp.font = _uiTheme.Font;
            }
        }

        private void ApplyButtonTheme(Button btn)
        {
            if (btn == null || _uiTheme == null) return;

            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                if (_uiTheme.TitleButtonImage != null)
                    img.sprite = _uiTheme.TitleButtonImage;
                img.color = _uiTheme.TitleButtonColor;
            }

            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.color = _uiTheme.TitleButtonTextColor;
        }
    }
}
