using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PrivacyLevel
    {
        [EnumMember(Value = "anonymous")] Anonymous,
        [EnumMember(Value = "name_only")] NameOnly,
        [EnumMember(Value = "public")] Public
    }

    public class ExternalTool : CanvasObject
    {
        public ExternalTool(CanvasConnector conn) { Connector = conn; }

        public override string ToString() => $"External tool {ID}: {Name}";
        internal override string CanvasObjectID => "external_tool";
        internal override string SaveUrl => $"courses/{CourseID}/external_tools/{ID}";
        internal override string GetUrl => SaveUrl;
        internal override bool SendWrapped => false;

        [JsonProperty("course_id")]
        public int CourseID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("privacy_level")]
        public PrivacyLevel PrivacyLevel { get; set; }
        [JsonProperty("consumer_key")]
        public string ConsumerKey { get; set; }
        [JsonProperty("shared_secret")]
        public string SharedSecret { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("custom_fields")]
        public Dictionary<string, string> CustomFields { get; set; }
    }
}
