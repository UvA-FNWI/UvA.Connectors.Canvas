using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    /// <summary>
    /// Represents an appointment group.
    /// Use with caution:
    /// - This feature makes little sense and the API documentation is incomplete. This is mostly a guess how it should work.
    /// - Appointment group authorization is very strange, account admin rights do not suffice. 
    /// </summary>
    public class AppointmentGroup : CanvasObject
    {
        public AppointmentGroup(CanvasConnector conn) { Connector = conn; }

        internal override string CanvasObjectID => "appointment_group";
        public override string ToString() => $"Appointment group {ID}: {Title}";
        internal override string GetUrl => $"appointment_groups/{ID}?include[]=appointments&include[]=child_events";
        internal override string SaveUrl => $"appointment_groups/{ID}";

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("appointments")]
        public IEnumerable<Appointment> Appointments { get; set; }

    }

    /// <summary>
    /// Represents both a time slot and a choice by a student, for some reason
    /// </summary>
    public class Appointment : CanvasObject
    {
        public Appointment() { }
        public Appointment(CanvasConnector conn) { Connector = conn; }

        public override string ToString() => $"Appointment {ID}: {Title}";
        
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

        [JsonProperty("child_events")]
        public Appointment[] ChildEvents { get; set; }
        [JsonProperty("user")]
        public User User { get; set; }
    }
}
