using System;
using Novella.Core;

namespace Novella.Commands
{
    public class StopVoiceCommandHandler : ICommandHandler
    {
        public string CommandType => "stop_voice";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            engine.IAudio?.StopVoice();
            onComplete?.Invoke();
        }
    }
}
