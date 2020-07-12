using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public class AssignmentGroup : CanvasObject
    {
        public AssignmentGroup(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"Assignment group {ID}: {Name}";
        internal override string CanvasObjectID => "assignment_group";
        internal override string SaveUrl => $"courses/{CourseID}/assignment_groups/{ID}";
        internal override bool SendWrapped => false;

        [JsonProperty("course_id")]
        public int CourseID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("position")]
        public int? Position { get; set; }
        [JsonProperty("group_weight")]
        public double? Weight { get; set; }

        [JsonProperty("assignments")]
        public List<Assignment> Assignments { get; set; }
    }
}
