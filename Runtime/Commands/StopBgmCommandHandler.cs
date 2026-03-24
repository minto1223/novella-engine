using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    public class StopBgmCommandHandler : ICommandHandler
    {
        public string CommandType => "stop_bgm";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (engine.IAudio == null)
            {
                Debug.LogWarning("[Novella] stop_bgm: Audio controller not assigned.");
                onComplete?.Invoke();
                return;
            }
            engine.TrackBgmStop();
            engine.IAudio.StopBgm(command.Duration, onComplete);
        }
    }
}
