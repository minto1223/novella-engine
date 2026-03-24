using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Novella.UI
{
    public class MoviePlayerController : MonoBehaviour
    {
        [SerializeField] private RawImage _displayImage;
        [SerializeField] private VideoPlayer _videoPlayer;

        private Action _onComplete;
        private bool _isPlaying;
        private RenderTexture _renderTexture;

        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            if (_videoPlayer == null)
                _videoPlayer = GetComponent<VideoPlayer>();

            if (_videoPlayer != null)
            {
                _videoPlayer.playOnAwake = false;
                _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                _videoPlayer.loopPointReached += OnMovieEnd;
            }

            if (_displayImage != null)
                _displayImage.gameObject.SetActive(false);
        }

        public void Play(string clipName, Action onComplete)
        {
            if (_videoPlayer == null)
            {
                Debug.LogError("[Novella] MoviePlayerController: VideoPlayer is not assigned.");
                onComplete?.Invoke();
                return;
            }

            var clip = Resources.Load<VideoClip>($"Movies/{clipName}");
            if (clip == null)
            {
                Debug.LogWarning($"[Novella] Movie not found: Movies/{clipName}");
                onComplete?.Invoke();
                return;
            }

            _onComplete = onComplete;
            _isPlaying = true;

            // RenderTexture作成
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
            }
            _renderTexture = new RenderTexture((int)clip.width, (int)clip.height, 0);
            _videoPlayer.targetTexture = _renderTexture;

            if (_displayImage != null)
            {
                _displayImage.texture = _renderTexture;
                _displayImage.gameObject.SetActive(true);
            }

            _videoPlayer.clip = clip;
            _videoPlayer.Play();
        }

        public void Stop()
        {
            if (!_isPlaying) return;
            _videoPlayer.Stop();
            Cleanup();
            _onComplete?.Invoke();
            _onComplete = null;
        }

        private void Update()
        {
            if (!_isPlaying) return;

            // スキップ: クリック/Enter/Spaceで停止
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                Stop();
            }
        }

        private void OnMovieEnd(VideoPlayer vp)
        {
            if (!_isPlaying) return;
            Cleanup();
            _onComplete?.Invoke();
            _onComplete = null;
        }

        private void Cleanup()
        {
            _isPlaying = false;
            if (_displayImage != null)
                _displayImage.gameObject.SetActive(false);
            if (_videoPlayer != null)
                _videoPlayer.Stop();
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
                _renderTexture = null;
            }
        }

        private void OnDestroy()
        {
            if (_videoPlayer != null)
                _videoPlayer.loopPointReached -= OnMovieEnd;
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
            }
        }
    }
}
