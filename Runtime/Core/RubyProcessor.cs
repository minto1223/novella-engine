using System.Text;
using System.Text.RegularExpressions;
using TMPro;

namespace Novella.Core
{
    /// <summary>
    /// ルビタグ {rb:漢字,かんじ} をTMProリッチテキストに変換するユーティリティ。
    /// </summary>
    public static class RubyProcessor
    {
        private static readonly Regex RubyRegex =
            new Regex(@"\{rb:([^,}]+),([^}]+)\}", RegexOptions.Compiled);

        /// <summary>ルビタグを含むか判定</summary>
        public static bool HasRuby(string text)
        {
            return !string.IsNullOrEmpty(text) && RubyRegex.IsMatch(text);
        }

        /// <summary>
        /// ルビタグをTMProリッチテキストに変換する。
        /// fontSize: TMPのフォントサイズ（ルビの位置・サイズ計算に使用）
        /// font: 実際に使用するフォントアセット（文字ごとの実幅を測るため。nullなら概算にフォールバック）
        /// </summary>
        public static string Convert(string text, float fontSize = 46f, TMP_FontAsset font = null)
        {
            if (string.IsNullOrEmpty(text) || !RubyRegex.IsMatch(text))
                return text;

            float rubySize = fontSize * 0.5f;
            float voffset = fontSize * 1.0f;

            return RubyRegex.Replace(text, m =>
            {
                string baseText = m.Groups[1].Value;
                string rubyText = m.Groups[2].Value;

                float baseWidth = MeasureWidth(baseText, fontSize, font);
                float rubyWidth = MeasureWidth(rubyText, rubySize, font);

                string rubyTag = $"<size={rubySize:F0}><voffset={voffset:F0}>{rubyText}</voffset></size>";

                if (rubyWidth <= baseWidth)
                {
                    // ルビの方が短い（または同じ）：ベース文字の上に中央揃えで乗せる
                    float leadIndent = (baseWidth - rubyWidth) / 2f;
                    float pullBack = rubyWidth + leadIndent;
                    return $"<space={leadIndent:F1}>{rubyTag}<space={-pullBack:F1}>{baseText}";
                }
                else
                {
                    // ルビの方が長い：ベース文字側に前後の余白を足してルビ幅に合わせる
                    float pad = (rubyWidth - baseWidth) / 2f;
                    return $"{rubyTag}<space={-rubyWidth:F1}><space={pad:F1}>{baseText}<space={pad:F1}>";
                }
            });
        }

        /// <summary>
        /// 文字列の描画幅を概算する。フォントアセットの字形情報があれば実測、なければヒューリスティック。
        /// </summary>
        private static float MeasureWidth(string text, float fontSize, TMP_FontAsset font)
        {
            if (string.IsNullOrEmpty(text))
                return 0f;

            if (font == null || font.faceInfo.pointSize <= 0)
                return text.Length * fontSize * 0.95f;

            float scale = fontSize / font.faceInfo.pointSize;
            float width = 0f;
            foreach (char c in text)
            {
                if (font.characterLookupTable.TryGetValue(c, out var ch))
                    width += ch.glyph.metrics.horizontalAdvance * scale;
                else
                    width += fontSize * 0.95f;
            }
            return width;
        }
    }
}
