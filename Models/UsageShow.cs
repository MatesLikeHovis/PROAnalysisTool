using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
namespace WPFBMI.Models
{
    public class UsageShow
    {
        [PrimaryKey, AutoIncrement]
        public int primary_key { get; set; }
        public string show_name { get; set; }
        public string show_num { get; set; }
        public int channel_id { get; set; }
    }
}
