using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public class Term : CanvasObject
    {
        public Term (CanvasConnector conn) { Connector = conn; }

        public override string ToString() => $"Term {ID}: {Name}";
        internal override string CanvasObjectID => "enrollment_term";

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("start_at")]
        public DateTime? StartAt { get; set; }
        [JsonProperty("end_at")]
        public DateTime? EndAt { get; set; }
        [JsonProperty("sis_term_id")]
        public string SISTermID { get; set; }
    }
}
