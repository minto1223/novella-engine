using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    public class PlayVoiceCommandHandler : ICommandHandler
    {
        public string CommandType => "play_voice";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (engine.IAudio == null)
            {
                Debug.LogWarning("[Novella] play_voice: Audio controller not assigned.");
                onComplete?.Invoke();
                return;
            }
            engine.IAudio.PlayVoice(command.Clip ?? command.Image, command.Volume, onComplete);
        }
    }
}
