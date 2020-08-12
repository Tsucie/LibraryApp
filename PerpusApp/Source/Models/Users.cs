using System.Text.Json.Serialization;

namespace PerpusApp.Source.Models
{
    public sealed class Users
    {
        public int u_id { get; set; }
        public int u_uc_id { get; set; }
        public string u_username { get; set; }
        public string u_password { get; set; }

        [JsonIgnore]
        public short? u_rec_status { get; set; }

        [JsonIgnore]
        public string u_rec_createdby { get; set; }

        [JsonIgnore]
        public string u_rec_created { get; set; }

        [JsonIgnore]
        public string u_rec_updatedby { get; set; }

        [JsonIgnore]
        public string u_rec_updated { get; set; }

        [JsonIgnore]
        public string u_rec_deletedby { get; set; }

        [JsonIgnore]
        public string u_rec_deleted { get; set; }

        //user_category
        public int uc_id { get; set; }
        public string uc_name { get; set; }
        public string uc_desc { get; set; }

        //user_photo
        public int up_id { get; set; }
        public int up_u_id { get; set; }
        public byte[] up_photo { get; set; }
        public string up_filename { get; set; }
        public short up_rec_status { get; set; }
    }
}