using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace WPFBMI.Models
{
    public class EFTrack
    {
        public int id { get; set; }
        public string? title_name { get; set; }
        public string? title_num { get; set; }
        public int? root_id { get; set; }
        public RootTrack root_track { get; set; }
    }
}
