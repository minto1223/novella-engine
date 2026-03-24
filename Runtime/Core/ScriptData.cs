using System.Collections.Generic;
using Newtonsoft.Json;

namespace Novella.Core
{
    public class NovellaScript
    {
        [JsonProperty("title")]
        public string Title;

        [JsonProperty("commands")]
        public List<ScriptCommand> Commands = new List<ScriptCommand>();
    }

    public class ScriptCommand
    {
        [JsonProperty("type")]
        public string Type;

        [JsonProperty("character")]
        public string Character;

        [JsonProperty("text")]
        public string Text;

        [JsonProperty("image")]
        public string Image;

        [JsonProperty("position")]
        public string Position; // left / center / right

        [JsonProperty("expression")]
        public string Expression;

        [JsonProperty("duration")]
        public float Duration;

        [JsonProperty("label")]
        public string Label;

        [JsonProperty("target")]
        public string Target;

        [JsonProperty("clip")]
        public string Clip;

        [JsonProperty("volume")]
        public float Volume;

        [JsonProperty("value")]
        public string Value;

        [JsonProperty("order")]
        public int Order = -1;

        [JsonProperty("layer")]
        public string Layer;

        [JsonProperty("choices")]
        public List<ChoiceOption> Choices;
    }

    public class ChoiceOption
    {
        [JsonProperty("text")]
        public string Text;

        [JsonProperty("target")]
        public string Target;

        [JsonProperty("set_flag")]
        public string SetFlag;

        [JsonProperty("flag_value")]
        public string FlagValue;

        /// <summary>
        /// 表示条件。"flag_name" → フラグがtrueなら表示。
        /// "!flag_name" → フラグがfalseなら表示。
        /// "flag_name==value" → フラグが指定値なら表示。
        /// "flag_name>=10" → 数値比較。
        /// 未指定なら常に表示。
        /// </summary>
        [JsonProperty("condition")]
        public string Condition;
    }
}
