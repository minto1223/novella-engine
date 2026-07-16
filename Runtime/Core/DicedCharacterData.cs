using System;
using UnityEngine;

namespace Novella.Core
{
    /// <summary>
    /// Dicing方式の立ち絵データ。
    /// 表情差分画像群を固定サイズのセルに分割し、内容が同一のセルを共有して
    /// 1枚のアトラスに詰めたもの。表情ごとに「グリッド位置→アトラス内セル番号」の
    /// マップを持ち、DicedImage が実行時に合成描画する。
    /// Editorメニュー Novella > Diced Character Builder で生成する。
    /// </summary>
    public class DicedCharacterData : ScriptableObject
    {
        public string characterId;
        public Texture2D atlas;

        [Tooltip("分割セルの1辺のピクセル数")]
        public int cellSize = 64;
        [Tooltip("アトラス上の各セル周囲に確保した余白（バイリニア/圧縮のにじみ対策）")]
        public int padding = 2;

        public int sourceWidth;
        public int sourceHeight;
        public int gridWidth;    // 横方向のセル数
        public int gridHeight;   // 縦方向のセル数（gridY=0 が画像下端）
        public int atlasColumns; // アトラス1行に並ぶセルスロット数

        public DicedExpression[] expressions;

        [Serializable]
        public class DicedExpression
        {
            public string name;  // 基本表情は "default"、それ以外はファイル名の {characterId}_ 以降
            public int[] cells;  // 長さ gridWidth*gridHeight。値はアトラス内セル番号、-1は完全透明セル
        }

        /// <summary>表情名からマップを取得。null/空は "default" 扱い。見つからなければnull。</summary>
        public DicedExpression FindExpression(string expressionName)
        {
            if (expressions == null) return null;
            string key = string.IsNullOrEmpty(expressionName) ? "default" : expressionName;
            foreach (var e in expressions)
            {
                if (e != null && e.name == key) return e;
            }
            return null;
        }

        public bool HasExpression(string expressionName) => FindExpression(expressionName) != null;
    }
}
