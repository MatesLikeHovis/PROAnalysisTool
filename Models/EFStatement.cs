using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace WPFBMI.Models
{
    public class EFStatement
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? period { get; set; }
        public string? participant { get; set; }
        public string? participant_num { get; set; }
    }
}
