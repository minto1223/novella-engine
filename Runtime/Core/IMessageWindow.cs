using System;

namespace Novella.Core
{
    public interface IMessageWindow
    {
        bool IsTyping { get; }
        void Show(string characterName, string text, Action onTypingComplete, bool isRead = false);
        void Hide();
        void SkipTyping();
        void ApplyTextSpeed(float charsPerSecond);
        void ApplyFontSize(float size);
        void ApplyWindowOpacity(float opacity);
        void SetDisplayMode(DisplayMode mode);
        void ClearNvlText();
        void SetPanelVisible(bool visible);
    }

    public enum DisplayMode
    {
        ADV,
        NVL
    }
}
