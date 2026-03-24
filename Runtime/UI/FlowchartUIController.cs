using System.Collections.Generic;
using Novella.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.UI
{
    /// <summary>
    /// プレイヤー向け分岐フローマップUI。
    /// FlowchartDefinitionからノードとエッジを描画し、到達済みルートをハイライトする。
    /// </summary>
    public class FlowchartUIController : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _contentContainer;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_FontAsset _font;

        [Header("Flowchart Data")]
        [SerializeField] private FlowchartDefinition _flowchart;

        [Header("Layout")]
        [SerializeField] private float _nodeWidth = 200f;
        [SerializeField] private float _nodeHeight = 60f;
        [SerializeField] private float _columnSpacing = 260f;
        [SerializeField] private float _rowSpacing = 100f;

        [Header("Colors")]
        [SerializeField] private Color _unlockedNodeColor = new Color(0.2f, 0.3f, 0.5f, 0.95f);
        [SerializeField] private Color _lockedNodeColor = new Color(0.12f, 0.12f, 0.12f, 0.7f);
        [SerializeField] private Color _endingNodeColor = new Color(0.5f, 0.2f, 0.3f, 0.95f);
        [SerializeField] private Color _startNodeColor = new Color(0.2f, 0.5f, 0.3f, 0.95f);
        [SerializeField] private Color _choiceNodeColor = new Color(0.5f, 0.4f, 0.15f, 0.95f);
        [SerializeField] private Color _unlockedTextColor = Color.white;
        [SerializeField] private Color _lockedTextColor = new Color(0.35f, 0.35f, 0.35f);
        [SerializeField] private Color _unlockedEdgeColor = new Color(0.4f, 0.6f, 0.9f, 0.8f);
        [SerializeField] private Color _lockedEdgeColor = new Color(0.25f, 0.25f, 0.25f, 0.4f);

        public bool IsOpen => _panel != null && _panel.activeSelf;

        private readonly Dictionary<string, RectTransform> _nodeRects = new Dictionary<string, RectTransform>();
        private readonly List<GameObject> _edgeObjects = new List<GameObject>();

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        public void Open()
        {
            BuildFlowchart();
            if (_panel != null)
            {
                _panel.SetActive(true);
                _panel.transform.SetAsLastSibling();
            }
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
        }

        public void Close()
        {
            if (_panel != null) _panel.SetActive(false);
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Close);
        }

        private void Update()
        {
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
                Close();
        }

        private void BuildFlowchart()
        {
            if (_contentContainer == null || _flowchart == null) return;

            // クリア
            foreach (Transform child in _contentContainer)
                Destroy(child.gameObject);
            _nodeRects.Clear();
            _edgeObjects.Clear();

            // 到達状況を計算
            var unlocked = new HashSet<string>();
            int totalNodes = 0;
            foreach (var node in _flowchart.nodes)
            {
                if (node == null) continue;
                totalNodes++;
                if (IsNodeUnlocked(node))
                    unlocked.Add(node.id);
            }

            // ノード生成
            foreach (var node in _flowchart.nodes)
            {
                if (node == null) continue;
                bool isUnlocked = unlocked.Contains(node.id);
                CreateNode(node, isUnlocked);
            }

            // エッジ生成
            foreach (var edge in _flowchart.edges)
            {
                if (edge == null) continue;
                bool isUnlocked = unlocked.Contains(edge.fromId) && unlocked.Contains(edge.toId);
                CreateEdge(edge, isUnlocked);
            }

            // 踏破率ヘッダー
            CreateProgressHeader(unlocked.Count, totalNodes);

            // コンテンツサイズ調整
            AdjustContentSize();
        }

        private bool IsNodeUnlocked(FlowNode node)
        {
            if (node.type == FlowNodeType.Start) return true;
            if (string.IsNullOrEmpty(node.unlockKey)) return false;

            if (node.type == FlowNodeType.Ending)
                return EndingManager.IsUnlocked(node.unlockKey);

            // Scene/Choice: SceneRecollectionManagerまたはReadManagerで判定
            return SceneRecollectionManager.IsCleared(node.unlockKey);
        }

        private void CreateNode(FlowNode node, bool isUnlocked)
        {
            var go = new GameObject($"Node_{node.id}");
            go.transform.SetParent(_contentContainer, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(_nodeWidth, _nodeHeight);
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0.5f, 0.5f);

            float x = 40f + node.column * _columnSpacing + _nodeWidth * 0.5f;
            float y = -(60f + node.row * _rowSpacing + _nodeHeight * 0.5f);
            rect.anchoredPosition = new Vector2(x, y);

            var img = go.AddComponent<Image>();
            if (isUnlocked)
            {
                switch (node.type)
                {
                    case FlowNodeType.Start: img.color = _startNodeColor; break;
                    case FlowNodeType.Ending: img.color = _endingNodeColor; break;
                    case FlowNodeType.Choice: img.color = _choiceNodeColor; break;
                    default: img.color = _unlockedNodeColor; break;
                }
            }
            else
            {
                img.color = _lockedNodeColor;
            }

            // テキスト
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8, 4);
            textRect.offsetMax = new Vector2(-8, -4);

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = isUnlocked ? node.title : "???";
            tmp.fontSize = 22;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = isUnlocked ? _unlockedTextColor : _lockedTextColor;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            if (_font != null) tmp.font = _font;

            _nodeRects[node.id] = rect;
        }

        private void CreateEdge(FlowEdge edge, bool isUnlocked)
        {
            if (!_nodeRects.TryGetValue(edge.fromId, out var fromRect)) return;
            if (!_nodeRects.TryGetValue(edge.toId, out var toRect)) return;

            var go = new GameObject($"Edge_{edge.fromId}_{edge.toId}");
            go.transform.SetParent(_contentContainer, false);
            go.transform.SetAsFirstSibling(); // ノードの後ろに配置

            var lineImg = go.AddComponent<Image>();
            lineImg.color = isUnlocked ? _unlockedEdgeColor : _lockedEdgeColor;
            lineImg.raycastTarget = false;

            var lineRect = go.GetComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0, 1);
            lineRect.anchorMax = new Vector2(0, 1);
            lineRect.pivot = new Vector2(0, 0.5f);

            Vector2 from = fromRect.anchoredPosition;
            Vector2 to = toRect.anchoredPosition;

            // 右端から左端へ接続
            Vector2 start = from + new Vector2(_nodeWidth * 0.5f, 0);
            Vector2 end = to - new Vector2(_nodeWidth * 0.5f, 0);

            // 同じ列なら下端から上端へ
            if (Mathf.Abs(from.x - to.x) < 1f)
            {
                start = from + new Vector2(0, -_nodeHeight * 0.5f);
                end = to + new Vector2(0, _nodeHeight * 0.5f);
            }

            Vector2 diff = end - start;
            float length = diff.magnitude;
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

            lineRect.anchoredPosition = start;
            lineRect.sizeDelta = new Vector2(length, 3f);
            lineRect.localRotation = Quaternion.Euler(0, 0, angle);

            _edgeObjects.Add(go);

            // エッジラベル
            if (!string.IsNullOrEmpty(edge.label) && isUnlocked)
            {
                var labelGO = new GameObject($"EdgeLabel_{edge.fromId}_{edge.toId}");
                labelGO.transform.SetParent(_contentContainer, false);
                var labelRect = labelGO.AddComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0, 1);
                labelRect.anchorMax = new Vector2(0, 1);
                labelRect.pivot = new Vector2(0.5f, 0.5f);
                labelRect.anchoredPosition = (start + end) * 0.5f + new Vector2(0, 14f);
                labelRect.sizeDelta = new Vector2(160, 24);

                var labelTmp = labelGO.AddComponent<TextMeshProUGUI>();
                labelTmp.text = edge.label;
                labelTmp.fontSize = 16;
                labelTmp.alignment = TextAlignmentOptions.Center;
                labelTmp.color = new Color(0.7f, 0.7f, 0.8f);
                if (_font != null) labelTmp.font = _font;
            }
        }

        private void CreateProgressHeader(int unlocked, int total)
        {
            var go = new GameObject("ProgressHeader");
            go.transform.SetParent(_contentContainer, false);
            go.transform.SetAsLastSibling();

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -10f);
            rect.sizeDelta = new Vector2(0, 36);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            float percent = total > 0 ? (float)unlocked / total * 100f : 0f;
            tmp.text = $"{_flowchart.title}  -  {unlocked}/{total} ({percent:F0}%)";
            tmp.fontSize = 26;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            if (_font != null) tmp.font = _font;
        }

        private void AdjustContentSize()
        {
            if (_contentContainer == null) return;
            var contentRect = _contentContainer.GetComponent<RectTransform>();
            if (contentRect == null) return;

            float maxX = 0, maxY = 0;
            foreach (var kvp in _nodeRects)
            {
                var pos = kvp.Value.anchoredPosition;
                float right = pos.x + _nodeWidth * 0.5f + 40f;
                float bottom = Mathf.Abs(pos.y) + _nodeHeight * 0.5f + 40f;
                if (right > maxX) maxX = right;
                if (bottom > maxY) maxY = bottom;
            }

            contentRect.sizeDelta = new Vector2(
                Mathf.Max(maxX, contentRect.sizeDelta.x),
                Mathf.Max(maxY + 60f, contentRect.sizeDelta.y)
            );
        }
    }
}
