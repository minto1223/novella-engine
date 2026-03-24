using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    /// <summary>
    /// Ken Burnsエフェクト: 背景画像をゆっくりズーム＆パンするスチル演出。
    /// 非ブロッキング（即座にonComplete → セリフと並行して動作）。
    /// JSON: { "type": "ken_burns", "value": "1.3", "position": "right", "duration": 5.0 }
    /// </summary>
    public class KenBurnsCommandHandler : ICommandHandler
    {
        public string CommandType => "ken_burns";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (engine.IBackground == null)
            {
                Debug.LogWarning("[Novella] ken_burns: Background controller not assigned.");
                onComplete?.Invoke();
                return;
            }

            float targetZoom = 1.2f;
            if (!string.IsNullOrEmpty(command.Value) &&
                float.TryParse(command.Value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float parsed))
                targetZoom = Mathf.Clamp(parsed, 0.5f, 3f);

            float duration = command.Duration > 0 ? command.Duration : 5f;
            string position = command.Position ?? "center";

            engine.IBackground.StartKenBurns(targetZoom, position, duration);

            // 非ブロッキング: 即座に次のコマンドへ
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// Ken Burnsエフェクト停止。
    /// JSON: { "type": "stop_ken_burns", "duration": 0.5 }
    /// </summary>
    public class StopKenBurnsCommandHandler : ICommandHandler
    {
        public string CommandType => "stop_ken_burns";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (engine.IBackground == null)
            {
                onComplete?.Invoke();
                return;
            }

            float resetDuration = command.Duration > 0 ? command.Duration : 0.5f;
            engine.IBackground.StopKenBurns(resetDuration, onComplete);
        }
    }
}
