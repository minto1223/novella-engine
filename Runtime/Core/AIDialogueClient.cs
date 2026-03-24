using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Novella.Core
{
    /// <summary>
    /// Claude APIを呼び出してセリフテキストを生成するクライアント。
    /// MonoBehaviour上でStartCoroutine(AIDialogueClient.Generate(...))として使用。
    /// </summary>
    public static class AIDialogueClient
    {
        private const string ApiUrl = "https://api.anthropic.com/v1/messages";
        private const string Model = "claude-haiku-4-5-20251001";
        private const int MaxTokens = 256;

        /// <summary>
        /// promptをClaude APIに送信し、生成されたテキストをonResultで返すコルーチン。
        /// APIキー未設定またはエラー時はフォールバックテキストを返す。
        /// </summary>
        public static IEnumerator Generate(
            string prompt,
            string apiKey,
            Action<string> onResult,
            Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogWarning("[Novella/AI] APIキーが設定されていません。NovellaEngineのAIApiKeyを設定してください。");
                onResult?.Invoke("（APIキーが未設定です）");
                yield break;
            }

            var body = new
            {
                model = Model,
                max_tokens = MaxTokens,
                messages = new[] { new { role = "user", content = prompt } }
            };

            string jsonBody = JsonConvert.SerializeObject(body);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonBody);

            using var req = new UnityWebRequest(ApiUrl, "POST");
            req.uploadHandler = new UploadHandlerRaw(bytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("x-api-key", apiKey);
            req.SetRequestHeader("anthropic-version", "2023-06-01");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                string err = $"HTTP {req.responseCode}: {req.error}";
                Debug.LogError($"[Novella/AI] {err}");
                onError?.Invoke(err);
                onResult?.Invoke("（AIの応答を取得できませんでした）");
                yield break;
            }

            try
            {
                var jobj = JObject.Parse(req.downloadHandler.text);
                string text = jobj["content"]?[0]?["text"]?.ToString() ?? "（応答なし）";
                onResult?.Invoke(text.Trim());
            }
            catch (Exception e)
            {
                Debug.LogError($"[Novella/AI] レスポンスの解析に失敗: {e.Message}");
                onResult?.Invoke("（AIの応答を解析できませんでした）");
            }
        }
    }
}
