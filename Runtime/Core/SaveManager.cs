using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Novella.Core
{
    public class SaveManager
    {
        private const string FilePrefix = "novella_save_";
        private const string QuickSaveFile = "novella_quicksave.json";
        private const string AutoSaveFile = "novella_autosave.json";
        private const string FileExt = ".json";

        /// <summary>最後にキャプチャしたゲーム画面のPNGデータ（UIなし状態）。</summary>
        private static byte[] _cachedScreenshot;

        private string SlotPath(int slot) =>
            Path.Combine(Application.persistentDataPath, $"{FilePrefix}{slot}{FileExt}");

        private string QuickSavePath =>
            Path.Combine(Application.persistentDataPath, QuickSaveFile);

        private string AutoSavePath =>
            Path.Combine(Application.persistentDataPath, AutoSaveFile);

        /// <summary>
        /// ゲーム画面のスクリーンショットをキャッシュする。
        /// メニューやUI表示前に呼ぶこと。
        /// </summary>
        public static IEnumerator CacheScreenshot()
        {
            yield return new WaitForEndOfFrame();

            try
            {
                var screenTex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                screenTex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                screenTex.Apply();

                // 縮小（幅320px）
                int thumbW = 320;
                int thumbH = (int)(Screen.height * (320f / Screen.width));
                var thumbTex = new Texture2D(thumbW, thumbH, TextureFormat.RGB24, false);

                for (int y = 0; y < thumbH; y++)
                {
                    for (int x = 0; x < thumbW; x++)
                    {
                        float u = (float)x / thumbW;
                        float v = (float)y / thumbH;
                        thumbTex.SetPixel(x, y, screenTex.GetPixelBilinear(u, v));
                    }
                }
                thumbTex.Apply();

                _cachedScreenshot = thumbTex.EncodeToPNG();

                UnityEngine.Object.Destroy(screenTex);
                UnityEngine.Object.Destroy(thumbTex);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Novella] Screenshot cache failed: {e.Message}");
            }
        }

        /// <summary>キャッシュ済みスクリーンショットをファイルに保存する。</summary>
        private static void SaveCachedScreenshot(string fileName)
        {
            if (_cachedScreenshot == null || _cachedScreenshot.Length == 0) return;
            try
            {
                string path = Path.Combine(Application.persistentDataPath, fileName);
                File.WriteAllBytes(path, _cachedScreenshot);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Novella] Screenshot save failed: {e.Message}");
            }
        }

        private static SaveData BuildSaveData(NovellaEngine engine, string thumbFile)
        {
            return new SaveData
            {
                ScriptPath = engine.CurrentScriptPath,
                CommandIndex = engine.CurrentIndex,
                Flags = engine.Flags.GetAll(),
                SavedAt = DateTime.Now.ToString("yyyy/MM/dd HH:mm"),
                Title = engine.CurrentScriptTitle,
                LastDialogue = BuildLastDialogue(engine),
                ThumbnailFile = thumbFile,
                Visual = engine.GetVisualState(),
            };
        }

        private static void ApplyLoad(SaveData data, NovellaEngine engine)
        {
            engine.Flags.SetAll(data.Flags);
            engine.LoadAndPlayFrom(data.ScriptPath, data.CommandIndex, data.Visual);
        }

        public void QuickSave(NovellaEngine engine)
        {
            string thumbFile = "novella_thumb_quick.png";
            var data = BuildSaveData(engine, thumbFile);
            File.WriteAllText(QuickSavePath, JsonConvert.SerializeObject(data, Formatting.Indented));
            SaveCachedScreenshot(thumbFile);
            Debug.Log("[Novella] Quick saved.");
        }

        public bool QuickLoad(NovellaEngine engine)
        {
            if (!File.Exists(QuickSavePath))
            {
                Debug.LogWarning("[Novella] No quick save found.");
                return false;
            }
            var data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(QuickSavePath));
            ApplyLoad(data, engine);
            Debug.Log($"[Novella] Quick loaded ({data.SavedAt})");
            return true;
        }

        public bool HasQuickSave() => File.Exists(QuickSavePath);

        public void AutoSave(NovellaEngine engine)
        {
            if (!SettingsData.AutoSave) return;
            string thumbFile = "novella_thumb_auto.png";
            var data = BuildSaveData(engine, thumbFile);
            File.WriteAllText(AutoSavePath, JsonConvert.SerializeObject(data, Formatting.Indented));
            SaveCachedScreenshot(thumbFile);
            Debug.Log("[Novella] Auto saved.");
        }

        public bool AutoLoad(NovellaEngine engine)
        {
            if (!File.Exists(AutoSavePath))
            {
                Debug.LogWarning("[Novella] No auto save found.");
                return false;
            }
            var data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(AutoSavePath));
            ApplyLoad(data, engine);
            Debug.Log($"[Novella] Auto loaded ({data.SavedAt})");
            return true;
        }

        public bool HasAutoSave() => File.Exists(AutoSavePath);

        public SaveData GetAutoSaveInfo()
        {
            if (!File.Exists(AutoSavePath)) return null;
            return JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(AutoSavePath));
        }

        private static string BuildLastDialogue(NovellaEngine engine)
        {
            var entries = engine.Backlog.Entries;
            if (entries.Count == 0) return "";
            var last = entries[entries.Count - 1];
            return string.IsNullOrEmpty(last.CharacterName)
                ? last.Text
                : $"{last.CharacterName}「{last.Text}」";
        }

        public void Save(int slot, NovellaEngine engine)
        {
            string thumbFile = $"novella_thumb_{slot}.png";
            var data = BuildSaveData(engine, thumbFile);
            File.WriteAllText(SlotPath(slot), JsonConvert.SerializeObject(data, Formatting.Indented));
            SaveCachedScreenshot(thumbFile);
            Debug.Log($"[Novella] Saved to slot {slot}");
        }

        public void Load(int slot, NovellaEngine engine)
        {
            var path = SlotPath(slot);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[Novella] Save slot {slot} not found.");
                return;
            }
            var data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(path));
            ApplyLoad(data, engine);
            Debug.Log($"[Novella] Loaded from slot {slot} ({data.SavedAt})");
        }

        public SaveData GetInfo(int slot)
        {
            var path = SlotPath(slot);
            if (!File.Exists(path)) return null;
            return JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(path));
        }

        /// <summary>サムネイル画像をSpriteとしてロードする。</summary>
        public static Sprite LoadThumbnail(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            string path = Path.Combine(Application.persistentDataPath, fileName);
            if (!File.Exists(path)) return null;

            try
            {
                byte[] data = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2);
                if (!tex.LoadImage(data)) return null;
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            catch
            {
                return null;
            }
        }
    }
}
