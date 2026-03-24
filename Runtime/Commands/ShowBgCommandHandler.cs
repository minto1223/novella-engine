using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    public class ShowBgCommandHandler : ICommandHandler
    {
        public string CommandType => "show_bg";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (engine.IBackground == null)
            {
                Debug.LogWarning("[Novella] show_bg: Background controller not assigned.");
                onComplete?.Invoke();
                return;
            }
            // CG記録
            if (!string.IsNullOrEmpty(command.Image))
                Novella.Core.CGManager.RecordCG(command.Image);

            engine.TrackBackground(command.Image);
            engine.IBackground.Show(command.Image, command.Duration, command.Value, onComplete);
        }
    }
}
