using System.Text.Json.Serialization;

namespace PerpusApp.Source.Models
{
    public sealed class UserStaff
    {
        public int stf_id { get; set; }
        public int stf_u_id { get; set; }
        public int stf_sc_id { get; set; }
        public string stf_fullname { get; set; }
        public string stf_email { get; set; }
        public string stf_contact { get; set; }
        public string stf_address { get; set; }
        public string stf_shift { get; set; }
        public short stf_status { get; set; }

        [JsonIgnore]
        public short stf_rec_status { get; set; }

        [JsonIgnore]
        public string stf_rec_createdby { get; set; }

        [JsonIgnore]
        public string stf_rec_created { get; set; }

        [JsonIgnore]
        public string stf_rec_updatedby { get; set; }

        [JsonIgnore]
        public string stf_rec_updated { get; set; }

        [JsonIgnore]
        public string stf_rec_deletedby { get; set; }

        [JsonIgnore]
        public string stf_rec_deleted { get; set; }

        //users
        public int u_id { get; set; }
        public int u_uc_id { get; set; }
        public string u_username { get; set; }
        public string u_password { get; set; }

        //staff_category
        public int sc_id { get; set; }
        public string sc_name { get; set; }
        public string sc_desc { get; set; }
    }
}