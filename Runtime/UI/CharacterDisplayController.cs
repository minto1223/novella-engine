using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.UI
{
    public class CharacterDisplayController : MonoBehaviour, Novella.Core.ICharacterDisplay
    {
        [Header("Character Slots")]
        [SerializeField] private Transform _leftSlot;
        [SerializeField] private Transform _centerSlot;
        [SerializeField] private Transform _rightSlot;

        [Header("Prefab")]
        [SerializeField] private GameObject _characterImagePrefab;

        private const float DefaultFade = 0.3f;
        private const float SlideDistance = 200f;
        private const float BounceHeight = 30f;
        private const float ExpressionCrossfade = 0.2f;

        private readonly Dictionary<string, Image> _activeCharacters =
            new Dictionary<string, Image>();

        private readonly Dictionary<string, CharacterAnimator> _animators =
            new Dictionary<string, CharacterAnimator>();

        // Dicing差分立ち絵データのキャッシュ（値がnull＝データ無しも記録して再Loadを避ける）
        private static readonly Dictionary<string, Novella.Core.DicedCharacterData> _dicedCache =
            new Dictionary<string, Novella.Core.DicedCharacterData>();

        private static Novella.Core.DicedCharacterData GetDicedData(string characterId)
        {
            if (!_dicedCache.TryGetValue(characterId, out var data))
            {
                data = Resources.Load<Novella.Core.DicedCharacterData>($"Characters/Diced/{characterId}");
                _dicedCache[characterId] = data;
            }
            return data;
        }

        public void ShowCharacter(string characterId, string expression, string position, float duration, Action onComplete)
        {
            ShowCharacter(characterId, expression, position, duration, null, onComplete);
        }

        public void ShowCharacter(string characterId, string expression, string position, float duration, string effect, Action onComplete)
        {
            ShowCharacter(characterId, expression, position, duration, effect, -1, onComplete);
        }

        public void ShowCharacter(string characterId, string expression, string position, float duration, string effect, int order, Action onComplete)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                Debug.LogWarning("[Novella] ShowCharacter: characterId is empty.");
                onComplete?.Invoke();
                return;
            }

            // Dicing差分立ち絵データがあればアトラス合成描画を優先し、フルスプライトは読み込まない
            var dicedData = GetDicedData(characterId);

            Sprite sprite = null;
            string spritePath = null;
            if (dicedData == null)
            {
                spritePath = string.IsNullOrEmpty(expression)
                    ? $"Characters/{characterId}"
                    : $"Characters/{characterId}_{expression}";

                sprite = Resources.Load<Sprite>(spritePath);
                if (sprite == null && !string.IsNullOrEmpty(expression))
                    sprite = Resources.Load<Sprite>($"Characters/{characterId}");
            }

            bool isNewCharacter = !_activeCharacters.TryGetValue(characterId, out var img);

            if (isNewCharacter)
            {
                if (_characterImagePrefab == null)
                {
                    Debug.LogError("[Novella] CharacterDisplayController: _characterImagePrefab is not assigned.");
                    onComplete?.Invoke();
                    return;
                }
                var slot = GetSlot(position);
                if (slot == null)
                {
                    Debug.LogError("[Novella] CharacterDisplayController: No slot assigned.");
                    onComplete?.Invoke();
                    return;
                }
                var go = Instantiate(_characterImagePrefab, slot);
                if (dicedData != null)
                {
                    // プレハブのImageをDicedImage（アトラス合成描画）に差し替える。
                    // DicedImageはImage派生なので以降のフェード・移動処理は共通で動く
                    var srcImg = go.GetComponent<Image>();
                    bool raycast = srcImg != null && srcImg.raycastTarget;
                    bool preserve = srcImg != null && srcImg.preserveAspect;
                    if (srcImg != null) DestroyImmediate(srcImg);
                    var diced = go.AddComponent<DicedImage>();
                    diced.raycastTarget = raycast;
                    diced.preserveAspect = preserve;
                    diced.Init(dicedData, expression);
                    img = diced;
                }
                else
                {
                    img = go.GetComponent<Image>();
                }
                _activeCharacters[characterId] = img;

                // アニメーター追加
                var animator = go.GetComponent<CharacterAnimator>();
                if (animator == null) animator = go.AddComponent<CharacterAnimator>();
                animator.Init(img, characterId, expression);
                _animators[characterId] = animator;
            }
            else
            {
                // 既存キャラ: 位置変更チェック
                var newSlot = GetSlot(position);
                if (newSlot != null && img.transform.parent != newSlot)
                {
                    img.transform.SetParent(newSlot, false);
                }

                // 表情変更: クロスフェード
                bool expressionChanged = false;
                Action applyNewExpression = null;
                if (img is DicedImage dicedImg)
                {
                    string key = DicedImage.Normalize(expression);
                    if (dicedImg.CurrentExpression != key)
                    {
                        if (dicedImg.Data != null && dicedImg.Data.HasExpression(key))
                        {
                            expressionChanged = true;
                            applyNewExpression = () => dicedImg.SetExpression(expression);
                        }
                        else
                        {
                            Debug.LogWarning($"[Novella] Diced expression not found: {characterId}/{key}");
                        }
                    }
                }
                else if (sprite != null && img.sprite != sprite)
                {
                    expressionChanged = true;
                    var newSprite = sprite;
                    var targetImg = img;
                    applyNewExpression = () => targetImg.sprite = newSprite;
                }

                if (expressionChanged)
                {
                    StartCoroutine(CrossfadeExpression(img, applyNewExpression, ExpressionCrossfade));
                    // アニメーター更新
                    if (_animators.TryGetValue(characterId, out var anim))
                        anim.UpdateExpression(expression, sprite);
                    // Z-order適用
                    if (order >= 0)
                        img.transform.SetSiblingIndex(order);
                    onComplete?.Invoke();
                    return;
                }
            }

            if (img is DicedImage)
            {
                // 表情はInit/SetExpression側で適用済み
            }
            else if (sprite != null)
                img.sprite = sprite;
            else
                Debug.LogWarning($"[Novella] Character sprite not found: {spritePath}");

            img.gameObject.SetActive(true);

            // Z-order適用
            if (order >= 0)
                img.transform.SetSiblingIndex(order);

            float fade = duration > 0f ? duration : DefaultFade;
            string e = (effect ?? "").ToLower();

            switch (e)
            {
                case "slide_left":
                    StartCoroutine(SlideIn(img, fade, Vector2.left, onComplete));
                    break;
                case "slide_right":
                    StartCoroutine(SlideIn(img, fade, Vector2.right, onComplete));
                    break;
                case "slide_up":
                    StartCoroutine(SlideIn(img, fade, Vector2.down, onComplete));
                    break;
                case "slide_down":
                    StartCoroutine(SlideIn(img, fade, Vector2.up, onComplete));
                    break;
                case "bounce":
                    StartCoroutine(BounceIn(img, fade, onComplete));
                    break;
                case "zoom_in":
                    StartCoroutine(ZoomIn(img, fade, onComplete));
                    break;
                default: // fade
                    StartCoroutine(FadeAlpha(img, 0f, 1f, fade, onComplete));
                    break;
            }
        }

        public void MoveCharacter(string characterId, string toPosition, float duration, Action onComplete)
        {
            MoveCharacter(characterId, toPosition, duration, -1, onComplete);
        }

        public void MoveCharacter(string characterId, string toPosition, float duration, int order, Action onComplete)
        {
            if (!_activeCharacters.TryGetValue(characterId, out var img))
            {
                Debug.LogWarning($"[Novella] MoveCharacter: '{characterId}' not displayed.");
                onComplete?.Invoke();
                return;
            }

            var newSlot = GetSlot(toPosition);
            if (newSlot == null)
            {
                onComplete?.Invoke();
                return;
            }

            float moveDur = duration > 0f ? duration : 0.4f;
            int capturedOrder = order;
            StartCoroutine(SlideTo(img, newSlot, moveDur, () =>
            {
                if (capturedOrder >= 0 && img != null)
                    img.transform.SetSiblingIndex(capturedOrder);
                onComplete?.Invoke();
            }));
        }

        public void HideCharacter(string characterId, Action onComplete)
        {
            HideCharacter(characterId, null, onComplete);
        }

        public void HideCharacter(string characterId, string effect, Action onComplete)
        {
            if (!string.IsNullOrEmpty(characterId) && _activeCharacters.TryGetValue(characterId, out var img))
            {
                _activeCharacters.Remove(characterId);
                _animators.Remove(characterId);
                string e = (effect ?? "").ToLower();

                Action cleanup = () =>
                {
                    if (img != null) Destroy(img.gameObject);
                    onComplete?.Invoke();
                };

                switch (e)
                {
                    case "slide_left":
                        StartCoroutine(SlideOut(img, DefaultFade, Vector2.left, cleanup));
                        break;
                    case "slide_right":
                        StartCoroutine(SlideOut(img, DefaultFade, Vector2.right, cleanup));
                        break;
                    case "slide_up":
                        StartCoroutine(SlideOut(img, DefaultFade, Vector2.up, cleanup));
                        break;
                    case "slide_down":
                        StartCoroutine(SlideOut(img, DefaultFade, Vector2.down, cleanup));
                        break;
                    case "zoom_out":
                        StartCoroutine(ZoomOut(img, DefaultFade, cleanup));
                        break;
                    default: // fade
                        StartCoroutine(FadeAlpha(img, 1f, 0f, DefaultFade, cleanup));
                        break;
                }
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        /// <summary>指定キャラクターのリップシンクを開始する</summary>
        public void StartTalking(string characterId)
        {
            if (!string.IsNullOrEmpty(characterId) && _animators.TryGetValue(characterId, out var anim))
                anim.StartTalking();
        }

        /// <summary>指定キャラクターのリップシンクを停止する</summary>
        public void StopTalking(string characterId)
        {
            if (!string.IsNullOrEmpty(characterId) && _animators.TryGetValue(characterId, out var anim))
                anim.StopTalking();
        }

        /// <summary>全立ち絵を即座に削除する（ロード時用）</summary>
        public void HideAllCharacters()
        {
            foreach (var img in _activeCharacters.Values)
            {
                if (img != null) Destroy(img.gameObject);
            }
            _activeCharacters.Clear();
            _animators.Clear();
        }

        /// <summary>全キャラクターのリップシンクを停止する</summary>
        public void StopAllTalking()
        {
            foreach (var anim in _animators.Values)
                anim.StopTalking();
        }

        /// <summary>表情クロスフェード: 一時的にコピーを重ねてフェード</summary>
        private IEnumerator CrossfadeExpression(Image img, Action applyNewExpression, float duration)
        {
            // 旧表情のコピーを同じ場所に生成（DicedImageもシリアライズ値ごと複製される）
            var oldGO = Instantiate(img.gameObject, img.transform.parent);
            var oldImg = oldGO.GetComponent<Image>();
            oldGO.transform.SetSiblingIndex(img.transform.GetSiblingIndex());

            // 新表情に差し替え
            applyNewExpression?.Invoke();
            img.transform.SetAsLastSibling();

            // 旧画像をフェードアウト
            float t = 0f;
            var c = oldImg.color;
            while (t < duration)
            {
                if (oldImg == null) break;
                t += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, t / duration);
                oldImg.color = c;
                yield return null;
            }

            if (oldGO != null) Destroy(oldGO);
        }

        /// <summary>キャラ移動: スロット間をスムーズに移動</summary>
        private IEnumerator SlideTo(Image img, Transform newSlot, float duration, Action onComplete)
        {
            var rect = img.GetComponent<RectTransform>();
            Vector3 startWorldPos = rect.position;

            // 一時的に新スロットの子にして目標位置を取得
            img.transform.SetParent(newSlot, true);
            Canvas.ForceUpdateCanvases();
            // anchoredPositionを0にした時のワールド位置を計算
            var savedAnchor = rect.anchoredPosition;
            rect.anchoredPosition = Vector2.zero;
            Vector3 endWorldPos = rect.position;

            // 元の位置に戻してアニメーション
            rect.position = startWorldPos;

            float t = 0f;
            while (t < duration)
            {
                if (img == null) break;
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / duration);
                rect.position = Vector3.Lerp(startWorldPos, endWorldPos, p);
                yield return null;
            }

            if (img != null)
            {
                rect.anchoredPosition = Vector2.zero;
            }
            onComplete?.Invoke();
        }

        /// <summary>スライドイン: 指定方向の逆側からスライド + フェードイン</summary>
        private IEnumerator SlideIn(Image img, float duration, Vector2 fromDirection, Action onComplete)
        {
            var rect = img.GetComponent<RectTransform>();
            Vector2 targetPos = rect.anchoredPosition;
            Vector2 startPos = targetPos + fromDirection * SlideDistance;
            rect.anchoredPosition = startPos;

            var c = img.color;
            c.a = 0f;
            img.color = c;

            float t = 0f;
            while (t < duration)
            {
                if (img == null) break;
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / duration);
                rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, p);
                c.a = p;
                img.color = c;
                yield return null;
            }

            if (img != null)
            {
                rect.anchoredPosition = targetPos;
                c.a = 1f;
                img.color = c;
            }
            onComplete?.Invoke();
        }

        /// <summary>スライドアウト: 指定方向にスライド + フェードアウト</summary>
        private IEnumerator SlideOut(Image img, float duration, Vector2 direction, Action onComplete)
        {
            var rect = img.GetComponent<RectTransform>();
            Vector2 startPos = rect.anchoredPosition;
            Vector2 endPos = startPos + direction * SlideDistance;

            var c = img.color;
            float startAlpha = c.a;

            float t = 0f;
            while (t < duration)
            {
                if (img == null) break;
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / duration);
                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, p);
                c.a = Mathf.Lerp(startAlpha, 0f, p);
                img.color = c;
                yield return null;
            }
            onComplete?.Invoke();
        }

        /// <summary>バウンスイン: 下から跳ねるように登場</summary>
        private IEnumerator BounceIn(Image img, float duration, Action onComplete)
        {
            var rect = img.GetComponent<RectTransform>();
            Vector2 targetPos = rect.anchoredPosition;
            Vector2 startPos = targetPos + Vector2.down * BounceHeight * 2f;
            rect.anchoredPosition = startPos;

            var c = img.color;
            c.a = 0f;
            img.color = c;

            float t = 0f;
            while (t < duration)
            {
                if (img == null) break;
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                float bounce = BounceEase(p);
                rect.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, bounce);
                c.a = Mathf.Clamp01(p * 3f);
                img.color = c;
                yield return null;
            }

            if (img != null)
            {
                rect.anchoredPosition = targetPos;
                c.a = 1f;
                img.color = c;
            }
            onComplete?.Invoke();
        }

        private static float BounceEase(float t)
        {
            if (t < 0.7f)
                return (t / 0.7f) * (t / 0.7f) * 1.2f;
            else if (t < 0.9f)
            {
                float p = (t - 0.7f) / 0.2f;
                return 1f + (1f - p * p) * 0.1f;
            }
            else
                return 1f;
        }

        /// <summary>ズームイン: 小さい状態から拡大しつつフェードイン</summary>
        private IEnumerator ZoomIn(Image img, float duration, Action onComplete)
        {
            var rect = img.GetComponent<RectTransform>();
            Vector3 targetScale = rect.localScale;
            rect.localScale = targetScale * 0.3f;

            var c = img.color;
            c.a = 0f;
            img.color = c;

            float t = 0f;
            while (t < duration)
            {
                if (img == null) break;
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / duration);
                rect.localScale = Vector3.Lerp(targetScale * 0.3f, targetScale, p);
                c.a = p;
                img.color = c;
                yield return null;
            }

            if (img != null)
            {
                rect.localScale = targetScale;
                c.a = 1f;
                img.color = c;
            }
            onComplete?.Invoke();
        }

        /// <summary>ズームアウト: 縮小しつつフェードアウト</summary>
        private IEnumerator ZoomOut(Image img, float duration, Action onComplete)
        {
            var rect = img.GetComponent<RectTransform>();
            Vector3 startScale = rect.localScale;

            var c = img.color;
            float startAlpha = c.a;

            float t = 0f;
            while (t < duration)
            {
                if (img == null) break;
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / duration);
                rect.localScale = Vector3.Lerp(startScale, startScale * 0.3f, p);
                c.a = Mathf.Lerp(startAlpha, 0f, p);
                img.color = c;
                yield return null;
            }
            onComplete?.Invoke();
        }

        private IEnumerator FadeAlpha(Image img, float from, float to, float duration, Action onComplete)
        {
            float t = 0f;
            var c = img.color;
            c.a = from;
            img.color = c;

            while (t < duration)
            {
                if (img == null) break;
                t += Time.deltaTime;
                c.a = Mathf.Lerp(from, to, t / duration);
                img.color = c;
                yield return null;
            }

            if (img != null)
            {
                c.a = to;
                img.color = c;
            }

            onComplete?.Invoke();
        }

        private Transform GetSlot(string position)
        {
            return position switch
            {
                "left"  => _leftSlot   ?? _centerSlot,
                "right" => _rightSlot  ?? _centerSlot,
                _       => _centerSlot
            };
        }
    }
}
