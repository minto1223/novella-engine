using System;
using Novella.Core;

namespace Novella.Commands
{
    public class ChoiceCommandHandler : ICommandHandler
    {
        public string CommandType => "choice";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (command.Choices == null || command.Choices.Count == 0)
            {
                UnityEngine.Debug.LogWarning("[Novella] choice: choices list is empty.");
                onComplete?.Invoke();
                return;
            }

            // 選択肢表示前にオートセーブ
            engine.SaveManager.AutoSave(engine);

            engine.IMessageWindow?.Hide();

            if (engine.IChoiceUI == null)
            {
                UnityEngine.Debug.LogWarning("[Novella] choice: ChoiceUI not assigned.");
                onComplete?.Invoke();
                return;
            }

            // 条件を満たす選択肢のみ表示
            var visibleChoices = new System.Collections.Generic.List<Novella.Core.ChoiceOption>();
            foreach (var choice in command.Choices)
            {
                if (engine.Flags.EvaluateCondition(choice.Condition))
                    visibleChoices.Add(choice);
            }

            if (visibleChoices.Count == 0)
            {
                UnityEngine.Debug.LogWarning("[Novella] choice: no choices passed condition filter.");
                onComplete?.Invoke();
                return;
            }

            engine.IChoiceUI.Show(visibleChoices, selected =>
            {
                // 「選択肢後もスキップを続ける」がOFFならスキップ解除
                if (!Novella.Core.SettingsData.SkipAfterChoice && engine.SkipMode)
                    engine.SetSkipMode(false);

                // フラグをセット
                if (!string.IsNullOrEmpty(selected.SetFlag))
                    engine.Flags.Set(selected.SetFlag, selected.FlagValue ?? "true");

                // ラベルへジャンプ
                if (!string.IsNullOrEmpty(selected.Target))
                    engine.JumpToLabel(selected.Target);

                onComplete?.Invoke();
            });
        }
    }
}
