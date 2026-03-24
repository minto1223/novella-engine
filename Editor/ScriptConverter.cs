#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Novella.Core;
using UnityEditor;
using UnityEngine;

namespace Novella.Editor
{
    public class ScriptConverter
    {
        private static readonly string ScriptsPath = "Assets/Novella/Resources/Scripts";

        // CSVヘッダー（ScriptParserと同じカラム順）
        private static readonly string[] CsvHeaders =
            { "type", "character", "text", "image", "position", "expression", "duration", "label", "target", "clip", "volume", "value", "order" };

        [MenuItem("Novella/Convert CSV to JSON")]
        public static void CsvToJson()
        {
            string file = EditorUtility.OpenFilePanel("Select CSV Script", ScriptsPath, "csv");
            if (string.IsNullOrEmpty(file)) return;

            string csv = File.ReadAllText(file, Encoding.UTF8);
            var script = ScriptParser.ParseCsv(csv);
            if (script == null)
            {
                Debug.LogError("[Novella Converter] Failed to parse CSV.");
                return;
            }

            string json = JsonConvert.SerializeObject(script, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

            string outputPath = Path.ChangeExtension(file, ".json");
            if (File.Exists(outputPath))
            {
                if (!EditorUtility.DisplayDialog("上書き確認",
                    $"{Path.GetFileName(outputPath)} は既に存在します。上書きしますか？", "上書き", "キャンセル"))
                    return;
            }

            File.WriteAllText(outputPath, json, Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log($"[Novella Converter] CSV → JSON 変換完了: {outputPath}");
        }

        [MenuItem("Novella/Convert JSON to CSV")]
        public static void JsonToCsv()
        {
            string file = EditorUtility.OpenFilePanel("Select JSON Script", ScriptsPath, "json");
            if (string.IsNullOrEmpty(file)) return;

            string json = File.ReadAllText(file, Encoding.UTF8);
            NovellaScript script;
            try
            {
                script = JsonConvert.DeserializeObject<NovellaScript>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Novella Converter] JSON parse error: {e.Message}");
                return;
            }

            if (script == null || script.Commands == null)
            {
                Debug.LogError("[Novella Converter] Invalid script data.");
                return;
            }

            var sb = new StringBuilder();

            // タイトル行
            if (!string.IsNullOrEmpty(script.Title))
                sb.AppendLine($"#title,{CsvEscape(script.Title)}");

            // ヘッダー行
            sb.AppendLine(string.Join(",", CsvHeaders));

            foreach (var cmd in script.Commands)
            {
                // choice コマンドは各選択肢を個別の行に展開
                if (cmd.Type == "choice" && cmd.Choices != null)
                {
                    foreach (var choice in cmd.Choices)
                    {
                        string flagField = "";
                        if (!string.IsNullOrEmpty(choice.SetFlag))
                        {
                            flagField = choice.SetFlag;
                            if (!string.IsNullOrEmpty(choice.FlagValue))
                                flagField += "=" + choice.FlagValue;
                        }

                        var choiceFields = new string[CsvHeaders.Length];
                        choiceFields[0] = "choice";                           // type
                        choiceFields[2] = CsvEscape(choice.Text ?? "");       // text
                        choiceFields[8] = CsvEscape(choice.Target ?? "");     // target
                        choiceFields[11] = CsvEscape(flagField);              // value (flag=value)
                        // 空フィールドを埋める
                        for (int i = 0; i < choiceFields.Length; i++)
                            choiceFields[i] ??= "";
                        sb.AppendLine(string.Join(",", choiceFields));
                    }
                    continue;
                }

                var fields = new string[CsvHeaders.Length];
                fields[0]  = CsvEscape(cmd.Type ?? "");
                fields[1]  = CsvEscape(cmd.Character ?? "");
                fields[2]  = CsvEscape(cmd.Text ?? "");
                fields[3]  = CsvEscape(cmd.Image ?? "");
                fields[4]  = CsvEscape(cmd.Position ?? "");
                fields[5]  = CsvEscape(cmd.Expression ?? "");
                fields[6]  = cmd.Duration != 0f ? cmd.Duration.ToString("G") : "";
                fields[7]  = CsvEscape(cmd.Label ?? "");
                fields[8]  = CsvEscape(cmd.Target ?? "");
                fields[9]  = CsvEscape(cmd.Clip ?? "");
                fields[10] = cmd.Volume != 0f ? cmd.Volume.ToString("G") : "";
                fields[11] = CsvEscape(cmd.Value ?? "");
                fields[12] = cmd.Order >= 0 ? cmd.Order.ToString() : "";
                sb.AppendLine(string.Join(",", fields));
            }

            string outputPath = Path.ChangeExtension(file, ".csv");
            if (File.Exists(outputPath))
            {
                if (!EditorUtility.DisplayDialog("上書き確認",
                    $"{Path.GetFileName(outputPath)} は既に存在します。上書きしますか？", "上書き", "キャンセル"))
                    return;
            }

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log($"[Novella Converter] JSON → CSV 変換完了: {outputPath}");
        }

        private static string CsvEscape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}
#endif
