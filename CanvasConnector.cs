using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using UvA.Connectors.Canvas.Helpers;


namespace UvA.DataNose.Connectors.Canvas
{
    public class CanvasConnector
    {
        public string TargetUrl { get; private set; }
        protected string BaseUrl { get; private set; }
        private string AccessToken;
        protected string UserAgent = "CanvasConnector";
        protected string Page { get; private set; }
        string NextUrl;

        Dictionary<Type, object> Collections = new Dictionary<Type, object>();
        List<T> GetCollection<T>() where T : CanvasObject
        {
            var type = typeof(T);
            if (!Collections.ContainsKey(type))
                Collections.Add(type, new List<T>());
            return Collections[type] as List<T>;
        }

        static JsonSerializerSettings Settings = new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local };

        public Assignment FindAssignmentById(int courseId, int assignmentId) => RetrieveObject<Assignment>($"courses/{courseId}/assignments/{assignmentId}");
        public Account FindAccountById(int id) => FindObjectById<Account>(id);
        public Course FindCourseById(int id) => FindObjectById<Course>(id);
        public Section FindSectionById(int id) => FindObjectById<Section>(id);
        public Course FindCourseById(string sisId) => RetrieveObject<Course>($"courses/sis_course_id:{sisId}");
        public AppointmentGroup FindAppointmentGroupById(int id) => FindObjectById<AppointmentGroup>(id);

        public List<CalendarEvent> GetEvents(string userLoginId, int[] courseIds, DateTime startDate, DateTime endDate, EventType? targetType = null) => 
            courseIds.Split(10).SelectMany(slice =>
                (targetType == null ? Tools.GetEnumEntries<EventType>() : new[] { targetType.Value }).SelectMany(type => {
                    var url = $"users/sis_login_id:{userLoginId}/calendar_events?{slice.ToSeparatedString(i => $"context_codes[]=course_{i}", "&")}&type={type.ToString().ToLower()}&excludes[]=assignment&start_date={CanvasObject.ToCanvasString(startDate)}&end_date={CanvasObject.ToCanvasString(endDate)}";
                    var resp = Request(url, "GET").Replace("\"id\":\"assignment_", "\"id\":\""); // the api weird string ids :-(
                    return JsonConvert.DeserializeObject<CalendarEvent[]>(resp, Settings);
                })).ToList();

        /// <summary>
        /// Creates a CanvasConnector
        /// </summary>
        /// <param name="canvasUrl">Target Canvas instance URL, e.g. https://example.instructure.com/</param>
        /// <param name="token">Canvas access token</param>
        public CanvasConnector(string canvasUrl, string token) : base()
        {
            TargetUrl = canvasUrl;
            this.BaseUrl = $"{canvasUrl}/api/v1/";
            this.AccessToken = token;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        private T FindObjectById<T>(int id, bool skip_retrieval = false) where T : CanvasObject
        {
            if (id == 0)
                return null;

            List<T> list = GetCollection<T>();
            T target = list.FirstOrDefault(s => s.ID == id);
            if (target == null)
            {
                target = (T)Activator.CreateInstance(typeof(T), new object[] { this });
                target.ID = id;
                if (!skip_retrieval)
                    RetrieveData(target);
                list.Add(target);
            }
            return target;
        }

        T RetrieveObject<T>(string url) where T : CanvasObject
        {
            T o = JsonConvert.DeserializeObject<T>(Request(url, "GET"), Settings);
            o.isRetrieved = true;
            o.Connector = this;
            GetCollection<T>().Add(o);
            return o;
        }

        public void Delete<T>(T o, (string key, string value)? param = null) where T : CanvasObject
        {
            var url = o.SaveUrl;
            if (param != null)
                url += $"?{param?.key}={param?.value}";
            Request(url, "DELETE");
            o.ID = null;
        }

        public void RetrieveData<T>(T o, string include = null) where T : CanvasObject
        {
            string s = null;
            try
            {
                var url = o.GetUrl;
                if (include != null)
                    url += $"?include[]={include}";
                s = Request(url, "GET");
            }
            catch (WebException e)
            {
                //
                throw e;
            }
            o.isRetrieved = true;
            JsonConvert.PopulateObject(s, o, Settings);
        }

        internal List<T> RetrieveCollection<T>(CanvasObject o, (string key, string value)[] extraParams, string include = null, string path = null, Action<T> initFunc = null) where T : CanvasObject
        {
            if (o.ID == 0)
                return null;
            var url = $"{o.GetUrl}/{path ?? typeof(T).Name.ToLower()}s";
            var pars = new List<(string Key, string Value)>() { ("per_page", "100") };
            if (include != null)
                pars.Add(("include[]", include));
            if (extraParams != null)
                 pars.AddRange(extraParams);
            url += "?" + pars.ToSeparatedString(p => $"{p.Key}={p.Value}", "&");
            var coll =  RetrieveCollection<T>(url);
            if (initFunc != null)
                coll.ForEach(initFunc);
            return coll;
        }

        internal List<T> RetrieveCollection<T>(CanvasObject o, string include = null, (string key, string value)? param = null, string path = null, Action<T> initFunc = null) where T : CanvasObject
            => RetrieveCollection<T>(o, param == null ? null : new[] { param.Value }, include, path, initFunc);

        internal IEnumerable<T> RetrieveArray<T>(string url) => JsonConvert.DeserializeObject<T[]>(Request(url, "GET"), Settings);

        List<T> RetrieveCollection<T>(string url) where T : CanvasObject
        {
            var s = Request(url, "GET");

            var collection = new List<T>();
            List<T> list = GetCollection<T>();


            while (true)
            {
                var token = JToken.Parse(s);
                JArray jcollection = (token is JArray ? token : (token.First as JProperty).Value) as JArray; // Canvas is not very consistent :-(

                foreach (var v in jcollection)
                {
                    if (typeof(T) == typeof(Page) || typeof(T) == typeof(SubmissionGroup)) // these have no id, strangely
                    {
                        var obj = v.ToObject<T>();
                        obj.ID = -1;
                        obj.isRetrieved = true;
                        obj.Connector = this;
                        collection.Add(obj);
                        continue;
                    }
                    int id = v["id"].Value<int>();

                    T target = list.FirstOrDefault(so => so.ID == id);
                    if (target != null)
                    {
                        collection.Add(target);
                        JsonConvert.PopulateObject(v.ToString(), target, Settings);
                        continue;
                    }

                    target = FindObjectById<T>(id, true);
                    target.isRetrieved = true;
                    JsonConvert.PopulateObject(v.ToString(), target, Settings);
                    collection.Add(target);
                }

                if (NextUrl != null)
                    s = Request(NextUrl, "GET");
                else
                    break;
            }


            return collection;
        }

        public IEnumerable<JToken> GetCollection(string url)
        {
            var list = new List<JToken>();
            NextUrl = url;
            while (NextUrl != null)
                list.AddRange(JToken.Parse(Request(NextUrl, "GET")));
            return list;
        }

        // TODO: switch to JSON for all relevant types and make sure this doesn't break anything
        bool UseJSON(CanvasObject o) => o is ExternalTool || o is Assignment || o is Folder || o is File || o is Course 
            || o is DiscussionEntry || o is DiscussionReply || o is Discussion || o is Page || o is ContentMigration
            || o is QuizReport;

        internal JToken Get(string url) => JToken.Parse(Request(url, "GET"));

        internal void Update(string url, params (string key, object value)[] pars)
            => Request(url, "PUT", "application/x-www-form-urlencoded", payload: pars.ToSeparatedString(p => $"{p.key}={CanvasObject.ToCanvasString(p.value)}", "&"));

        internal void Create(string url, params (string key, object value)[] pars)
            => Request(url, "POST", "application/x-www-form-urlencoded", payload: pars.ToSeparatedString(p => $"{p.key}={HttpUtility.UrlEncode(CanvasObject.ToCanvasString(p.value))}", "&"));

        internal void UpdateData(CanvasObject o)
        {
            var response = Request(o.SaveUrl, "PUT", payload: GetPayload(o), contentType: UseJSON(o) ? "application/json" : "application/x-www-form-urlencoded");
            JsonConvert.PopulateObject(response, o, Settings);
        }

        public string GetSessionlessLaunchUrl(int courseId, int assignmentId)
            => (string)JObject.Parse(Request($"courses/{courseId}/external_tools/sessionless_launch?assignment_id={assignmentId}&launch_type=assessment", "GET"))["url"];

        string GetPayload(CanvasObject o)
        {
            if (UseJSON(o))
            {
                if (o.SendWrapped)
                    return JsonConvert.SerializeObject(new Dictionary<string, object> { [o.EntityName.ToLower()] = o });
                else
                    return JsonConvert.SerializeObject(o);
            }
            else
                return o.GetValues().ToSeparatedString(p => $"{p.key}={HttpUtility.UrlDecode(p.value)}", "&");
        }

        public void Create(CanvasObject o)
        {
            var response = Request(o.SaveUrl, "POST", payload: GetPayload(o), contentType: UseJSON(o)? "application/json" : "application/x-www-form-urlencoded");
            o.isRetrieved = true;
            JsonConvert.PopulateObject(response, o, Settings);

            // TODO, insert into collections..
        }

        string Request(string path, string method, string contentType = "application/x-www-form-urlencoded", string payload = null)
        {
            NextUrl = null;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(path.StartsWith("http") ? path : (BaseUrl + path));
            req.Method = method;
            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            if (!string.IsNullOrEmpty(AccessToken))
                req.Headers.Add("Authorization", $"Bearer {AccessToken}");
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            req.UserAgent = UserAgent;
            req.Timeout = 1000 * 60 * 10; // 10-mins

            if (method == "POST" || method == "PUT")
            {
                req.ContentType = contentType;
                if (payload != null)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(payload);
                    req.ContentLength = bytes.Length;
                    using (var stream = req.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
            }

            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                {
                    Page = reader.ReadToEnd();
                }
                var link = resp.Headers["Link"];
                if (link != null && link.Contains("rel=\"next\""))
                    NextUrl = link.Split(',').FirstOrDefault(p => p.Contains("rel=\"next\"")).Split(';').First().TrimStart('<').TrimEnd('>');
            }

            return Page;
        }

        public void DownloadFile(string path, string fileName)
        {
            WebClient client = new WebClient();
            client.Headers.Add("Authorization", $"Bearer {AccessToken}");
            client.DownloadFile(path, fileName);
        }

        GraphQLHttpClient _Client;
        GraphQLHttpClient Client => _Client ?? (_Client = new GraphQLHttpClient(new GraphQLHttpClientOptions
        {
            EndPoint = new Uri(BaseUrl.Replace("v1", "graphql")),
            ConfigureWebsocketOptions = opt => opt.SetRequestHeader("Authorization", $"Bearer {AccessToken}")
        }, new NewtonsoftJsonSerializer()));

        public void SetPostPolicy(int courseId, bool postManually)
        {
            var res = Client.SendMutationAsync<string>(new GraphQLRequest
            {
                Query = @"
                mutation SetPolicy($courseId: ID!, $postManually: Boolean!) {
                  setCoursePostPolicy(input: {courseId: $courseId , postManually: $postManually}) {
                    postPolicy {
                      postManually
                    }
                  }
                }
                ",
                OperationName = "SetPolicy",
                Variables = new
                {
                    courseId,
                    postManually
                }
            }).Result;
            if (res.Errors?.Any() == true)
                throw new Exception(res.Errors.First().Message);
        }
    }

    public class OAuth2Token
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
