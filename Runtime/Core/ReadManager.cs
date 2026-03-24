using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Novella.Core
{
    /// <summary>
    /// コマンドの既読状態を管理する。
    /// スクリプトパス + コマンドインデックスの組み合わせで既読を記録。
    /// </summary>
    public static class ReadManager
    {
        private const string PrefsKey = "novella_read_commands";
        private static HashSet<string> _readSet;

        private static void EnsureLoaded()
        {
            if (_readSet != null) return;
            string json = PlayerPrefs.GetString(PrefsKey, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var list = JsonConvert.DeserializeObject<List<string>>(json);
                    _readSet = list != null ? new HashSet<string>(list) : new HashSet<string>();
                }
                catch
                {
                    _readSet = new HashSet<string>();
                }
            }
            else
            {
                _readSet = new HashSet<string>();
            }
        }

        /// <summary>
        /// 指定コマンドを既読としてマークする。
        /// </summary>
        public static void MarkRead(string scriptPath, int commandIndex)
        {
            EnsureLoaded();
            string key = $"{scriptPath}:{commandIndex}";
            if (_readSet.Add(key))
                Save();
        }

        /// <summary>
        /// 指定コマンドが既読かどうか。
        /// </summary>
        public static bool IsRead(string scriptPath, int commandIndex)
        {
            EnsureLoaded();
            return _readSet.Contains($"{scriptPath}:{commandIndex}");
        }

        /// <summary>
        /// 既読データをクリアする。
        /// </summary>
        public static void ClearAll()
        {
            _readSet = new HashSet<string>();
            PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.Save();
        }

        private static void Save()
        {
            var list = new List<string>(_readSet);
            string json = JsonConvert.SerializeObject(list);
            PlayerPrefs.SetString(PrefsKey, json);
            PlayerPrefs.Save();
        }
    }
}
