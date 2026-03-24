using System.Collections.Generic;
using UnityEngine;

namespace Novella.Core
{
    public static class EndingManager
    {
        private const string KeyPrefix = "novella_ending_";
        private const string KeyList = "novella_ending_list";

        public static void RecordEnding(string endingLabel)
        {
            if (string.IsNullOrEmpty(endingLabel)) return;
            string key = KeyPrefix + endingLabel;
            if (PlayerPrefs.HasKey(key)) return;

            PlayerPrefs.SetString(key, "1");

            string list = PlayerPrefs.GetString(KeyList, "");
            if (!string.IsNullOrEmpty(list))
                list += "," + endingLabel;
            else
                list = endingLabel;
            PlayerPrefs.SetString(KeyList, list);
            PlayerPrefs.Save();
        }

        public static bool IsUnlocked(string endingLabel)
        {
            return PlayerPrefs.HasKey(KeyPrefix + endingLabel);
        }

        public static List<string> GetUnlockedEndings()
        {
            string list = PlayerPrefs.GetString(KeyList, "");
            var result = new List<string>();
            if (string.IsNullOrEmpty(list)) return result;

            var seen = new HashSet<string>();
            foreach (var entry in list.Split(','))
            {
                string label = entry.Trim();
                if (!string.IsNullOrEmpty(label) && seen.Add(label))
                    result.Add(label);
            }
            return result;
        }

        public static void ClearAll()
        {
            var endings = GetUnlockedEndings();
            foreach (var label in endings)
                PlayerPrefs.DeleteKey(KeyPrefix + label);
            PlayerPrefs.DeleteKey(KeyList);
            PlayerPrefs.Save();
        }
    }
}
