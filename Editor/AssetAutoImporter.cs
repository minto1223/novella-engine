#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Novella.Editor
{
    public class AssetAutoImporter
    {
        [MenuItem("Novella/Auto Import Assets")]
        public static void ImportAll()
        {
            int count = 0;
            count += ImportSprites("Assets/Novella/Resources/Backgrounds", SpriteAlignment.Center);
            count += ImportSprites("Assets/Novella/Resources/Characters", SpriteAlignment.BottomCenter);
            count += ImportAudio("Assets/Novella/Resources/Audio/BGM");
            count += ImportAudio("Assets/Novella/Resources/Audio/SE");
            count += ImportAudio("Assets/Novella/Resources/Audio/Voice");
            count += ImportMovies("Assets/Novella/Resources/Movies");

            AssetDatabase.Refresh();

            if (count > 0)
                Debug.Log($"[Novella] Auto Import: {count} asset(s) updated.");
            else
                Debug.Log("[Novella] Auto Import: All assets are already configured correctly.");
        }

        private static int ImportSprites(string folder, SpriteAlignment alignment)
        {
            if (!Directory.Exists(folder)) return 0;

            int count = 0;
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                bool changed = false;

                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    changed = true;
                }

                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    changed = true;
                }

                // Characters はピボットを BottomCenter に設定
                if (alignment == SpriteAlignment.BottomCenter)
                {
                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    if (settings.spriteAlignment != (int)SpriteAlignment.BottomCenter)
                    {
                        settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                        settings.spritePivot = new Vector2(0.5f, 0f);
                        importer.SetTextureSettings(settings);
                        changed = true;
                    }
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                    count++;
                }
            }
            return count;
        }

        private static int ImportAudio(string folder)
        {
            if (!Directory.Exists(folder)) return 0;

            int count = 0;
            var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folder });

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null) continue;

                // BGMはストリーミング、SE/Voiceはデフォルト設定
                bool isBgm = folder.Contains("BGM");
                var settings = importer.defaultSampleSettings;
                bool changed = false;

                if (isBgm && settings.loadType != AudioClipLoadType.Streaming)
                {
                    settings.loadType = AudioClipLoadType.Streaming;
                    importer.defaultSampleSettings = settings;
                    changed = true;
                }
                else if (!isBgm && settings.loadType == AudioClipLoadType.Streaming)
                {
                    settings.loadType = AudioClipLoadType.DecompressOnLoad;
                    importer.defaultSampleSettings = settings;
                    changed = true;
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                    count++;
                }
            }
            return count;
        }

        private static int ImportMovies(string folder)
        {
            if (!Directory.Exists(folder)) return 0;

            // VideoClipはUnityが自動でインポートするので、存在確認のみ
            var guids = AssetDatabase.FindAssets("", new[] { folder });
            int count = 0;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path)) continue;
                string ext = Path.GetExtension(path).ToLower();
                if (ext == ".mp4" || ext == ".webm" || ext == ".ogv" || ext == ".mov")
                    count++;
            }
            if (count > 0)
                Debug.Log($"[Novella] Auto Import: Found {count} movie file(s) in Movies/ (auto-imported by Unity).");
            return 0; // movies don't need reimport
        }
    }
}
#endif
