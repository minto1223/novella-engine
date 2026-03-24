using System;
using Novella.Core;

namespace Novella.Commands
{
    public class JumpCommandHandler : ICommandHandler
    {
        public string CommandType => "jump";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            engine.JumpToLabel(command.Target);
            onComplete?.Invoke();
        }
    }
}
