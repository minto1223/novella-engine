using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    /// <summary>
    /// 数値フラグに加算する。
    /// JSON: { "type": "add_flag", "target": "affection", "value": "5" }
    /// CSV:  add_flag,,,,,,,,affection,,,, 5
    /// value未指定時は +1。負の値で減算も可能。
    /// </summary>
    public class AddFlagCommandHandler : ICommandHandler
    {
        public string CommandType => "add_flag";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            string flagName = command.Target;
            if (string.IsNullOrEmpty(flagName))
            {
                Debug.LogWarning("[Novella] add_flag: target not specified.");
                onComplete?.Invoke();
                return;
            }

            int amount = 1;
            if (!string.IsNullOrEmpty(command.Value) && int.TryParse(command.Value, out int v))
                amount = v;

            engine.Flags.Add(flagName, amount);
            onComplete?.Invoke();
        }
    }
}
