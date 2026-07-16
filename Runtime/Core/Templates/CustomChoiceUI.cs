using System;
using System.Collections.Generic;
using UnityEngine;

namespace Novella.Core.Templates
{
    /// <summary>
    /// カスタム選択肢UIのテンプレート。
    /// このクラスを継承して独自の選択肢表示を実装し、
    /// NovellaEngine の Custom UI Overrides > Choice UI にアサインしてください。
    /// </summary>
    public abstract class CustomChoiceUI : MonoBehaviour, IChoiceUI
    {
        /// <summary>
        /// 選択肢を表示する。プレイヤーが選択したら onSelected を呼ぶこと。
        /// 条件フィルタリングは済んだ状態で呼ばれる。
        /// </summary>
        public abstract void Show(List<ChoiceOption> choices, Action<ChoiceOption> onSelected);

        /// <summary>
        /// 表示中の選択肢を閉じる。ロード時など選択待ちの中断時に呼ばれる。
        /// 選択肢UIを画面から消す処理をオーバーライドして実装すること（既定は何もしない）。
        /// </summary>
        public virtual void Hide() { }
    }
}
