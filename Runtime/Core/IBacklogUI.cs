using System;
using System.Collections.Generic;

namespace Novella.Core
{
    public interface IBacklogUI
    {
        void Rebuild(IReadOnlyList<BacklogEntry> entries);
        void SetJumpCallback(Action<string, int> onJump);
    }
}
