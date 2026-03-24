using System;
using UnityEngine;

namespace Novella.Core.Templates
{
    /// <summary>
    /// カスタムオーディオプレイヤーのテンプレート。
    /// このクラスを継承して独自の音声再生を実装し、
    /// NovellaEngine の Custom UI Overrides > Audio Player にアサインしてください。
    /// </summary>
    public abstract class CustomAudioPlayer : MonoBehaviour, IAudioPlayer
    {
        public abstract bool IsVoicePlaying { get; }

        public abstract void PlayBgm(string clipName, float volume, Action onComplete);
        public abstract void StopBgm(float fadeDuration, Action onComplete);
        public virtual void FadeBgm(float targetVolume, float duration, Action onComplete) { onComplete?.Invoke(); }
        public virtual void PlayBgm(string clipName, float volume, float fadeInDuration, Action onComplete)
        {
            PlayBgm(clipName, volume, onComplete);
        }
        public abstract void PlaySe(string clipName, float volume, Action onComplete);
        public virtual void StopSe() { }
        public abstract void PlayVoice(string clipName, float volume, Action onComplete);
        public abstract void StopVoice();

        public virtual void SetBgmVolume(float volume) { }
        public virtual void SetSeVolume(float volume) { }
        public virtual void SetVoiceVolume(float volume) { }
    }
}
