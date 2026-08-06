// 条件式評価（NovellaExpression / FlagManager）と calc の回帰テスト。
// Unityを起動せず `dotnet run` で実行でき、パッケージ本体のソースを直接コンパイルして検証する。
// 使い方は README.md を参照。
using System;
using System.Reflection;
using Novella.Core;
using Novella.Commands;

internal static class Program
{
    private static FlagManager _flags;
    private static MethodInfo _legacy;
    private static int _pass, _fail, _diff;

    private static int Main()
    {
        SetUpFlags();

        Section("1. 従来記法（新実装の結果が期待値と一致し、かつ旧実装とも一致すること）");
        Compat("demo_flag", true);
        Compat("false_flag", false);
        Compat("undefined_flag", false);
        Compat("!demo_flag", false);
        Compat("!false_flag", true);
        Compat("!undefined_flag", true);
        Compat("score == 100", true);
        Compat("score==100", true);
        Compat("route == alice", true);
        Compat("route==alice", true);
        Compat("route == bob", false);
        Compat("route != bob", true);
        Compat("score >= 100", true);
        Compat("score > 100", false);
        Compat("score <= 100", true);
        Compat("score < 100", false);
        Compat("hp >= 10 AND route == alice", true);
        Compat("hp >= 100 AND route == alice", false);
        Compat("demo_flag OR false_flag", true);
        Compat("false_flag OR undefined_flag", false);
        Compat("hp > 10 AND demo_flag OR false_flag", true);
        Compat("false_flag AND demo_flag OR one", true);
        Compat("yes_flag", true);
        Compat("one", true);
        Compat("count", false);
        Compat("undefined_flag >= 1", false);
        Compat("undefined_flag == ", true); // 旧実装も true（空文字どうしの比較）

        Section("2. 新しく書けるようになった式");
        Expect("(demo_flag AND false_flag) OR one", true);
        Expect("demo_flag AND (false_flag OR one)", true);
        Expect("(demo_flag AND false_flag) OR count", false);
        Expect("hp > mp", true);
        Expect("mp > hp", false);
        Expect("hp - 40 > mp - 15", true);
        Expect("score / 2 == 50", true);
        Expect("score * 2 == 200", true);
        Expect("score % 30 == 10", true);
        Expect("hp + mp == 70", true);
        Expect("-hp < 0", true);
        Expect("!(demo_flag AND false_flag)", true);
        Expect("demo_flag && !false_flag", true);
        Expect("false_flag || one", true);
        Expect("NOT false_flag", true);
        Expect("route == \"alice\"", true);
        Expect("route == 'alice'", true);
        Expect("route != \"bob\"", true);
        Expect("名声 >= 30", true);
        Expect("名声 * 2 == 60", true);
        Expect("hp * 2 - mp >= 80", true);
        Expect("(hp - 10) * 2 == 80", true);
        Expect("", true); // 条件なし＝常に真

        Section("3. 解釈できない式は従来方式にフォールバックする（例外を投げない）");
        Fallback("((demo_flag");
        Fallback("score == ");
        Fallback("score = 100");
        Fallback("demo_flag &");

        Section("4. calc");
        var engine = new NovellaEngine();
        engine.Flags.Set("hp", "50");
        engine.Flags.Set("atk", "12");
        engine.Flags.Set("def", "4");
        engine.Flags.Set("armor", "3");
        engine.Flags.Set("bonus", "3");
        engine.Flags.Set("reward", "20");

        Calc(engine, "score", "=100", "100");
        Calc(engine, "score", "+50", "150");
        Calc(engine, "score", "*2", "300");
        Calc(engine, "score", "/3", "100");
        Calc(engine, "score", "-100", "0");
        Calc(engine, "level", "10", "10");
        Calc(engine, "ending", "good_end", "good_end"); // 数式でない値は文字列として代入
        Calc(engine, "zero", "/0", "0");                // 0除算は対象を変更しない
        Calc(engine, "damage", "=atk * 2 - def", "20");
        Calc(engine, "hp", "-(atk - armor)", "41");
        Calc(engine, "gold", "+reward * bonus", "60");
        Calc(engine, "mod", "=atk % 5", "2");
        Calc(engine, "copy", "hp", "41");               // 定義済みフラグの値をコピー
        Calc(engine, "hp", "%7", "6");

        engine.Flags.Set("safe", "99");
        Calc(engine, "safe", "+((", "99");              // 壊れた式は対象を変更しない
        Calc(engine, "safe", "*", "99");
        Calc(engine, "safe", "=", "99");

        Section("5. 意図的に変えた挙動（旧実装との差分）");
        _flags.Set("padded", "05");
        Console.WriteLine($"  ゼロ埋めの等値比較 padded == 5   : 新={_flags.EvaluateCondition("padded == 5")} / 旧={Legacy("padded == 5")}");
        _flags.Set("alice", "bob");
        Console.WriteLine($"  右辺が定義済みフラグ route == alice: 新={_flags.EvaluateCondition("route == alice")} / 旧={Legacy("route == alice")}");
        Console.WriteLine("    → 文字列として比較したいときは route == \"alice\" とクォートする");

        Console.WriteLine();
        Console.WriteLine($"結果: pass={_pass} fail={_fail} / 従来記法での新旧差異={_diff}");
        Console.WriteLine($"警告ログ {UnityEngine.Debug.Warnings.Count} 件");
        foreach (var w in UnityEngine.Debug.Warnings) Console.WriteLine("  WARN: " + w);

        return _fail == 0 ? 0 : 1;
    }

    private static void SetUpFlags()
    {
        _flags = new FlagManager();
        _flags.Set("demo_flag", "true");
        _flags.Set("false_flag", "false");
        _flags.Set("score", "100");
        _flags.Set("hp", "50");
        _flags.Set("mp", "20");
        _flags.Set("route", "alice");
        _flags.Set("count", "0");
        _flags.Set("yes_flag", "yes");
        _flags.Set("one", "1");
        _flags.Set("名声", "30");
        _legacy = typeof(FlagManager).GetMethod("EvaluateLegacy", BindingFlags.NonPublic | BindingFlags.Instance);
        if (_legacy == null) throw new InvalidOperationException("FlagManager.EvaluateLegacy が見つかりません");
    }

    private static bool Legacy(string condition) => (bool)_legacy.Invoke(_flags, new object[] { condition });

    private static void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }

    private static void Compat(string condition, bool expected)
    {
        bool now = _flags.EvaluateCondition(condition);
        bool old = Legacy(condition);
        bool ok = now == expected;
        if (ok) _pass++; else _fail++;
        if (now != old) _diff++;
        Console.WriteLine($"  [{(ok ? "OK" : "NG")}] {condition,-40} => {now,-5} (期待 {expected} / 旧 {old}){(now != old ? "  ★新旧差異" : "")}");
    }

    private static void Expect(string condition, bool expected)
    {
        bool now = _flags.EvaluateCondition(condition);
        bool ok = now == expected;
        if (ok) _pass++; else _fail++;
        Console.WriteLine($"  [{(ok ? "OK" : "NG")}] {condition,-40} => {now} (期待 {expected})");
    }

    private static void Fallback(string condition)
    {
        try
        {
            bool now = _flags.EvaluateCondition(condition);
            _pass++;
            Console.WriteLine($"  [OK] {condition,-40} => {now} (フォールバックで評価)");
        }
        catch (Exception e)
        {
            _fail++;
            Console.WriteLine($"  [NG] {condition,-40} => 例外 {e.GetType().Name}: {e.Message}");
        }
    }

    private static void Calc(NovellaEngine engine, string target, string value, string expected)
    {
        new CalcCommandHandler().Execute(new ScriptCommand { Target = target, Value = value }, engine, null);
        string actual = engine.Flags.Get(target);
        bool ok = actual == expected;
        if (ok) _pass++; else _fail++;
        Console.WriteLine($"  [{(ok ? "OK" : "NG")}] calc {target} {value,-18} => {actual} (期待 {expected})");
    }
}
