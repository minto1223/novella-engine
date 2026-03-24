using UnityEngine;

namespace Novella.UI
{
    /// <summary>
    /// モバイル向けタッチ操作の補助。
    /// - 長押し(0.5秒以上): スキップモード開始、離すと解除
    /// - 上スワイプ: バックログ表示
    /// NovellaEngineと同じGameObjectにアタッチ。
    /// </summary>
    public class MobileTouchHandler : MonoBehaviour
    {
        [SerializeField] private Novella.Core.NovellaEngine _engine;

        private const float LongPressThreshold = 0.5f;
        private const float SwipeThreshold = 80f;

        private float _touchStartTime;
        private Vector2 _touchStartPos;
        private bool _longPressActive;

        private void Awake()
        {
            if (_engine == null)
                _engine = GetComponent<Novella.Core.NovellaEngine>();
        }

        private void Update()
        {
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
            if (Input.touchCount == 0)
            {
                if (_longPressActive)
                {
                    _longPressActive = false;
                    if (_engine != null)
                        _engine.SetSkipMode(false);
                }
                return;
            }

            var touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _touchStartTime = Time.time;
                    _touchStartPos = touch.position;
                    _longPressActive = false;
                    break;

                case TouchPhase.Stationary:
                case TouchPhase.Moved:
                    if (!_longPressActive && Time.time - _touchStartTime >= LongPressThreshold)
                    {
                        float dist = Vector2.Distance(touch.position, _touchStartPos);
                        if (dist < SwipeThreshold)
                        {
                            _longPressActive = true;
                            if (_engine != null)
                                _engine.SetSkipMode(true);
                        }
                    }
                    break;

                case TouchPhase.Ended:
                    if (_longPressActive)
                    {
                        _longPressActive = false;
                        if (_engine != null)
                            _engine.SetSkipMode(false);
                    }
                    else
                    {
                        // スワイプ判定
                        Vector2 delta = touch.position - _touchStartPos;
                        if (delta.magnitude >= SwipeThreshold)
                        {
                            // 上スワイプ → バックログ
                            if (delta.y > Mathf.Abs(delta.x))
                            {
                                if (_engine != null && _engine.BacklogUI != null && !_engine.BacklogUI.IsOpen)
                                    _engine.BacklogUI.Open();
                            }
                        }
                    }
                    break;
            }
#endif
        }
    }
}
