using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UvA.DataNose.Connectors.Canvas
{
    public class Conversation : CanvasObject
    {
        public Conversation(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"Conversation {ID}: {Subject}";
        internal override bool SendWrapped => false;

        internal override string SaveUrl => $"conversations/{ID}";

        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("body")]
        public string Body { get; set; }
        [JsonProperty("recipients")]
        public string[] Recipients { get; set; }

    }
}
