using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UvA.DataNose.Connectors.Canvas
{
    public class AssignmentOverride : CanvasObject
    {
        public AssignmentOverride(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"Assignment override {ID}: {Title}";
        internal override string CanvasObjectID => "assignment_override";
        internal override string SaveUrl => $"{Assignment.SaveUrl}/overrides/{ID}";

        [JsonIgnore]
        public Assignment Assignment { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("due_at")]
        public DateTime? DueDate { get; set; }
        [JsonProperty("lock_at")]
        public DateTime? AvailableFrom { get; set; }
        [JsonProperty("unlock_at")]
        public DateTime? AvailableUntil { get; set; }
        [JsonProperty("student_ids")]
        public int[] StudentIDs { get; set; }
        [JsonProperty("course_section_id")]
        public int? SectionID { get; set; }
    }
}
