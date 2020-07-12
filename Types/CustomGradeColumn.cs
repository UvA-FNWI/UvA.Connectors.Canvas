using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public class CustomGradeColumn : CanvasObject
    {
        public CustomGradeColumn(CanvasApiConnector conn) { Connector = conn; }

        internal override string CanvasObjectID => "column";
        internal override string SaveUrl => $"courses/{CourseID}/custom_gradebook_columns/{ID}";

        public int CourseID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("position")]
        public int? Position { get; set; }
        [JsonProperty("read_only")]
        public bool IsReadOnly { get; set; }
        [JsonProperty("hidden")]
        public bool? IsHidden { get; set; }
        [JsonProperty("teacher_notes")]
        public bool? IsTeacherNotes { get; set; }

        public Dictionary<int, string> Data =>
            Connector.GetCollection($"courses/{CourseID}/custom_gradebook_columns/{ID}/data?per_page=100")
                .ToDictionary(t => (int)t["user_id"], t => (string)t["content"]);

        public void SetData(int userID, string content)
            => Connector.Update($"{SaveUrl}/data/{userID}", ("column_data[content]", content));
    }
}
