using System;
using Novella.Core;

namespace Novella.Commands
{
    public class LabelCommandHandler : ICommandHandler
    {
        public string CommandType => "label";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            string label = command.Label;

            // 回想モード: 終了ラベルに到達したらタイトルに戻る
            if (!string.IsNullOrEmpty(label) && engine.CheckRecollectionEndLabel(label))
                return; // onCompleteを呼ばない → 実行停止

            // シーン自動記録: SceneDefinitionの終了ラベル通過を検知
            if (!string.IsNullOrEmpty(label))
                engine.AutoRecordScene(label);

            onComplete?.Invoke();
        }
    }
}
