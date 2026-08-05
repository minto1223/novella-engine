#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Novella.Editor
{
    /// <summary>
    /// ビルダー系Editorツールが使う既定フォントの取得口。
    ///
    /// 【なぜ直パス定数ではダメか】
    /// 各ビルダーは `AssetDatabase.LoadAssetAtPath("Assets/...")` でフォントを直接指していたが、
    /// UPM経由で導入した場合パッケージは `Packages/<package-name>/...` 配下に置かれるため、
    /// `Assets/` 始まりのパスは解決できずnullになる。
    /// その場合TMPが既定フォント（Latin専用）にフォールバックし、日本語が全て豆腐になる。
    ///
    /// そのため直パス → 名前検索の順に探す。`ThemeAssetLocator` と同じ方針。
    /// </summary>
    public static class NovellaEditorFont
    {
        private const string PrimaryPath = "Assets/Novella/Fonts/NotoSansJP SDF.asset";
        private const string FontName = "NotoSansJP SDF";

        private static TMP_FontAsset _cached;

        /// <summary>既定の日本語フォント（NotoSansJP SDF）を返す。見つからなければnull。</summary>
        public static TMP_FontAsset Load()
        {
            if (_cached != null) return _cached;

            _cached = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PrimaryPath);
            if (_cached != null) return _cached;

            // UPM導入時（Packages/配下）やフォルダ移動後はGUID検索で拾う
            foreach (var guid in AssetDatabase.FindAssets($"\"{FontName}\" t:TMP_FontAsset"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                _cached = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (_cached != null) return _cached;
            }

            Debug.LogWarning(
                $"[Novella] {FontName} が見つかりませんでした。生成したテキストはTMPの既定フォント" +
                "（Latinのみ）になり、日本語が表示できません。");
            return null;
        }
    }
}
#endif
