using System;
using System.Collections.Generic;

namespace Novella.Core
{
    public interface IChoiceUI
    {
        void Show(List<ChoiceOption> choices, Action<ChoiceOption> onSelected);

        /// <summary>表示中の選択肢を閉じる。ロード時など選択待ちを中断する場合に呼ばれる。</summary>
        void Hide();
    }
}
