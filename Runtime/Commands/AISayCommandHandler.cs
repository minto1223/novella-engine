using System;
using Novella.Core;

namespace Novella.Commands
{
    /// <summary>
    /// ai_sayコマンド：command.TextをプロンプトとしてClaude APIに送り、
    /// 生成されたセリフをMessageWindowに表示する。
    ///
    /// JSON例:
    /// {
    ///   "type": "ai_say",
    ///   "character": "ナレーター",
    ///   "text": "この世界の美しさについて詩的に一言述べてください。"
    /// }
    /// </summary>
    public class AISayCommandHandler : ICommandHandler
    {
        public string CommandType => "ai_say";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            engine.StartCoroutine(AIDialogueClient.Generate(
                prompt: command.Text,
                apiKey: engine.AIApiKey,
                onResult: (generatedText) =>
                {
                    engine.Backlog.Add(command.Character, generatedText);
                    engine.IBacklogUI?.Rebuild(engine.Backlog.Entries);
                    engine.IMessageWindow?.Show(command.Character, generatedText, () =>
                        engine.WaitForInput(onComplete), engine.CurrentCommandWasRead);
                }
            ));
        }
    }
}
