using Newtonsoft.Json;
using System;

namespace UvA.DataNose.Connectors.Canvas
{
    public class Section : CanvasObject
    {
        public Section(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"Section {ID}: {Name}";
        internal override string CanvasObjectID => "course_section";
        internal override string SaveUrl => ID == null ? $"courses/{CourseID}/sections/{ID}" : $"sections/{ID}"; 


        [JsonProperty("course_id")]
        public int CourseID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("start_at")]
        public DateTime? StartAt { get; set; }
        [JsonProperty("end_at")]
        public DateTime? EndAt { get; set; }
        [JsonProperty("restrict_enrollments_to_section_dates")]
        public bool? RestrictEnrollmentsToSectionDates { get; set; }
        [JsonProperty("sis_section_id")]
        public string SISSectionID { get; set; }
        [JsonProperty("enable_sis_reactivation")]
        public bool? EnableSISReactivation { get; set; }
    }
}
