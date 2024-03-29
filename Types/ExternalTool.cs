﻿using Newtonsoft.Json;
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

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ToolVisibility
    {
        [EnumMember(Value = "members")] Members,
        [EnumMember(Value = "admins")] Teachers
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum NavigationVisibility
    {
        [EnumMember(Value = "disabled")] Hide,
        [EnumMember(Value = "enabled")] Show
    }

    public class ExternalTool : CanvasObject
    {
        public ExternalTool(CanvasApiConnector conn) { Connector = conn; }

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
        [JsonProperty("client_id")]
        public string ClientId { get; set; }
        [JsonProperty("custom_fields")]
        public Dictionary<string, string> CustomFields { get; set; }
        [JsonProperty("course_navigation")]
        public NavigationSettings CourseNavigationSettings { get; set; }
    }

    public class NavigationSettings
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("visibility")]
        public ToolVisibility? Visibility { get; set; }
        [JsonProperty("default")]
        public NavigationVisibility ShowInNavigation { get; set; }
        [JsonProperty("enabled")]
        public bool? IsEnabled { get; set; }

    }
}
