using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public class Page : CanvasObject
    {
        public Page(CanvasConnector conn) { Connector = conn; }

        public override string ToString() => $"Page {Url}: {Title}";
        internal override string SaveUrl => $"courses/{CourseID}/pages/{Url}";
        internal override string GetUrl => $"courses/{CourseID}/pages/{Url}";
        internal override string CanvasObjectID => "wiki_page";

        [JsonProperty("course_id")]
        public int CourseID { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("body")]
        public string Body { get; set; }
        [JsonProperty("published")]
        public bool IsPublished { get; set; }

        public string GetBody()
        {
            Connector.RetrieveData(this);
            return Body;
        }

        [JsonIgnore]
        public string FullUrl => $"{Connector.TargetUrl}/{SaveUrl}";
        [JsonIgnore]
        public string FullApiUrl => $"{Connector.TargetUrl}/api/v1/{SaveUrl}";
    }
}
