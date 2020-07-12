using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public class GroupCategory : CanvasObject
    {
        public GroupCategory(CanvasApiConnector conn) { Connector = conn; }

        internal override string GetUrl => SaveUrl;
        internal override string SaveUrl => ID == null ? $"courses/{CourseID}/group_categories/{ID}" : $"group_categories/{ID}";

        [JsonProperty("course_id")]
        public int CourseID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("sis_group_category_id")]
        public string SISGroupCategoryID { get; set; }

        private List<Group> _Groups;
        [JsonIgnore]
        public List<Group> Groups => _Groups ?? (_Groups = Connector.RetrieveCollection<Group>(this));
    }
}
