using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public enum ModuleItemType
    {
        File, Page, Discussion, Assignment, Quiz, SubHeader, ExternalUrl, ExternalTool
    }

    public class ModuleItem : CanvasObject
    {
        public ModuleItem(CanvasConnector conn) { Connector = conn; }

        public override string ToString() => $"Module item {ID}: {Title} ({Type})";
        internal override string CanvasObjectID => "module_item";

        [JsonProperty("module_id")]
        public int ModuleID { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("type")]
        public ModuleItemType Type { get; set; }
        [JsonProperty("content_id")]
        public int? ContentID { get; set; } 
        [JsonProperty("published")]
        public bool IsPublished { get; set; }
    }
}
