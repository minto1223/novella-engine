using UnityEngine;

namespace Novella.Core
{
    public static class SettingsData
    {
        private const string KeyTextSpeed       = "novella_text_speed";
        private const string KeyBgmVolume       = "novella_bgm_volume";
        private const string KeySeVolume        = "novella_se_volume";
        private const string KeyVoiceVolume     = "novella_voice_volume";
        private const string KeyAutoDelay       = "novella_auto_delay";
        private const string KeyWindowOpacity   = "novella_window_opacity";
        private const string KeyFontSize        = "novella_font_size";
        private const string KeyFullscreen      = "novella_fullscreen";
        private const string KeySkipUnread      = "novella_skip_unread";
        private const string KeySkipAfterChoice = "novella_skip_after_choice";
        private const string KeyAutoSave        = "novella_auto_save";

        // デフォルト値
        public const float DefaultTextSpeed     = 40f;
        public const float DefaultBgmVolume     = 1f;
        public const float DefaultSeVolume      = 1f;
        public const float DefaultVoiceVolume   = 1f;
        public const float DefaultAutoDelay     = 2f;
        public const float DefaultWindowOpacity = 0.75f;
        public const float DefaultFontSize      = 46f;
        public const int   DefaultFullscreen    = 1;
        public const int   DefaultSkipUnread    = 0;
        public const int   DefaultSkipAfterChoice = 0;
        public const int   DefaultAutoSave        = 1;

        public static float TextSpeed
        {
            get => PlayerPrefs.GetFloat(KeyTextSpeed, DefaultTextSpeed);
            set { PlayerPrefs.SetFloat(KeyTextSpeed, value); PlayerPrefs.Save(); }
        }

        public static float BgmVolume
        {
            get => PlayerPrefs.GetFloat(KeyBgmVolume, DefaultBgmVolume);
            set { PlayerPrefs.SetFloat(KeyBgmVolume, value); PlayerPrefs.Save(); }
        }

        public static float SeVolume
        {
            get => PlayerPrefs.GetFloat(KeySeVolume, DefaultSeVolume);
            set { PlayerPrefs.SetFloat(KeySeVolume, value); PlayerPrefs.Save(); }
        }

        public static float VoiceVolume
        {
            get => PlayerPrefs.GetFloat(KeyVoiceVolume, DefaultVoiceVolume);
            set { PlayerPrefs.SetFloat(KeyVoiceVolume, value); PlayerPrefs.Save(); }
        }

        public static float AutoDelay
        {
            get => PlayerPrefs.GetFloat(KeyAutoDelay, DefaultAutoDelay);
            set { PlayerPrefs.SetFloat(KeyAutoDelay, value); PlayerPrefs.Save(); }
        }

        public static float WindowOpacity
        {
            get => PlayerPrefs.GetFloat(KeyWindowOpacity, DefaultWindowOpacity);
            set { PlayerPrefs.SetFloat(KeyWindowOpacity, value); PlayerPrefs.Save(); }
        }

        public static float FontSize
        {
            get => PlayerPrefs.GetFloat(KeyFontSize, DefaultFontSize);
            set { PlayerPrefs.SetFloat(KeyFontSize, value); PlayerPrefs.Save(); }
        }

        public static bool Fullscreen
        {
            get => PlayerPrefs.GetInt(KeyFullscreen, DefaultFullscreen) == 1;
            set { PlayerPrefs.SetInt(KeyFullscreen, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool SkipUnread
        {
            get => PlayerPrefs.GetInt(KeySkipUnread, DefaultSkipUnread) == 1;
            set { PlayerPrefs.SetInt(KeySkipUnread, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool SkipAfterChoice
        {
            get => PlayerPrefs.GetInt(KeySkipAfterChoice, DefaultSkipAfterChoice) == 1;
            set { PlayerPrefs.SetInt(KeySkipAfterChoice, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool AutoSave
        {
            get => PlayerPrefs.GetInt(KeyAutoSave, DefaultAutoSave) == 1;
            set { PlayerPrefs.SetInt(KeyAutoSave, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static void ResetAll()
        {
            TextSpeed     = DefaultTextSpeed;
            BgmVolume     = DefaultBgmVolume;
            SeVolume      = DefaultSeVolume;
            VoiceVolume   = DefaultVoiceVolume;
            AutoDelay     = DefaultAutoDelay;
            WindowOpacity = DefaultWindowOpacity;
            FontSize      = DefaultFontSize;
            Fullscreen    = DefaultFullscreen == 1;
            SkipUnread    = DefaultSkipUnread == 1;
            SkipAfterChoice = DefaultSkipAfterChoice == 1;
            AutoSave        = DefaultAutoSave == 1;
        }
    }
}
