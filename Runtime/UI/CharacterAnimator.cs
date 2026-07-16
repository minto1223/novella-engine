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
    /// Dicing立ち絵（DicedImage）の場合はアトラス内の表情
    /// "{expression}_blink" / "{expression}_talk"（基本表情なら "blink" / "talk"）を使う。
    /// 該当バリアントが存在しない場合は何もしない。
    /// </summary>
    public class CharacterAnimator : MonoBehaviour
    {
        private Image _image;
        private DicedImage _diced; // DicedImageの場合のみ非null
        private string _characterId;
        private string _expression;

        // フルスプライト用
        private Sprite _normalSprite;
        private Sprite _blinkSprite;
        private Sprite _talkSprite;

        // Diced用（アトラス内表情名）
        private string _normalExpr;
        private string _blinkExpr;
        private string _talkExpr;

        private bool _hasBlink;
        private bool _hasTalk;

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
            _diced = image as DicedImage;
            _characterId = characterId;
            _expression = expression;

            LoadVariants();
            StartBlinking();
        }

        public void UpdateExpression(string newExpression, Sprite normalSprite)
        {
            // 現在のアニメーションを停止
            StopBlinking();
            StopTalking();

            _expression = newExpression;
            _normalSprite = normalSprite;

            LoadVariants();
            StartBlinking();

            if (_isTalking) StartTalking();
        }

        public void StartTalking()
        {
            _isTalking = true;
            if (_hasTalk && _lipSyncCoroutine == null)
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
            // 通常表情に戻す
            ShowNormal();
        }

        private void LoadVariants()
        {
            if (_diced != null && _diced.Data != null)
            {
                bool baseExpr = string.IsNullOrEmpty(_expression);
                _normalExpr = baseExpr ? "default" : _expression;
                _blinkExpr = baseExpr ? "blink" : $"{_expression}_blink";
                _talkExpr = baseExpr ? "talk" : $"{_expression}_talk";
                _hasBlink = _diced.Data.HasExpression(_blinkExpr);
                _hasTalk = _diced.Data.HasExpression(_talkExpr);
                return;
            }

            string basePath = string.IsNullOrEmpty(_expression)
                ? $"Characters/{_characterId}"
                : $"Characters/{_characterId}_{_expression}";

            if (_normalSprite == null)
                _normalSprite = Resources.Load<Sprite>(basePath);
            _blinkSprite = Resources.Load<Sprite>($"{basePath}_blink");
            _talkSprite = Resources.Load<Sprite>($"{basePath}_talk");
            _hasBlink = _blinkSprite != null;
            _hasTalk = _talkSprite != null;
        }

        private void ShowNormal()
        {
            if (_image == null) return;
            if (_diced != null) _diced.SetExpression(_normalExpr);
            else if (_normalSprite != null) _image.sprite = _normalSprite;
        }

        private void ShowBlink()
        {
            if (_image == null) return;
            if (_diced != null) _diced.SetExpression(_blinkExpr);
            else if (_blinkSprite != null) _image.sprite = _blinkSprite;
        }

        private void ShowTalk()
        {
            if (_image == null) return;
            if (_diced != null) _diced.SetExpression(_talkExpr);
            else if (_talkSprite != null) _image.sprite = _talkSprite;
        }

        private void StartBlinking()
        {
            if (_hasBlink && _blinkCoroutine == null)
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

                if (_image == null || !_hasBlink) yield break;

                // リップシンク中はまばたきを使わない（衝突回避）
                if (_lipSyncCoroutine != null) continue;

                ShowBlink();
                yield return new WaitForSeconds(_blinkDuration);

                if (_lipSyncCoroutine == null)
                    ShowNormal();
            }
        }

        private IEnumerator LipSyncLoop()
        {
            bool mouthOpen = false;
            while (_isTalking)
            {
                if (_image == null) yield break;

                mouthOpen = !mouthOpen;
                if (mouthOpen) ShowTalk();
                else ShowNormal();

                yield return new WaitForSeconds(_lipSyncInterval);
            }

            // 終了時は通常に戻す
            ShowNormal();
            _lipSyncCoroutine = null;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
