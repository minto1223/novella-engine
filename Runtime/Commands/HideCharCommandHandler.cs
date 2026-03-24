using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    public class HideCharCommandHandler : ICommandHandler
    {
        public string CommandType => "hide_char";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (engine.ICharacterLayer == null)
            {
                Debug.LogWarning("[Novella] hide_char: CharacterLayer not assigned.");
                onComplete?.Invoke();
                return;
            }
            string characterId = command.Character ?? command.Image;
            engine.TrackCharacterHide(characterId);
            engine.ICharacterLayer.HideCharacter(characterId, command.Value, onComplete);
        }
    }
}
