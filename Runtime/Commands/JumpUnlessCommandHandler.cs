using System;
using Novella.Core;

namespace Novella.Commands
{
    public class JumpUnlessCommandHandler : ICommandHandler
    {
        public string CommandType => "jump_unless";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (!engine.Flags.EvaluateCondition(command.Label))
                engine.JumpToLabel(command.Target);
            onComplete?.Invoke();
        }
    }
}
