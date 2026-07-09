using System;
using System.Collections;
using UnityEngine;

namespace Novella.UI
{
    public class AudioController : MonoBehaviour, Novella.Core.IAudioPlayer
    {
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _seSource;
        [SerializeField] private AudioSource _voiceSource;

        private AudioSource _bgmSource2;
        private bool _usingSource2;
        private Coroutine _crossfadeCoroutine;

        private AudioSource ActiveBgmSource => _usingSource2 ? _bgmSource2 : _bgmSource;
        private AudioSource InactiveBgmSource => _usingSource2 ? _bgmSource : _bgmSource2;

        private void Awake()
        {
            if (_bgmSource != null && _bgmSource2 == null)
            {
                _bgmSource2 = gameObject.AddComponent<AudioSource>();
                _bgmSource2.playOnAwake = false;
                _bgmSource2.loop = true;
                _bgmSource2.volume = 0f;
            }
        }

        public void PlayBgm(string clipName, float volume, Action onComplete)
        {
            PlayBgm(clipName, volume, 0f, onComplete);
        }

        public void PlayBgm(string clipName, float volume, float fadeInDuration, Action onComplete)
        {
            if (_bgmSource == null)
            {
                Debug.LogError("[Novella] AudioController: _bgmSource is not assigned.");
                onComplete?.Invoke();
                return;
            }
            var clip = Resources.Load<AudioClip>($"Audio/BGM/{clipName}");
            if (clip == null)
            {
                Debug.LogWarning($"[Novella] BGM not found: Audio/BGM/{clipName}");
                onComplete?.Invoke();
                return;
            }

            float targetVolume = Mathf.Clamp01((volume > 0 ? volume : 1f) * Novella.Core.SettingsData.BgmVolume);

            // 現在BGMが再生中ならクロスフェード
            if (ActiveBgmSource.isPlaying)
            {
                if (_crossfadeCoroutine != null)
                    StopCoroutine(_crossfadeCoroutine);

                _usingSource2 = !_usingSource2;
                var newSource = ActiveBgmSource;
                var oldSource = InactiveBgmSource;

                newSource.clip = clip;
                newSource.volume = 0f;
                newSource.loop = true;
                newSource.Play();

                float dur = fadeInDuration > 0 ? fadeInDuration : 1.0f;
                _crossfadeCoroutine = StartCoroutine(CrossfadeBgm(oldSource, newSource, targetVolume, dur, onComplete));
            }
            else
            {
                var source = ActiveBgmSource;
                source.clip = clip;
                source.loop = true;

                if (fadeInDuration > 0f)
                {
                    source.volume = 0f;
                    source.Play();
                    if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                    _fadeCoroutine = StartCoroutine(FadeBgmVolume(targetVolume, fadeInDuration, onComplete));
                }
                else
                {
                    source.volume = targetVolume;
                    source.Play();
                    onComplete?.Invoke();
                }
            }
        }

        public void StopBgm(float fadeDuration, Action onComplete)
        {
            if (_crossfadeCoroutine != null)
            {
                StopCoroutine(_crossfadeCoroutine);
                _crossfadeCoroutine = null;
            }

            if (fadeDuration > 0f)
                StartCoroutine(FadeOutBgm(fadeDuration, onComplete));
            else
            {
                _bgmSource.Stop();
                if (_bgmSource2 != null) _bgmSource2.Stop();
                onComplete?.Invoke();
            }
        }

        private Coroutine _fadeCoroutine;

        public void FadeBgm(float targetVolume, float duration, Action onComplete)
        {
            float scaledTarget = Mathf.Clamp01(targetVolume * Novella.Core.SettingsData.BgmVolume);
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            if (duration <= 0f)
            {
                SetBgmVolume(scaledTarget);
                onComplete?.Invoke();
                return;
            }
            _fadeCoroutine = StartCoroutine(FadeBgmVolume(scaledTarget, duration, onComplete));
        }

        private IEnumerator FadeBgmVolume(float targetVolume, float duration, Action onComplete)
        {
            var source = ActiveBgmSource;
            if (source == null || !source.isPlaying)
            {
                onComplete?.Invoke();
                yield break;
            }
            float startVol = source.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVol, targetVolume, elapsed / duration);
                yield return null;
            }
            source.volume = targetVolume;
            _fadeCoroutine = null;
            onComplete?.Invoke();
        }

        public void StopSe()
        {
            if (_seSource != null) _seSource.Stop();
        }

        public void PlaySe(string clipName, float volume, Action onComplete)
        {
            if (_seSource == null)
            {
                Debug.LogError("[Novella] AudioController: _seSource is not assigned.");
                onComplete?.Invoke();
                return;
            }
            var clip = Resources.Load<AudioClip>($"Audio/SE/{clipName}");
            if (clip == null)
            {
                Debug.LogWarning($"[Novella] SE not found: Audio/SE/{clipName}");
                onComplete?.Invoke();
                return;
            }
            _seSource.PlayOneShot(clip, Mathf.Clamp01(volume > 0 ? volume : 1f));
            onComplete?.Invoke();
        }

        public void SetBgmVolume(float volume)
        {
            float v = Mathf.Clamp01(volume);
            if (_bgmSource != null) _bgmSource.volume = v;
            if (_bgmSource2 != null) _bgmSource2.volume = v;
        }

        public void SetSeVolume(float volume)
        {
            _seSource.volume = Mathf.Clamp01(volume);
        }

        public void PlayVoice(string clipName, float volume, Action onComplete)
        {
            if (_voiceSource == null)
            {
                Debug.LogError("[Novella] AudioController: _voiceSource is not assigned.");
                onComplete?.Invoke();
                return;
            }
            var clip = Resources.Load<AudioClip>($"Audio/Voice/{clipName}");
            if (clip == null)
            {
                Debug.LogWarning($"[Novella] Voice not found: Audio/Voice/{clipName}");
                onComplete?.Invoke();
                return;
            }
            _voiceSource.Stop();
            _voiceSource.clip = clip;
            _voiceSource.volume = Mathf.Clamp01((volume > 0 ? volume : 1f) * Novella.Core.SettingsData.VoiceVolume);
            _voiceSource.loop = false;
            _voiceSource.Play();
            onComplete?.Invoke();
        }

        public void StopVoice()
        {
            if (_voiceSource != null) _voiceSource.Stop();
        }

        public bool IsVoicePlaying =>
            _voiceSource != null && _voiceSource.isPlaying;

        public void SetVoiceVolume(float volume)
        {
            if (_voiceSource != null) _voiceSource.volume = Mathf.Clamp01(volume);
        }

        private IEnumerator CrossfadeBgm(AudioSource oldSource, AudioSource newSource, float targetVolume, float duration, Action onComplete)
        {
            float oldStartVolume = oldSource.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                oldSource.volume = Mathf.Lerp(oldStartVolume, 0f, t);
                newSource.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }
            oldSource.Stop();
            oldSource.volume = 0f;
            newSource.volume = targetVolume;
            _crossfadeCoroutine = null;
            onComplete?.Invoke();
        }

        private IEnumerator FadeOutBgm(float duration, Action onComplete)
        {
            float startVol1 = _bgmSource.isPlaying ? _bgmSource.volume : 0f;
            float startVol2 = _bgmSource2 != null && _bgmSource2.isPlaying ? _bgmSource2.volume : 0f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (_bgmSource.isPlaying) _bgmSource.volume = Mathf.Lerp(startVol1, 0f, t);
                if (_bgmSource2 != null && _bgmSource2.isPlaying) _bgmSource2.volume = Mathf.Lerp(startVol2, 0f, t);
                yield return null;
            }
            _bgmSource.Stop();
            _bgmSource.volume = startVol1;
            if (_bgmSource2 != null)
            {
                _bgmSource2.Stop();
                _bgmSource2.volume = 0f;
            }
            onComplete?.Invoke();
        }
    }
}
