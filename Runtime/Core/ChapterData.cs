using UnityEngine;

namespace Novella.Core
{
    [CreateAssetMenu(fileName = "ChapterList", menuName = "Novella/Chapter List")]
    public class ChapterList : ScriptableObject
    {
        public ChapterEntry[] Chapters = new ChapterEntry[0];
    }

    [System.Serializable]
    public class ChapterEntry
    {
        [Tooltip("章タイトル（UI表示用）")]
        public string Title;

        [Tooltip("Resources/ 以下のスクリプトパス（拡張子不要）")]
        public string ScriptPath;

        [Tooltip("スクリプトファイル直接指定（設定するとScriptPathより優先）")]
        public TextAsset ScriptAsset;

        [Tooltip("サムネイル画像（任意）")]
        public Sprite Thumbnail;

        /// <summary>実際に使用するスクリプトパスを返す。</summary>
        public string ResolvedPath =>
            ScriptAsset != null ? "Scripts/" + ScriptAsset.name : ScriptPath;
    }
}
