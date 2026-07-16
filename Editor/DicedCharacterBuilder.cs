using System.Collections.Generic;
using System.IO;
using Novella.Core;
using UnityEditor;
using UnityEngine;

namespace Novella.Editor
{
    /// <summary>
    /// Resources/Characters の表情差分PNG群から、Dicing方式のアトラスと
    /// DicedCharacterData を生成するツール。
    /// 対象: {characterId}.png（基本表情=default）と {characterId}_*.png（表情差分）。
    /// _blink / _talk 付きファイルも独立した表情として取り込まれ、
    /// CharacterAnimator のまばたき・口パクにそのまま使われる。
    /// </summary>
    public class DicedCharacterBuilder : EditorWindow
    {
        private const string SourceDir = "Assets/Novella/Resources/Characters";
        private const string OutputDir = "Assets/Novella/Resources/Characters/Diced";
        private const int Padding = 2;
        private const int MaxAtlasSize = 8192;

        private string _characterId = "";
        private int _cellSize = 64;
        private Vector2 _scroll;
        private string _lastReport = "";

        [MenuItem("Novella/Diced Character Builder")]
        public static void Open()
        {
            var win = GetWindow<DicedCharacterBuilder>("Diced Character Builder");
            win.minSize = new Vector2(360, 240);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Dicing差分立ち絵ビルダー", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                $"{SourceDir}/ の {{ID}}.png と {{ID}}_*.png をセル分割し、重複を除いた1枚のアトラスに統合します。" +
                "生成後は show_char が自動的にアトラス版を使います（元PNGは削除可能。ただしCG回想等で直接参照している場合を除く）。",
                MessageType.Info);

            _characterId = EditorGUILayout.TextField("Character ID", _characterId);
            _cellSize = Mathf.Clamp(EditorGUILayout.IntField("Cell Size (px)", _cellSize), 16, 256);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_characterId)))
                {
                    if (GUILayout.Button("Build", GUILayout.Height(28)))
                        _lastReport = Build(_characterId.Trim(), _cellSize);
                }
                if (GUILayout.Button("検出されたIDを一覧", GUILayout.Height(28)))
                    _lastReport = ListCandidates();
            }

            if (!string.IsNullOrEmpty(_lastReport))
            {
                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                EditorGUILayout.HelpBox(_lastReport, MessageType.None);
                EditorGUILayout.EndScrollView();
            }
        }

        private static string ListCandidates()
        {
            if (!Directory.Exists(SourceDir)) return $"{SourceDir} が見つかりません。";
            var ids = new SortedSet<string>();
            foreach (var f in Directory.GetFiles(SourceDir, "*.png"))
            {
                string name = Path.GetFileNameWithoutExtension(f);
                int us = name.IndexOf('_');
                ids.Add(us < 0 ? name : name.Substring(0, us));
            }
            return ids.Count == 0 ? "PNGが見つかりません。" : "候補ID:\n- " + string.Join("\n- ", ids);
        }

        private static string Build(string characterId, int cellSize)
        {
            // --- 1. ソース画像収集（インポート設定に依存しないようファイルから直接読む） ---
            if (!Directory.Exists(SourceDir)) return $"{SourceDir} が見つかりません。";

            var sources = new List<(string expr, Color32[] pixels)>();
            int srcW = 0, srcH = 0;
            foreach (var file in Directory.GetFiles(SourceDir, characterId + "*.png"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string expr;
                if (name == characterId) expr = "default";
                else if (name.StartsWith(characterId + "_")) expr = name.Substring(characterId.Length + 1);
                else continue;

                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!tex.LoadImage(File.ReadAllBytes(file)))
                {
                    DestroyImmediate(tex);
                    return $"読み込み失敗: {file}";
                }
                if (srcW == 0) { srcW = tex.width; srcH = tex.height; }
                else if (tex.width != srcW || tex.height != srcH)
                {
                    DestroyImmediate(tex);
                    return $"画像サイズが不一致です: {name} は {tex.width}x{tex.height}（他は {srcW}x{srcH}）。全表情を同一サイズに揃えてください。";
                }
                sources.Add((expr, tex.GetPixels32()));
                DestroyImmediate(tex);
            }
            if (sources.Count == 0) return $"'{characterId}' のPNGが {SourceDir} に見つかりません。";

            // --- 2. セル分割 + 重複排除 ---
            int gridW = Mathf.CeilToInt((float)srcW / cellSize);
            int gridH = Mathf.CeilToInt((float)srcH / cellSize);
            int slot = cellSize + Padding * 2;

            var blocks = new List<Color32[]>();               // 一意なセル（padding込みスロット画素）
            var hashMap = new Dictionary<long, List<int>>();  // ハッシュ→候補インデックス
            var expressions = new List<DicedCharacterData.DicedExpression>();

            foreach (var (expr, pixels) in sources)
            {
                var cells = new int[gridW * gridH];
                for (int gy = 0; gy < gridH; gy++)
                {
                    for (int gx = 0; gx < gridW; gx++)
                    {
                        var block = ExtractBlock(pixels, srcW, srcH, gx, gy, cellSize, Padding, slot, out bool coreEmpty);
                        if (coreEmpty)
                        {
                            cells[gy * gridW + gx] = -1;
                            continue;
                        }

                        long hash = HashBlock(block);
                        int found = -1;
                        if (hashMap.TryGetValue(hash, out var candidates))
                        {
                            foreach (int c in candidates)
                            {
                                if (BlocksEqual(blocks[c], block)) { found = c; break; }
                            }
                        }
                        if (found < 0)
                        {
                            blocks.Add(block);
                            found = blocks.Count - 1;
                            if (candidates == null) hashMap[hash] = candidates = new List<int>();
                            candidates.Add(found);
                        }
                        cells[gy * gridW + gx] = found;
                    }
                }
                expressions.Add(new DicedCharacterData.DicedExpression { name = expr, cells = cells });
            }

            // --- 3. アトラス構築 ---
            int count = blocks.Count;
            int cols = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(count)));
            int rows = Mathf.Max(1, Mathf.CeilToInt((float)count / cols));
            int atlasW = RoundUp4(cols * slot);
            int atlasH = RoundUp4(rows * slot);
            if (atlasW > MaxAtlasSize || atlasH > MaxAtlasSize)
                return $"アトラスが{MaxAtlasSize}pxを超えます（{atlasW}x{atlasH}）。cellSizeを大きくするか表情数を減らしてください。";

            var atlasPixels = new Color32[atlasW * atlasH]; // 既定で透明(0,0,0,0)
            for (int i = 0; i < count; i++)
            {
                int ox = (i % cols) * slot;
                int oy = (i / cols) * slot;
                var block = blocks[i];
                for (int y = 0; y < slot; y++)
                {
                    int dst = (oy + y) * atlasW + ox;
                    int src = y * slot;
                    for (int x = 0; x < slot; x++)
                        atlasPixels[dst + x] = block[src + x];
                }
            }

            var atlasTex = new Texture2D(atlasW, atlasH, TextureFormat.RGBA32, false);
            atlasTex.SetPixels32(atlasPixels);
            atlasTex.Apply();
            byte[] png = atlasTex.EncodeToPNG();
            DestroyImmediate(atlasTex);

            Directory.CreateDirectory(OutputDir);
            string atlasPath = $"{OutputDir}/{characterId}_atlas.png";
            File.WriteAllBytes(atlasPath, png);
            AssetDatabase.ImportAsset(atlasPath);

            var importer = (TextureImporter)AssetImporter.GetAtPath(atlasPath);
            importer.textureType = TextureImporterType.Default;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = MaxAtlasSize;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.SaveAndReimport();
            var atlasAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);

            // --- 4. データアセット生成/更新（GUIDを保つため既存があれば上書き更新） ---
            string dataPath = $"{OutputDir}/{characterId}.asset";
            var data = AssetDatabase.LoadAssetAtPath<DicedCharacterData>(dataPath);
            bool isNew = data == null;
            if (isNew) data = ScriptableObject.CreateInstance<DicedCharacterData>();

            data.characterId = characterId;
            data.atlas = atlasAsset;
            data.cellSize = cellSize;
            data.padding = Padding;
            data.sourceWidth = srcW;
            data.sourceHeight = srcH;
            data.gridWidth = gridW;
            data.gridHeight = gridH;
            data.atlasColumns = cols;
            data.expressions = expressions.ToArray();

            if (isNew) AssetDatabase.CreateAsset(data, dataPath);
            else EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();

            long srcPx = (long)sources.Count * srcW * srcH;
            long atlasPx = (long)atlasW * atlasH;
            string report =
                $"'{characterId}' 完了: {sources.Count}枚 ({srcW}x{srcH}) → 一意セル{count}個 → アトラス {atlasW}x{atlasH}\n" +
                $"ピクセル数 {100.0 * atlasPx / srcPx:F1}%（元の{sources.Count}枚合計比）\n" +
                $"表情: {string.Join(", ", expressions.ConvertAll(e => e.name))}\n" +
                $"出力: {dataPath}";
            Debug.Log($"[Novella] Diced build: {report}");
            return report;
        }

        /// <summary>
        /// グリッドセルをpadding込みで切り出す。paddingにはソース画像の実際の隣接画素を使う
        /// （バイリニア補間でセル境界の見た目が元画像と一致するように）。範囲外は透明。
        /// </summary>
        private static Color32[] ExtractBlock(Color32[] pixels, int srcW, int srcH,
            int gx, int gy, int cellSize, int pad, int slot, out bool coreEmpty)
        {
            var block = new Color32[slot * slot];
            coreEmpty = true;

            int baseX = gx * cellSize - pad;
            int baseY = gy * cellSize - pad;
            for (int y = 0; y < slot; y++)
            {
                int sy = baseY + y;
                if (sy < 0 || sy >= srcH) continue; // 範囲外は透明のまま
                int srcRow = sy * srcW;
                int dstRow = y * slot;
                for (int x = 0; x < slot; x++)
                {
                    int sx = baseX + x;
                    if (sx < 0 || sx >= srcW) continue;
                    var c = pixels[srcRow + sx];
                    block[dstRow + x] = c;
                    // 透明判定はコア領域（padding除く）のみ対象
                    if (c.a != 0 && x >= pad && x < slot - pad && y >= pad && y < slot - pad)
                        coreEmpty = false;
                }
            }
            return block;
        }

        private static long HashBlock(Color32[] block)
        {
            unchecked
            {
                ulong h = 14695981039346656037UL; // FNV-1a 64bit
                for (int i = 0; i < block.Length; i++)
                {
                    var c = block[i];
                    h = (h ^ c.r) * 1099511628211UL;
                    h = (h ^ c.g) * 1099511628211UL;
                    h = (h ^ c.b) * 1099511628211UL;
                    h = (h ^ c.a) * 1099511628211UL;
                }
                return (long)h;
            }
        }

        private static bool BlocksEqual(Color32[] a, Color32[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].r != b[i].r || a[i].g != b[i].g || a[i].b != b[i].b || a[i].a != b[i].a)
                    return false;
            }
            return true;
        }

        private static int RoundUp4(int v) => (v + 3) & ~3;
    }
}
