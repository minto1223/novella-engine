using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Novella.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.Core
{
    public class NovellaEngine : MonoBehaviour
    {
        [Header("UI Controllers")]
        public MessageWindowController MessageWindow;
        public BackgroundController Background;
        public CharacterDisplayController CharacterLayer;
        public AudioController Audio;
        public ChoiceUIController ChoiceUI;
        public BacklogUIController BacklogUI;
        public Novella.UI.MenuUIController MenuUI;
        public Novella.UI.MoviePlayerController MoviePlayer;

        // インターフェース経由でのアクセス（カスタムUIへの差し替えに使用）
        public IMessageWindow     IMessageWindow  => _messageWindow  ?? MessageWindow  as IMessageWindow;
        public IBackgroundDisplay IBackground     => _background     ?? Background     as IBackgroundDisplay;
        public ICharacterDisplay  ICharacterLayer => _characterLayer ?? CharacterLayer as ICharacterDisplay;
        public IAudioPlayer       IAudio          => _audio          ?? Audio          as IAudioPlayer;
        public IChoiceUI          IChoiceUI       => _choiceUI       ?? ChoiceUI       as IChoiceUI;
        public IBacklogUI         IBacklogUI      => _backlogUI      ?? BacklogUI      as IBacklogUI;
        public IMenuUI            IMenuUI         => _menuUI         ?? MenuUI         as IMenuUI;

        [Header("Custom UI Overrides (optional)")]
        [SerializeField] private MonoBehaviour _messageWindowOverride;
        [SerializeField] private MonoBehaviour _backgroundOverride;
        [SerializeField] private MonoBehaviour _characterLayerOverride;
        [SerializeField] private MonoBehaviour _audioOverride;
        [SerializeField] private MonoBehaviour _choiceUIOverride;
        [SerializeField] private MonoBehaviour _backlogUIOverride;
        [SerializeField] private MonoBehaviour _menuUIOverride;

        private IMessageWindow     _messageWindow;
        private IBackgroundDisplay _background;
        private ICharacterDisplay  _characterLayer;
        private IAudioPlayer       _audio;
        private IChoiceUI          _choiceUI;
        private IBacklogUI         _backlogUI;
        private IMenuUI            _menuUI;

        [Header("UI Theme (optional)")]
        public NovellaUITheme UITheme;

        [Header("Auto / Skip")]
        public TMP_Text AutoLabel;
        private float _autoDelay = 2f;

        [Header("Save Settings")]
        [SerializeField, Min(1)] private int _saveSlotCount = 3;
        public int SaveSlotCount => _saveSlotCount;

        [Header("Character Definitions")]
        public CharacterDefinition[] CharacterDefinitions;

        [Header("Scene Definitions (Recollection)")]
        public SceneDefinition[] SceneDefinitions;

        [Header("AI Settings")]
        public string AIApiKey = "";

        private NovellaScript _currentScript;
        private string _currentScriptPath;
        private int _currentIndex;
        private bool _isRunning;
        private Action _pendingAdvanceCallback;
        private readonly Dictionary<string, int> _labelIndex = new Dictionary<string, int>();

        private bool _autoMode;
        private float _autoTimer = -1f;
        private bool _skipMode;
        private bool _windowHidden;

        // 回想モード
        private bool _isRecollectionMode;
        private string _recollectionEndLabel;
        private Dictionary<string, string> _savedFlags;

        public bool IsRecollectionMode => _isRecollectionMode;

        // 視覚状態トラッキング（セーブ/ロード用）
        private string _currentBackground;
        private string _currentBgmClip;
        private float _currentBgmVolume = 1f;
        private readonly Dictionary<string, CharacterState> _currentCharacters = new Dictionary<string, CharacterState>();

        public void TrackBackground(string imageName) => _currentBackground = imageName;
        public void TrackBgm(string clipName, float volume) { _currentBgmClip = clipName; _currentBgmVolume = volume; }
        public void TrackBgmStop() { _currentBgmClip = null; _currentBgmVolume = 1f; }
        public void TrackCharacterShow(string id, string expression, string position)
        {
            if (_currentCharacters.TryGetValue(id, out var existing))
            {
                if (!string.IsNullOrEmpty(expression)) existing.Expression = expression;
                if (!string.IsNullOrEmpty(position)) existing.Position = position;
            }
            else
            {
                _currentCharacters[id] = new CharacterState { CharacterId = id, Expression = expression, Position = position ?? "center" };
            }
        }
        public void TrackCharacterHide(string id) => _currentCharacters.Remove(id);
        public void TrackDisplayMode(string mode) => _currentDisplayMode = mode ?? "adv";

        private string _currentDisplayMode = "adv";

        public VisualState GetVisualState() => new VisualState
        {
            BackgroundImage = _currentBackground,
            BgmClip = _currentBgmClip,
            BgmVolume = _currentBgmVolume,
            Characters = new System.Collections.Generic.List<CharacterState>(_currentCharacters.Values),
            DisplayMode = _currentDisplayMode,
        };

        public bool AutoMode => _autoMode;
        public bool SkipMode => _skipMode;
        public event System.Action<bool> OnSkipModeChanged;
        public bool WindowHidden => _windowHidden;

        /// <summary>
        /// メッセージウィンドウ・HUDを一時非表示にしてCG鑑賞モードにする。
        /// 再度呼ぶと復帰。
        /// </summary>
        public void ToggleWindowHide()
        {
            _windowHidden = !_windowHidden;
            IMessageWindow?.SetPanelVisible(!_windowHidden);
        }

        public void ToggleAutoMode()
        {
            _autoMode = !_autoMode;
            _autoTimer = -1f;
            UpdateAutoLabel();
        }

        public void SetSkipMode(bool enabled)
        {
            _skipMode = enabled;
        }

        public void ApplyAutoDelay(float delay)
        {
            _autoDelay = Mathf.Max(0.1f, delay);
        }

        public string CurrentScriptPath => _currentScriptPath;
        public string CurrentScriptTitle => _currentScript?.Title ?? "";
        public int CurrentIndex => _currentIndex - 1; // 実行済みコマンドのindex
        public bool CurrentCommandWasRead { get; private set; }

        public readonly FlagManager Flags = new FlagManager();
        public readonly BacklogManager Backlog = new BacklogManager();
        public readonly SaveManager SaveManager = new SaveManager();

        private Dictionary<string, CharacterDefinition> _charDefMap;

        /// <summary>
        /// キャラIDからキャラ定義を取得。未定義ならnull。
        /// </summary>
        public CharacterDefinition GetCharacterDef(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return null;
            if (_charDefMap == null) BuildCharDefMap();
            _charDefMap.TryGetValue(characterId, out var def);
            return def;
        }

        private void BuildCharDefMap()
        {
            _charDefMap = new Dictionary<string, CharacterDefinition>();
            if (CharacterDefinitions == null) return;
            foreach (var def in CharacterDefinitions)
            {
                if (def != null && !string.IsNullOrEmpty(def.characterId))
                    _charDefMap[def.characterId] = def;
            }
        }

        private readonly Dictionary<string, ICommandHandler> _handlers =
            new Dictionary<string, ICommandHandler>();

        private void Awake()
        {
            ResolveUIOverrides();
            RegisterDefaultHandlers();
            UIThemeApplicator.Apply(UITheme, this);
            IMenuUI?.Init(this);
            IBacklogUI?.SetJumpCallback(OnBacklogJump);
            if (BacklogUI != null) BacklogUI.SetEngine(this);
        }

        private void OnBacklogJump(string scriptPath, int commandIndex)
        {
            // バックログをジャンプ先のエントリも含めてトリム（再実行で再追加されるため）
            while (Backlog.Entries.Count > 0)
            {
                var last = Backlog.Entries[Backlog.Entries.Count - 1];
                Backlog.RemoveLast();
                if (last.ScriptPath == scriptPath && last.CommandIndex == commandIndex)
                    break;
            }
            IBacklogUI?.Rebuild(Backlog.Entries);

            LoadAndPlayFrom(scriptPath, commandIndex);
        }

        private void ResolveUIOverrides()
        {
            _messageWindow  = _messageWindowOverride  as IMessageWindow;
            _background     = _backgroundOverride     as IBackgroundDisplay;
            _characterLayer = _characterLayerOverride as ICharacterDisplay;
            _audio          = _audioOverride          as IAudioPlayer;
            _choiceUI       = _choiceUIOverride       as IChoiceUI;
            _backlogUI      = _backlogUIOverride      as IBacklogUI;
            _menuUI         = _menuUIOverride         as IMenuUI;
        }

        private void RegisterDefaultHandlers()
        {
            RegisterHandler(new Novella.Commands.SayCommandHandler());
            RegisterHandler(new Novella.Commands.ShowBgCommandHandler());
            RegisterHandler(new Novella.Commands.ShowCharCommandHandler());
            RegisterHandler(new Novella.Commands.HideCharCommandHandler());
            RegisterHandler(new Novella.Commands.WaitCommandHandler());
            RegisterHandler(new Novella.Commands.EndCommandHandler());
            RegisterHandler(new Novella.Commands.PlayBgmCommandHandler());
            RegisterHandler(new Novella.Commands.StopBgmCommandHandler());
            RegisterHandler(new Novella.Commands.PlaySeCommandHandler());
            RegisterHandler(new Novella.Commands.ChoiceCommandHandler());
            RegisterHandler(new Novella.Commands.SetFlagCommandHandler());
            RegisterHandler(new Novella.Commands.LabelCommandHandler());
            RegisterHandler(new Novella.Commands.JumpCommandHandler());
            RegisterHandler(new Novella.Commands.JumpIfCommandHandler());
            RegisterHandler(new Novella.Commands.JumpUnlessCommandHandler());
            RegisterHandler(new Novella.Commands.AISayCommandHandler());
            RegisterHandler(new Novella.Commands.NextScriptCommandHandler());
            RegisterHandler(new Novella.Commands.ShakeCommandHandler());
            RegisterHandler(new Novella.Commands.FlashCommandHandler());
            RegisterHandler(new Novella.Commands.FadeCommandHandler());
            RegisterHandler(new Novella.Commands.ShowTitleCommandHandler());
            RegisterHandler(new Novella.Commands.PlayVoiceCommandHandler());
            RegisterHandler(new Novella.Commands.StopVoiceCommandHandler());
            RegisterHandler(new Novella.Commands.MoveCharCommandHandler());
            RegisterHandler(new Novella.Commands.AddFlagCommandHandler());
            RegisterHandler(new Novella.Commands.ZoomCommandHandler());
            RegisterHandler(new Novella.Commands.PanCommandHandler());
            RegisterHandler(new Novella.Commands.ResetCameraCommandHandler());
            RegisterHandler(new Novella.Commands.InputTextCommandHandler());
            RegisterHandler(new Novella.Commands.PlayParticleCommandHandler());
            RegisterHandler(new Novella.Commands.StopParticleCommandHandler());
            RegisterHandler(new Novella.Commands.SetLanguageCommandHandler());
            RegisterHandler(new Novella.Commands.PlayMovieCommandHandler());
            RegisterHandler(new Novella.Commands.StopMovieCommandHandler());
            RegisterHandler(new Novella.Commands.CalcCommandHandler());
            RegisterHandler(new Novella.Commands.SetModeCommandHandler());
            RegisterHandler(new Novella.Commands.ClearCommandHandler());
            RegisterHandler(new Novella.Commands.FadeBgmCommandHandler());
            RegisterHandler(new Novella.Commands.StopSeCommandHandler());
            RegisterHandler(new Novella.Commands.SetVolumeCommandHandler());
            RegisterHandler(new Novella.Commands.KenBurnsCommandHandler());
            RegisterHandler(new Novella.Commands.StopKenBurnsCommandHandler());
        }

        public void RegisterHandler(ICommandHandler handler)
        {
            _handlers[handler.CommandType] = handler;
        }

        public void LoadAndPlay(string resourcePath)
        {
            _currentScriptPath = resourcePath;
            var script = ScriptParser.LoadFromResources(resourcePath);
            if (script != null) Play(script);
        }

        public void LoadAndPlayDirect(NovellaScript script, string scriptName)
        {
            // Resourcesパスとして再ロード可能な形式で保存
            // scriptNameが "Scripts/" で始まっていなければ補完する
            if (!string.IsNullOrEmpty(scriptName) && !scriptName.StartsWith("Scripts/"))
            {
                // Resources/Scripts/ に存在するか確認
                var check = Resources.Load<TextAsset>($"Scripts/{scriptName}");
                if (check != null)
                    scriptName = $"Scripts/{scriptName}";
            }
            _currentScriptPath = scriptName;
            if (script != null) Play(script);
        }

        public void LoadAndPlayFrom(string resourcePath, int index)
            => LoadAndPlayFrom(resourcePath, index, null);

        public void LoadAndPlayFrom(string resourcePath, int index, VisualState visualState)
        {
            _currentScriptPath = resourcePath;
            var script = ScriptParser.LoadFromResources(resourcePath);
            // パスが不完全な場合のフォールバック（旧セーブデータ互換）
            if (script == null && !resourcePath.StartsWith("Scripts/"))
            {
                string fallback = $"Scripts/{resourcePath}";
                script = ScriptParser.LoadFromResources(fallback);
                if (script != null)
                    _currentScriptPath = fallback;
            }
            if (script == null) return;
            _currentScript = script;
            _currentIndex = Mathf.Clamp(index, 0, script.Commands.Count);
            _isRunning = true;
            _pendingAdvanceCallback = null;
            BuildLabelIndex(script);
            IMessageWindow?.Hide();
            // ScreenEffectOverlay（フェード等）をリセット
            ResetScreenEffectOverlay();
            // 視覚状態を復元（セーブデータから or スクリプトから再構築）
            if (visualState != null && !string.IsNullOrEmpty(visualState.BackgroundImage))
                RestoreVisualState(visualState);
            else
                RebuildVisualStateFromScript(script, _currentIndex);
            // 選択肢UIが残っていればクリア
            if (IChoiceUI is MonoBehaviour choiceMB)
            {
                var hideMethod = choiceMB.GetType().GetMethod("Hide");
                hideMethod?.Invoke(choiceMB, null);
            }
            Debug.Log($"[Novella] Loaded: {script.Title} @ index {_currentIndex}");
            ExecuteNext();
        }

        public void Play(NovellaScript script)
        {
            _currentScript = script;
            _currentIndex = 0;
            _isRunning = true;
            _pendingAdvanceCallback = null;
            BuildLabelIndex(script);
            Debug.Log($"[Novella] Playing: {script.Title}");
            ExecuteNext();
        }

        /// <summary>回想モードでシーンを再生する。フラグを隔離し、終了ラベルで停止してタイトルに戻る。</summary>
        public void PlayRecollection(string scriptPath, string startLabel, string endLabel)
        {
            // フラグを退避
            _savedFlags = Flags.GetAll();
            _isRecollectionMode = true;
            _recollectionEndLabel = endLabel;

            if (!string.IsNullOrEmpty(startLabel))
            {
                var script = ScriptParser.LoadFromResources(scriptPath);
                if (script == null) { EndRecollection(); return; }
                _currentScriptPath = scriptPath;
                BuildLabelIndex(script);

                int labelIdx = -1;
                for (int i = 0; i < script.Commands.Count; i++)
                {
                    if (script.Commands[i].Type == "label" && script.Commands[i].Label == startLabel)
                    { labelIdx = i; break; }
                }

                if (labelIdx >= 0)
                    LoadAndPlayFrom(scriptPath, labelIdx);
                else
                    LoadAndPlay(scriptPath);
            }
            else
            {
                LoadAndPlay(scriptPath);
            }
        }

        /// <summary>回想モードを終了し、フラグを復元してタイトルに戻る。</summary>
        public void EndRecollection()
        {
            _isRunning = false;
            _isRecollectionMode = false;
            _recollectionEndLabel = null;

            // フラグを復元
            if (_savedFlags != null)
            {
                Flags.SetAll(_savedFlags);
                _savedFlags = null;
            }

            // タイトルに戻る
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene("TitleScene");
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
        }

        /// <summary>ラベル通過時に呼ばれ、回想終了ラベルチェックとシーン自動記録を行う。</summary>
        public bool CheckRecollectionEndLabel(string label)
        {
            if (_isRecollectionMode && !string.IsNullOrEmpty(_recollectionEndLabel) && label == _recollectionEndLabel)
            {
                EndRecollection();
                return true;
            }
            return false;
        }

        /// <summary>ラベル通過時にSceneDefinitionの終了ラベルに到達したらシーンを自動記録する。</summary>
        public void AutoRecordScene(string label)
        {
            if (SceneDefinitions == null || _isRecollectionMode) return;
            foreach (var def in SceneDefinitions)
            {
                if (def != null && !string.IsNullOrEmpty(def.endLabel) && def.endLabel == label)
                {
                    SceneRecollectionManager.RecordScene(def.sceneId, def.title);
                }
            }
        }

        public void RestoreVisualState(VisualState state)
        {
            if (state == null) return;

            // 既存の立ち絵を全てクリア
            ICharacterLayer?.HideAllCharacters();

            // 背景を即座に復元
            if (!string.IsNullOrEmpty(state.BackgroundImage) && IBackground != null)
            {
                _currentBackground = state.BackgroundImage;
                IBackground.Show(state.BackgroundImage, 0f, null, null);
            }

            // キャラクターを即座に復元
            _currentCharacters.Clear();
            if (state.Characters != null && ICharacterLayer != null)
            {
                foreach (var ch in state.Characters)
                {
                    _currentCharacters[ch.CharacterId] = ch;
                    ICharacterLayer.ShowCharacter(ch.CharacterId, ch.Expression, ch.Position, 0f, null, -1, null);
                }
            }

            // BGMを復元
            if (!string.IsNullOrEmpty(state.BgmClip) && IAudio != null)
            {
                _currentBgmClip = state.BgmClip;
                _currentBgmVolume = state.BgmVolume;
                IAudio.PlayBgm(state.BgmClip, state.BgmVolume, null);
            }

            // 表示モードを復元
            if (!string.IsNullOrEmpty(state.DisplayMode))
            {
                _currentDisplayMode = state.DisplayMode;
                var mode = state.DisplayMode == "nvl" ? DisplayMode.NVL : DisplayMode.ADV;
                IMessageWindow?.SetDisplayMode(mode);
            }
        }

        /// <summary>
        /// スクリプトのコマンドを遡って視覚状態を復元する。
        /// 旧セーブデータ（VisualStateなし）の場合のフォールバック。
        /// </summary>
        private void RebuildVisualStateFromScript(NovellaScript script, int upToIndex)
        {
            string bgImage = null;
            string bgmClip = null;
            float bgmVolume = 1f;
            string displayMode = "adv";
            var chars = new Dictionary<string, CharacterState>();

            for (int i = 0; i < upToIndex && i < script.Commands.Count; i++)
            {
                var cmd = script.Commands[i];
                switch (cmd.Type)
                {
                    case "show_bg":
                        bgImage = cmd.Image;
                        break;
                    case "show_char":
                        string charId = cmd.Character ?? cmd.Image;
                        if (!string.IsNullOrEmpty(charId))
                        {
                            if (chars.TryGetValue(charId, out var existing))
                            {
                                if (!string.IsNullOrEmpty(cmd.Expression)) existing.Expression = cmd.Expression;
                                if (!string.IsNullOrEmpty(cmd.Position)) existing.Position = cmd.Position;
                            }
                            else
                            {
                                chars[charId] = new CharacterState
                                {
                                    CharacterId = charId,
                                    Expression = cmd.Expression,
                                    Position = cmd.Position ?? "center"
                                };
                            }
                        }
                        break;
                    case "hide_char":
                        string hideId = cmd.Character ?? cmd.Image;
                        if (!string.IsNullOrEmpty(hideId)) chars.Remove(hideId);
                        break;
                    case "move_char":
                        string moveId = cmd.Character ?? cmd.Image;
                        if (!string.IsNullOrEmpty(moveId) && chars.TryGetValue(moveId, out var mc))
                        {
                            if (!string.IsNullOrEmpty(cmd.Position)) mc.Position = cmd.Position;
                        }
                        break;
                    case "play_bgm":
                        bgmClip = cmd.Clip ?? cmd.Image;
                        bgmVolume = cmd.Volume > 0 ? cmd.Volume : 1f;
                        break;
                    case "stop_bgm":
                        bgmClip = null;
                        bgmVolume = 1f;
                        break;
                    case "set_mode":
                        displayMode = (cmd.Value ?? "adv").ToLower();
                        break;
                }
            }

            var rebuilt = new VisualState
            {
                BackgroundImage = bgImage,
                BgmClip = bgmClip,
                BgmVolume = bgmVolume,
                Characters = new System.Collections.Generic.List<CharacterState>(chars.Values),
                DisplayMode = displayMode,
            };
            RestoreVisualState(rebuilt);
        }

        private void ResetScreenEffectOverlay()
        {
            Canvas canvas = null;
            if (Background != null)
                canvas = Background.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                var canvasGo = GameObject.Find("NovellaCanvas");
                if (canvasGo != null) canvas = canvasGo.GetComponent<Canvas>();
            }
            if (canvas == null) return;

            var overlay = canvas.transform.Find("ScreenEffectOverlay");
            if (overlay != null)
            {
                overlay.gameObject.SetActive(false);
                var img = overlay.GetComponent<Image>();
                if (img != null) img.color = new Color(0, 0, 0, 0);
            }
        }

        private void BuildLabelIndex(NovellaScript script)
        {
            _labelIndex.Clear();
            for (int i = 0; i < script.Commands.Count; i++)
            {
                var cmd = script.Commands[i];
                if (cmd.Type == "label" && !string.IsNullOrEmpty(cmd.Label))
                    _labelIndex[cmd.Label] = i;
            }
        }

        public void JumpToLabel(string label)
        {
            if (_labelIndex.TryGetValue(label, out int index))
                _currentIndex = index;
            else
                Debug.LogWarning($"[Novella] Label not found: '{label}'");
        }

        public void Stop()
        {
            _isRunning = false;
            _pendingAdvanceCallback = null;
        }

        /// <summary>
        /// コマンドハンドラがユーザー入力待ちにするために呼ぶ。
        /// ゲーム画面のスクリーンショットをキャッシュする（セーブサムネイル用）。
        /// </summary>
        public void WaitForInput(Action onAdvance)
        {
            _pendingAdvanceCallback = onAdvance;
            StartCoroutine(SaveManager.CacheScreenshot());
        }

        private void ExecuteNext()
        {
            if (!_isRunning) return;

            if (_currentScript == null || _currentIndex >= _currentScript.Commands.Count)
            {
                Debug.Log("[Novella] Script finished.");
                _isRunning = false;
                return;
            }

            int execIndex = _currentIndex;
            var command = _currentScript.Commands[_currentIndex++];

            // 既読判定 → 既読マーク
            CurrentCommandWasRead = ReadManager.IsRead(_currentScriptPath, execIndex);
            ReadManager.MarkRead(_currentScriptPath, execIndex);

            if (_handlers.TryGetValue(command.Type, out var handler))
            {
                try
                {
                    handler.Execute(command, this, ExecuteNext);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Novella] Error in '{command.Type}' (index {_currentIndex - 1}): {e.Message}");
                    ExecuteNext();
                }
            }
            else
            {
                Debug.LogWarning($"[Novella] Unknown command: '{command.Type}'. Skipping.");
                ExecuteNext();
            }
        }

        private void Update()
        {
            // メニュー・サブパネル・バックログが開いている間はゲーム入力を無視
            if (IMenuUI != null && IMenuUI.IsBlockingInput) return;
            if (BacklogUI != null && BacklogUI.IsOpen) return;

            // ウィンドウ非表示モード：Hキーまたは右クリックで切り替え
            if (Input.GetKeyDown(KeyCode.H) || Input.GetMouseButtonDown(1))
            {
                ToggleWindowHide();
                return;
            }
            // ウィンドウ非表示中は左クリック/スペース/Enterで復帰
            if (_windowHidden)
            {
                bool restore = Input.GetMouseButtonDown(0)
                    || Input.GetKeyDown(KeyCode.Space)
                    || Input.GetKeyDown(KeyCode.Return);
                if (restore)
                    ToggleWindowHide();
                return;
            }

            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            // Aキーでオートモード切り替え
            if (Input.GetKeyDown(KeyCode.A))
                ToggleAutoMode();

            // スキップ：Ctrl押しっぱなし or スキップモードON
            // 未読スキップOFFの場合、現在のコマンドが初見（未読）ならスキップしない
            bool canSkip = ctrl || _skipMode;
            if (canSkip && !SettingsData.SkipUnread && !CurrentCommandWasRead)
            {
                canSkip = false;
                // 未読到達時にスキップモードを自動解除
                if (_skipMode)
                {
                    _skipMode = false;
                    OnSkipModeChanged?.Invoke(false);
                }
            }

            if (canSkip)
            {
                if (IMessageWindow != null && IMessageWindow.IsTyping)
                {
                    IMessageWindow.SkipTyping();
                    return;
                }
                if (_pendingAdvanceCallback != null)
                {
                    _autoTimer = -1f;
                    var cb = _pendingAdvanceCallback;
                    _pendingAdvanceCallback = null;
                    cb.Invoke();
                    return;
                }
            }

            // オートモード：タイピング完了後にタイマーで自動進行（ボイス再生中は待機）
            bool voicePlaying = IAudio != null && IAudio.IsVoicePlaying;
            if (_autoMode && _pendingAdvanceCallback != null && IMessageWindow != null && !IMessageWindow.IsTyping && !voicePlaying)
            {
                if (_autoTimer < 0f) _autoTimer = _autoDelay;
                _autoTimer -= Time.deltaTime;
                if (_autoTimer <= 0f)
                {
                    _autoTimer = -1f;
                    var cb = _pendingAdvanceCallback;
                    _pendingAdvanceCallback = null;
                    cb.Invoke();
                    return;
                }
            }

            // マウスホイール下で既読部分を高速送り
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (wheel < 0f && _pendingAdvanceCallback != null
                && _currentScript != null && _currentIndex > 0
                && ReadManager.IsRead(_currentScriptPath, _currentIndex))
            {
                if (IMessageWindow != null && IMessageWindow.IsTyping)
                    IMessageWindow.SkipTyping();
                _autoTimer = -1f;
                var cb = _pendingAdvanceCallback;
                _pendingAdvanceCallback = null;
                cb.Invoke();
                return;
            }

            // 通常クリック/スペース/エンター
            bool mouseClick = Input.GetMouseButtonDown(0) && !UIInputUtil.IsPointerOverInteractableUI();
            bool advance = mouseClick
                        || Input.GetKeyDown(KeyCode.Space)
                        || Input.GetKeyDown(KeyCode.Return);

            if (!advance) return;

            if (IMessageWindow != null && IMessageWindow.IsTyping)
            {
                IMessageWindow.SkipTyping();
                _autoTimer = -1f;
                return;
            }

            if (_pendingAdvanceCallback != null)
            {
                _autoTimer = -1f;
                var cb = _pendingAdvanceCallback;
                _pendingAdvanceCallback = null;
                cb.Invoke();
            }
        }

        private void UpdateAutoLabel()
        {
            if (AutoLabel == null) return;
            if (_autoMode)
            {
                AutoLabel.text = "AUTO";
                AutoLabel.gameObject.SetActive(true);
            }
            else
            {
                AutoLabel.text = "";
                AutoLabel.gameObject.SetActive(false);
            }
        }

        // フラグ展開用正規表現: {flag_name}
        private static readonly Regex FlagPlaceholderRegex =
            new Regex(@"\{([a-zA-Z_][a-zA-Z0-9_]*)\}", RegexOptions.Compiled);

        /// <summary>
        /// テキスト中の {flag_name} をフラグ値に展開する。
        /// 未設定フラグはそのまま残す。
        /// </summary>
        public string ResolveText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            // ローカライズ展開: #key# → 翻訳値
            text = LocalizationManager.Instance.Resolve(text);
            // リッチテキスト変換: {b},{i},{c:},{size:} → TMProタグ
            text = RichTextProcessor.Convert(text);
            // フラグ展開: {flag_name} → フラグ値
            return FlagPlaceholderRegex.Replace(text, m =>
            {
                string flagName = m.Groups[1].Value;
                // インラインコマンド (w, s, sr) は展開しない
                if (flagName == "w" || flagName == "s" || flagName == "sr") return m.Value;
                string val = Flags.Get(flagName);
                return val ?? m.Value;
            });
        }

    }
}
