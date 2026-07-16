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
        private static bool _dirty;

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
        /// 保存は即座には行わず、SaveIfDirty()呼び出し時にまとめて書き込む。
        /// </summary>
        public static void MarkRead(string scriptPath, int commandIndex)
        {
            EnsureLoaded();
            string key = $"{scriptPath}:{commandIndex}";
            if (_readSet.Add(key))
                _dirty = true;
        }

        /// <summary>
        /// 未保存の既読データがあれば書き込む。
        /// シーン遷移・アプリ終了・セーブ実行時などの節目で呼ぶこと。
        /// </summary>
        public static void SaveIfDirty()
        {
            if (!_dirty || _readSet == null) return;
            Save();
            _dirty = false;
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
            _dirty = false;
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
