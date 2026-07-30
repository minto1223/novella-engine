using UnityEditor;
using UnityEngine;

namespace NovellaEditor
{
    /// <summary>
    /// スキナー各種が使うテーマアセットの探索。
    ///
    /// 本体プロジェクトでは Assets/Novella/... に置かれているが、
    /// UPMパッケージとして導入された場合は Packages/com.novella.engine/... になる。
    /// パス直指定だと後者で見つからないため、まず直パスを試し、
    /// 駄目なら名前でプロジェクト全体を検索する。
    /// </summary>
    internal static class ThemeAssetLocator
    {
        private const string LocalThemeDir = "Assets/Novella/UI/Sprites/Theme/";
        private const string LocalDataDir = "Assets/Novella/Data/";

        /// <summary>テーマスプライトを取得する（引数はファイル名。例 "button_pill.png"）。</summary>
        public static Sprite Sprite(string fileName)
        {
            var direct = AssetDatabase.LoadAssetAtPath<Sprite>(LocalThemeDir + fileName);
            if (direct != null) return direct;

            var name = System.IO.Path.GetFileNameWithoutExtension(fileName);
            foreach (var guid in AssetDatabase.FindAssets($"{name} t:Sprite"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) != name) continue;
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null) return sprite;
            }
            Debug.LogWarning($"[ThemeAssetLocator] sprite '{fileName}' not found");
            return null;
        }

        /// <summary>ボタンスタイル等のアセットを取得する（引数は拡張子なしのアセット名）。</summary>
        public static T Asset<T>(string assetName) where T : Object
        {
            var direct = AssetDatabase.LoadAssetAtPath<T>($"{LocalDataDir}{assetName}.asset");
            if (direct != null) return direct;

            foreach (var guid in AssetDatabase.FindAssets($"{assetName} t:{typeof(T).Name}"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) != assetName) continue;
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null) return asset;
            }
            Debug.LogWarning($"[ThemeAssetLocator] asset '{assetName}' ({typeof(T).Name}) not found");
            return null;
        }
    }
}
