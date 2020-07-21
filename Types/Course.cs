using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace UvA.DataNose.Connectors.Canvas
{
    public enum CourseEvent
    {
        [EnumMember(Value = "offer")] Publish,
        [EnumMember(Value = "claim")] Unpublish,
        [EnumMember(Value = "conclude")] Conclude,
        [EnumMember(Value = "delete")] Delete,
        [EnumMember(Value = "undelete")] Undelete,
    }

    public enum CourseState
    {
        [EnumMember(Value = "unpublished")] Unpublished,
        [EnumMember(Value = "available")] Available,
        [EnumMember(Value = "completed")] Concluded,
        [EnumMember(Value = "deleted")] Deleted
    }

    public enum FeatureFlag
    {
        [EnumMember(Value = "new_gradebook")] NewGradebook,
        [EnumMember(Value = "quizzes_next")] QuizzesNext,
    }

    public enum FlagState
    {
        [EnumMember(Value = "off")] Off,
        [EnumMember(Value = "allowed")] Allowed,
        [EnumMember(Value = "on")] On,
    }

    public class Course : CanvasObject
    {
        public Course(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"Course {ID}: {Name} ({CourseCode})";
        internal override string CanvasObjectID => "course";
        internal override string SaveUrl => ID != null ? $"courses/{ID}" : $"accounts/{AccountID}/courses";

        /// <summary>
        /// Use this to retrieve the term
        /// </summary>
        [JsonProperty("enrollment_term_id")]
        public int? EnrollmentTermID { get; set; }
        /// <summary>
        /// Use this to set the term
        /// </summary>
        [JsonProperty("term_id")]
        public string TermID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("account_id")]
        public int AccountID { get; set; }
        [JsonProperty("uuid")]
        public string UUID { get; set; }
        [JsonProperty("start_at")]
        public DateTime? StartDate { get; set; }
        [JsonProperty("end_at")]
        public DateTime? EndDate{ get; set; }
        [JsonProperty("course_code")]
        public string CourseCode { get; set; }
        [JsonProperty("sis_course_id")]
        public string SISCourseID { get; set; }
        [JsonProperty("event")]
        public CourseEvent? Event { get; set; }
        [JsonProperty("restrict_enrollments_to_course_dates")]
        public bool UseOverrideDates { get; set; }
        [JsonProperty("workflow_state")]
        public CourseState State { get; set; }

        [JsonProperty("grading_standard_enabled")]
        public bool? EnableGradingScheme { get; set; }
        [JsonProperty("grading_standard_id")]
        public int? GradingSchemeID { get; set; }
        [JsonProperty("hide_final_grades")]
        public bool? HideGradeTotals { get; set; }

        [DataMember(Name = "offer")]
        public bool? Publish { get; set; }
        [DataMember(Name = "enroll_me")]
        public bool? EnrollCurrentUser { get; set; }

        /// <summary>
        /// Sets the TermID based on SIS term ID
        /// </summary>
        public string SISTermID { set => TermID = "sis_term_id:" + value; }

        private List<Section> _Sections;
        [JsonIgnore]
        public List<Section> Sections { get => _Sections ?? (_Sections = Connector.RetrieveCollection<Section>(this)); }

        private List<Assignment> _Assignments;
        [JsonIgnore]
        public List<Assignment> Assignments => _Assignments ?? (_Assignments = Connector.RetrieveCollection<Assignment>(this, initFunc: a => a.CourseID = ID.Value));

        private List<Quiz> _Quizzes;
        [JsonIgnore]
        public List<Quiz> Quizzes => _Quizzes ?? (_Quizzes = Connector.RetrieveCollection<Quiz>(this, path: "quizze", initFunc: a => a.CourseID = ID.Value));

        private List<Discussion> _Announcements;
        [JsonIgnore]
        public List<Discussion> Announcements => _Announcements ?? (_Announcements = Connector.RetrieveCollection<Discussion>(this, path: "discussion_topic", param: ("only_announcements", "true"), initFunc: d => d.CourseID = ID.Value));

        private List<Discussion> _Discussions;
        [JsonIgnore]
        public List<Discussion> Discussions => _Discussions ?? (_Discussions = Connector.RetrieveCollection<Discussion>(this, path: "discussion_topic", initFunc: d => d.CourseID = ID.Value));

        private List<ExternalFeed> _ExternalFeeds;
        [JsonIgnore]
        public List<ExternalFeed> ExternalFeeds => _ExternalFeeds ?? (_ExternalFeeds = Connector.RetrieveCollection<ExternalFeed>(this, path: "external_feed", initFunc: d => d.CourseID = ID.Value));

        private List<Module> _Modules;
        [JsonIgnore]
        public List<Module> Modules => _Modules ?? (_Modules = Connector.RetrieveCollection<Module>(this, initFunc: m => m.CourseID = ID.Value));

        private List<CustomGradeColumn> _CustomGradeColumns;
        [JsonIgnore]
        public List<CustomGradeColumn> CustomGradeColumns => _CustomGradeColumns ?? (_CustomGradeColumns = Connector.RetrieveCollection<CustomGradeColumn>(this, param: ("include_hidden", "true"), path: "custom_gradebook_column", initFunc: c => c.CourseID = ID.Value));

        public void LoadAssignments() => _Assignments = Connector.RetrieveCollection<Assignment>(this);

        private List<Enrollment> _Enrollments;
        [JsonIgnore]
        public List<Enrollment> Enrollments => _Enrollments ?? (_Enrollments = Connector.RetrieveCollection<Enrollment>(this));

        private List<Enrollment> _UserEnrollments;
        /// <summary>
        /// Gets the enrollments on this course for the currently active user
        /// </summary>
        [JsonIgnore]
        public List<Enrollment> UserEnrollments => _UserEnrollments ?? (_UserEnrollments = Connector.RetrieveCollection<Enrollment>(this, param: ("user_id", "self")));

        public List<Enrollment> GetEnrollmentsByType(EnrollmentType type) => Connector.RetrieveCollection<Enrollment>(this, null, ("type", ToCanvasString(type)));
        public List<Enrollment> GetEnrollmentsByType(EnrollmentType type, string[] sisuserIds) 
            => Connector.RetrieveCollection<Enrollment>(this, sisuserIds.Select(i => ("sis_user_id[]", i)).Append(("type", ToCanvasString(type))).ToArray());

        public List<User> GetUsersByType(EnrollmentType type) => Connector.RetrieveCollection<User>(this, null, ("enrollment_type", type.ToString().ToLower()));

        private List<Folder> _Folders;
        [JsonIgnore]
        public List<Folder> Folders => _Folders ?? (_Folders = Connector.RetrieveCollection<Folder>(this));

        private List<File> _Files;
        [JsonIgnore]
        public List<File> Files => _Files ?? (_Files = Connector.RetrieveCollection<File>(this));

        private List<GroupCategory> _GroupCategories;
        [JsonIgnore]
        public List<GroupCategory> GroupCategories => _GroupCategories ?? (_GroupCategories = Connector.RetrieveCollection<GroupCategory>(this, path: "group_categorie", initFunc: g => g.CourseID = ID.Value));

        private List<AssignmentGroup> _AssignmentGroups;
        [JsonIgnore]
        public List<AssignmentGroup> AssignmentGroups => _AssignmentGroups ?? (_AssignmentGroups = Connector.RetrieveCollection<AssignmentGroup>(this, "assignments", path: "assignment_group", initFunc: g => { g.Assignments?.ForEach(z => z.Connector = Connector); }));

        private Dictionary<string, bool> _Permissions;
        [JsonIgnore]
        public Dictionary<string, bool> Permissions => _Permissions ?? (_Permissions = Connector.Get($"courses/{ID}/permissions").ToObject<Dictionary<string, bool>>());

        private List<Page> _Pages;
        [JsonIgnore]
        public List<Page> Pages => _Pages ?? (_Pages = Connector.RetrieveCollection<Page>(this, "page", initFunc: p => p.CourseID = ID.Value));

        private List<ExternalTool> _ExternalTools;
        [JsonIgnore]
        public List<ExternalTool> ExternalTools => _ExternalTools ?? (_ExternalTools = Connector.RetrieveCollection<ExternalTool>(this, path: "external_tool", initFunc: a => a.CourseID = ID.Value));

        private List<ContentMigration> _ContentMigrations;
        [JsonIgnore]
        public List<ContentMigration> ContentMigrations => _ContentMigrations ?? (_ContentMigrations = Connector.RetrieveCollection<ContentMigration>(this, path: "content_migration"));

        [JsonIgnore]
        public IEnumerable<Tab> Tabs => Connector.RetrieveArray<Tab>($"{SaveUrl}/tabs");
        public void UpdateTab(Tab tab) => Connector.Update($"{SaveUrl}/tabs/{tab.ID}",
            ("hidden", tab.IsHidden));

        public void SetFeatureFlag(FeatureFlag flag, FlagState state)
            => Connector.Update($"{SaveUrl}/features/flags/{ToCanvasString(flag)}", ("state", ToCanvasString(state)));

        /// <summary>
        /// Returns all graded submissions for the course, with limited fields for performance reasons
        /// </summary>
        /// <param name="sisuserID">Optional. If set, retrieve only the submissions for this student</param>
        /// <param name="gradedOnly">If <c>true</c>, retrieve only submitted with a grade</param>
        public IEnumerable<SubmissionGroup> GetSubmissions(string sisuserID = null, bool gradedOnly = true)
            => GetSubmissions(new[] { sisuserID }, gradedOnly);

        /// <summary>
        /// Returns all graded submissions for the course, with limited fields for performance reasons
        /// </summary>
        /// <param name="sisuserIDs">Optional. If set, retrieve only the submissions for these students</param>
        /// <param name="gradedOnly">If <c>true</c>, retrieve only submitted with a grade</param>
        public IEnumerable<SubmissionGroup> GetSubmissions(string[] sisuserIDs, bool gradedOnly)
        {
            var pars = new List<(string, string)>
            {
                ("grouped", "true"),
                ("exclude_response_fields[]", "preview_url"),
                ("response_fields[]", "assignment_id"),
                ("response_fields[]", "score"),
                ("response_fields[]", "grade"),
                ("response_fields[]", "excused"),
                ("response_fields[]", "id"),
            };
            if (sisuserIDs?.Where(s => s != null).Any() == true)
                pars.AddRange(sisuserIDs.Select(s => ("student_ids[]", $"sis_user_id:{s}")));
            else
                pars.Add(("student_ids[]", "all"));
            if (gradedOnly)
                pars.Add(("workflow_state", "graded"));
            else
                pars.Add(("response_fields[]", "submitted_at"));
            return Connector.RetrieveCollection<SubmissionGroup>(this, path: "students/submission", extraParams: pars.ToArray());
        }
    }
}
