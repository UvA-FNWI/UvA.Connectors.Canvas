using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public class Discussion : CanvasObject
    {
        public Discussion(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"Discussion {ID}: {Title}";
        internal override string CanvasObjectID => "discussion_topic";
        internal override string SaveUrl => $"courses/{CourseID}/discussion_topics/{ID}";
        internal override string GetUrl => $"courses/{CourseID}/discussion_topics/{ID}";
        internal override bool SendWrapped => false;

        [JsonProperty("course_id")]
        public int CourseID { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("posted_at")]
        public DateTime? PostedAt { get; set; }
        [JsonProperty("delayed_post_at")]
        public DateTime? DelayedPostAt { get; set; }
        [JsonProperty("is_announcement")]
        public bool? IsAnnouncement { get; set; }
        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonIgnore]
        public List<DiscussionEntry> Entries => Connector.RetrieveCollection<DiscussionEntry>(this, path: "entrie", initFunc: e => { e.TopicID = ID; e.CourseID = CourseID; });
    }

    public class DiscussionEntry : CanvasObject
    {
        public DiscussionEntry(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"Entry {ID}: {Message}";
        internal override string SaveUrl => $"courses/{CourseID}/discussion_topics/{TopicID}/entries/{ID}";
        internal override string GetUrl => $"courses/{CourseID}/discussion_topics/{TopicID}/entries/{ID}";
        internal override bool SendWrapped => false;

        [JsonIgnore]
        public int CourseID { get; set; }
        [JsonIgnore]
        public int? TopicID { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("user_id")]
        public int? UserID { get; set; }
        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonIgnore]
        public List<DiscussionReply> Replies => Connector.RetrieveCollection<DiscussionReply>(this, path: "replie", initFunc: r => r.Entry = this);
    }

    public class DiscussionReply : CanvasObject
    {
        public DiscussionReply(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"Reply {ID}: {Message}";
        internal override string SaveUrl => $"{Entry.SaveUrl}/replies/{ID}";
        internal override string GetUrl => $"{Entry.GetUrl}/replies/{ID}";
        internal override bool SendWrapped => false;

        [JsonIgnore]
        public DiscussionEntry Entry { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("user_id")]
        public int? UserID { get; set; }
        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}
