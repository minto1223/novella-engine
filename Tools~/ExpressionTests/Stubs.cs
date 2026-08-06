// Unityを起動せずに式評価とcalcを検証するための最小スタブ。
// ここに定義するのはテスト対象が参照する型だけで、実装は本体のソースをそのまま使う。
using System;

namespace UnityEngine
{
    public static class Debug
    {
        public static readonly System.Collections.Generic.List<string> Warnings = new System.Collections.Generic.List<string>();

        public static void Log(object message) { }
        public static void LogWarning(object message) { Warnings.Add(message?.ToString()); }
    }
}

namespace Novella.Core
{
    public class ScriptCommand
    {
        public string Target;
        public string Value;
    }

    public interface ICommandHandler
    {
        string CommandType { get; }
        void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete);
    }

    /// <summary>本体のNovellaEngineのうち、CalcCommandHandlerが触る部分だけを持つスタブ。</summary>
    public class NovellaEngine
    {
        public readonly FlagManager Flags = new FlagManager();
    }
}
