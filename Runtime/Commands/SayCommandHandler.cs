using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    public class SayCommandHandler : ICommandHandler
    {
        public string CommandType => "say";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            // 前のリップシンク・ボイスを停止
            engine.ICharacterLayer?.StopAllTalking();
            engine.IAudio?.StopVoice();

            // clip指定があればボイスを同時再生 & リップシンク開始
            if (!string.IsNullOrEmpty(command.Clip))
            {
                engine.IAudio?.PlayVoice(command.Clip, command.Volume, null);
                if (!string.IsNullOrEmpty(command.Character))
                    engine.ICharacterLayer?.StartTalking(command.Character);
            }

            // キャラ定義から表示名とカラーを解決
            string displayName = command.Character;
            string coloredName = command.Character;
            var charDef = engine.GetCharacterDef(command.Character);
            if (charDef != null)
            {
                if (!string.IsNullOrEmpty(charDef.displayName))
                    displayName = charDef.displayName;
                coloredName = $"<color=#{ColorUtility.ToHtmlStringRGB(charDef.nameColor)}>{displayName}</color>";
            }

            string resolvedText = engine.ResolveText(command.Text);
            string nameColorHex = charDef != null ? "#" + ColorUtility.ToHtmlStringRGB(charDef.nameColor) : null;
            engine.Backlog.Add(displayName, resolvedText, engine.CurrentScriptPath, engine.CurrentIndex, command.Clip, nameColorHex);
            engine.IBacklogUI?.Rebuild(engine.Backlog.Entries);
            engine.IMessageWindow?.Show(coloredName, resolvedText, () =>
            {
                // テキスト表示完了時にリップシンク停止
                if (!string.IsNullOrEmpty(command.Character))
                    engine.ICharacterLayer?.StopTalking(command.Character);
                engine.WaitForInput(onComplete);
            }, engine.CurrentCommandWasRead);
        }
    }
}
