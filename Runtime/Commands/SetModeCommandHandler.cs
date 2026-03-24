using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    /// <summary>
    /// ADV/NVLモードを切り替える。
    /// JSON: { "type": "set_mode", "value": "nvl" }
    /// - value: "adv" または "nvl"
    /// </summary>
    public class SetModeCommandHandler : ICommandHandler
    {
        public string CommandType => "set_mode";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            string mode = (command.Value ?? "adv").ToLower();
            var displayMode = mode == "nvl" ? DisplayMode.NVL : DisplayMode.ADV;
            engine.TrackDisplayMode(mode);
            engine.IMessageWindow?.SetDisplayMode(displayMode);
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// NVLモードの蓄積テキストをクリアする。
    /// JSON: { "type": "clear" }
    /// </summary>
    public class ClearCommandHandler : ICommandHandler
    {
        public string CommandType => "clear";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            engine.IMessageWindow?.ClearNvlText();
            onComplete?.Invoke();
        }
    }
}
