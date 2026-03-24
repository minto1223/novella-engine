using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    public class PlaySeCommandHandler : ICommandHandler
    {
        public string CommandType => "play_se";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (engine.IAudio == null)
            {
                Debug.LogWarning("[Novella] play_se: Audio controller not assigned.");
                onComplete?.Invoke();
                return;
            }
            engine.IAudio.PlaySe(command.Clip ?? command.Image, command.Volume, onComplete);
        }
    }
}
