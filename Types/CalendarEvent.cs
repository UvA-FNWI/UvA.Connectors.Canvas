using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace UvA.DataNose.Connectors.Canvas
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EventType
    {
        [EnumMember(Value = "event")] Event,
        [EnumMember(Value = "assignment")] Assignment
    }

    public class CalendarEvent : CanvasObject
    {
        public CalendarEvent(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"CalendarEvent {ID}: {Title}";
        internal override string CanvasObjectID => "calendar_event";
        internal override string SaveUrl => ID == 0 ? "calendar_events" : $"calendar_events/{ID}";

        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("context_code")]
        public string ContextCode { get; set; }
        [JsonProperty("start_at")]
        public DateTime? StartAt { get; set; }
        [JsonProperty("end_at")]
        public DateTime? EndAt { get; set; }
        [JsonProperty("location_name")]
        public string LocationName { get; set; }
        [JsonProperty("location_address")]
        public string LocationAddress { get; set; }
        [JsonProperty("html_url")]
        public string Url { get; set; }
        [JsonProperty("type")]
        public EventType Type { get; set; }

        [JsonProperty("child_event_data")]
        public ChildEvent[] ChildEvents { get; set; }
    }

    public class ChildEvent : CanvasObject
    {
        [JsonProperty("context_code")]
        public string ContextCode { get; set; }
        [JsonProperty("start_at")]
        public DateTime? StartAt { get; set; }
        [JsonProperty("end_at")]
        public DateTime? EndAt { get; set; }
    }
}
