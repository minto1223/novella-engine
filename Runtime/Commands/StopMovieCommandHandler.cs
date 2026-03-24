using System;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    public class StopMovieCommandHandler : ICommandHandler
    {
        public string CommandType => "stop_movie";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            if (engine.MoviePlayer != null)
                engine.MoviePlayer.Stop();
            onComplete?.Invoke();
        }
    }
}
