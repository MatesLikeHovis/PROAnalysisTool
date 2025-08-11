using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
namespace WPFBMI.Models
{
    public class UsageChannel
    {

        [PrimaryKey, AutoIncrement]
        public int primary_key { get; set; }
        public string source_name { get; set; }
        public string source_country { get; set; }
    }
}
