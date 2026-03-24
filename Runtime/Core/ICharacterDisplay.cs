using System;

namespace Novella.Core
{
    public interface ICharacterDisplay
    {
        void ShowCharacter(string characterId, string expression, string position, float duration, Action onComplete);
        void ShowCharacter(string characterId, string expression, string position, float duration, string effect, Action onComplete);
        void ShowCharacter(string characterId, string expression, string position, float duration, string effect, int order, Action onComplete);
        void HideCharacter(string characterId, Action onComplete);
        void HideCharacter(string characterId, string effect, Action onComplete);
        void MoveCharacter(string characterId, string toPosition, float duration, Action onComplete);
        void MoveCharacter(string characterId, string toPosition, float duration, int order, Action onComplete);
        void StartTalking(string characterId);
        void StopTalking(string characterId);
        void StopAllTalking();
        void HideAllCharacters();
    }
}
