using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public class Submission : CanvasObject
    {
        public Submission(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"Submission {ID}";
        internal override string CanvasObjectID => "submission";
        internal override string SaveUrl => $"courses/{CourseID}/assignments/{AssignmentID}/submissions/{UserID}";

        [JsonProperty("course_id")]
        public int CourseID { get; set; }
        [JsonProperty("assignment_id")]
        public int AssignmentID { get; set; }
        [JsonProperty("user_id")]
        public string UserID { get; set; }

        /// <summary>
        /// Use this for setting grades
        /// </summary>
        [JsonProperty("posted_grade")]
        public string PostedGrade { get; set; }

        /// <summary>
        /// Use this for reading grades
        /// </summary>
        [JsonProperty("grade")]
        public string Grade { get; set; }
        [JsonProperty("score")]
        public double? Score { get; set; }
        //[JsonProperty("submission_comments")]
        //public string[] Comments { get; set; }
        [JsonProperty("submitted_at")]
        public DateTime? SubmittedAt { get; set; }
        [JsonProperty("graded_at")]
        public DateTime? GradedAt { get; set; }
        [JsonProperty("grader_id")]
        public int? GraderID { get; set; }

        /// <summary>
        /// Submission body for text submissions
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; set; }

        /// <summary>
        /// Use this for reading excused status
        /// </summary>
        [JsonProperty("excused")]
        public bool? IsExcused { get; set; }
        [JsonProperty("missing")]
        public bool? IsMissing { get; set; }

        /// <summary>
        /// Use this for setting excused status
        /// </summary>
        [JsonProperty("excuse")]
        public bool? SetExcused { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("rubric_assessment")]
        public Dictionary<string, RubricScore> RubricScores { get; set; }

        /// <summary>
        /// Sets the UserID based on SIS user ID. Only for creating new enrolments
        /// </summary>
        public string SISUserID { set => UserID = "sis_user_id:" + value; }
    }

    public class RubricScore
    {
        [JsonProperty("rating_id")]
        public string RatingId { get; set; }
        [JsonProperty("points")]
        public double Points { get; set; }
    }

    public class SubmissionGroup : CanvasObject
    {
        [JsonProperty("submissions")]
        public Submission[] Submissions { get; set; }
        [JsonProperty("user_id")]
        public int UserID { get; set; }
        [JsonProperty("section_id")]
        public int SectionID { get; set; }
        [JsonProperty("sis_user_id")]
        public string SISUserID { get; set; }
    }
}
