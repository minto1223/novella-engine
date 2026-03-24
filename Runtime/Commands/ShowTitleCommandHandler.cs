using System;
using System.Collections;
using Novella.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.Commands
{
    /// <summary>
    /// 章タイトルを画面中央にフェード表示する。
    /// JSON: { "type": "show_title", "text": "第1章：はじまりの日", "duration": 3.0 }
    /// CSV:  show_title,,第1章：はじまりの日,,,,3.0,,,,,
    /// duration: 表示時間（デフォルト3秒、フェードイン0.5秒+表示+フェードアウト0.5秒）
    /// </summary>
    public class ShowTitleCommandHandler : ICommandHandler
    {
        public string CommandType => "show_title";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            string text = engine.ResolveText(command.Text ?? "");
            float totalDuration = command.Duration > 0f ? command.Duration : 3f;

            engine.StartCoroutine(DoShowTitle(engine, text, totalDuration, onComplete));
        }

        private IEnumerator DoShowTitle(NovellaEngine engine, string text, float totalDuration, Action onComplete)
        {
            Canvas canvas = null;
            if (engine.Background != null)
                canvas = engine.Background.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                var canvasGo = GameObject.Find("NovellaCanvas");
                if (canvasGo != null) canvas = canvasGo.GetComponent<Canvas>();
            }
            if (canvas == null) { onComplete?.Invoke(); yield break; }

            // タイトル用GameObjectを作成
            var go = new GameObject("ChapterTitle");
            go.transform.SetParent(canvas.transform, false);
            go.transform.SetAsLastSibling();

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 64;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
            tmp.color = new Color(1f, 1f, 1f, 0f);
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;

            // フォント設定（シーン内の既存TMPからフォントを取得）
            var existingTmp = canvas.GetComponentInChildren<TMP_Text>(true);
            if (existingTmp != null && existingTmp.font != null)
                tmp.font = existingTmp.font;

            float fadeTime = 0.5f;
            float holdTime = Mathf.Max(0f, totalDuration - fadeTime * 2);

            // フェードイン
            yield return FadeText(tmp, 0f, 1f, fadeTime);
            // 表示維持
            yield return new WaitForSeconds(holdTime);
            // フェードアウト
            yield return FadeText(tmp, 1f, 0f, fadeTime);

            UnityEngine.Object.Destroy(go);
            onComplete?.Invoke();
        }

        private IEnumerator FadeText(TMP_Text tmp, float from, float to, float duration)
        {
            float t = 0f;
            var c = tmp.color;
            while (t < duration)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(from, to, t / duration);
                tmp.color = c;
                yield return null;
            }
            c.a = to;
            tmp.color = c;
        }
    }
}
