using System.Collections.Generic;
using UnityEngine;

namespace Novella.Core
{
    /// <summary>
    /// クリア済みシーンを記録・管理する。PlayerPrefsで永続化。
    /// SceneDefinitionのsceneIdまたはスクリプトパスをキーとして使用。
    /// </summary>
    public static class SceneRecollectionManager
    {
        private const string KeyPrefix = "novella_scene_";
        private const string KeyList = "novella_scene_list";

        /// <summary>シーンをクリア済みとして記録する。</summary>
        public static void RecordScene(string sceneId, string title)
        {
            if (string.IsNullOrEmpty(sceneId)) return;
            string key = KeyPrefix + sceneId;
            if (PlayerPrefs.HasKey(key)) return;

            PlayerPrefs.SetString(key, title ?? sceneId);

            string list = PlayerPrefs.GetString(KeyList, "");
            if (!string.IsNullOrEmpty(list))
                list += "," + sceneId;
            else
                list = sceneId;
            PlayerPrefs.SetString(KeyList, list);
            PlayerPrefs.Save();
        }

        /// <summary>シーンがクリア済みかどうか。</summary>
        public static bool IsCleared(string sceneId)
        {
            return PlayerPrefs.HasKey(KeyPrefix + sceneId);
        }

        /// <summary>クリア済みシーンの一覧を取得する。(sceneId, title) のペア。</summary>
        public static List<(string id, string title)> GetClearedScenes()
        {
            string list = PlayerPrefs.GetString(KeyList, "");
            var result = new List<(string, string)>();
            if (string.IsNullOrEmpty(list)) return result;

            var seen = new HashSet<string>();
            foreach (var entry in list.Split(','))
            {
                string id = entry.Trim();
                if (string.IsNullOrEmpty(id) || !seen.Add(id)) continue;
                string title = PlayerPrefs.GetString(KeyPrefix + id, id);
                result.Add((id, title));
            }
            return result;
        }

        /// <summary>回想記録をすべてクリアする。</summary>
        public static void ClearAll()
        {
            var scenes = GetClearedScenes();
            foreach (var (id, _) in scenes)
                PlayerPrefs.DeleteKey(KeyPrefix + id);
            PlayerPrefs.DeleteKey(KeyList);
            PlayerPrefs.Save();
        }
    }
}
