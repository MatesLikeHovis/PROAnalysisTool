using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFBMI.Models
{
    public class EFChannel
    {
        public int id { get; set; }
        public string? source_name { get; set; }
        public string? source_country { get; set; }
    }
}
