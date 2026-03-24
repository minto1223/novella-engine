using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    public class PlayMovieCommandHandler : ICommandHandler
    {
        public string CommandType => "play_movie";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (engine.MoviePlayer == null)
            {
                Debug.LogWarning("[Novella] play_movie: MoviePlayerController not assigned.");
                onComplete?.Invoke();
                return;
            }
            string clipName = command.Clip ?? command.Image;
            engine.MoviePlayer.Play(clipName, onComplete);
        }
    }
}
