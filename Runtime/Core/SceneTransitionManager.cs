using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Novella.Core
{
    /// <summary>
    /// シーン間フェードトランジションを管理するシングルトン。
    /// Awake() 時にフェード用 Canvas を自動生成するため、
    /// TitleScene の任意の GameObject にアタッチするだけで使用可能。
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [SerializeField] private float _fadeDuration = 0.5f;

        private CanvasGroup _fadeCanvasGroup;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateFadeCanvas();
        }

        /// <summary>フェードアウト→シーンロード→フェードインを実行する。</summary>
        public void LoadScene(string sceneName, Action onLoaded = null)
        {
            StartCoroutine(FadeAndLoad(sceneName, onLoaded));
        }

        private IEnumerator FadeAndLoad(string sceneName, Action onLoaded)
        {
            yield return StartCoroutine(Fade(0f, 1f));
            yield return SceneManager.LoadSceneAsync(sceneName);
            onLoaded?.Invoke();
            yield return StartCoroutine(Fade(1f, 0f));
        }

        private IEnumerator Fade(float from, float to)
        {
            float elapsed = 0f;
            _fadeCanvasGroup.alpha = from;
            _fadeCanvasGroup.blocksRaycasts = true;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / _fadeDuration);
                yield return null;
            }

            _fadeCanvasGroup.alpha = to;
            _fadeCanvasGroup.blocksRaycasts = (to >= 1f);
        }

        private void CreateFadeCanvas()
        {
            // Canvas
            var canvasGO = new GameObject("FadeCanvas");
            canvasGO.transform.SetParent(transform);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // 最前面

            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // 全画面黒パネル
            var panelGO = new GameObject("FadePanel");
            panelGO.transform.SetParent(canvasGO.transform, false);

            var rect = panelGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var image = panelGO.AddComponent<Image>();
            image.color = Color.black;

            // CanvasGroup（アルファ制御用）
            _fadeCanvasGroup = canvasGO.AddComponent<CanvasGroup>();
            _fadeCanvasGroup.alpha = 0f;
            _fadeCanvasGroup.blocksRaycasts = false;
            _fadeCanvasGroup.interactable = false;
        }
    }
}
