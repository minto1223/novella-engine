using System;
using System.Collections;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    public class WaitCommandHandler : ICommandHandler
    {
        public string CommandType => "wait";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            engine.StartCoroutine(WaitRoutine(command.Duration, onComplete));
        }

        private IEnumerator WaitRoutine(float duration, Action onComplete)
        {
            yield return new WaitForSeconds(duration > 0 ? duration : 0.1f);
            onComplete?.Invoke();
        }
    }
}
