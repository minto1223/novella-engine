using System;
using System.Collections;
using Novella.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.Commands
{
    /// <summary>
    /// 画面演出コマンド。
    /// shake: 画面を揺らす（duration, valueで強度）
    /// flash: 画面を白/黒フラッシュ（duration, valueで色 "white"/"black"）
    /// fade:  画面を暗転/明転（duration, valueで "in"/"out"）
    /// </summary>
    public class ShakeCommandHandler : ICommandHandler
    {
        public string CommandType => "shake";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            float duration = command.Duration > 0f ? command.Duration : 0.4f;
            float intensity = 10f;
            if (!string.IsNullOrEmpty(command.Value) && float.TryParse(command.Value, out float v))
                intensity = v;

            // Screen Space - Overlay Canvasはカメラ移動が反映されないため、
            // Canvas内の全子要素のanchoredPositionを直接揺らす
            Canvas canvas = null;
            if (engine.Background != null)
                canvas = engine.Background.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                var canvasGo = GameObject.Find("NovellaCanvas");
                if (canvasGo != null) canvas = canvasGo.GetComponent<Canvas>();
            }

            if (canvas != null)
            {
                engine.StartCoroutine(DoShakeCanvas(canvas.transform, duration, intensity, onComplete));
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        private IEnumerator DoShakeCanvas(Transform canvasTransform, float duration, float intensity, Action onComplete)
        {
            // 全直下子要素のRectTransformと元位置を記録
            var children = new System.Collections.Generic.List<(RectTransform rt, Vector2 origPos)>();
            for (int i = 0; i < canvasTransform.childCount; i++)
            {
                var rt = canvasTransform.GetChild(i) as RectTransform;
                if (rt != null)
                    children.Add((rt, rt.anchoredPosition));
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float x = UnityEngine.Random.Range(-intensity, intensity);
                float y = UnityEngine.Random.Range(-intensity, intensity);
                var offset = new Vector2(x, y);

                foreach (var (rt, origPos) in children)
                    rt.anchoredPosition = origPos + offset;

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 元の位置に復元
            foreach (var (rt, origPos) in children)
                rt.anchoredPosition = origPos;

            onComplete?.Invoke();
        }
    }

    public class FlashCommandHandler : ICommandHandler
    {
        public string CommandType => "flash";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            float duration = command.Duration > 0f ? command.Duration : 0.3f;
            Color color = command.Value == "black" ? Color.black : Color.white;

            engine.StartCoroutine(DoFlash(engine, color, duration, onComplete));
        }

        private IEnumerator DoFlash(NovellaEngine engine, Color color, float duration, Action onComplete)
        {
            var panel = GetOrCreateOverlay(engine);
            color.a = 1f;
            panel.color = color;
            panel.gameObject.SetActive(true);

            float half = duration * 0.5f;
            // フェードイン
            float t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                color.a = Mathf.Lerp(0f, 1f, t / half);
                panel.color = color;
                yield return null;
            }
            // フェードアウト
            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, t / half);
                panel.color = color;
                yield return null;
            }

            panel.gameObject.SetActive(false);
            onComplete?.Invoke();
        }

        internal static Image GetOrCreateOverlay(NovellaEngine engine)
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

            var existing = canvas.transform.Find("ScreenEffectOverlay");
            if (existing != null) return existing.GetComponent<Image>();

            var go = new GameObject("ScreenEffectOverlay");
            go.transform.SetParent(canvas.transform, false);
            go.transform.SetAsLastSibling();

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
    }

    public class FadeCommandHandler : ICommandHandler
    {
        public string CommandType => "fade";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            float duration = command.Duration > 0f ? command.Duration : 0.5f;
            bool fadeOut = command.Value != "in"; // デフォルトは暗転（out）
            Color color = command.Target == "white" ? Color.white : Color.black;

            engine.StartCoroutine(DoFade(engine, color, fadeOut, duration, onComplete));
        }

        private IEnumerator DoFade(NovellaEngine engine, Color color, bool fadeOut, float duration, Action onComplete)
        {
            var panel = FlashCommandHandler.GetOrCreateOverlay(engine);
            if (panel == null) { onComplete?.Invoke(); yield break; }

            panel.gameObject.SetActive(true);

            float from = fadeOut ? 0f : 1f;
            float to = fadeOut ? 1f : 0f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                color.a = Mathf.Lerp(from, to, t / duration);
                panel.color = color;
                yield return null;
            }

            color.a = to;
            panel.color = color;

            if (!fadeOut)
                panel.gameObject.SetActive(false);

            onComplete?.Invoke();
        }
    }
}
