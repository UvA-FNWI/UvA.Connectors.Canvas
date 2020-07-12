using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace UvA.DataNose.Connectors.Canvas
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EnrollmentType
    {
        [EnumMember(Value = "StudentEnrollment")] Student,
        [EnumMember(Value = "StudentViewEnrollment")] StudentView,
        [EnumMember(Value = "TeacherEnrollment")] Teacher,
        [EnumMember(Value = "TaEnrollment")] TA,
        [EnumMember(Value = "ObserverEnrollment")] Observer,
        [EnumMember(Value = "DesignerEnrollment")] Designer
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EnrollmentState
    {
        [EnumMember(Value = "active")] Active,
        [EnumMember(Value = "invited")] Invited,
        [EnumMember(Value = "inactive")] Inactive,
        [EnumMember(Value = "creation_pending")] CreationPending
    }

    public enum EnrollmentDeleteAction { Conclude, Delete, Deactivate }

    public class Enrollment : CanvasObject
    {
        public Enrollment(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"Enrollment {User?.Name} in {CourseID}";
        internal override string CanvasObjectID => "enrollment";
        internal override string SaveUrl => CourseID != null ? $"courses/{CourseID}/enrollments/{ID}"
            : SISCourseID != null ? $"courses/sis_course_id:{SISCourseID}/enrollments"
            : $"sections/sis_section_id:{SISSectionID}/enrollments";

        [JsonProperty("course_section_id")]
        public int? SectionID { get; set; }
        [JsonProperty("course_id")]
        public int? CourseID { get; set; }

        [JsonProperty("user_id")]
        public string UserID { get; set; }
        [JsonProperty("type")]
        public EnrollmentType Type { get; set; }
        [JsonProperty("enrollment_state")]
        public EnrollmentState State { get; set; }

        [JsonProperty("role_id")]
        public int? RoleID { get; set; }
        [JsonProperty("limit_privileges_to_course_section")]
        public bool? LimitInteractionToSection { get; set; }

        [JsonProperty("sis_section_id")]
        public string SISSectionID { get; set; }

        /// <summary>
        /// For creating new enrolments only
        /// </summary>
        [JsonIgnore]
        public string SISCourseID { get; set; }

        [JsonProperty("grades")]
        public EnrollmentGrades Grades { get; set; }

        /// <summary>
        /// Sets the UserID based on login ID. Only for creating new enrolments
        /// </summary>
        public string LoginID { set => UserID = "sis_login_id:" + value; }

        /// <summary>
        /// Sets the UserID based on SIS user ID. Only for creating new enrolments
        /// </summary>
        public string SISUserID { set => UserID = "sis_user_id:" + value; }
       
        [JsonProperty("user")]
        public User User { get; set; }

        public void Delete(EnrollmentDeleteAction action = EnrollmentDeleteAction.Delete)
        {
            Connector.Delete(this, ("task", action.ToString().ToLower()));
        }
    }

    public class EnrollmentGrades
    {
        [JsonProperty("current_score")]
        public double? ComputedCurrentScore { get; set; }
        [JsonProperty("final_score")]
        public double? ComputedFinalScore { get; set; }

        [JsonProperty("current_grade")]
        public string ComputedCurrentGrade { get; set; }
        [JsonProperty("final_grade")]
        public string ComputedFinalGrade { get; set; }

        [JsonProperty("unposted_current_score")]
        public double? ComputedCurrentScoreUnposted { get; set; }
        [JsonProperty("unposted_final_score")]
        public double? ComputedFinalScoreUnposted { get; set; }

        [JsonProperty("unposted_current_grade")]
        public string ComputedCurrentGradeUnposted { get; set; }
        [JsonProperty("unposted_final_grade")]
        public string ComputedFinalGradeUnposted { get; set; }
    }
}
