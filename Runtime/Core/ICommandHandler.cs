using System;

namespace Novella.Core
{
    public interface ICommandHandler
    {
        string CommandType { get; }
        void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete);
    }
}
