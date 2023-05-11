using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public class Account : CanvasObject
    {
        public Account(CanvasApiConnector conn) { Connector = conn; }

        public override string ToString() => $"Account {ID}: {Name}";
        internal override string CanvasObjectID => "account";
        internal override string SaveUrl => $"accounts/{ID}";

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("parent_account_id")]
        public int? ParentAccountID { get; set; }
        [JsonProperty("sis_account_id")]
        public string SISAccountId { get; set; }
        
        List<Course> _Courses;
        [JsonIgnore]
        public List<Course> Courses => _Courses ?? (_Courses = Connector.RetrieveCollection<Course>(this));

        List<Term> _Terms;
        /// <summary>
        /// Retrieves the terms for the room account (throws an exception on subaccounts)
        /// </summary>
        [JsonIgnore]
        public List<Term> Terms => _Terms ?? (_Terms = Connector.RetrieveCollection<Term>(this));

        List<Admin> _Admins;
        public List<Admin> Admins
        {
            get
            {
                if (_Admins == null)
                {
                    _Admins = Connector.RetrieveCollection<Admin>(this);
                    _Admins.ForEach(a => a.AccountID = ID.Value);
                }
                return _Admins;
            }
        }

        public IEnumerable<Course> GetCoursesBySISTerm(string sisTerm) => Connector.RetrieveCollection<Course>(this, param: ("enrollment_term_id", $"sis_term_id:{sisTerm}"));
        public IEnumerable<Course> GetCoursesBySISTerms(params string[] sisTerms) => sisTerms.SelectMany(t => Connector.RetrieveCollection<Course>(this, param: ("enrollment_term_id", $"sis_term_id:{t}")));
        public IEnumerable<Course> GetCoursesByTerm(int term) => Connector.RetrieveCollection<Course>(this, param: ("enrollment_term_id", term.ToString()));
        public IEnumerable<Course> GetCoursesByState(CourseState state) => Connector.RetrieveCollection<Course>(this, param: ("state[]", state.ToString().ToLower()));
        public IEnumerable<Account> GetSubAccounts(bool recursive) => Connector.RetrieveCollection<Account>(this, param: ("recursive", recursive.ToString()), path: "sub_account");
    }
}
