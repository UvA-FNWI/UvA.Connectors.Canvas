using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public class Folder : CanvasObject
    {
        public Folder(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"Folder {ID}: {Name}";
        internal override string SaveUrl => $"courses/{CourseID}/folders/{ID}";
        internal override bool SendWrapped => false;

        [JsonProperty("course_id")]
        public int CourseID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("parent_folder_id")]
        public int? ParentFolderID { get; set; }

        private List<File> _Files;
        [JsonIgnore]
        public List<File> Files => _Files ?? (_Files = Connector.RetrieveCollection<File>(this));

        //[JsonProperty("parent_folder_path")]
        //public string ParentFolderPath { get; set; }
    }
}
