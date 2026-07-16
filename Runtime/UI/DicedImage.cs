using Novella.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.UI
{
    /// <summary>
    /// DicedCharacterData をアトラスから合成描画するImage。
    /// Imageを継承しているため、CharacterDisplayController の色フェード・複製・
    /// 移動アニメーション等の既存処理がそのまま適用できる（spriteは使用しない）。
    /// </summary>
    public class DicedImage : Image
    {
        [SerializeField] private DicedCharacterData _data;
        [SerializeField] private string _expression = "default";

        public DicedCharacterData Data => _data;
        public string CurrentExpression => _expression;

        public override Texture mainTexture =>
            _data != null && _data.atlas != null ? _data.atlas : base.mainTexture;

        public void Init(DicedCharacterData data, string expression)
        {
            _data = data;
            _expression = Normalize(expression);
            if (_data != null && !_data.HasExpression(_expression))
            {
                Debug.LogWarning($"[Novella] DicedImage: expression '{_expression}' not found in '{_data.characterId}'. Falling back to 'default'.");
                _expression = "default";
            }
            SetAllDirty();
        }

        /// <summary>表情を切り替える。存在しない表情名は無視（警告のみ）。</summary>
        public void SetExpression(string expression)
        {
            string key = Normalize(expression);
            if (_expression == key) return;
            if (_data != null && !_data.HasExpression(key))
            {
                Debug.LogWarning($"[Novella] DicedImage: expression '{key}' not found in '{_data.characterId}'.");
                return;
            }
            _expression = key;
            SetVerticesDirty();
        }

        public static string Normalize(string expression) =>
            string.IsNullOrEmpty(expression) ? "default" : expression;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (_data == null)
            {
                // データ未設定時は通常のImageとして振る舞う
                base.OnPopulateMesh(vh);
                return;
            }

            vh.Clear();

            var exp = _data.FindExpression(_expression);
            if (exp == null || exp.cells == null || _data.atlas == null ||
                _data.sourceWidth <= 0 || _data.sourceHeight <= 0 || _data.atlasColumns <= 0)
                return; // 描画しない（nullスプライトのImageのように白矩形を出さない）

            Rect r = GetDrawRect();
            float scaleX = r.width / _data.sourceWidth;
            float scaleY = r.height / _data.sourceHeight;
            int cs = _data.cellSize;
            int pad = _data.padding;
            int slot = cs + pad * 2;
            float atlasW = _data.atlas.width;
            float atlasH = _data.atlas.height;
            Color32 col = color;

            int vert = 0;
            for (int gy = 0; gy < _data.gridHeight; gy++)
            {
                for (int gx = 0; gx < _data.gridWidth; gx++)
                {
                    int cellIndex = exp.cells[gy * _data.gridWidth + gx];
                    if (cellIndex < 0) continue;

                    // 端のセルはソース画像に収まる分だけ描く
                    int cw = Mathf.Min(cs, _data.sourceWidth - gx * cs);
                    int ch = Mathf.Min(cs, _data.sourceHeight - gy * cs);

                    // 頂点位置（gy=0 が画像下端。ビルダーのGetPixels32と同じ座標系）
                    float x0 = r.x + gx * cs * scaleX;
                    float y0 = r.y + gy * cs * scaleY;
                    float x1 = x0 + cw * scaleX;
                    float y1 = y0 + ch * scaleY;

                    // UV（スロット内のpadding分だけ内側を参照）
                    int ax = (cellIndex % _data.atlasColumns) * slot + pad;
                    int ay = (cellIndex / _data.atlasColumns) * slot + pad;
                    float u0 = ax / atlasW;
                    float v0 = ay / atlasH;
                    float u1 = (ax + cw) / atlasW;
                    float v1 = (ay + ch) / atlasH;

                    vh.AddVert(new Vector3(x0, y0), col, new Vector2(u0, v0));
                    vh.AddVert(new Vector3(x0, y1), col, new Vector2(u0, v1));
                    vh.AddVert(new Vector3(x1, y1), col, new Vector2(u1, v1));
                    vh.AddVert(new Vector3(x1, y0), col, new Vector2(u1, v0));
                    vh.AddTriangle(vert + 0, vert + 1, vert + 2);
                    vh.AddTriangle(vert + 2, vert + 3, vert + 0);
                    vert += 4;
                }
            }
        }

        /// <summary>preserveAspect対応の描画先Rect（Imageの挙動を踏襲）</summary>
        private Rect GetDrawRect()
        {
            Rect r = GetPixelAdjustedRect();
            if (!preserveAspect || _data.sourceHeight <= 0 || r.height <= 0f) return r;

            float srcRatio = (float)_data.sourceWidth / _data.sourceHeight;
            float rectRatio = r.width / r.height;
            if (srcRatio > rectRatio)
            {
                float h = r.width / srcRatio;
                r.y += (r.height - h) * rectTransform.pivot.y;
                r.height = h;
            }
            else
            {
                float w = r.height * srcRatio;
                r.x += (r.width - w) * rectTransform.pivot.x;
                r.width = w;
            }
            return r;
        }
    }
}
