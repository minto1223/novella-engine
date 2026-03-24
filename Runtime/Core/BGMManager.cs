using System.Collections.Generic;
using UnityEngine;

namespace Novella.Core
{
    /// <summary>
    /// 再生済みBGMを記録・管理する。PlayerPrefsで永続化。
    /// </summary>
    public static class BGMManager
    {
        private const string KeyPrefix = "novella_bgm_";
        private const string KeyList = "novella_bgm_list";

        /// <summary>BGMを再生済みとして記録する。</summary>
        public static void RecordBGM(string clipName, string displayTitle = null)
        {
            if (string.IsNullOrEmpty(clipName)) return;
            string key = KeyPrefix + clipName;
            if (PlayerPrefs.HasKey(key)) return;

            PlayerPrefs.SetString(key, displayTitle ?? clipName);

            string list = PlayerPrefs.GetString(KeyList, "");
            if (!string.IsNullOrEmpty(list))
                list += "," + clipName;
            else
                list = clipName;
            PlayerPrefs.SetString(KeyList, list);
            PlayerPrefs.Save();
        }

        /// <summary>BGMが再生済みかどうか。</summary>
        public static bool IsPlayed(string clipName)
        {
            return PlayerPrefs.HasKey(KeyPrefix + clipName);
        }

        /// <summary>再生済みBGMの一覧を取得する。(clipName, displayTitle) のペア。</summary>
        public static List<(string clipName, string title)> GetPlayedBGMs()
        {
            string list = PlayerPrefs.GetString(KeyList, "");
            var result = new List<(string, string)>();
            if (string.IsNullOrEmpty(list)) return result;

            var seen = new HashSet<string>();
            foreach (var entry in list.Split(','))
            {
                string name = entry.Trim();
                if (string.IsNullOrEmpty(name) || !seen.Add(name)) continue;
                string title = PlayerPrefs.GetString(KeyPrefix + name, name);
                result.Add((name, title));
            }
            return result;
        }

        /// <summary>BGM記録をすべてクリアする。</summary>
        public static void ClearAll()
        {
            var bgms = GetPlayedBGMs();
            foreach (var (name, _) in bgms)
                PlayerPrefs.DeleteKey(KeyPrefix + name);
            PlayerPrefs.DeleteKey(KeyList);
            PlayerPrefs.Save();
        }
    }
}
