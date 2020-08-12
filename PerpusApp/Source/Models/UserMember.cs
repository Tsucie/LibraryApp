using System.Text.Json.Serialization;

namespace PerpusApp.Source.Models
{
    public sealed class UserMember
    {
        public int m_id { get; set; }
        public int m_u_id { get; set; }
        public int? m_bks_id { get; set; }
        public string m_class { get; set; }
        public string m_fullname { get; set; }
        public string m_email { get; set; }
        public string m_contact { get; set; }
        public string m_address { get; set; }
        public short m_status { get; set; }

        [JsonIgnore]
        public short m_rec_status { get; set; }

        [JsonIgnore]
        public string m_rec_createdby { get; set; }

        [JsonIgnore]
        public string m_rec_created { get; set; }

        [JsonIgnore]
        public string m_rec_updatedby { get; set; }

        [JsonIgnore]
        public string m_rec_updated { get; set; }

        [JsonIgnore]
        public string m_rec_deletedby { get; set; }

        [JsonIgnore]
        public string m_rec_deleted { get; set; }

        //users
        public int u_id { get; set; }
        public int u_uc_id { get; set; }
        public string u_username { get; set; }
        public string u_password { get; set; }
    }
}