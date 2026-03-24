using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    /// <summary>
    /// 表示中のキャラをスムーズに別のポジションに移動する。
    /// JSON: { "type": "move_char", "character": "alice", "position": "right", "duration": 0.5 }
    /// </summary>
    public class MoveCharCommandHandler : ICommandHandler
    {
        public string CommandType => "move_char";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (engine.ICharacterLayer == null)
            {
                Debug.LogWarning("[Novella] move_char: CharacterLayer not assigned.");
                onComplete?.Invoke();
                return;
            }
            string characterId = command.Character ?? command.Image;
            int order = ShowCharCommandHandler.ResolveLayer(command.Layer, command.Order);
            engine.TrackCharacterShow(characterId, null, command.Position);
            engine.ICharacterLayer.MoveCharacter(
                characterId,
                command.Position,
                command.Duration,
                order,
                onComplete
            );
        }
    }
}
