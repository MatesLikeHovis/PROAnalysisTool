using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
namespace WPFBMI.Models
{
    public class UsageStatement
    {

        [PrimaryKey, AutoIncrement]
        public int primary_key { get; set; }
        public string name { get; set; }
        public string period { get; set; }
        public string participant { get; set; }
        public string participant_num { get; set; }
    }
}
