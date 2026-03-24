using System;
using System.Collections.Generic;
using UnityEngine;

namespace Novella.Core
{
    /// <summary>
    /// プレイヤー向け分岐フローマップの定義。
    /// ノード（シーン・分岐・エンディング）とエッジ（接続線）でストーリー構造を可視化する。
    /// </summary>
    [CreateAssetMenu(fileName = "FlowchartDefinition", menuName = "Novella/Flowchart Definition")]
    public class FlowchartDefinition : ScriptableObject
    {
        public string title = "Story Flowchart";
        public List<FlowNode> nodes = new List<FlowNode>();
        public List<FlowEdge> edges = new List<FlowEdge>();
    }

    [Serializable]
    public class FlowNode
    {
        [Tooltip("ノードの一意ID")]
        public string id;

        [Tooltip("表示タイトル")]
        public string title;

        [Tooltip("ノード種別")]
        public FlowNodeType type = FlowNodeType.Scene;

        [Tooltip("到達判定に使うキー（SceneDefinitionのsceneId、エンディングラベル等）")]
        public string unlockKey;

        [Tooltip("グリッド上の列（0始まり、左から右）")]
        public int column;

        [Tooltip("グリッド上の行（0始まり、上から下）")]
        public int row;
    }

    [Serializable]
    public enum FlowNodeType
    {
        Scene,      // 通常シーン
        Choice,     // 分岐点
        Ending,     // エンディング
        Start       // 開始
    }

    [Serializable]
    public class FlowEdge
    {
        [Tooltip("接続元ノードID")]
        public string fromId;

        [Tooltip("接続先ノードID")]
        public string toId;

        [Tooltip("エッジのラベル（選択肢テキスト等、任意）")]
        public string label;
    }
}
