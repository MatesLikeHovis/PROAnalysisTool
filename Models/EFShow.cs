using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFBMI.Models
{
    public class EFShow
    {
        public int id { get; set; }
        public string? show_name { get; set; }
        public string? show_num { get; set; }
        public EFChannel channel { get; set; }
        public int channel_id { get; set; }
    }
}
