using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Novella.Core
{
    public static class ScriptParser
    {
        public static NovellaScript ParseJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<NovellaScript>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Novella] JSON parse error: {e.Message}");
                return null;
            }
        }

        public static NovellaScript ParseCsv(string csv)
        {
            try
            {
                var script = new NovellaScript();
                var lines = ParseCsvLines(csv);
                // choice行をまとめるバッファ
                List<ChoiceOption> pendingChoices = null;

                foreach (var fields in lines)
                {
                    if (fields.Count == 0) continue;

                    // タイトル行: #title,タイトル名
                    if (fields[0].StartsWith("#title"))
                    {
                        script.Title = fields.Count > 1 ? fields[1] : "";
                        continue;
                    }
                    // コメント行
                    if (fields[0].StartsWith("#") || fields[0].StartsWith("//"))
                        continue;
                    // ヘッダー行スキップ
                    if (fields[0] == "type") continue;

                    string type = Get(fields, 0).Trim();
                    if (string.IsNullOrEmpty(type)) continue;

                    // choice行: 連続した choice 行を1つの choice コマンドにまとめる
                    if (type == "choice")
                    {
                        if (pendingChoices == null)
                            pendingChoices = new List<ChoiceOption>();

                        var option = new ChoiceOption
                        {
                            Text = Get(fields, 2),    // text列
                            Target = Get(fields, 8),   // target列
                        };
                        // value列に "flag=value" 形式
                        string flagField = Get(fields, 11);
                        if (!string.IsNullOrEmpty(flagField) && flagField.Contains("="))
                        {
                            var parts = flagField.Split('=');
                            option.SetFlag = parts[0].Trim();
                            option.FlagValue = parts.Length > 1 ? parts[1].Trim() : "true";
                        }
                        pendingChoices.Add(option);
                        continue;
                    }

                    // choice以外の行が来たら、溜まったchoiceをフラッシュ
                    FlushChoices(script, ref pendingChoices);

                    var cmd = new ScriptCommand { Type = type };
                    string character = Get(fields, 1);
                    string text      = Get(fields, 2);
                    string image     = Get(fields, 3);
                    string position  = Get(fields, 4);
                    string expression = Get(fields, 5);
                    string duration  = Get(fields, 6);
                    string label     = Get(fields, 7);
                    string target    = Get(fields, 8);
                    string clip      = Get(fields, 9);
                    string volume    = Get(fields, 10);
                    string value     = Get(fields, 11);
                    string order     = Get(fields, 12);

                    if (!string.IsNullOrEmpty(character))  cmd.Character  = character;
                    if (!string.IsNullOrEmpty(text))       cmd.Text       = text;
                    if (!string.IsNullOrEmpty(image))      cmd.Image      = image;
                    if (!string.IsNullOrEmpty(position))   cmd.Position   = position;
                    if (!string.IsNullOrEmpty(expression)) cmd.Expression = expression;
                    if (!string.IsNullOrEmpty(label))      cmd.Label      = label;
                    if (!string.IsNullOrEmpty(target))     cmd.Target     = target;
                    if (!string.IsNullOrEmpty(clip))       cmd.Clip       = clip;
                    if (!string.IsNullOrEmpty(value))      cmd.Value      = value;

                    if (!string.IsNullOrEmpty(duration) && float.TryParse(duration, out float d))
                        cmd.Duration = d;
                    if (!string.IsNullOrEmpty(volume) && float.TryParse(volume, out float v))
                        cmd.Volume = v;
                    if (!string.IsNullOrEmpty(order) && int.TryParse(order, out int o))
                        cmd.Order = o;

                    script.Commands.Add(cmd);
                }

                // 末尾にchoiceが残っている場合
                FlushChoices(script, ref pendingChoices);

                return script;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Novella] CSV parse error: {e.Message}");
                return null;
            }
        }

        private static void FlushChoices(NovellaScript script, ref List<ChoiceOption> pending)
        {
            if (pending == null || pending.Count == 0) return;
            var cmd = new ScriptCommand
            {
                Type = "choice",
                Choices = new List<ChoiceOption>(pending)
            };
            script.Commands.Add(cmd);
            pending = null;
        }

        /// <summary>
        /// RFC 4180準拠のCSVパーサー。ダブルクォート内のカンマ・改行に対応。
        /// </summary>
        private static List<List<string>> ParseCsvLines(string csv)
        {
            var result = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();
            bool inQuotes = false;
            int i = 0;

            while (i < csv.Length)
            {
                char c = csv[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < csv.Length && csv[i + 1] == '"')
                        {
                            field.Append('"');
                            i += 2;
                        }
                        else
                        {
                            inQuotes = false;
                            i++;
                        }
                    }
                    else
                    {
                        field.Append(c);
                        i++;
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                        i++;
                    }
                    else if (c == ',')
                    {
                        row.Add(field.ToString());
                        field.Clear();
                        i++;
                    }
                    else if (c == '\r' || c == '\n')
                    {
                        row.Add(field.ToString());
                        field.Clear();
                        if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n')
                            i++;
                        i++;
                        if (row.Count > 0 && !(row.Count == 1 && string.IsNullOrEmpty(row[0])))
                            result.Add(row);
                        row = new List<string>();
                    }
                    else
                    {
                        field.Append(c);
                        i++;
                    }
                }
            }

            // 最終行
            row.Add(field.ToString());
            if (row.Count > 0 && !(row.Count == 1 && string.IsNullOrEmpty(row[0])))
                result.Add(row);

            return result;
        }

        private static string Get(List<string> fields, int index)
        {
            return index < fields.Count ? fields[index].Trim() : "";
        }

        /// <summary>
        /// リソースからスクリプトを読み込む。JSON/CSV自動判定。
        /// </summary>
        public static NovellaScript LoadFromResources(string resourcePath)
        {
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
            {
                Debug.LogError($"[Novella] Script not found at Resources/{resourcePath}");
                return null;
            }
            return Parse(asset.text);
        }

        /// <summary>
        /// テキスト内容からJSON/CSVを自動判定してパースする。
        /// </summary>
        public static NovellaScript Parse(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            string trimmed = text.TrimStart();
            // JSONは { で始まる
            if (trimmed.StartsWith("{"))
                return ParseJson(text);
            return ParseCsv(text);
        }
    }
}
