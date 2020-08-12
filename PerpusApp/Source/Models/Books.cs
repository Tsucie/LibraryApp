using System.Text.Json.Serialization;

namespace PerpusApp.Source.Models
{
    public sealed class Books
    {
        public int bks_id { get; set; }
        public int bks_bc_id { get; set; }
        public string bks_code { get; set; }
        public string bks_name { get; set; }
        public string bks_writer { get; set; }
        public string bks_launcher { get; set; }
        public string bks_launchingtime { get; set; }
        public int bks_page { get; set; }
        public int bks_price { get; set; }

        [JsonIgnore]
        public short bks_rec_status { get; set; }

        [JsonIgnore]
        public string bks_rec_createdby { get; set; }

        [JsonIgnore]
        public string bks_rec_created { get; set; }

        [JsonIgnore]
        public string bks_rec_updatedby { get; set; }

        [JsonIgnore]
        public string bks_rec_updated { get; set; }

        [JsonIgnore]
        public string bks_rec_deletedby { get; set; }

        [JsonIgnore]
        public string bks_rec_deleted { get; set; }

        // book_category
        public int bc_id { get; set; }
        public int bc_br_id { get; set; }
        public string bc_name { get; set; }
        public string bc_desc { get; set; }

        // book_rack
        public int br_id { get; set; }
        public string br_code { get; set; }
        public string br_room { get; set; }
        public string br_desc { get; set; }
    }
}