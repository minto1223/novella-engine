using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    public class SetVolumeCommandHandler : ICommandHandler
    {
        public string CommandType => "set_volume";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (engine.IAudio == null)
            {
                Debug.LogWarning("[Novella] set_volume: Audio controller not assigned.");
                onComplete?.Invoke();
                return;
            }

            float vol = command.Volume > 0 ? command.Volume : 1f;
            if (!string.IsNullOrEmpty(command.Value))
            {
                if (float.TryParse(command.Value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float parsed))
                    vol = parsed;
            }

            string target = (command.Target ?? "").ToLowerInvariant();
            switch (target)
            {
                case "bgm":
                    engine.IAudio.SetBgmVolume(vol);
                    break;
                case "se":
                    engine.IAudio.SetSeVolume(vol);
                    break;
                case "voice":
                    engine.IAudio.SetVoiceVolume(vol);
                    break;
                default:
                    Debug.LogWarning($"[Novella] set_volume: unknown target '{command.Target}'. Use bgm/se/voice.");
                    break;
            }
            onComplete?.Invoke();
        }
    }
}
