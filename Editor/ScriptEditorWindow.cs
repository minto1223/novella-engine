#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Novella.Core;
using UnityEditor;
using UnityEngine;

public class ScriptEditorWindow : EditorWindow
{
    // --- Script list ---
    private List<string> _scriptPaths = new List<string>();
    private string[] _scriptNames = new string[0];
    private int _selectedScript = -1;

    // --- Editing state ---
    private NovellaScript _script;
    private Vector2 _listScroll;
    private Vector2 _cmdScroll;
    private int _selectedCmd = -1;
    private bool _dirty;

    // --- Ruby insert helper ---
    private bool _showRubyPopup;
    private string _rubyBase = "";
    private string _rubyText = "";

    // --- Character Definitions ---
    private bool _showCharDefs;
    private CharacterDefinition[] _charDefs;
    private Vector2 _charDefScroll;

    // --- Splitter ---
    private float _splitter1 = 150f;
    private float _splitter2Ratio = 0.55f; // command list ratio of remaining space
    private bool _draggingSplitter1;
    private bool _draggingSplitter2;

    // --- Command type list ---
    private static readonly string[] CommandTypes = {
        "say", "show_bg", "show_char", "hide_char", "move_char",
        "wait", "play_bgm", "stop_bgm", "fade_bgm", "play_se", "stop_se",
        "play_voice", "stop_voice", "set_volume",
        "set_flag", "add_flag", "label", "jump", "jump_if", "jump_unless",
        "choice", "next_script", "shake", "flash", "fade", "show_title",
        "zoom", "pan", "reset_camera", "input_text",
        "play_particle", "stop_particle", "set_language",
        "play_movie", "stop_movie", "ken_burns", "stop_ken_burns",
        "calc", "set_mode", "clear", "end"
    };

    // --- Field definitions per command type ---
    private static readonly Dictionary<string, string[]> FieldsByType = new Dictionary<string, string[]>
    {
        { "say",          new[] { "character", "text", "clip" } },
        { "show_bg",      new[] { "image", "duration", "value" } },
        { "show_char",    new[] { "character", "expression", "position", "value", "order", "layer" } },
        { "hide_char",    new[] { "character", "value" } },
        { "move_char",    new[] { "character", "position", "duration", "layer" } },
        { "wait",         new[] { "duration" } },
        { "play_bgm",    new[] { "clip", "volume", "value", "duration" } },
        { "stop_bgm",    new[] { "duration" } },
        { "fade_bgm",   new[] { "value", "duration" } },
        { "play_se",     new[] { "clip", "volume" } },
        { "stop_se",     new string[0] },
        { "play_voice",  new[] { "clip", "volume" } },
        { "stop_voice",  new string[0] },
        { "set_flag",    new[] { "label", "value" } },
        { "add_flag",    new[] { "target", "value" } },
        { "label",       new[] { "label" } },
        { "jump",        new[] { "target" } },
        { "jump_if",     new[] { "label", "target" } },
        { "jump_unless", new[] { "label", "target" } },
        { "choice",      new string[0] },
        { "next_script", new[] { "target" } },
        { "shake",       new[] { "duration", "value" } },
        { "flash",       new[] { "duration", "value" } },
        { "fade",        new[] { "duration", "value", "target" } },
        { "show_title",  new[] { "text", "duration" } },
        { "zoom",        new[] { "value", "duration" } },
        { "pan",         new[] { "target", "duration" } },
        { "reset_camera", new[] { "duration" } },
        { "input_text",  new[] { "target", "text", "value" } },
        { "play_particle", new[] { "value", "duration" } },
        { "stop_particle", new[] { "value", "duration" } },
        { "set_language", new[] { "value" } },
        { "play_movie",  new[] { "clip" } },
        { "stop_movie",  new string[0] },
        { "calc",        new[] { "target", "value" } },
        { "set_mode",    new[] { "value" } },
        { "ken_burns",   new[] { "value", "position", "duration" } },
        { "stop_ken_burns", new[] { "duration" } },
        { "set_volume",  new[] { "target", "value" } },
        { "clear",       new string[0] },
        { "end",         new string[0] },
    };

    [MenuItem("Novella/Script Editor")]
    public static void ShowWindow()
    {
        GetWindow<ScriptEditorWindow>("Script Editor");
    }

    private void OnEnable()
    {
        RefreshScriptList();
        RefreshCharDefs();
    }

    private void RefreshScriptList()
    {
        var dir = Path.Combine(Application.dataPath, "Novella/Resources/Scripts");
        _scriptPaths.Clear();
        if (Directory.Exists(dir))
        {
            _scriptPaths = Directory.GetFiles(dir, "*.json")
                .OrderBy(p => p).ToList();
        }
        _scriptNames = _scriptPaths.Select(p => Path.GetFileNameWithoutExtension(p)).ToArray();
    }

    private void OnGUI()
    {
        // Flowchart からのジャンプ受信
        int jumpTo = EditorPrefs.GetInt("Novella_ScriptEditor_JumpTo", -1);
        if (jumpTo >= 0)
        {
            EditorPrefs.DeleteKey("Novella_ScriptEditor_JumpTo");
            if (_script != null && jumpTo < _script.Commands.Count)
                _selectedCmd = jumpTo;
        }

        EditorGUILayout.BeginHorizontal();

        const float splitterW = 4f;
        float totalWidth = position.width;
        float remaining = totalWidth - _splitter1 - splitterW * 2;
        float cmdListW = Mathf.Max(100, remaining * _splitter2Ratio);
        float detailW = Mathf.Max(100, remaining - cmdListW);

        // ===== Left: Script list =====
        EditorGUILayout.BeginVertical(GUILayout.Width(_splitter1));
        DrawScriptList();
        EditorGUILayout.EndVertical();

        // --- Splitter 1 ---
        DrawSplitter(ref _draggingSplitter1, ref _splitter1, 80f, totalWidth * 0.4f, splitterW);

        // ===== Center: Command list =====
        EditorGUILayout.BeginVertical(GUILayout.Width(cmdListW));
        DrawCommandList();
        EditorGUILayout.EndVertical();

        // --- Splitter 2 ---
        var s2Rect = DrawSplitterHandle(ref _draggingSplitter2, splitterW);
        if (_draggingSplitter2)
        {
            float mouseX = Event.current.mousePosition.x;
            float s2Start = _splitter1 + splitterW;
            float relX = mouseX - s2Start;
            float ratio = relX / remaining;
            _splitter2Ratio = Mathf.Clamp(ratio, 0.2f, 0.8f);
            Repaint();
        }

        // ===== Right: Command detail =====
        EditorGUILayout.BeginVertical(GUILayout.Width(detailW));
        DrawCommandDetail();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    // ===================== Script List =====================
    private void DrawScriptList()
    {
        EditorGUILayout.LabelField("Scripts", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh"))
            RefreshScriptList();

        _listScroll = EditorGUILayout.BeginScrollView(_listScroll);
        for (int i = 0; i < _scriptNames.Length; i++)
        {
            bool selected = i == _selectedScript;
            var style = selected ? EditorStyles.boldLabel : EditorStyles.label;
            if (GUILayout.Button(_scriptNames[i], style))
            {
                if (ConfirmDiscard())
                    LoadScript(i);
            }
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        if (GUILayout.Button("+ New Script"))
            CreateNewScript();

        EditorGUILayout.Space(8);
        DrawCharacterDefinitions();
    }

    // ===================== Command List =====================
    private void DrawCommandList()
    {
        EditorGUILayout.LabelField("Commands", EditorStyles.boldLabel);

        if (_script == null)
        {
            EditorGUILayout.HelpBox("Select a script.", MessageType.Info);
            return;
        }

        // Title
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Title:", GUILayout.Width(40));
        string newTitle = EditorGUILayout.TextField(_script.Title);
        if (newTitle != _script.Title) { _script.Title = newTitle; _dirty = true; }
        EditorGUILayout.EndHorizontal();

        // Toolbar
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+Add", GUILayout.Width(50)))
        {
            _script.Commands.Add(new ScriptCommand { Type = "say" });
            _selectedCmd = _script.Commands.Count - 1;
            _dirty = true;
        }
        GUI.enabled = _selectedCmd >= 0;
        if (GUILayout.Button("Dup", GUILayout.Width(40)))
            DuplicateCommand();
        if (GUILayout.Button("Del", GUILayout.Width(40)))
            DeleteCommand();
        if (GUILayout.Button("Up", GUILayout.Width(30)) && _selectedCmd > 0)
            SwapCommands(_selectedCmd, _selectedCmd - 1);
        if (GUILayout.Button("Dn", GUILayout.Width(30)) && _selectedCmd < _script.Commands.Count - 1)
            SwapCommands(_selectedCmd, _selectedCmd + 1);
        GUI.enabled = true;

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("▶ Preview", GUILayout.Width(70)))
            StartPreview();
        var saveStyle = _dirty ? new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold } : GUI.skin.button;
        if (GUILayout.Button(_dirty ? "SAVE*" : "Save", saveStyle, GUILayout.Width(60)))
            SaveScript();
        EditorGUILayout.EndHorizontal();

        // Command list
        _cmdScroll = EditorGUILayout.BeginScrollView(_cmdScroll);
        for (int i = 0; i < _script.Commands.Count; i++)
        {
            var cmd = _script.Commands[i];
            bool isSel = i == _selectedCmd;

            var bgColor = GUI.backgroundColor;
            if (isSel) GUI.backgroundColor = new Color(0.4f, 0.7f, 1f);

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUI.backgroundColor = bgColor;

            EditorGUILayout.LabelField($"{i:000}", GUILayout.Width(30));
            EditorGUILayout.LabelField(cmd.Type ?? "???", EditorStyles.boldLabel, GUILayout.Width(90));

            string preview = GetCommandPreview(cmd);
            EditorGUILayout.LabelField(preview, EditorStyles.miniLabel);

            if (GUILayout.Button(">>", GUILayout.Width(28)))
                _selectedCmd = i;

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    // ===================== Command Detail =====================
    private void DrawCommandDetail()
    {
        EditorGUILayout.LabelField("Detail", EditorStyles.boldLabel);

        if (_script == null || _selectedCmd < 0 || _selectedCmd >= _script.Commands.Count)
        {
            EditorGUILayout.HelpBox("Select a command.", MessageType.Info);
            return;
        }

        var cmd = _script.Commands[_selectedCmd];

        // Type dropdown
        int typeIdx = System.Array.IndexOf(CommandTypes, cmd.Type);
        int newTypeIdx = EditorGUILayout.Popup("Type", Mathf.Max(0, typeIdx), CommandTypes);
        if (newTypeIdx != typeIdx && newTypeIdx >= 0)
        {
            cmd.Type = CommandTypes[newTypeIdx];
            _dirty = true;
        }

        EditorGUILayout.Space();

        // Fields based on type
        if (FieldsByType.TryGetValue(cmd.Type, out var fields))
        {
            foreach (var field in fields)
                DrawField(cmd, field);
        }

        // Choice editor
        if (cmd.Type == "choice")
            DrawChoiceEditor(cmd);
    }

    // ===================== Field drawing =====================
    private void DrawField(ScriptCommand cmd, string field)
    {
        switch (field)
        {
            case "character":
                cmd.Character = StringField("Character", cmd.Character, ref _dirty);
                break;
            case "text":
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Text");
                if (GUILayout.Button("Ruby", GUILayout.Width(50)))
                {
                    _showRubyPopup = !_showRubyPopup;
                    if (_showRubyPopup) { _rubyBase = ""; _rubyText = ""; }
                }
                EditorGUILayout.EndHorizontal();
                if (_showRubyPopup)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    _rubyBase = EditorGUILayout.TextField("Base", _rubyBase);
                    _rubyText = EditorGUILayout.TextField("Ruby", _rubyText);
                    EditorGUILayout.BeginHorizontal();
                    GUI.enabled = !string.IsNullOrEmpty(_rubyBase) && !string.IsNullOrEmpty(_rubyText);
                    if (GUILayout.Button("Insert"))
                    {
                        cmd.Text = (cmd.Text ?? "") + $"{{rb:{_rubyBase},{_rubyText}}}";
                        _dirty = true;
                        _showRubyPopup = false;
                    }
                    GUI.enabled = true;
                    if (GUILayout.Button("Cancel")) _showRubyPopup = false;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
                string newText = EditorGUILayout.TextArea(cmd.Text ?? "", GUILayout.MinHeight(60));
                if (newText != (cmd.Text ?? "")) { cmd.Text = newText; _dirty = true; }
                break;
            case "image":
                cmd.Image = StringField("Image", cmd.Image, ref _dirty);
                break;
            case "position":
                cmd.Position = StringField("Position", cmd.Position, ref _dirty);
                break;
            case "expression":
                cmd.Expression = StringField("Expression", cmd.Expression, ref _dirty);
                break;
            case "clip":
                cmd.Clip = StringField("Clip", cmd.Clip, ref _dirty);
                break;
            case "label":
                cmd.Label = StringField("Label", cmd.Label, ref _dirty);
                break;
            case "target":
                cmd.Target = StringField("Target", cmd.Target, ref _dirty);
                break;
            case "value":
                cmd.Value = StringField("Value", cmd.Value, ref _dirty);
                break;
            case "duration":
                float newDur = EditorGUILayout.FloatField("Duration", cmd.Duration);
                if (newDur != cmd.Duration) { cmd.Duration = newDur; _dirty = true; }
                break;
            case "volume":
                float newVol = EditorGUILayout.FloatField("Volume", cmd.Volume);
                if (newVol != cmd.Volume) { cmd.Volume = newVol; _dirty = true; }
                break;
            case "order":
                int newOrd = EditorGUILayout.IntField("Order", cmd.Order);
                if (newOrd != cmd.Order) { cmd.Order = newOrd; _dirty = true; }
                break;
            case "layer":
                cmd.Layer = StringField("Layer (front/back/num)", cmd.Layer, ref _dirty);
                break;
        }
    }

    private void DrawChoiceEditor(ScriptCommand cmd)
    {
        if (cmd.Choices == null)
            cmd.Choices = new List<ChoiceOption>();

        EditorGUILayout.LabelField("Choices", EditorStyles.boldLabel);

        int removeIdx = -1;
        for (int i = 0; i < cmd.Choices.Count; i++)
        {
            var c = cmd.Choices[i];
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            c.Text = StringField("Text", c.Text, ref _dirty);
            c.Target = StringField("Target", c.Target, ref _dirty);
            c.SetFlag = StringField("SetFlag", c.SetFlag, ref _dirty);
            c.FlagValue = StringField("FlagValue", c.FlagValue, ref _dirty);
            c.Condition = StringField("Condition", c.Condition, ref _dirty);
            if (GUILayout.Button("Remove Choice", GUILayout.Width(120)))
                removeIdx = i;
            EditorGUILayout.EndVertical();
        }

        if (removeIdx >= 0) { cmd.Choices.RemoveAt(removeIdx); _dirty = true; }

        if (GUILayout.Button("+ Add Choice"))
        {
            cmd.Choices.Add(new ChoiceOption());
            _dirty = true;
        }
    }

    // ===================== Character Definitions =====================
    private void RefreshCharDefs()
    {
        var guids = AssetDatabase.FindAssets("t:CharacterDefinition");
        _charDefs = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<CharacterDefinition>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(d => d != null)
            .OrderBy(d => d.characterId)
            .ToArray();
    }

    private void DrawCharacterDefinitions()
    {
        _showCharDefs = EditorGUILayout.Foldout(_showCharDefs, "Characters", true, EditorStyles.foldoutHeader);
        if (!_showCharDefs) return;

        if (GUILayout.Button("Refresh"))
            RefreshCharDefs();

        if (_charDefs == null || _charDefs.Length == 0)
        {
            EditorGUILayout.HelpBox("No CharacterDefinition assets found.", MessageType.Info);
        }
        else
        {
            _charDefScroll = EditorGUILayout.BeginScrollView(_charDefScroll, GUILayout.MaxHeight(200));
            foreach (var def in _charDefs)
            {
                if (def == null) continue;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUI.BeginChangeCheck();
                def.characterId = EditorGUILayout.TextField("ID", def.characterId);
                def.displayName = EditorGUILayout.TextField("Name", def.displayName);
                def.nameColor = EditorGUILayout.ColorField("Color", def.nameColor);
                def.defaultExpression = EditorGUILayout.TextField("Expression", def.defaultExpression);
                if (EditorGUI.EndChangeCheck())
                    EditorUtility.SetDirty(def);

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        if (GUILayout.Button("+ New Character"))
            CreateNewCharDef();
    }

    private void CreateNewCharDef()
    {
        string folder = "Assets/Novella/Data/Characters";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Novella/Data"))
                AssetDatabase.CreateFolder("Assets/Novella", "Data");
            AssetDatabase.CreateFolder("Assets/Novella/Data", "Characters");
        }

        string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/CharDef_New.asset");
        var def = ScriptableObject.CreateInstance<CharacterDefinition>();
        def.characterId = "new_char";
        def.displayName = "New Character";
        def.nameColor = Color.white;
        AssetDatabase.CreateAsset(def, path);
        AssetDatabase.SaveAssets();
        RefreshCharDefs();
    }

    // ===================== Helpers =====================
    private static string StringField(string label, string val, ref bool dirty)
    {
        string newVal = EditorGUILayout.TextField(label, val ?? "");
        if (newVal != (val ?? "")) dirty = true;
        return string.IsNullOrEmpty(newVal) ? null : newVal;
    }

    private string GetCommandPreview(ScriptCommand cmd)
    {
        if (cmd == null) return "";
        switch (cmd.Type)
        {
            case "say":
                string who = cmd.Character ?? "";
                string txt = cmd.Text ?? "";
                if (txt.Length > 30) txt = txt.Substring(0, 30) + "...";
                return string.IsNullOrEmpty(who) ? txt : $"{who}: {txt}";
            case "show_bg": return cmd.Image ?? "";
            case "show_char":
            case "hide_char":
            case "move_char": return $"{cmd.Character} ({cmd.Position})";
            case "label": return cmd.Label ?? "";
            case "jump":
            case "next_script": return cmd.Target ?? "";
            case "jump_if":
            case "jump_unless": return $"{cmd.Label} -> {cmd.Target}";
            case "set_flag":
            case "add_flag": return $"{cmd.Label ?? cmd.Target}={cmd.Value}";
            case "choice": return cmd.Choices != null ? $"{cmd.Choices.Count} choices" : "0 choices";
            case "show_title": return cmd.Text ?? "";
            case "play_bgm":
            case "play_se":
            case "play_voice": return cmd.Clip ?? "";
            case "zoom": return $"x{cmd.Value}";
            case "pan": return cmd.Target ?? "";
            default: return "";
        }
    }

    private void LoadScript(int index)
    {
        _selectedScript = index;
        _selectedCmd = -1;
        string json = File.ReadAllText(_scriptPaths[index]);
        _script = JsonConvert.DeserializeObject<NovellaScript>(json);
        _dirty = false;
    }

    private void StartPreview()
    {
        if (_script == null || _selectedScript < 0)
        {
            Debug.LogWarning("[Novella] Preview: No script selected.");
            return;
        }

        // 未保存なら先に保存
        if (_dirty) SaveScript();

        // スクリプトパスを Resources からの相対パスに変換
        string fullPath = _scriptPaths[_selectedScript];
        string resourcesMarker = "Resources/";
        int idx = fullPath.Replace("\\", "/").IndexOf(resourcesMarker);
        string resourcePath = idx >= 0
            ? fullPath.Replace("\\", "/").Substring(idx + resourcesMarker.Length)
            : "Scripts/" + _scriptNames[_selectedScript];
        // 拡張子を除去
        if (resourcePath.EndsWith(".json"))
            resourcePath = resourcePath.Substring(0, resourcePath.Length - 5);

        // EditorPrefs にプレビュー情報を書き込み
        EditorPrefs.SetString("Novella_PreviewScript", resourcePath);
        EditorPrefs.SetInt("Novella_PreviewIndex", Mathf.Max(0, _selectedCmd));

        // Play Mode 開始
        if (!EditorApplication.isPlaying)
        {
            // SampleScene を開いてから再生
            string sampleScene = "Assets/Scenes/SampleScene.unity";
            if (System.IO.File.Exists(sampleScene))
            {
                UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(sampleScene);
            }
            EditorApplication.isPlaying = true;
        }
    }

    private void SaveScript()
    {
        if (_script == null || _selectedScript < 0) return;

        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
        string json = JsonConvert.SerializeObject(_script, settings);
        File.WriteAllText(_scriptPaths[_selectedScript], json);
        _dirty = false;
        AssetDatabase.Refresh();
        Debug.Log($"[Novella] Saved: {_scriptNames[_selectedScript]}");
    }

    private void CreateNewScript()
    {
        string dir = Path.Combine(Application.dataPath, "Novella/Resources/Scripts");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string name = "new_script";
        string path = Path.Combine(dir, name + ".json");
        int n = 1;
        while (File.Exists(path))
        {
            name = $"new_script_{n++}";
            path = Path.Combine(dir, name + ".json");
        }

        var newScript = new NovellaScript { Title = name, Commands = new List<ScriptCommand>() };
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
        File.WriteAllText(path, JsonConvert.SerializeObject(newScript, settings));
        AssetDatabase.Refresh();
        RefreshScriptList();

        int idx = _scriptPaths.IndexOf(path);
        if (idx >= 0) LoadScript(idx);
    }

    private void DeleteCommand()
    {
        if (_selectedCmd < 0 || _selectedCmd >= _script.Commands.Count) return;
        _script.Commands.RemoveAt(_selectedCmd);
        _dirty = true;
        if (_selectedCmd >= _script.Commands.Count)
            _selectedCmd = _script.Commands.Count - 1;
    }

    private void DuplicateCommand()
    {
        if (_selectedCmd < 0 || _selectedCmd >= _script.Commands.Count) return;
        var src = _script.Commands[_selectedCmd];
        var json = JsonConvert.SerializeObject(src);
        var copy = JsonConvert.DeserializeObject<ScriptCommand>(json);
        _script.Commands.Insert(_selectedCmd + 1, copy);
        _selectedCmd++;
        _dirty = true;
    }

    private void SwapCommands(int a, int b)
    {
        var tmp = _script.Commands[a];
        _script.Commands[a] = _script.Commands[b];
        _script.Commands[b] = tmp;
        _selectedCmd = b;
        _dirty = true;
    }

    private bool ConfirmDiscard()
    {
        if (!_dirty) return true;
        return EditorUtility.DisplayDialog("Unsaved Changes",
            "Save changes before switching?", "Discard", "Cancel");
    }

    // ===================== Splitter =====================
    private void DrawSplitter(ref bool dragging, ref float value, float min, float max, float width)
    {
        var rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(width), GUILayout.ExpandHeight(true));
        rect.width = width;
        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));
        EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);

        var e = Event.current;
        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            dragging = true;
            e.Use();
        }
        if (dragging && e.type == EventType.MouseDrag)
        {
            value = Mathf.Clamp(e.mousePosition.x, min, max);
            Repaint();
            e.Use();
        }
        if (e.type == EventType.MouseUp && dragging)
        {
            dragging = false;
            e.Use();
        }
    }

    private Rect DrawSplitterHandle(ref bool dragging, float width)
    {
        var rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(width), GUILayout.ExpandHeight(true));
        rect.width = width;
        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));
        EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);

        var e = Event.current;
        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            dragging = true;
            e.Use();
        }
        if (e.type == EventType.MouseUp && dragging)
        {
            dragging = false;
            e.Use();
        }
        return rect;
    }
}
#endif
