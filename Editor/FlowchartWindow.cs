#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Novella.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Novella.Editor
{
    public class FlowchartWindow : EditorWindow
    {
        private FlowchartGraphView _graphView;
        private List<string> _scriptPaths = new List<string>();
        private string[] _scriptNames = new string[0];
        private int _selectedScript = -1;

        [MenuItem("Novella/Flowchart")]
        public static void ShowWindow()
        {
            GetWindow<FlowchartWindow>("Flowchart");
        }

        private void OnEnable()
        {
            RefreshScriptList();
        }

        private void CreateGUI()
        {
            // Toolbar
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.height = 24;
            toolbar.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            toolbar.style.paddingLeft = 4;
            toolbar.style.paddingRight = 4;

            var dropdown = new DropdownField("Script", _scriptNames.ToList(), 0);
            dropdown.style.minWidth = 200;
            dropdown.RegisterValueChangedCallback(evt =>
            {
                int idx = System.Array.IndexOf(_scriptNames, evt.newValue);
                if (idx >= 0 && idx != _selectedScript)
                {
                    _selectedScript = idx;
                    RebuildGraph();
                }
            });
            toolbar.Add(dropdown);

            var refreshBtn = new Button(() =>
            {
                RefreshScriptList();
                dropdown.choices = _scriptNames.ToList();
                if (_scriptNames.Length > 0)
                    dropdown.value = _scriptNames[0];
            }) { text = "Refresh" };
            refreshBtn.style.marginLeft = 8;
            toolbar.Add(refreshBtn);

            rootVisualElement.Add(toolbar);

            // GraphView
            _graphView = new FlowchartGraphView();
            _graphView.StretchToParentSize();
            _graphView.style.top = 24;
            rootVisualElement.Add(_graphView);

            if (_scriptNames.Length > 0)
            {
                _selectedScript = 0;
                RebuildGraph();
            }
        }

        private void RefreshScriptList()
        {
            var dir = Path.Combine(Application.dataPath, "Novella/Resources/Scripts");
            _scriptPaths.Clear();
            if (Directory.Exists(dir))
                _scriptPaths = Directory.GetFiles(dir, "*.json").OrderBy(p => p).ToList();
            _scriptNames = _scriptPaths.Select(p => Path.GetFileNameWithoutExtension(p)).ToArray();
        }

        private void RebuildGraph()
        {
            if (_graphView == null || _selectedScript < 0 || _selectedScript >= _scriptPaths.Count) return;

            string json = File.ReadAllText(_scriptPaths[_selectedScript]);
            NovellaScript script;
            try
            {
                script = JsonConvert.DeserializeObject<NovellaScript>(json);
            }
            catch { return; }

            if (script?.Commands == null) return;
            _graphView.BuildFromScript(script);
        }
    }

    // ==================== GraphView ====================
    public class FlowchartGraphView : GraphView
    {
        public FlowchartGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            style.flexGrow = 1;
        }

        public void BuildFromScript(NovellaScript script)
        {
            // Clear
            foreach (var edge in edges.ToList()) RemoveElement(edge);
            foreach (var node in nodes.ToList()) RemoveElement(node);

            var commands = script.Commands;
            if (commands == null || commands.Count == 0) return;

            // 1) ラベル位置を収集
            var labelIndices = new Dictionary<string, int>();
            for (int i = 0; i < commands.Count; i++)
            {
                if (commands[i].Type == "label" && !string.IsNullOrEmpty(commands[i].Label))
                    labelIndices[commands[i].Label] = i;
            }

            // 2) ブロック分割: labelで始まるブロック + 先頭ブロック
            var blocks = new List<(string name, int startIndex, int endIndex)>();
            int blockStart = 0;
            string blockName = "START";

            for (int i = 0; i < commands.Count; i++)
            {
                if (commands[i].Type == "label" && !string.IsNullOrEmpty(commands[i].Label))
                {
                    if (i > blockStart)
                        blocks.Add((blockName, blockStart, i - 1));
                    blockName = commands[i].Label;
                    blockStart = i;
                }
            }
            blocks.Add((blockName, blockStart, commands.Count - 1));

            // 3) ノード作成
            var blockNodes = new Dictionary<string, FlowchartNode>();
            float x = 0f;
            float y = 0f;
            float nodeWidth = 220f;
            float spacing = 60f;
            int col = 0;
            int maxPerRow = 5;

            foreach (var block in blocks)
            {
                var summary = BuildBlockSummary(commands, block.startIndex, block.endIndex);
                var node = new FlowchartNode(block.name, summary, block.startIndex);
                node.SetPosition(new Rect(x, y, nodeWidth, 0));
                AddElement(node);
                blockNodes[block.name] = node;

                col++;
                if (col >= maxPerRow)
                {
                    col = 0;
                    x = 0;
                    y += 200f;
                }
                else
                {
                    x += nodeWidth + spacing;
                }
            }

            // 4) エッジ作成
            foreach (var block in blocks)
            {
                var node = blockNodes[block.name];
                bool hasJump = false;

                for (int i = block.startIndex; i <= block.endIndex; i++)
                {
                    var cmd = commands[i];

                    if (cmd.Type == "jump" && !string.IsNullOrEmpty(cmd.Target))
                    {
                        ConnectNodes(node, cmd.Target, blockNodes, "jump");
                        hasJump = true;
                    }
                    else if ((cmd.Type == "jump_if" || cmd.Type == "jump_unless") && !string.IsNullOrEmpty(cmd.Target))
                    {
                        string condition = cmd.Type == "jump_if"
                            ? $"if {cmd.Label ?? "?"}"
                            : $"unless {cmd.Label ?? "?"}";
                        ConnectNodes(node, cmd.Target, blockNodes, condition);
                    }
                    else if (cmd.Type == "choice" && cmd.Choices != null)
                    {
                        foreach (var choice in cmd.Choices)
                        {
                            if (!string.IsNullOrEmpty(choice.Target))
                                ConnectNodes(node, choice.Target, blockNodes, choice.Text ?? "choice");
                        }
                        hasJump = true;
                    }
                    else if (cmd.Type == "next_script" && !string.IsNullOrEmpty(cmd.Target))
                    {
                        // next_scriptは外部参照なのでラベル表示のみ
                        hasJump = true;
                    }
                    else if (cmd.Type == "end")
                    {
                        hasJump = true;
                    }
                }

                // ブロック末尾にjump/end/choiceがなければ次のブロックへフォールスルー
                if (!hasJump)
                {
                    int blockIdx = blocks.IndexOf(block);
                    if (blockIdx + 1 < blocks.Count)
                    {
                        string nextName = blocks[blockIdx + 1].name;
                        ConnectNodes(node, nextName, blockNodes, "→");
                    }
                }
            }
        }

        private void ConnectNodes(FlowchartNode from, string targetLabel,
            Dictionary<string, FlowchartNode> blockNodes, string edgeLabel)
        {
            if (!blockNodes.TryGetValue(targetLabel, out var toNode)) return;

            var edge = new Edge
            {
                output = from.OutputPort,
                input = toNode.InputPort
            };
            from.OutputPort.Connect(edge);
            toNode.InputPort.Connect(edge);
            AddElement(edge);
        }

        private string BuildBlockSummary(List<ScriptCommand> commands, int start, int end)
        {
            var lines = new List<string>();
            int shown = 0;
            for (int i = start; i <= end && shown < 4; i++)
            {
                var cmd = commands[i];
                if (cmd.Type == "label") continue;

                string line = cmd.Type;
                if (cmd.Type == "say")
                    line = $"say: {Truncate(cmd.Text, 20)}";
                else if (cmd.Type == "show_bg")
                    line = $"show_bg: {cmd.Image}";
                else if (cmd.Type == "show_char")
                    line = $"show_char: {cmd.Character}";
                else if (cmd.Type == "jump")
                    line = $"→ {cmd.Target}";
                else if (cmd.Type == "jump_if")
                    line = $"if({cmd.Label}) → {cmd.Target}";
                else if (cmd.Type == "jump_unless")
                    line = $"unless({cmd.Label}) → {cmd.Target}";
                else if (cmd.Type == "choice")
                    line = $"choice ({cmd.Choices?.Count ?? 0} options)";
                else if (cmd.Type == "next_script")
                    line = $"→ next: {cmd.Target}";
                else if (cmd.Type == "end")
                    line = "END";

                lines.Add(line);
                shown++;
            }

            int total = 0;
            for (int i = start; i <= end; i++)
                if (commands[i].Type != "label") total++;
            if (total > 4)
                lines.Add($"... +{total - 4} more");

            return string.Join("\n", lines);
        }

        private string Truncate(string s, int maxLen)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= maxLen ? s : s.Substring(0, maxLen) + "…";
        }
    }

    // ==================== Node ====================
    public class FlowchartNode : Node
    {
        public Port InputPort { get; private set; }
        public Port OutputPort { get; private set; }
        public int CommandIndex { get; private set; }

        public FlowchartNode(string blockName, string summary, int commandIndex)
        {
            title = blockName;
            CommandIndex = commandIndex;

            // Input port
            InputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPort.portName = "";
            inputContainer.Add(InputPort);

            // Output port
            OutputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            OutputPort.portName = "";
            outputContainer.Add(OutputPort);

            // Summary label
            var label = new Label(summary);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.fontSize = 11;
            label.style.color = new Color(0.8f, 0.8f, 0.8f);
            label.style.paddingLeft = 8;
            label.style.paddingRight = 8;
            label.style.paddingBottom = 4;
            mainContainer.Add(label);

            // Click to open Script Editor at this position
            RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 2)
                {
                    EditorPrefs.SetInt("Novella_ScriptEditor_JumpTo", commandIndex);
                    ScriptEditorWindow window = EditorWindow.GetWindow<ScriptEditorWindow>("Script Editor");
                    window.Focus();
                }
            });

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
#endif
