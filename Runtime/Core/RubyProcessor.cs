using System.Text;
using System.Text.RegularExpressions;

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
        /// </summary>
        public static string Convert(string text, float fontSize = 46f)
        {
            if (string.IsNullOrEmpty(text) || !RubyRegex.IsMatch(text))
                return text;

            float rubySize = fontSize * 0.5f;
            float voffset = fontSize * 0.65f;

            return RubyRegex.Replace(text, m =>
            {
                string baseText = m.Groups[1].Value;
                string rubyText = m.Groups[2].Value;
                float rubyWidth = rubyText.Length * rubySize * 0.95f;

                return $"<size={rubySize:F0}><voffset={voffset:F0}>{rubyText}</voffset></size><space={-rubyWidth:F0}>{baseText}";
            });
        }
    }
}
