using System.IO;
using UnityEditor;
using UnityEngine;

namespace NovellaEditor
{
    /// <summary>
    /// テーマ用のグラデーションPNG（アルファ付き）を生成するMCP作業用ユーティリティ。
    /// </summary>
    public static class ThemeTextureGenerator
    {
        private const string Dir = "Assets/Novella/UI/Sprites/Theme";

        [MenuItem("Tools/Novella/Generate Theme Gradients")]
        public static void Generate()
        {
            // 左スクリム: 白を左(α0.92)→34%(α0.75)→62%(α0)で右へフェード
            var scrim = new Texture2D(512, 4, TextureFormat.RGBA32, false);
            var baseCol = new Color(240f / 255f, 248f / 255f, 253f / 255f);
            for (int x = 0; x < 512; x++)
            {
                float t = x / 511f;
                float a = t < 0.34f / 0.62f
                    ? Mathf.Lerp(0.92f, 0.75f, t / (0.34f / 0.62f))
                    : Mathf.Lerp(0.75f, 0f, (t - 0.34f / 0.62f) / (1f - 0.34f / 0.62f));
                for (int y = 0; y < 4; y++)
                    scrim.SetPixel(x, y, new Color(baseCol.r, baseCol.g, baseCol.b, a));
            }
            Save(scrim, "scrim_left_white.png");

            // ベース背景: 淡い160度グラデ (E7F2FA→DCECF7→E9F0F6)
            var bg = new Texture2D(64, 512, TextureFormat.RGBA32, false);
            Color c0 = Hex("E7F2FA"), c1 = Hex("DCECF7"), c2 = Hex("E9F0F6");
            for (int y = 0; y < 512; y++)
            {
                float t = 1f - y / 511f; // 上→下
                var c = t < 0.5f ? Color.Lerp(c0, c1, t * 2f) : Color.Lerp(c1, c2, (t - 0.5f) * 2f);
                for (int x = 0; x < 64; x++)
                    bg.SetPixel(x, y, c);
            }
            Save(bg, "bg_sky_gradient.png");

            AssetDatabase.Refresh();
            foreach (var name in new[] { "scrim_left_white.png", "bg_sky_gradient.png" })
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath($"{Dir}/{name}");
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
            Debug.Log("[ThemeTextureGenerator] Generated gradients.");
        }

        private static void Save(Texture2D tex, string name)
        {
            tex.Apply();
            File.WriteAllBytes(Path.Combine(Dir, name), tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }
    }
}
