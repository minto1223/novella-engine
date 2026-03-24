using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    /// <summary>
    /// 数値変数に対して四則演算・代入を行う。
    /// JSON例:
    ///   { "type": "calc", "target": "affection", "value": "+5" }
    ///   { "type": "calc", "target": "gold", "value": "-100" }
    ///   { "type": "calc", "target": "score", "value": "*2" }
    ///   { "type": "calc", "target": "hp", "value": "/2" }
    ///   { "type": "calc", "target": "level", "value": "=10" }
    ///   { "type": "calc", "target": "count", "value": "3" }  (= と同義)
    /// </summary>
    public class CalcCommandHandler : ICommandHandler
    {
        public string CommandType => "calc";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            string target = command.Target;
            string expr = command.Value;

            if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(expr))
            {
                Debug.LogWarning("[Novella] calc: target and value are required.");
                onComplete?.Invoke();
                return;
            }

            expr = expr.Trim();
            int current = engine.Flags.GetInt(target);
            int result;

            if (expr.Length >= 2)
            {
                char op = expr[0];
                string numStr = expr.Substring(1).Trim();

                if ((op == '+' || op == '-' || op == '*' || op == '/' || op == '=')
                    && int.TryParse(numStr, out int operand))
                {
                    switch (op)
                    {
                        case '+': result = current + operand; break;
                        case '-': result = current - operand; break;
                        case '*': result = current * operand; break;
                        case '/': result = operand != 0 ? current / operand : current; break;
                        case '=': result = operand; break;
                        default:  result = current; break;
                    }
                    engine.Flags.Set(target, result.ToString());
                    onComplete?.Invoke();
                    return;
                }
            }

            // 演算子なし → 代入
            if (int.TryParse(expr, out int directValue))
                engine.Flags.Set(target, directValue.ToString());
            else
                engine.Flags.Set(target, expr);

            onComplete?.Invoke();
        }
    }
}
