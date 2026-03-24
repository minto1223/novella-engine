using System;
using System.Collections.Generic;

namespace Novella.Core
{
    public interface IChoiceUI
    {
        void Show(List<ChoiceOption> choices, Action<ChoiceOption> onSelected);
    }
}
