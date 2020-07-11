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
    public enum ContentMigrationType
    {
        [EnumMember(Value = "common_cartridge_importer")] CommonCartridgeImport,
        [EnumMember(Value = "course_copy_importer")] CourseCopy
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MigrationState
    {
        [EnumMember(Value = "pre_processing")] PreProcessing,
        [EnumMember(Value = "pre_processed")] PreProcessed,
        [EnumMember(Value = "running")] Running,
        [EnumMember(Value = "waiting_for_select")] WaitingForSelect,
        [EnumMember(Value = "completed")] Completed,
        [EnumMember(Value = "failed")] Failed,
        [EnumMember(Value = "queued")] Queued
    }

    public class ContentMigration : CanvasObject
    {
        public ContentMigration(CanvasConnector conn) { Connector = conn; }

        internal override string SaveUrl => $"courses/{CourseID}/content_migrations";
        internal override string GetUrl => $"{SaveUrl}/{ID}";
        internal override bool SendWrapped => false;

        public int CourseID { get; set; }

        [JsonProperty("migration_type")]
        public ContentMigrationType Type { get; set; }
        [JsonProperty("workflow_state")]
        public MigrationState State { get; set; }
        [JsonProperty("progress_url")]
        public string ProgressUrl { get; set; }

        [JsonIgnore]
        public string SourceCourseID { get => Settings.SourceCourseID; set => Settings.SourceCourseID = value; }

        [JsonProperty("selective_import")]
        public bool? IsSelectiveImport { get; set; }

        [JsonProperty("settings")]
        public ContentMigrationSettings Settings { get; set; } = new ContentMigrationSettings();
        [JsonProperty("date_shift_options")]
        public DateShiftOptions DateShiftOptions { get; set; }
        [JsonProperty("select")]
        public ContentMigrationSelection Selection { get; set; }
    }

    public class ContentMigrationSelection
    {
        [JsonProperty("quizzes")]
        public int[] QuizIDs { get; set; }
    }

    public class ContentMigrationSettings
    {
        [JsonProperty("source_course_id")]
        public string SourceCourseID { get; set; }
        [JsonProperty("file_url")]
        public string FileUrl { get; set; }
    }

    public class DateShiftOptions
    {
        [JsonProperty("shift_dates")]
        public bool? ShiftDates { get; set; }
        [JsonProperty("old_start_date")]
        public DateTime? OldStartDate { get; set; }
        [JsonProperty("old_end_date")]
        public DateTime? OldEndDate { get; set; }
        [JsonProperty("new_start_date")]
        public DateTime? NewStartDate { get; set; }
        [JsonProperty("new_end_date")]
        public DateTime? NewEndDate { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExportState
    {
        [EnumMember(Value = "created")] Created,
        [EnumMember(Value = "exporting")] Exporting,
        [EnumMember(Value = "exported")] Exported,
        [EnumMember(Value = "failed")] Failed
    }

    public class ContentExport : CanvasObject
    {
        public ContentExport(CanvasConnector conn) { Connector = conn; }

        internal override string SaveUrl => $"courses/{CourseID}/content_exports";
        internal override string GetUrl => $"{SaveUrl}/{ID}";

        public int CourseID { get; set; }

        [JsonProperty("export_type")]
        public string Type => "common_cartridge";
        [JsonProperty("skip_notifications")]
        public bool SkipNotifications { get; set; } = true;
        [JsonProperty("workflow_state")]
        public ExportState State { get; set; }
        [JsonProperty("attachment")]
        public Attachment Attachment { get; set; }
        [JsonProperty("progress_url")]
        public string ProgressUrl { get; set; }
    }

    public class Attachment 
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
