using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public class ExternalFeed : CanvasObject
    {
        public ExternalFeed(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"External feed {ID}: {Name}";
        internal override string CanvasObjectID => "external_feed";
        internal override string SaveUrl => $"courses/{CourseID}/external_feeds/{ID}";

        [JsonIgnore]
        public int CourseID { get; set; }
        [JsonProperty("display_name")]
        public string Name { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
