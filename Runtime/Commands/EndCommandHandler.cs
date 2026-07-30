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

            // 通常プレイでもタイトルへ戻す。
            // 以前は Stop() するだけだったため、最後まで進めると背景だけが残って
            // 何も起きない状態になっていた（ESCメニュー以外に抜け道が無かった）
            engine.ReturnToTitle();
            // onComplete は呼ばない（スクリプト終了）
        }
    }
}
