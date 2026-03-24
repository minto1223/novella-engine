using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    public class EndCommandHandler : ICommandHandler
    {
        public string CommandType => "end";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            Debug.Log("[Novella] End command: script complete.");

            // エンディング記録
            if (!string.IsNullOrEmpty(command.Label))
                EndingManager.RecordEnding(command.Label);

            // シーン回想用に記録
            SceneRecollectionManager.RecordScene(engine.CurrentScriptPath, engine.CurrentScriptTitle);

            engine.IMessageWindow?.Hide();

            // 回想モードならタイトルに戻る
            if (engine.IsRecollectionMode)
            {
                engine.EndRecollection();
                return;
            }

            engine.Stop();
            // onComplete は呼ばない（スクリプト終了）
        }
    }
}
