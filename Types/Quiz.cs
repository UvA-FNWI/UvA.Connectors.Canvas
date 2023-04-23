using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public class Quiz : CanvasObject
    {
        public Quiz(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"Assignment {ID}: {Name}";
        internal override string CanvasObjectID => "quiz";
        internal override string SaveUrl => $"courses/{CourseID}/quizzes/{ID}";
        internal override string GetUrl => $"courses/{CourseID}/quizzes/{ID}";

        [JsonProperty("course_id")]
        public int CourseID { get; set; }
        [JsonProperty("title")]
        public string Name { get; set; }
        [JsonProperty("published")]
        public bool IsPublished { get; set; }
        [JsonProperty("unpublishable")]
        public bool IsUnpublishable { get; set; }
        [JsonProperty("due_at")]
        public DateTime? DueDate { get; set; }
        [JsonProperty("assignment_group_id")]
        public int? AssignmentGroupID { get; set; }
        [JsonProperty("points_possible")]
        public int? PointsPossible { get; set; }
        [JsonProperty("quiz_type")]
        public QuizType Type { get; set; }

        List<QuizSubmission> _Submissions;
        public List<QuizSubmission> Submissions => _Submissions ?? (_Submissions = Connector.RetrieveCollection<QuizSubmission>(this, path: "submission", initFunc: s => s.CourseID = CourseID ));
    }

    public class QuizSubmission : CanvasObject
    {
        public QuizSubmission(CanvasApiConnector conn) { Connector = conn; }

        internal override string GetUrl => $"courses/{CourseID}/quizzes/{QuizID}/submissions/{ID}";

        [JsonIgnore]
        public int CourseID { get; set; }

        [JsonProperty("quiz_id")]
        public int QuizID { get; set; }
        [JsonProperty("user_ID")]
        public int UserID { get; set; }
        [JsonProperty("finished_at")]
        public DateTime? FinishedDate { get; set; }
        [JsonProperty("score")]
        public double? Score { get; set; }
        [JsonProperty("attempt")]
        public int? Attempt { get; set; }

        [JsonIgnore]
        public IEnumerable<QuizSubmissionQuestion> Answers => Connector.Get($"quiz_submissions/{ID}/questions")["quiz_submission_questions"].ToObject<QuizSubmissionQuestion[]>();

    }

    public class QuizSubmissionQuestion
    {
        [JsonProperty("id")]
        public int QuestionID { get; set; }
        [JsonProperty("answer")]
        public JToken AnswerData { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum QuizReportType
    { 
        [EnumMember(Value = "student_analysis")] StudentAnalysis,
        [EnumMember(Value = "item_analysis")] ItemAnalysis
    }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum QuizType
    { 
        [EnumMember(Value = "practice_quiz")] PracticeQuiz,
        [EnumMember(Value = "assignment")] Assignment,
        [EnumMember(Value = "graded_survey")] GradedSurvey,
        [EnumMember(Value = "survey")] Survey
    }

    public class QuizReport : CanvasObject
    {
        public QuizReport(CanvasApiConnector conn) { Connector = conn; }

        internal override string SaveUrl => $"courses/{CourseID}/quizzes/{QuizID}/reports/{ID}";
        internal override string GetUrl => SaveUrl;
        internal override string CanvasObjectID => "quiz_report";

        [JsonIgnore]
        public int? CourseID { get; set; }

        [JsonProperty("quiz_id")]
        public int? QuizID { get; set; }
        [JsonProperty("report_type")]
        public QuizReportType Type { get; set; }

        [JsonProperty("file")]
        public File File { get; set; }

        public void Update() => Connector.RetrieveData(this, "file");
    }
}
