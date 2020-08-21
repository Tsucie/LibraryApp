using System.Text.Json.Serialization;

namespace PerpusApp.Source.Models
{
    public sealed class UserSite
    {
        public int s_id { get; set; }
        public int s_u_id { get; set; }
        public string s_fullname { get; set; }
        public string s_email { get; set; }
        public string s_contact { get; set; }
        public string s_address { get; set; }
        public short s_status { get; set; }

        [JsonIgnore]
        public short? s_rec_status { get; set; }

        [JsonIgnore]
        public string s_rec_createdby { get; set; }

        [JsonIgnore]
        public string s_rec_created { get; set; }

        [JsonIgnore]
        public string s_rec_updatedby { get; set; }

        [JsonIgnore]
        public string s_rec_updated { get; set; }

        [JsonIgnore]
        public string s_rec_deletedby { get; set; }

        [JsonIgnore]
        public string s_rec_deleted { get; set; }

        //users
        public int u_id { get; set; }
        public int u_uc_id { get; set; }
        public string u_username { get; set; }
        public string u_password { get; set; }
        public string new_password { get; set; }

        // user_photo
        public string up_filename { get; set; }
        public byte[] up_photo { get; set; }
    }
}