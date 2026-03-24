using System;
using UnityEngine;

namespace Novella.Core.Templates
{
    /// <summary>
    /// カスタム背景表示のテンプレート。
    /// このクラスを継承して独自の背景切替ロジックを実装し、
    /// NovellaEngine の Custom UI Overrides > Background にアサインしてください。
    /// </summary>
    public abstract class CustomBackgroundDisplay : MonoBehaviour, IBackgroundDisplay
    {
        /// <summary>背景を表示する。表示完了後に onComplete を呼ぶこと。</summary>
        public abstract void Show(string imageName, float duration, Action onComplete);

        /// <summary>トランジション付きで背景を表示する。</summary>
        public virtual void Show(string imageName, float duration, string transition, Action onComplete)
        {
            Show(imageName, duration, onComplete);
        }

        /// <summary>Ken Burnsエフェクト（ゆっくりズーム＆パン）を開始する。</summary>
        public virtual void StartKenBurns(float targetZoom, string position, float duration) { }

        /// <summary>Ken Burnsエフェクトを停止し、リセットする。</summary>
        public virtual void StopKenBurns(float resetDuration, Action onComplete) { onComplete?.Invoke(); }
    }
}
