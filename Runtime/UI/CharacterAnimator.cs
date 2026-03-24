using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.UI
{
    /// <summary>
    /// 立ち絵のまばたき・リップシンクを自動制御するコンポーネント。
    /// スプライト命名規則:
    ///   通常: Characters/{id}_{expression}
    ///   まばたき: Characters/{id}_{expression}_blink
    ///   口パク: Characters/{id}_{expression}_talk
    /// 該当スプライトが存在しない場合は何もしない。
    /// </summary>
    public class CharacterAnimator : MonoBehaviour
    {
        private Image _image;
        private string _characterId;
        private string _expression;
        private Sprite _normalSprite;
        private Sprite _blinkSprite;
        private Sprite _talkSprite;

        private bool _isTalking;
        private Coroutine _blinkCoroutine;
        private Coroutine _lipSyncCoroutine;

        [Header("Blink Settings")]
        private float _blinkIntervalMin = 2f;
        private float _blinkIntervalMax = 6f;
        private float _blinkDuration = 0.12f;

        [Header("Lip Sync Settings")]
        private float _lipSyncInterval = 0.12f;

        public void Init(Image image, string characterId, string expression)
        {
            _image = image;
            _characterId = characterId;
            _expression = expression;

            LoadSprites();
            StartBlinking();
        }

        public void UpdateExpression(string newExpression, Sprite normalSprite)
        {
            // 現在のアニメーションを停止
            StopBlinking();
            StopTalking();

            _expression = newExpression;
            _normalSprite = normalSprite;

            LoadSprites();
            StartBlinking();

            if (_isTalking) StartTalking();
        }

        public void StartTalking()
        {
            _isTalking = true;
            if (_talkSprite != null && _lipSyncCoroutine == null)
                _lipSyncCoroutine = StartCoroutine(LipSyncLoop());
        }

        public void StopTalking()
        {
            _isTalking = false;
            if (_lipSyncCoroutine != null)
            {
                StopCoroutine(_lipSyncCoroutine);
                _lipSyncCoroutine = null;
            }
            // 通常スプライトに戻す
            if (_image != null && _normalSprite != null)
                _image.sprite = _normalSprite;
        }

        private void LoadSprites()
        {
            string basePath = string.IsNullOrEmpty(_expression)
                ? $"Characters/{_characterId}"
                : $"Characters/{_characterId}_{_expression}";

            _normalSprite = Resources.Load<Sprite>(basePath);
            _blinkSprite = Resources.Load<Sprite>($"{basePath}_blink");
            _talkSprite = Resources.Load<Sprite>($"{basePath}_talk");
        }

        private void StartBlinking()
        {
            if (_blinkSprite != null && _blinkCoroutine == null)
                _blinkCoroutine = StartCoroutine(BlinkLoop());
        }

        private void StopBlinking()
        {
            if (_blinkCoroutine != null)
            {
                StopCoroutine(_blinkCoroutine);
                _blinkCoroutine = null;
            }
        }

        private IEnumerator BlinkLoop()
        {
            while (true)
            {
                float wait = Random.Range(_blinkIntervalMin, _blinkIntervalMax);
                yield return new WaitForSeconds(wait);

                if (_image == null || _blinkSprite == null) yield break;

                // リップシンク中はまばたきスプライトを使わない（衝突回避）
                if (_lipSyncCoroutine != null) continue;

                _image.sprite = _blinkSprite;
                yield return new WaitForSeconds(_blinkDuration);

                if (_image != null && _normalSprite != null)
                    _image.sprite = _normalSprite;
            }
        }

        private IEnumerator LipSyncLoop()
        {
            bool mouthOpen = false;
            while (_isTalking)
            {
                if (_image == null) yield break;

                mouthOpen = !mouthOpen;
                _image.sprite = mouthOpen ? _talkSprite : _normalSprite;

                yield return new WaitForSeconds(_lipSyncInterval);
            }

            // 終了時は通常に戻す
            if (_image != null && _normalSprite != null)
                _image.sprite = _normalSprite;
            _lipSyncCoroutine = null;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
