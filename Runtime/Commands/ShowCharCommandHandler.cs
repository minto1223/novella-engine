using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    public class ShowCharCommandHandler : ICommandHandler
    {
        public string CommandType => "show_char";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (engine.ICharacterLayer == null)
            {
                Debug.LogWarning("[Novella] show_char: CharacterLayer not assigned.");
                onComplete?.Invoke();
                return;
            }
            string characterId = command.Character ?? command.Image;
            int order = ResolveLayer(command.Layer, command.Order);
            engine.TrackCharacterShow(characterId, command.Expression, command.Position);
            engine.ICharacterLayer.ShowCharacter(
                characterId,
                command.Expression,
                command.Position,
                command.Duration,
                command.Value,
                order,
                onComplete
            );
        }

        /// <summary>layerパラメータをorder値に変換。front=9999, back=0, 数値=そのまま。</summary>
        internal static int ResolveLayer(string layer, int fallbackOrder)
        {
            if (string.IsNullOrEmpty(layer)) return fallbackOrder;
            switch (layer.ToLower())
            {
                case "front": return 9999;
                case "back": return 0;
                default:
                    if (int.TryParse(layer, out int val)) return val;
                    return fallbackOrder;
            }
        }
    }
}
