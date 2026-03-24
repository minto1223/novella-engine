using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.UI
{
    public class BackgroundController : MonoBehaviour, Novella.Core.IBackgroundDisplay
    {
        [SerializeField] private Image _backgroundImage;
        private const float DefaultFade = 0.4f;

        private RectTransform _bgRect;

        private void Awake()
        {
            if (_backgroundImage != null)
                _bgRect = _backgroundImage.GetComponent<RectTransform>();
        }

        public void Show(string imageName, float duration, Action onComplete)
        {
            Show(imageName, duration, null, onComplete);
        }

        public void Show(string imageName, float duration, string transition, Action onComplete)
        {
            if (_backgroundImage == null)
            {
                Debug.LogError("[Novella] BackgroundController: _backgroundImage is not assigned.");
                onComplete?.Invoke();
                return;
            }

            var sprite = string.IsNullOrEmpty(imageName)
                ? null
                : Resources.Load<Sprite>($"Backgrounds/{imageName}");

            if (sprite == null && !string.IsNullOrEmpty(imageName))
                Debug.LogWarning($"[Novella] Background not found: Backgrounds/{imageName}");

            float fade = duration > 0f ? duration : (sprite != null ? DefaultFade : 0f);

            if (fade <= 0f || sprite == null)
            {
                if (sprite != null) _backgroundImage.sprite = sprite;
                onComplete?.Invoke();
                return;
            }

            string t = (transition ?? "crossfade").ToLower();
            switch (t)
            {
                case "wipe_left":
                    StartCoroutine(WipeTransition(sprite, fade, Vector2.left, onComplete));
                    break;
                case "wipe_right":
                    StartCoroutine(WipeTransition(sprite, fade, Vector2.right, onComplete));
                    break;
                case "wipe_up":
                    StartCoroutine(WipeTransition(sprite, fade, Vector2.up, onComplete));
                    break;
                case "wipe_down":
                    StartCoroutine(WipeTransition(sprite, fade, Vector2.down, onComplete));
                    break;
                case "fade_white":
                    StartCoroutine(ColorFadeTransition(sprite, fade, Color.white, onComplete));
                    break;
                case "fade_black":
                    StartCoroutine(ColorFadeTransition(sprite, fade, Color.black, onComplete));
                    break;
                case "slide_left":
                    StartCoroutine(SlideTransition(sprite, fade, Vector2.left, onComplete));
                    break;
                case "slide_right":
                    StartCoroutine(SlideTransition(sprite, fade, Vector2.right, onComplete));
                    break;
                case "dissolve":
                    StartCoroutine(DissolveTransition(sprite, fade, onComplete));
                    break;
                default: // crossfade
                    StartCoroutine(CrossFade(sprite, fade, onComplete));
                    break;
            }
        }

        private IEnumerator CrossFade(Sprite nextSprite, float duration, Action onComplete)
        {
            float half = duration * 0.5f;
            yield return StartCoroutine(FadeAlpha(1f, 0f, half));
            _backgroundImage.sprite = nextSprite;
            yield return StartCoroutine(FadeAlpha(0f, 1f, half));
            onComplete?.Invoke();
        }

        /// <summary>
        /// ワイプトランジション: マスク風にスライドして切り替え
        /// 実装: 古い画像をスライドアウト→新しい画像を即表示
        /// </summary>
        private IEnumerator WipeTransition(Sprite nextSprite, float duration, Vector2 direction, Action onComplete)
        {
            if (_bgRect == null) _bgRect = _backgroundImage.GetComponent<RectTransform>();
            var canvas = _backgroundImage.GetComponentInParent<Canvas>();
            float canvasWidth = canvas != null ? ((RectTransform)canvas.transform).rect.width : 1920f;
            float canvasHeight = canvas != null ? ((RectTransform)canvas.transform).rect.height : 1080f;

            Vector2 startPos = Vector2.zero;
            Vector2 endPos = new Vector2(direction.x * canvasWidth, direction.y * canvasHeight);

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / duration);
                _bgRect.anchoredPosition = Vector2.Lerp(startPos, endPos, p);
                yield return null;
            }

            // 新しいスプライトに差し替えて位置をリセット
            _backgroundImage.sprite = nextSprite;
            _bgRect.anchoredPosition = Vector2.zero;
            var c = _backgroundImage.color;
            c.a = 1f;
            _backgroundImage.color = c;
            onComplete?.Invoke();
        }

        /// <summary>
        /// スライドトランジション: 新しい画像がスライドインして古い画像を押し出す
        /// </summary>
        private IEnumerator SlideTransition(Sprite nextSprite, float duration, Vector2 direction, Action onComplete)
        {
            if (_bgRect == null) _bgRect = _backgroundImage.GetComponent<RectTransform>();
            var canvas = _backgroundImage.GetComponentInParent<Canvas>();
            float canvasWidth = canvas != null ? ((RectTransform)canvas.transform).rect.width : 1920f;

            // 一時的な新画像を作成
            var tempGO = new GameObject("BgSlideTemp");
            tempGO.transform.SetParent(_backgroundImage.transform.parent, false);
            var tempImg = tempGO.AddComponent<Image>();
            tempImg.sprite = nextSprite;
            tempImg.preserveAspect = _backgroundImage.preserveAspect;
            var tempRect = tempGO.GetComponent<RectTransform>();
            tempRect.anchorMin = _bgRect.anchorMin;
            tempRect.anchorMax = _bgRect.anchorMax;
            tempRect.sizeDelta = _bgRect.sizeDelta;
            tempRect.anchoredPosition = new Vector2(-direction.x * canvasWidth, 0f);

            float t = 0f;
            Vector2 oldStart = Vector2.zero;
            Vector2 oldEnd = new Vector2(direction.x * canvasWidth, 0f);
            Vector2 newStart = tempRect.anchoredPosition;
            Vector2 newEnd = Vector2.zero;

            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / duration);
                _bgRect.anchoredPosition = Vector2.Lerp(oldStart, oldEnd, p);
                tempRect.anchoredPosition = Vector2.Lerp(newStart, newEnd, p);
                yield return null;
            }

            // 差し替え
            _backgroundImage.sprite = nextSprite;
            _bgRect.anchoredPosition = Vector2.zero;
            var c = _backgroundImage.color;
            c.a = 1f;
            _backgroundImage.color = c;
            Destroy(tempGO);
            onComplete?.Invoke();
        }

        /// <summary>
        /// 色フェードトランジション: 白/黒オーバーレイを経由して切り替え
        /// </summary>
        private IEnumerator ColorFadeTransition(Sprite nextSprite, float duration, Color fadeColor, Action onComplete)
        {
            float half = duration * 0.5f;

            // オーバーレイを背景画像の前面に生成
            var overlay = GetOrCreateFadeOverlay();
            overlay.gameObject.SetActive(true);
            fadeColor.a = 0f;
            overlay.color = fadeColor;

            // オーバーレイを透明 → 不透明にフェード（画面が白/黒に覆われる）
            float t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                float p = t / half;
                fadeColor.a = Mathf.Lerp(0f, 1f, p);
                overlay.color = fadeColor;
                yield return null;
            }
            fadeColor.a = 1f;
            overlay.color = fadeColor;

            // オーバーレイで隠れている間にスプライト差し替え
            _backgroundImage.sprite = nextSprite;
            _backgroundImage.color = Color.white;

            // オーバーレイを不透明 → 透明にフェード（新しい背景が現れる）
            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                float p = t / half;
                fadeColor.a = Mathf.Lerp(1f, 0f, p);
                overlay.color = fadeColor;
                yield return null;
            }

            overlay.gameObject.SetActive(false);
            onComplete?.Invoke();
        }

        /// <summary>
        /// 背景トランジション用のフェードオーバーレイを取得/生成
        /// </summary>
        private Image GetOrCreateFadeOverlay()
        {
            var parent = _backgroundImage.transform.parent;
            var existing = parent.Find("BgFadeOverlay");
            if (existing != null) return existing.GetComponent<Image>();

            var go = new GameObject("BgFadeOverlay");
            go.transform.SetParent(parent, false);
            // 背景画像の直後に配置
            go.transform.SetSiblingIndex(_backgroundImage.transform.GetSiblingIndex() + 1);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0);
            img.raycastTarget = false;
            go.SetActive(false);

            return img;
        }

        /// <summary>
        /// ディゾルブトランジション: 新旧画像を重ねてアルファブレンドで切り替え
        /// </summary>
        private IEnumerator DissolveTransition(Sprite nextSprite, float duration, Action onComplete)
        {
            // 一時的な新画像を背景の前面に重ねる
            var tempGO = new GameObject("BgDissolveTemp");
            tempGO.transform.SetParent(_backgroundImage.transform.parent, false);
            tempGO.transform.SetSiblingIndex(_backgroundImage.transform.GetSiblingIndex() + 1);
            var tempImg = tempGO.AddComponent<Image>();
            tempImg.sprite = nextSprite;
            tempImg.preserveAspect = _backgroundImage.preserveAspect;
            tempImg.raycastTarget = false;
            var tempRect = tempGO.GetComponent<RectTransform>();
            tempRect.anchorMin = _bgRect.anchorMin;
            tempRect.anchorMax = _bgRect.anchorMax;
            tempRect.sizeDelta = _bgRect.sizeDelta;
            tempRect.anchoredPosition = Vector2.zero;
            tempImg.color = new Color(1f, 1f, 1f, 0f);

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                tempImg.color = new Color(1f, 1f, 1f, p);
                yield return null;
            }

            _backgroundImage.sprite = nextSprite;
            _backgroundImage.color = Color.white;
            Destroy(tempGO);
            onComplete?.Invoke();
        }

        private Coroutine _kenBurnsCoroutine;

        public void StartKenBurns(float targetZoom, string position, float duration)
        {
            if (_bgRect == null && _backgroundImage != null)
                _bgRect = _backgroundImage.GetComponent<RectTransform>();
            if (_bgRect == null) return;

            if (_kenBurnsCoroutine != null) StopCoroutine(_kenBurnsCoroutine);
            _kenBurnsCoroutine = StartCoroutine(DoKenBurns(targetZoom, position, duration));
        }

        public void StopKenBurns(float resetDuration, Action onComplete)
        {
            if (_kenBurnsCoroutine != null)
            {
                StopCoroutine(_kenBurnsCoroutine);
                _kenBurnsCoroutine = null;
            }
            if (_bgRect == null) { onComplete?.Invoke(); return; }

            if (resetDuration > 0f)
                StartCoroutine(ResetKenBurns(resetDuration, onComplete));
            else
            {
                _bgRect.localScale = Vector3.one;
                _bgRect.anchoredPosition = Vector2.zero;
                onComplete?.Invoke();
            }
        }

        private IEnumerator DoKenBurns(float targetZoom, string position, float duration)
        {
            Vector3 startScale = _bgRect.localScale;
            Vector3 endScale = new Vector3(targetZoom, targetZoom, 1f);

            Vector2 canvasSize = _bgRect.rect.size;
            Vector2 pivot = GetKenBurnsPivot(position);
            Vector2 centerOffset = (pivot - new Vector2(0.5f, 0.5f)) * canvasSize;
            Vector2 targetPos = -centerOffset * (targetZoom - 1f);

            Vector2 startPos = _bgRect.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                _bgRect.localScale = Vector3.Lerp(startScale, endScale, t);
                _bgRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            _bgRect.localScale = endScale;
            _bgRect.anchoredPosition = targetPos;
            _kenBurnsCoroutine = null;
        }

        private IEnumerator ResetKenBurns(float duration, Action onComplete)
        {
            Vector3 startScale = _bgRect.localScale;
            Vector2 startPos = _bgRect.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                _bgRect.localScale = Vector3.Lerp(startScale, Vector3.one, t);
                _bgRect.anchoredPosition = Vector2.Lerp(startPos, Vector2.zero, t);
                yield return null;
            }

            _bgRect.localScale = Vector3.one;
            _bgRect.anchoredPosition = Vector2.zero;
            onComplete?.Invoke();
        }

        private static Vector2 GetKenBurnsPivot(string position)
        {
            switch ((position ?? "center").ToLower())
            {
                case "left":         return new Vector2(0.25f, 0.5f);
                case "right":        return new Vector2(0.75f, 0.5f);
                case "top":          return new Vector2(0.5f, 0.75f);
                case "bottom":       return new Vector2(0.5f, 0.25f);
                case "top_left":     return new Vector2(0.25f, 0.75f);
                case "top_right":    return new Vector2(0.75f, 0.75f);
                case "bottom_left":  return new Vector2(0.25f, 0.25f);
                case "bottom_right": return new Vector2(0.75f, 0.25f);
                default:             return new Vector2(0.5f, 0.5f);
            }
        }

        private IEnumerator FadeAlpha(float from, float to, float duration)
        {
            float t = 0f;
            var c = _backgroundImage.color;
            while (t < duration)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(from, to, t / duration);
                _backgroundImage.color = c;
                yield return null;
            }
            c.a = to;
            _backgroundImage.color = c;
        }
    }
}
