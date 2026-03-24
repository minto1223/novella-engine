using System;

namespace Novella.Core
{
    public interface IBackgroundDisplay
    {
        void Show(string imageName, float duration, Action onComplete);
        void Show(string imageName, float duration, string transition, Action onComplete);
        void StartKenBurns(float targetZoom, string position, float duration);
        void StopKenBurns(float resetDuration, Action onComplete);
    }
}
