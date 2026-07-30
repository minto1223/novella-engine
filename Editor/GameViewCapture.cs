using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NovellaEditor
{
    /// <summary>
    /// Screen Space - Overlay のCanvasを一時カメラでRenderTextureに描画してPNG保存する
    /// MCP作業用ユーティリティ。Game Viewの再描画タイミングに依存しない。
    /// </summary>
    public static class GameViewCapture
    {
        private const int Width = 1920;
        private const int Height = 1080;

        [MenuItem("Tools/Novella/Capture Game View")]
        public static void Capture()
        {
            var dir = Path.Combine(Application.dataPath, "Screenshots");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "gameview.png");

            var camGo = new GameObject("~CaptureCamera") { hideFlags = HideFlags.HideAndDontSave };
            var cam = camGo.AddComponent<Camera>();
            cam.backgroundColor = Color.white;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.cullingMask = ~0;
            cam.orthographic = true;

            var urpData = camGo.AddComponent<UniversalAdditionalCameraData>();
            urpData.renderType = CameraRenderType.Base;

            var rt = RenderTexture.GetTemporary(Width, Height, 24, RenderTextureFormat.ARGB32);
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var restore = new System.Collections.Generic.List<(Canvas c, RenderMode m, Camera cam, float pd)>();

            try
            {
                foreach (var c in canvases)
                {
                    if (!c.isRootCanvas || c.renderMode == RenderMode.WorldSpace) continue;
                    restore.Add((c, c.renderMode, c.worldCamera, c.planeDistance));
                    c.renderMode = RenderMode.ScreenSpaceCamera;
                    c.worldCamera = cam;
                    c.planeDistance = 10f;
                }

                Canvas.ForceUpdateCanvases();
                cam.targetTexture = rt;
                cam.Render();

                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                tex.Apply();
                RenderTexture.active = prev;

                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                Debug.Log($"[GameViewCapture] Saved {path}");
            }
            finally
            {
                foreach (var (c, m, wc, pd) in restore)
                {
                    c.renderMode = m;
                    c.worldCamera = wc;
                    c.planeDistance = pd;
                }
                cam.targetTexture = null;
                RenderTexture.ReleaseTemporary(rt);
                Object.DestroyImmediate(camGo);
            }
        }
    }
}
