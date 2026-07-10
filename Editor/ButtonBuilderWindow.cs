#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Novella.Core;
using Novella.UI;

/// <summary>
/// Novella > Button Builder
/// TitleScene / SampleScene のボタンを自由に追加・削除できるEditorWindow。
/// </summary>
public class ButtonBuilderWindow : EditorWindow
{
    // ---- シーンタブ ----
    private int _tab = 0;
    private readonly string[] _tabNames = { "Title", "Game" };

    // ---- モード ----
    private int _mode = 0; // 0=追加, 1=削除
    private readonly string[] _modeNames = { "追加", "削除" };

    // ---- Title タブ定義 ----
    private static readonly string[] TitleFunctions =
    {
        "New Game", "Continue", "Quit", "Reset",
        "CG Gallery", "BGM Gallery", "Scene Recollection",
        "Chapter Select", "Ending List", "Flowchart", "Settings"
    };
    private static readonly string[] TitleFields =
    {
        "_newGameButton", "_continueButton", "_quitButton", "_resetButton",
        "_galleryButton", "_bgmGalleryButton", "_recollectionButton",
        "_chapterSelectButton", "_endingListButton", "_flowchartButton", "_settingsButton"
    };
    private static readonly string[] TitleDefaultLabels =
    {
        "NEW GAME", "CONTINUE", "QUIT", "RESET",
        "CG", "BGM", "RECOLLECT", "CHAPTER", "ENDING", "MAP", "OPTION"
    };

    // ---- Game タブ定義 ----
    private static readonly string[] GameFunctions =
    {
        "Quick Save", "Quick Load", "Save", "Load",
        "Auto", "Skip", "Backlog", "Menu"
    };
    private static readonly string[] GameFields =
    {
        "_quickSaveButton", "_quickLoadButton", "_saveButton", "_loadButton",
        "_autoButton", "_skipButton", "_backlogButton", "_menuButton"
    };
    private static readonly string[] GameDefaultLabels =
    {
        "QS", "QL", "SAVE", "LOAD", "AUTO", "SKIP", "LOG", "MENU"
    };

    // ---- 追加モード パラメータ ----
    private int _funcIndex = 0;
    private GameObject _parentTarget = null;
    private bool _useImage = false;
    private string _labelText = "";
    private Sprite _buttonSprite = null;
    private Color _buttonColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    private Color _textColor = Color.white;
    private float _fontSize = 36f;
    private bool _freeLayout = false; // LayoutGroup無視・自由配置

    // ---- 削除モード パラメータ ----
    private int _removeIndex = 0;

    private const string FontPath = "Assets/font_1_kokugl_1.asset";

    [MenuItem("Novella/Button Builder")]
    public static void Open()
    {
        var win = GetWindow<ButtonBuilderWindow>("Button Builder");
        win.minSize = new Vector2(360, 440);
        win.AutoDetectTab();
    }

    private void OnEnable() => AutoDetectTab();

    private void AutoDetectTab()
    {
        string scene = EditorSceneManager.GetActiveScene().name;
        _tab = scene == "TitleScene" ? 0 : 1;
        ResetParams();
    }

    private void ResetParams()
    {
        _funcIndex = 0;
        _removeIndex = 0;
        _parentTarget = null;
        _useImage = false;
        _buttonSprite = null;
        _buttonColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        _textColor = Color.white;
        _fontSize = 36f;
        UpdateDefaultLabel();
    }

    private void UpdateDefaultLabel()
    {
        if (_tab == 0 && _funcIndex < TitleDefaultLabels.Length)
            _labelText = TitleDefaultLabels[_funcIndex];
        else if (_tab == 1 && _funcIndex < GameDefaultLabels.Length)
            _labelText = GameDefaultLabels[_funcIndex];
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(6);

        // シーンタブ
        int newTab = GUILayout.Toolbar(_tab, _tabNames);
        if (newTab != _tab) { _tab = newTab; ResetParams(); }

        EditorGUILayout.Space(6);

        // 追加/削除モード
        int newMode = GUILayout.Toolbar(_mode, _modeNames);
        if (newMode != _mode) { _mode = newMode; _removeIndex = 0; }

        EditorGUILayout.Space(10);

        if (_mode == 0)
            DrawAddMode();
        else
            DrawRemoveMode();
    }

    // =========================================================
    // 追加モード
    // =========================================================
    private void DrawAddMode()
    {
        string[] functions = _tab == 0 ? TitleFunctions : GameFunctions;

        EditorGUI.BeginChangeCheck();
        _funcIndex = EditorGUILayout.Popup("機能", _funcIndex, functions);
        if (EditorGUI.EndChangeCheck()) UpdateDefaultLabel();

        EditorGUILayout.Space(6);

        EditorGUILayout.LabelField("配置先", EditorStyles.boldLabel);
        _parentTarget = (GameObject)EditorGUILayout.ObjectField(
            "Parent GameObject", _parentTarget, typeof(GameObject), true);

        string autoPath = _tab == 0 ? "TitleCanvas/ButtonRow" : "NovellaCanvas/CameraRoot/HUDPanel";
        EditorGUILayout.HelpBox($"未指定の場合は自動で {autoPath} に配置します。", MessageType.Info);

        EditorGUILayout.Space(6);

        EditorGUILayout.LabelField("ボタンの見た目", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Toggle(!_useImage, "テキスト", EditorStyles.radioButton)) _useImage = false;
        if (GUILayout.Toggle(_useImage, "画像", EditorStyles.radioButton)) _useImage = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        if (!_useImage)
        {
            _labelText = EditorGUILayout.TextField("表示テキスト", _labelText);
            _textColor = EditorGUILayout.ColorField("文字色", _textColor);
            _fontSize = EditorGUILayout.FloatField("フォントサイズ", _fontSize);
        }
        else
        {
            _buttonSprite = (Sprite)EditorGUILayout.ObjectField(
                "ボタン画像", _buttonSprite, typeof(Sprite), false);
        }

        EditorGUILayout.Space(4);
        _buttonColor = EditorGUILayout.ColorField("ボタン色", _buttonColor);

        EditorGUILayout.Space(8);

        _freeLayout = EditorGUILayout.ToggleLeft("自由配置（レイアウトグループを無視）", _freeLayout);
        if (_freeLayout)
            EditorGUILayout.HelpBox("追加後にSceneビューまたはInspectorで位置・サイズを自由に調整できます。", MessageType.Info);

        EditorGUILayout.Space(8);

        GUI.backgroundColor = new Color(0.3f, 0.7f, 0.3f);
        if (GUILayout.Button("ボタンを追加", GUILayout.Height(36)))
            AddButton();
        GUI.backgroundColor = Color.white;
    }

    // =========================================================
    // 削除モード
    // =========================================================
    private void DrawRemoveMode()
    {
        var configured = GetConfiguredButtons();

        if (configured.Count == 0)
        {
            EditorGUILayout.HelpBox("配線済みのボタンがありません。", MessageType.Info);
            return;
        }

        // ラベル一覧を生成（"機能名 : GameObjectName"）
        var labels = new string[configured.Count];
        for (int i = 0; i < configured.Count; i++)
            labels[i] = $"{configured[i].funcName}  :  {configured[i].button?.name ?? "(null)"}";

        EditorGUILayout.LabelField("削除するボタンを選択", EditorStyles.boldLabel);
        _removeIndex = Mathf.Clamp(_removeIndex, 0, configured.Count - 1);
        _removeIndex = EditorGUILayout.Popup(_removeIndex, labels);

        EditorGUILayout.Space(8);

        var target = configured[_removeIndex];

        // 対象ボタンの情報を表示
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("機能", target.funcName);
        EditorGUILayout.LabelField("GameObject", target.button != null ? target.button.name : "(null)");
        EditorGUILayout.LabelField("フィールド", target.fieldName);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        GUI.backgroundColor = new Color(0.8f, 0.25f, 0.25f);
        if (GUILayout.Button("削除する", GUILayout.Height(36)))
        {
            if (EditorUtility.DisplayDialog(
                "ボタン削除",
                $"「{target.funcName}」ボタン ({target.button?.name}) を削除しますか？\nコントローラーの参照もクリアされます。",
                "削除", "キャンセル"))
            {
                RemoveButton(target);
                _removeIndex = 0;
            }
        }
        GUI.backgroundColor = Color.white;
    }

    // =========================================================
    // 配線済みボタン取得
    // =========================================================
    private struct ButtonEntry
    {
        public string funcName;
        public string fieldName;
        public Button button;
    }

    private List<ButtonEntry> GetConfiguredButtons()
    {
        var result = new List<ButtonEntry>();

        if (_tab == 0)
        {
            var manager = Object.FindFirstObjectByType<TitleManager>();
            if (manager == null) return result;

            var so = new SerializedObject(manager);
            for (int i = 0; i < TitleFields.Length; i++)
            {
                var prop = so.FindProperty(TitleFields[i]);
                if (prop == null || prop.objectReferenceValue == null) continue;
                var btn = prop.objectReferenceValue as Button;
                result.Add(new ButtonEntry
                {
                    funcName = TitleFunctions[i],
                    fieldName = TitleFields[i],
                    button = btn
                });
            }
        }
        else
        {
            var hud = Object.FindFirstObjectByType<HUDController>();
            if (hud == null) return result;

            var so = new SerializedObject(hud);
            for (int i = 0; i < GameFields.Length; i++)
            {
                var prop = so.FindProperty(GameFields[i]);
                if (prop == null || prop.objectReferenceValue == null) continue;
                var btn = prop.objectReferenceValue as Button;
                result.Add(new ButtonEntry
                {
                    funcName = GameFunctions[i],
                    fieldName = GameFields[i],
                    button = btn
                });
            }
        }

        return result;
    }

    // =========================================================
    // 削除実行
    // =========================================================
    private void RemoveButton(ButtonEntry entry)
    {
        // コントローラーの参照をクリア
        if (_tab == 0)
        {
            var manager = Object.FindFirstObjectByType<TitleManager>();
            if (manager != null)
            {
                var so = new SerializedObject(manager);
                var prop = so.FindProperty(entry.fieldName);
                if (prop != null) { prop.objectReferenceValue = null; so.ApplyModifiedProperties(); }
                EditorUtility.SetDirty(manager);
            }
        }
        else
        {
            var hud = Object.FindFirstObjectByType<HUDController>();
            if (hud != null)
            {
                var so = new SerializedObject(hud);
                var prop = so.FindProperty(entry.fieldName);
                if (prop != null) { prop.objectReferenceValue = null; so.ApplyModifiedProperties(); }
                EditorUtility.SetDirty(hud);
            }
        }

        // GameObjectを削除
        if (entry.button != null)
        {
            string name = entry.button.name;
            DestroyImmediate(entry.button.gameObject);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[Novella] {name} を削除しました。");
        }
    }

    // =========================================================
    // 追加実行
    // =========================================================
    private void AddButton()
    {
        string[] functions = _tab == 0 ? TitleFunctions : GameFunctions;
        string funcName = functions[_funcIndex];

        Transform parent = ResolveParent();
        if (parent == null) return;

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        string btnName = funcName.Replace(" ", "") + "Button";
        var existing = parent.Find(btnName);
        if (existing != null) DestroyImmediate(existing.gameObject);

        var go = new GameObject(btnName);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = GetReferenceSize(parent);

        var img = go.AddComponent<Image>();
        if (_useImage && _buttonSprite != null)
        {
            img.sprite = _buttonSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }
        else
        {
            img.color = _buttonColor;
        }

        var btn = go.AddComponent<Button>();

        // 自由配置モード: LayoutGroupの制御を無効化
        if (_freeLayout)
        {
            var le = go.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
        }

        if (!_useImage)
        {
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = _labelText;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = _fontSize;
            tmp.color = _textColor;
            if (font != null) tmp.font = font;
        }

        WireButton(btn, funcName);

        EditorUtility.SetDirty(parent.gameObject);
        EditorSceneManager.MarkSceneDirty(parent.gameObject.scene);
        Debug.Log($"[Novella] {btnName} を {parent.name} に追加しました。");
    }

    private Transform ResolveParent()
    {
        if (_parentTarget != null) return _parentTarget.transform;

        string autoPath = _tab == 0 ? "TitleCanvas/ButtonRow" : "NovellaCanvas/CameraRoot/HUDPanel";
        var go = GameObject.Find(autoPath);
        if (go != null) return go.transform;

        // CameraRootが無い旧構成へのフォールバック
        if (_tab != 0)
        {
            var legacy = GameObject.Find("NovellaCanvas/HUDPanel");
            if (legacy != null) return legacy.transform;
        }

        Debug.LogError($"[Novella] {autoPath} が見つかりません。配置先を手動で指定してください。");
        return null;
    }

    private Vector2 GetReferenceSize(Transform parent)
    {
        foreach (Transform child in parent)
        {
            var rt = child.GetComponent<RectTransform>();
            if (rt != null && child.GetComponent<Button>() != null)
                return rt.sizeDelta;
        }
        return _tab == 0 ? new Vector2(200f, 60f) : new Vector2(68f, 40f);
    }

    private void WireButton(Button btn, string funcName)
    {
        if (_tab == 0)
        {
            var manager = Object.FindFirstObjectByType<TitleManager>();
            if (manager == null) { Debug.LogWarning("[Novella] TitleManager が見つかりません。手動で配線してください。"); return; }

            int idx = System.Array.IndexOf(TitleFunctions, funcName);
            if (idx < 0 || idx >= TitleFields.Length) return;

            var so = new SerializedObject(manager);
            var prop = so.FindProperty(TitleFields[idx]);
            if (prop != null) { prop.objectReferenceValue = btn; so.ApplyModifiedProperties(); }
            EditorUtility.SetDirty(manager);
        }
        else
        {
            var hud = Object.FindFirstObjectByType<HUDController>();
            if (hud == null) { Debug.LogWarning("[Novella] HUDController が見つかりません。手動で配線してください。"); return; }

            int idx = System.Array.IndexOf(GameFunctions, funcName);
            if (idx < 0 || idx >= GameFields.Length) return;

            var so = new SerializedObject(hud);
            var prop = so.FindProperty(GameFields[idx]);
            if (prop != null) { prop.objectReferenceValue = btn; so.ApplyModifiedProperties(); }
            EditorUtility.SetDirty(hud);
        }
    }
}
#endif
