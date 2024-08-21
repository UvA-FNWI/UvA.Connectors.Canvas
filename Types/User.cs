using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public class User : CanvasObject
    {
        public User(CanvasApiConnector conn) { Connector = conn; }

        public IEnumerable<Course> GetCourses(string include) => Connector.RetrieveCollection<Course>(this, param: ("include[]", include));
        public override string ToString() => $"User {Name}";
        internal override string CanvasObjectID => "user";
        internal override string SaveUrl => $"users/{ID}";

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("sortable_name")]
        public string SortableName { get; set; }
        [JsonProperty("sis_user_id")]
        public string SISUserID { get; set; }
        [JsonProperty("login_id")]
        public string LoginID { get; set; }
    }
}
