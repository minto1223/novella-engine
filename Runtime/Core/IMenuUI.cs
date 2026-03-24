namespace Novella.Core
{
    public interface IMenuUI
    {
        void Init(NovellaEngine engine);
        /// <summary>メニュー・サブパネルが開いていてゲーム入力をブロック中か</summary>
        bool IsBlockingInput { get; }
    }
}
