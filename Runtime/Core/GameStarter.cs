using UnityEngine;

namespace Novella.Core
{
    public class GameStarter : MonoBehaviour
    {
        [SerializeField] private NovellaEngine _engine;

        [Header("Start Script")]
        [Tooltip("Resources/Scripts/ 以下のパス（拡張子不要）。ScriptAsset指定時はそちらが優先されます")]
        [SerializeField] private string _startScript = "Scripts/chapter01";

        [Tooltip("スクリプトファイルを直接指定（JSON/CSV自動判定）。設定するとパス指定より優先されます")]
        [SerializeField] private TextAsset _scriptAsset;

        private void Start()
        {
            if (_engine == null)
                _engine = FindFirstObjectByType<NovellaEngine>();

            if (_engine == null)
            {
                Debug.LogError("[Novella] NovellaEngine not found.");
                return;
            }

            // Editor Preview Mode
#if UNITY_EDITOR
            string previewScript = UnityEditor.EditorPrefs.GetString("Novella_PreviewScript", "");
            if (!string.IsNullOrEmpty(previewScript))
            {
                int previewIndex = UnityEditor.EditorPrefs.GetInt("Novella_PreviewIndex", 0);
                UnityEditor.EditorPrefs.DeleteKey("Novella_PreviewScript");
                UnityEditor.EditorPrefs.DeleteKey("Novella_PreviewIndex");
                _engine.LoadAndPlayFrom(previewScript, previewIndex);
                return;
            }
#endif

            string mode = PlayerPrefs.GetString("novella_start_mode", "new");

            if (mode == "quickload")
            {
                PlayerPrefs.DeleteKey("novella_start_mode");
                var saveManager = new SaveManager();
                saveManager.QuickLoad(_engine);
            }
            else if (mode == "autoload")
            {
                PlayerPrefs.DeleteKey("novella_start_mode");
                var saveManager = new SaveManager();
                saveManager.AutoLoad(_engine);
            }
            else if (mode == "recollection")
            {
                PlayerPrefs.DeleteKey("novella_start_mode");
                string scriptPath = PlayerPrefs.GetString("novella_recollection_script", "");
                string startLabel = PlayerPrefs.GetString("novella_recollection_start", "");
                string endLabel = PlayerPrefs.GetString("novella_recollection_end", "");
                PlayerPrefs.DeleteKey("novella_recollection_script");
                PlayerPrefs.DeleteKey("novella_recollection_start");
                PlayerPrefs.DeleteKey("novella_recollection_end");
                if (!string.IsNullOrEmpty(scriptPath))
                    _engine.PlayRecollection(scriptPath, startLabel, endLabel);
                else
                    Debug.LogError("[Novella] recollection: script path not set.");
            }
            else if (mode == "chapter_select")
            {
                PlayerPrefs.DeleteKey("novella_start_mode");
                string scriptPath = PlayerPrefs.GetString("novella_chapter_script", "");
                string jumpLabel = PlayerPrefs.GetString("novella_chapter_label", "");
                PlayerPrefs.DeleteKey("novella_chapter_script");
                PlayerPrefs.DeleteKey("novella_chapter_label");
                if (!string.IsNullOrEmpty(scriptPath))
                {
                    if (!string.IsNullOrEmpty(jumpLabel))
                    {
                        // ラベル位置を検索してジャンプ
                        int labelIndex = FindLabelIndex(scriptPath, jumpLabel);
                        if (labelIndex >= 0)
                            _engine.LoadAndPlayFrom(scriptPath, labelIndex);
                        else
                            _engine.LoadAndPlay(scriptPath);
                    }
                    else
                    {
                        _engine.LoadAndPlay(scriptPath);
                    }
                }
                else
                    Debug.LogError("[Novella] chapter_select: script path not set.");
            }
            else if (mode == "load")
            {
                int slot = PlayerPrefs.GetInt("novella_load_slot", 0);
                PlayerPrefs.DeleteKey("novella_start_mode");
                var saveManager = new SaveManager();
                saveManager.Load(slot, _engine);
            }
            else
            {
                PlayerPrefs.DeleteKey("novella_start_mode");

                // ScriptAsset が指定されていればそちらを使用（JSON/CSV自動判定）
                var overrideAsset = LoadScriptAssetFromPrefs();
                if (overrideAsset != null)
                {
                    var script = ScriptParser.Parse(overrideAsset.text);
                    if (script != null)
                    {
                        _engine.LoadAndPlayDirect(script, overrideAsset.name);
                        return;
                    }
                }

                if (_scriptAsset != null)
                {
                    var script = ScriptParser.Parse(_scriptAsset.text);
                    if (script != null)
                    {
                        _engine.LoadAndPlayDirect(script, _scriptAsset.name);
                        return;
                    }
                }

                string path = PlayerPrefs.GetString("novella_first_script", _startScript);
                _engine.LoadAndPlay(path);
            }
        }

        private TextAsset LoadScriptAssetFromPrefs()
        {
            string path = PlayerPrefs.GetString("novella_first_script", "");
            PlayerPrefs.DeleteKey("novella_first_script");
            if (string.IsNullOrEmpty(path)) return null;
            return Resources.Load<TextAsset>(path);
        }

        private int FindLabelIndex(string resourcePath, string label)
        {
            var script = ScriptParser.LoadFromResources(resourcePath);
            if (script?.Commands == null) return -1;
            for (int i = 0; i < script.Commands.Count; i++)
            {
                if (script.Commands[i].Type == "label" && script.Commands[i].Label == label)
                    return i;
            }
            return -1;
        }
    }
}
