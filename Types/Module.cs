using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public class Module : CanvasObject
    {
        public Module(CanvasConnector conn) { Connector = conn; }

        public override string ToString() => $"Module {ID}: {Name}";
        internal override string CanvasObjectID => "module";
        internal override string SaveUrl => $"courses/{CourseID}/modules/{ID}";
        internal override string GetUrl => $"courses/{CourseID}/modules/{ID}";

        [JsonProperty("course_id")]
        public int CourseID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("published")]
        public bool IsPublished { get; set; }
        [JsonProperty("unlock_at")]
        public DateTime? UnlockDate { get; set; }
        [JsonProperty("position")]
        public int? Position { get; set; }

        private List<ModuleItem> _Items;
        [JsonIgnore]
        public List<ModuleItem> Items => _Items ?? (_Items = Connector.RetrieveCollection<ModuleItem>(this, path: "item", initFunc: m => m.ModuleID = ID.Value));
    }
}
