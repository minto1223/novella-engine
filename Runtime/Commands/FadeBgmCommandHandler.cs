using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    public class FadeBgmCommandHandler : ICommandHandler
    {
        public string CommandType => "fade_bgm";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (engine.IAudio == null)
            {
                Debug.LogWarning("[Novella] fade_bgm: Audio controller not assigned.");
                onComplete?.Invoke();
                return;
            }

            float targetVolume = 1f;
            if (!string.IsNullOrEmpty(command.Value))
            {
                if (float.TryParse(command.Value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float parsed))
                    targetVolume = parsed;
            }
            else if (command.Volume > 0)
            {
                targetVolume = command.Volume;
            }

            float duration = command.Duration > 0 ? command.Duration : 1f;
            engine.IAudio.FadeBgm(targetVolume, duration, onComplete);
        }
    }
}
