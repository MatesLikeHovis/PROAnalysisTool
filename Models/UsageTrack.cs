using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
namespace WPFBMI.Models
{
    public class UsageTrack
    {
        [PrimaryKey, AutoIncrement]
        public int primary_key { get; set; }
        public string title_name { get; set; }
        public int title_num { get; set; }
        public string genre { get; set; }
        public string subgenre { get; set; }
        public string publisher { get; set; }
        public string library { get; set; }
    }
}
