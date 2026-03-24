using UnityEngine;

namespace Novella.Core
{
    [CreateAssetMenu(fileName = "CharacterDefinition", menuName = "Novella/Character Definition")]
    public class CharacterDefinition : ScriptableObject
    {
        [Tooltip("スクリプトで使うキャラID（例: alice）")]
        public string characterId;

        [Tooltip("表示名（例: アリス）")]
        public string displayName;

        [Tooltip("名前欄のカラー")]
        public Color nameColor = Color.white;

        [Tooltip("デフォルト表情（例: normal）")]
        public string defaultExpression = "normal";
    }
}
