using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    public class PlayBgmCommandHandler : ICommandHandler
    {
        public string CommandType => "play_bgm";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (engine.IAudio == null)
            {
                Debug.LogWarning("[Novella] play_bgm: Audio controller not assigned.");
                onComplete?.Invoke();
                return;
            }
            string clipName = command.Clip ?? command.Image;
            BGMManager.RecordBGM(clipName);
            engine.TrackBgm(clipName, command.Volume > 0 ? command.Volume : 1f);
            float fadeIn = 0f;
            if (!string.IsNullOrEmpty(command.Value) && command.Value == "fade_in")
                fadeIn = command.Duration > 0 ? command.Duration : 1f;
            engine.IAudio.PlayBgm(clipName, command.Volume, fadeIn, onComplete);
        }
    }
}
