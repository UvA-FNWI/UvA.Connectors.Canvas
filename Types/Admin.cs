using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvA.DataNose.Connectors.Canvas
{
    public class Admin : CanvasObject
    {
        public Admin(CanvasConnector conn) { Connector = conn; }

        public override string ToString() => $"Admin {ID}: role {RoleID}, account {AccountID}";
        internal override string SaveUrl => $"accounts/{AccountID}/admins/{User?.ID}";

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("account_id")]
        public int AccountID { get; set; }
        [JsonProperty("role_id")]
        public int RoleID { get; set; }
        [JsonProperty("send_confirmation")]
        public bool SendConfirmation { get; set; }

        /// <summary>
        /// Sets the UserID based on login ID. Only for creating new enrolments
        /// </summary>
        public string LoginID { set => UserID = "sis_login_id:" + value; }

        [JsonProperty("user_id")]
        public string UserID { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        public void Delete()
            => Connector.Delete(this, ("role_id", RoleID.ToString()));
    }
}
