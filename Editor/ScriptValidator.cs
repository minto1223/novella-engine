#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Novella.Editor
{
    public class ScriptValidator
    {
        private static readonly HashSet<string> ValidCommandTypes = new HashSet<string>
        {
            "say", "show_bg", "show_char", "hide_char", "wait", "end",
            "play_bgm", "stop_bgm", "play_se", "choice", "set_flag", "add_flag",
            "label", "jump", "jump_if", "jump_unless", "ai_say", "next_script",
            "shake", "flash", "fade", "show_title", "play_voice", "stop_voice",
            "move_char", "zoom", "pan", "reset_camera", "input_text",
            "play_particle", "stop_particle", "set_language",
            "play_movie", "stop_movie", "calc", "set_mode", "clear"
        };

        [MenuItem("Novella/Validate Scripts")]
        public static void ValidateAll()
        {
            string basePath = "Assets/Novella/Resources/Scripts";
            if (!Directory.Exists(basePath))
            {
                Debug.LogWarning("[Novella Validator] Scripts folder not found: " + basePath);
                return;
            }

            var jsonFiles = Directory.GetFiles(basePath, "*.json", SearchOption.AllDirectories);
            int totalErrors = 0;
            int totalWarnings = 0;

            foreach (var file in jsonFiles)
            {
                var (errors, warnings) = ValidateFile(file);
                totalErrors += errors;
                totalWarnings += warnings;
            }

            if (totalErrors == 0 && totalWarnings == 0)
                Debug.Log($"[Novella Validator] All {jsonFiles.Length} scripts are valid.");
            else
                Debug.Log($"[Novella Validator] Checked {jsonFiles.Length} scripts: {totalErrors} error(s), {totalWarnings} warning(s).");
        }

        private static (int errors, int warnings) ValidateFile(string filePath)
        {
            int errors = 0;
            int warnings = 0;
            string fileName = Path.GetFileName(filePath);

            string json;
            try
            {
                json = File.ReadAllText(filePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Novella Validator] {fileName}: Failed to read file: {ex.Message}");
                return (1, 0);
            }

            Core.NovellaScript script;
            try
            {
                script = JsonConvert.DeserializeObject<Core.NovellaScript>(json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Novella Validator] {fileName}: JSON parse error: {ex.Message}");
                return (1, 0);
            }

            if (script?.Commands == null || script.Commands.Count == 0)
            {
                Debug.LogWarning($"[Novella Validator] {fileName}: No commands found.");
                return (0, 1);
            }

            // ラベル収集
            var definedLabels = new HashSet<string>();
            var referencedLabels = new HashSet<string>();

            for (int i = 0; i < script.Commands.Count; i++)
            {
                var cmd = script.Commands[i];
                int line = i + 1;

                // コマンドタイプチェック
                if (string.IsNullOrEmpty(cmd.Type))
                {
                    Debug.LogError($"[Novella Validator] {fileName}:{line} - Command type is empty.");
                    errors++;
                    continue;
                }
                if (!ValidCommandTypes.Contains(cmd.Type))
                {
                    Debug.LogError($"[Novella Validator] {fileName}:{line} - Unknown command type: \"{cmd.Type}\"");
                    errors++;
                }

                // ラベル定義
                if (cmd.Type == "label" && !string.IsNullOrEmpty(cmd.Label))
                {
                    if (!definedLabels.Add(cmd.Label))
                    {
                        Debug.LogWarning($"[Novella Validator] {fileName}:{line} - Duplicate label: \"{cmd.Label}\"");
                        warnings++;
                    }
                }

                // ラベル参照チェック
                if ((cmd.Type == "jump" || cmd.Type == "jump_if" || cmd.Type == "jump_unless")
                    && !string.IsNullOrEmpty(cmd.Target))
                {
                    referencedLabels.Add(cmd.Target);
                }

                // choice の target チェック
                if (cmd.Type == "choice" && cmd.Choices != null)
                {
                    foreach (var choice in cmd.Choices)
                    {
                        if (!string.IsNullOrEmpty(choice.Target))
                            referencedLabels.Add(choice.Target);
                    }
                }

                // アセット参照チェック
                if (cmd.Type == "show_bg" && !string.IsNullOrEmpty(cmd.Image))
                {
                    if (Resources.Load<Sprite>($"Backgrounds/{cmd.Image}") == null)
                    {
                        Debug.LogWarning($"[Novella Validator] {fileName}:{line} - Background not found: Backgrounds/{cmd.Image}");
                        warnings++;
                    }
                }
                if (cmd.Type == "show_char" && !string.IsNullOrEmpty(cmd.Character))
                {
                    string spritePath = string.IsNullOrEmpty(cmd.Expression)
                        ? $"Characters/{cmd.Character}"
                        : $"Characters/{cmd.Character}_{cmd.Expression}";
                    if (Resources.Load<Sprite>(spritePath) == null
                        && Resources.Load<Sprite>($"Characters/{cmd.Character}") == null)
                    {
                        Debug.LogWarning($"[Novella Validator] {fileName}:{line} - Character sprite not found: {spritePath}");
                        warnings++;
                    }
                }
                if (cmd.Type == "play_bgm")
                {
                    string clipName = cmd.Clip ?? cmd.Image;
                    if (!string.IsNullOrEmpty(clipName) && Resources.Load<AudioClip>($"Audio/BGM/{clipName}") == null)
                    {
                        Debug.LogWarning($"[Novella Validator] {fileName}:{line} - BGM not found: Audio/BGM/{clipName}");
                        warnings++;
                    }
                }
                if (cmd.Type == "play_se")
                {
                    string clipName = cmd.Clip ?? cmd.Image;
                    if (!string.IsNullOrEmpty(clipName) && Resources.Load<AudioClip>($"Audio/SE/{clipName}") == null)
                    {
                        Debug.LogWarning($"[Novella Validator] {fileName}:{line} - SE not found: Audio/SE/{clipName}");
                        warnings++;
                    }
                }
                if (cmd.Type == "play_voice")
                {
                    string clipName = cmd.Clip ?? cmd.Image;
                    if (!string.IsNullOrEmpty(clipName) && Resources.Load<AudioClip>($"Audio/Voice/{clipName}") == null)
                    {
                        Debug.LogWarning($"[Novella Validator] {fileName}:{line} - Voice not found: Audio/Voice/{clipName}");
                        warnings++;
                    }
                }
            }

            // 未定義ラベルへの参照
            foreach (var label in referencedLabels)
            {
                if (!definedLabels.Contains(label))
                {
                    Debug.LogError($"[Novella Validator] {fileName}: Jump to undefined label: \"{label}\"");
                    errors++;
                }
            }

            // 未使用ラベル
            foreach (var label in definedLabels)
            {
                if (!referencedLabels.Contains(label))
                {
                    Debug.LogWarning($"[Novella Validator] {fileName}: Unused label: \"{label}\"");
                    warnings++;
                }
            }

            return (errors, warnings);
        }
    }
}
#endif
