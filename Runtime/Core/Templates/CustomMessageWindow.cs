using System;
using UnityEngine;

namespace Novella.Core.Templates
{
    /// <summary>
    /// カスタムメッセージウィンドウのテンプレート。
    /// このクラスを継承して独自の表示ロジックを実装し、
    /// NovellaEngine の Custom UI Overrides > Message Window にアサインしてください。
    /// </summary>
    public abstract class CustomMessageWindow : MonoBehaviour, IMessageWindow
    {
        public abstract bool IsTyping { get; }

        /// <summary>セリフを表示する。表示完了後に onTypingComplete を呼ぶこと。</summary>
        public abstract void Show(string characterName, string text, Action onTypingComplete, bool isRead = false);

        /// <summary>メッセージウィンドウを非表示にする。</summary>
        public abstract void Hide();

        /// <summary>タイピングアニメーションをスキップして全文表示する。</summary>
        public abstract void SkipTyping();

        /// <summary>テキスト表示速度を設定する（文字/秒）。</summary>
        public virtual void ApplyTextSpeed(float charsPerSecond) { }

        /// <summary>フォントサイズを設定する。</summary>
        public virtual void ApplyFontSize(float size) { }

        /// <summary>ウィンドウの透明度を設定する（0〜1）。</summary>
        public virtual void ApplyWindowOpacity(float opacity) { }

        /// <summary>ADV/NVL表示モードを切り替える。</summary>
        public virtual void SetDisplayMode(DisplayMode mode) { }

        /// <summary>NVLモードの蓄積テキストをクリアする。</summary>
        public virtual void ClearNvlText() { }

        /// <summary>パネルの表示/非表示を切り替える（UI非表示モード用）。</summary>
        public virtual void SetPanelVisible(bool visible) { }
    }
}
