using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    /// <summary>
    /// 数値変数に対して四則演算・代入を行う。
    /// 値には他のフラグを含む計算式を書ける（整数演算。詳しい構文は NovellaExpression を参照）。
    /// JSON例:
    ///   { "type": "calc", "target": "affection", "value": "+5" }
    ///   { "type": "calc", "target": "gold", "value": "-100" }
    ///   { "type": "calc", "target": "score", "value": "*2" }
    ///   { "type": "calc", "target": "hp", "value": "/2" }
    ///   { "type": "calc", "target": "level", "value": "=10" }
    ///   { "type": "calc", "target": "count", "value": "3" }  (= と同義)
    ///   { "type": "calc", "target": "damage", "value": "=atk * 2 - def" }
    ///   { "type": "calc", "target": "hp", "value": "-(atk - armor)" }
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

            // 先頭が演算子なら複合代入（"+5" → target = target + 5）
            char op = expr[0];
            if (op == '+' || op == '-' || op == '*' || op == '/' || op == '%' || op == '=')
            {
                string rhs = expr.Substring(1).Trim();
                string rhsValue = null;
                string rhsError = "右辺がありません";

                if (rhs.Length > 0 &&
                    engine.Flags.TryEvaluateExpression(rhs, out rhsValue, out rhsError))
                {
                    int operand = NovellaExpression.ToInt(rhsValue);
                    int result;

                    switch (op)
                    {
                        case '+': result = current + operand; break;
                        case '-': result = current - operand; break;
                        case '*': result = current * operand; break;
                        case '/': result = operand != 0 ? current / operand : current; break;
                        case '%': result = operand != 0 ? current % operand : current; break;
                        default:  result = operand; break; // '='
                    }

                    if ((op == '/' || op == '%') && operand == 0)
                        Debug.LogWarning($"[Novella] calc: 0で割ろうとしたため {target} を変更しませんでした。");

                    engine.Flags.Set(target, result.ToString());
                    onComplete?.Invoke();
                    return;
                }

                // 演算子付きで右辺が評価できない場合は、値を壊さないよう何もしない
                Debug.LogWarning($"[Novella] calc: 式を評価できませんでした: \"{expr}\" ({rhsError})。{target} は変更していません。");
                onComplete?.Invoke();
                return;
            }

            // 演算子なし → 式として評価して代入。評価できなければ文字列としてそのまま代入
            if (engine.Flags.TryEvaluateExpression(expr, out string value, out _))
                engine.Flags.Set(target, value);
            else
                engine.Flags.Set(target, expr);

            onComplete?.Invoke();
        }
    }
}
