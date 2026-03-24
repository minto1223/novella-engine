using System.Text.RegularExpressions;

namespace Novella.Core
{
    /// <summary>
    /// カスタムリッチテキストタグをTMProリッチテキストに変換するプリプロセッサ。
    /// {b}太字{/b} → <b>太字</b>
    /// {i}斜体{/i} → <i>斜体</i>
    /// {c:red}赤文字{/c} → <color=red>赤文字</color>
    /// {size:60}大きい{/size} → <size=60>大きい</size>
    /// </summary>
    public static class RichTextProcessor
    {
        private static readonly Regex BoldOpen = new Regex(@"\{b\}", RegexOptions.Compiled);
        private static readonly Regex BoldClose = new Regex(@"\{/b\}", RegexOptions.Compiled);
        private static readonly Regex ItalicOpen = new Regex(@"\{i\}", RegexOptions.Compiled);
        private static readonly Regex ItalicClose = new Regex(@"\{/i\}", RegexOptions.Compiled);
        private static readonly Regex ColorOpen = new Regex(@"\{c:([^}]+)\}", RegexOptions.Compiled);
        private static readonly Regex ColorClose = new Regex(@"\{/c\}", RegexOptions.Compiled);
        private static readonly Regex SizeOpen = new Regex(@"\{size:([^}]+)\}", RegexOptions.Compiled);
        private static readonly Regex SizeClose = new Regex(@"\{/size\}", RegexOptions.Compiled);

        public static string Convert(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            text = BoldOpen.Replace(text, "<b>");
            text = BoldClose.Replace(text, "</b>");
            text = ItalicOpen.Replace(text, "<i>");
            text = ItalicClose.Replace(text, "</i>");
            text = ColorOpen.Replace(text, "<color=$1>");
            text = ColorClose.Replace(text, "</color>");
            text = SizeOpen.Replace(text, "<size=$1>");
            text = SizeClose.Replace(text, "</size>");

            return text;
        }
    }
}
