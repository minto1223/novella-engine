using System;
using Novella.Core;

namespace Novella.Commands
{
    public class JumpIfCommandHandler : ICommandHandler
    {
        public string CommandType => "jump_if";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (engine.Flags.EvaluateCondition(command.Label))
                engine.JumpToLabel(command.Target);
            onComplete?.Invoke();
        }
    }
}
