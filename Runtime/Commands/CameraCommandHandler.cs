using System;
using System.Collections;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    /// <summary>
    /// カメラ演出コマンド。
    /// zoom: 画面をズームイン/アウト。value="1.5" で1.5倍、"1" でリセット。
    ///   JSON: { "type": "zoom", "value": "1.5", "duration": 0.5, "position": "right" }
    ///   position: "left"/"right"/"center"/"top"/"bottom" でズーム先を指定（デフォルトcenter）
    /// pan: 画面をスライド移動。value="100,0" でX方向に100px移動。
    ///   JSON: { "type": "pan", "value": "100,50", "duration": 0.5 }
    /// reset_camera: ズーム・パンをリセット。
    ///   JSON: { "type": "reset_camera", "duration": 0.3 }
    /// </summary>
    public class ZoomCommandHandler : ICommandHandler
    {
        public string CommandType => "zoom";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            float duration = command.Duration > 0f ? command.Duration : 0.5f;
            float targetScale = 1f;
            if (!string.IsNullOrEmpty(command.Value) && float.TryParse(command.Value, out float s))
                targetScale = Mathf.Clamp(s, 0.5f, 3f);

            // ズーム先の pivot
            Vector2 pivot = GetPivot(command.Position);

            var canvas = FindNovellaCanvas(engine);
            if (canvas == null) { onComplete?.Invoke(); return; }

            engine.StartCoroutine(DoZoom(canvas, targetScale, pivot, duration, onComplete));
        }

        private static Vector2 GetPivot(string position)
        {
            switch (position?.ToLower())
            {
                case "left":   return new Vector2(0.25f, 0.5f);
                case "right":  return new Vector2(0.75f, 0.5f);
                case "top":    return new Vector2(0.5f, 0.75f);
                case "bottom": return new Vector2(0.5f, 0.25f);
                default:       return new Vector2(0.5f, 0.5f);
            }
        }

        private IEnumerator DoZoom(RectTransform canvas, float targetScale, Vector2 pivot, float duration, Action onComplete)
        {
            Vector3 startScale = canvas.localScale;
            Vector3 endScale = new Vector3(targetScale, targetScale, 1f);

            // pivotに向かってオフセット計算
            Vector2 canvasSize = canvas.rect.size;
            Vector2 centerOffset = (pivot - new Vector2(0.5f, 0.5f)) * canvasSize;
            Vector2 targetPos = -centerOffset * (targetScale - 1f);

            Vector2 startPos = canvas.anchoredPosition;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / duration);
                canvas.localScale = Vector3.Lerp(startScale, endScale, p);
                canvas.anchoredPosition = Vector2.Lerp(startPos, targetPos, p);
                yield return null;
            }
            canvas.localScale = endScale;
            canvas.anchoredPosition = targetPos;
            onComplete?.Invoke();
        }

        /// <summary>
        /// ズーム/パン対象のRectTransformを取得する。
        /// Screen Space - OverlayのCanvasはルート自身のTransform（scale/position）が描画に反映されないため、
        /// Canvas直下の"CameraRoot"ラッパーを対象にする。存在しない場合はCanvas自体にフォールバック（Overlayでは効果なし）。
        /// </summary>
        internal static RectTransform FindNovellaCanvas(NovellaEngine engine)
        {
            Canvas canvas = null;
            if (engine.Background != null)
                canvas = engine.Background.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                var canvasGo = GameObject.Find("NovellaCanvas");
                if (canvasGo != null) canvas = canvasGo.GetComponent<Canvas>();
            }
            if (canvas == null) return null;

            var cameraRoot = canvas.transform.Find("CameraRoot");
            if (cameraRoot != null) return cameraRoot.GetComponent<RectTransform>();

            return canvas.GetComponent<RectTransform>();
        }
    }

    public class PanCommandHandler : ICommandHandler
    {
        public string CommandType => "pan";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            float duration = command.Duration > 0f ? command.Duration : 0.5f;
            Vector2 offset = Vector2.zero;

            if (!string.IsNullOrEmpty(command.Value))
            {
                var parts = command.Value.Split(',');
                if (parts.Length >= 2
                    && float.TryParse(parts[0].Trim(), out float x)
                    && float.TryParse(parts[1].Trim(), out float y))
                    offset = new Vector2(x, y);
            }

            var canvas = ZoomCommandHandler.FindNovellaCanvas(engine);
            if (canvas == null) { onComplete?.Invoke(); return; }

            engine.StartCoroutine(DoPan(canvas, offset, duration, onComplete));
        }

        private IEnumerator DoPan(RectTransform canvas, Vector2 offset, float duration, Action onComplete)
        {
            Vector2 start = canvas.anchoredPosition;
            Vector2 end = start + offset;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                canvas.anchoredPosition = Vector2.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t / duration));
                yield return null;
            }
            canvas.anchoredPosition = end;
            onComplete?.Invoke();
        }
    }

    public class ResetCameraCommandHandler : ICommandHandler
    {
        public string CommandType => "reset_camera";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            float duration = command.Duration > 0f ? command.Duration : 0.3f;
            var canvas = ZoomCommandHandler.FindNovellaCanvas(engine);
            if (canvas == null) { onComplete?.Invoke(); return; }

            engine.StartCoroutine(DoReset(canvas, duration, onComplete));
        }

        private IEnumerator DoReset(RectTransform canvas, float duration, Action onComplete)
        {
            Vector3 startScale = canvas.localScale;
            Vector2 startPos = canvas.anchoredPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / duration);
                canvas.localScale = Vector3.Lerp(startScale, Vector3.one, p);
                canvas.anchoredPosition = Vector2.Lerp(startPos, Vector2.zero, p);
                yield return null;
            }
            canvas.localScale = Vector3.one;
            canvas.anchoredPosition = Vector2.zero;
            onComplete?.Invoke();
        }
    }
}
