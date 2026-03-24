using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    /// <summary>
    /// 次のスクリプトファイルをロードして再生する。
    /// JSON: { "type": "next_script", "target": "Scripts/chapter02" }
    /// CSV:  next_script,,,,,,,,Scripts/chapter02,,,
    /// </summary>
    public class NextScriptCommandHandler : ICommandHandler
    {
        public string CommandType => "next_script";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            string target = command.Target;
            if (string.IsNullOrEmpty(target))
            {
                Debug.LogError("[Novella] next_script: target が指定されていません。");
                engine.Stop();
                return;
            }

            // 現チャプターを回想記録
            SceneRecollectionManager.RecordScene(engine.CurrentScriptPath, engine.CurrentScriptTitle);

            // チャプター切替前にオートセーブ
            engine.SaveManager.AutoSave(engine);

            Debug.Log($"[Novella] next_script: loading '{target}'");
            engine.LoadAndPlay(target);
        }
    }
}
