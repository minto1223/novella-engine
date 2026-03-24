using System;
using UnityEngine;

namespace Novella.Core.Templates
{
    /// <summary>
    /// カスタム立ち絵表示のテンプレート。
    /// このクラスを継承して独自のキャラクター表示を実装し、
    /// NovellaEngine の Custom UI Overrides > Character Layer にアサインしてください。
    /// </summary>
    public abstract class CustomCharacterDisplay : MonoBehaviour, ICharacterDisplay
    {
        /// <summary>キャラクターを表示する。表示完了後に onComplete を呼ぶこと。</summary>
        public abstract void ShowCharacter(string characterId, string expression, string position, float duration, Action onComplete);

        public virtual void ShowCharacter(string characterId, string expression, string position, float duration, string effect, Action onComplete)
        {
            ShowCharacter(characterId, expression, position, duration, onComplete);
        }

        public virtual void ShowCharacter(string characterId, string expression, string position, float duration, string effect, int order, Action onComplete)
        {
            ShowCharacter(characterId, expression, position, duration, effect, onComplete);
        }

        /// <summary>キャラクターを非表示にする。</summary>
        public abstract void HideCharacter(string characterId, Action onComplete);

        public virtual void HideCharacter(string characterId, string effect, Action onComplete)
        {
            HideCharacter(characterId, onComplete);
        }

        /// <summary>キャラクターを別のポジションに移動する。</summary>
        public virtual void MoveCharacter(string characterId, string toPosition, float duration, Action onComplete)
        {
            onComplete?.Invoke();
        }

        public virtual void MoveCharacter(string characterId, string toPosition, float duration, int order, Action onComplete)
        {
            MoveCharacter(characterId, toPosition, duration, onComplete);
        }

        /// <summary>リップシンク開始（ボイス再生時に呼ばれる）。</summary>
        public virtual void StartTalking(string characterId) { }

        /// <summary>リップシンク停止。</summary>
        public virtual void StopTalking(string characterId) { }

        /// <summary>全キャラクターのリップシンクを停止。</summary>
        public virtual void StopAllTalking() { }

        /// <summary>全立ち絵を即座に削除する（ロード時用）。</summary>
        public virtual void HideAllCharacters() { }
    }
}
