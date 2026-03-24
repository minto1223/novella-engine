using System;
using Novella.Core;

namespace Novella.Commands
{
    public class SetFlagCommandHandler : ICommandHandler
    {
        public string CommandType => "set_flag";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (!string.IsNullOrEmpty(command.Label))
                engine.Flags.Set(command.Label, command.Value ?? "true");
            onComplete?.Invoke();
        }
    }
}
