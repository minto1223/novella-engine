using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Novella.Core
{
    /// <summary>
    /// 条件式・計算式を評価する再帰下降パーサ。
    /// フラグ値はすべて文字列で保持されるため、評価結果も文字列で返す
    /// （真偽は "true"/"false"、数値は整数の文字列表現）。
    ///
    /// 使える構文:
    ///   括弧          ( a AND b ) OR c
    ///   論理          AND / OR / !   （&amp;&amp; / || / NOT も同義）
    ///   比較          == != &lt; &lt;= &gt; &gt;=
    ///   算術          + - * / %      （整数演算。単項マイナス可）
    ///   文字列        "alice" / 'alice'
    ///
    /// 識別子（クォートなしの語）は「フラグが定義済みならその値、未定義ならその語自身を文字列として扱う」。
    /// これにより route == alice のような従来からの書き方がそのまま動く。
    /// alice という名前のフラグを別途定義している場合だけ解釈が変わるため、
    /// 文字列として比較したいことが明らかな場合は "alice" とクォートするのが安全。
    /// </summary>
    public static class NovellaExpression
    {
        /// <summary>フラグ名から値を引く。未定義ならnullを返すこと。</summary>
        public delegate string FlagLookup(string name);

        // ---------------------------------------------------------------
        // 公開API
        // ---------------------------------------------------------------

        /// <summary>式を評価して文字列値を得る。構文エラー等で評価できなければfalse。</summary>
        public static bool TryEvaluate(string expression, FlagLookup lookup, out string value, out string error)
        {
            value = null;
            error = null;

            if (string.IsNullOrEmpty(expression))
            {
                error = "式が空です";
                return false;
            }

            try
            {
                var tokens = Tokenize(expression);
                var parser = new Parser(tokens, lookup, expression);
                value = parser.ParseAll();
                return true;
            }
            catch (ExpressionException e)
            {
                error = e.Message;
                return false;
            }
        }

        /// <summary>式を真偽として評価する。</summary>
        public static bool TryEvaluateBool(string expression, FlagLookup lookup, out bool result, out string error)
        {
            result = false;
            if (!TryEvaluate(expression, lookup, out string value, out error)) return false;
            result = IsTruthy(value);
            return true;
        }

        /// <summary>式を整数として評価する。</summary>
        public static bool TryEvaluateInt(string expression, FlagLookup lookup, out int result, out string error)
        {
            result = 0;
            if (!TryEvaluate(expression, lookup, out string value, out error)) return false;
            if (!int.TryParse(value, out result))
            {
                error = $"数値として解釈できません: \"{value}\"";
                return false;
            }
            return true;
        }

        /// <summary>Novella既定の真偽解釈。"true" / "1" / "yes" のみ真。</summary>
        public static bool IsTruthy(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return value == "true" || value == "1" || value == "yes";
        }

        /// <summary>数値として解釈する。解釈できなければ0（従来のGetIntと同じ扱い）。</summary>
        public static int ToInt(string value)
        {
            return int.TryParse(value, out int n) ? n : 0;
        }

        // ---------------------------------------------------------------
        // トークナイザ
        // ---------------------------------------------------------------

        private enum TokenType { Number, Str, Ident, Op, LParen, RParen, End }

        private struct Token
        {
            public TokenType Type;
            public string Text;

            public Token(TokenType type, string text)
            {
                Type = type;
                Text = text;
            }
        }

        private sealed class ExpressionException : Exception
        {
            public ExpressionException(string message) : base(message) { }
        }

        /// <summary>識別子（フラグ名・裸の文字列）を構成しうる文字か。日本語のフラグ名も通す。</summary>
        private static bool IsIdentChar(char c)
        {
            if (char.IsWhiteSpace(c)) return false;
            switch (c)
            {
                case '(': case ')':
                case '!': case '=': case '<': case '>':
                case '+': case '-': case '*': case '/': case '%':
                case '&': case '|':
                case '"': case '\'':
                    return false;
            }
            return true;
        }

        private static List<Token> Tokenize(string src)
        {
            var tokens = new List<Token>();
            int i = 0;

            while (i < src.Length)
            {
                char c = src[i];

                if (char.IsWhiteSpace(c)) { i++; continue; }

                if (c == '(') { tokens.Add(new Token(TokenType.LParen, "(")); i++; continue; }
                if (c == ')') { tokens.Add(new Token(TokenType.RParen, ")")); i++; continue; }

                // 文字列リテラル
                if (c == '"' || c == '\'')
                {
                    char quote = c;
                    i++;
                    var sb = new StringBuilder();
                    while (i < src.Length && src[i] != quote)
                    {
                        // \" \' \\ のみエスケープとして扱う
                        if (src[i] == '\\' && i + 1 < src.Length &&
                            (src[i + 1] == quote || src[i + 1] == '\\'))
                        {
                            sb.Append(src[i + 1]);
                            i += 2;
                            continue;
                        }
                        sb.Append(src[i]);
                        i++;
                    }
                    if (i >= src.Length) throw new ExpressionException("文字列リテラルが閉じられていません");
                    i++; // 閉じクォート
                    tokens.Add(new Token(TokenType.Str, sb.ToString()));
                    continue;
                }

                // 2文字演算子
                if (i + 1 < src.Length)
                {
                    string two = src.Substring(i, 2);
                    if (two == ">=" || two == "<=" || two == "==" || two == "!=" ||
                        two == "&&" || two == "||")
                    {
                        tokens.Add(new Token(TokenType.Op, two));
                        i += 2;
                        continue;
                    }
                }

                // 1文字演算子
                if (c == '<' || c == '>' || c == '!' ||
                    c == '+' || c == '-' || c == '*' || c == '/' || c == '%')
                {
                    tokens.Add(new Token(TokenType.Op, c.ToString()));
                    i++;
                    continue;
                }

                if (c == '=') throw new ExpressionException("'=' は比較に使えません（'==' を使ってください）");
                if (c == '&' || c == '|') throw new ExpressionException($"'{c}' 単体は使えません（'{c}{c}' を使ってください）");

                // 数値 or 識別子
                if (char.IsDigit(c))
                {
                    int start = i;
                    while (i < src.Length && char.IsDigit(src[i])) i++;
                    // 123abc のように数字で始まる識別子は識別子として読み直す
                    if (i < src.Length && IsIdentChar(src[i]))
                    {
                        while (i < src.Length && IsIdentChar(src[i])) i++;
                        tokens.Add(new Token(TokenType.Ident, src.Substring(start, i - start)));
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Number, src.Substring(start, i - start)));
                    }
                    continue;
                }

                if (IsIdentChar(c))
                {
                    int start = i;
                    while (i < src.Length && IsIdentChar(src[i])) i++;
                    tokens.Add(new Token(TokenType.Ident, src.Substring(start, i - start)));
                    continue;
                }

                throw new ExpressionException($"解釈できない文字です: '{c}'");
            }

            tokens.Add(new Token(TokenType.End, ""));
            return tokens;
        }

        // ---------------------------------------------------------------
        // パーサ（優先順位: OR < AND < 等値 < 比較 < 加減 < 乗除 < 単項）
        // ---------------------------------------------------------------

        private sealed class Parser
        {
            private readonly List<Token> _tokens;
            private readonly FlagLookup _lookup;
            private readonly string _source;
            private int _pos;

            public Parser(List<Token> tokens, FlagLookup lookup, string source)
            {
                _tokens = tokens;
                _lookup = lookup;
                _source = source;
            }

            public string ParseAll()
            {
                string value = ParseOr();
                if (Current.Type != TokenType.End)
                    throw new ExpressionException($"余分なトークンがあります: '{Current.Text}'");
                return value;
            }

            private Token Current => _tokens[_pos];

            /// <summary>現在のトークンが指定の演算子ならひとつ進めてtrue。</summary>
            private bool MatchOp(params string[] ops)
            {
                var t = Current;
                if (t.Type != TokenType.Op) return false;
                foreach (var op in ops)
                {
                    if (t.Text == op) { _pos++; return true; }
                }
                return false;
            }

            /// <summary>AND / OR / NOT はキーワード（識別子）として現れる。大文字のみ従来互換。</summary>
            private bool MatchKeyword(string keyword)
            {
                var t = Current;
                if (t.Type == TokenType.Ident && t.Text == keyword) { _pos++; return true; }
                return false;
            }

            private string ParseOr()
            {
                string left = ParseAnd();
                while (true)
                {
                    if (MatchKeyword("OR") || MatchOp("||"))
                    {
                        // 副作用のない式のため短絡はせず、両辺を評価する
                        string right = ParseAnd();
                        left = Bool(IsTruthy(left) || IsTruthy(right));
                    }
                    else return left;
                }
            }

            private string ParseAnd()
            {
                string left = ParseEquality();
                while (true)
                {
                    if (MatchKeyword("AND") || MatchOp("&&"))
                    {
                        string right = ParseEquality();
                        left = Bool(IsTruthy(left) && IsTruthy(right));
                    }
                    else return left;
                }
            }

            private string ParseEquality()
            {
                string left = ParseComparison();
                while (true)
                {
                    if (MatchOp("==")) left = Bool(AreEqual(left, ParseComparison()));
                    else if (MatchOp("!=")) left = Bool(!AreEqual(left, ParseComparison()));
                    else return left;
                }
            }

            private string ParseComparison()
            {
                string left = ParseAdditive();
                while (true)
                {
                    if (MatchOp(">=")) left = Bool(ToInt(left) >= ToInt(ParseAdditive()));
                    else if (MatchOp("<=")) left = Bool(ToInt(left) <= ToInt(ParseAdditive()));
                    else if (MatchOp(">")) left = Bool(ToInt(left) > ToInt(ParseAdditive()));
                    else if (MatchOp("<")) left = Bool(ToInt(left) < ToInt(ParseAdditive()));
                    else return left;
                }
            }

            private string ParseAdditive()
            {
                string left = ParseMultiplicative();
                while (true)
                {
                    if (MatchOp("+")) left = Num(ToInt(left) + ToInt(ParseMultiplicative()));
                    else if (MatchOp("-")) left = Num(ToInt(left) - ToInt(ParseMultiplicative()));
                    else return left;
                }
            }

            private string ParseMultiplicative()
            {
                string left = ParseUnary();
                while (true)
                {
                    if (MatchOp("*")) left = Num(ToInt(left) * ToInt(ParseUnary()));
                    else if (MatchOp("/")) left = Num(Divide(ToInt(left), ToInt(ParseUnary())));
                    else if (MatchOp("%")) left = Num(Modulo(ToInt(left), ToInt(ParseUnary())));
                    else return left;
                }
            }

            private string ParseUnary()
            {
                if (MatchOp("!") || MatchKeyword("NOT")) return Bool(!IsTruthy(ParseUnary()));
                if (MatchOp("-")) return Num(-ToInt(ParseUnary()));
                if (MatchOp("+")) return Num(ToInt(ParseUnary()));
                return ParsePrimary();
            }

            private string ParsePrimary()
            {
                var t = Current;

                switch (t.Type)
                {
                    case TokenType.Number:
                        _pos++;
                        return t.Text;

                    case TokenType.Str:
                        _pos++;
                        return t.Text;

                    case TokenType.Ident:
                        _pos++;
                        // 定義済みフラグならその値、未定義なら語そのものを文字列として扱う
                        return _lookup?.Invoke(t.Text) ?? t.Text;

                    case TokenType.LParen:
                    {
                        _pos++;
                        string inner = ParseOr();
                        if (Current.Type != TokenType.RParen)
                            throw new ExpressionException("')' が足りません");
                        _pos++;
                        return inner;
                    }

                    case TokenType.End:
                        throw new ExpressionException("式が途中で終わっています");

                    default:
                        throw new ExpressionException($"予期しないトークンです: '{t.Text}'");
                }
            }

            private int Divide(int a, int b)
            {
                if (b == 0)
                {
                    Debug.LogWarning($"[Novella] 式のゼロ除算を無視しました: \"{_source}\"");
                    return a;
                }
                return a / b;
            }

            private int Modulo(int a, int b)
            {
                if (b == 0)
                {
                    Debug.LogWarning($"[Novella] 式のゼロ剰余を無視しました: \"{_source}\"");
                    return a;
                }
                return a % b;
            }

            /// <summary>両辺が数値なら数値として、そうでなければ文字列として比較する。</summary>
            private static bool AreEqual(string a, string b)
            {
                if (int.TryParse(a, out int x) && int.TryParse(b, out int y)) return x == y;
                return string.Equals(a, b, StringComparison.Ordinal);
            }

            private static string Bool(bool value) => value ? "true" : "false";

            private static string Num(int value) => value.ToString();
        }
    }
}
