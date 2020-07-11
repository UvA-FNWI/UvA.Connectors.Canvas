using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public class Group : CanvasObject
    {
        public Group(CanvasConnector conn) { Connector = conn; }

        internal override string GetUrl => $"groups/{ID}";
        internal override string SaveUrl => $"group_categories/{GroupCategoryID}/groups/{ID}";

        public int? GroupCategoryID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("sis_group_id")]
        public string SISGroupID { get; set; }

        private List<User> _Users;
        [JsonIgnore]
        public List<User> Users => _Users ?? (_Users = Connector.RetrieveCollection<User>(this));

        public void AddMember(string sisId)
            => Connector.Create($"groups/{ID}/memberships", ("user_id", "sis_user_id:" + sisId));
    }
}
