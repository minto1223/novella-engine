using System.Collections.Generic;
using UnityEngine;

namespace Novella.Core
{
    /// <summary>
    /// 閲覧済みCG（背景画像）を記録・管理する。
    /// PlayerPrefsで永続化。
    /// </summary>
    public static class CGManager
    {
        private const string KeyPrefix = "novella_cg_";
        private const string KeyList = "novella_cg_list";

        /// <summary>CGを閲覧済みとして記録する。</summary>
        public static void RecordCG(string imageName)
        {
            if (string.IsNullOrEmpty(imageName)) return;
            string key = KeyPrefix + imageName;
            if (PlayerPrefs.GetInt(key, 0) == 1) return;
            PlayerPrefs.SetInt(key, 1);

            // リストに追加（カンマ区切り）
            string list = PlayerPrefs.GetString(KeyList, "");
            if (!string.IsNullOrEmpty(list))
                list += "," + imageName;
            else
                list = imageName;
            PlayerPrefs.SetString(KeyList, list);
            PlayerPrefs.Save();
        }

        /// <summary>CG が閲覧済みかどうか。</summary>
        public static bool IsViewed(string imageName)
        {
            return PlayerPrefs.GetInt(KeyPrefix + imageName, 0) == 1;
        }

        /// <summary>閲覧済みCG名の一覧を取得する。</summary>
        public static List<string> GetViewedCGs()
        {
            string list = PlayerPrefs.GetString(KeyList, "");
            var result = new List<string>();
            if (string.IsNullOrEmpty(list)) return result;

            // 重複除去
            var seen = new HashSet<string>();
            foreach (var name in list.Split(','))
            {
                string trimmed = name.Trim();
                if (!string.IsNullOrEmpty(trimmed) && seen.Add(trimmed))
                    result.Add(trimmed);
            }
            return result;
        }

        /// <summary>CG記録をすべてクリアする。</summary>
        public static void ClearAll()
        {
            var list = GetViewedCGs();
            foreach (var name in list)
                PlayerPrefs.DeleteKey(KeyPrefix + name);
            PlayerPrefs.DeleteKey(KeyList);
            PlayerPrefs.Save();
        }
    }
}
