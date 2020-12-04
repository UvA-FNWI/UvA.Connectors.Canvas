using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UvA.DataNose.Connectors.Canvas
{
    public class Collaboration : CanvasObject
    {
        public Collaboration(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"Collaboration {ID}: {Title}";
        internal override string CanvasObjectID => "collaboration";
        internal override string GetUrl => $"courses/{ContextID}/collaborations/{ID}";

        [JsonProperty("context_id")]
        public int ContextID { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("user_id")]
        public int UserID { get; set; }
        [JsonProperty("created_at")]
        public DateTime DateCreated { get; set; }
        [JsonProperty("collaboration_type")]
        public string Type { get; set; }
        [JsonProperty("user_name")]
        public string UserName { get; set; }
    }
}
