using UnityEngine;

namespace Novella.Core
{
    /// <summary>
    /// 回想モード用のシーン定義。
    /// スクリプト内のラベル区間を1つの「シーン」として定義し、
    /// 既読時にリプレイ可能にする。
    /// </summary>
    [CreateAssetMenu(fileName = "SceneDefinition", menuName = "Novella/Scene Definition")]
    public class SceneDefinition : ScriptableObject
    {
        [Tooltip("シーンの一意ID（記録用）")]
        public string sceneId;

        [Tooltip("回想画面に表示するタイトル")]
        public string title;

        [Tooltip("スクリプトのResourcesパス（例: Scripts/chapter01）")]
        public string scriptPath;

        [Tooltip("開始ラベル（空なら先頭から）")]
        public string startLabel;

        [Tooltip("終了ラベル（空なら最後まで）")]
        public string endLabel;

        [Tooltip("回想画面のサムネイル（任意）")]
        public Sprite thumbnail;

        [Tooltip("表示順序（小さいほど先）")]
        public int sortOrder;
    }
}
