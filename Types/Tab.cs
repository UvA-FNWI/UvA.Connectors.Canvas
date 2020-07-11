using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public class Tab 
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("hidden")]
        public bool IsHidden { get; set; }
        [JsonProperty("position")]
        public int Position { get; set; }

        public override string ToString() => $"{Label} {(IsHidden ? " (hidden)" : "")}";
    }
}
