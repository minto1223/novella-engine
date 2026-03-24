using System;
using System.Collections.Generic;

namespace Novella.Core
{
    [Serializable]
    public class SaveData
    {
        public string ScriptPath;
        public int CommandIndex;
        public Dictionary<string, string> Flags = new Dictionary<string, string>();
        public string SavedAt;
        public string Title;         // 章タイトル
        public string LastDialogue;  // 最後のセリフ（"キャラ「テキスト」" 形式）
        public string ThumbnailFile; // サムネイル画像ファイル名（persistentDataPath内）

        // 視覚状態の保存
        public VisualState Visual;
    }

    [Serializable]
    public class VisualState
    {
        public string BackgroundImage;
        public string BgmClip;
        public float BgmVolume = 1f;
        public List<CharacterState> Characters = new List<CharacterState>();
        public string DisplayMode; // "adv" or "nvl"
    }

    [Serializable]
    public class CharacterState
    {
        public string CharacterId;
        public string Expression;
        public string Position;
    }
}
