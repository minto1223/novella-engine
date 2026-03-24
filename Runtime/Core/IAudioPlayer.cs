using System;

namespace Novella.Core
{
    public interface IAudioPlayer
    {
        void PlayBgm(string clipName, float volume, Action onComplete);
        void StopBgm(float fadeDuration, Action onComplete);
        void FadeBgm(float targetVolume, float duration, Action onComplete);
        void PlayBgm(string clipName, float volume, float fadeInDuration, Action onComplete);
        void PlaySe(string clipName, float volume, Action onComplete);
        void StopSe();
        void PlayVoice(string clipName, float volume, Action onComplete);
        void StopVoice();
        bool IsVoicePlaying { get; }
        void SetBgmVolume(float volume);
        void SetSeVolume(float volume);
        void SetVoiceVolume(float volume);
    }
}
