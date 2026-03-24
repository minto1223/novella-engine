using System;
using Novella.Core;

namespace Novella.Commands
{
    /// <summary>
    /// ゲーム言語を切り替える。
    /// JSON: { "type": "set_language", "value": "en" }
    /// - value: 言語コード（Resources/Localization/内のJSONファイル名）
    /// </summary>
    public class SetLanguageCommandHandler : ICommandHandler
    {
        public string CommandType => "set_language";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            string lang = command.Value;
            if (!string.IsNullOrEmpty(lang))
                LocalizationManager.Instance.SetLanguage(lang);
            onComplete?.Invoke();
        }
    }
}
