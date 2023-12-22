using System;
using System.Collections.Generic;
using UvA.DataNose.Connectors.Canvas;
using Newtonsoft.Json.Linq;

namespace UvA.Connectors.Canvas
{
    public interface ICanvasApiConnector
    {
        List<Assignment> FindAssignmentsById(int courseId);
        Assignment FindAssignmentById(int courseId, int assignmentId);
        Account FindAccountById(int id);
        Course FindCourseById(int id);
        Section FindSectionById(int id);
        Course FindCourseById(string sisId);
        AppointmentGroup FindAppointmentGroupById(int id);

        List<CalendarEvent> GetEvents(string userLoginId, int[] courseIds, DateTime startDate, DateTime endDate, EventType? targetType = null);

        void Delete<T>(T o, (string key, string value)? param = null) where T : CanvasObject;
        void RetrieveData<T>(T o, string include = null) where T : CanvasObject;

        IEnumerable<JToken> GetCollection(string url);
        string GetSessionlessLaunchUrl(int courseId, int assignmentId);

        void Create(CanvasObject o);

        void DownloadFile(string path, string fileName);
        void SetPostPolicy(int courseId, bool postManually);

    }
}
