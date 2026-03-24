using System.Collections.Generic;

namespace Novella.Core
{
    public class BacklogEntry
    {
        public string CharacterName;
        public string Text;
        public string ScriptPath;
        public int CommandIndex;
        public string VoiceClip;
        public string NameColorHex; // "#RRGGBB" or null
    }

    public class BacklogManager
    {
        private readonly List<BacklogEntry> _entries = new List<BacklogEntry>();

        public IReadOnlyList<BacklogEntry> Entries => _entries;

        public void Add(string characterName, string text, string scriptPath = null, int commandIndex = -1, string voiceClip = null, string nameColorHex = null)
        {
            _entries.Add(new BacklogEntry
            {
                CharacterName = characterName,
                Text = text,
                ScriptPath = scriptPath,
                CommandIndex = commandIndex,
                VoiceClip = voiceClip,
                NameColorHex = nameColorHex
            });
        }

        public void RemoveLast()
        {
            if (_entries.Count > 0)
                _entries.RemoveAt(_entries.Count - 1);
        }

        public void Clear() => _entries.Clear();
    }
}
