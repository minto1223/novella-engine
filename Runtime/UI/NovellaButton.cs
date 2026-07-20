using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Novella.Core;

namespace Novella.UI
{
    /// <summary>
    /// NovellaButtonStyle（4状態スタイル）を読んでボタンの見た目を制御するコンポーネント。
    /// 装着するとUnity標準のColorTint遷移は無効化され（競合防止）、
    /// 枠線・角括弧・sheen（光）の装飾は実行時に自動生成される。
    /// スタイル未設定の間は何もしない（従来のテーマ適用にフォールバック）。
    /// ホバーとuGUIナビゲーションのフォーカス（Select）は同一の演出として扱う。
    /// </summary>
    [RequireComponent(typeof(Button))]
    [DisallowMultipleComponent]
    public class NovellaButton : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler,
        ISelectHandler, IDeselectHandler
    {
        private enum VisualState { Normal, Hover, Pressed, Disabled }

        [SerializeField]
        [Tooltip("このボタンのスタイル。未設定ならテーマ側スタイルが自動割り当てされる")]
        private NovellaButtonStyle _style;

        [SerializeField]
        [Tooltip("文字色もスタイルで制御する。AUTO/SKIPのように他の処理がON/OFF表示で文字色を変えるボタンではOFFにする")]
        private bool _controlLabelColor = true;

        private const float BorderThickness = 2f;
        private const float CornerSize = 14f;
        private const float CornerInset = 4f;
        private const float SheenDuration = 0.6f;

        private Button _button;
        private Image _background;
        private TMP_Text _label;
        private AudioSource _seSource;
        private Vector3 _baseScale;

        private Image[] _borderBars;
        private CanvasGroup _cornersGroup;
        private Image[] _cornerBars;
        private RectTransform _sheenRect;
        private Image _sheenImage;

        private bool _hovered;
        private bool _pressed;
        private bool _selected;
        private bool _lastInteractable = true;
        private bool _initialized;
        private VisualState _currentState;
        private Coroutine _transition;
        private Coroutine _sheenCo;

        private static Sprite _sheenSprite;

        public bool HasStyle => _style != null;

        public NovellaButtonStyle Style => _style;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _background = GetComponent<Image>();
            _label = GetComponentInChildren<TMP_Text>(true);
            _baseScale = transform.localScale;

            if (_style != null)
                Initialize();
        }

        private void OnEnable()
        {
            _hovered = false;
            _pressed = false;
            _selected = false;
            if (_initialized)
                ApplyState(ComputeState(), instant: true);
        }

        private void Update()
        {
            if (!_initialized) return;
            if (_button.interactable != _lastInteractable)
            {
                _lastInteractable = _button.interactable;
                ApplyState(ComputeState(), instant: false);
            }
        }

        /// <summary>スタイルを割り当てて見た目を初期化する。テーマ適用側から呼ばれる。</summary>
        public void SetStyle(NovellaButtonStyle style)
        {
            if (style == null) return;
            _style = style;
            if (_button == null) return; // Awake前ならAwakeで初期化される
            Initialize();
        }

        private void Initialize()
        {
            if (!Application.isPlaying) return;

            // 競合防止: Unity標準のColorTint遷移を切り、色制御をこちらに一本化する
            _button.transition = Selectable.Transition.None;
            _lastInteractable = _button.interactable;

            if (_style.BackgroundSprite != null && _background != null)
            {
                _background.sprite = _style.BackgroundSprite;
                _background.type = Image.Type.Sliced;
            }

            if (!_initialized)
            {
                BuildDecorations();
                _initialized = true;
            }

            foreach (var bar in _cornerBars)
                bar.color = _style.CornerColor;

            ApplyState(ComputeState(), instant: true);
        }

        // ---- 状態管理 ----

        private VisualState ComputeState()
        {
            if (!_button.interactable) return VisualState.Disabled;
            if (_pressed) return VisualState.Pressed;
            if (_hovered || _selected) return VisualState.Hover;
            return VisualState.Normal;
        }

        private ButtonStateStyle GetStateStyle(VisualState state)
        {
            switch (state)
            {
                case VisualState.Hover: return _style.Hover;
                case VisualState.Pressed: return _style.Pressed;
                case VisualState.Disabled: return _style.Disabled;
                default: return _style.Normal;
            }
        }

        private void Refresh()
        {
            if (!_initialized) return;
            var next = ComputeState();
            if (next == _currentState) return;
            ApplyState(next, instant: false);
        }

        private void ApplyState(VisualState state, bool instant)
        {
            _currentState = state;
            var target = GetStateStyle(state);

            if (_transition != null)
            {
                StopCoroutine(_transition);
                _transition = null;
            }

            if (!instant && target.EnterSe != null)
                PlaySe(target.EnterSe);

            if (target.PlaySheen && !instant)
                StartSheen();
            else if (!target.PlaySheen)
                StopSheen();

            float duration = instant ? 0f : _style.TransitionDuration;
            if (duration <= 0f || !gameObject.activeInHierarchy)
            {
                SetVisual(target, 1f, CaptureVisual());
                return;
            }

            _transition = StartCoroutine(TransitionTo(target, duration));
        }

        private IEnumerator TransitionTo(ButtonStateStyle target, float duration)
        {
            var from = CaptureVisual();
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
                SetVisual(target, p, from);
                yield return null;
            }
            SetVisual(target, 1f, from);
            _transition = null;
        }

        private struct VisualSnapshot
        {
            public Color Background;
            public Color Border;
            public Color Text;
            public float CornerAlpha;
            public float Scale;
        }

        private VisualSnapshot CaptureVisual()
        {
            return new VisualSnapshot
            {
                Background = _background != null ? _background.color : Color.white,
                Border = _borderBars[0].color,
                Text = _label != null ? _label.color : Color.white,
                CornerAlpha = _cornersGroup.alpha,
                Scale = _baseScale.x > 0f ? transform.localScale.x / _baseScale.x : 1f,
            };
        }

        private void SetVisual(ButtonStateStyle target, float p, VisualSnapshot from)
        {
            if (_background != null)
                _background.color = Color.Lerp(from.Background, target.BackgroundColor, p);

            var borderColor = Color.Lerp(from.Border, target.BorderColor, p);
            foreach (var bar in _borderBars)
                bar.color = borderColor;

            if (_label != null && _controlLabelColor)
                _label.color = Color.Lerp(from.Text, target.TextColor, p);

            _cornersGroup.alpha = Mathf.Lerp(from.CornerAlpha, target.ShowCorners ? 1f : 0f, p);
            transform.localScale = _baseScale * Mathf.Lerp(from.Scale, target.Scale, p);
        }

        // ---- 入力イベント（ホバー＝フォーカス同一視） ----

        public void OnPointerEnter(PointerEventData eventData) { _hovered = true; Refresh(); }
        public void OnPointerExit(PointerEventData eventData) { _hovered = false; _pressed = false; Refresh(); }
        public void OnPointerDown(PointerEventData eventData) { _pressed = true; Refresh(); }
        public void OnPointerUp(PointerEventData eventData)
        {
            _pressed = false;
            // クリックでuGUIの選択状態が残ると、マウスを離してもホバー演出（選択＝ホバー扱い）が
            // 固定されてしまうため、ポインタ操作による選択は解除する（パッド/キーボードの選択は影響なし）
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
                EventSystem.current.SetSelectedGameObject(null);
            Refresh();
        }
        public void OnSelect(BaseEventData eventData) { _selected = true; Refresh(); }
        public void OnDeselect(BaseEventData eventData) { _selected = false; Refresh(); }

        // ---- SE ----

        private void PlaySe(AudioClip clip)
        {
            if (_seSource == null)
            {
                _seSource = gameObject.AddComponent<AudioSource>();
                _seSource.playOnAwake = false;
                _seSource.spatialBlend = 0f;
            }
            _seSource.PlayOneShot(clip);
        }

        // ---- 装飾の自動生成 ----

        private void BuildDecorations()
        {
            // sheenをボタン矩形で切り抜くためのマスク
            if (GetComponent<RectMask2D>() == null)
                gameObject.AddComponent<RectMask2D>();

            BuildBorder();
            BuildCorners();
            BuildSheen();
        }

        private RectTransform CreateChild(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        private Image CreateBar(string name, Transform parent)
        {
            var rt = CreateChild(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.raycastTarget = false;
            return img;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void BuildBorder()
        {
            var root = CreateChild("NovellaBorder", transform);
            Stretch(root);

            _borderBars = new Image[4];
            // 上下: 横に伸ばす / 左右: 縦に伸ばす
            for (int i = 0; i < 4; i++)
            {
                var bar = CreateBar(i == 0 ? "Top" : i == 1 ? "Bottom" : i == 2 ? "Left" : "Right", root);
                var rt = bar.rectTransform;
                switch (i)
                {
                    case 0: rt.anchorMin = new Vector2(0, 1); rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 1f); rt.sizeDelta = new Vector2(0, BorderThickness); break;
                    case 1: rt.anchorMin = Vector2.zero; rt.anchorMax = new Vector2(1, 0); rt.pivot = new Vector2(0.5f, 0f); rt.sizeDelta = new Vector2(0, BorderThickness); break;
                    case 2: rt.anchorMin = Vector2.zero; rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0f, 0.5f); rt.sizeDelta = new Vector2(BorderThickness, 0); break;
                    case 3: rt.anchorMin = new Vector2(1, 0); rt.anchorMax = Vector2.one; rt.pivot = new Vector2(1f, 0.5f); rt.sizeDelta = new Vector2(BorderThickness, 0); break;
                }
                rt.anchoredPosition = Vector2.zero;
                _borderBars[i] = bar;
            }
        }

        private void BuildCorners()
        {
            var root = CreateChild("NovellaCorners", transform);
            Stretch(root);
            _cornersGroup = root.gameObject.AddComponent<CanvasGroup>();
            _cornersGroup.alpha = 0f;
            _cornersGroup.interactable = false;
            _cornersGroup.blocksRaycasts = false;

            _cornerBars = new Image[8];
            int idx = 0;
            for (int x = 0; x <= 1; x++)
            {
                for (int y = 0; y <= 1; y++)
                {
                    var corner = CreateChild($"Corner_{(y == 1 ? "T" : "B")}{(x == 0 ? "L" : "R")}", root);
                    var anchor = new Vector2(x, y);
                    corner.anchorMin = anchor;
                    corner.anchorMax = anchor;
                    corner.pivot = anchor;
                    corner.sizeDelta = new Vector2(CornerSize, CornerSize);
                    corner.anchoredPosition = new Vector2(x == 0 ? CornerInset : -CornerInset, y == 0 ? CornerInset : -CornerInset);

                    // L字 = 横バー + 縦バー（コーナー側に密着）
                    var h = CreateBar("H", corner);
                    h.rectTransform.anchorMin = anchor;
                    h.rectTransform.anchorMax = anchor;
                    h.rectTransform.pivot = anchor;
                    h.rectTransform.sizeDelta = new Vector2(CornerSize, BorderThickness);
                    h.rectTransform.anchoredPosition = Vector2.zero;
                    _cornerBars[idx++] = h;

                    var v = CreateBar("V", corner);
                    v.rectTransform.anchorMin = anchor;
                    v.rectTransform.anchorMax = anchor;
                    v.rectTransform.pivot = anchor;
                    v.rectTransform.sizeDelta = new Vector2(BorderThickness, CornerSize);
                    v.rectTransform.anchoredPosition = Vector2.zero;
                    _cornerBars[idx++] = v;
                }
            }
        }

        private void BuildSheen()
        {
            _sheenRect = CreateChild("NovellaSheen", transform);
            _sheenRect.anchorMin = new Vector2(0f, 0.5f);
            _sheenRect.anchorMax = new Vector2(0f, 0.5f);
            _sheenRect.pivot = new Vector2(0.5f, 0.5f);
            _sheenRect.localRotation = Quaternion.Euler(0f, 0f, -12f);

            _sheenImage = _sheenRect.gameObject.AddComponent<Image>();
            _sheenImage.sprite = GetSheenSprite();
            _sheenImage.color = new Color(1f, 0.92f, 0.72f, 0.35f);
            _sheenImage.raycastTarget = false;
            _sheenRect.gameObject.SetActive(false);
        }

        private static Sprite GetSheenSprite()
        {
            if (_sheenSprite != null) return _sheenSprite;

            // 中央が明るい横方向グラデーションを生成
            const int w = 64;
            var tex = new Texture2D(w, 1, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[w];
            for (int i = 0; i < w; i++)
            {
                float a = Mathf.Sin(Mathf.PI * i / (w - 1f));
                pixels[i] = new Color(1f, 1f, 1f, a * a);
            }
            tex.SetPixels(pixels);
            tex.Apply();
            _sheenSprite = Sprite.Create(tex, new Rect(0, 0, w, 1), new Vector2(0.5f, 0.5f));
            _sheenSprite.name = "NovellaSheenGradient";
            return _sheenSprite;
        }

        private void StartSheen()
        {
            if (_sheenCo != null) StopCoroutine(_sheenCo);
            if (!gameObject.activeInHierarchy) return;
            _sheenCo = StartCoroutine(SheenSweep());
        }

        private void StopSheen()
        {
            if (_sheenCo != null)
            {
                StopCoroutine(_sheenCo);
                _sheenCo = null;
            }
            if (_sheenRect != null)
                _sheenRect.gameObject.SetActive(false);
        }

        private IEnumerator SheenSweep()
        {
            var rect = ((RectTransform)transform).rect;
            float sheenWidth = Mathf.Max(24f, rect.width * 0.35f);
            _sheenRect.sizeDelta = new Vector2(sheenWidth, rect.height * 1.6f);
            _sheenRect.gameObject.SetActive(true);

            float t = 0f;
            float startX = -sheenWidth;
            float endX = rect.width + sheenWidth;
            while (t < SheenDuration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / SheenDuration);
                _sheenRect.anchoredPosition = new Vector2(Mathf.Lerp(startX, endX, p), 0f);
                yield return null;
            }
            _sheenRect.gameObject.SetActive(false);
            _sheenCo = null;
        }
    }
}
