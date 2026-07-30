using UnityEngine;
using UnityEngine.UI;

namespace Novella.UI
{
    /// <summary>
    /// クリック待ちマーカー（▼）の点滅アニメーション。
    /// 1周期でフェード＋上下バウンスし、タイピング中は非表示になる。
    /// </summary>
    public class NextMarkerBlinker : MonoBehaviour
    {
        [SerializeField] private MessageWindowController _messageWindow;
        [SerializeField] private float _period = 1.1f;
        [SerializeField] private float _bobPixels = 3f;
        [SerializeField, Range(0f, 1f)] private float _minAlpha = 0.25f;

        private Graphic _graphic;
        private RectTransform _rect;
        private Vector2 _basePosition;

        private void Awake()
        {
            _graphic = GetComponent<Graphic>();
            _rect = GetComponent<RectTransform>();
            _basePosition = _rect.anchoredPosition;
            if (_messageWindow == null)
                _messageWindow = FindFirstObjectByType<MessageWindowController>();
        }

        private void Update()
        {
            bool visible = _messageWindow == null || !_messageWindow.IsTyping;
            if (_graphic != null && _graphic.enabled != visible)
                _graphic.enabled = visible;
            if (!visible) return;

            float phase = Time.unscaledTime % _period / _period;
            float wave = 0.5f + 0.5f * Mathf.Sin(phase * Mathf.PI * 2f);

            if (_graphic != null)
            {
                Color c = _graphic.color;
                c.a = Mathf.Lerp(_minAlpha, 1f, wave);
                _graphic.color = c;
            }
            _rect.anchoredPosition = _basePosition + new Vector2(0f, wave * _bobPixels);
        }
    }
}
