using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    public class StopSeCommandHandler : ICommandHandler
    {
        public string CommandType => "stop_se";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (engine.IAudio == null)
            {
                Debug.LogWarning("[Novella] stop_se: Audio controller not assigned.");
                onComplete?.Invoke();
                return;
            }
            engine.IAudio.StopSe();
            onComplete?.Invoke();
        }
    }
}
