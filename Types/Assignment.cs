using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GradingType
    {
        [EnumMember(Value = "pass_fail")] PassFail,
        [EnumMember(Value = "percent")] Percentage,
        [EnumMember(Value = "letter_grade")] Letters,
        [EnumMember(Value = "gpa_scale")] GPA,
        [EnumMember(Value = "points")] Points,
        [EnumMember(Value = "not_graded")] NotGraded
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SubmissionType
    {
        [EnumMember(Value = "online_quiz")] Quiz,
        [EnumMember(Value = "none")] None,
        [EnumMember(Value = "on_paper")] OnPaper,
        [EnumMember(Value = "discussion_topic")] Discussion,
        [EnumMember(Value = "media_recording")] MediaRecording,
        [EnumMember(Value = "external_tool")] ExternalTool,
        [EnumMember(Value = "online_upload")] Upload,
        [EnumMember(Value = "online_text_entry")] TextEntry,
        [EnumMember(Value = "online_url")] Url,
        [EnumMember(Value = "not_graded")] NotGraded,
        [EnumMember(Value = "wiki_page")] Page,
        [EnumMember(Value = "student_annotation")] StudentAnnotation
    }

    public class Assignment : CanvasObject
    {
        public Assignment(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"Assignment {ID}: {Name}";
        internal override string CanvasObjectID => "assignment";
        internal override string SaveUrl => $"courses/{CourseID}/assignments/{ID}";
        internal override string GetUrl => $"courses/{CourseID}/assignments/{ID}";

        [JsonProperty("course_id")]
        public int CourseID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("grading_type")]
        public GradingType GradingType { get; set; }
        [JsonProperty("published")]
        public bool IsPublished { get; set; }
        [JsonProperty("html_url")]
        public string Url { get; set; }
        [JsonProperty("points_possible")]
        public double? PointsPossible { get; set; }
        [JsonProperty("due_at")]
        public DateTime? DueDate { get; set; }
        [JsonProperty("lock_at")]
        public DateTime? AvailableFrom { get; set; }
        [JsonProperty("unlock_at")]
        public DateTime? AvailableUntil { get; set; }
        [JsonProperty("submission_types")]
        public SubmissionType[] SubmissionTypes { get; set; }
        [JsonProperty("muted")]
        public bool IsMuted { get; set; }
        [JsonProperty("external_tool_tag_attributes")]
        public ExternalToolAttributes ExternalToolAttributes { get; set; }
        [JsonProperty("has_submitted_submissions")]
        public bool HasSubmittedSubmissions { get; set; }
        [JsonProperty("assignment_group_id")]
        public int? AssignmentGroupID { get; set; }
        [JsonProperty("position")]
        public int? Position { get; set; }
        [JsonProperty("secure_params")]
        public string SecureParameters { get; set; }
        [JsonProperty("rubric")]
        public RubricCriterion[] RubricCriteria { get; set; }
        [JsonProperty("only_visible_to_overrides")]
        public bool? IsOverrideOnly { get; set; }

        /// <summary>
        /// Undocumented property. Should be 'online' to set a similarity checking tool
        /// </summary>
        [JsonProperty("submission_type")]
        public string SubmissionType { get; set; }

        [JsonProperty("configuration_tool_type")]
        public string ToolType { get; set; }
        [JsonProperty("similarityDetectionTool")]
        public string SimilarityTool { get; set; }

        // These are undocumented, but useful
        [JsonProperty("original_course_id")]
        public int? OriginalCourseID { get; set; }
        [JsonProperty("original_assignment_id")]
        public int? OriginalAssignmentID { get; set; }

        private List<Submission> _Submissions;
        [JsonIgnore]
        public List<Submission> Submissions => _Submissions ?? (_Submissions = Connector.RetrieveCollection<Submission>(this, "user"));

        private List<AssignmentOverride> _Overrides;
        [JsonIgnore]
        public List<AssignmentOverride> Overrides => _Overrides ?? (_Overrides = Connector.RetrieveCollection<AssignmentOverride>(this, path: "override"));

        public void LoadSubmissions(bool includeRubric = false) => _Submissions = Connector.RetrieveCollection<Submission>(this, "user", includeRubric ? ("include[]", "rubric_assessment") : ((string, string)?)null);
        public void SaveSubmissions(IEnumerable<Submission> subs)
            => Connector.Create($"{SaveUrl}/submissions/update_grades", subs.Select(s => s.IsExcused == true ? ($"grade_data[{s.UserID}][excuse]", (object)"true") : ($"grade_data[{s.UserID}][posted_grade]", s.PostedGrade)).ToArray());
    }

    public class RubricCriterion
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("points")]
        public double MaximumPoints { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class ExternalToolAttributes
    {
        [JsonProperty("content_id")]
        public string ContentID { get; set; }

        /// <summary>
        /// This is probably an enum but it's not documented.
        /// Only value observed so far: context_external_tool
        /// </summary>
        [JsonProperty("content_type")]
        public string ContentType { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("new_tab")]
        public bool? NewTab { get; set; }
    }
}
