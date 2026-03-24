using System.Collections.Generic;
using UnityEngine;

namespace Novella.Core
{
    public class FlagManager
    {
        private readonly Dictionary<string, string> _flags = new Dictionary<string, string>();

        public void Set(string name, string value)
        {
            _flags[name] = value;
            Debug.Log($"[Novella] Flag set: {name} = {value}");
        }

        public string Get(string name)
        {
            return _flags.TryGetValue(name, out var v) ? v : null;
        }

        public bool IsTrue(string name)
        {
            var v = Get(name);
            if (v == null) return false;
            return v == "true" || v == "1" || v == "yes";
        }

        /// <summary>数値としてフラグを取得する。未設定や非数値なら0。</summary>
        public int GetInt(string name)
        {
            var v = Get(name);
            if (v != null && int.TryParse(v, out int n)) return n;
            return 0;
        }

        /// <summary>数値フラグに加算する。</summary>
        public void Add(string name, int amount)
        {
            int current = GetInt(name);
            Set(name, (current + amount).ToString());
        }

        /// <summary>
        /// 条件式を評価する。
        /// "flag" → IsTrue, "!flag" → !IsTrue,
        /// "flag==val", "flag!=val", "flag>=n", "flag<=n", "flag>n", "flag&lt;n"
        /// AND / OR で複合条件を記述可能（AND優先、左から評価）。
        /// 例: "affection>=10 AND route==alice"
        /// 例: "has_key OR has_lockpick"
        /// </summary>
        public bool EvaluateCondition(string condition)
        {
            if (string.IsNullOrEmpty(condition)) return true;

            condition = condition.Trim();

            // OR分割（ORはANDより優先度が低い）
            var orParts = SplitCondition(condition, " OR ");
            if (orParts.Length > 1)
            {
                foreach (var part in orParts)
                    if (EvaluateCondition(part)) return true;
                return false;
            }

            // AND分割
            var andParts = SplitCondition(condition, " AND ");
            if (andParts.Length > 1)
            {
                foreach (var part in andParts)
                    if (!EvaluateCondition(part)) return false;
                return true;
            }

            // 単一条件の評価
            return EvaluateSingle(condition);
        }

        private bool EvaluateSingle(string condition)
        {
            condition = condition.Trim();

            // NOT
            if (condition.StartsWith("!"))
                return !IsTrue(condition.Substring(1).Trim());

            // 比較演算子
            string[] ops = { ">=", "<=", "!=", "==", ">", "<" };
            foreach (var op in ops)
            {
                int idx = condition.IndexOf(op);
                if (idx < 0) continue;

                string flagName = condition.Substring(0, idx).Trim();
                string rhs = condition.Substring(idx + op.Length).Trim();
                string flagVal = Get(flagName) ?? "";

                switch (op)
                {
                    case "==": return flagVal == rhs;
                    case "!=": return flagVal != rhs;
                    case ">=": return GetInt(flagName) >= ParseInt(rhs);
                    case "<=": return GetInt(flagName) <= ParseInt(rhs);
                    case ">":  return GetInt(flagName) > ParseInt(rhs);
                    case "<":  return GetInt(flagName) < ParseInt(rhs);
                }
            }

            // 単純なフラグ名 → IsTrue
            return IsTrue(condition);
        }

        private static string[] SplitCondition(string condition, string separator)
        {
            var parts = new System.Collections.Generic.List<string>();
            int idx;
            while ((idx = condition.IndexOf(separator, System.StringComparison.Ordinal)) >= 0)
            {
                parts.Add(condition.Substring(0, idx));
                condition = condition.Substring(idx + separator.Length);
            }
            parts.Add(condition);
            return parts.ToArray();
        }

        private static int ParseInt(string s)
        {
            return int.TryParse(s, out int n) ? n : 0;
        }

        public void Clear() => _flags.Clear();

        public Dictionary<string, string> GetAll() => new Dictionary<string, string>(_flags);

        public void SetAll(Dictionary<string, string> flags)
        {
            _flags.Clear();
            if (flags == null) return;
            foreach (var kv in flags) _flags[kv.Key] = kv.Value;
        }
    }
}
