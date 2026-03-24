using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Novella.Core
{
    /// <summary>
    /// ローカライズ管理。Resources/Localization/{lang}.json から翻訳をロード。
    /// JSON形式: { "greeting": "こんにちは", "farewell": "さようなら" }
    /// テキスト中の #key# をローカライズ値に置換する。
    /// </summary>
    public class LocalizationManager
    {
        private static LocalizationManager _instance;
        public static LocalizationManager Instance => _instance ??= new LocalizationManager();

        private string _currentLanguage = "ja";
        private Dictionary<string, string> _translations = new Dictionary<string, string>();
        private readonly List<string> _availableLanguages = new List<string>();

        private const string PrefKey = "novella_language";

        public string CurrentLanguage => _currentLanguage;
        public IReadOnlyList<string> AvailableLanguages => _availableLanguages;

        public LocalizationManager()
        {
            DetectAvailableLanguages();
            string saved = PlayerPrefs.GetString(PrefKey, "");
            if (!string.IsNullOrEmpty(saved) && _availableLanguages.Contains(saved))
                _currentLanguage = saved;
            else if (_availableLanguages.Count > 0)
                _currentLanguage = _availableLanguages[0];
            LoadLanguage(_currentLanguage);
        }

        public void SetLanguage(string lang)
        {
            if (_currentLanguage == lang) return;
            _currentLanguage = lang;
            PlayerPrefs.SetString(PrefKey, lang);
            PlayerPrefs.Save();
            LoadLanguage(lang);
            Debug.Log($"[Novella] Language set to: {lang}");
        }

        public string Get(string key)
        {
            return _translations.TryGetValue(key, out var val) ? val : null;
        }

        /// <summary>
        /// テキスト中の #key# をローカライズ値に置換する。
        /// 見つからないキーはそのまま残す。
        /// </summary>
        public string Resolve(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            return System.Text.RegularExpressions.Regex.Replace(text, @"#([a-zA-Z_][a-zA-Z0-9_.]*)#", m =>
            {
                string key = m.Groups[1].Value;
                string val = Get(key);
                return val ?? m.Value;
            });
        }

        private void LoadLanguage(string lang)
        {
            _translations.Clear();
            var asset = Resources.Load<TextAsset>($"Localization/{lang}");
            if (asset == null)
            {
                Debug.LogWarning($"[Novella] Localization file not found: Localization/{lang}");
                return;
            }

            try
            {
                _translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(asset.text)
                    ?? new Dictionary<string, string>();
                Debug.Log($"[Novella] Loaded {_translations.Count} translations for '{lang}'");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Novella] Failed to parse localization file '{lang}': {e.Message}");
            }
        }

        private void DetectAvailableLanguages()
        {
            _availableLanguages.Clear();
            var assets = Resources.LoadAll<TextAsset>("Localization");
            foreach (var asset in assets)
                _availableLanguages.Add(asset.name);
            if (_availableLanguages.Count == 0)
                _availableLanguages.Add("ja");
        }

        /// <summary>言語一覧を再スキャンする</summary>
        public void RefreshLanguages()
        {
            DetectAvailableLanguages();
        }
    }
}
